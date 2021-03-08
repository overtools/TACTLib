using System.Collections.Generic;
using System.Collections.ObjectModel;
using TACTLib;
using TACTLib.Agent.Protobuf;

namespace TACTView.Models {
    public record CASCEntry(TACTProduct DetectedProduct, string? Directory, string? Code, ProductInstall Install) {
        public ICollection<FlavoredCASCEntry> Flavors { get; set; } = new Collection<FlavoredCASCEntry>();

        public override string ToString() {
            return ProductHelpers.HumanReadableProduct(DetectedProduct);
        }
    }
}
