using System.Collections.Generic;
using System.IO;

namespace TACTLib.Config {
    public class InstallationInfo {
        public Dictionary<string, string> Values;
        
        public InstallationInfo(string path) {
            using (StreamReader reader = new StreamReader(path)) {
                Parse(reader);
            }
        }

        private void Parse(TextReader reader) {
            string[] keys = null;
            Values = new Dictionary<string, string>();
            
            for (int i = 0; i < 0xFF; i++) {
                string line = reader.ReadLine();
                if (line == null) break;
                
                string[] tokens = line.Split('|');

                if (i == 0) {
                    keys = new string[tokens.Length];

                    for (int j = 0; j < tokens.Length; j++) {
                        keys[j] = tokens[j].Split('!')[0].Replace(" ", "");
                    }
                } else {
                    if (keys == null) break;
                    
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    for (int j = 0; j < tokens.Length; ++j) {
                        vals[keys[j]] = tokens[j];
                    }

                    if (vals.ContainsKey("Active") && vals["Active"] == "1") {
                        Values = vals;
                        break;
                    }
                }
            }
        }
    }
}