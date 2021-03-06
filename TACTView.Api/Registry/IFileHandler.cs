using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using TACTView.Api.Models;

namespace TACTView.Api.Registry {
    [PublicAPI]
    public interface IFileHandler : IRegistryBase {
        public bool IsValidFor(IFileEntry entry);
        public Control? GetControl(IFileEntry entry);
        public Span<byte> GetData();
    }
}
