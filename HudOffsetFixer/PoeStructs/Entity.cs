using System;
using System.Collections.Generic;
using PoeHUD.Framework;

namespace HudOffsetFixer.PoeStructs
{
    public class Entity
    {
        public static long VTablePtr;

        public static List<string> ReadComponentsNames(long componentLookup)
        {
            var M = Memory.Instance;
            var result = new List<string>();

            // the first address is a base object that doesn't contain a component, so read the first component
            var address = M.ReadLong(componentLookup);
            var stuckCounter = 0;

            while (address != componentLookup && address != 0 && address != -1 && stuckCounter++ < 50)
            {
                var name = M.ReadString(M.ReadLong(address + 0x10));
                var nameStart = name;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    var arr = name.ToCharArray();
                    arr = Array.FindAll(arr, c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-');
                    name = new string(arr);
                }

                if (string.IsNullOrWhiteSpace(name) || name != nameStart)
                    break;

                if (!result.Contains(name) && !string.IsNullOrWhiteSpace(name))
                    result.Add(name);

                address = M.ReadLong(address);
            }

            return result;
        }
    }
}
