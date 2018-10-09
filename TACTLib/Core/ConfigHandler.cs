using System;
using System.IO;
using TACTLib.Client;
using TACTLib.Config;

namespace TACTLib.Core {
    public class ConfigHandler {
        /// <summary>Build config</summary>
        public readonly BuildConfig BuildConfig;
        //public readonly PatchConfig PatchConfig;

        /// <summary>CDN config</summary>
        public readonly CDNConfig CDNConfig;

        /// <summary>Keyring config</summary>
        public readonly Keyring Keyring;

        public ConfigHandler(ClientHandler client) {
            LoadFromInstallationInfo(client, "BuildKey", out BuildConfig);
            LoadFromInstallationInfo(client, "CDNKey", out CDNConfig);
            LoadFromInstallationInfo(client, "Keyring", out Keyring);
        }

        private void LoadFromInstallationInfo<T>(ClientHandler client, string name, out T @out) where T : Config.Config {
            string key = client.InstallationInfo.Values[name];
            using (Stream stream = client.OpenConfigKey(key)) {
                if (client.InstallationInfo.Values.ContainsKey(name)) {
                    LoadConfig(client, stream, out @out);
                } else {
                    @out = null;
                }
            }
        }

        private static void LoadConfig<T>(ClientHandler client, Stream stream, out T @out) {
            // hmm
            @out = (T) Activator.CreateInstance(typeof(T), client, stream);
        }
    }
}
