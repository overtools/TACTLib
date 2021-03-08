using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using TACTLib;
using TACTLib.Core.Product.MNDX;
using TACTLib.Core.Product.Tank;
using TACTView.Api;
using TACTView.Api.Models;
using TACTView.Api.Registry;

namespace TACTView.Connectors {
    [PublicAPI]
    public class TankConnector : IProductConnector {
        public TankConnector(ProductHandler_Tank handler, IProgressReporter progress) {
            Handler = handler;
            Progress = progress;
        }

        public IProgressReporter Progress { get; init; }

        private ProductHandler_Tank Handler { get; }

        public void GetEntries(IDirectoryEntry root) {
            var i = 0;
            var cache = new Dictionary<string, IDirectoryEntry>();
            Progress.Report(i, Handler.m_assets.Count, 0, "Loading files...");
            foreach (var guid in Handler.m_assets.Keys) {
                Progress.Report(++i, Handler.m_assets.Count, 0, $"Adding {TankGUID.AsHexString(guid)}");
                var folder = TankGUID.Type(guid).ToString("X3");
                if (!cache.TryGetValue(folder, out var subdirectory)) {
                    subdirectory = root.CreateDirectory(folder);
                    cache[folder] = subdirectory;
                }

                var cmfData = Handler.GetContentManifestForAsset(guid);
                if(cmfData.TryGet(guid, out var hashData)) {
                    subdirectory.CreateFile(TankGUID.AsString(guid), Locale.None, hashData.ContentKey, hashData.Size);   
                } else {
                    Logger.Error("Tank", $"Can't find cmf data for {TankGUID.AsString(guid)}");
                }
            }

            Progress.Report(0, 1, 0, "Idle");
        }

        public Stream? OpenFile(IFileEntry entry) {
            if (entry.CustomData is ulong guid) {
                return Handler.OpenFile(guid);
            }

            return null;
        }
    }
}
