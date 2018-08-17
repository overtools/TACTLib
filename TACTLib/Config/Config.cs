using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TACTLib.Container;

namespace TACTLib.Config {
    public class Config {
        public Dictionary<string, List<string>> Values;

        protected Config(string containerPath, string key) {
            string path = Path.Combine(containerPath, ContainerHandler.ConfigDirectory, key.Substring(0, 2), key.Substring(2, 2), key);

            using (StreamReader reader = new StreamReader(path)) {
                Read(reader);
            }
        }

        private void Read(TextReader reader) {
            Values = new Dictionary<string, List<string>>();
            for (int i = 0; i < 0xFF; i++) {
                string line = reader.ReadLine();
                if (line == null) break;
                
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                string[] tokens = line.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length != 2)
                    throw new Exception("Config: tokens.Length != 2");

                string[] values = tokens[1].Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                List<string> valuesList = values.ToList();
                Values.Add(tokens[0].Trim(), valuesList);
            }
        }
    }
}