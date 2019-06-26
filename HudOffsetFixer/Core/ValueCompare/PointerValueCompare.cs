using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;

namespace HudOffsetFixer.Core.ValueCompare
{
    public class PointerValueCompare : IValueCompare<PossiblePointerInfo>
    {
        private readonly IValueCompare<long> _valueCompare;

        public PointerValueCompare(IValueCompare<long> valueCompare)
        {
            _valueCompare = valueCompare;
        }

        public bool Compare(PossiblePointerInfo value)
        {
            return _valueCompare.Compare(value.PointerAddress);
        }
    }
}
