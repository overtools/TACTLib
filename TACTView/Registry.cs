using System;
using JetBrains.Annotations;
using TACTLib;
using TACTView.Api.Registry;

namespace TACTView {
    [PublicAPI]
    public class Registry : IRegistry<IFileHandler>, IRegistry<IProductConnector>, IRegistry<IPlugin> {
        bool IRegistry<IFileHandler>.Register<T>(TACTProduct product, string name) {
            throw new NotImplementedException();
        }

        bool IRegistry<IProductConnector>.Register<T>(TACTProduct product, string name) {
            throw new NotImplementedException();
        }

        bool IRegistry<IPlugin>.Register<T>(TACTProduct product, string name) {
            throw new NotImplementedException();
        }
    }
}
