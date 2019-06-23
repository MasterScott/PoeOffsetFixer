using System;

namespace HudOffsetFixer.Core.ValueCompare
{
    public class FloatValueCompare : IValueCompare<float>
    {
        private readonly float _compareValue;
        private readonly float _tolerance;

        public FloatValueCompare(float compareValue, float tolerance)
        {
            _compareValue = compareValue;
            _tolerance = tolerance;
        }

        public bool Compare(float value)
        {
            return Math.Abs(value - _compareValue) < _tolerance;
        }
    }
}
