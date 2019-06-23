using System.Collections.Generic;

namespace HudOffsetFixer.Core.SearchStrategies.PointerStrategy.MultiValue
{
    public interface IMultiValueOffsetsFilter
    {
        bool Pass(List<List<int>> offsetGroups);
    }
}