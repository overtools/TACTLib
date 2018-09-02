using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TACTLib.Client;
using TACTLib.Container;

namespace TACTLib.Core.Product.WorldOfWarcraft {
    [ProductHandler(TACTProduct.WorldOfWarcraft)]
    public class ProductHandler_WorldOfWarcraft : IProductHandler {
        private ClientHandler _client;

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
        
        public ProductHandler_WorldOfWarcraft(ClientHandler client, Stream stream) {
            _client = client;
            
            List<CASWarcraftV6> blocks = new List<CASWarcraftV6>();

            using (BinaryReader reader = new BinaryReader(stream)) {
                while (stream.Length - stream.Position > 0) {
                    CASWarcraftV6 block = new CASWarcraftV6(reader);
                    blocks.Add(block);

                    int blockIdx = blocks.Count - 1;

                    for (int i = 0; i < block.Records.Length; ++i) {
                        if (Assets.ContainsKey(block.Records[i].LookupHash)) continue;
                        Assets[block.Records[i].LookupHash] = new Asset(blockIdx, i);
                    }
                }
            }

            Blocks = blocks.ToArray();
        }
    }
}