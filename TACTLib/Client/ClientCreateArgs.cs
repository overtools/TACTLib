// ReSharper disable ConvertToConstant.Global

namespace TACTLib.Client {
    /// <summary>
    /// Client runtime args
    /// </summary>
    public class ClientCreateArgs {
        public enum InstallMode {
            Local,
            Remote
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
        public IHandlerArgs? HandlerArgs { get; set; } = null;

        public InstallMode VersionSource { get; set; } = InstallMode.Local;

        private bool m_useContainerBacking = true;
        public bool UseContainer {
            get => m_useContainerBacking || VersionSource == InstallMode.Local;
            set => m_useContainerBacking = value;
        }

        private bool m_onlineBacking;
        /// <summary>
        /// Download erroring files
        /// </summary>
        public bool Online {
            get => m_onlineBacking || VersionSource == InstallMode.Remote;
            set => m_onlineBacking = value;
        }

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
        public string? TextLanguage { get; set; } = null;

        /// <summary>
        /// Speech language to load in case product db doesn't
        /// </summary>
        public string? SpeechLanguage { get; set; } = null;

        /// <summary>
        /// The depot product flavor to load
        /// </summary>
        public string? Product { get; set; } = "pro";

        /// <summary>
        /// When true, it will load a support keyring for salsa keys not in the keyring.
        /// </summary>
        public bool LoadSupportKeyring { get; set; } = true;

        /// <summary>
        /// When true, it will load a support keyring remotely from the url set in <see cref="RemoteKeyringUrl"/>
        /// </summary>
        public bool LoadSupportKeyringFromRemote { get; set; } = true;

        /// <summary>
        /// URL to load the remote keyring from if <see cref="LoadSupportKeyringFromRemote"/> is true
        /// </summary>
        public string? RemoteKeyringUrl { get; set; }

        /// <summary>
        /// Support keyring to load, if null it will attempt to load "{TACTProduct}.keyring"
        /// </summary>
        public string? SupportKeyring { get; set; } = null;

        public string? OverrideBuildConfig { get; set; }
        public string? OverrideVersionName { get; set; }

        public ClientHandler? TryShareCDNIndexWithHandler { get; set; }
    }
}