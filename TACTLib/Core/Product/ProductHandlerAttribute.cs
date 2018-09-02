using System;

namespace TACTLib.Core.Product {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ProductHandlerAttribute : Attribute {
        public TACTProduct Product;

        public ProductHandlerAttribute(TACTProduct product) {
            Product = product;
        }
    }
}
