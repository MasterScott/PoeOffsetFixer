using System.Collections.Generic;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.Special
{
    public class ReturnExistingOffsetStrategy : IOffsetSearch
    {
        private readonly IOffsetSearch _source;
        private readonly int _applyOffset;

        public ReturnExistingOffsetStrategy(IOffsetSearch source, int applyOffset = 0)
        {
            _source = source;
            _applyOffset = applyOffset;
        }

        public List<int> SearchOffsets(OffsetSearchParams searchParams, Memory memory)
        {
            if (_source.FoundOffsets.Count == 1)
            {
                return new List<int>{_source.FoundOffsets[0] + _applyOffset};
            }
            else if (_source.FoundOffsets.Count > 1)
            {
                return new List<int>();
            }

            return _source.FoundOffsets;
        }

        public List<int> FoundOffsets { get; set; }
    }
}
