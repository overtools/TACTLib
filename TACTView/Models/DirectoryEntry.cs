using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TACTLib;
using TACTLib.Container;
using TACTView.Api.Models;

namespace TACTView.Models {
    internal record DirectoryEntry(string Name) : IDirectoryEntry {
        private DirectoryEntry(string name, IDirectoryEntry? parent) : this(name) {
            Parent = parent;
            Parent?.Children.Add(this);
        }

        public IEnumerable<IDirectoryEntry> SubDirectories => Children.Where(x => x is not FileEntry).OrderBy(x => x.Name);
        public IDirectoryEntry? Parent { get; init; }
        public ObservableCollection<IDirectoryEntry> Children { get; init; } = new();
        public object? CustomData { get; init; }

        public IDirectoryEntry CreateDirectory(string name) {
            var parts = name.Split(new[] {'/', '\\'}, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            DirectoryEntry lastDir = this;
            foreach (var part in parts) {
                var existing = lastDir.FindChild(part, StringComparison.InvariantCultureIgnoreCase);
                lastDir = existing != null ? (DirectoryEntry) existing : new DirectoryEntry(part, lastDir);
            }

            return lastDir;
        }

        public IFileEntry CreateFile(string name, Locale locale, CKey key, long size) {
            var file = new FileEntry(name, size, locale, key, this);
            Children.Add(file);
            return file;
        }

        private IDirectoryEntry? FindChild(string name, StringComparison comparisonType) {
            return Children.FirstOrDefault(x => x.Name.Equals(name, comparisonType));
        }
    }
}
