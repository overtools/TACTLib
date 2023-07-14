using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TACTLib.Protocol;

namespace TACTLib.Config {
    public class InstallationInfo {
        public Dictionary<string, string> Values { get; private set; } = null!;

        public InstallationInfo(Dictionary<string, string> values) {
            Values = values;
        }

        public InstallationInfo(string path, string product) {
            using StreamReader reader = new StreamReader(path);
            Parse(reader, product);
        }

        public InstallationInfo(INetworkHandler netHandle, string region) {
            Values = netHandle.CreateInstallationInfo(region);
        }

        public static List<Dictionary<string, string>> ParseToDict(TextReader reader) {
            string[]? keys = null;
            List<Dictionary<string, string>> ret = new List<Dictionary<string, string>>();

            string? line;
            while ((line = reader.ReadLine()?.Trim()) != null) {
                if (line.Length == 0) {
                    continue;
                }

                string[] tokens = line.Split('|');

                if (keys == null) {
                    keys = new string[tokens.Length];

                    for (var j = 0; j < tokens.Length; j++) {
                        keys[j] = tokens[j].Split('!')[0].Replace(" ", "");
                    }
                } else {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    for (var j = 0; j < tokens.Length; ++j) {
                        vals[keys[j]] = tokens[j];
                    }

                    ret.Add(vals);
                }
            }

            return ret;
        }

        private void Parse(TextReader reader, string product) {
            var vals = ParseToDict(reader);

            var activeProducts = vals.Where(x => x["Active"] == "1").ToArray();
            if (activeProducts.Length == 0) {
                Logger.Error("InstallationInfo", "Found no Active products in installation info. Searching all.");
                activeProducts = vals.ToArray();
            }

            if (activeProducts.Length == 0) {
                throw new Exception($"Installation info contains 0 products. Requested product: {product}");
            }

            var valuesMatchingRequestedProduct = activeProducts.Where(x => {
                if (!x.TryGetValue("Product", out var foundProduct)) return false;
                return foundProduct.Equals(product, StringComparison.OrdinalIgnoreCase);
            }).SingleOrDefault();

            if (valuesMatchingRequestedProduct != null) {
                Values = valuesMatchingRequestedProduct;
                return;
            }

            // fenris has empty product field
            var containsProductField = activeProducts.Any(x => x.TryGetValue("Product", out var foundProduct) && !string.IsNullOrWhiteSpace(foundProduct));
            if (!containsProductField) {
                if (activeProducts.Length > 1) throw new Exception($"Installation info didn't contain (useful) Product field but >1 product was found. Requested product: {product}");
                Values = activeProducts.Single();
                return;
            }

            throw new Exception($"Failed to find installation info for requested product {product} (found [{string.Join(", ", vals.Select(x => {
                x.TryGetValue("Product", out var foundProduct);
                foundProduct ??= "null";
                return foundProduct;
            }))}])");
        }
    }

    public class InstallationInfoFile {
        public List<Dictionary<string, string>> Values { get; }

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
                data = Values.FirstOrDefault(x => x.GetValueOrDefault("Product") == product && x.GetValueOrDefault("Branch") == region);
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
