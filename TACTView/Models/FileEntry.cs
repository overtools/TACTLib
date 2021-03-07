using TACTLib;
using TACTLib.Container;
using TACTView.Api.Models;

namespace TACTView.Models {
    internal record FileEntry(string Name, long Size, Locale Locale, CKey Key) : DirectoryEntry(Name), IFileEntry {
        internal FileEntry(string name, long size, Locale locale, CKey key, IDirectoryEntry? parent) : this(name, size, locale, key) {
            Parent = parent;
            Parent?.Children.Add(this);
        }
    }
}
