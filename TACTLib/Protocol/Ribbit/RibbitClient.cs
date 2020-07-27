using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using TACTLib.Config;

namespace TACTLib.Protocol.Ribbit {
    public class RibbitClient {
        public readonly string m_host;
        
        public RibbitClient(string host) {
            m_host = host;
        }
        
        public Stream Get(string query) {
            using (TcpClient client = new TcpClient()) {
                if (!Uri.TryCreate(m_host, UriKind.RelativeOrAbsolute, out var uri)) {
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
                            if (line.StartsWith("##")) continue;
                            writer.WriteLine(line);
                        }
                    }
                }

                ms.Position = 0;
                
                return ms;
            }
        }

        public List<Dictionary<string, string>> GetKV(string query) {
            using (var stream = Get(query)) {
                if (stream == null) return null;
                using (var streamReader = new StreamReader(stream))
                    return InstallationInfo.ParseToDict(streamReader);
            }
        }

        public List<Dictionary<string, string>> GetVersions(string product) {
            return GetKV($"v1/products/{product}/versions");
        }
        
        public Dictionary<string, string> GetVersion(string product, string region) {
            return GetKV($"v1/products/{product}/versions").FirstOrDefault(x => x["Region"] == region);
        }
        
        public List<Dictionary<string, string>> GetBGDL(string product) {
            return GetKV($"v1/products/{product}/bgdl");
        }
        
        public Dictionary<string, string> GetBGDL(string product, string region) {
            return GetKV($"v1/products/{product}/bgdl").FirstOrDefault(x => x["Region"] == region);
        }
        
        public List<Dictionary<string, string>> GetCDNs(string product) {
            return GetKV($"v1/products/{product}/cdns");
        }
        
        public Dictionary<string, string> GetCDNs(string product, string region) {
            return GetKV($"v1/products/{product}/cdns").FirstOrDefault(x => x["Region"] == region);
        }
        
        public List<Dictionary<string, string>> GetSummary() {
            return GetKV("v1/summary");
        }
    }
}
