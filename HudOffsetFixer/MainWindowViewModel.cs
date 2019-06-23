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

            var inGameState = new StructureOffset("InGameState", null, maxStructSize: 0x1000, baseAddress: 0x582C4630C0);

            AddStringSearch(inGameState, "State Name", "InGameState");
            AddIntSearch(inGameState, "Camera.Width", 2048);
            AddIntSearch(inGameState, "Camera.Height", 1099);

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
            var uiWidthHeightSearch = new ValueReaderStrategy(uiWidthHeightReading, alignment: 8);

            //...create multi value offset search strategy with this strategies.
            //Strategies should goes in order to expected offset order. If no- change the settings of DefaultMultiValueOffsetsFilter
            var uiMultiValueSearch = new MultiValueStrategy(new DefaultMultiValueOffsetsFilter(), uiSelfReferencingPointer, uiChildAmount,
                uiWidthHeightSearch);

            var uiRoot = new StructureOffset("Ui Root", new SubStructSearchStrategy(uiMultiValueSearch, subStructSize: 0x300, checkVmt: true),
                maxStructSize: 0x300);

            inGameState.Child.Add(uiRoot);

            AddIntSearch(uiRoot, "UiRoot.Width", 2560);
            AddIntSearch(uiRoot, "UiRoot.Height", 1600);

            #endregion

           

            #region ServerData

            ServerDataOffsets(inGameState);

            #endregion

            #region IngameData

            var inGameDataAreaNameStructSearch = StringInSubStruct("Lush Hideout", subStructSize: 0x10, checkVmt: false);
            var inGameDataStructSearch = SubStructSearch(inGameDataAreaNameStructSearch, subStructSize: 0x600, checkVmt: true);
            var inGameData = new StructureOffset("InGameData", inGameDataStructSearch, maxStructSize: 0x600);
            inGameState.Child.Add(inGameData);

            inGameData.Child.Add(new DataOffset("Area Name", inGameDataAreaNameStructSearch));
            AddByteSearch(inGameData, "Area Level", 60, false, 8);

            #region Player

            PlayerOffsets(inGameData);

            #endregion

            //var names = Entity.ReadComponentsNames(0x0000007021B47130);

            #endregion

            Nodes.Clear();
            Nodes.Add(inGameState);
            //Nodes.Add(serverData);
            //Nodes.Add(inGameData);
            //Nodes.Add(player);
            _initialState = inGameState;
            //_initialState = player;

        }

        private void ServerDataOffsets(StructureOffset inGameState)
        {
            var serverDataAdapter =
                new SingleOffsetSearchAdapter(new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>(LEAGUE_NAME))));

            var serverData = new StructureOffset("ServerData", new SubStructSearchStrategy(serverDataAdapter, subStructSize: 0x8000, checkVmt: true),
                maxStructSize: 0x8000);

            inGameState.Child.Add(serverData);

            AddStringSearch(serverData, "League Name", "Standard");

            //GUildName
            var serverDataGuildName =
                new SingleOffsetSearchAdapter(
                    new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>("Stridemann's GUild"))));

            var serverDataGuildSearch = new SubStructSearchStrategy(serverDataGuildName, subStructSize: 0x8, checkVmt: false);
            serverData.Child.Add(new DataOffset("Guild Name", serverDataGuildSearch));

            AddIntSearch(serverData, "Azurite Amount", 0);
            AddUShortSearch(serverData, "Sulphite Amount", 0);


            var inventId = new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(1)), 8);
            var arraySize = new PointersSearchStrategy(new ArrayLengthDoublePointerCompare(new DefaultValueCompare<int>(2)));
            var uiMultiValueSearch = new MultiValueStrategy(new DefaultMultiValueOffsetsFilter(), inventId);

            var playerInventories = new SubStructSearchStrategy(uiMultiValueSearch, 0x20, false);
            serverData.Child.Add(new StructureOffset("PlayerInventories(Array)", playerInventories, 0x10));

        }

        private void PlayerOffsets(StructureOffset inGameData)
        {
            //GridPos:218/335
            //WorldPos:2375/3646,739
            //Rotation:1,5708
            //reaction: 1 (byte)

            var player = new StructureOffset("Player",
                SubStructSearch(
                    SubStructSearch(StringSearch(new StringValueComparer("Metadata/Characters/", StringCompareType.StartWith)), 0x50, true), 8,
                    false), maxStructSize: 0x100);

            #region Entity.ComponentLookup

            var entityComponentLookupSearch =
                new PointersSearchStrategy(new EntityComponentLookupPointerCompare(new List<string> {"Life", "Player"}));

            var entityLookupDeph2 = SubStructSearch(entityComponentLookupSearch, 0x100, false);
            var entityLookupDeph1 = SubStructSearch(entityLookupDeph2, 0x100, true);

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
                new SubStructSearchStrategy(new SingleOffsetSearchAdapter(worldPosReader), subStructSize: 0x100, checkVmt: true),
                maxStructSize: 0x300);
            player.Child.Add(entityPositionedComponent);

            AddIntSearch(entityPositionedComponent, "GridPosX", 218);
            AddIntSearch(entityPositionedComponent, "GridPosY", 335);
            AddFloatSearch(entityPositionedComponent, "WorldPosX", 2375, 50);
            AddFloatSearch(entityPositionedComponent, "WorldPosY", 3646.739f, 50);
            AddFloatSearch(entityPositionedComponent, "Rotation", 1.570f, 0.02f);
            AddByteSearch(entityPositionedComponent, "Reaction", 1);

            inGameData.Child.Add(player);
        }

        private IOffsetSearch StringInSubStruct(string value, int subStructSize, bool checkVmt, bool firstFound = false)
        {
            return SubStructSearch(StringSearch(value, firstFound), subStructSize: subStructSize, checkVmt: checkVmt);
        }

        private IOffsetSearch SubStructSearch(IOffsetSearch offsetSearch, int subStructSize, bool checkVmt)
        {
            return new SubStructSearchStrategy(new SingleOffsetSearchAdapter(offsetSearch), subStructSize, checkVmt);
        }

        private IOffsetSearch StringSearch(string value, bool firstFound = false)
        {
            return new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>(value)), checkVmt: false, firstFound: firstFound);
        }

        private IOffsetSearch StringSearch(IValueCompare<string> comparer, bool firstFound = false)
        {
            return new PointersSearchStrategy(new StringValueReader(comparer), checkVmt: false, firstFound: firstFound);
        }

        private void AddStringSearch(StructureOffset structOffset, string offsetName, string value, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>(value)), firstFound)));
        }

        private void AddIntSearch(StructureOffset structOffset, string offsetName, int value, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(value)), sizeof(int), firstFound: firstFound)));
        }

        private void AddFloatSearch(StructureOffset structOffset, string offsetName, float value, float tolerance, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new FloatValueReader(new FloatValueCompare(value, tolerance)), sizeof(int), firstFound: firstFound)));
        }

        private void AddByteSearch(StructureOffset structOffset, string offsetName, byte value, bool firstFound = false, int alignment = 1)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new ByteValueReader(new DefaultValueCompare<byte>(value)), alignment, firstFound: firstFound)));
        }

        private void AddUShortSearch(StructureOffset structOffset, string offsetName, ushort value, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new UShortValueReader(new DefaultValueCompare<ushort>(value)), sizeof(ushort), firstFound: firstFound)));
        }
    }
}
