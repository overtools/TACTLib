using System.Collections.Generic;
using System.IO;
using System.Linq;
using TACTLib.Protocol;

namespace TACTLib.Config {
    public class InstallationInfoFile {
        public readonly List<Dictionary<string, string>> Values;

        public InstallationInfoFile(string path) {
            using StreamReader reader = new StreamReader(path);
            Values = InstallationInfo.ParseToDict(reader);
        }

        public InstallationInfoFile(INetworkHandler netHandle, string region) {
            Values = new List<Dictionary<string, string>> { netHandle.CreateInstallationInfo(region) };
        }

        /// <summary>
        /// Returns an <see cref="InstallationInfo"/> for the given product if it exists, returns the first active entry for the specified product
        /// Optionally you can specify a region if you want to get the installation info for a specific region
        /// </summary>
        /// <param name="product">product code</param>
        /// <param name="region">optional region</param>
        public InstallationInfo? GetInstallationInfoForProduct(string? product, string? region = null) {
            if (string.IsNullOrEmpty(product)) {
                return null;
            }

            Dictionary<string, string>? data;
            if (!string.IsNullOrEmpty(region)) {
                data = Values.FirstOrDefault(x => CollectionExtensions.GetValueOrDefault<string, string>(x, "Product") == product && CollectionExtensions.GetValueOrDefault<string, string>(x, "Branch") == region);
            } else {
                data = Values.OrderByDescending(x => x.GetValueOrDefault("Active") == "1").FirstOrDefault(x => x.GetValueOrDefault("Product") == product);
            }

            return data != null ? new InstallationInfo(data) : null;
        }

        /// <summary>
        /// Returns an <see cref="InstallationInfo"/> for the first active (or not active) entry if one exists
        /// </summary>
        public InstallationInfo? GetFirstOrDefault() {
            var data = Values.OrderByDescending(x => x["Active"] == "1").FirstOrDefault();
            return data != null ? new InstallationInfo(data) : null;
        }
    }
}
