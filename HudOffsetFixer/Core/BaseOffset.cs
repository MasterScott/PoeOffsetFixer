using System.Collections.Generic;
using HudOffsetFixer.Core.SearchStrategies;

namespace HudOffsetFixer.Core
{
    public class BaseOffset : BaseViewModel
    {
        public string Name { get; set; }
        public readonly IOffsetSearch OffsetSearch;
        private int _usedOffset = -1;
        private List<int> _foundOffsets = new List<int>();

        public List<int> FoundOffsets
        {
            get => _foundOffsets;
            set =>  SetProperty(ref _foundOffsets, value);
        }

        public int UsedOffset
        {
            get => _usedOffset;
            set => SetProperty(ref _usedOffset, value);
        }

        public bool OffsetsFound => FoundOffsets.Count > 0;
        public BaseOffset(string name, IOffsetSearch offsetSearch)
        {
            Name = name;
            OffsetSearch = offsetSearch;
        }
    }
}
