using System;
using System.Collections.Generic;
using PoeHUD.Framework;

namespace HudOffsetFixer.PoeStructs
{
    public class Entity
    {
        public static int ComponentLookupOffset1;
        public static int ComponentLookupOffset2;
        public static int ComponentLookupOffset3;
        public static int ComponentListOffset;

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

        public static Dictionary<string, long> GetComponents(long entityAddress)
        {
            var M = Memory.Instance;
            var dictionary = new Dictionary<string, long>();
            var componentLookup = M.ReadLong(entityAddress + ComponentLookupOffset1, ComponentLookupOffset2, ComponentLookupOffset3);
            var componentList = M.ReadLong(entityAddress + ComponentListOffset);

            // the first address is a base object that doesn't contain a component, so read the first component
            var address = M.ReadLong(componentLookup);

            while (address != componentLookup && address != 0 && address != -1)
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

                var componentAddress = M.ReadLong(componentList + M.ReadInt(address + 0x18) * 8);

                if (!dictionary.ContainsKey(name) && !string.IsNullOrWhiteSpace(name))
                    dictionary.Add(name, componentAddress);

                address = M.ReadLong(address);
            }

            return dictionary;
        }

        public static long GetComponentAddress(long entityAddress, string componentName)
        {
            var comps = Entity.GetComponents(entityAddress);

            if (!comps.TryGetValue(componentName, out var compAddress))
            {
                LogWindow.LogError($"Component is not found: {componentName}. EntityAddres: {entityAddress:X}");
                return -1;
            }

            return compAddress;
        }
    }
}
