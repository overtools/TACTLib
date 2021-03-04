using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TACTLib;
using TACTLib.Core.Product;
using TACTView.Api;
using TACTView.Api.Models;

namespace TACTView.Connectors {
    public class CommonV2Connector : IProductConnector {
        public TACTProduct[] GetValidProducts() => new[] {TACTProduct.StarCraft1, TACTProduct.Warcraft3};

        public void Initialize(IProductHandler handler) {
            throw new System.NotImplementedException();
        }

        public Task<ObservableCollection<IDirectoryEntry>> GetFiles() {
            throw new System.NotImplementedException();
        }
    }
}
