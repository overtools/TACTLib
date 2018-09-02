using System.IO;
using TACTLib.Container;
using TACTLib.Helpers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace TACTLib.Core.Product.WorldOfWarcraft {
    public class CASWarcraftV6 {
        public int RecordCount { get; }
        public ContentFlags Flags { get; }
        public Locale Locale { get; }
        public int[] Delta { get; }
        public CASWarcraftV6Record[] Records { get; }

        public CASWarcraftV6(BinaryReader reader) {
            RecordCount = reader.ReadInt32();
            Flags = (ContentFlags)reader.ReadUInt32();
            Locale = (Locale)reader.ReadUInt32();
            Delta = reader.ReadArray<int>(RecordCount);
            Records =  reader.ReadArray<CASWarcraftV6Record>(RecordCount);
        }

        public int FileId(int index) {
            return index == 0 ? Delta[index] : FileId(index - 1) + 1 + Delta[index];
        }
    }
}
