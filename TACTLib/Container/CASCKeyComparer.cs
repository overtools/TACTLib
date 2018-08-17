using System.Collections.Generic;

namespace TACTLib.Container {
    public class CASCKeyComparer : IEqualityComparer<EKey>, IEqualityComparer<CKey> {
        private const uint FnvPrime32 = 16777619;
        private const uint FnvOffset32 = 2166136261;
        
        public static CASCKeyComparer Instance = new CASCKeyComparer();
        
        public unsafe bool Equals(EKey x, EKey y) {
            return Equals(x.Value, y.Value, EKey.CASC_EKEY_SIZE);
        }
        
        public unsafe bool Equals(CKey x, CKey y) {
            return Equals(x.Value, y.Value, CKey.CASC_CKEY_SIZE);
        }

        private static unsafe bool Equals(byte* valA, byte* valB, int count) {
            for (int i = 0; i < count; ++i)
                if (valA[i] != valB[i])
                    return false;

            return true;
        }

        public unsafe int GetHashCode(EKey obj) {
            return To32BitFnv1aHash((uint*) &obj);
        }

        public unsafe int GetHashCode(CKey obj) {
            return To32BitFnv1aHash((uint*) &obj);
        }

        // ReSharper disable once InconsistentNaming
        public static unsafe int To32BitFnv1aHash(uint* ptr) {
            uint hash = FnvOffset32;

            for (int i = 0; i < 4; i++) {
                hash ^= ptr[i];
                hash *= FnvPrime32;
            }

            return unchecked((int) hash);
        }
    }
}