using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TACTLib;
using TACTLib.Core.Product;
using TACTView.Api;
using TACTView.Api.Models;

namespace TACTView.Connectors {
    public class WorldOfWarcraftV6Connector : IProductConnector {
        public TACTProduct[] GetValidProducts() => new[] {TACTProduct.WorldOfWarcraft};

            public void Initialize(IProductHandler handler) {
            throw new System.NotImplementedException();
        }

        public Task<ObservableCollection<IDirectoryEntry>> GetFiles() {
            throw new System.NotImplementedException();
        }
    }
}
