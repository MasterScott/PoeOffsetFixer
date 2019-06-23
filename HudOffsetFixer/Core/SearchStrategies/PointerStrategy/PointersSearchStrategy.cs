using System.Collections.Generic;
using System.Diagnostics.Contracts;
using HudOffsetFixer.Core.Utils;
using HudOffsetFixer.Core.ValueCompare;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.PointerStrategy
{
    public class PointersSearchStrategy : BaseOffsetSearchStrategy, IOffsetSearch
    {
        private readonly bool _checkVMT;
        private readonly IValueCompare<PossiblePointerInfo> _valueComparer;

        public PointersSearchStrategy(IValueCompare<PossiblePointerInfo> valueComparer, bool checkVmt = false, bool firstFound = false) : base(8,
            firstFound)
        {
            _valueComparer = valueComparer;
            _checkVMT = checkVmt;
        }

        public List<int> SearchOffsets(OffsetSearchParams searchParams, Memory memory)
        {
            Contract.Assume(searchParams != null);
            Contract.Assume(searchParams.StructureBytes != null);
            Contract.Assume(_valueComparer != null);
            Contract.Assume(memory != null);

            //if (searchParams.StructAddress == 0x00000058946BDE60)
            //{
            //    var t = 0;
            //}

            if (searchParams.PossiblePointers == null)
                searchParams.PossiblePointers = Helpers.SearchPossiblePointers(searchParams.StructureBytes, searchParams.StructAddress);

            var result = new List<int>();

            foreach (var searchParamsPossiblePointer in searchParams.PossiblePointers)
            {
                if (_checkVMT && !searchParamsPossiblePointer.HasVMT)
                    continue;

                if (_valueComparer.Compare(searchParamsPossiblePointer))
                {
                    result.Add(searchParamsPossiblePointer.Offset);

                    if (FirstFound)
                        break;
                }
            }

            FoundOffsets.AddRange(result);
            return result;
        }

        public List<int> FoundOffsets { get; set; } = new List<int>();
    }
}
