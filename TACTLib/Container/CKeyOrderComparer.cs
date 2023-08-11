using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace TACTLib.Container {
    public class CKeyOrderComparer : IComparer<CKey> {
        public static readonly CKeyOrderComparer Instance = new CKeyOrderComparer();
            
        public int Compare(CKey x, CKey y) {
            return CKeyCompare(y, x);
        }
        
        public static int CKeyCompare(CKey left, CKey right)
        {
            var leftSpan = MemoryMarshal.CreateReadOnlySpan(ref left, 1).AsBytes();
            var rightSpan = MemoryMarshal.CreateReadOnlySpan(ref right, 1).AsBytes();

            var leftU0 = BinaryPrimitives.ReadUInt64BigEndian(leftSpan);
            var rightU0 = BinaryPrimitives.ReadUInt64BigEndian(rightSpan);

            var compareA = rightU0.CompareTo(leftU0);
            if (compareA != 0) return compareA;

            var leftU1 = BinaryPrimitives.ReadUInt64BigEndian(leftSpan);
            var rightU1 = BinaryPrimitives.ReadUInt64BigEndian(rightSpan);
            return rightU1.CompareTo(leftU1);
        }
    }
}