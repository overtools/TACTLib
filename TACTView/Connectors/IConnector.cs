using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TACTLib;
using TACTLib.Client;
using TACTView.Api.Models;

namespace TACTView.Connectors {
    public interface IConnector {
        void Initialize(ClientHandler handler);
        Task<ObservableCollection<IDirectoryEntry>> GetFiles();
    }
}
