using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    public class IntValueReader : IValueCompare<ValueReaderData>
    {
        private readonly IValueCompare<int> _valueCompare;

        public IntValueReader(IValueCompare<int> valueCompare)
        {
            _valueCompare = valueCompare;
        }

        public unsafe bool Compare(ValueReaderData value)
        {
            var intPtr = (int*) value.Ptr;
            var intVal = *intPtr;

            return _valueCompare.Compare(intVal);
        }
    }
}
