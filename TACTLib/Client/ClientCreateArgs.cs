namespace TACTLib.Client {
    public class ClientCreateArgs {
        public TankArgs Tank = new TankArgs();

        public bool Online; // todo
        // will probably need some other fields too
        
        public string ExtraFileEnding = ""; // extra file "extension" that is appended to every file
        // can be used to protect archives from agent trying to "repair" them
        // rename every file except the executable so TACTLib can still detect which product it is
        
        public class TankArgs {
            public string TextLanguage = "enUS";
            public string SpokenLanguage = "enUS";
            public bool CacheAPM = true;

            // For dev tools
            public bool LoadAPM = true;
            public bool LoadCMF = true;
        }
    }
}