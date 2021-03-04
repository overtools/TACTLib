using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TACTView.Api.Models;

namespace TACTView.Models {
    public record DirectoryEntry(string Name) : IDirectoryEntry {
        public DirectoryEntry(string name, IDirectoryEntry? parent) : this(name) {
            Parent = parent;
            Parent?.Children.Add(this);
        }
        public IDirectoryEntry? Parent { get; init; }
        public ObservableCollection<IDirectoryEntry> Children { get; init; } = new();
        public object? CustomData { get; init; }
        public IEnumerable<IDirectoryEntry> SubDirectories => Children.Where(x => x is not FileEntry);

        public IDirectoryEntry CreateDirectory(string name) {
            var parts = name.Split(new [] { '/', '\\' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            DirectoryEntry lastDir = this;
            foreach (var part in parts) {
                var existing = lastDir.FindChild(part, StringComparison.InvariantCultureIgnoreCase);
                lastDir = existing != null ? (DirectoryEntry) existing : new DirectoryEntry(part, lastDir);
            }
            return lastDir;
        }

        private IDirectoryEntry? FindChild(string name, StringComparison comparisonType) {
            return Children.FirstOrDefault(x => x.Name.Equals(name, comparisonType));
        }
    }
}
