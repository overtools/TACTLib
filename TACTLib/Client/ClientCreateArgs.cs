// ReSharper disable ConvertToConstant.Global

namespace TACTLib.Client {
    /// <summary>
    /// Client runtime args
    /// </summary>
    public class ClientCreateArgs {
        /// <summary>
        /// Handler specific args
        /// </summary>
        /// <seealso cref="TACTLib.Client.HandlerArgs.ClientCreateArgs_Tank"/>
        /// <seealso cref="TACTLib.Client.HandlerArgs.ClientCreateArgs_WorldOfWarcraftV6"/> 
        public IHandlerArgs HandlerArgs;

        /// <summary>
        /// TODO
        /// </summary>
        public bool Online { get; set; } = false;
        // will probably need some other fields too

        /// <summary>
        /// Product DB filename, used for backup purposes
        /// </summary>
        public string ProductDBFilename { get; set; } = ".product.db";

        /// <summary>
        /// Build Info filename, used for backup purposes
        /// </summary>
        public string BuildInfoFilename { get; set; } = ".build.info";
        
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
    }
}