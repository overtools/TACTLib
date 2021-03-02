using System.Collections.Generic;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTView.Models;

namespace TACTView.Connectors {
    public class CommonV2Connector : IConnector {
        public void Initialize(ClientHandler handler) {
            throw new System.NotImplementedException();
        }

        public Task<List<FileEntry>> GetFiles() {
            throw new System.NotImplementedException();
        }
    }
}
