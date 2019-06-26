using System;
using System.Collections.Generic;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.MultiValue
{
    public class MultiValueStrategy : ISubStructOffsetSearch
    {
        private readonly IOffsetSearch[] _offsetSearches;
        private readonly IMultiValueOffsetsFilter _offsetsFilter;

        public MultiValueStrategy(IMultiValueOffsetsFilter offsetsFilter, params IOffsetSearch[] offsetSearches)
        {
            _offsetSearches = offsetSearches;
            _offsetsFilter = offsetsFilter;
        }

        public bool Search(OffsetSearchParams searchParams, Memory memory)
        {
            var offsetGroups = new List<List<int>>();

            foreach (var offsetSearch in _offsetSearches)
            {
                var offsetGroup = offsetSearch.SearchOffsets(searchParams, memory);
                offsetGroups.Add(offsetGroup);
            }

            return _offsetsFilter.Pass(offsetGroups);
        }
    }

    [Flags]
    public enum MultiValueOffsetsFilterType
    {
        Default = 0,
        StrictOrder = 1,
        AllAtLeastOneOffset = 2
    }
}
