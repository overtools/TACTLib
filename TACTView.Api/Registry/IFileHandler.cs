using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using TACTView.Api.Models;

namespace TACTView.Api.Registry {
    [PublicAPI]
    public interface IFileHandler : IRegistryBase {
        bool IsValidFor(IDirectoryEntry entry);
        Control? GetControl(IDirectoryEntry entry);
        Span<byte> GetData();
    }
}
