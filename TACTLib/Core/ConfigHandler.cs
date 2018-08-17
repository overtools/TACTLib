using System;
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
            if (client.InstallationInfo.Values.ContainsKey(name)) {
                LoadConfig(client, client.InstallationInfo.Values[name], out @out);
            } else {
                @out = null;
            }
        }

        private void LoadConfig<T>(ClientHandler client, string key, out T @out) {
            // hmm
            @out = (T)Activator.CreateInstance(typeof(T), client.ContainerHandler.ContainerDirectory, key);
        }
    }
}