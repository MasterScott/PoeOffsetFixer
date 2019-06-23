using System;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;

namespace HudOffsetFixer.Core.ValueCompare
{
    public class ArrayLengthDoublePointerCompare : IValueCompare<PossiblePointerInfo>
    {
        private readonly IValueCompare<int> _arrayLengthCompare; //this can be used as range compare (not only const value)

        public ArrayLengthDoublePointerCompare(IValueCompare<int> arrayLengthCompare)
        {
            _arrayLengthCompare = arrayLengthCompare;
        }

        private long _prevPointerAddress;
        private int _prevPointerOffset;

        public bool Compare(PossiblePointerInfo value)
        {
            //pointers should goes one after another sequentially
            if (_prevPointerAddress == 0 || value.Offset - _prevPointerOffset != 8) 
            {
                _prevPointerAddress = value.PointerAddress;
                _prevPointerOffset = value.Offset;
                return false;
            }

            var pointersDiff = value.PointerAddress - _prevPointerAddress;
            var arrayLength = (int) (pointersDiff / sizeof(long));

            if (_arrayLengthCompare.Compare(arrayLength))
            {
                return true;
            }

            _prevPointerAddress = value.PointerAddress;
            _prevPointerOffset = value.Offset;

            return false;
        }
    }
}
