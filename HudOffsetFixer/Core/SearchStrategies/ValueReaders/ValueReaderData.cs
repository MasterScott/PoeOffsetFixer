using System;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    /// <summary>
    /// Do not cache this class! Pointer is no longer valid out of "fixed" statement
    /// </summary>
    public unsafe class ValueReaderData : IComparable<ValueReaderData>
    {
        public byte[] DataArray;
        public int DataArrayPos;
        public void* Ptr;

        public int CompareTo(ValueReaderData other)
        {
            //No implementation. This in only to be assignable to IValueCompare<(IComparable)> 
            throw new NotImplementedException();
        }
    }
}