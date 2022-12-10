using System;
using System.IO;
using System.Reflection;
using TACTLib.Client;
using TACTLib.Config;

namespace TACTLib.Core {
    public class ConfigHandler {
        /// <summary>Build config</summary>
        public readonly BuildConfig BuildConfig;

        /// <summary>CDN config</summary>
        public readonly CDNConfig CDNConfig;

        /// <summary>Keyring config</summary>
        public readonly Keyring Keyring;

        public ConfigHandler(ClientHandler client) {
            LoadFromInstallationInfo(client, "BuildKey", out BuildConfig? buildConfig);
            LoadFromInstallationInfo(client, "CDNKey", out CDNConfig? cdnConfig);
            LoadFromInstallationInfo(client, "Keyring", out Keyring? keyring);

            if (buildConfig == null) throw new NullReferenceException(nameof(buildConfig));
            BuildConfig = buildConfig;

            if (cdnConfig == null) throw new NullReferenceException(nameof(cdnConfig));
            CDNConfig = cdnConfig;

            if (keyring == null) {
                keyring = new Keyring(null);
            }
            Keyring = keyring;
            Keyring.LoadSupportKeyrings(client);
        }

        private void LoadFromInstallationInfo<T>(ClientHandler client, string name, out T? @out) where T : Config.Config {
            if (client.InstallationInfo.Values.TryGetValue(name, out var key) && !string.IsNullOrWhiteSpace(key)) {
                using (var stream = client.OpenConfigKey(key)) {
                    LoadConfig(stream, out @out);
                }
            } else {
                @out = null;
            }
        }

        private static void LoadConfig<T>(Stream? stream, out T @out) {
            // hmm
            @out = (T) Activator.CreateInstance(typeof(T), stream)!;
        }
    }
}