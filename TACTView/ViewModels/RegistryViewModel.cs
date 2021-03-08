using System.ComponentModel;
using System.Linq;
using DragonLib;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using TACTLib;
using TACTView.Api.Registry;
using TACTView.Models;

namespace TACTView.ViewModels {
    [PublicAPI]
    public class RegistryViewModel : Singleton<RegistryViewModel>, IRegistry<IFileHandler>, IRegistry<IProductConnector>, IRegistry<IPlugin> {
        public BindingList<RegistryEntry<IFileHandler>> FileHandlers { get; } = new();
        public BindingList<RegistryEntry<IProductConnector>> ProductConnectors { get; } = new();
        public BindingList<RegistryEntry<IPlugin>> Plugins { get; } = new();

        bool IRegistry<IFileHandler>.Register<T>(TACTProduct product, string name) {
            FileHandlers.Add(new RegistryEntry<IFileHandler>(name, product, typeof(T)));
            return true;
        }

        bool IRegistry<IPlugin>.Register<T>(TACTProduct product, string name) {
            Plugins.Add(new RegistryEntry<IPlugin>(name, product, typeof(T)));
            return true;
        }

        bool IRegistry<IProductConnector>.Register<T>(TACTProduct product, string name) {
            if (ProductConnectors.Any(x => x.Product == product)) return false;
            ProductConnectors.Add(new RegistryEntry<IProductConnector>(name, product, typeof(T)));
            return true;
        }

        public IFileHandler[] GetFileHandlers(TACTProduct product, IServiceCollection collection) {
            return FileHandlers.Where(x => x.Product == product).Select(x => x.CreateInstance(collection)).ToArray();
        }

        public IPlugin[] GetPlugins(TACTProduct product, IServiceCollection collection) {
            return Plugins.Where(x => x.Product == product).Select(x => x.CreateInstance(collection)).ToArray();
        }

        public bool HasProductConnector(TACTProduct product) {
            return ProductConnectors.Any(x => x.Product == product);
        }

        public IProductConnector? GetProductConnector(TACTProduct product, IServiceCollection collection) {
            return ProductConnectors.FirstOrDefault(x => x.Product == product)?.CreateInstance(collection);
        }
    }
}
