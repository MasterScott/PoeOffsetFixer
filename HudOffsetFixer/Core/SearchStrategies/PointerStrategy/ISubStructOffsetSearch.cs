using PoeHUD.Framework;

namespace HudOffsetFixer.Core.SearchStrategies.PointerStrategy
{
    public interface ISubStructOffsetSearch
    {
        bool Search(OffsetSearchParams searchParams, Memory memory);
    }
}
