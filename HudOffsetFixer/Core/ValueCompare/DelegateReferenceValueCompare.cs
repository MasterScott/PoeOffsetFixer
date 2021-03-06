﻿using System;

namespace HudOffsetFixer.Core.ValueCompare
{
    public class DelegateReferenceValueCompare<T> : IValueCompare<T> where T : IComparable<T>
    {
        private readonly Func<T> _compareValue;

        public DelegateReferenceValueCompare(Func<T> compareValue)
        {
            _compareValue = compareValue;
        }

        public bool Compare(T value)
        {
            return value.Equals(_compareValue());
        }
    }
}
