using System.IO;
using TACTLib.Client;
using TACTLib.Core.Product.CommonV2;

namespace TACTLib.Core.Product.D2
{
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable once InconsistentNaming
    [ProductHandler(TACTProduct.Diablo2)]
    public class ProductHandler_D2R : IProductHandler
    {
        private ClientHandler m_client { get; }

        public RootFile[] m_rootFiles { get; }
        public ProductHandler_D2R(ClientHandler client, Stream stream)
        {
            m_client = client;

            using (var reader = new StreamReader(stream))
            {
                m_rootFiles = RootFile.ParseList(reader).ToArray();
            }
        }
    }
}
