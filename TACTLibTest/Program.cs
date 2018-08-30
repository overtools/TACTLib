using System.IO;
using System.Threading;
using TACTLib;
using TACTLib.Client;
using TACTLib.Core.Product.Tank;

namespace TACTLibTest {
    internal class Program {
        public static void Main(string[] args) {
            //const string path = @"C:\ow\game\Overwatch\";
            //const string path = @"D:\Games\Call of Duty Black Ops 4";
            
            Logger.RegisterBasic();
            ClientHandler clientHandler = new ClientHandler(args[0], new ClientCreateArgs());
            
            Thread.Sleep(100000);

            if (clientHandler.ProductHandler is ProductHandler_Tank tankHandler) {
                using (Stream stream = tankHandler.OpenFile(0xE00000000000895)) {
                    
                }
            }
        }
    }
}