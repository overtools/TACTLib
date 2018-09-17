using System.Collections.Generic;
using System.IO;
using TACTLib.Container;

namespace TACTLib.Protocol {
    public interface INetworkHandler {
        Dictionary<string, string> CreateInstallationInfo(string region);
        Stream OpenData(CKey key);
        Stream OpenConfig(string key);
    }
}