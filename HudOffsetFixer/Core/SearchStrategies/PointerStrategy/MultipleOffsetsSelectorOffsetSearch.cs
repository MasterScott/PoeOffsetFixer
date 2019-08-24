using System.Collections.Generic;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.PointerStrategy
{
    public class MultipleOffsetsSelectorOffsetSearch : BaseOffsetSearchStrategy, IOffsetSearch
    {
        private readonly IOffsetSearch _subSearch;
        private readonly int _expectedOffsetsCount;
        private readonly int _selectOffsetOnIndex;

        public MultipleOffsetsSelectorOffsetSearch(
            IOffsetSearch subSearch,
            int expectedOffsetsCount,
            int selectOffsetOnIndex) : base(0, false)
        {
            _subSearch = subSearch;
            _expectedOffsetsCount = expectedOffsetsCount;
            _selectOffsetOnIndex = selectOffsetOnIndex;
        }

        public List<int> SearchOffsets(OffsetSearchParams searchParams, Memory memory)
        {
            var results = _subSearch.SearchOffsets(searchParams, memory);

            if (results.Count != _expectedOffsetsCount)
            {
                return new List<int>();
            }

            var selectedOffset = results[_selectOffsetOnIndex];
            FoundOffsets.Add(selectedOffset);
            return new List<int> {selectedOffset};
        }

        public List<int> FoundOffsets { get; set; } = new List<int>();
    }
}
