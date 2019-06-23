using HudOffsetFixer.Core.SearchStrategies.PointerStrategy;
using HudOffsetFixer.Core.ValueCompare;
using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.ValueReaders
{
    public class StringValueReader : IValueCompare<PossiblePointerInfo>
    {
        private readonly IValueCompare<string> _compareValue;

        public StringValueReader(IValueCompare<string> compareValue)
        {
            _compareValue = compareValue;
        }

        public bool Compare(PossiblePointerInfo pointerData)
        {
            var value = Memory.Instance.ReadStringU(pointerData.PointerAddress);

            //TODO: Here can be a string optimization (in poe), see NativeStringReader in HUD code.
            //Perfectly it should be implemented too. Example: in Player component -> Name
            //or just read string directly without reading address using:
            //var realData = M.ReadStringU(pointerData.StructBaseAddress + pointerData.Offset)
            //TODO: Make this optionally

            if (_compareValue.Compare(value))
                return true; //for debug purposes (breakpoint)

            return false;
        }
    }
}
