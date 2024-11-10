using System;
using System.Collections.Generic;

namespace TACTLib.Core.Key {
    public class CASCKeyComparer : IEqualityComparer<TruncatedKey>, IEqualityComparer<FullKey> {
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
        
        public int GetHashCode(TruncatedKey obj) {
            var h = new HashCode();
            h.AddBytes(obj);
            return h.ToHashCode();
        }

        public int GetHashCode(FullKey obj) {
            var h = new HashCode();
            h.AddBytes(obj);
            return h.ToHashCode();
        }
    }
}