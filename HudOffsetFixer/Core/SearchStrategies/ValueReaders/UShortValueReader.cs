using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    public class UShortValueReader : IValueCompare<ValueReaderData>
    {
        private readonly IValueCompare<ushort> _valueCompare;

        public UShortValueReader(IValueCompare<ushort> valueCompare)
        {
            _valueCompare = valueCompare;
        }

        public unsafe bool Compare(ValueReaderData value)
        {
            var intPtr = (ushort*) value.Ptr;
            var intVal = *intPtr;

            return _valueCompare.Compare(intVal);
        }
    }
}
