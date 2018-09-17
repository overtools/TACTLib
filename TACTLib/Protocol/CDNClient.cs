using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using TACTLib.Client;
using TACTLib.Container;

namespace TACTLib.Protocol {
    public abstract class CDNClient : INetworkHandler {
        protected readonly ClientHandler client;

        protected CDNClient(ClientHandler handler) {
            client = handler;
        }

        public abstract Dictionary<string, string> CreateInstallationInfo(string region);

        public Stream OpenData(CKey key) {
            // todo: caching
            return FetchCDN("data", key.ToHexString());
        }

        public Stream OpenConfig(string key) {
            // todo: caching
            return FetchCDN("config", key);
        }

        private Stream FetchCDN(string type, string key) {
            key = key.ToLower();
            using (WebClient web = new WebClient()) {
                var hosts = client.InstallationInfo.Values["CDNHosts"].Split(' ');
                foreach (var host in hosts) {
                    try {
                        if (host == "cdn.blizzard.com") {
                            continue; // satan was here
                        }
                        var stream = web.OpenRead($"http://{host}/{client.InstallationInfo.Values["CDNPath"]}/{type}/{key.Substring(0, 2)}/{key.Substring(2, 2)}/{key}");
                        if (stream != null) {
                            return stream;
                        }
                    } catch {
                        // ignored
                    }
                }
            }

            return null;
        }
    }
}
