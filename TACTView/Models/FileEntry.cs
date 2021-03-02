using System.Collections.Generic;
using TACTLib;
using TACTLib.Container;

namespace TACTView.Models {
    public record FileEntry(string Name, long Size, Locale Locale, CKey Key) {
        public FileEntry(string name, long size, Locale locale, CKey key, FileEntry? parent) : this(name, size, locale, key) {
            Parent = parent;
            Parent?.Children.Add(this);
        }
        public FileEntry? Parent { get; }
        public List<FileEntry> Children { get; } = new();
    }
}
