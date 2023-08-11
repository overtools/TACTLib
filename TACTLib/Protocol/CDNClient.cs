using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TACTLib.Client;

namespace TACTLib.Protocol {
    public abstract class CDNClient : INetworkHandler {
        protected readonly ClientHandler client;

        private static readonly HttpClientHandler s_httpClientHandler = new HttpClientHandler
        {
            // unlikely to be supported by cdn but...
            AutomaticDecompression = DecompressionMethods.All
        };
        private static readonly HttpClient s_httpClient = new HttpClient(s_httpClientHandler);

        protected CDNClient(ClientHandler handler) {
            client = handler;
        }

        public abstract Dictionary<string, string> CreateInstallationInfo(string region);

        public byte[]? OpenData(CKey key) {
            // todo: caching
            return FetchCDN("data", key.ToHexString());
        }

        public Stream? OpenConfig(string key) {
            // todo: caching
            var data = FetchCDN("config", key);
            if (data == null) return null;
            return new MemoryStream(data);
        }

        public byte[]? FetchCDN(string type, string key, (int, int)? range=null, string? suffix=null) {
            key = key.ToLower();
            var hosts = client.InstallationInfo.Values["CDNHosts"].Split(' ');
            foreach (var host in hosts) {
                try {
                    if (host == "cdn.blizzard.com" || host == "us.cdn.blizzard.com") {
                        continue; // satan was here
                    }
                    var url = $"http://{host}/{client.InstallationInfo.Values["CDNPath"]}/{type}/{key.Substring(0, 2)}/{key.Substring(2, 2)}/{key}";
                    Logger.Info("CDN", $"Fetching file {url}");
                    if (suffix != null) url += suffix;

                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                    if (range != null)
                    {
                        requestMessage.Headers.Range = new RangeHeaderValue(range.Value.Item1, range.Value.Item2);
                    }

                    using var response = s_httpClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                    if (!response.IsSuccessStatusCode) continue;
                    
                    var result = response.Content.ReadAsByteArrayAsync().Result;

                    return result;
                } catch {
                    // ignored
                }
            }

            return null;
        }
    }
}
