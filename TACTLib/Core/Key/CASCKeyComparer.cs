using System;
using System.Collections.Generic;

namespace TACTLib.Core.Key {
    public class CASCKeyComparer : IEqualityComparer<EKey>, IEqualityComparer<CKey> {
        private const uint FnvPrime32 = 0x1000193;
        private const uint FnvOffset32 = 0x811C9DC5;
        
        /// <summary>Static instance</summary>
        public static CASCKeyComparer Instance = new CASCKeyComparer();
        
        public unsafe bool Equals(EKey x, EKey y) {
            return Equals(x.Value, y.Value, EKey.CASC_TRUNCATED_KEY_SIZE);
        }
        
        public unsafe bool Equals(CKey x, CKey y) {
            return Equals(x.Value, y.Value, CKey.CASC_FULL_KEY_SIZE);
        }

        private static unsafe bool Equals(byte* valA, byte* valB, int count) {
            var spanA = new ReadOnlySpan<byte>(valA, count);
            var spanB = new ReadOnlySpan<byte>(valB, count);
            return spanA.SequenceEqual(spanB);
        }
        
        public unsafe int GetHashCode(EKey obj) {
            var hash = FnvOffset32;
            var ptr = (uint*) &obj;

            for (var i = 0; i < 2; i++) {
                hash ^= ptr[i];
                hash *= FnvPrime32;
            }

            var hashPtr = (byte*) &hash;
            var b = *((byte*)ptr + 8);
            for (var i = 0; i < 4; ++i) {
                hashPtr[i] ^= b;
            }

            return unchecked((int) hash);
        }

        public unsafe int GetHashCode(CKey obj) {
            var hash = FnvOffset32;
            
            var ptr = (uint*) &obj;

            for (var i = 0; i < 4; i++) {
                hash ^= ptr[i];
                hash *= FnvPrime32;
            }

            return unchecked((int) hash);
        }
    }
}