using System;
using System.IO;
using System.Text;
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

            using (BinaryReader reader = new BinaryReader(stream))
            {
                string str = Encoding.ASCII.GetString(reader.ReadBytes((int)stream.Length));

                string[] array = str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                m_rootFiles = new RootFile[array.Length - 1];
                for (var i = 1; i < array.Length; i++)
                {
                    m_rootFiles[i - 1] = new RootFile(array[i].Split('|')[0], array[i].Split('|')[1]);
                }
            }
        }

        /// <inheritdoc />
        public Stream OpenFile(object key)
        {
            throw new NotImplementedException();
        }
    }
}
