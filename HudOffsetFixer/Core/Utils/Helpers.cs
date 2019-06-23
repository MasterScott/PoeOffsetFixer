using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;
using HudOffsetFixer.MemorySections;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.Utils
{
    public class Helpers
    {
        public static unsafe List<PossiblePointerInfo> SearchPossiblePointers(byte[] data, long structBaseAddress)
        {
            Contract.Assume(data != null);

            var result = new List<PossiblePointerInfo>();

            fixed (void* ptr = &data[0])
            {
                var longPtr = (long*) ptr;

                for (var i = 0; i < data.Length / sizeof(long); i++)
                {
                    var ptrValue = *longPtr;
                    var section = MemorySectionsProcessor.Instance.GetSectionToPointer((IntPtr) ptrValue);

                    if (section != null)
                    {
                        if (section.Category == SectionCategory.HEAP)
                            result.Add(new PossiblePointerInfo(ptrValue, i * sizeof(long), section, structBaseAddress));
                    }

                    longPtr++;
                }
            }

            return result;
        }

        public static bool StructHasVMT(long structAddress)
        {
            var vmtAddr = Memory.Instance.ReadLong(structAddress);

            if (vmtAddr <= 0)
                return false;

            var section = MemorySectionsProcessor.Instance.GetSectionToPointer((IntPtr) vmtAddr);

            if (section == null)
                return false;

            return section.Category == SectionCategory.DATA;
        }
    }
}
