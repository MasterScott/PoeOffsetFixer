using PoeHUD.Framework;

namespace HudOffsetFixer.Core
{
    public class OffsetsFixer
    {
        private readonly Memory _memory;

        public OffsetsFixer(Memory memory)
        {
            _memory = memory;
        }

        public void FixStructureChilds(StructureOffset initialStructure)
        {
            initialStructure.StructureBytes = _memory.ReadBytes(initialStructure.BaseAddress, initialStructure.MaxStructSize);

            foreach (var structureOffsetChild in initialStructure.Child)
            {
                var searchParams = new OffsetSearchParams(initialStructure.StructureBytes, initialStructure.BaseAddress);
                structureOffsetChild.FoundOffsets = structureOffsetChild.OffsetSearch.SearchOffsets(searchParams, _memory);
                structureOffsetChild.UsedOffset = structureOffsetChild.OffsetIsFound ? structureOffsetChild.FoundOffsets[0] : -1;

                var offsetsCount = structureOffsetChild.FoundOffsets.Count;

                if (offsetsCount == 0)
                {
                    structureOffsetChild.SearchStatus = OffsetSearchStatus.NotFound;
                }
                else if (offsetsCount == 1)
                {
                    structureOffsetChild.SearchStatus = OffsetSearchStatus.FoundSingle;
                }
                else if (offsetsCount > 1)
                {
                    structureOffsetChild.SearchStatus = OffsetSearchStatus.FoundMultiple;
                }

                if (structureOffsetChild is StructureOffset structureOffset)
                {
                    if (structureOffset.ShouldProcess() && structureOffsetChild.FoundOffsets.Count == 1)
                    {
                        structureOffset.BaseAddress = _memory.ReadLong(initialStructure.BaseAddress + structureOffsetChild.UsedOffset);
                        FixStructureChilds(structureOffset);
                        structureOffset.OnOffsetsFound();
                    }
                    else
                    {
                        structureOffset.FoundOffsets.Clear();
                        structureOffset.BaseAddress = -1;
                        structureOffsetChild.UsedOffset = -1;
                    }
                }
            }
        }
    }
}
