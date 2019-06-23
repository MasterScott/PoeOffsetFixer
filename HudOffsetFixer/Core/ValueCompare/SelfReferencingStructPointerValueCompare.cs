using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;

namespace HudOffsetFixer.Core.ValueCompare
{
    public class SelfReferencingStructPointerValueCompare : IValueCompare<PossiblePointerInfo>
    {
        public bool Compare(PossiblePointerInfo value)
        {
            return value.OwnerStructAddress == value.PointerAddress;
        }
    }
}
