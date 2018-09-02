using System.Runtime.InteropServices;
using TACTLib.Container;

namespace TACTLib.Core.Product.WorldOfWarcraft {
    
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CASWarcraftV6Record {
        public CKey ContentKey;
        public ulong LookupHash;
    }
}
