using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TACTLib;
using TACTLib.Agent;
using TACTLib.Core.Product;
using TACTView.Models;

namespace TACTView.ViewModels {
    public class MenuViewModel {
        public MenuViewModel() {
            var db = new AgentDatabase();
            var entries = new Dictionary<TACTProduct, Collection<CASCEntry>>();
            foreach (var install in db.Data.ProductInstall) {
                var product = TACTProduct.Unknown;
                try {
                    product = ProductHelpers.ProductFromUID(install.ProductCode);
                } catch {
                    // ignored
                }

                if (ShouldIgnore(product, install.ProductCode)) continue;

                if (!entries.TryGetValue(product, out var collection)) {
                    collection = new Collection<CASCEntry>();
                    entries[product] = collection;
                }

                collection.Add(new CASCEntry(product, install.Settings.InstallPath, install.ProductCode, install));
            }

            foreach (var (type, installs) in entries)
                switch (installs.Count) {
                    case 0:
                        continue;
                    case 1:
                        Installed.Add(installs.First());
                        break;
                    default: {
                        var sample = installs.First();
                        var flavored = new CASCEntry(type, null, sample.Install.ProductFamily, sample.Install);
                        foreach (var install in installs) flavored.Flavors.Add(new FlavoredCASCEntry(type, install.Install.Settings.InstallPath, install.Install.Settings.GameSubfolder, install.Install.ProductCode, install.Install));
                        Installed.Add(flavored);
                        break;
                    }
                }
        }

        public ICollection<RecentItem> Recent { get; set; } = new Collection<RecentItem>();
        public ICollection<CASCEntry> Installed { get; set; } = new Collection<CASCEntry>();

        private static bool ShouldIgnore(TACTProduct product, string code) {
            switch (product) {
                case TACTProduct.Agent: // not containerised
                case TACTProduct.BattleNetApp: // not containerised
                case TACTProduct.Catalog: // all streamed
                case TACTProduct.Destiny2: // root file is in exe, also not on bnet anymore
                case TACTProduct.Hearthstone: // root file is in exe
                case TACTProduct.Unknown: // ?
                    return true;
                case TACTProduct.Overwatch:
                    return code.ToLower() != "pro";
                default:
                    return !ProductHandlerFactory.HasHandler(product);
            }
        }
    }
}
