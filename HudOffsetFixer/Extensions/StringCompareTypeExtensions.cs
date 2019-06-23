using System.Collections.Generic;
using System.Linq;
using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Extensions
{
    public static class StringCompareTypeExtensions
    {
        public static bool HasFlagFast(this StringCompareType value, StringCompareType flag)
        {
            return (value & flag) != 0;
        }

        public static string Format(this List<int> offsets)
        {
            if (offsets == null)
                return "null";
            return string.Join(", ", offsets.Select(x => $"0x{x:X}"));
        }
    }
}