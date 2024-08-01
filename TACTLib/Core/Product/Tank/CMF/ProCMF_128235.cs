using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_128235 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx -= header.m_buildVersion & 511;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_dataCount & 511];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 29;
                buffer[i] ^= (byte)(digest[SignedMod(kidx + header.m_dataCount, SHA1_DIGESTSIZE)] + 1);
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xC7, 0xF1, 0x50, 0x74, 0x1D, 0x89, 0x82, 0xB4, 0xC4, 0xC3, 0xC2, 0xC7, 0x49, 0x1D, 0x4F, 0xE5, 
            0x33, 0x88, 0x0B, 0xF0, 0x3A, 0x91, 0x6A, 0xF7, 0xB6, 0x0B, 0x51, 0xFB, 0x4E, 0x08, 0x08, 0xBC, 
            0xA9, 0x8A, 0xC0, 0x65, 0x5B, 0x39, 0xC0, 0x4E, 0xF2, 0xCA, 0x57, 0x22, 0x72, 0xB9, 0xEC, 0x9C, 
            0x13, 0xE0, 0xBC, 0xE2, 0xD4, 0x8B, 0x7D, 0xF2, 0x6D, 0x26, 0x1B, 0x88, 0xBF, 0xF1, 0x0C, 0xB9, 
            0x60, 0x14, 0x99, 0x60, 0xA4, 0xD3, 0xD7, 0x99, 0x1A, 0x6D, 0x9E, 0x77, 0xC0, 0x53, 0x39, 0x86, 
            0xA2, 0x56, 0x39, 0xF1, 0xDD, 0xAC, 0x81, 0xE9, 0x47, 0xBF, 0xAB, 0x6A, 0x0F, 0x85, 0x3C, 0x35, 
            0x75, 0xAC, 0x33, 0x25, 0xC0, 0xE8, 0x03, 0x46, 0x66, 0x49, 0xB8, 0x79, 0x15, 0x09, 0xFB, 0xB0, 
            0xFE, 0x74, 0x56, 0x58, 0x72, 0x65, 0xCF, 0x43, 0x32, 0xF9, 0x7C, 0xAB, 0x49, 0xDC, 0x44, 0x06, 
            0x65, 0x52, 0xA4, 0x32, 0x75, 0x35, 0xF9, 0x11, 0x4F, 0xA5, 0x9D, 0xAB, 0x78, 0x17, 0xF5, 0x43, 
            0x1C, 0xC6, 0xA8, 0x31, 0x7F, 0x64, 0xA7, 0xFB, 0x67, 0x44, 0x92, 0xE6, 0xED, 0x08, 0x52, 0x6B, 
            0x09, 0x41, 0xA2, 0x43, 0x97, 0x2F, 0xB1, 0x9E, 0xD7, 0x4E, 0x48, 0xB0, 0x6C, 0x12, 0x9F, 0x0E, 
            0x04, 0x23, 0xB4, 0xF7, 0x0F, 0x9C, 0x63, 0xC3, 0x87, 0x28, 0x7B, 0xA5, 0x9B, 0x5F, 0x0C, 0x4D, 
            0xAA, 0xD7, 0xAB, 0xFE, 0xC2, 0x93, 0x3F, 0xF3, 0x23, 0xC2, 0x52, 0x36, 0x61, 0xC7, 0x97, 0xAF, 
            0x7F, 0x2E, 0x62, 0x04, 0x2B, 0xE6, 0x3C, 0xEA, 0x20, 0x0F, 0x6C, 0x53, 0xA0, 0x0F, 0xE9, 0xA6, 
            0x8D, 0x57, 0x42, 0x35, 0x1E, 0x48, 0xEF, 0xA7, 0x43, 0x1D, 0x4D, 0x18, 0x58, 0xE6, 0x2C, 0x49, 
            0xBE, 0x6A, 0x5E, 0xF6, 0x7F, 0x41, 0x0F, 0x83, 0x4D, 0x8A, 0xA2, 0x57, 0x7F, 0x48, 0x05, 0xA9, 
            0x31, 0xD3, 0x3F, 0xCD, 0x90, 0xFC, 0xA4, 0xED, 0x90, 0x08, 0xAA, 0xB2, 0x32, 0x7D, 0x23, 0x23, 
            0x5A, 0xED, 0x84, 0xE9, 0x64, 0xE9, 0x7F, 0x56, 0x19, 0x84, 0x5C, 0x26, 0x11, 0x51, 0xB5, 0x83, 
            0xF6, 0x26, 0x3D, 0xA8, 0xC3, 0x91, 0x98, 0x15, 0xE6, 0x0C, 0x8B, 0x5D, 0x54, 0xAF, 0x6B, 0x5F, 
            0xB4, 0xE8, 0xC9, 0xF7, 0x98, 0x61, 0x42, 0xBA, 0x26, 0xA1, 0xB1, 0xF2, 0x2A, 0x83, 0xAF, 0x21, 
            0xDC, 0x41, 0x71, 0x42, 0xFD, 0x42, 0x60, 0xC5, 0x89, 0x5B, 0x3F, 0x7F, 0xA2, 0x61, 0xB0, 0xFE, 
            0x5C, 0x2D, 0x80, 0x28, 0xC8, 0x87, 0xDB, 0xD7, 0x6F, 0xB1, 0x53, 0x9D, 0x84, 0xF2, 0x86, 0x30, 
            0x1D, 0x24, 0x68, 0x99, 0x2A, 0xDC, 0x36, 0x6F, 0xEE, 0x00, 0xA3, 0x0B, 0xEF, 0x16, 0x6F, 0x29, 
            0x15, 0x00, 0x46, 0x4C, 0x92, 0x57, 0x6C, 0xA8, 0x49, 0xEE, 0xAD, 0xF9, 0xB4, 0x84, 0x3A, 0x83, 
            0x88, 0x42, 0xE8, 0x45, 0x2C, 0x19, 0x64, 0xE4, 0x33, 0xD7, 0x26, 0xE5, 0x9D, 0x89, 0xB4, 0xAE, 
            0x24, 0x26, 0x12, 0x74, 0x92, 0xFF, 0xB2, 0x58, 0x93, 0x95, 0x88, 0x36, 0x7F, 0xD3, 0x73, 0x60, 
            0x36, 0x53, 0xC0, 0x0D, 0xD9, 0x90, 0xE1, 0x04, 0xB2, 0xE2, 0x91, 0xFC, 0x98, 0xA9, 0x35, 0x36, 
            0x43, 0x66, 0x36, 0x7B, 0x29, 0x5B, 0x9C, 0x3D, 0x45, 0x8A, 0x5F, 0xD4, 0x8E, 0xB9, 0x2C, 0x53, 
            0xF5, 0xFA, 0x10, 0x2D, 0x25, 0xFD, 0x29, 0x13, 0xE4, 0xE9, 0x86, 0xEC, 0x14, 0x80, 0xFC, 0x49, 
            0x19, 0x46, 0x61, 0xF4, 0xF0, 0x8E, 0xAC, 0xD5, 0xBA, 0x42, 0xD5, 0x79, 0xA4, 0xC5, 0xD7, 0x15, 
            0x1C, 0x02, 0xCD, 0x54, 0x48, 0xCC, 0x1D, 0x75, 0xAF, 0xBE, 0xA6, 0xA3, 0xF3, 0xE3, 0x96, 0xB5, 
            0x18, 0x7C, 0xC8, 0x20, 0x20, 0x69, 0x3E, 0x86, 0x3D, 0x9C, 0x14, 0x83, 0xE6, 0x28, 0x3A, 0xB5
        };
    }
}