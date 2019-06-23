using System.Collections.Generic;
using System.Diagnostics.Contracts;
using HudOffsetFixer.Core.Utils;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.PointerStrategy
{
    public class SubPointersSearchStrategy : IOffsetSearch
    {
        private readonly bool _checkVMT;
        private readonly ISubStructOffsetSearch _subStructSearch;
        private readonly int _subStructSize;

        public SubPointersSearchStrategy(ISubStructOffsetSearch subStructSearch, int subStructSize, bool checkVmt)
        {
            _subStructSearch = subStructSearch;
            _subStructSize = subStructSize;
            _checkVMT = checkVmt;
        }

        public List<int> SearchOffsets(OffsetSearchParams searchParams, Memory memory)
        {
            Contract.Assume(searchParams != null);
            Contract.Assume(searchParams.StructureBytes != null);
            Contract.Assume(_subStructSearch != null);
            Contract.Assume(memory != null);

            if (searchParams.PossiblePointers == null)
                searchParams.PossiblePointers = Helpers.SearchPossiblePointers(searchParams.StructureBytes, searchParams.StructAddress);

            var result = new List<int>();

            foreach (var searchParamsPossiblePointer in searchParams.PossiblePointers)
            {
                if (_checkVMT && !searchParamsPossiblePointer.HasVMT)
                    continue;

                var bytes = memory.ReadBytes(searchParamsPossiblePointer.PointerAddress, _subStructSize);
                var subSearchParams = new OffsetSearchParams(bytes, searchParamsPossiblePointer.PointerAddress);

                if (_subStructSearch.Search(subSearchParams, memory))
                    result.Add(searchParamsPossiblePointer.Offset);
            }

            FoundOffsets.AddRange(result);
            return result;
        }

        public List<int> FoundOffsets { get; set; } = new List<int>();
    }
}
