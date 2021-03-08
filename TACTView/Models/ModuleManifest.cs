using JetBrains.Annotations;

namespace TACTView.Models {
    [PublicAPI]
    internal record ModuleManifest(string Name, string Version, string[] Authors, string EntryPoint, string MainModule) {
        internal object? Instance { get; set; }
    }
}
