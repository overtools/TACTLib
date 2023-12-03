namespace TACTLib.Core.VFS {
    public class VFSFile {
        public string? Name;
        public CKey CKey;
        public CKey EKey;
        public int Offset;
        public int SpanSize;
        public int ContentSize;
        public int EncodedSize;
        public string? ESpec;
    }
}
