namespace HudOffsetFixer.Core.SearchStrategies
{
    public abstract class BaseOffsetSearchStrategy
    {
        public int Alignment { get; }
        public bool FirstFound;

        protected BaseOffsetSearchStrategy(int alignment, bool firstFound)
        {
            Alignment = alignment;
            FirstFound = firstFound;
        }
    }
}