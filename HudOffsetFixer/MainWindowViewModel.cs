//#define SEARCH_INGAMESTATE

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using HudOffsetFixer.Core;
using HudOffsetFixer.Core.SearchStrategies;
using HudOffsetFixer.Core.SearchStrategies.MultiValue;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;
using HudOffsetFixer.Core.SearchStrategies.Special;
using HudOffsetFixer.Core.SearchStrategies.ValueReaders;
using HudOffsetFixer.Core.Utils;
using HudOffsetFixer.Core.ValueCompare;
using HudOffsetFixer.Extensions;
using HudOffsetFixer.MemorySections;
using HudOffsetFixer.PoeStructs;
using PoeHUD.Controllers;
using PoeHUD.Framework;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace HudOffsetFixer
{
    //Login:    poeoffsetfix@yopmail.com
    //Pass:     poeoffsetfix@yopmail.com
    public class MainWindowViewModel : BaseViewModel
    {
        public static MainWindowViewModel Instance;
        private ICommand _connectCommand;
        private ICommand _searchCommand;
        private OffsetsFixer OffsetsFixer;
        private PoeProcessController PoeProcess;
        const string LEAGUE_NAME = "Standard";

        public MainWindowViewModel()
        {
            Instance = this;
        }

        public BaseOffset SelectedOffset { get; set; }
        public ObservableCollection<BaseOffset> Nodes { get; } = new ObservableCollection<BaseOffset>();
        private readonly List<StructureOffset> _initialStates = new List<StructureOffset>();

        public ICommand Connect
        {
            get { return _connectCommand ?? (_connectCommand = new CommandHandler(ConnectToPoe, () => true)); }
        }

        public ICommand Search
        {
            get { return _searchCommand ?? (_searchCommand = new CommandHandler(SearchOffsets, () => true)); }
        }

        public void OffsetsHierarchyOnSelectedItemChanged(BaseOffset newSelectedItem)
        {
            SelectedOffset = newSelectedItem;
            RaisePropertyChanged(nameof(SelectedOffset));
        }

        private void SearchOffsets()
        {
            foreach (var structureOffset in _initialStates)
            {
                OffsetsFixer.FixStructureChilds(structureOffset);
            }
        }

        public void ConnectToPoe()
        {
            PoeProcess = new PoeProcessController();
            PoeProcess.ConnectToProcess();
            OffsetsFixer = new OffsetsFixer(Memory.Instance);

            AddOffsetData();
        }

        private void AddOffsetData()
        {
            _initialStates.Clear();

#if SEARCH_INGAMESTATE //TODO: Push to ckeckbox
              if (!SearchInGameState(out var debugRectValueAddress))
            {
                MessageBox.Show("Can't find debug rectangle state in poe (searching InGameState automatically)");
                return;
            }

            //Use debugRectValueAddress in CheatEngine and check who writes to that address and save the offset
            //https://dl.dropboxusercontent.com/s/2h2f9l8wyna10k1/cheatengine-x86_64_2019-06-26_18-44-38.png
            var inGameStateAddr = debugRectValueAddress - 0x568; 

#else
            var inGameStateAddr = 0x713C82DF80;
#endif

            var inGameState = new StructureOffset("InGameState", null, maxStructSize: 0x1000, baseAddress: inGameStateAddr);
            _initialStates.Add(inGameState);

            inGameState.AddStringSearch("State Name", "InGameState");
            inGameState.AddIntSearch("Camera.Width", 2048);
            inGameState.AddIntSearch("Camera.Height", 1099);

            #region UiRoot

            //Check/search child array to have 2 child
            var uiChildAmount = new PointersSearchStrategy(new ArrayLengthDoublePointerCompare(new DefaultValueCompare<int>(compareValue: 2)),
                checkVmt: false);

            //...have a pointer to itself
            var uiSelfReferencingPointer = new PointersSearchStrategy(new SelfReferencingStructPointerValueCompare(), checkVmt: true);

            //...search for Width/Height of UiRoot
            var uiRootWidthValueCompare = new FloatValueCompare(compareValue: 2560, tolerance: 1);
            var uiRootHeightValueCompare = new FloatValueCompare(compareValue: 1600, tolerance: 1);
            var uiWidthHeightReading = new MultipleFloatValueReader(uiRootWidthValueCompare, uiRootHeightValueCompare);
            var uiWidthHeightSearch = new ValueReaderStrategy(uiWidthHeightReading, alignment: 4);

            //...create multi value offset search strategy with this strategies.
            //Strategies should goes in order to expected offset order. If no- change the settings of DefaultMultiValueOffsetsFilter
            var uiMultiValueSearch = new MultiValueStrategy(new DefaultMultiValueOffsetsFilter(), uiSelfReferencingPointer, uiChildAmount,
                uiWidthHeightSearch);

            var uiRoot = new StructureOffset("Ui Root", new SubPointersSearchStrategy(uiMultiValueSearch, subStructSize: 0x300, checkVmt: true),
                maxStructSize: 0x300);

            uiRoot.Child.Add(new DataOffset("ChildPtr", uiChildAmount));

            inGameState.Child.Add(uiRoot);

            uiRoot.AddFloatSearch("UiElement.Width", 2560, 1);
            uiRoot.AddFloatSearch("UiElement.Height", 1600, 1);
            uiRoot.AddFloatSearch("UiElement.Scale", 0.686875f, 0.02f);

            #endregion

            #region IngameData

            var inGameData = IngameDataOffsets(inGameState);

            #region Player

            #region ServerData

            ServerDataOffsets(inGameState);

            #endregion

            PlayerOffsets(inGameData);

            #endregion

            //var names = Entity.ReadComponentsNames(0x0000007021B47130);

            #endregion

            Nodes.Clear();

            foreach (var structureOffset in _initialStates)
            {
                Nodes.Add(structureOffset);
            }
        }

        private bool SearchInGameState(out long debugRectAddr)
        {
            var debugRectAddresses = new LinkedList<long>();
            byte debugValue = 2;

            foreach (var section in MemorySectionsProcessor.Instance.Sections)
            {
                if (section.Category != SectionCategory.HEAP)
                    continue;

                var sectionSize = section.Size.ToInt32();
                var sectionBytes = Memory.Instance.ReadBytes(section.Begin.ToInt64(), sectionSize);

                for (var i = 0; i < sectionBytes.Length; i++)
                {
                    if (sectionBytes[i] == debugValue)
                    {
                        debugRectAddresses.AddLast(section.Begin.ToInt64() + i);
                    }
                }
            }

            FocusWindow();

            while (debugRectAddresses.Count > 1)
            {
                debugValue++;

                if (debugValue == 3)
                    debugValue = 0;

                FocusWindow();
                KeyboardHelpers.KeyPress(Keys.F1);

                var currentNode = debugRectAddresses.Last;

                while (currentNode != null)
                {
                    var previous = currentNode.Previous;

                    if (Memory.Instance.ReadByte(currentNode.Value) != debugValue)
                    {
                        debugRectAddresses.Remove(currentNode);
                    }

                    currentNode = previous;
                }
            }

            if (debugRectAddresses.Count == 1)
            {
                debugRectAddr = debugRectAddresses.First.Value;
                return true;
            }

            debugRectAddr = 0;
            return false;
        }

        private void FocusWindow()
        {
            WinApi.ShowWindow(Memory.Instance.Process.MainWindowHandle, 3);
            WinApi.SetForegroundWindow(Memory.Instance.Process.MainWindowHandle);
        }

        private static StructureOffset IngameDataOffsets(StructureOffset inGameState)
        {
            var inGameDataAreaNameStructSearch = StrategyUtils.StringInSubStruct("Lush Hideout", subStructSize: 0x10, checkVmt: false);
            var inGameDataStructSearch = inGameDataAreaNameStructSearch.SubStructSearch(subStructSize: 0x600, checkVmt: true);
            var inGameData = new StructureOffset("InGameData", inGameDataStructSearch, maxStructSize: 0x600);
            inGameState.Child.Add(inGameData);

            inGameData.Child.Add(new DataOffset("Area Name", inGameDataAreaNameStructSearch));
            inGameData.AddByteSearch("Area Level", 60, false, 8);
            return inGameData;
        }

        private void ServerDataOffsets(StructureOffset inGameState)
        {
            var serverDataAdapter = new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>(LEAGUE_NAME))).Adapter();

            var serverData = new StructureOffset("ServerData",
                new SubPointersSearchStrategy(serverDataAdapter, subStructSize: 0x8000, checkVmt: true),
                maxStructSize: 0x8000);

            inGameState.Child.Add(serverData);

            serverData.AddStringSearch("League Name", "Standard", true);

            //GUildName
            var serverDataGuildName =
                new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>("Stridemann's GUild"))).Adapter();

            var serverDataGuildSearch = new SubPointersSearchStrategy(serverDataGuildName, subStructSize: 0x8, checkVmt: false);
            serverData.Child.Add(new DataOffset("Guild Name", serverDataGuildSearch));

            //serverData.AddIntSearch("Azurite Amount", 0);
            //serverData.AddUShortSearch("Sulphite Amount", 0);

            var inventId = new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(1)), 4);
            var inventId2 = new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(2)), 4);
            var inventRows = new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(5)), 4);
            var inventCols = new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(12)), 4);

            var inventoriesMultiValueSearch = new MultiValueStrategy(new DefaultMultiValueOffsetsFilter(),
                inventId,
                new SubPointersSearchStrategy(inventRows.Adapter(), 0x100, false),
                new SubPointersSearchStrategy(inventCols.Adapter(), 0x100, false),
                inventId2
            );

            var inventoryHolderSearch = new SubPointersSearchStrategy(inventoriesMultiValueSearch, 0x100, false);
            var playerInventories = new StructureOffset("PlayerInventories (InventoryHolder)", inventoryHolderSearch, 0x10);
            serverData.Child.Add(playerInventories);

            playerInventories.AddIntSearch("InventoryHolder.InventType", 1);

            var serverInventoryColsRowsMultiValueSearch = new MultiValueStrategy(new DefaultMultiValueOffsetsFilter(), inventRows);
            var serverInventorySearch = new SubPointersSearchStrategy(serverInventoryColsRowsMultiValueSearch, 0x100, false);
            var serverInventory = new StructureOffset("InventoryHolder.ServerInventory", serverInventorySearch, 0x100);
            playerInventories.Child.Add(serverInventory);

            serverInventory.AddIntSearch("Rows", 5);
            serverInventory.AddIntSearch("Cols", 12);

            var reader = new MultipleIntValueReader(
                    new DefaultValueCompare<int>(0), //posX
                    new DefaultValueCompare<int>(0), //posY
                    new DefaultValueCompare<int>(1), //sizeX
                    new DefaultValueCompare<int>(1)) //sizeY
                ;

            var inventSlotItemSearch = new ValueReaderStrategy(reader, 4).SubStructSearch(0x38, false);
            var readItemsList = inventSlotItemSearch.SubStructSearch(0x38, false);
            var itemsList = new StructureOffset("ItemsList", readItemsList, 0x8);
            serverInventory.Child.Add(itemsList);

            var inventSlotItem = new StructureOffset("InventSlotItem", inventSlotItemSearch, 0x8);
            itemsList.Child.Add(inventSlotItem);
        }

        private void PlayerOffsets(StructureOffset inGameData)
        {
            var player = new StructureOffset("Player",
                StrategyUtils.StringSearch(new StringValueComparer("Metadata/Characters/", StringCompareType.StartWith)).SubStructSearch(0x50, true)
                    .SubStructSearch(8, false),
                maxStructSize: 0x60);

            #region PositionedComponent

            var worldX = new FloatValueCompare(compareValue: 2375, tolerance: 50);
            var worldY = new FloatValueCompare(compareValue: 3646.739f, tolerance: 50);
            var worldPos = new MultipleFloatValueReader(worldX, worldY);
            var worldPosReader = new ValueReaderStrategy(worldPos, alignment: 4);

            var entityPositionedComponent = new StructureOffset("Entity.PositionedComponent",
                new SubPointersSearchStrategy(worldPosReader.Adapter(), subStructSize: 0x200, checkVmt: true),
                maxStructSize: 0x300);

            player.Child.Add(entityPositionedComponent);
            inGameData.Child.Add(player);

            #endregion

            #region Entity.ComponentLookup

            //Positioned seems always the first component, so you can pointerscan inside Entity to Positioned string using Structure Spider Advanced
            var entityComponentLookupSearch =
                new PointersSearchStrategy(new EntityComponentLookupPointerCompare(new List<string> {"Life", "Player"}));

            var entityLookupDeph2 = entityComponentLookupSearch.SubStructSearch(0x100, false);
            var entityLookupDeph1 = entityLookupDeph2.SubStructSearch(0x100, true);

            var entityInternalStruct = new StructureOffset("Entity.ComponentLookup", entityLookupDeph1, 8);
            player.Child.Add(entityInternalStruct);

            entityInternalStruct.OnOffsetsFound += delegate
            {
                var finalOffsets = new List<int>();
                finalOffsets.AddRange(entityLookupDeph1.FoundOffsets);
                finalOffsets.AddRange(entityLookupDeph2.FoundOffsets);
                finalOffsets.AddRange(entityComponentLookupSearch.FoundOffsets);
                entityInternalStruct.FoundOffsets = finalOffsets;

                entityInternalStruct.DebugInfo = $"({entityLookupDeph1.FoundOffsets.Format()}, " +
                                                 $"{entityLookupDeph2.FoundOffsets.Format()}, " +
                                                 $"{entityComponentLookupSearch.FoundOffsets.Format()})";

                if (entityInternalStruct.SearchStatus == OffsetSearchStatus.FoundSingle)
                {
                    Entity.ComponentLookupOffset1 = entityLookupDeph1.FoundOffsets[0];
                    Entity.ComponentLookupOffset2 = entityLookupDeph2.FoundOffsets[0];
                    Entity.ComponentLookupOffset3 = entityComponentLookupSearch.FoundOffsets[0];
                }
            };

            var listSearch = new PointerValueCompare(new DelegateReferenceValueCompare<long>(() => entityPositionedComponent.BaseAddress));
            var listPointerSearch = new PointersSearchStrategy(listSearch).SubStructSearch(100, false);
            var entityComponentList = new StructureOffset("Entity.ComponentList", listPointerSearch, 0x30); //not 0x30, actually it is on 0x8
            entityComponentList.ShouldProcess = () => entityPositionedComponent.OffsetIsFound;
            player.Child.Add(entityComponentList);

            entityComponentList.OnOffsetsFound += delegate
            {
                if (entityComponentList.SearchStatus == OffsetSearchStatus.FoundSingle)
                {
                    Entity.ComponentListOffset = entityComponentList.FoundOffsets[0];
                }
            };

            #endregion

            #region Components Fix

            #region Life

            var lifeComponent = new StructureOffset("LifeComponent", null, maxStructSize: 0x300);
            _initialStates.Add(lifeComponent);

            player.OnOffsetsFound += delegate { lifeComponent.BaseAddress = Entity.GetComponentAddress(player.BaseAddress, "Life"); };

            //var searchMaxHp = new ValueReaderStrategy(new MultipleIntValueReader(
            //    new DefaultValueCompare<int>(97),
            //    new DefaultValueCompare<int>(97)), 4);
            //lifeComponent.Child.Add(new DataOffset("MaxHp", searchMaxHp));     //this will work only when they goes strictly one after another
            //lifeComponent.Child.Add(new DataOffset("CurHp", new ReturnExistingOffsetStrategy(searchMaxHp, 0x4)));

            lifeComponent.Child.Add(new DataOffset("MaxHp",
                new MultipleOffsetsSelectorOffsetSearch(new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(97)), 4), 2, 0)));

            lifeComponent.Child.Add(new DataOffset("CurHp",
                new MultipleOffsetsSelectorOffsetSearch(new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(97)), 4), 2, 1)));

            //var searchMaxMana = new ValueReaderStrategy(new MultipleIntValueReader(
            //    new DefaultValueCompare<int>(74),
            //    new DefaultValueCompare<int>(66)), 4);
            //lifeComponent.Child.Add(new DataOffset("MaxMana", searchMaxMana));
            //lifeComponent.Child.Add(new DataOffset("CurMana", new ReturnExistingOffsetStrategy(searchMaxMana, 0x4)));
            lifeComponent.Child.Add(new DataOffset("MaxMana", new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(74)), 4)));
            lifeComponent.Child.Add(new DataOffset("CurMana", new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(66)), 4)));

            //var searchReservedMana = new ValueReaderStrategy(new MultipleIntValueReader(
            //    new DefaultValueCompare<int>(0),
            //    new DefaultValueCompare<int>(10)), 4);
            //lifeComponent.Child.Add(new DataOffset("ReservedFlatMana", searchReservedMana));
            //lifeComponent.Child.Add(new DataOffset("ReservedPercentMana", new ReturnExistingOffsetStrategy(searchReservedMana, 0x4)));
            lifeComponent.Child.Add(new DataOffset("ReservedFlatMana", new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(0)), 4)));
            lifeComponent.Child.Add(new DataOffset("ReservedPercentMana", new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(10)), 4)));

            //var searchEs = new ValueReaderStrategy(new MultipleIntValueReader(
            //    new DefaultValueCompare<int>(34),
            //    new DefaultValueCompare<int>(34)), 4);
            //lifeComponent.Child.Add(new DataOffset("MaxEs", searchEs));
            //lifeComponent.Child.Add(new DataOffset("CurEs", new ReturnExistingOffsetStrategy(searchEs, 0x4)));
            lifeComponent.Child.Add(new DataOffset("MaxEs", new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(34)), 4)));
            lifeComponent.Child.Add(new DataOffset("CurEs", new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(34)), 4)));

            var buffStruct = StrategyUtils.StringInSubStruct("sand_stance", 0x20, false, false); //todo:set to 0x10
            var buffStruct1 = buffStruct.SubStructSearch(0x20, false);
            var buffStruct2 = buffStruct1.SubStructSearch(0x20, false);
            var buffStruct3 = buffStruct2.SubStructSearch(0x20, false);

            lifeComponent.Child.Add(new StructureOffset("Buffs", buffStruct3, 0x20));

            #endregion

            #region Positioned

            var positionedComponent = new StructureOffset("PositionedComponent", null, maxStructSize: 0x300);

            _initialStates.Add(positionedComponent);
            player.OnOffsetsFound += delegate { positionedComponent.BaseAddress = Entity.GetComponentAddress(player.BaseAddress, "Positioned"); };

            positionedComponent.AddIntSearch("GridPosX", 218);
            positionedComponent.AddIntSearch("GridPosY", 335);
            positionedComponent.AddFloatSearch("WorldPosX", 2375, 50);
            positionedComponent.AddFloatSearch("WorldPosY", 3646.739f, 50);
            positionedComponent.AddFloatSearch("Rotation", 1.570f, 0.02f);
            positionedComponent.AddByteSearch("Reaction", 1);

            #endregion

            #region PlayerComponent

            var playerComponent = new StructureOffset("PlayerComponent", null, maxStructSize: 0x100);

            _initialStates.Add(playerComponent);
            player.OnOffsetsFound += delegate { playerComponent.BaseAddress = Entity.GetComponentAddress(player.BaseAddress, "Player"); };

            playerComponent.AddIntSearch("Exp", 4219);
            playerComponent.AddIntSearch("Strength", 14);
            playerComponent.AddIntSearch("Dexterity", 29);
            playerComponent.AddIntSearch("Intelligence", 32);
            playerComponent.AddIntSearch("Level", 4);

            #endregion

            #endregion
        }
    }
}
