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
using TACTLib.Protocol;
using TACTLib.Protocol.NGDP;
using TACTLib.Protocol.Ribbit;

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

        public readonly INetworkHandler NetHandle;

        /// <summary>The base path of the container. E.g where the game executables are.</summary>
        public readonly string BasePath;

        public readonly ClientCreateArgs CreateArgs;

        public ClientHandler(string basePath, ClientCreateArgs createArgs) {
            basePath = basePath ?? "";
            BasePath = basePath;
            CreateArgs = createArgs;
            
            if (!Directory.Exists(basePath)) throw new FileNotFoundException("invalid archive directory");

            string dbPath = Path.Combine(basePath, createArgs.ProductDatabaseFilename);

            try {
                if (File.Exists(dbPath)) {
                    using (var _ = new PerfCounter("AgentDatabase::ctor`string`bool"))
                        foreach(var install in new AgentDatabase(dbPath).Data.ProductInstall)
                        {
                            if(string.IsNullOrEmpty(createArgs.Flavor) || install.Settings.GameSubfolder.Contains(createArgs.Flavor))
                            {
                                AgentProduct = install;
                                break;
                            }
                        }
                    if (AgentProduct == null) {
                        throw new InvalidDataException();
                    }

                    Product = ProductHelpers.ProductFromUID(AgentProduct.ProductCode);
                } else {
                    throw new InvalidDataException();
                }
            } catch {
                try {
                    Product = ProductHelpers.ProductFromLocalInstall(basePath);
                } catch {
                    if (createArgs.Mode == ClientCreateArgs.InstallMode.CASC) {  // if we need an archive then we should be able to detect the product
                        throw;
                    }
                    Product = createArgs.OnlineProduct;
                }

                AgentProduct = new ProductInstall {
                    ProductCode = createArgs.Product ?? ProductHelpers.UIDFromProduct(Product),
                    Settings = new UserSettings {
                        SelectedTextLanguage = createArgs.TextLanguage ?? "enUS",
                        SelectedSpeechLanguage = createArgs.SpeechLanguage ?? "enUS",
                        PlayRegion = "us"
                    }
                };

                if (AgentProduct.Settings.SelectedSpeechLanguage == AgentProduct.Settings.SelectedTextLanguage) {
                    AgentProduct.Settings.Languages.Add(new LanguageSetting {
                        Language = AgentProduct.Settings.SelectedTextLanguage,
                        Option = LanguageOption.LangoptionTextAndSpeech
                    });
                } else {
                    AgentProduct.Settings.Languages.Add(new LanguageSetting {
                        Language = AgentProduct.Settings.SelectedTextLanguage,
                        Option = LanguageOption.LangoptionText
                    });
                    
                    AgentProduct.Settings.Languages.Add(new LanguageSetting {
                        Language = AgentProduct.Settings.SelectedSpeechLanguage,
                        Option = LanguageOption.LangoptionSpeech
                    });
                }
            }

            if (string.IsNullOrWhiteSpace(createArgs.TextLanguage)) {
                createArgs.TextLanguage = AgentProduct.Settings.SelectedTextLanguage;
            }

            if (string.IsNullOrWhiteSpace(createArgs.SpeechLanguage)) {
                createArgs.SpeechLanguage = AgentProduct.Settings.SelectedSpeechLanguage;
            }
            
            if (createArgs.Online) {
                using (var _ = new PerfCounter("INetworkHandler::ctor`ClientHandler"))
                    if (createArgs.Mode.ToString() == "Ribbit") {
                        NetHandle = new RibbitClient(this);
                    } else {
                        NetHandle = new NGDPClient(this);
                    }
            }
            
            if(createArgs.Mode == ClientCreateArgs.InstallMode.CASC) {
                string installationInfoPath = Path.Combine(basePath, createArgs.InstallInfoFileName) + createArgs.ExtraFileEnding;
                if (!File.Exists(installationInfoPath)) {
                    throw new FileNotFoundException(installationInfoPath);
                }

                using (var _ = new PerfCounter("InstallationInfo::ctor`string"))
                    InstallationInfo = new InstallationInfo(installationInfoPath, AgentProduct.ProductCode);
            } else {
                using (var _ = new PerfCounter("InstallationInfo::ctor`INetworkHandler"))
                    InstallationInfo = new InstallationInfo(NetHandle, createArgs.OnlineRegion);
            }

            Logger.Info("CASC", $"{Product} build {InstallationInfo.Values["Version"]}");

            if (createArgs.Mode == ClientCreateArgs.InstallMode.CASC) {
                Logger.Info("CASC", "Initializing...");
                using (var _ = new PerfCounter("ContainerHandler::ctor`ClientHandler"))
                    ContainerHandler = new ContainerHandler(this);
            }

            using (var _ = new PerfCounter("ConfigHandler::ctor`ClientHandler"))
                ConfigHandler = new ConfigHandler(this);
    
            using (var _ = new PerfCounter("EncodingHandler::ctor`ClientHandler"))
                EncodingHandler = new EncodingHandler(this);

            if (ConfigHandler.BuildConfig.VFSRoot != null) {
                using (var _ = new PerfCounter("VFSFileTree::ctor`ClientHandler"))
                    VFS = new VFSFileTree(this);
            }

            using (var _ = new PerfCounter("ProductHandlerFactory::GetHandler`TACTProduct`ClientHandler`Stream"))
                ProductHandler = ProductHandlerFactory.GetHandler(Product, this, OpenCKey(ConfigHandler.BuildConfig.Root.ContentKey));
            
            Logger.Info("CASC", "Ready");
        }

        /// <summary>
        /// Open a file from Content Key
        /// </summary>
        /// <param name="key">Content Key of the file</param>
        /// <returns>Loaded file</returns>
        public Stream OpenCKey(CKey key) {
            if (EncodingHandler != null && EncodingHandler.TryGetEncodingEntry(key, out EncodingHandler.CKeyEntry entry)) {
                return OpenEKey(entry.EKey);
            }
            if (CreateArgs.Online) {
                using (var stream = NetHandle.OpenData(key)) {
                    if (stream != null) {
                        var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        ms.Position = 0;
                        return new BLTEStream(this, ms);
                    }
                }
            }
            Debugger.Log(0, "ContainerHandler", $"Missing encoding entry for CKey {key.ToHexString()}\n");
            return null;
        }

        /// <summary>
        /// Open a file from Encoding Key
        /// </summary>
        /// <param name="key">The Encoding Key</param>
        /// <returns>Loaded file</returns>
        public Stream OpenEKey(EKey key) {  // ekey = value of ckey in encoding table
            Stream stream = default;
            if (CreateArgs.Mode == ClientCreateArgs.InstallMode.CASC) {
                stream = ContainerHandler?.OpenEKey(key);
            }
            return stream == null ? null : new BLTEStream(this, stream);
        }

        /// <summary>
        /// Open a file from Encoding Key
        /// </summary>
        /// <param name="key">The Long Encoding Key</param>
        /// <returns>Loaded file</returns>
        public Stream OpenEKey(CKey key) {  // ekey = value of ckey in encoding table
            Stream stream = default;
            if (CreateArgs.Mode == ClientCreateArgs.InstallMode.CASC) {
                stream = ContainerHandler?.OpenEKey(key.AsEKey());
            }

            if (stream != null || !CreateArgs.Online) return stream == null ? null : new BLTEStream(this, stream);
            
            stream = NetHandle.OpenData(key);
            if (stream == null) return null;
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            return new BLTEStream(this, ms);
        }

        public Stream OpenConfigKey(string key) {
            if (CreateArgs.Mode == ClientCreateArgs.InstallMode.CASC) {
                var path = Path.Combine(ContainerHandler?.ContainerDirectory, ContainerHandler.ConfigDirectory, key.Substring(0, 2), key.Substring(2, 2), key);
                if (File.Exists(path + CreateArgs.ExtraFileEnding)) {
                    return File.OpenRead(path + CreateArgs.ExtraFileEnding);
                }

                if (File.Exists(path)) {
                    return File.OpenRead(path);
                }
            }

            return CreateArgs.Online ? NetHandle.OpenConfig(key) : null;
        }
    }
}
