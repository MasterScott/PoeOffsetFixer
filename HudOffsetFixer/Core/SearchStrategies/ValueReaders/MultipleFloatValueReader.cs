using System.Linq;
using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    /// <summary>
    /// Read multiple floats that goes sequentially one after another. Strict order
    /// </summary>
    public class MultipleFloatValueReader : IValueCompare<ValueReaderData>
    {
        private readonly IValueCompare<float>[] _comparers;

        public MultipleFloatValueReader(params IValueCompare<float>[] comparers)
        {
            _comparers = comparers;
        }

        public unsafe bool Compare(ValueReaderData value)
        {
            var floatPtr = (float*) value.Ptr;
            return _comparers.All(x => x.Compare(*floatPtr++));
        }
    }
}
