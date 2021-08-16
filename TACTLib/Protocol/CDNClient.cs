using System.Collections.Generic;
using System.IO;
using System.Net;
using TACTLib.Client;
using TACTLib.Container;

namespace TACTLib.Protocol {
    public abstract class CDNClient : INetworkHandler {
        protected readonly ClientHandler client;

        protected CDNClient(ClientHandler handler) {
            client = handler;
        }

        public abstract Dictionary<string, string> CreateInstallationInfo(string region);

        public Stream? OpenData(CKey key) {
            // todo: caching
            return FetchCDN("data", key.ToHexString());
        }

        public Stream? OpenConfig(string key) {
            // todo: caching
            return FetchCDN("config", key);
        }

        public Stream? FetchCDN(string type, string key, (int, int)? range=null, string? suffix=null) {
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
                    HttpWebRequest req = WebRequest.CreateHttp(url);
                    if (range != null)
                    {
                        req.AddRange(range.Value.Item1, range.Value.Item2);
                    }

                    using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse())
                    {
                        using var respStr = resp.GetResponseStream();
                        
                        MemoryStream ms = new MemoryStream();
                        respStr.CopyTo(ms);
                        ms.Position = 0;
                        return ms;
                    }
                } catch {
                    // ignored
                }
            }

            return null;
        }
    }
}
