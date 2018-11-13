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
            System.Diagnostics.Debugger.Break();
            if (clientHandler.ProductHandler is ProductHandler_Tank tankHandler) {
                using (Stream stream = tankHandler.OpenFile(0xE00000000000895)) {
                    
                }
            }
            
            System.Diagnostics.Debugger.Break();
            ClientHandler onlineClientHandler = new ClientHandler(args[0], new ClientCreateArgs {
                Mode = ClientCreateArgs.InstallMode.Ribbit,
                OnlineProduct = TACTProduct.Overwatch
            });
            System.Diagnostics.Debugger.Break();
            if (onlineClientHandler.ProductHandler is ProductHandler_Tank onlineTankHandler) {
                using (Stream stream = onlineTankHandler.OpenFile(0xE00000000000895)) {
                    
                }
            }
        }
    }
}