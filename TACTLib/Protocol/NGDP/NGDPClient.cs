using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TACTLib.Client;
using TACTLib.Config;

namespace TACTLib.Protocol.NGDP {
    public class NGDPClient : CDNClient {
        public NGDPClient(ClientHandler handler) : base(handler) { }

        private static Dictionary<string, string> RenameMapVersions = new Dictionary<string, string> {
            {"Region", "Branch"},
            {"BuildConfig", "BuildKey"},
            {"CDNConfig", "CDNKey"},
            {"KeyRing", "Keyring"},
            {"VersionsName", "Version"},
        };

        private static Dictionary<string, string> RenameMapCDNs = new Dictionary<string, string> {
            {"Path", "CDNPath"},
            {"Hosts", "CDNHosts"},
            {"Servers", "CDNServers"},
        };

        public override Dictionary<string, string> CreateInstallationInfo(string region) {
            using (StreamReader cdnsReader = new StreamReader(GetRoot($"/{client.GetProduct()}/cdns"))) {
                using (StreamReader versionReader = new StreamReader(GetRoot($"/{client.GetProduct()}/versions"))) {
                    var cdns = InstallationInfo.ParseToDict(cdnsReader).FirstOrDefault(x => x["Name"] == region);
                    var versions = InstallationInfo.ParseToDict(versionReader).FirstOrDefault(x => x["Region"] == region);
                    var bi = new Dictionary<string, string> {
                        {"Active", "1"},
                        {"InstallKey", ""},
                        {"IMSize", ""},
                        {"Tags", "Online NGDP"},
                        {"Armadillo", ""},
                        {"LastActivated", "0"},
                        {"BuildComplete", "1"}
                    };

                    if (versions != null) {
                        foreach (var pair in RenameMapVersions) {
                            bi[pair.Value] = versions[pair.Key];
                        }
                    }

                    if (cdns != null) {
                        foreach (var pair in RenameMapCDNs) {
                            bi[pair.Value] = cdns[pair.Key];
                        }
                    }

                    return bi;
                }
            }
        }

        private Stream GetRoot(string url) {
            Logger.Info("CDN", $"Fetching NGDP {url}");
            using (WebClient web = new WebClient()) {
                return web.OpenRead($"{client.CreateArgs.OnlineRootHost}{url}");
            }
        }
    }
}
