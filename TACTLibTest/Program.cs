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
            var ver = ribbit.GetVersions("osib");
            
            //System.Diagnostics.Debugger.Break();
            ClientHandler clientHandler = new ClientHandler(args[0], new ClientCreateArgs
            {
                Flavor = "retail"
            });

            Logger.Info("LOAD", clientHandler.Product.ToString("G"));

            /*
            ClientHandler onlineClientHandler = new ClientHandler(args[0], new ClientCreateArgs {
                Mode = ClientCreateArgs.InstallMode.Ribbit,
                OnlineProduct = TACTProduct.Overwatch
            });
            Logger.Info("LOAD", onlineClientHandler.Product.ToString("G"));
            */
            /*
            var d2Handler = clientHandler.ProductHandler as TACTLib.Core.Product.ProductHandler_D2R;
            foreach (var f in d2Handler.m_rootFiles)
            {
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    clientHandler.OpenCKey(f.MD5).CopyTo(memoryStream);
                    var file = memoryStream.ToArray();
                }
            }*/
        }
    }
}