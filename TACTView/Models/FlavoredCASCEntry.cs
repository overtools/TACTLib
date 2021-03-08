using TACTLib;
using TACTLib.Agent.Protobuf;

namespace TACTView.Models {
    public record FlavoredCASCEntry(TACTProduct DetectedProduct, string Root, string Flavor, string Code, ProductInstall Install) {
        public override string ToString() {
            return Code;
        }
    }
}
