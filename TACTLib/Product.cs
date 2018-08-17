using System;
using System.IO;

namespace TACTLib {
    public enum Product {
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
        /// Get <see cref="Product"/> from product uid
        /// </summary>
        /// <param name="uid">Product uid</param>
        /// <returns>Product type</returns>
        /// <exception cref="NotImplementedException">Product is unknown</exception>
        public static Product ProductFromUID(string uid)
        {
            if (uid.StartsWith("hero"))
                return Product.HeroesOfTheStorm;
            
            if (uid.StartsWith("hs"))
                return Product.Hearthstone;

            if (uid.StartsWith("w3"))
                return Product.Warcraft3;

            if (uid.StartsWith("s1"))
                return Product.StarCraft1;

            if (uid.StartsWith("s2"))
                return Product.StarCraft2;

            if (uid.StartsWith("wow"))
                return Product.WorldOfWarcraft;

            if (uid.StartsWith("d3"))
                return Product.Diablo3;

            if (uid.StartsWith("agent"))
                return Product.Agent;

            if (uid.StartsWith("pro"))
                return Product.Overwatch;

            if (uid.StartsWith("bna"))
                return Product.BattleNetApp;

            if (uid.StartsWith("dst2"))
                return Product.Destiny2;

            if (uid.StartsWith("viper")) {
                return Product.BlackOps4;
            }
            throw new NotImplementedException($"unsupported product \"{uid}\"");
        }

        /// <summary>Get <see cref="Product"/> from install directory</summary>
        /// <param name="path">Container base path</param>
        /// <returns>Detected product</returns>
        /// <exception cref="NotImplementedException">Unable to recognise product type</exception>
        public static Product ProductFromLocalInstall(string path) {
            if (Directory.Exists(Path.Combine(path, "HeroesData")))
                return Product.HeroesOfTheStorm;

            if (Directory.Exists(Path.Combine(path, "SC2Data")))
                return Product.StarCraft2;

            if (Directory.Exists(Path.Combine(path, "Hearthstone_Data")))
                return Product.Hearthstone;

            if (File.Exists(Path.Combine(path, "Warcraft III.exe")))
                return Product.Warcraft3;

            if (Directory.Exists(Path.Combine(path, "Data")))
            {
                if (File.Exists(Path.Combine(path, "Diablo III.exe")))
                    return Product.Diablo3;

                if (File.Exists(Path.Combine(path, "Wow.exe")) || File.Exists(Path.Combine(path, "WowT.exe")) || File.Exists(Path.Combine(path, "WowB.exe")))
                    return Product.WorldOfWarcraft;

                if (File.Exists(Path.Combine(path, "Agent.exe")))
                    return Product.Agent;

                if (File.Exists(Path.Combine(path, "Battle.net.exe")))
                    return Product.BattleNetApp;

                if (File.Exists(Path.Combine(path, "Overwatch.exe")))
                    return Product.Overwatch;

                if (File.Exists(Path.Combine(path, "StarCraft.exe")))
                    return Product.StarCraft1;
                
                if (File.Exists(Path.Combine(path, "BlackOps4.exe")))
                    return Product.BlackOps4;
            }

            throw new NotImplementedException("unable to detect product");
        }
    }
}