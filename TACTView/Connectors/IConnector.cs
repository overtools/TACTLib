using System.Collections.Generic;
using System.Threading.Tasks;
using TACTLib;
using TACTLib.Client;
using TACTView.Models;

namespace TACTView.Connectors {
    public interface IConnector {
        void Initialize(ClientHandler handler);
        Task<List<FileEntry>> GetFiles();
    }
}
