using System;
using System.Collections.Generic;
using PoeHUD.Framework;

namespace PoeOffsetFix.Logic
{
    public class StructureOffset : BaseOffset
    {
        public List<BaseOffset> Childs = new List<BaseOffset>();
        public int MaxStructSize;
        public long BaseAddress;

        public StructureOffset(string name, IResultCompare resultCompare) : base(name, resultCompare)
        {
        }
    }

    public class DataOffset : BaseOffset
    {
        public DataOffset(string name, IResultCompare resultCompare) : base(name, resultCompare)
        {
        }
    }

    public class BaseOffset
    {
        public readonly string Name;
        public readonly IResultCompare ResultCompare;
        public List<int> FoundOffsets = new List<int>();

        public BaseOffset(string name, IResultCompare resultCompare)
        {
            Name = name;
            ResultCompare = resultCompare;
        }
    }

    public interface IResultCompare
    {
        List<int> SearchData(byte[] arrayData, Memory M); //returns offset of found data
    }

    public class BaseResultCompare
    {
        public int Alignment;
        public bool FirstFound;

        public BaseResultCompare(int alignment, bool firstFound)
        {
            Alignment = alignment;
            FirstFound = firstFound;
        }
    }

    public class ByteResultCompare : BaseResultCompare, IResultCompare
    {
        public byte CompareData;

        public ByteResultCompare(int alignment, bool firstFound) : base(alignment, firstFound)
        {
        }

        public List<int> SearchData(byte[] arrayData, Memory M)
        {
            var result = new List<int>();

            for (var i = 0; i < arrayData.Length; i += Alignment)
            {
                var realByte = arrayData[i];

                if (realByte == CompareData)
                    result.Add(i);
            }

            return result;
        }
    }

    public class StringResultCompare : BaseResultCompare, IResultCompare
    {
        public string CompareData;

        public StringResultCompare(bool firstFound, string compareData) : base(8, firstFound)
        {
            CompareData = compareData;
        }

        public List<int> SearchData(byte[] arrayData, Memory M)
        {
            var result = new List<int>();

            for (var i = 0; i < arrayData.Length; i += Alignment)
            {
                var ptrData = BitConverter.ToInt64(arrayData, i);

                if (ptrData == 0)
                    continue;

                //TODO: Check pointer range
                //TODO: Use nativeStringReader

                var realData = M.ReadStringU(ptrData);

                if (realData == CompareData)
                    result.Add(i);
            }

            return result;
        }
    }

    public class OffsetsFixer
    {
        private readonly Memory _memory;
        public StructureOffset InitialStructure { get; }

        public OffsetsFixer(Memory memory, StructureOffset initialStructure)
        {
            _memory = memory;
            InitialStructure = initialStructure;
        }

        public void FixStructureChilds()
        {
            var memBytes = _memory.ReadBytes(InitialStructure.BaseAddress, InitialStructure.MaxStructSize);

            foreach (var structureOffsetChild in InitialStructure.Childs)
            {
                structureOffsetChild.FoundOffsets = structureOffsetChild.ResultCompare.SearchData(memBytes, _memory);
            }
        }
    }
}
