using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Core.Product {
    public static class ProductHandlerFactory {
        public static IProductHandler GetHandler(TACTProduct product, ClientHandler client, Stream root) {
            var handler = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => typeof(IProductHandler).IsAssignableFrom(x) && x.GetCustomAttribute<ProductHandlerAttribute>()?.Product == product);
            if (handler == null) {
                return null;
            }

            using (var _ = new PerfCounter($"{handler.Name}::ctor"))
                return (IProductHandler)Activator.CreateInstance(handler, client, root);
        }
    }
}
