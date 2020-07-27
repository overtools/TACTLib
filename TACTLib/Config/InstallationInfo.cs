using System.Collections.Generic;
using System.IO;
using System.Linq;
using TACTLib.Protocol;

namespace TACTLib.Config {
    public class InstallationInfo {
        public Dictionary<string, string> Values { get; private set; }
        
        public InstallationInfo(string path, string product) {
            using (StreamReader reader = new StreamReader(path)) {
                Parse(reader, product);
            }
        }

        public InstallationInfo(INetworkHandler netHandle, string region) {
            Values = netHandle.CreateInstallationInfo(region);
        }

        public static List<Dictionary<string, string>> ParseToDict(TextReader reader) {
            string[] keys = null;
            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();
            
            for (int i = 0; i < 0xFF; i++) {
                string line = reader.ReadLine()?.Trim();
                if (line == null) break;

                if (line.Length == 0) {
                    continue;
                }
                
                string[] tokens = line.Split('|');

                if (keys == null) {
                    keys = new string[tokens.Length];

                    for (int j = 0; j < tokens.Length; j++) {
                        keys[j] = tokens[j].Split('!')[0].Replace(" ", "");
                    }
                } else {
                    if (keys == null) break; // todo: this can't happen...
                    
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    for (int j = 0; j < tokens.Length; ++j) {
                        vals[keys[j]] = tokens[j];
                    }
                    
                    ret.Add(vals);
                }
            }

            return ret;
        }

        private void Parse(TextReader reader, string product) {
            var vals = ParseToDict(reader);
            Values = (Dictionary<string, string>) vals.FirstOrDefault(x => {
                if (x.TryGetValue("Product", out var entryProduct) && !entryProduct.Equals(product)) {
                    return false;
                }
                return x["Active"] == "1";
            });
        }
    }
}