using TACTLib;
using TACTLib.Client;
using TACTLib.Protocol.Ribbit;

namespace TACTLibTest {
    internal static class Program {
        public static void Main(string[] args) {
            //const string path = @"C:\ow\game\Overwatch\";
            //const string path = @"D:\Games\Call of Duty Black Ops 4";

            Logger.RegisterBasic();

            ClientHandler onlineClientHandler = new ClientHandler(args[0], new ClientCreateArgs {
                Product = "pro",
                UseContainer = true,
                LoadSupportKeyring = true
            });
            Logger.Info("LOAD", onlineClientHandler.Product.ToString("G"));
        }
    }
}