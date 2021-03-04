using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TACTLib;
using TACTLib.Core.Product;
using TACTView.Api.Models;

namespace TACTView.Api {
    public interface IProductConnector {
        TACTProduct[] GetValidProducts();
        void Initialize(IProductHandler handler);
        Task<ObservableCollection<IDirectoryEntry>> GetFiles();
    }
}
