using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TACTLib;

namespace TACTView.Models {
    internal record RegistryEntry<T>(string Name, TACTProduct Product) where T : class {
        internal Type Type { get; init; }

        internal T CreateInstance(ServiceCollection collection) {
            var provider = collection.AddSingleton<T>().AddSingleton(Type).BuildServiceProvider();
            collection.Remove(collection.Last());
            return provider.GetRequiredService<T>();
        }
    }
}
