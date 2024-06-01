using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_125687 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[(length * Keytable[0]) & 511];
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
                kidx += ((uint)header.m_dataCount + digest[SignedMod(header.m_dataCount, SHA1_DIGESTSIZE)]) % 17;
                buffer[i] = digest[SignedMod(kidx, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x58, 0xA0, 0x8E, 0x4B, 0x65, 0x11, 0x5A, 0xB8, 0xB7, 0x6A, 0x2D, 0x82, 0x31, 0xC7, 0xBD, 0xF1, 
            0x00, 0x52, 0x93, 0x9F, 0xC7, 0xE5, 0x0D, 0x1B, 0xD4, 0xE3, 0x56, 0xB8, 0x30, 0xA1, 0x75, 0x16, 
            0x31, 0xDE, 0xB7, 0xBB, 0xAA, 0x84, 0x40, 0xFF, 0x73, 0x5D, 0xA5, 0xD6, 0xD2, 0x1D, 0xD0, 0x2C, 
            0x60, 0x61, 0xFC, 0xF9, 0xA0, 0xD8, 0x91, 0xCD, 0x87, 0xCA, 0x4D, 0xF3, 0xCA, 0xA6, 0x79, 0x0B, 
            0x95, 0xEB, 0x6E, 0x1A, 0xA2, 0x91, 0xDE, 0xF6, 0x2D, 0x91, 0xBC, 0x1B, 0x6A, 0x0C, 0x02, 0xF8, 
            0x5F, 0x24, 0x8B, 0x8D, 0x9D, 0x85, 0x69, 0x4C, 0x15, 0xD3, 0x1F, 0xDB, 0x97, 0x9C, 0x3E, 0x89, 
            0x1F, 0xEF, 0x98, 0xED, 0x1C, 0x36, 0x66, 0xEC, 0x70, 0x20, 0x02, 0x5A, 0x99, 0x4D, 0x80, 0xCF, 
            0x88, 0x2D, 0x67, 0xB7, 0xDA, 0x8A, 0x65, 0x9E, 0x59, 0x0C, 0xC9, 0xE9, 0x8C, 0xFC, 0x63, 0x8E, 
            0xD8, 0x8F, 0x1B, 0x40, 0x66, 0x50, 0x94, 0x64, 0x12, 0xF1, 0x46, 0x9D, 0x1C, 0x38, 0xAB, 0x6C, 
            0x13, 0xE8, 0xA1, 0x78, 0x9C, 0x6B, 0x73, 0x23, 0xAE, 0x13, 0x75, 0xF7, 0x8D, 0x72, 0x08, 0x87, 
            0xF3, 0xF1, 0xDE, 0x16, 0x1E, 0x4E, 0x73, 0xC5, 0xE2, 0x86, 0x57, 0xB4, 0x88, 0x18, 0xBE, 0xA9, 
            0x72, 0xE9, 0x48, 0xF4, 0xCA, 0x4D, 0x87, 0x8A, 0x47, 0xBE, 0x79, 0xAB, 0xBB, 0x8F, 0x2B, 0x4E, 
            0x65, 0x27, 0x7D, 0x69, 0x7F, 0x28, 0xC0, 0x83, 0xEC, 0x9D, 0x7D, 0x56, 0x3D, 0x91, 0x25, 0xBD, 
            0xC3, 0x01, 0x6D, 0x66, 0x76, 0x0D, 0xA4, 0x76, 0xD6, 0x7B, 0xC0, 0xF1, 0x5A, 0x40, 0x43, 0x1A, 
            0x7D, 0x4F, 0x6A, 0x9D, 0xC2, 0x51, 0x9C, 0xDA, 0x72, 0x81, 0xE6, 0x25, 0x22, 0x51, 0xA3, 0xE0, 
            0xFE, 0xA3, 0x61, 0x70, 0x69, 0xAE, 0xC8, 0xF3, 0x56, 0xA9, 0x3A, 0x49, 0x67, 0xE2, 0xD2, 0x24, 
            0x0D, 0x1F, 0xF5, 0xE5, 0xC6, 0x51, 0x4F, 0xE3, 0x59, 0xEF, 0x32, 0x10, 0xBD, 0x77, 0x7E, 0x1A, 
            0xAB, 0x06, 0x57, 0xC8, 0x27, 0xFF, 0xE7, 0x45, 0x9B, 0x37, 0xBC, 0x00, 0xAA, 0xA0, 0x96, 0x35, 
            0xD0, 0xA5, 0x01, 0x17, 0x5A, 0x40, 0x31, 0x54, 0x20, 0x79, 0xB4, 0x8C, 0x5F, 0xBE, 0xB7, 0xEC, 
            0x53, 0x09, 0xAD, 0xDD, 0x7F, 0xFB, 0x45, 0x74, 0x6E, 0x3E, 0xF5, 0xE6, 0x4F, 0x8B, 0x2C, 0xE4, 
            0x4D, 0xD2, 0xEF, 0x34, 0x73, 0x8B, 0x28, 0xA8, 0x8E, 0xD3, 0xCB, 0x27, 0x8C, 0xA5, 0xE6, 0x76, 
            0x68, 0x1A, 0x77, 0x84, 0x80, 0xDC, 0xA9, 0xD9, 0x85, 0xBF, 0xA0, 0xF5, 0xF5, 0x3C, 0xD1, 0x0D, 
            0x70, 0xBB, 0xA2, 0x39, 0x53, 0xB8, 0x5F, 0xF8, 0x16, 0x48, 0xC2, 0xF4, 0x06, 0x86, 0x91, 0x0F, 
            0x6B, 0xF6, 0x39, 0x73, 0x51, 0x92, 0x50, 0x0B, 0xF5, 0xAF, 0xD3, 0x25, 0x27, 0x2B, 0x13, 0x4A, 
            0x8E, 0xD2, 0x64, 0x1B, 0x71, 0x1B, 0x72, 0xAA, 0x99, 0xD7, 0xAA, 0x59, 0xC9, 0x45, 0x0A, 0xB5, 
            0x05, 0x7F, 0x64, 0xED, 0xB0, 0xC3, 0xA2, 0x5B, 0xD3, 0x85, 0x0E, 0xC7, 0x55, 0x78, 0x7E, 0x6A, 
            0x49, 0xCC, 0x96, 0x3C, 0x25, 0x90, 0x10, 0x2B, 0xC0, 0xC2, 0xC8, 0x25, 0x60, 0xA9, 0x26, 0xC2, 
            0xE7, 0x4E, 0xA4, 0xFE, 0xB8, 0xEA, 0xBB, 0x65, 0x41, 0x80, 0x6A, 0xE9, 0x54, 0xFA, 0xA9, 0x08, 
            0x8E, 0xD4, 0x68, 0xD2, 0x84, 0xC6, 0xAA, 0x5F, 0x4C, 0x1E, 0x43, 0xCB, 0x5A, 0x9F, 0x24, 0x24, 
            0x1B, 0xC6, 0x34, 0xBF, 0x07, 0x61, 0x2C, 0x4C, 0xD4, 0x11, 0x88, 0xAC, 0xA2, 0xE3, 0xEF, 0xB8, 
            0x53, 0xD1, 0xEA, 0x14, 0xBA, 0x60, 0xAB, 0x30, 0x0F, 0xF2, 0xDE, 0x46, 0xF2, 0x99, 0x46, 0x98, 
            0x02, 0x3B, 0x78, 0x7A, 0x78, 0xDE, 0x7A, 0x17, 0xDD, 0xCE, 0x3D, 0xBF, 0x1E, 0x94, 0xB2, 0x2B
        };
    }
}