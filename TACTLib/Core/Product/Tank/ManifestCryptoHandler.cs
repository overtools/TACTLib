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

        private static void FindProviders() {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) {
                AddProviders<ICMFEncryptionProc>(asm, Providers);
                AddProviders<ITRGEncryptionProc>(asm, Providers);
            }
        }

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

        public static void AddProviders<T>(Assembly asm, Dictionary<TACTProduct, Dictionary<Type, Dictionary<uint, object>>> providers) {
            if (!s_headerTypeToProviderType.ContainsKey(typeof(T))) {
                var thisInterface = typeof(T).GetInterfaces().First(x =>
                                                                      x.IsGenericType &&
                                                                      x.GetGenericTypeDefinition() == typeof(IManifestCrypto<>));
                s_headerTypeToProviderType[thisInterface.GetGenericArguments()[0]] = typeof(T);
            }

            List<Type> types = asm.GetTypes().Where(tt => tt != typeof(T) && typeof(T).IsAssignableFrom(tt)).ToList();
            foreach (Type tt in types) {
                if (tt.IsInterface) continue;

                var metadata = tt.GetCustomAttribute<ManifestCryptoAttribute>();
                if (metadata == null) continue;

                if (!providers.TryGetValue(metadata.Product, out var providerCryptoTypeMap)) {
                    providerCryptoTypeMap = new Dictionary<Type, Dictionary<uint, object>>();
                    providers[metadata.Product] = providerCryptoTypeMap;
                }
                if (!providerCryptoTypeMap.TryGetValue(typeof(T), out var typeVersionMap)) {
                    typeVersionMap = new Dictionary<uint, object>();
                    providerCryptoTypeMap[typeof(T)] = typeVersionMap;
                }

                var provider = (T)Activator.CreateInstance(tt)!;
                if (metadata.AutoDetectVersion) {
                    typeVersionMap[uint.Parse(tt.Name.Split('_')[1])] = provider;
                }

                if (metadata.BuildVersions != null) {
                    foreach (var buildVersion in metadata.BuildVersions) {
                        typeVersionMap[buildVersion] = provider;
                    }
                }
            }
        }

        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public class ManifestCryptoAttribute : Attribute {
            public bool AutoDetectVersion = true;
            public TACTProduct Product = TACTProduct.Overwatch;
            public uint[]? BuildVersions = new uint[0];
        }
    }
}