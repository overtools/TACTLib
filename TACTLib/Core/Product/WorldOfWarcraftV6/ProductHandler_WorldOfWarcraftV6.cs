using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using TACTLib.Client;
using TACTLib.Client.HandlerArgs;

// ReSharper disable NotAccessedField.Global

namespace TACTLib.Core.Product.WorldOfWarcraftV6 {
    [ProductHandler(TACTProduct.WorldOfWarcraft)]
    public class ProductHandler_WorldOfWarcraftV6 : IProductHandler {
        public CASWarcraftV6[] Blocks;

        public ConcurrentDictionary<ulong, Asset> Assets = new ConcurrentDictionary<ulong, Asset>();

        public struct Asset {
            public int BlockIdx;
            public int RecordIdx;

            public Asset(int blockIdx, int recordIdx) {
                BlockIdx = blockIdx;
                RecordIdx = recordIdx;
            }
        }

        public ProductHandler_WorldOfWarcraftV6(ClientHandler client, Stream stream) {
            var clientArgs = client.CreateArgs.HandlerArgs as ClientCreateArgs_WorldOfWarcraftV6 ?? new ClientCreateArgs_WorldOfWarcraftV6();

            List<CASWarcraftV6> blocks = new List<CASWarcraftV6>();

            using (BinaryReader reader = new BinaryReader(stream)) {
                while (stream.Length - stream.Position > 0) {
                    CASWarcraftV6 block = new CASWarcraftV6(reader);
                    blocks.Add(block);

                    var blockIdx = blocks.Count - 1;

                    for (var i = 0; i < block.Records.Length; ++i) {
                        if (Assets.ContainsKey(block.Records[i].LookupHash)) continue;
                        Assets[block.Records[i].LookupHash] = new Asset(blockIdx, i);
                    }
                }
            }

            Blocks = blocks.ToArray();
        }

        /// <inheritdoc />
        public Stream OpenFile(object key) {
            switch (key) {
                case string path:
                    return OpenFile(path);
                case ulong lookup:
                    return OpenFile(lookup);
                default:
                    throw new InvalidDataException(nameof(key));
            }
        }

        /// <summary>
        /// Open file by string path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public Stream OpenFile(string path) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Open file by Jenkins lookup3 hash
        /// </summary>
        /// <param name="lookup"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public Stream OpenFile(ulong lookup) {
            throw new NotImplementedException();
        }
    }
}