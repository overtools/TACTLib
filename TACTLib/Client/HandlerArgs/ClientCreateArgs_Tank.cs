using TACTLib.Core.Product.Tank;

namespace TACTLib.Client.HandlerArgs {
    /// <inheritdoc />
    /// <summary>
    /// Args specific to Tank
    /// </summary>
    public class ClientCreateArgs_Tank : IHandlerArgs {
        /// <summary>
        /// Cache parsed APM file data
        /// </summary>
        public bool CacheAPM { get; set; } = true;

        /// <summary>
        ///     Manifest Region declaration. Only two valid values are RDEV and RCN.
        /// </summary>
        public string ManifestRegion { get; set; } = ProductHandler_Tank.REGION_DEV;

        /// <summary>
        /// Load manifest files. Flag here to allow disabling for development purposes
        /// </summary>
        public bool LoadManifest { get; set; } = true;
        
        /// <summary>
        /// Load bundles for lookup. Flag here to allow disabling for development purposes
        /// </summary>
        public bool LoadBundlesForLookup { get; set; } = true;
    }
}
