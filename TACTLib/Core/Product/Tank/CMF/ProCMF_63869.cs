using static TACTLib.Core.Product.Tank.CMFCryptHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
  [CMFMetadataAttribute(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
  public class ProCMF_63869 : ICMFEncryptionProc
  {
    public byte[] Key(CMFHeader header, int length)
    {
      byte[] buffer = new byte[length];
      uint kidx, okidx;
      kidx = okidx = Keytable[length + 256];
      for (uint i = 0; i != length; ++i)
      {
        buffer[i] = Keytable[SignedMod(kidx, 512)];
        kidx += (uint)header.EntryCount;
      }
     return buffer;
    }

    public byte[] IV(CMFHeader header, byte[] digest, int length)
    {
      byte[] buffer = new byte[length];
      uint kidx, okidx;
      kidx = okidx = Keytable[SignedMod((2 * digest[13]) - length, 512)];
      for (int i = 0; i != length; ++i)
      {
        buffer[i] = Keytable[SignedMod(kidx, 512)];
        kidx = header.BuildVersion - kidx;
        buffer[i] ^= digest[SignedMod(kidx + i, SHA1_DIGESTSIZE)];
      }
      return buffer;
    }

    private static readonly byte[] Keytable =
    {
      0x9E, 0x7F, 0x61, 0x85, 0x44, 0x1D, 0x18, 0xAF, 0xE3, 0x7E, 0x63, 0x97, 0xBF, 0x3C, 0x88, 0x3A, 
      0x4F, 0xB8, 0xFC, 0x0C, 0x82, 0xA1, 0x9B, 0x63, 0x2A, 0x91, 0xAC, 0x85, 0x96, 0xAE, 0x89, 0xDF, 
      0x06, 0x62, 0xF5, 0x78, 0xB9, 0x19, 0xFB, 0xCB, 0xA5, 0xF9, 0x9D, 0x69, 0x5A, 0x30, 0xC2, 0xE6, 
      0x45, 0x31, 0x9B, 0xAC, 0x2D, 0x1C, 0x7C, 0x40, 0x44, 0x94, 0x7B, 0xF7, 0xAE, 0x47, 0x50, 0xF2, 
      0xFB, 0x30, 0x6A, 0xFE, 0xCB, 0x30, 0x6A, 0x40, 0x08, 0x1D, 0x88, 0x01, 0x57, 0xF9, 0x62, 0x87, 
      0xEE, 0x76, 0x3D, 0xA9, 0x20, 0x3D, 0x01, 0xA4, 0x43, 0xC5, 0x35, 0x40, 0xA2, 0x40, 0x71, 0x5D, 
      0x25, 0xE1, 0x9C, 0x91, 0xCE, 0xC8, 0xC6, 0x03, 0xEA, 0x0E, 0x23, 0x66, 0x8C, 0x7D, 0x90, 0xB9, 
      0x59, 0xBA, 0x80, 0xCD, 0xF2, 0xB2, 0xA0, 0x6D, 0x96, 0x8E, 0x18, 0x5A, 0x42, 0x26, 0x87, 0x29, 
      0x39, 0x6C, 0x1D, 0x4C, 0x67, 0xD0, 0xDA, 0xEC, 0x9D, 0xEB, 0xC3, 0xB0, 0x40, 0x86, 0xD4, 0xB3, 
      0xC8, 0xB4, 0x00, 0x78, 0x82, 0x74, 0x1C, 0x79, 0x78, 0x90, 0x1E, 0x87, 0xC1, 0x37, 0x78, 0x17, 
      0x44, 0x3A, 0x10, 0x3D, 0xC1, 0x27, 0xFE, 0x71, 0x23, 0x83, 0xD8, 0xAD, 0xF9, 0x9D, 0x9C, 0x30, 
      0xAD, 0x87, 0xFC, 0x83, 0x46, 0x65, 0x26, 0xEC, 0xF1, 0x1A, 0x8C, 0x50, 0x7A, 0x1D, 0xD7, 0x62, 
      0x20, 0xDC, 0xC1, 0xCF, 0xC7, 0x89, 0x6A, 0xCA, 0x27, 0x93, 0xA3, 0xC3, 0xAB, 0xB7, 0xC1, 0x3C, 
      0x41, 0x6C, 0xDC, 0x37, 0xA6, 0x20, 0x4D, 0x35, 0x19, 0x28, 0x9B, 0x22, 0x54, 0xCA, 0x9F, 0xAD, 
      0xF0, 0xA4, 0x3D, 0x5E, 0xD4, 0x59, 0xE4, 0xF6, 0xC4, 0x78, 0x78, 0x3D, 0x01, 0x27, 0xE1, 0xE4, 
      0xFB, 0x44, 0xD9, 0xAB, 0xDB, 0x26, 0xE2, 0x3C, 0x76, 0x60, 0x8F, 0x05, 0xBD, 0x17, 0xE3, 0x66, 
      0x97, 0x33, 0x2F, 0x68, 0x78, 0xAF, 0x7B, 0x7F, 0x00, 0xCA, 0xA2, 0xA6, 0x62, 0x7D, 0xE3, 0x8E, 
      0xCF, 0xE2, 0xF7, 0x43, 0x55, 0xFC, 0x8A, 0xA8, 0xAC, 0xC1, 0xA1, 0xF2, 0x4C, 0x00, 0x85, 0x53, 
      0xDA, 0xCE, 0x7E, 0x83, 0x49, 0x6F, 0x45, 0x19, 0xFB, 0x99, 0xD7, 0x77, 0x69, 0xB2, 0xEF, 0xA8, 
      0x7C, 0x71, 0x88, 0x43, 0x0D, 0x58, 0x17, 0xD2, 0x47, 0x32, 0x27, 0x9F, 0x05, 0xCB, 0x01, 0x72, 
      0xC7, 0x5C, 0x95, 0x8D, 0xF0, 0xB2, 0xFB, 0x43, 0x04, 0x9F, 0xF2, 0x22, 0x4C, 0xBF, 0xC9, 0xFF, 
      0x35, 0x96, 0x35, 0x29, 0xC7, 0x25, 0xCA, 0xFF, 0x35, 0x60, 0x2F, 0x0C, 0x52, 0xCD, 0x1C, 0x12, 
      0x31, 0xC5, 0xA2, 0xC5, 0x1C, 0xFC, 0x03, 0x2A, 0xAD, 0xC0, 0xFD, 0xFB, 0x03, 0x24, 0x17, 0x07, 
      0xCD, 0x79, 0xC4, 0x27, 0x45, 0x08, 0x0B, 0xD8, 0x25, 0x79, 0xE0, 0xFB, 0xB4, 0x4C, 0xBA, 0x2A, 
      0x01, 0xD2, 0x63, 0x79, 0x10, 0xD5, 0x9C, 0x75, 0x03, 0x4A, 0xC5, 0xA9, 0x2E, 0xE6, 0x8F, 0x2D, 
      0x4E, 0x40, 0x30, 0xF3, 0x6F, 0x3A, 0x8D, 0x4C, 0xAF, 0x96, 0x8F, 0x81, 0x9B, 0xEC, 0xCA, 0x84, 
      0xF0, 0x4B, 0xC6, 0xD1, 0x26, 0xBE, 0x1F, 0xFD, 0x30, 0x38, 0xC2, 0x16, 0x85, 0xF7, 0x8B, 0xAF, 
      0x72, 0x53, 0xC2, 0x2F, 0x2D, 0xDD, 0xA6, 0x8C, 0xCD, 0x8F, 0x9E, 0x23, 0x35, 0x09, 0x32, 0xD3, 
      0x2A, 0xF7, 0xA6, 0x1D, 0xE4, 0xF9, 0xE8, 0x31, 0x67, 0x2A, 0x42, 0x5A, 0xC2, 0xF3, 0xC9, 0x40, 
      0x1C, 0x7B, 0xB4, 0xE1, 0xF1, 0x47, 0xB2, 0xF2, 0x1F, 0x74, 0x10, 0x4D, 0xB1, 0xF3, 0x3F, 0xB0, 
      0xB2, 0x35, 0x68, 0xF3, 0xD3, 0xF0, 0xA1, 0x33, 0xDD, 0x39, 0x32, 0x08, 0xAB, 0xAF, 0x46, 0x36, 
      0x0B, 0xE5, 0xDD, 0xC1, 0xBB, 0x2E, 0x15, 0x13, 0x27, 0x6C, 0x03, 0x8C, 0x79, 0x6B, 0x44, 0xE4
    };
  }
}