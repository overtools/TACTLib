using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TACTLib.Config {
    public class Config {
        public Dictionary<string, List<string>> Values;

        protected Config(Stream? stream) {
            Values = new Dictionary<string, List<string>>();
            if (stream == null) return;
            
            using (var reader = new StreamReader(stream)) {
                Read(reader);
            }
        }

        private void Read(TextReader reader) {
            string? line;
            while ((line = reader.ReadLine()) != null) {
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