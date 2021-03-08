using JetBrains.Annotations;
using TACTLib;
using TACTView.Api.Registry;

namespace TACTView.Connectors {
    [PublicAPI]
    public class TankPlugin {
        public TankPlugin(IRegistry<IProductConnector> productRegistry) {
            productRegistry.Register<TankConnector>(TACTProduct.Overwatch, "Overwatch");
        }
    }
}
