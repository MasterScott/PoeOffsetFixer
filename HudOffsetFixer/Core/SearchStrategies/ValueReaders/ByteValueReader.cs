using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    public class ByteValueReader : IValueCompare<ValueReaderData>
    {
        private readonly IValueCompare<byte> _valueCompare;

        public ByteValueReader(IValueCompare<byte> valueCompare)
        {
            _valueCompare = valueCompare;
        }

        public unsafe bool Compare(ValueReaderData value)
        {
            var intPtr = (byte*) value.Ptr;
            var intVal = *intPtr;

            return _valueCompare.Compare(intVal);
        }
    }
}
