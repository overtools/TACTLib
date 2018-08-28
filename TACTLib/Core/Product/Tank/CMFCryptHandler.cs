using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace TACTLib.Core.Product.Tank {
    public class CMFCryptHandler {
        #region Helpers
        // ReSharper disable once InconsistentNaming
        internal const uint SHA1_DIGESTSIZE = 20;
        
        internal static uint Constrain(long value) {
            return (uint)(value % uint.MaxValue);
        }
        
        internal static long SignedMod(long a, long b) {
            return a % b < 0 ? a % b + b : a % b;
        }
        #endregion
        
        private static readonly Dictionary<TACTLib.Product, Dictionary<uint, ICMFEncryptionProc>> Providers = new Dictionary<TACTLib.Product, Dictionary<uint, ICMFEncryptionProc>>();
        
        private static void FindProviders(TACTLib.Product product) {
            Providers[product] = new Dictionary<uint, ICMFEncryptionProc>();
            Assembly asm = typeof(ICMFEncryptionProc).Assembly;
            AddProviders(asm);
        }
        
        public static void GenerateKeyIV(string name, ContentManifestFile.CMFHeader header, TACTLib.Product product, out byte[] key, out byte[] iv) {
            if (!Providers.ContainsKey(product)) {
                FindProviders(product);
            }

            byte[] digest = CreateDigest(name);

            ICMFEncryptionProc provider;
            if (Providers[product].ContainsKey(header.BuildVersion)) {
                //TankLib.Helpers.Logger.Info("CASC", $"Using CMF procedure {header.BuildVersion}");
                provider = Providers[product][header.BuildVersion];
            } else {
                //TankLib.Helpers.Logger.Warn("CASC", $"No CMF procedure for build {header.BuildVersion}, trying closest version");
                try {
                    KeyValuePair<uint, ICMFEncryptionProc> pair = Providers[product].Where(it => it.Key < header.BuildVersion).OrderByDescending(it => it.Key).First();
                    //TankLib.Helpers.Logger.Info("CASC", $"Using CMF procedure {pair.Key}");
                    provider = pair.Value;
                } catch {
                    throw new CryptographicException("Missing CMF generators");
                }
            }

            key = provider.Key(header, 32);
            iv = provider.IV(header, digest, 16);

            //name = Path.GetFileNameWithoutExtension(name);
            //TankLib.Helpers.Logger.Debug("CMF", $"{name}: key={string.Join(" ", key.Select(x => x.ToString("X2")))}");
            //TankLib.Helpers.Logger.Debug("CMF", $"{name}: iv={string.Join(" ", iv.Select(x => x.ToString("X2")))}");
        }
        
        private static byte[] CreateDigest(string value) {
            byte[] digest;
            using (SHA1 shaM = new SHA1Managed()) {
                byte[] stringBytes = Encoding.ASCII.GetBytes(value);
                digest = shaM.ComputeHash(stringBytes);
            }
            return digest;
        }

        public static void AddProviders(Assembly asm) {
            Type t = typeof(ICMFEncryptionProc);
            List<Type> types = asm.GetTypes().Where(tt => tt != t && t.IsAssignableFrom(tt)).ToList();
            foreach (Type tt in types) {
                if (tt.IsInterface) {
                    continue;
                }
                CMFMetadataAttribute metadata = tt.GetCustomAttribute<CMFMetadataAttribute>();
                if (metadata == null) {
                    continue;
                }

                if (!Providers.ContainsKey(metadata.Product)) {
                    Providers[metadata.Product] = new Dictionary<uint, ICMFEncryptionProc>();
                }
                ICMFEncryptionProc provider = (ICMFEncryptionProc)Activator.CreateInstance(tt);
                if (metadata.AutoDetectVersion) {
                    Providers[metadata.Product][uint.Parse(tt.Name.Split('_')[1])] = provider;
                }

                if (metadata.BuildVersions != null) {
                    foreach (uint buildVersion in metadata.BuildVersions) {
                        Providers[metadata.Product][buildVersion] = provider;
                    }
                }
            }
        }
        
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public class CMFMetadataAttribute : Attribute {
            public bool AutoDetectVersion = true;
            public TACTLib.Product Product = TACTLib.Product.Overwatch;
            public uint[] BuildVersions = new uint[0];
        }
    }
}