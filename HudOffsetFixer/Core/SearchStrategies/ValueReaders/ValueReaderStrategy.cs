using System.Collections.Generic;
using HudOffsetFixer.Core.ValueCompare;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    /// <summary>
    ///     Read and returns a void* ptr according to alignment
    /// </summary>
    public class ValueReaderStrategy : BaseOffsetSearchStrategy, IOffsetSearch
    {
        protected readonly IValueCompare<ValueReaderData> _valueCompare;
        private int _skipStartValues;
        private int _maxReadValues;

        public ValueReaderStrategy(
            IValueCompare<ValueReaderData> valueCompare,
            int alignment,

            //int maxReadValues = -1, 
            //int skipStartValues = 0,
            bool firstFound = false) :
            base(alignment, firstFound)
        {
            _valueCompare = valueCompare;

            //_maxReadValues = maxReadValues;
            //_skipStartValues = skipStartValues;
        }

        public unsafe List<int> SearchOffsets(OffsetSearchParams searchParams, Memory memory)
        {
            var result = new List<int>();

            var data = new ValueReaderData
            {
                DataArray = searchParams.StructureBytes
            };

            fixed (void* ptr = searchParams.StructureBytes)
            {
                var intPtr = (byte*) ptr;

                //intPtr += _skipStartValues * Alignment;

                for (var i = 0; i < searchParams.StructureBytes.Length - Alignment; i += Alignment)
                {
                    data.DataArrayPos = i;
                    data.Ptr = intPtr;

                    if (_valueCompare.Compare(data))
                        result.Add(i);

                    intPtr += Alignment;
                }
            }

            FoundOffsets.AddRange(result);
            return result;
        }

        public List<int> FoundOffsets { get; set; } = new List<int>();
    }
}
