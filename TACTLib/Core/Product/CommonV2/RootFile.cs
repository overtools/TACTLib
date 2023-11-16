using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable file NotAccessedField.Global
namespace TACTLib.Core.Product.CommonV2 {
    public class RootFile {
        public string? FileID;
        public CKey MD5;
        public byte ChunkID;
        public byte Priority;
        public byte MPriority;
        public string? FileName;
        public string? InstallPath;
        
        public RootFile()
        {
        }
        
        private RootFile(ReadOnlySpan<string> columns, string row)
        {
            Span<Range> ranges = stackalloc Range[columns.Length];
            MemoryExtensions.Split(row, ranges, '|');
            
            for (var i = 0; i < columns.Length; i++)
            {
                var valueSpan = row.AsSpan(ranges[i]);
                
                switch (columns[i])
                {
                    case "FILEID":
                    {
                        FileID = row[ranges[i]];
                        break;
                    }
                    case "MD5":
                    {
                        MD5 = CKey.FromString(valueSpan);
                        break;
                    }
                    case "CHUNK_ID":
                    {
                        ChunkID = byte.Parse(valueSpan);
                        break;
                    }
                    case "PRIORITY":
                    {
                        Priority = byte.Parse(valueSpan);
                        break;
                    }
                    case "MPRIORITY":
                    {
                        MPriority = byte.Parse(valueSpan);
                        break;
                    }
                    case "FILENAME":
                    {
                        FileName = row[ranges[i]];
                        break;
                    }
                    case "INSTALLPATH":
                    {
                        InstallPath = row[ranges[i]];
                        break;
                    }
                    default:
                    {
                        Logger.Debug("RootFile", $"Unknown column {columns[i]}");
                        break;
                    }
                }
            }
        }
        
        public static List<RootFile> ParseList(StreamReader reader)
        {
            var header = reader.ReadLine()!;
            if (header[0] != '#') throw new InvalidDataException($"bad header: \"{header}\"");
            
            var files = new List<RootFile>();
            
            var columns = header.Substring(1).Split('|');
            while (reader.ReadLine() is { } row)
            {
                var rootFile = new RootFile(columns, row);
                files.Add(rootFile);
            }
            
            return files;
        }
    }
}
