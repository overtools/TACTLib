using System.IO;
using TACTLib;
using TACTLib.Client;
using TACTLib.Core.Product.Tank;

namespace TACTLibTest {
    internal static class Program {
        public static void Main(string[] args) {
            //const string path = @"C:\ow\game\Overwatch\";
            //const string path = @"D:\Games\Call of Duty Black Ops 4";
            
            Logger.RegisterBasic();
            
            //System.Diagnostics.Debugger.Break();
            ClientHandler clientHandler = new ClientHandler(args[0], new ClientCreateArgs());

            Logger.Info("LOAD", clientHandler.Product.ToString("G"));

            return;
            /*
            ClientHandler onlineClientHandler = new ClientHandler(args[0], new ClientCreateArgs {
                Mode = ClientCreateArgs.InstallMode.Ribbit,
                OnlineProduct = TACTProduct.Overwatch
            });
            Logger.Info("LOAD", onlineClientHandler.Product.ToString("G"));
            */
        }
    }
}