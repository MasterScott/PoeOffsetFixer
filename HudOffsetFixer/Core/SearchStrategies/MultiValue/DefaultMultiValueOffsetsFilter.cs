using System.Collections.Generic;
using System.Linq;

namespace HudOffsetFixer.Core.SearchStrategies.MultiValue
{
    public class DefaultMultiValueOffsetsFilter : IMultiValueOffsetsFilter
    {
        private readonly MultiValueOffsetsFilterType _filterType;

        public DefaultMultiValueOffsetsFilter(
            MultiValueOffsetsFilterType filterType = MultiValueOffsetsFilterType.AllAtLeastOneOffset | MultiValueOffsetsFilterType.StrictOrder)
        {
            _filterType = filterType;
        }

        public bool Pass(List<List<int>> offsetGroups)
        {
            if ((_filterType & MultiValueOffsetsFilterType.AllAtLeastOneOffset) != 0)
            {
                if (offsetGroups.Any(x => x.Count == 0))
                    return false;
            }

            //if (offsets.Count > 1)//no sense because this filter always will have at least 2 offset groups
            {
                if ((_filterType & MultiValueOffsetsFilterType.StrictOrder) != 0)
                {
                    //The main idea is to check that previous offset range be less than current, so probably the prev offset will be less than current
                    var previous = offsetGroups[0];
                    for (var i = 1; i < offsetGroups.Count; i++)
                    {
                        var offset = offsetGroups[i];

                        if (previous.Min() > offset.Max())
                            return false;

                        previous = offset;
                    }
                }
            }

            return true;
        }
    }
}