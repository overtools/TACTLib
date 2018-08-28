namespace TACTLib.Client {
    public class ClientCreateArgs {
        public TankArgs Tank = new TankArgs();
        
        public class TankArgs {
            public string TextLanguage = "enUS";
            public string SpokenLanguage = "enUS";
            public bool CacheAPM = true;
        }
    }
}