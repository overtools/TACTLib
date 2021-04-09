using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TACTLib.Client;
using TACTLib.Client.HandlerArgs;
using TACTLib.Core.Product.CommonV2;
using TACTLib.Exceptions;
using TACTLib.Helpers;

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

            var clientArgs = client.CreateArgs.HandlerArgs as ClientCreateArgs_Tank ?? new ClientCreateArgs_Tank();

            using (BinaryReader reader = new BinaryReader(stream))
            {
                string str = Encoding.ASCII.GetString(reader.ReadBytes((int)stream.Length));

                string[] array = str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                m_rootFiles = new RootFile[array.Length - 1];
                for (int i = 1; i < array.Length; i++)
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
