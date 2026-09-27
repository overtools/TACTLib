using System;

namespace TACTLib.Container;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class StaticContainerHandlerAttribute : Attribute {
	public TACTProduct Product;

	public StaticContainerHandlerAttribute(TACTProduct product) {
		Product = product;
	}
}
