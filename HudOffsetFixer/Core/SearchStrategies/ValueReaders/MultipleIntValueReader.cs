using System.Linq;
using HudOffsetFixer.Core.ValueCompare;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    /// <summary>
    /// Read multiple ints that goes sequentially one after another. Strict order
    /// </summary>
    public class MultipleIntValueReader : IValueCompare<ValueReaderData>
    {
        private readonly IValueCompare<int>[] _comparers;

        public MultipleIntValueReader(params IValueCompare<int>[] comparers)
        {
            _comparers = comparers;
        }

        public unsafe bool Compare(ValueReaderData value)
        {
            var intPtr = (int*) value.Ptr;
            return _comparers.All(x => x.Compare(*intPtr++));
        }
    }
}
