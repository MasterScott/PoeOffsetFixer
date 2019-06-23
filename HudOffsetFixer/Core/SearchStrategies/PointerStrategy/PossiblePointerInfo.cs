using System;
using HudOffsetFixer.Core.Utils;
using HudOffsetFixer.MemorySections;

namespace HudOffsetFixer.Core.SearchStrategies.PointerStrategy
{
    public class PossiblePointerInfo : IComparable<PossiblePointerInfo>
    {
        public long OwnerStructAddress;
        public long PointerAddress;
        public int Offset;
        public Section MemorySection;
        private bool? _hasVmt;

        public bool HasVMT
        {
            get
            {
                if (!_hasVmt.HasValue)
                {
                    _hasVmt = Helpers.StructHasVMT(PointerAddress);
                }

                return _hasVmt.Value;
            }
        }

        public PossiblePointerInfo(long pointerAddress, int offset, Section memorySection, long ownerStructAddress)
        {
            PointerAddress = pointerAddress;
            Offset = offset;
            MemorySection = memorySection;
            OwnerStructAddress = ownerStructAddress;
        }

        public int CompareTo(PossiblePointerInfo other)
        {
            //No implementation. This in only to be assignable to IValueCompare<(IComparable)> 
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"StructAddr: {OwnerStructAddress:X}, Pointer: {PointerAddress:X}({MemorySection.Category}), Offset: {Offset}";
        }
    }
}
