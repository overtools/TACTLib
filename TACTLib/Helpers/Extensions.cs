using System;
using System.Buffers;
using System.IO;
using static TACTLib.Utils;

namespace TACTLib.Helpers {
    public static class Extensions {
        #region BinaryReader
        
        /// <summary>
        /// Read struct from a BinaryReader
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <typeparam name="T">Struct to read</typeparam>
        /// <returns>Read struct</returns>
        public static T Read<T>(this BinaryReader reader) where T : struct {
            byte[] result = reader.ReadBytes(FastStruct<T>.Size);
            return FastStruct<T>.ArrayToStructure(result);
        }

        /// <summary>
        /// Read array of structs from a BinaryReader
        /// </summary>
        /// <param name="reader">Source reader</param>
        /// <typeparam name="T">Struct to read</typeparam>
        /// <returns>Array of read structs</returns>
        public static T[] ReadArray<T>(this BinaryReader reader) where T : struct
        {
            int numBytes = (int)reader.ReadInt64();
            if (numBytes == 0)
            {
                return new T[0];
            }

            byte[] result = reader.ReadBytes(numBytes);

            reader.BaseStream.Position += (0 - numBytes) & 0x07;
            return FastStruct<T>.ReadArray(result);
        }
        
        /// <summary>
        /// Read array of structs from a reader
        /// </summary>
        /// <param name="reader">Target reader</param>
        /// <param name="count">Count</param>
        /// <typeparam name="T">Struct to read</typeparam>
        /// <returns>Stuct array</returns>
        public static T[] ReadArray<T>(this BinaryReader reader, int count) where T : struct
        {
            if(count == 0)
            {
                return new T[0];
            }

            int numBytes = FastStruct<T>.Size * count;

            byte[] result = reader.ReadBytes(numBytes);

            return FastStruct<T>.ReadArray(result);
        }

        /// <summary>
        /// Write a struct to a BinaryWriter
        /// </summary>
        /// <param name="writer">Target writer</param>
        /// <param name="struct">Struct instance to write</param>
        /// <typeparam name="T">Struct type</typeparam>
        public static void Write<T>(this BinaryWriter writer, T @struct) where T : struct
        {
            writer.Write(FastStruct<T>.StructureToArray(@struct));
        }
        
        /// <summary>
        /// Write an array of structs to a BinaryWriter
        /// </summary>
        /// <param name="writer">Target write</param>
        /// <param name="struct">Struct array to write</param>
        /// <typeparam name="T">Struct type</typeparam>
        public static void WriteStructArray<T>(this BinaryWriter writer, T[] @struct) where T : struct
        {
            writer.Write(FastStruct<T>.WriteArray(@struct));
        }
        
        /// <summary>Read a big endian 32-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static unsafe int ReadInt32BE(this BinaryReader reader) {
            int val = reader.ReadInt32();
            
            return Int32FromPtrBE((byte*)&val);
        }
        /// <summary>Read a big endian 16-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static unsafe short ReadInt16BE(this BinaryReader reader) {
            short val = reader.ReadInt16();
            
            return Int16FromPtrBE((byte*)&val);
        }
        
        /// <summary>Read a big endian 16-bit uint</summary>
        // ReSharper disable once InconsistentNaming
        public static ushort ReadUInt16BE(this BinaryReader reader) {
            return (ushort)ReadInt16BE(reader);
        }

        /// <summary>Read a big-endian 24-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static int ReadInt24BE(this BinaryReader reader) {
            byte[] data = reader.ReadBytes(3);
            return data[2] | (data[1] << 8) | (data[0] << 16);
        }
        #endregion
        
        /// <summary>Convert <see cref="Span{T}"/> to a hexadecimal string</summary>
        public static string ToHexString(this Span<byte> data) {
            return BitConverter.ToString(data.ToArray()).Replace("-", string.Empty);
        }
        
        /// <summary>Copy bytes from one stream to another</summary>
        public static void CopyBytes(this Stream input, Stream output, int bytes, int bufferSize=81920) {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try {
                int read;
                while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0) {
                    output.Write(buffer, 0, read);
                    bytes -= read;
                }
            } finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Reads stream data to span.
        /// https://github.com/dotnet/coreclr/blob/1456a38ed9ee3eda8022b9f162a45334723a0d7a/src/System.Private.CoreLib/shared/System/IO/Stream.cs
        /// </summary>
        /// <param name="input"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public static int Read(this Stream input, in Memory<byte> buffer) {
            byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                int numRead = input.Read(sharedBuffer, 0, buffer.Length);
                if ((uint)numRead > buffer.Length)
                {
                    throw new IOException("Stream was too long");
                }
                new Memory<byte>(sharedBuffer, 0, numRead).CopyTo(buffer);
                return numRead;
            }
            finally { ArrayPool<byte>.Shared.Return(sharedBuffer); }
        }
    }
}