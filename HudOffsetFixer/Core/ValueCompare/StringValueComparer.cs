using System;
using HudOffsetFixer.Extensions;

namespace HudOffsetFixer.Core.ValueCompare
{
    public class StringValueComparer : IValueCompare<string>
    {
        private readonly StringCompareType _compareType;
        private readonly string _compareValue;

        public StringValueComparer(string compareValue, StringCompareType compareType)
        {
            _compareType = compareType;

            if (_compareType.HasFlagFast(StringCompareType.IgnoreCase))
                _compareValue = compareValue.ToUpper();
            else
                _compareValue = compareValue;
        }

        public bool Compare(string value)
        {
            if (_compareType.HasFlagFast(StringCompareType.IgnoreCase))
            {
                value = value.ToUpper();
            }

            switch (_compareType)
            {
                case StringCompareType.Equals: return value.Equals(_compareValue);
                case StringCompareType.Contains: return value.Contains(_compareValue);
                case StringCompareType.StartWith: return value.StartsWith(_compareValue);
                case StringCompareType.EndsWith: return value.EndsWith(_compareValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [Flags]
    public enum StringCompareType
    {
        Equals = 1 << 0,
        Contains = 1 << 1,
        StartWith = 1 << 2,
        EndsWith = 1 << 3,
        IgnoreCase = 1 << 4
    }
}
