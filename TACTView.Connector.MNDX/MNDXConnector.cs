using System.Collections.Generic;
using System.IO;
using TACTLib.Core.Product.MNDX;
using TACTView.Api.Models;
using TACTView.Api.Registry;

namespace TACTView.Connectors {
    public class MNDXConnector : IProductConnector {
        public MNDXConnector(ProductHandler_MNDX handler) {
            Handler = handler;
        }

        private ProductHandler_MNDX Handler { get; }

        public void GetEntries(IDirectoryEntry root) {
            foreach (var entry in Handler.Entries) {
                var subdirectory = root.CreateDirectory(Path.GetDirectoryName(entry.Path) ?? string.Empty);
                subdirectory.CreateFile(Path.GetFileName(entry.Path), entry.Locale, entry.Info.Key, entry.Info.FileSize);
            }
        }
    }
}
