using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HudOffsetFixer.Core;
using HudOffsetFixer.Core.SearchStrategies;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy.MultiValue;
using HudOffsetFixer.Core.SearchStrategies.ValueReaders;
using HudOffsetFixer.Core.ValueCompare;
using HudOffsetFixer.Core.ValueCompare.Special;
using HudOffsetFixer.Extensions;
using PoeHUD.Controllers;
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
        public ObservableCollection<BaseOffset> Nodes { get; set; } = new ObservableCollection<BaseOffset>();
        private StructureOffset _initialState;

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
            OffsetsFixer.FixStructureChilds(_initialState);
        }

        public void ConnectToPoe()
        {
            PoeProcess = new PoeProcessController();
            PoeProcess.ConnectToProcess();
            OffsetsFixer = new OffsetsFixer(GameController.Instance.Memory);

            var inGameState = new StructureOffset("InGameState", null, maxStructSize: 0x1000, baseAddress: 0x6E40A75BA0);

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

            #region ServerData

            ServerDataOffsets(inGameState);

            #endregion

            #region IngameData

            var inGameDataAreaNameStructSearch = StringInSubStruct("Lush Hideout", subStructSize: 0x10, checkVmt: false);
            var inGameDataStructSearch = inGameDataAreaNameStructSearch.SubStructSearch(subStructSize: 0x600, checkVmt: true);
            var inGameData = new StructureOffset("InGameData", inGameDataStructSearch, maxStructSize: 0x600);
            inGameState.Child.Add(inGameData);

            inGameData.Child.Add(new DataOffset("Area Name", inGameDataAreaNameStructSearch));
            inGameData.AddByteSearch("Area Level", 60, false, 8);

            #region Player

            PlayerOffsets(inGameData);

            #endregion

            //var names = Entity.ReadComponentsNames(0x0000007021B47130);

            #endregion

            Nodes.Clear();
            Nodes.Add(inGameState);
            _initialState = inGameState;
        }

        private void ServerDataOffsets(StructureOffset inGameState)
        {
            var serverDataAdapter = new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>(LEAGUE_NAME))).Adapter();

            var serverData = new StructureOffset("ServerData",
                new SubPointersSearchStrategy(serverDataAdapter, subStructSize: 0x8000, checkVmt: true),
                maxStructSize: 0x8000);

            inGameState.Child.Add(serverData);

            serverData.AddStringSearch("League Name", "Standard");

            //GUildName
            var serverDataGuildName =
                new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>("Stridemann's GUild"))).Adapter();

            var serverDataGuildSearch = new SubPointersSearchStrategy(serverDataGuildName, subStructSize: 0x8, checkVmt: false);
            serverData.Child.Add(new DataOffset("Guild Name", serverDataGuildSearch));

            serverData.AddIntSearch("Azurite Amount", 0);
            serverData.AddUShortSearch("Sulphite Amount", 0);

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
            //GridPos:218/335
            //WorldPos:2375/3646,739
            //Rotation:1,5708
            //reaction: 1 (byte)

            var player = new StructureOffset("Player", 
                    StringSearch(new StringValueComparer("Metadata/Characters/", StringCompareType.StartWith)).
                        SubStructSearch(0x50, true).
                        SubStructSearch(8,false), 
                    maxStructSize: 0x100);

            #region Entity.ComponentLookup

            var entityComponentLookupSearch =
                new PointersSearchStrategy(new EntityComponentLookupPointerCompare(new List<string> {"Life", "Player"}));

            var entityLookupDeph2 = entityComponentLookupSearch.SubStructSearch(0x100, false);
            var entityLookupDeph1 = entityLookupDeph2.SubStructSearch(0x100, true);

            var entityInternalStruct = new StructureOffset("Entity.ComponentLookup", entityLookupDeph1, 8);
            player.Child.Add(entityInternalStruct);

            entityInternalStruct.OnOffsetsFound += delegate
            {
                entityInternalStruct.DebugInfo = $"({entityLookupDeph1.FoundOffsets.Format()}), " +
                                                 $"({entityLookupDeph2.FoundOffsets.Format()}), " +
                                                 $"({entityComponentLookupSearch.FoundOffsets.Format()})";
            };

            #endregion

            var worldX = new FloatValueCompare(compareValue: 2375, tolerance: 50);
            var worldY = new FloatValueCompare(compareValue: 3646.739f, tolerance: 50);
            var worldPos = new MultipleFloatValueReader(worldX, worldY);
            var worldPosReader = new ValueReaderStrategy(worldPos, alignment: 4);

            var entityPositionedComponent = new StructureOffset("Entity.PositionedComponent",
                new SubPointersSearchStrategy(worldPosReader.Adapter(), subStructSize: 0x100, checkVmt: true),
                maxStructSize: 0x300);

            player.Child.Add(entityPositionedComponent);

            entityPositionedComponent.AddIntSearch("GridPosX", 218);
            entityPositionedComponent.AddIntSearch("GridPosY", 335);
            entityPositionedComponent.AddFloatSearch("WorldPosX", 2375, 50);
            entityPositionedComponent.AddFloatSearch("WorldPosY", 3646.739f, 50);
            entityPositionedComponent.AddFloatSearch("Rotation", 1.570f, 0.02f);
            entityPositionedComponent.AddByteSearch("Reaction", 1);

            inGameData.Child.Add(player);
        }

        private IOffsetSearch StringInSubStruct(string value, int subStructSize, bool checkVmt, bool firstFound = false)
        {
            return SubStructSearch(StringSearch(value, firstFound), subStructSize: subStructSize, checkVmt: checkVmt);
        }

        private IOffsetSearch SubStructSearch(IOffsetSearch offsetSearch, int subStructSize, bool checkVmt)
        {
            return new SubPointersSearchStrategy(offsetSearch.Adapter(), subStructSize, checkVmt);
        }

        private IOffsetSearch StringSearch(string value, bool firstFound = false)
        {
            return new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>(value)), checkVmt: false, firstFound: firstFound);
        }

        private IOffsetSearch StringSearch(IValueCompare<string> comparer, bool firstFound = false)
        {
            return new PointersSearchStrategy(new StringValueReader(comparer), checkVmt: false, firstFound: firstFound);
        }
    }
}
