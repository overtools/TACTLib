using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using TACTView.Api.Models;

namespace TACTView.Api.Registry {
    [PublicAPI]
    public interface IFileHandler : IRegistryBase {
        bool IsValidFor(IFileEntry entry);
        Control? GetControl(IFileEntry entry);
        Span<byte> GetData();
    }
}
