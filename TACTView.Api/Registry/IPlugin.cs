using System.Windows;
using JetBrains.Annotations;

namespace TACTView.Api.Registry {
    [PublicAPI]
    public interface IPlugin : IRegistryBase {
        Window? GetControl();
    }
}
