using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Container;

public static class StaticContainerHandlerFactory {
	private static readonly Dictionary<TACTProduct, Type> _handlers = new();

	public static IContainerHandler? GetHandler(TACTProduct product, ClientHandler client) {
		var handlerType = GetHandlerType(product);
		if (handlerType == null) return null;

		using var _ = new PerfCounter($"{handlerType.Name}::ctor`ClientHandler`Stream");
		return (IContainerHandler)Activator.CreateInstance(handlerType, client)!;
	}

	public static Type? GetHandlerType(TACTProduct product) {
		if (!_handlers.TryGetValue(product, out var type)) {
			type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => typeof(IContainerHandler).IsAssignableFrom(x) && x.GetCustomAttributes<StaticContainerHandlerAttribute>().Any(i => i.Product == product));
		}
		return type;
	}

	public static void SetHandler(TACTProduct product, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type) {
		_handlers[product] = type;
	}
}
