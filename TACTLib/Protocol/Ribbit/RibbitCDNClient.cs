using System.Collections.Generic;
using System.Linq;
using TACTLib.Client;

namespace TACTLib.Protocol.Ribbit {
    public class RibbitCDNClient : CDNClient {
        public RibbitClient m_client;
        
        public RibbitCDNClient(ClientHandler handler) : base(handler) {
            m_client = new RibbitClient(client.CreateArgs.OnlineRootHost);
        }

        private static readonly Dictionary<string, string> RenameMapVersions = new Dictionary<string, string> {
            {"Region", "Branch"},
            {"BuildConfig", "BuildKey"},
            {"CDNConfig", "CDNKey"},
            {"KeyRing", "Keyring"},
            {"VersionsName", "Version"},
        };

        private static readonly Dictionary<string, string> RenameMapCDNs = new Dictionary<string, string> {
            {"Path", "CDNPath"},
            {"Hosts", "CDNHosts"},
            {"Servers", "CDNServers"}
        };

        public override Dictionary<string, string> CreateInstallationInfo(string region) {
            var cdns = m_client.GetKV($"v1/products/{client.GetProduct()}/cdns")?.FirstOrDefault(x => x["Name"] == region);
            var versions = m_client.GetKV($"v1/products/{client.GetProduct()}/versions")?.FirstOrDefault(x => x["Region"] == region);
            
            var bi = new Dictionary<string, string> {
                {"Active", "1"},
                {"InstallKey", ""},
                {"IMSize", ""},
                {"Tags", "Online Ribbit"},
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
