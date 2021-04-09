using System.Collections.Generic;
using TACTLib.Container;

// ReSharper disable file NotAccessedField.Global
namespace TACTLib.Core.Product.CommonV2 {
    public class RootFile {
        public string FileID;
        public CKey MD5;
        public byte ChunkID;
        public byte Priority;
        public byte MPriority;
        public string FileName;
        public string InstallPath;
        
        public RootFile(IReadOnlyList<string> data) {
            FileID = data[0];
            MD5 = CKey.FromString(data[1]);
            ChunkID = byte.Parse(data[2]);
            Priority = byte.Parse(data[3]);
            MPriority = byte.Parse(data[4]);
            FileName = data[5];
            InstallPath = data[6];
        }
        public RootFile(string fileName, string md5)  {
            FileName = fileName;
            MD5 = CKey.FromString(md5);
        }
    }
}
