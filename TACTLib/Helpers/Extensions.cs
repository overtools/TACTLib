using System;
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
        public static int ReadInt32BE(this BinaryReader reader) {
            byte[] val = reader.ReadBytes(4);
            return Int32FromSpanBE(val);
        }
        /// <summary>Read a big endian 16-bit int</summary>
        // ReSharper disable once InconsistentNaming
        public static short ReadInt16BE(this BinaryReader reader) {
            byte[] val = reader.ReadBytes(2);
            return Int16FromSpanBE(val);
        }
        
        /// <summary>Read a big endian 16-bit uint</summary>
        // ReSharper disable once InconsistentNaming
        public static ushort ReadUInt16BE(this BinaryReader reader) {
            return (ushort)ReadInt16BE(reader);
        }
        #endregion
        
        /// <summary>Convert <see cref="Span{T}"/> to a hexadecimal string</summary>
        public static string ToHexString(this Span<byte> data) {
            return BitConverter.ToString(data.ToArray()).Replace("-", string.Empty);
        }
        
        /// <summary>Copy bytes from one stream to another</summary>
        public static void CopyBytes(this Stream input, Stream output, int bytes) {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0) {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}