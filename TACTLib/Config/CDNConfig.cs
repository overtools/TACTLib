using System.Collections.Generic;
using System.IO;

namespace TACTLib.Config {
    public class CDNConfig : Config {
        public CDNConfig(Stream? stream) : base(stream) { }

        public List<string> Archives => Values["archives"];
    }
}