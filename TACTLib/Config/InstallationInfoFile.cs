using System.Collections.Generic;
using System.IO;

namespace TACTLib.Config {
    public class InstallationInfoFile {
        public readonly List<Dictionary<string, string>> Values;

        public InstallationInfoFile(string path) {
            using var reader = new StreamReader(path);
            Values = InstallationInfo.ParseToDict(reader);
        }
    }
}
