using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using TACTLib.Agent.Protobuf;

namespace TACTLib.Agent {
    public class AgentDatabase {
        public string FilePath { get; }
        public ProductDatabase Data { get; }

        public AgentDatabase() : this(null, false) { }

        // ReSharper disable once IntroduceOptionalParameters.Global
        public AgentDatabase(string path) : this(path, true) { }

        public AgentDatabase(string path, bool singleInstall) {
            FilePath = path;
            if (string.IsNullOrWhiteSpace(FilePath)) {
                FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net", "Agent", "product.db");
            }

            using (Stream product = File.OpenRead(FilePath)) {
                if (singleInstall) {
                    Data = new ProductDatabase {
                        ProductInstalls = new List<ProductInstall> {
                            Serializer.Deserialize<ProductInstall>(product)
                        }
                    };
                } else {
                    Data = Serializer.Deserialize<ProductDatabase>(product);
                }
            }
        }
    }
}
