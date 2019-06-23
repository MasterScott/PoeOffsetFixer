using HudOffsetFixer.Core.SearchStrategies;

namespace HudOffsetFixer.Core
{
    /// <summary>
    /// This is actually the data (some property) offset, mostly of primitive type 
    /// </summary>
    /// <seealso cref="BaseOffset" />
    public class DataOffset : BaseOffset
    {
        public DataOffset(string name, IOffsetSearch offsetSearch) : base(name, offsetSearch)
        {
        }
    }
}