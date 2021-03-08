using JetBrains.Annotations;
using TACTLib;

namespace TACTView.Api.Registry {
    [PublicAPI]
    public interface IRegistry<in T> where T : IRegistryBase {
        public bool Register<TU>(TACTProduct product, string name) where TU : T;
    }
}
