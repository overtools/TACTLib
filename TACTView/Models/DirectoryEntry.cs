using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TACTLib;
using TACTLib.Container;
using TACTView.Api.Models;

namespace TACTView.Models {
    internal record DirectoryEntry(string Name, long Size, Locale Locale, CKey Key, bool IsDirectory) : IDirectoryEntry {
        private DirectoryEntry(string name, long size, Locale locale, CKey key, bool isDirectory, IDirectoryEntry? parent) : this(name, size, locale, key, isDirectory) {
            Parent = parent;
            Parent?.Children.Add(this);
        }

        public IEnumerable<IDirectoryEntry> SubDirectories => Children.Where(x => x.IsDirectory).OrderBy(x => x.Name);
        public IDirectoryEntry? Parent { get; init; }
        public ObservableCollection<IDirectoryEntry> Children { get; init; } = new();
        public object? CustomData { get; init; }

        public IDirectoryEntry CreateDirectory(string name) {
            var parts = name.Split(new[] {'/', '\\'}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            DirectoryEntry lastDir = this;
            foreach (var part in parts) {
                var existing = lastDir.FindChild(part, StringComparison.InvariantCultureIgnoreCase);
                lastDir = existing != null ? (DirectoryEntry) existing : new DirectoryEntry(part, 0, Locale.None, new CKey(), true, lastDir);
            }

            return lastDir;
        }

        public IDirectoryEntry CreateFile(string name, Locale locale, CKey key, long size, object? customData = null) {
            return new DirectoryEntry(name, size, locale, key, false, this) {CustomData = customData};
        }

        private IDirectoryEntry? FindChild(string name, StringComparison comparisonType) {
            return Children.FirstOrDefault(x => x.Name.Equals(name, comparisonType));
        }
    }
}
