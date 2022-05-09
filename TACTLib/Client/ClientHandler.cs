using System;
using System.Collections.Generic;
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

        public readonly string? ProductCode = null;

        /// <summary>
        /// The installation info of the container
        /// </summary>
        public readonly InstallationInfo InstallationInfo;

        /// <summary>
        /// The installation info of the container
        /// </summary>
        public readonly InstallationInfoFile? InstallationInfoFile;

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
        public readonly ProductInstall? AgentProduct;

        public readonly INetworkHandler? NetHandle;

        /// <summary>The base path of the container. E.g where the game executables are.</summary>
        public readonly string BasePath;

        public readonly string? InstallationInfoPath;

        public readonly ClientCreateArgs CreateArgs;

        public readonly CDNIndexHandler? m_cdnIdx;

        public ClientHandler(string? basePath, ClientCreateArgs createArgs) {
            BasePath = basePath ?? ""; // should it be empty string? lol
            CreateArgs = createArgs;
            ProductCode = createArgs.Product;

            // If we are using a container OR if InstallMode == Local
            if (createArgs.UseContainer) {
                if (!Directory.Exists(BasePath)) {
                    throw new FileNotFoundException($"Invalid archive directory. Directory {BasePath} does not exist. Please specify a valid directory.");
                }

                try {
                    // if someone specified a flavor, try and see what flavor and fix the base path
                    var flavorInfoPath = Path.Combine(BasePath, ".flavor.info");
                    if (File.Exists(flavorInfoPath)) {
                        // mixed installation, store the product code to be used below
                        ProductCode = File.ReadLines(flavorInfoPath).Skip(1).First();
                        Product = ProductHelpers.ProductFromUID(ProductCode);
                        BasePath = Path.GetFullPath(Path.Combine(BasePath, "../")); // base path is a directory up from the flavor

                        Logger.Info("Core", $".flavor.info detected. Found product \"{ProductCode}\"");
                    }
                } catch (Exception ex) {
                    Logger.Warn("Core", $"Failed reading .flavor.info file! {ex.Message}");
                }

                // ensure to see the .build.info file exists. if it doesn't then we can't continue
                InstallationInfoPath = Path.Combine(BasePath, createArgs.InstallInfoFileName) + createArgs.ExtraFileEnding;
                if (!File.Exists(InstallationInfoPath)) {
                    throw new FileNotFoundException($"Invalid archive directory! {InstallationInfoPath} was not found. You must provide the path to a valid install.");
                }

                // If there was no flavor specified, try to find the flavor in the .build.info file
                ProductCode ??= createArgs.Product ?? ProductHelpers.TryGetUIDFromProduct(ProductHelpers.TryGetProductFromLocalInstall(BasePath));
                InstallationInfoFile = new InstallationInfoFile(InstallationInfoPath);

                // If product is unknown it means we loaded on the base path and not a flavor e.g. C:/Games/Overwatch
                // so we need to load the .build.info file and get the product from there
                if (Product == TACTProduct.Unknown) {
                    var installationInfo = InstallationInfoFile.GetInstallationInfoForProduct(ProductCode);

                    if (installationInfo == null) {
                        Logger.Warn("Core", $"Failed to find product \"{ProductCode}\" in {createArgs.InstallInfoFileName} file! Using first available.");
                        installationInfo = InstallationInfoFile.GetFirstOrDefault();
                    }

                    // if there's no data in the .build.info file? Shouldn't really be possible
                    if (installationInfo == null) {
                        throw new Exception($"Failed to find a valid product in {createArgs.InstallInfoFileName} file!");
                    }

                    // If product code is null this ProductFromUID will throw an exception
                    ProductCode = installationInfo.Values.GetValueOrDefault("Product");
                    Product = ProductHelpers.ProductFromUID(ProductCode);
                    Logger.Info("Core", $"Found product \"{ProductCode}\" via {createArgs.InstallInfoFileName}");
                }
            }

            // If there is no product specified it's because we aren't using a container or we're using online mode.
            // Find the product from the productCode provided by createArgs or if there is none, find it from the local install path
            // tho i'm not sure what the chances are of there being an install path provided if you're loading from remote as it would be kind of redundant but whatever
            if (Product == TACTProduct.Unknown) {
                Product = ProductHelpers.TryGetProductFromUID(ProductCode);

                // if no product was specified via ClientCreateArgs, try and find it from the local install path
                if (Product == TACTProduct.Unknown) {
                    Product = ProductHelpers.TryGetProductFromLocalInstall(BasePath);
                }

                if (Product == TACTProduct.Unknown) {
                    if (createArgs.VersionSource == ClientCreateArgs.InstallMode.Remote) {
                        throw new Exception("Failed to determine TACT Product. This is required if you're loading from remote.");
                    }

                    Logger.Warn("Core", "Failed to determine TACT Product! This could potentially cause issues!");
                }
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
                // should always exist as it's fetched above but we can't continue without being able to load the installation info
                if (!File.Exists(InstallationInfoPath)) {
                    throw new FileNotFoundException(InstallationInfoPath);
                }

                using var _ = new PerfCounter("InstallationInfo::ctor`string");
                InstallationInfo = new InstallationInfo(InstallationInfoPath!, ProductCode!);
            } else {
                using var _ = new PerfCounter("InstallationInfo::ctor`INetworkHandler");
                InstallationInfo = new InstallationInfo(NetHandle!, createArgs.OnlineRegion);
            }

            // try to load the agent database and use the selected language if we don't already have one specified
            if (createArgs.UseContainer) {
                AgentProduct = TryGetAgentDatabase();
                if (AgentProduct != null) {
                    if (string.IsNullOrWhiteSpace(createArgs.TextLanguage)) {
                        createArgs.TextLanguage = AgentProduct.Settings.SelectedTextLanguage;
                        CreateArgs.TextLanguage = AgentProduct.Settings.SelectedTextLanguage;
                    }

                    if (string.IsNullOrWhiteSpace(createArgs.SpeechLanguage)) {
                        createArgs.SpeechLanguage = AgentProduct.Settings.SelectedSpeechLanguage;
                        CreateArgs.SpeechLanguage = AgentProduct.Settings.SelectedSpeechLanguage;
                    }
                }
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

            if (createArgs.Online) {
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

            if (CreateArgs.Online) {
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
            if (m_cdnIdx!.CDNIndexData.TryGetValue(key, out var cdnIdx)) {
                netMemStream = m_cdnIdx.OpenDataFile(cdnIdx);
            }

            if (netMemStream == null) {
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

        public string? GetProduct() => ProductCode;

        /// <summary>
        /// Tries to load an agent database for the current product
        /// </summary>
        /// <param name="createArgs"></param>
        public ProductInstall? TryGetAgentDatabase() {
            try {
                var dbPath = Path.Combine(BasePath, CreateArgs.ProductDatabaseFilename);
                if (File.Exists(dbPath)) {
                    using var _ = new PerfCounter("AgentDatabase::ctor`string`bool");
                    return new AgentDatabase(dbPath).Data.ProductInstall.FirstOrDefault(x => x.ProductCode == ProductCode);
                }
            } catch (Exception ex) {
                Logger.Warn("Core", $"Failed loading Agent DB. {ex.Message}. This can be ignored.");
            }

            return null;
        }
    }
}