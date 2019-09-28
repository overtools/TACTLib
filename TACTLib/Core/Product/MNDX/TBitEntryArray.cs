using System.Collections.Generic;
using System.IO;
using static TACTLib.Helpers.Extensions;

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal class TBitEntryArray : List<int>
    {
        private int BitsPerEntry;
        private int EntryBitMask;
        private int TotalEntries;

        public new int this[int index]
        {
            get
            {
                int dwItemIndex = (index * BitsPerEntry) >> 0x05;
                int dwStartBit = (index * BitsPerEntry) & 0x1F;
                int dwEndBit = dwStartBit + BitsPerEntry;
                int dwResult;

                // If the end bit index is greater than 32,
                // we also need to load from the next 32-bit item
                if (dwEndBit > 0x20)
                {
                    dwResult = (base[dwItemIndex + 1] << (0x20 - dwStartBit)) | (int)((uint)base[dwItemIndex] >> dwStartBit);
                }
                else
                {
                    dwResult = base[dwItemIndex] >> dwStartBit;
                }

                // Now we also need to mask the result by the bit mask
                return dwResult & EntryBitMask;
            }
        }

        public TBitEntryArray(BinaryReader reader) : base(reader.ReadArray<int>())
        {
            BitsPerEntry = reader.ReadInt32();
            EntryBitMask = reader.ReadInt32();
            TotalEntries = (int)reader.ReadInt64();
        }
    }
}
