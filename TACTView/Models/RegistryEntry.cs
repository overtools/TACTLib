using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TACTLib;

namespace TACTView.Models {
    public record RegistryEntry<T>(string Name, TACTProduct Product, Type Type) where T : class {
        public T CreateInstance(IServiceCollection collection) {
            var provider = collection.AddSingleton(typeof(T), Type).BuildServiceProvider();
            collection.Remove(collection.Last());
            return provider.GetRequiredService<T>();
        }
    }
}
