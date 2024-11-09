using System;
using System.IO;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace TACTLib.Helpers {
    public static class Extensions {
        #region BinaryReader
        
        public static void DefinitelyRead(this Stream stream, Span<byte> buffer)
        {
            stream.ReadExactly(buffer);
        }

        public static void DefinitelyRead(this BinaryReader reader, Span<byte> buffer)
        {
            DefinitelyRead(reader.BaseStream, buffer);
        }
        
        /// <summary>
        /// Read struct from a BinaryReader
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <typeparam name="T">Struct to read</typeparam>
        /// <returns>Read struct</returns>
        public static T Read<T>(this BinaryReader reader) where T : unmanaged
        {
            return reader.BaseStream.Read<T>();
        }
        
        public static unsafe T Read<T>(this Stream stream) where T : unmanaged
        {
            var result = default(T);
            stream.DefinitelyRead(new Span<byte>(&result, sizeof(T)));
            return result;
        }
        
        /// <summary>
        /// Read array of structs from a reader
        /// </summary>
        /// <param name="reader">Target reader</param>
        /// <param name="count">Count</param>
        /// <typeparam name="T">Struct to read</typeparam>
        /// <returns>Stuct array</returns>
        public static T[] ReadArray<T>(this BinaryReader reader, int count) where T : unmanaged
        {
            return reader.BaseStream.ReadArray<T>(count);
        }
        
        public static T[] ReadArray<T>(this Stream stream, int count) where T : unmanaged
        {
            if (count == 0) return Array.Empty<T>();
            
            var result = new T[count];
            stream.DefinitelyRead(result.AsSpan().AsBytes());
            return result;
        }

        /// <summary>
        /// Write a struct to a BinaryWriter
        /// </summary>
        /// <param name="writer">Target writer</param>
        /// <param name="struct">Struct instance to write</param>
        /// <typeparam name="T">Struct type</typeparam>
        public static void Write<T>(this BinaryWriter writer, T @struct) where T : unmanaged
        {
            var bytes = MemoryMarshal.CreateReadOnlySpan(ref @struct, 1).AsBytes();
            writer.Write(bytes);
        }
        
        /// <summary>
        /// Write an array of structs to a BinaryWriter
        /// </summary>
        /// <param name="writer">Target write</param>
        /// <param name="struct">Struct array to write</param>
        /// <typeparam name="T">Struct type</typeparam>
        public static void WriteStructArray<T>(this BinaryWriter writer, T[] @struct) where T : unmanaged
        {
            var bytes = @struct.AsSpan().AsBytes();
            writer.Write(bytes);
        }
        
        /// <summary>Read a big endian 32-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static int ReadInt32BE(this BinaryReader reader)
        {
            var s = reader.Read<UInt32BE>();
            return (int)s.ToInt();
        }
        /// <summary>Read a big endian 16-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static short ReadInt16BE(this BinaryReader reader)
        {
            return (short)ReadUInt16BE(reader);
        }
        
        /// <summary>Read a big endian 16-bit uint</summary>
        // ReSharper disable once InconsistentNaming
        public static ushort ReadUInt16BE(this BinaryReader reader)
        {
            var s = reader.Read<UInt16BE>();
            return s.ToInt();
        }

        /// <summary>Read a big-endian 24-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static int ReadInt24BE(this BinaryReader reader)
        {
            var s = reader.Read<UInt24BE>();
            return s.ToInt();
        }
        #endregion
        
        /// <summary>Convert <see cref="Span{T}"/> to a hexadecimal string</summary>
        public static string ToHexString(this ReadOnlySpan<byte> data)
        {
            return Convert.ToHexString(data);
        }
    }
}