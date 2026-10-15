namespace TACTLib.Core.VFS {
    public record struct VFSFile {
        public string? Name;
        public int Offset;
        public CKey EKey;
        public string? ESpec;
        public int ESize;
        public CKey? CKey;
        public int CSize;
    }
}
