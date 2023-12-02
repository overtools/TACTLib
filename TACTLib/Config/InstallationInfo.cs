using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TACTLib.Config {
    public class InstallationInfo {
        public readonly Dictionary<string, string> Values;

        public InstallationInfo(Dictionary<string, string> values) {
            Values = values;
        }

        public InstallationInfo(List<Dictionary<string, string>> vals, string product) {
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

        public static List<Dictionary<string, string>> ParseToDict(TextReader reader) {
            string[]? keys = null;
            var valueDicts = new List<Dictionary<string, string>>();

            string? line;
            while ((line = reader.ReadLine()?.Trim()) != null) {
                if (line.Length == 0)
                {
                    continue;
                }
                if (line.StartsWith("##"))
                {
                    continue;
                }

                var tokens = line.Split('|');

                if (keys == null) {
                    keys = new string[tokens.Length];

                    for (var j = 0; j < tokens.Length; j++) {
                        keys[j] = tokens[j].Split('!')[0].Replace(" ", "");
                    }
                } else {
                    var values = new Dictionary<string, string>();
                    for (var j = 0; j < tokens.Length; ++j) {
                        values[keys[j]] = tokens[j];
                    }

                    valueDicts.Add(values);
                }
            }

            return valueDicts;
        }
    }
}
