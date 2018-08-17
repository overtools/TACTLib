using System;
using System.IO;

namespace TACTLib {
    public enum Product {
        Agent,
        BattleNetApp,
        Catalog,
        Diablo3,
        Destiny2,
        HeroesOfTheStorm,
        Hearthstone,
        Overwatch,
        StarCraft1,
        StarCraft2,
        BlackOps4,
        Warcraft3,
        WorldOfWarcraft
    }

    public static class ProductHelpers {
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