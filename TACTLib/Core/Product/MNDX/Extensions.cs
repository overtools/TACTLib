using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TACTLib.Core.Product.MNDX
{
    public static class Extensions
    {
        /// <summary>
        /// Read array of structs from a BinaryReader
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <typeparam name="T">Struct to read</typeparam>
        /// <returns>Array of read structs</returns>
        public static T[] ReadArray<T>(this BinaryReader reader) where T : unmanaged
        {
            var numBytes = (int)reader.ReadInt64();
            if (numBytes == 0)
            {
                return Array.Empty<T>();
            }

            var result = reader.ReadBytes(numBytes);

            reader.BaseStream.Position += (0 - numBytes) & 0x07;
            return MemoryMarshal.Cast<byte, T>(result).ToArray();
        }
    }
}