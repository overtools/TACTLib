using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TACTLib.Exceptions;

namespace TACTLib.Core.Product.Tank {
    public static class ManifestCryptoHandler {
        #region Helpers
        // ReSharper disable once InconsistentNaming
        public const uint SHA1_DIGESTSIZE = 20;

        public static uint Constrain(long value) {
            return (uint)(value % uint.MaxValue);
        }

        public static int SignedMod(long p1, long p2)
        {
            var a = (int)p1;
            var b = (int)p2;
            return (a % b) < 0 ? (a % b + b) : (a % b);
        }
        #endregion

        public static bool AttemptFallbackManifests = false;
        private static readonly Dictionary<TACTProduct, Dictionary<Type, Dictionary<uint, object>>> Providers = new Dictionary<TACTProduct, Dictionary<Type, Dictionary<uint, object>>>();
        private static readonly Dictionary<Type, Type> s_headerTypeToProviderType = new Dictionary<Type, Type>();
        private static bool _baseProvidersFound;

        public static void GenerateKeyIV<T>(string name, string manifestType, T header, uint buildVersion, TACTProduct product, out byte[] key, out byte[] iv) {
            if (!_baseProvidersFound) {
                FindProviders();
                _baseProvidersFound = true;
            }

            if (!s_headerTypeToProviderType.TryGetValue(typeof(T), out var providerType)) {
                throw new InvalidDataException($"[Manifest]: Unable to get crypto provider for {typeof(T)}");
            }
            if (!Providers.TryGetValue(product, out var cryptoTypeMap)) {
                throw new InvalidDataException($"[Manifest]: {product} does not have any crypto providers?");
            }
            if (!cryptoTypeMap.TryGetValue(providerType, out var providerVersions)) {
                throw new InvalidDataException($"[Manifest]: {product} does not have any {providerType} providers?");
            }

            byte[] digest = CreateDigest(name);

            if (providerVersions.TryGetValue(buildVersion, out var providerRaw)) {
                Logger.Info("Manifest", $"Using {manifestType} procedure {buildVersion} for {name}");
            } else {
                if (!AttemptFallbackManifests) {
                    throw new UnsupportedBuildVersionException($"Build version {buildVersion} is not supported");
                }

                Logger.Warn("Manifest", $"No {manifestType} procedure for build {buildVersion}, trying closest version");
                try {
                    var pair = providerVersions.Where(it => it.Key < buildVersion).OrderByDescending(it => it.Key).First();
                    Logger.Info("Manifest", $"Using {manifestType} procedure {pair.Key}");
                    providerRaw = pair.Value;
                } catch {
                    throw new CryptographicException($"Missing {manifestType} generators");
                }
            }

            var provider = (IManifestCrypto<T>) providerRaw;
            key = provider.Key(header, 32);
            try {
                iv = provider.IV(header, digest, 16);
            } catch (Exception ex) {
                iv = new byte[16];
                Logger.Error("Manifest", $"Error generating IV but we dont care i guess: {ex}");
            }

            name = Path.GetFileNameWithoutExtension(name);
            Logger.Debug(manifestType, $"{name} key:{string.Join(" ", key.Select(x => x.ToString("X2")))}");
            Logger.Debug(manifestType, $"{name} iv:{string.Join(" ", iv.Select(x => x.ToString("X2")))}");
        }

        public static BinaryReader GetDecryptedReader<T>(string name, string manifestType, T header, uint buildVersion, TACTProduct product, Stream stream) {
            GenerateKeyIV(name, manifestType, header, buildVersion, product, out byte[] key, out byte[] iv);

            using (Aes aes = Aes.Create()) {
                aes.KeySize = 128;
                aes.FeedbackSize = 128;
                aes.BlockSize = 128;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                return new BinaryReader(cryptoStream);
            }
        }

        private static byte[] CreateDigest(string value) {
            byte[] digest;
            using (SHA1 shaM = new SHA1Managed()) {
                byte[] stringBytes = Encoding.ASCII.GetBytes(value);
                digest = shaM.ComputeHash(stringBytes);
            }
            return digest;
        }

        public static Dictionary<Type, Dictionary<uint, object>> GetManifestProvidersForProduct(TACTProduct product) {
            if (!Providers.TryGetValue(product, out var providerCryptoTypeMap)) {
                providerCryptoTypeMap = new Dictionary<Type, Dictionary<uint, object>>();
                Providers[product] = providerCryptoTypeMap;
            }
            return providerCryptoTypeMap;
        }

        public static Dictionary<uint, object> GetManifestProvidersForProductAndInterface(TACTProduct product, Type t) {
            var providerCryptoTypeMap = GetManifestProvidersForProduct(product);

            if (!providerCryptoTypeMap.TryGetValue(t, out var typeVersionMap)) {
                typeVersionMap = new Dictionary<uint, object>();
                providerCryptoTypeMap[t] = typeVersionMap;
            }
            return typeVersionMap;
        }

        public static void AddProvider(TACTProduct product, Type @interface, object provider, uint buildVersion) {
            if (!s_headerTypeToProviderType.ContainsKey(@interface)) {
                var thisInterface = @interface.GetInterfaces().First(x =>
                                                                                 x.IsGenericType &&
                                                                                 x.GetGenericTypeDefinition() == typeof(IManifestCrypto<>));
                s_headerTypeToProviderType[thisInterface.GetGenericArguments()[0]] = @interface;
            }

            var typeVersionMap = GetManifestProvidersForProductAndInterface(product, @interface);
            typeVersionMap[buildVersion] = provider;
        }

        public static void AddProvidersFromAssembly<T>(Assembly asm) {
            List<Type> types = asm.GetTypes().Where(tt => tt != typeof(T) && typeof(T).IsAssignableFrom(tt)).ToList();
            foreach (Type tt in types) {
                if (tt.IsInterface) continue;

                var metadata = tt.GetCustomAttribute<ManifestCryptoAttribute>();
                if (metadata == null) continue;

                var provider = (T)Activator.CreateInstance(tt)!;
                if (metadata.AutoDetectVersion) {
                    AddProvider(metadata.Product, typeof(T), provider, uint.Parse(tt.Name.Split('_')[1]));
                }

                if (metadata.BuildVersions != null) {
                    foreach (var buildVersion in metadata.BuildVersions) {
                        AddProvider(metadata.Product, typeof(T), provider, buildVersion);
                    }
                }
            }
        }

        private static void FindProviders() {
            Assembly asm = typeof(ICMFEncryptionProc).Assembly;
            AddProvidersFromAssembly<ICMFEncryptionProc>(asm);
            AddProvidersFromAssembly<ITRGEncryptionProc>(asm);
        }

        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public class ManifestCryptoAttribute : Attribute {
            public bool AutoDetectVersion = true;
            public TACTProduct Product = TACTProduct.Overwatch;
            public uint[]? BuildVersions = new uint[0];
        }
    }
}