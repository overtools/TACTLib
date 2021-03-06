using System;
using System.IO;
using Google.Protobuf;
using TACTLib.Agent.Protobuf;

namespace TACTLib.Agent {
    public class AgentDatabase {
        public string FilePath { get; }
        public Database Data { get; }

        public AgentDatabase() : this(null) { }

        public AgentDatabase(string path) {
            FilePath = path;
            if (string.IsNullOrWhiteSpace(FilePath)) {
                FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net", "Agent", "product.db");
            }

            var bytes = File.ReadAllBytes(FilePath);
            try {
                Data = Database.Parser.ParseFrom(bytes);
            } catch (InvalidProtocolBufferException) {
                Data = new Database {
                    ProductInstall = {ProductInstall.Parser.ParseFrom(bytes)}
                };
            }
        }
    }
}
