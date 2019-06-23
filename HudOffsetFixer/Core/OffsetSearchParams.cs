using System.Collections.Generic;
using HudOffsetFixer.Core.SearchStrategies;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;

namespace HudOffsetFixer.Core
{
    public class OffsetSearchParams
    {
        public OffsetSearchParams(byte[] structureBytes, long structAddress)
        {
            StructureBytes = structureBytes;
            StructAddress = structAddress;
        }

        public long StructAddress;
        public byte[] StructureBytes { get; }
        public List<PossiblePointerInfo> PossiblePointers { get; set; }
        public override string ToString()
        {
            return $"StructAddr: {StructAddress:X}, Pointers: {PossiblePointers.Count}";
        }
    }
}
