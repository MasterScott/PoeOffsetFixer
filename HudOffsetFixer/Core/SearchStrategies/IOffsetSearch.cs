using System.Collections.Generic;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies
{
    public interface IOffsetSearch
    {
        List<int> SearchOffsets(OffsetSearchParams searchParams, Memory memory); //returns all found data offsets
        List<int> FoundOffsets { get; set; }
    }
}