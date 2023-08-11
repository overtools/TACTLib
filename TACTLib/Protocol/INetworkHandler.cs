using System.Collections.Generic;
using System.IO;

namespace TACTLib.Protocol {
    public interface INetworkHandler {
        Dictionary<string, string> CreateInstallationInfo(string region);
        byte[]? OpenData(CKey key);
        Stream? OpenConfig(string key);
    }
}