using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTView.Api.Models;

namespace TACTView.Connectors {
    public class MNDXConnector : IConnector {
        public void Initialize(ClientHandler handler) {
            throw new System.NotImplementedException();
        }

        public Task<ObservableCollection<IDirectoryEntry>> GetFiles() {
            throw new System.NotImplementedException();
        }
    }
}
