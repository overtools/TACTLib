using TACTLib;
using TACTLib.Client;
using TACTLib.Protocol.Ribbit;

namespace TACTLibTest {
    internal static class Program {
        public static void Main(string[] args) {
            //const string path = @"C:\ow\game\Overwatch\";
            //const string path = @"D:\Games\Call of Duty Black Ops 4";
            
            Logger.RegisterBasic();

            var ribbit = new RibbitClient(ClientCreateArgs.US_RIBBIT);
            var summary = ribbit.GetSummary();
            var ver = ribbit.GetVersions("pro");
            
            //System.Diagnostics.Debugger.Break();
            var clientHandler = new ClientHandler(args[0], new ClientCreateArgs
            {
                Flavor = "retail"
            });

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