using System.IO;
using TACTLib.Client;

namespace TACTLib.Config {
    public class PatchConfig : Config {
        public PatchConfig(ClientHandler client, Stream? stream) : base(client, stream) {
            
        }
    }
}