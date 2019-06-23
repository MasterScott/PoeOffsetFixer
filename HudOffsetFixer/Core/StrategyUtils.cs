using HudOffsetFixer.Core.SearchStrategies;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;
using HudOffsetFixer.Core.SearchStrategies.ValueReaders;
using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core
{
    public static class StrategyUtils
    {
        public static ISubStructOffsetSearch Adapter(this IOffsetSearch offsetSearch)
        {
            return new SingleOffsetSearchAdapter(offsetSearch);
        }

        public static IOffsetSearch SubStructSearch(this IOffsetSearch offsetSearch, int subStructSize, bool checkVmt)
        {
            return new SubPointersSearchStrategy(offsetSearch.Adapter(), subStructSize, checkVmt);
        }

        public static void AddStringSearch(this StructureOffset structOffset, string offsetName, string value, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new PointersSearchStrategy(new StringValueReader(new DefaultValueCompare<string>(value)), firstFound)));
        }

        public static void AddIntSearch(this StructureOffset structOffset, string offsetName, int value, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new IntValueReader(new DefaultValueCompare<int>(value)), sizeof(int), firstFound: firstFound)));
        }

        public static void AddFloatSearch(this StructureOffset structOffset, string offsetName, float value, float tolerance, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new FloatValueReader(new FloatValueCompare(value, tolerance)), sizeof(int), firstFound: firstFound)));
        }

        public static void AddByteSearch(this StructureOffset structOffset, string offsetName, byte value, bool firstFound = false, int alignment = 1)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new ByteValueReader(new DefaultValueCompare<byte>(value)), alignment, firstFound: firstFound)));
        }

        public static void AddUShortSearch(this StructureOffset structOffset, string offsetName, ushort value, bool firstFound = false)
        {
            structOffset.Child.Add(new DataOffset(offsetName,
                new ValueReaderStrategy(new UShortValueReader(new DefaultValueCompare<ushort>(value)), sizeof(ushort), firstFound: firstFound)));
        }
    }
}
