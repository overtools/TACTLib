using System.Collections.Generic;
using JetBrains.Annotations;
using TACTView.Api.Models;

namespace TACTView.Api.Registry {
    [PublicAPI]
    public interface IProductConnector : IRegistryBase {
        List<IDirectoryEntry> GetEntries();
    }
}
