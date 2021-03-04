using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TACTLib.Core.Product;
using TACTView.Api.Models;

namespace TACTView.Api {
    public interface IConnector {
        void Initialize(IProductHandler handler);
        Task<ObservableCollection<IDirectoryEntry>> GetFiles();
    }
}
