using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using TACTLib.Client;
using TACTLib.Config;

namespace TACTLib.Protocol.Ribbit {
    public class RibbitClient : CDNClient {
        public RibbitClient(ClientHandler handler) : base(handler) { }

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
            using (StreamReader cdnsReader = new StreamReader(GetRoot(client.CreateArgs.OnlineRootHost, $"v1/products/{ProductHelpers.UIDFromProduct(client.Product)}/cdns"))) {
                using (StreamReader versionReader = new StreamReader(GetRoot(client.CreateArgs.OnlineRootHost, $"v1/products/{ProductHelpers.UIDFromProduct(client.Product)}/versions"))) {
                    var cdns = InstallationInfo.ParseInternal(cdnsReader).FirstOrDefault(x => x["Name"] == region);
                    var versions = InstallationInfo.ParseInternal(versionReader).FirstOrDefault(x => x["Region"] == region);
                    var bi = new Dictionary<string, string> {
                        {"Active", "1"},
                        {"InstallKey", ""},
                        {"IMSize", ""},
                        {"Tags", "Online Ribbit"},
                        {"Armadillo", ""},
                        {"LastActivated", "0"},
                        {"BuildComplete", "1"}
                    };

                    foreach (var pair in RenameMapVersions) {
                        bi[pair.Value] = versions?[pair.Key];
                    }

                    foreach (var pair in RenameMapCDNs) {
                        bi[pair.Value] = cdns?[pair.Key];
                    }

                    return bi;
                }
            }
        }

        private static Stream GetRoot(string host, string query) {
            using (TcpClient client = new TcpClient()) {
                if (!Uri.TryCreate(host, UriKind.RelativeOrAbsolute, out var uri)) {
                    return null;
                }
                client.Connect(uri.Host, uri.Port);
                var buffer = Encoding.ASCII.GetBytes(query + "\r\n");
                var stream = client.GetStream();
                
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                var ms = new MemoryStream();
                
                using (StreamReader reader = new StreamReader(stream)) {
                    var text = reader.ReadToEnd().Split('\n');
                    var boundary = text.FirstOrDefault(x => x.Trim().StartsWith("Content-Type:"))?.Split(';').FirstOrDefault(x => x.Trim().StartsWith("boundary="))?.Split('"')[1].Trim();
                    var data = text.SkipWhile(x => x.Trim() != "--" + boundary).Skip(1).TakeWhile(x => x.Trim() != "--" + boundary).Skip(1);
                    using (StreamWriter writer = new StreamWriter(ms, Encoding.ASCII, 1024, true)) {
                        foreach (var line in data) {
                            writer.WriteLine(line);
                        }
                    }
                }

                ms.Position = 0;
                
                return ms;
            }
        }
    }
}
