using System;
using System.Collections.Generic;
using HudOffsetFixer.Core.SearchStrategies;

namespace HudOffsetFixer.Core
{
    public class StructureOffset : BaseOffset
    {
        private long _baseAddress;
        public List<BaseOffset> Child { get; set; } = new List<BaseOffset>();
        public int MaxStructSize { get; set; }
        public string DebugInfo { get; set; }

        public Action OnOffsetsFound = delegate {};
        public Func<bool> ShouldProcess = () => true;

        public long BaseAddress
        {
            get => _baseAddress;
            set => SetProperty(ref _baseAddress, value);
        }

        public byte[] StructureBytes { get; set; }
        public StructureOffset(string name, IOffsetSearch offsetSearch, int maxStructSize, long baseAddress = -1) : base(name, offsetSearch)
        {
            MaxStructSize = maxStructSize;
            BaseAddress = baseAddress;
        }
    }
}
