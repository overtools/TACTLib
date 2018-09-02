using System;
using System.IO;

namespace TACTLib {
    public enum TACTProduct {
        /// <summary>fallback</summary>
        Unknown,
        
        /// <summary>agent</summary>
        Agent,

        /// <summary>bna</summary>
        BattleNetApp,

        /// <summary>catalogs</summary>
        Catalog,

        /// <summary>d3, d3b, d3cn, d3t</summary>
        Diablo3,

        /// <summary>dst2, dst2a, dst2dev, dst2e1, dst2t</summary>
        Destiny2,

        /// <summary>hero, heroc, herot</summary>
        HeroesOfTheStorm,

        /// <summary>hsb, hst</summary>
        Hearthstone,

        /// <summary>pro, prot, proc*, proc2*, proc3, prodev, prov</summary>
        Overwatch,

        /// <summary>s1, s1a, s1t</summary>
        StarCraft1,

        /// <summary>s2, s2b, s2t, sc2</summary>
        StarCraft2,

        /// <summary>viper, viperdev</summary>
        BlackOps4,

        /// <summary>w3, w3t, war3</summary>
        Warcraft3,

        /// <summary>wow, wow_beta, wowdev, wowe1, wowe2, wowe3, wowt, wowv, wowz</summary>
        WorldOfWarcraft
    }

    public static class ProductHelpers {
        /// <summary>
        /// Get <see cref="TACTProduct"/> from product uid
        /// </summary>
        /// <param name="uid">Product uid</param>
        /// <returns>Product type</returns>
        /// <exception cref="NotImplementedException">Product is unknown</exception>
        public static TACTProduct ProductFromUID(string uid) {
            if (uid.StartsWith("hero"))
                return TACTProduct.HeroesOfTheStorm;

            if (uid.StartsWith("hs"))
                return TACTProduct.Hearthstone;

            if (uid.StartsWith("w3"))
                return TACTProduct.Warcraft3;

            if (uid.StartsWith("s1"))
                return TACTProduct.StarCraft1;

            if (uid.StartsWith("s2"))
                return TACTProduct.StarCraft2;

            if (uid.StartsWith("wow"))
                return TACTProduct.WorldOfWarcraft;

            if (uid.StartsWith("d3"))
                return TACTProduct.Diablo3;

            if (uid.StartsWith("agent"))
                return TACTProduct.Agent;

            if (uid.StartsWith("pro"))
                return TACTProduct.Overwatch;

            if (uid.StartsWith("bna"))
                return TACTProduct.BattleNetApp;

            if (uid.StartsWith("dst2"))
                return TACTProduct.Destiny2;

            if (uid.StartsWith("viper")) {
                return TACTProduct.BlackOps4;
            }

            throw new NotImplementedException($"unsupported product \"{uid}\"");
        }

        /// <summary>
        /// Get product uid from <see cref="TACTProduct"/> 
        /// </summary>
        /// <param name="product">product</param>
        /// <returns>Product type</returns>
        /// <exception cref="ArgumentOutOfRangeException">Product is unknown</exception>
        public static string UIDFromProduct(TACTProduct product) {
            switch (product) {
                case TACTProduct.HeroesOfTheStorm:
                    return "hero";
                case TACTProduct.Hearthstone:
                    return "hsb";
                case TACTProduct.Warcraft3:
                    return "w3";
                case TACTProduct.StarCraft1:
                    return "s1";
                case TACTProduct.StarCraft2:
                    return "s2";
                case TACTProduct.WorldOfWarcraft:
                    return "wow";
                case TACTProduct.Diablo3:
                    return "d3";
                case TACTProduct.Agent:
                    return "agent";
                case TACTProduct.Overwatch:
                    return "pro";
                case TACTProduct.BattleNetApp:
                    return "bna";
                case TACTProduct.Destiny2:
                    return "dst2";
                case TACTProduct.BlackOps4:
                    return "viper";
                case TACTProduct.Catalog:
                    return "catalogs";
                default:
                    throw new ArgumentOutOfRangeException(nameof(product), product, null);
            }
        }

        /// <summary>Get <see cref="TACTProduct"/> from install directory</summary>
        /// <param name="path">Container base path</param>
        /// <returns>Detected product</returns>
        /// <exception cref="NotImplementedException">Unable to recognise product type</exception>
        public static TACTProduct ProductFromLocalInstall(string path) {
            if (Directory.Exists(Path.Combine(path, "HeroesData")))
                return TACTProduct.HeroesOfTheStorm;

            if (Directory.Exists(Path.Combine(path, "SC2Data")))
                return TACTProduct.StarCraft2;

            if (Directory.Exists(Path.Combine(path, "Hearthstone_Data")))
                return TACTProduct.Hearthstone;

            if (File.Exists(Path.Combine(path, "Warcraft III.exe")))
                return TACTProduct.Warcraft3;

            if (Directory.Exists(Path.Combine(path, "Data"))) {
                if (File.Exists(Path.Combine(path, "Diablo III.exe")))
                    return TACTProduct.Diablo3;

                if (File.Exists(Path.Combine(path, "Wow.exe")) || File.Exists(Path.Combine(path, "WowT.exe")) || File.Exists(Path.Combine(path, "WowB.exe")))
                    return TACTProduct.WorldOfWarcraft;

                if (File.Exists(Path.Combine(path, "Agent.exe")))
                    return TACTProduct.Agent;

                if (File.Exists(Path.Combine(path, "Battle.net.exe")))
                    return TACTProduct.BattleNetApp;

                if (File.Exists(Path.Combine(path, "Overwatch.exe")))
                    return TACTProduct.Overwatch;

                if (File.Exists(Path.Combine(path, "StarCraft.exe")))
                    return TACTProduct.StarCraft1;

                if (File.Exists(Path.Combine(path, "BlackOps4.exe")))
                    return TACTProduct.BlackOps4;
            }

            throw new NotImplementedException("unable to detect product");
        }
    }
}