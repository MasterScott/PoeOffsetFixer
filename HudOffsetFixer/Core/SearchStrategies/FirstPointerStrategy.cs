using System.Collections.Generic;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies
{
    public class FirstPointerStrategy : IOffsetSearch
    {
        public List<int> SearchOffsets(OffsetSearchParams searchParams, Memory memory)
        {
            return new List<int> {0};
        }

        public List<int> FoundOffsets { get; set; } = new List<int> {0};
    }
}
