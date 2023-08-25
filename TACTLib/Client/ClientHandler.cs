using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using TACTLib.Agent;
using TACTLib.Agent.Protobuf;
using TACTLib.Config;
using TACTLib.Container;
using TACTLib.Core;
using TACTLib.Core.Product;
using TACTLib.Core.VFS;
using TACTLib.Exceptions;
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
        public TACTProduct Product => ProductHelpers.TryGetProductFromUID(ProductCode);

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
        public readonly IContainerHandler? ContainerHandler;

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

        public readonly ClientCreateArgs CreateArgs;

        public readonly CDNIndexHandler? m_cdnIdx;

        public ClientHandler(string? basePath, ClientCreateArgs createArgs) {
            CreateArgs = createArgs;

            if (basePath == "?autodetect") {
                using var _ = new PerfCounter("AgentDatabase::ctor`string`bool");
                try {
                    basePath = new AgentDatabase().Data.ProductInstall.FirstOrDefault(x => x.ProductCode == CreateArgs.Product)?.Settings?.InstallPath;
                } catch {
                    basePath = "";
                }
            }

            BasePath = basePath ?? ""; // should it be empty string? lol
            ProductCode = CreateArgs.Product;

            if (CreateArgs.UseContainer) {
                if (!Directory.Exists(BasePath)) {
                    throw new FileNotFoundException($"Invalid archive directory. Directory {BasePath} does not exist. Please specify a valid directory.");
                }

                // if someone specified a flavor, try and see what flavor and fix the base path
                var flavorInfoPath = Path.Combine(BasePath, ".flavor.info");
                if (File.Exists(flavorInfoPath)) {
                    try {
                        // mixed installation, store the product code to be used below
                        ProductCode = File.ReadLines(flavorInfoPath).Skip(1).First();
                        BasePath = Path.GetFullPath(Path.Combine(BasePath, "../")); // base path is a directory up from the flavor
                        Logger.Info("Core", $".flavor.info detected. Found product \"{ProductCode}\"");
                    } catch (Exception ex) {
                        Logger.Warn("Core", $"Failed reading .flavor.info file! {ex.Message}");
                    }
                }

                ProductCode ??= ProductHelpers.TryGetUIDFromProduct(ProductHelpers.TryGetProductFromLocalInstall(BasePath));
            }

            if (Product == TACTProduct.Unknown) {
                throw new Exception($"Failed to determine TACT Product at `{BasePath}`");
            }

            var staticBuildConfigPath = Path.Combine(BasePath, "data", ".build.config"); // todo: um
            var isStaticContainer = File.Exists(staticBuildConfigPath);
            if (isStaticContainer) {
                if (CreateArgs.VersionSource != ClientCreateArgs.InstallMode.Local) throw new Exception("only local version sources are supported for static containers (steam)");
                CreateArgs.Online = false;

                using var buildConfigStream = File.OpenRead(staticBuildConfigPath);
                var buildConfig = new Config.BuildConfig(buildConfigStream);
                ConfigHandler = ConfigHandler.ForStaticContainer(this, buildConfig);
            } else if (CreateArgs.VersionSource == ClientCreateArgs.InstallMode.Local) {
                // ensure to see the .build.info file exists. if it doesn't then we can't continue
                var installationInfoPath = Path.Combine(BasePath, CreateArgs.InstallInfoFileName) + CreateArgs.ExtraFileEnding;
                if (!File.Exists(installationInfoPath)) {
                    throw new FileNotFoundException($"Invalid archive directory! {installationInfoPath} was not found. You must provide the path to a valid install.");
                }

                InstallationInfoFile = new InstallationInfoFile(installationInfoPath);
            }

            if (CreateArgs.Online) {
                using var _ = new PerfCounter("INetworkHandler::ctor`ClientHandler");
                if (CreateArgs.OnlineRootHost.StartsWith("ribbit:")) {
                    NetHandle = new RibbitCDNClient(this);
                } else {
                    NetHandle = new NGDPClient(this);
                }
            }

            if (isStaticContainer) {
                InstallationInfo = new InstallationInfo(new Dictionary<string, string> {
                    {"Version", ConfigHandler!.BuildConfig.Values["build-name"][0]}
                });
            } else if (CreateArgs.VersionSource == ClientCreateArgs.InstallMode.Local) {
                InstallationInfo = new InstallationInfo(InstallationInfoFile!.Values, ProductCode!);
            } else {
                using var _ = new PerfCounter("InstallationInfo::ctor`INetworkHandler");
                InstallationInfo = new InstallationInfo(NetHandle!, CreateArgs.OnlineRegion);
            }

            if (CreateArgs.OverrideBuildConfig != null) {
                InstallationInfo.Values["BuildKey"] = CreateArgs.OverrideBuildConfig;
            }
            if (CreateArgs.OverrideVersionName != null) {
                InstallationInfo.Values["Version"] = CreateArgs.OverrideVersionName;
            }

            // try to load the agent database and use the selected language if we don't already have one specified
            if (CreateArgs.UseContainer) {
                AgentProduct = TryGetAgentDatabase();
                if (AgentProduct != null) {
                    if (string.IsNullOrWhiteSpace(CreateArgs.TextLanguage)) {
                        CreateArgs.TextLanguage = AgentProduct.Settings.SelectedTextLanguage;
                    }

                    if (string.IsNullOrWhiteSpace(CreateArgs.SpeechLanguage)) {
                        CreateArgs.SpeechLanguage = AgentProduct.Settings.SelectedSpeechLanguage;
                    }
                }
            }

            if (string.IsNullOrEmpty(CreateArgs.TextLanguage) && string.IsNullOrEmpty(CreateArgs.SpeechLanguage)) {
                Logger.Error("Core", "Failed to detect language! Defaulting to enUS");
                CreateArgs.TextLanguage = "enUS";
                CreateArgs.SpeechLanguage = "enUS";
            } else if (string.IsNullOrEmpty(CreateArgs.TextLanguage)) {
                Logger.Error("Core", "Failed to detect text language! Defaulting to enUS");
                CreateArgs.TextLanguage = "enUS";
            } else if (string.IsNullOrEmpty(CreateArgs.SpeechLanguage)) {
                Logger.Error("Core", "Failed to detect speech language! Defaulting to enUS");
                CreateArgs.SpeechLanguage = "enUS";
            }

            Logger.Info("CASC", $"{Product} build {InstallationInfo.Values["Version"]}");

            if (CreateArgs.UseContainer) {
                Logger.Info("CASC", "Initializing...");
                if (isStaticContainer) {
                    ContainerHandler = new StaticContainerHandler(this);
                } else {
                    using var _ = new PerfCounter("ContainerHandler::ctor`ClientHandler");
                    ContainerHandler = new ContainerHandler(this);
                }
            }

            if (ConfigHandler == null) {
                // static container does early init
                using (var _ = new PerfCounter("ConfigHandler::ctor`ClientHandler"))
                    ConfigHandler = ConfigHandler.ForDynamicContainer(this);
            }

            using (var _ = new PerfCounter("EncodingHandler::ctor`ClientHandler"))
                EncodingHandler = new EncodingHandler(this);

            if (ConfigHandler.BuildConfig.VFSRoot != null) {
                using var _ = new PerfCounter("VFSFileTree::ctor`ClientHandler");
                using var vfsStream = OpenCKey(ConfigHandler.BuildConfig.VFSRoot!.ContentKey)!;
                VFS = new VFSFileTree(this, vfsStream);
            }

            if (CreateArgs.Online) {
                if (CanShareCDNData(CreateArgs.TryShareCDNIndexWithHandler)) {
                    m_cdnIdx = CreateArgs.TryShareCDNIndexWithHandler.m_cdnIdx;
                } else {
                    m_cdnIdx = CDNIndexHandler.Initialize(this);
                }
            }

            ProductHandler = CreateProductHandler();

            Logger.Info("CASC", "Ready");
        }

        public IProductHandler? CreateProductHandler() {
            using var _ = new PerfCounter("ProductHandlerFactory::GetHandler`TACTProduct`ClientHandler`Stream");
            return ProductHandlerFactory.GetHandler(Product, this, OpenCKey(ConfigHandler.BuildConfig.Root.ContentKey)!);
        }

        private bool CanShareCDNData([NotNullWhen(true)] ClientHandler? other) {
            if (other?.m_cdnIdx == null) return false;

            var cdnConfig = ConfigHandler.CDNConfig;
            var otherCDNConfig = other.ConfigHandler.CDNConfig;

            var archivesMatch = otherCDNConfig.Archives.Count == cdnConfig.Archives.Count;

            if (archivesMatch) {
                // count is same, compare all archive hashes
                for (int i = 0; i < cdnConfig.Archives.Count; i++) {
                    archivesMatch &= otherCDNConfig.Archives[i] == cdnConfig.Archives[i];
                    if (!archivesMatch) break;
                }
            }

            if (!archivesMatch) {
                Logger.Warn("CDN", "Builds are using different CDN archives. Unable to share data");
            }
            return archivesMatch;
        }
        
        public Stream? OpenEKey(FullEKey fullEKey, int eSize) {  // ekey = value of ckey in encoding table
            var fromContainer = TryOpenEKeyFromContainer(fullEKey, eSize);
            if (fromContainer != null) return fromContainer;

            return TryOpenEKeyFromRemote(fullEKey, eSize);
        }
        
        public Stream? OpenCKey(CKey key) {
            // todo: EncodingHandler can't be null after constructor has finished, but can be during init
            if (EncodingHandler != null && EncodingHandler.TryGetEncodingEntry(key, out var eKeys) && eKeys.Length > 0) {
                var fromContainer = TryOpenEKeyListFromContainer(eKeys);
                if (fromContainer != null) return fromContainer;
                   
                // oopsie. it aint resident in local apparently. lets just try first from other sources
                var first = eKeys[0];
                Logger.Debug("ClientHandler", $"fallback for {key.ToHexString()}: {first.ToHexString()}");
                return TryOpenEKeyFromRemote(first, EncodingHandler.GetEncodedSize(first));
            }

            if (CreateArgs.Online) {
                return new MemoryStream(BLTEDecoder.Decode(this, NetHandle!.OpenData(key)));
            }

            Debugger.Log(0, "ContainerHandler", $"Missing encoding entry for CKey {key.ToHexString()}\n");
            return null;
        }
        
        private Stream? TryOpenEKeyListFromContainer(ReadOnlySpan<FullEKey> eKeys) {
            if (ContainerHandler == null) return null;
            if (EncodingHandler == null) return null; // cant get here but okay

            foreach (var ekey in eKeys) {
                if (!ContainerHandler.CheckResidency(ekey)) {
                    //Logger.Debug("ClientHandler", $"skipping ekey {ekey.ToHexString()} as it is not resident locally");
                    continue;
                }

                var fromContainer = TryOpenEKeyFromContainer(ekey, EncodingHandler.GetEncodedSize(ekey));
                if (fromContainer != null) return fromContainer;
            }
            return null;
        }

        private Stream? TryOpenEKeyFromContainer(FullEKey fullEKey, int eSize) {
            if (ContainerHandler == null) return null;
            try {
                var cascBlte = OpenEKeyFromContainer(fullEKey, eSize);
                if (cascBlte != null) return cascBlte;
            } catch (Exception e) {
                if (!CreateArgs.Online) throw;
                if (e is BLTEKeyException) throw;
                Logger.Warn("CASC", $"Unable to open {fullEKey.ToHexString()} from CASC. Will try to download. Exception: {e}");
            }
            return null;
        }
        
        private Stream? OpenEKeyFromContainer(FullEKey fullEKey, int eSize) {  // ekey = value of ckey in encoding table
            if (ContainerHandler == null) return null;
            var stream = ContainerHandler.OpenEKey(fullEKey, eSize);
            return stream == null ? null : new MemoryStream(BLTEDecoder.Decode(this, stream.Value.AsSpan()));
        }

        private Stream? TryOpenEKeyFromRemote(FullEKey fullEKey, int eSize) {
            if (!CreateArgs.Online) return null;

            byte[]? netMemStream = null;
            if (m_cdnIdx != null && m_cdnIdx.TryGetIndexEntry(fullEKey, out var cdnIdx)) {
                netMemStream = m_cdnIdx.OpenDataFile(cdnIdx);
            }

            if (netMemStream == null) {
                netMemStream = NetHandle!.OpenData(fullEKey);
            }

            if (netMemStream == null) return null;
            return new MemoryStream(BLTEDecoder.Decode(this, netMemStream.AsSpan()));
        }

        public Stream? OpenConfigKey(string key) {
            if (ContainerHandler is not ContainerHandler dynamicContainer) {
                throw new Exception("this method is only supported for dynamic containers");
            }
            var path = Path.Combine(dynamicContainer.ContainerDirectory, Container.ContainerHandler.ConfigDirectory, key.Substring(0, 2), key.Substring(2, 2), key);
            if (File.Exists(path + CreateArgs.ExtraFileEnding)) {
                return File.OpenRead(path + CreateArgs.ExtraFileEnding);
            }

            if (File.Exists(path)) {
                return File.OpenRead(path);
            }

            return CreateArgs.Online ? NetHandle!.OpenConfig(key) : null;
        }

        public string? GetProduct() => ProductCode;

        /// <summary>
        /// Tries to load an agent database for the current product
        /// </summary>
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