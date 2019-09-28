// ReSharper disable ConvertToConstant.Global

namespace TACTLib.Client {
    /// <summary>
    /// Client runtime args
    /// </summary>
    public class ClientCreateArgs {
        public enum InstallMode {
            /// <summary>
            /// Local file loading
            /// </summary>
            CASC,
            /// <summary>
            /// Online Ribbit
            /// </summary>
            Ribbit,
            /// <summary>
            /// Online NGDP
            /// </summary>
            NGDP,
        }

        public const string US_NGDP = "http://us.patch.battle.net:1119";
        public const string EU_NGDP = "http://eu.patch.battle.net:1119";
        public const string CN_NGDP = "http://cn.patch.battle.net:1119";
        public const string KR_NGDP = "http://kr.patch.battle.net:1119";
        public const string TW_NGDP = "http://tw.patch.battle.net:1119";

        public const string US_RIBBIT = "ribbit://us.version.battle.net:1119";
        public const string EU_RIBBIT = "ribbit://eu.version.battle.net:1119";
        public const string CN_RIBBIT = "ribbit://cn.version.battle.net:1119";
        public const string KR_RIBBIT = "ribbit://kr.version.battle.net:1119";
        public const string TW_RIBBIT = "ribbit://tw.version.battle.net:1119";

        /// <summary>
        /// Root host for online download
        /// </summary>
        public string OnlineRootHost { get; set; } = US_RIBBIT;
        
        /// <summary>
        /// Handler specific args
        /// </summary>
        /// <seealso cref="TACTLib.Client.HandlerArgs.ClientCreateArgs_Tank"/>
        /// <seealso cref="TACTLib.Client.HandlerArgs.ClientCreateArgs_WorldOfWarcraftV6"/> 
        public IHandlerArgs HandlerArgs { get; set; } = null;

        /// <summary>
        /// Sets 
        /// </summary>
        public InstallMode Mode { get; set; } = InstallMode.CASC;

        /// <summary>
        /// Download erroring files
        /// </summary>
        public bool Online { get; set; } = true;

        /// <summary>
        /// Region to load from NGDP/Ribbit.
        /// </summary>
        public string OnlineRegion { get; set; } = "us";

        /// <summary>
        /// Name of the product database file
        /// </summary>
        /// <seealso cref="TACTLib.Agent.AgentDatabase"/>
        public string ProductDatabaseFilename { get; set; } = ".product.db";

        /// <summary>
        /// Name of the installation info file
        /// </summary>
        /// <seealso cref="TACTLib.Config.InstallationInfo"/>
        public string InstallInfoFileName { get; set; } = ".build.info";
        
        /// <summary>
        /// extra file "extension" that is appended to every file
        /// can be used to protect archives from agent trying to "repair" them
        /// rename every file except the executable so TACTLib can still detect which product it is
        /// </summary>
        public string ExtraFileEnding { get; set; } = "";
        
        /// <summary>
        /// Text Language to load in case product db doesn't
        /// </summary>
        public string TextLanguage { get; set; } = null;
        
        /// <summary>
        /// Speech language to load in case product db doesn't
        /// </summary>
        public string SpeechLanguage { get; set; } = null;

        /// <summary>
        /// The online product branch to load
        /// </summary>
        public TACTProduct OnlineProduct { get; set; } = TACTProduct.Catalog;

        /// <summary>
        /// The depot flavor to load
        /// </summary>
        public string Flavor { get; set; } = "retail";
    }
}