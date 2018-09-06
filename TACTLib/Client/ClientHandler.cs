using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TACTLib.Agent;
using TACTLib.Agent.Protobuf;
using TACTLib.Config;
using TACTLib.Container;
using TACTLib.Core;
using TACTLib.Core.Product;
using TACTLib.Helpers;
// ReSharper disable NotAccessedField.Global

namespace TACTLib.Client {
    public class ClientHandler {
        /// <summary>
        /// The <see cref="Product"/> that this container belongs to.
        /// </summary>
        public readonly TACTProduct Product;

        /// <summary>
        /// The installation info of the container
        /// </summary>
        /// <seealso cref="ClientCreateArgs.InstallInfoFileName"/>
        public readonly InstallationInfo InstallationInfo;
        
        /// <summary>Container handler</summary>
        public readonly ContainerHandler ContainerHandler;
        
        /// <summary>Encoding table handler</summary>
        public readonly EncodingHandler EncodingHandler;
        
        /// <summary>Configuration handler</summary>
        public readonly ConfigHandler ConfigHandler;

        /// <summary>Virtual File System</summary>
        public readonly VFSFileTree VFS;

        /// <summary>Product specific Root File handler</summary>
        public readonly IProductHandler ProductHandler;

        /// <summary>BNA Agent DB</summary>
        /// <seealso cref="ClientCreateArgs.ProductDatabaseFilename"/>
        public readonly ProductInstall AgentProduct;

        /// <summary>The base path of the container. E.g where the game executables are.</summary>
        public readonly string BasePath;

        public readonly ClientCreateArgs CreateArgs;

        public ClientHandler(string basePath, ClientCreateArgs createArgs) {
            BasePath = basePath;
            CreateArgs = createArgs;

            string dbPath = Path.Combine(basePath, createArgs.ProductDatabaseFilename);

            try {
                if (File.Exists(dbPath)) {
                    using (var _ = new PerfCounter("AgentDatabase::ctor"))
                        AgentProduct = new AgentDatabase(dbPath, true).Data.ProductInstalls[0];
                    if (AgentProduct == null) {
                        throw new InvalidDataException();
                    }

                    Product = ProductHelpers.ProductFromUID(AgentProduct.ProductCode);
                } else {
                    throw new InvalidDataException();
                }
            } catch {
                Product = ProductHelpers.ProductFromLocalInstall(basePath);
                AgentProduct = new ProductInstall {
                    ProductCode = ProductHelpers.UIDFromProduct(Product),
                    Settings = new UserSettings {
                        SelectedTextLanguage = createArgs.TextLanguage ?? "enUS",
                        SelectedSpeechLanguage = createArgs.SpeechLanguage ?? "enUS",
                        PlayRegion = "us",
                        Languages = new List<LanguageSetting>()
                    }
                };

                if (AgentProduct.Settings.SelectedSpeechLanguage == AgentProduct.Settings.SelectedTextLanguage) {
                    AgentProduct.Settings.Languages.Add(new LanguageSetting {
                        Language = AgentProduct.Settings.SelectedTextLanguage,
                        Option = LanguageOption.TextAndSpeech
                    });
                } else {
                    AgentProduct.Settings.Languages.Add(new LanguageSetting {
                        Language = AgentProduct.Settings.SelectedTextLanguage,
                        Option = LanguageOption.Text
                    });
                    
                    AgentProduct.Settings.Languages.Add(new LanguageSetting {
                        Language = AgentProduct.Settings.SelectedSpeechLanguage,
                        Option = LanguageOption.Speech
                    });
                }
            }

            if (string.IsNullOrWhiteSpace(createArgs.TextLanguage)) {
                createArgs.TextLanguage = AgentProduct.Settings.SelectedTextLanguage;
            }

            if (string.IsNullOrWhiteSpace(createArgs.SpeechLanguage)) {
                createArgs.SpeechLanguage = AgentProduct.Settings.SelectedSpeechLanguage;
            }
            
            string installationInfoPath = Path.Combine(basePath, createArgs.InstallInfoFileName) + createArgs.ExtraFileEnding;
            if (!File.Exists(installationInfoPath)) {
                throw new FileNotFoundException(installationInfoPath);
            }
            using (var _ = new PerfCounter("InstallationInfo::ctor"))
                InstallationInfo = new InstallationInfo(installationInfoPath);
            
            Logger.Info("CASC", $"{Product} build {InstallationInfo.Values["Version"]}");
            
            Logger.Info("CASC", "Initializing...");
            using (var _ = new PerfCounter("ContainerHandler::ctor"))
                ContainerHandler = new ContainerHandler(this);
            using (var _ = new PerfCounter("ConfigHandler::ctor"))
                ConfigHandler = new ConfigHandler(this);
    
            using (var _ = new PerfCounter("EncodingHandler::ctor"))
                EncodingHandler = new EncodingHandler(this);

            if (ConfigHandler.BuildConfig.VFSRoot != null) {
                using (var _ = new PerfCounter("VFSFileTree::ctor"))
                    VFS = new VFSFileTree(this);
            }

            using (var _ = new PerfCounter("ProductHandlerFactory::GetHandler"))
                ProductHandler = ProductHandlerFactory.GetHandler(Product, this, OpenCKey(ConfigHandler.BuildConfig.Root.ContentKey));
            
            Logger.Info("CASC", "Ready");
        }

        /// <summary>
        /// Open a file from Content Key
        /// </summary>
        /// <param name="key">Content Key of the file</param>
        /// <returns>Loaded file</returns>
        public Stream OpenCKey(CKey key) {
            if (EncodingHandler.TryGetEncodingEntry(key, out EncodingHandler.CKeyEntry entry)) {
                return OpenEKey(entry.EKey);
            }
            Debugger.Log(0, "ContainerHandler", $"Missing encoding entry for CKey {key.ToHexString()}");
            return null;
        }

        /// <summary>
        /// Open a file from Encoding Key
        /// </summary>
        /// <param name="key">The Encoding Key</param>
        /// <returns>Loaded file</returns>
        public Stream OpenEKey(EKey key) {  // ekey = value of ckey in encoding table
            var stream = ContainerHandler.OpenEKey(key);
            return stream == null ? null : new BLTEStream(this, stream);
        }
    }
}