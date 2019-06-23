using System;

namespace HudOffsetFixer.Core.ValueCompare
{
    public interface IValueCompare<in T> where T : IComparable<T>
    {
        bool Compare(T value);
    }
}
