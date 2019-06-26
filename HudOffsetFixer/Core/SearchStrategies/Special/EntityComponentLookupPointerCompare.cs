using System.Collections.Generic;
using System.Linq;
using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;
using HudOffsetFixer.Core.ValueCompare;
using HudOffsetFixer.PoeStructs;

namespace HudOffsetFixer.Core.SearchStrategies.Special
{
    public class EntityComponentLookupPointerCompare : IValueCompare<PossiblePointerInfo>
    {
        private readonly List<string> _expectedComponents;

        public EntityComponentLookupPointerCompare(List<string> expectedComponents)
        {
            _expectedComponents = expectedComponents;
        }

        public bool Compare(PossiblePointerInfo value)
        {
            var components = Entity.ReadComponentsNames(value.PointerAddress);

            if (components.Count == 0)
                return false;

            var contains = _expectedComponents.All(x => components.Contains(x));

            if (contains)
                return true;//for debugging purposes (breakpoint)

            return contains;
        }
    }
}
