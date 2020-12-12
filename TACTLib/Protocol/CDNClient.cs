using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using TACTLib.Client;
using TACTLib.Container;

namespace TACTLib.Protocol {
    public abstract class CDNClient : INetworkHandler {
        protected readonly ClientHandler client;
        private readonly HttpClient m_httpClient;

        protected CDNClient(ClientHandler handler) {
            client = handler;
            m_httpClient = new HttpClient();
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

        public Stream FetchCDN(string type, string key, (int, int)? range=null, string suffix=null) {
            key = key.ToLower();
            
            var hosts = client.InstallationInfo.Values["CDNHosts"].Split(' ');
            foreach (var host in hosts) {
                try {
                    if (host == "cdn.blizzard.com" || host == "us.cdn.blizzard.com") {
                        continue; // satan was here
                    }
                    var url = $"http://{host}/{client.InstallationInfo.Values["CDNPath"]}/{type}/{key.Substring(0, 2)}/{key.Substring(2, 2)}/{key}";
                    if (suffix != null) url += suffix;

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    if (range != null) {
                        request.Headers.Range = new RangeHeaderValue(range.Value.Item1, range.Value.Item2);
                    }
                    var response = m_httpClient.SendAsync(request).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode) continue;
                    return response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                } catch {
                    // ignored
                }
            }

            return null;
        }
    }
}
