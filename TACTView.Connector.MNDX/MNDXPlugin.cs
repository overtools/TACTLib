using JetBrains.Annotations;
using TACTLib;
using TACTView.Api.Registry;

namespace TACTView.Connectors {
    [PublicAPI]
    public class MNDXPlugin {
        public MNDXPlugin(IRegistry<IProductConnector> productRegistry) {
            productRegistry.Register<MNDXConnector>(TACTProduct.HeroesOfTheStorm, "Heroes of the Storm");
            productRegistry.Register<MNDXConnector>(TACTProduct.StarCraft2, "StarCraft II");
        }
    }
}
