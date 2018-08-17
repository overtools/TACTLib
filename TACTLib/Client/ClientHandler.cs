using System.Diagnostics;
using System.IO;
using TACTLib.Config;
using TACTLib.Container;
using TACTLib.Core;
using TACTLib.Helpers;

namespace TACTLib.Client {
    public class ClientHandler {
        public readonly InstallationInfo InstallationInfo;
        public readonly Product Product;
        public readonly ContainerHandler ContainerHandler;
        public readonly EncodingHandler EncodingHandler;
        public readonly ConfigHandler ConfigHandler;

        public readonly string BasePath;

        public const string InstallationInfoFile = ".build.info";

        public ClientHandler(string basePath) {
            BasePath = basePath;
            
            Product = ProductHelpers.ProductFromLocalInstall(basePath);
            
            string installationInfoPath = Path.Combine(basePath, InstallationInfoFile);
            if (!File.Exists(installationInfoPath)) {
                throw new FileNotFoundException(installationInfoPath);
            }
            InstallationInfo = new InstallationInfo(installationInfoPath);
            
            ContainerHandler = new ContainerHandler(this);
            ConfigHandler = new ConfigHandler(this);
    
            using (var _ = new PerfCounter("EncodingHandler::ctor")) {
                EncodingHandler = new EncodingHandler(this);
            }
            
            using (Stream rootStream = OpenCKey(ConfigHandler.BuildConfig.Root.ContentKey)) {
                
            }
        }

        public Stream OpenCKey(CKey key) {
            if (EncodingHandler.TryGetEncodingEntry(key, out EncodingHandler.CKeyEntry entry)) {
                return OpenEKey(entry.EKey);
            }
            Debugger.Log(0, "ContainerHandler", $"Missing encoding entry for CKey {key.ToHexString()}");
            return null;
        }

        public Stream OpenEKey(EKey key) {  // ekey = value of ckey in encoding table
            var stream = ContainerHandler.OpenEKey(key);
            return stream == null ? null : new BLTEStream(this, stream);
        }
    }
}