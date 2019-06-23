using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    public class FloatValueReader : IValueCompare<ValueReaderData>
    {
        private readonly IValueCompare<float> _valueCompare;

        public FloatValueReader(IValueCompare<float> valueCompare)
        {
            _valueCompare = valueCompare;
        }

        public unsafe bool Compare(ValueReaderData value)
        {
            var intPtr = (float*) value.Ptr;
            var intVal = *intPtr;

            return _valueCompare.Compare(intVal);
        }
    }
}
