using System.IO;
using TACTLib.Client;

namespace TACTLib.Config {
    public class CDNConfig : Config {
        public CDNConfig(ClientHandler client, Stream stream) : base(client, stream) { }
    }
}