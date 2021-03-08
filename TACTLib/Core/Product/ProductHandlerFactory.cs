using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Core.Product {
    public static class ProductHandlerFactory {
        private static readonly Dictionary<TACTProduct, Type> _handlers = new Dictionary<TACTProduct, Type>();

        static ProductHandlerFactory() {
            SetHandlersFromAssembly(Assembly.GetExecutingAssembly());
        }

        public static void SetHandlersFromAssembly(Assembly assembly) {
            foreach(var type in assembly.GetTypes().Where(x => typeof(IProductHandler).IsAssignableFrom(x)))
            {
                var attributes = type.GetCustomAttributes<ProductHandlerAttribute>();
                foreach (var attribute in attributes) {
                    if (_handlers.ContainsKey(attribute.Product)) {
                        Logger.Warn("ProductHandlerFactory", $"Duplicate product handler for type {attribute.Product:G} ({type})");
                        continue;
                    }

                    _handlers[attribute.Product] = type;
                }
            }
        }

        public static IProductHandler GetHandler(TACTProduct product, ClientHandler client, Stream root) {
            var handlerType = GetHandlerType(product);
            if (handlerType == null) return null;

            using (var _ = new PerfCounter($"{handlerType.Name}::ctor"))
                return (IProductHandler)Activator.CreateInstance(handlerType, client, root);
        }

        public static Type GetHandlerType(TACTProduct product) {
            if (!_handlers.TryGetValue(product, out var type)) {
                type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => typeof(IProductHandler).IsAssignableFrom(x) && x.GetCustomAttributes<ProductHandlerAttribute>().Any(i => i.Product == product));
            }
            return type;
        }

        public static bool HasHandler(TACTProduct product) {
            return _handlers.ContainsKey(product);
        }
        
        public static void SetHandler(TACTProduct product, Type type) {
            _handlers[product] = type;
        }
    }
}
