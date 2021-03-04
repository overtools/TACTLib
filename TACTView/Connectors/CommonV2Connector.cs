using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TACTLib.Core.Product;
using TACTView.Api;
using TACTView.Api.Models;

namespace TACTView.Connectors {
    // TODO: move to separate DLL
    public class CommonV2Connector : IConnector {
        public void Initialize(IProductHandler handler) {
            throw new System.NotImplementedException();
        }

        public Task<ObservableCollection<IDirectoryEntry>> GetFiles() {
            throw new System.NotImplementedException();
        }
    }
}
