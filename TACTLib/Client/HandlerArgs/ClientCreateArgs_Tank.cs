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
        /// Load manifest files, for development purposes
        /// </summary>
        public bool LoadManifest { get; set; } = true;
    }
}
