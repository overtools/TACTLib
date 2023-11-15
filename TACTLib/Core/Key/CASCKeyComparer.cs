using System;
using System.Collections.Generic;

namespace TACTLib.Core.Key {
    public class CASCKeyComparer : IEqualityComparer<TruncatedKey>, IEqualityComparer<FullKey> {
        private const uint FnvPrime32 = 0x1000193;
        private const uint FnvOffset32 = 0x811C9DC5;
        
        /// <summary>Static instance</summary>
        public static readonly CASCKeyComparer Instance = new CASCKeyComparer();
        
        public bool Equals(TruncatedKey x, TruncatedKey y) {
            return CASCKeyComparer.Equals(x, y);
        }
        
        public bool Equals(FullKey x, FullKey y) {
            return CASCKeyComparer.Equals(x, y);
        }

        private static bool Equals(ReadOnlySpan<byte> spanA, ReadOnlySpan<byte> spanB) {
            return spanA.SequenceEqual(spanB);
        }
        
        public unsafe int GetHashCode(TruncatedKey obj) {
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

        public unsafe int GetHashCode(FullKey obj) {
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