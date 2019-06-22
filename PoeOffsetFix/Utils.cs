using System;

namespace PoeOffsetFix
{
    public class Utils
    {
        public static long ToAddr(string hex) => Convert.ToInt64(hex, 16);
    }
}
