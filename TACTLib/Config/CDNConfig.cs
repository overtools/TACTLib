using System.Collections.Generic;
using System.IO;
using TACTLib.Client;

namespace TACTLib.Config {
    public class CDNConfig : Config {
        public CDNConfig(ClientHandler client, Stream stream) : base(client, stream) { }

        public List<string> Archives => Values["archives"];
    }
}