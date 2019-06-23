using System.Collections.Generic;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.PointerStrategy
{
    public class SingleOffsetSearchAdapter : ISubStructOffsetSearch
    {
        private readonly IOffsetSearch _offsetSearch;
        public List<int> FoundOffsets;

        public SingleOffsetSearchAdapter(IOffsetSearch offsetSearch)
        {
            _offsetSearch = offsetSearch;
        }

        public bool Search(OffsetSearchParams searchParams, Memory memory)
        {
            FoundOffsets = _offsetSearch.SearchOffsets(searchParams, memory);
            return FoundOffsets.Count > 0;
        }
    }
}
