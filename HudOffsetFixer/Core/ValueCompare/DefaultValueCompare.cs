using System;

namespace HudOffsetFixer.Core.ValueCompare
{
    public class DefaultValueCompare<T> : IValueCompare<T> where T : IComparable<T>
    {
        private readonly T _compareValue;

        public DefaultValueCompare(T compareValue)
        {
            _compareValue = compareValue;
        }

        public bool Compare(T value)
        {
            return value.Equals(_compareValue);
        }
    }
}
