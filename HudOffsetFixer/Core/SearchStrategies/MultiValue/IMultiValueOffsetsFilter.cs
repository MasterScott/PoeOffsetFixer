using System.Collections.Generic;

namespace HudOffsetFixer.Core.SearchStrategies.MultiValue
{
    public interface IMultiValueOffsetsFilter
    {
        bool Pass(List<List<int>> offsetGroups);
    }
}