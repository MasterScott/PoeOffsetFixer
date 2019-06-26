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
        private OffsetSearchStatus _searchStatus;

        public List<int> FoundOffsets
        {
            get => _foundOffsets;
            set => SetProperty(ref _foundOffsets, value);
        }

        public int UsedOffset
        {
            get => _usedOffset;
            set => SetProperty(ref _usedOffset, value);
        }

        public OffsetSearchStatus SearchStatus
        {
            get => _searchStatus;
            set => SetProperty(ref _searchStatus, value);
        }

        public bool OffsetIsFound => FoundOffsets.Count == 1;

        public BaseOffset(string name, IOffsetSearch offsetSearch)
        {
            Name = name;
            OffsetSearch = offsetSearch;
        }
    }

    public enum OffsetSearchStatus
    {
        Unknown,
        NotFound,
        FoundSingle,
        FoundMultiple
    }
}
