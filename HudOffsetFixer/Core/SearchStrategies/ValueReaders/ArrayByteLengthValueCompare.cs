using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    public class ArrayByteLengthValueCompare : IValueCompare<ValueReaderData>
    {
        private readonly int _arrayByteLength;

        public ArrayByteLengthValueCompare(int arrayByteLength)
        {
            _arrayByteLength = arrayByteLength;
        }

        public unsafe bool Compare(ValueReaderData value)
        {
            var intPtr = (long*) value.Ptr;
            var addr1 = *intPtr;
            intPtr++;
            var addr2 = *intPtr;
            return addr2 - addr1 == _arrayByteLength;
        }
    }
}
