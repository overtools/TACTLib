using System.ComponentModel;
using System.Linq;
using DragonLib;
using JetBrains.Annotations;
using TACTLib;
using TACTView.Api.Registry;
using TACTView.Models;

namespace TACTView.ViewModels {
    [PublicAPI]
    internal class RegistryViewModel : Singleton<RegistryViewModel>, IRegistry<IFileHandler>, IRegistry<IProductConnector>, IRegistry<IPlugin> {
        public BindingList<RegistryEntry<IFileHandler>> FileHandlers { get; } = new();
        public BindingList<RegistryEntry<IProductConnector>> ProductConnectors { get; } = new();
        public BindingList<RegistryEntry<IPlugin>> Plugins { get; } = new();

        bool IRegistry<IFileHandler>.Register<T>(TACTProduct product, string name) {
            if (FileHandlers.Any(x => x.Product == product)) return false;
            FileHandlers.Add(new RegistryEntry<IFileHandler>(name, product) {Type = typeof(T)});
            return true;
        }

        bool IRegistry<IProductConnector>.Register<T>(TACTProduct product, string name) {
            if (ProductConnectors.Any(x => x.Product == product)) return false;
            ProductConnectors.Add(new RegistryEntry<IProductConnector>(name, product) {Type = typeof(T)});
            return true;
        }

        bool IRegistry<IPlugin>.Register<T>(TACTProduct product, string name) {
            if (Plugins.Any(x => x.Product == product)) return false;
            Plugins.Add(new RegistryEntry<IPlugin>(name, product) {Type = typeof(T)});
            return true;
        }
    }
}
