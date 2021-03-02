using System.Collections.Generic;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTView.Models;

namespace TACTView.Connectors {
    public class MNDXConnector : IConnector {
        public void Initialize(ClientHandler handler) {
            throw new System.NotImplementedException();
        }

        public Task<List<FileEntry>> GetFiles() {
            throw new System.NotImplementedException();
        }
    }
}
