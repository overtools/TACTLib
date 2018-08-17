using TACTLib.Client;

namespace TACTLibTest {
    internal class Program {
        public static void Main(string[] args) {
            //const string path = @"C:\ow\game\Overwatch\";
            //const string path = @"D:\Games\Call of Duty Black Ops 4";
            
            ClientHandler clientHandler = new ClientHandler(args[0]);
        }
    }
}