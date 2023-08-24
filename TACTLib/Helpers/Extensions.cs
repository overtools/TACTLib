using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace TACTLib.Helpers {
    public static class Extensions {
        #region BinaryReader
        
        public static void DefinitelyRead(this Stream stream, Span<byte> buffer)
        {
            while (buffer.Length > 0)
            {
                var read = stream.Read(buffer);
                if (read == 0) throw new EndOfStreamException();
                buffer = buffer.Slice(read);
            }
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
        public static unsafe T Read<T>(this BinaryReader reader) where T : unmanaged
        {
            Span<byte> stackMemory = stackalloc byte[sizeof(T)];
            reader.DefinitelyRead(stackMemory);
            return MemoryMarshal.Read<T>(stackMemory);
        }

        /// <summary>
        /// Read array of structs from a BinaryReader
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <typeparam name="T">Struct to read</typeparam>
        /// <returns>Array of read structs</returns>
        public static T[] ReadArray<T>(this BinaryReader reader) where T : unmanaged
        {
            // todo: why is this here? this is extremely specific
            
            var numBytes = (int)reader.ReadInt64();
            if (numBytes == 0)
            {
                return Array.Empty<T>();
            }

            var result = reader.ReadBytes(numBytes);

            reader.BaseStream.Position += (0 - numBytes) & 0x07;
            return MemoryMarshal.Cast<byte, T>(result).ToArray();
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
            if (count == 0) return Array.Empty<T>();
            
            var result = new T[count];
            reader.DefinitelyRead(MemoryMarshal.Cast<T, byte>(result));
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
        public static int ReadInt32BE(this BinaryReader reader) {
            var val = reader.ReadInt32();
            if (BitConverter.IsLittleEndian)
            {
                val = BinaryPrimitives.ReverseEndianness(val);
            }
            return val;
        }
        /// <summary>Read a big endian 16-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static short ReadInt16BE(this BinaryReader reader)
        {
            var val = reader.ReadInt16();
            if (BitConverter.IsLittleEndian)
            {
                val = BinaryPrimitives.ReverseEndianness(val);
            }
            return val;
        }
        
        /// <summary>Read a big endian 16-bit uint</summary>
        // ReSharper disable once InconsistentNaming
        public static ushort ReadUInt16BE(this BinaryReader reader)
        {
            return (ushort)ReadInt16BE(reader);
        }

        /// <summary>Read a big-endian 24-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static int ReadInt24BE(this BinaryReader reader)
        {
            Span<byte> data = stackalloc byte[3];
            reader.DefinitelyRead(data);
            return data[2] | (data[1] << 8) | (data[0] << 16);
        }
        #endregion
        
        /// <summary>Convert <see cref="Span{T}"/> to a hexadecimal string</summary>
        public static string ToHexString(this ReadOnlySpan<byte> data)
        {
            return BitConverter.ToString(data.ToArray()).Replace("-", string.Empty);
        }
    }
}