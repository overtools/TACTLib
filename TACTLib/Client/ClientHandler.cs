using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public readonly TACTProduct Product = TACTProduct.Unknown;

        /// <summary>
        /// The installation info of the container
        /// </summary>
        /// <seealso cref="ClientCreateArgs.InstallInfoFileName"/>
        public readonly InstallationInfo InstallationInfo;

        /// <summary>Container handler</summary>
        public readonly ContainerHandler? ContainerHandler;

        /// <summary>Encoding table handler</summary>
        public readonly EncodingHandler? EncodingHandler;

        /// <summary>Configuration handler</summary>
        public readonly ConfigHandler ConfigHandler;

        /// <summary>Virtual File System</summary>
        public readonly VFSFileTree? VFS;

        /// <summary>Product specific Root File handler</summary>
        public readonly IProductHandler? ProductHandler;

        /// <summary>BNA Agent DB</summary>
        /// <seealso cref="ClientCreateArgs.ProductDatabaseFilename"/>
        public readonly ProductInstall AgentProduct;

        public readonly INetworkHandler? NetHandle;

        /// <summary>The base path of the container. E.g where the game executables are.</summary>
        public readonly string BasePath;

        public readonly ClientCreateArgs CreateArgs;

        public readonly CDNIndexHandler? m_cdnIdx;

        public ClientHandler(string? basePath, ClientCreateArgs createArgs) {
            basePath ??= "";
            BasePath = basePath;
            CreateArgs = createArgs;
            string? installationInfoPath = null;
            string? flavorInfoProductCode = null;

            if (createArgs.UseContainer) {
                if (!Directory.Exists(basePath)) {
                    throw new FileNotFoundException($"Invalid archive directory. Directory {basePath} does not exist. Please specify a valid directory.");
                }

                try {
                    // if someone specified a flavor, try and see what flavor and fix the base path
                    var flavorInfoPath = Path.Combine(basePath, ".flavor.info");
                    if (File.Exists(flavorInfoPath)) {
                        // mixed installation, store the product code to be used below
                        flavorInfoProductCode = File.ReadLines(flavorInfoPath).Skip(1).First();
                        Product = ProductHelpers.ProductFromUID(flavorInfoProductCode);
                        BasePath = basePath = Path.GetFullPath(Path.Combine(basePath, "../")); // base path is a directory up from the flavor

                        Logger.Info("Core", $".flavor.info detected. Found product \"{flavorInfoProductCode}\"");
                    }
                } catch (Exception ex) {
                    Logger.Warn("Core", $"Failed reading .flavor.info file? {ex.Message}");
                }

                // ensure to see the .build.info file exists. if it doesn't then we can't continue
                installationInfoPath = Path.Combine(basePath, createArgs.InstallInfoFileName) + createArgs.ExtraFileEnding;
                if (!File.Exists(installationInfoPath)) {
                    throw new FileNotFoundException($"Invalid Overwatch archive? {installationInfoPath} was not found. You must provide the path to a valid Overwatch install.");
                }
            }

            var dbPath = Path.Combine(basePath, createArgs.ProductDatabaseFilename);

            try {
                // try and load product and agent data from the product database
                if (File.Exists(dbPath)) {
                    using (var _ = new PerfCounter("AgentDatabase::ctor`string`bool"))
                        foreach(var install in new AgentDatabase(dbPath).Data.ProductInstall) {
                            if (!string.IsNullOrEmpty(createArgs.Flavor) && !install.Settings.GameSubfolder.Contains(createArgs.Flavor) && (createArgs.Flavor != "retail" || !string.IsNullOrEmpty(install.Settings.GameSubfolder))) continue;
                            AgentProduct = install;
                            break;
                        }

                    if (AgentProduct == null) {
                        throw new InvalidDataException();
                    }

                    Product = ProductHelpers.ProductFromUID(AgentProduct.ProductCode);
                } else {
                    throw new InvalidDataException();
                }
            } catch {
                // if product db reading failed and we don't already have a product (from the .flavor.info at the top), try to lookup from the current directory
                if (Product == TACTProduct.Unknown) {
                    try {
                        Product = ProductHelpers.ProductFromLocalInstall(basePath);
                    } catch {
                        if (createArgs.VersionSource == ClientCreateArgs.InstallMode.Local) {  // if we need an archive then we should be able to detect the product
                            throw;
                        }

                        Product = createArgs.OnlineProduct;
                    }
                }

                AgentProduct = new ProductInstall {
                    ProductCode = flavorInfoProductCode ?? createArgs.Product ?? ProductHelpers.UIDFromProduct(Product),
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
                using var _ = new PerfCounter("INetworkHandler::ctor`ClientHandler");
                if (createArgs.OnlineRootHost.StartsWith("ribbit:")) {
                    NetHandle = new RibbitCDNClient(this);
                } else {
                    NetHandle = new NGDPClient(this);
                }
            }

            if (createArgs.VersionSource == ClientCreateArgs.InstallMode.Local) {
                if (!File.Exists(installationInfoPath)) {
                    throw new FileNotFoundException(installationInfoPath);
                }

                using var _ = new PerfCounter("InstallationInfo::ctor`string");
                InstallationInfo = new InstallationInfo(installationInfoPath!, AgentProduct.ProductCode);
            } else {
                using var _ = new PerfCounter("InstallationInfo::ctor`INetworkHandler");
                InstallationInfo = new InstallationInfo(NetHandle!, createArgs.OnlineRegion);
            }

            Logger.Info("CASC", $"{Product} build {InstallationInfo.Values["Version"]}");

            if (createArgs.UseContainer) {
                Logger.Info("CASC", "Initializing...");
                using var _ = new PerfCounter("ContainerHandler::ctor`ClientHandler");
                ContainerHandler = new ContainerHandler(this);
            }

            using (var _ = new PerfCounter("ConfigHandler::ctor`ClientHandler"))
                ConfigHandler = new ConfigHandler(this);

            using (var _ = new PerfCounter("EncodingHandler::ctor`ClientHandler"))
                EncodingHandler = new EncodingHandler(this);

            if (ConfigHandler.BuildConfig.VFSRoot != null) {
                using var _ = new PerfCounter("VFSFileTree::ctor`ClientHandler");
                VFS = new VFSFileTree(this);
            }

            if (createArgs.Online)
            {
                m_cdnIdx = CDNIndexHandler.Initialize(this);
            }

            using (var _ = new PerfCounter("ProductHandlerFactory::GetHandler`TACTProduct`ClientHandler`Stream"))
                ProductHandler = ProductHandlerFactory.GetHandler(Product, this, OpenCKey(ConfigHandler.BuildConfig.Root.ContentKey)!);

            Logger.Info("CASC", "Ready");
        }

        /// <summary>
        /// Open a file from Content Key
        /// </summary>
        /// <param name="key">Content Key of the file</param>
        /// <returns>Loaded file</returns>
        public Stream? OpenCKey(CKey key) {
            // todo: EncodingHandler can't be null after constructor has finished, but can be during init
            if (EncodingHandler != null && EncodingHandler.TryGetEncodingEntry(key, out var entry)) {
                return OpenEKey(entry.EKey);
            }
            if (CreateArgs.Online)
            {
                return new BLTEStream(this, NetHandle!.OpenData(key));
            }
            Debugger.Log(0, "ContainerHandler", $"Missing encoding entry for CKey {key.ToHexString()}\n");
            return null;
        }

        /// <summary>
        /// Open a file from Encoding Key
        /// </summary>
        /// <param name="key">The Encoding Key</param>
        /// <returns>Loaded file</returns>
        public Stream? OpenEKey(EKey key) {  // ekey = value of ckey in encoding table
            var stream = ContainerHandler?.OpenEKey(key);
            return stream == null ? null : new BLTEStream(this, stream);
        }

        /// <summary>
        /// Open a file from Encoding Key
        /// </summary>
        /// <param name="key">The Long Encoding Key</param>
        /// <returns>Loaded file</returns>
        public Stream? OpenEKey(CKey key) {  // ekey = value of ckey in encoding table
            if (ContainerHandler != null) {
                try {
                    var cascBlte = OpenEKey(key.AsEKey());
                    if (cascBlte != null) return cascBlte;
                } catch (Exception e) {
                    if (!CreateArgs.Online) throw;
                    Logger.Warn("CASC", $"Unable to open {key.ToHexString()} from CASC. Will try to download. Exception: {e}");
                }
            }
            if (!CreateArgs.Online) return null;

            Stream? netMemStream = null;
            if (m_cdnIdx!.CDNIndexData.TryGetValue(key, out var cdnIdx))
            {
                netMemStream = m_cdnIdx.OpenDataFile(cdnIdx);
            }
            if (netMemStream == null)
            {
                netMemStream = NetHandle!.OpenData(key);
            }

            if (netMemStream == null) return null;
            return new BLTEStream(this, netMemStream);
        }

        public Stream? OpenConfigKey(string key) {
            if (ContainerHandler != null) {
                var path = Path.Combine(ContainerHandler.ContainerDirectory, ContainerHandler.ConfigDirectory, key.Substring(0, 2), key.Substring(2, 2), key);
                if (File.Exists(path + CreateArgs.ExtraFileEnding)) {
                    return File.OpenRead(path + CreateArgs.ExtraFileEnding);
                }

                if (File.Exists(path)) {
                    return File.OpenRead(path);
                }
            }

            return CreateArgs.Online ? NetHandle!.OpenConfig(key) : null;
        }

        public string GetProduct() => CreateArgs.Product ?? ProductHelpers.UIDFromProduct(Product);
    }
}
