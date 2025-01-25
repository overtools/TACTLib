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

        public string ManifestPlatform { get; set; } = PLATFORM_WIN;

        /// <summary>
        ///     Manifest Region declaration. Only two valid values are RDEV and RCN.
        /// </summary>
        public string ManifestRegion { get; set; } = REGION_DEV;

        /// <summary>
        /// Load manifest files. Flag here to allow disabling for development purposes
        /// </summary>
        public bool LoadManifest { get; set; } = true;
        
        /// <summary>
        /// Load bundles for lookup. Flag here to allow disabling for development purposes
        /// </summary>
        public bool LoadBundlesForLookup { get; set; } = true;
        
        public const string REGION_DEV = "DEV";
        public const string REGION_CN = "CN";

        public const string PLATFORM_WIN = "Win";
        public const string PLATFORM_WINPRISM = "WinPrism";
    }
}
