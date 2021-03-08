using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TACTLib.Core.Product.MNDX;
using TACTView.Api;
using TACTView.Api.Models;
using TACTView.Api.Registry;

namespace TACTView.Connectors {
    [PublicAPI]
    public class MNDXConnector : IProductConnector {
        public MNDXConnector(ProductHandler_MNDX handler, IProgressReporter progress) {
            Handler = handler;
            Progress = progress;
        }

        public IProgressReporter Progress { get; init; }

        private ProductHandler_MNDX Handler { get; }

        public void GetEntries(IDirectoryEntry root) {
            var i = 0;
            var cache = new Dictionary<string, IDirectoryEntry>();
            Progress.Report(i, Handler.Entries.Count, 0, "Loading files...");
            foreach (var entry in Handler.Entries) {
                Progress.Report(++i, Handler.Entries.Count, 0, $"Adding {entry.Path}");
                var path = Path.GetDirectoryName(entry.Path) ?? string.Empty;
                if (!cache.TryGetValue(path.ToUpper(), out var subdirectory)) {
                    subdirectory = root.CreateDirectory(path);
                    cache[path.ToUpper()] = subdirectory;
                }

                subdirectory.CreateFile(Path.GetFileName(entry.Path), entry.Locale, entry.Info.Key, entry.Info.FileSize);
            }

            Progress.Report(0, 1, 0, "Idle");
        }

        public Stream? OpenFile(IFileEntry entry) {
            return Handler.OpenFile(entry.Key);
        }
    }
}
