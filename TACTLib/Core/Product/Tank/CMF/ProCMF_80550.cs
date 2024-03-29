using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_80550 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += (uint)header.m_dataCount;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[((2 * digest[13]) - length) & 511];
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
            0x87, 0xD3, 0x74, 0xCE, 0xEF, 0x52, 0x8A, 0x14, 0x22, 0x05, 0x10, 0xDB, 0xE8, 0xCC, 0xD5, 0x8F, 
            0xEE, 0xB5, 0x66, 0x7C, 0x01, 0x27, 0x57, 0xCD, 0x5E, 0x18, 0x57, 0xCA, 0x9E, 0x48, 0xD8, 0x9C, 
            0x63, 0x27, 0xE1, 0x7C, 0x81, 0x83, 0x4D, 0xF7, 0x0A, 0xF1, 0xB1, 0xC6, 0xDA, 0xB3, 0x0C, 0x79, 
            0xCE, 0x9A, 0x15, 0xBA, 0xDA, 0x2D, 0xA8, 0x8A, 0x3D, 0x9B, 0x48, 0x3A, 0x1F, 0x25, 0x86, 0x52, 
            0x9D, 0x45, 0x39, 0xF2, 0xB1, 0xC2, 0xB6, 0x04, 0x92, 0xCB, 0x46, 0x63, 0xB6, 0x7D, 0xE4, 0xA2, 
            0xCB, 0x8D, 0x63, 0x1C, 0x26, 0xE6, 0x4B, 0x44, 0x76, 0x5A, 0x8C, 0xD3, 0x1C, 0xA5, 0xB7, 0x21, 
            0x7A, 0x3E, 0xDF, 0x75, 0xCF, 0xE0, 0x70, 0x2A, 0xE0, 0xDE, 0x51, 0x4F, 0x20, 0x3A, 0x7E, 0x14, 
            0xC6, 0xB5, 0x3E, 0xD3, 0xA8, 0x19, 0xE8, 0xD8, 0xC3, 0x2E, 0xF6, 0x64, 0xA5, 0x17, 0xDF, 0x84, 
            0x54, 0x58, 0x1D, 0x2D, 0x86, 0x46, 0xF8, 0x64, 0xF0, 0xB9, 0xCA, 0x27, 0x75, 0xAB, 0x07, 0xC6, 
            0x1C, 0x86, 0xF3, 0x95, 0x48, 0x4C, 0x1E, 0x8E, 0x1A, 0xAA, 0xE9, 0x8F, 0x85, 0xE9, 0x8A, 0xA0, 
            0xAF, 0x3B, 0x97, 0x8C, 0x76, 0x13, 0x05, 0xD8, 0x35, 0x5B, 0xAC, 0x68, 0xF7, 0xA7, 0x8B, 0x6F, 
            0xFB, 0xDA, 0xFB, 0x46, 0x94, 0xB1, 0xC2, 0x9C, 0xCF, 0x27, 0x10, 0xB5, 0x1D, 0x54, 0x72, 0xBD, 
            0xE1, 0x77, 0xB7, 0xC5, 0x9B, 0x9A, 0xF2, 0x00, 0x7F, 0xF9, 0xAA, 0x67, 0xA9, 0x2E, 0x5E, 0xAD, 
            0x1E, 0xF2, 0x63, 0x97, 0x40, 0x5B, 0xB2, 0x68, 0x36, 0x25, 0xBC, 0xDB, 0xD7, 0x91, 0x48, 0x7E, 
            0xEF, 0x6A, 0x6F, 0x34, 0x4E, 0x7F, 0xD9, 0x69, 0x4A, 0xB2, 0xAA, 0xC2, 0xC2, 0x39, 0x0C, 0x94, 
            0x34, 0x62, 0x09, 0x46, 0x8F, 0x5A, 0x0B, 0x16, 0xA1, 0xF8, 0x2C, 0x71, 0x66, 0x78, 0xD1, 0x4A, 
            0xD4, 0x72, 0x47, 0x4B, 0x87, 0xAD, 0x64, 0x4E, 0xDC, 0x5A, 0x8F, 0x5C, 0x4B, 0x53, 0x98, 0x3C, 
            0x5C, 0x40, 0x96, 0xDE, 0xE4, 0x29, 0xFF, 0x55, 0x3D, 0x5A, 0x4B, 0xA7, 0x8A, 0x83, 0xD8, 0x6F, 
            0x64, 0x46, 0x5F, 0xD6, 0xD8, 0x2F, 0x60, 0x13, 0x10, 0x6B, 0x30, 0x6D, 0x83, 0x49, 0xD2, 0xD2, 
            0xB4, 0x41, 0xD2, 0x4D, 0xAD, 0x07, 0xE8, 0x1A, 0x78, 0x47, 0xFC, 0x0A, 0x6E, 0xA1, 0x28, 0x50, 
            0x03, 0x85, 0x85, 0x05, 0x32, 0x8E, 0x80, 0x3A, 0xC1, 0xD1, 0xB3, 0xDE, 0xF6, 0x02, 0x03, 0x68, 
            0xBC, 0xF7, 0xAD, 0x39, 0x6C, 0xDA, 0x0D, 0x43, 0xA4, 0x15, 0x8E, 0x19, 0x14, 0xBE, 0xD6, 0xB4, 
            0x81, 0xCA, 0x71, 0xDB, 0xDA, 0x9E, 0x62, 0xE5, 0x64, 0x28, 0xDA, 0x85, 0x15, 0xFA, 0xF7, 0xBE, 
            0x91, 0x86, 0x6A, 0x7F, 0xC6, 0x92, 0xFE, 0x8B, 0x5D, 0xE8, 0xF8, 0x87, 0x88, 0x1D, 0xE3, 0x2B, 
            0xC5, 0x79, 0x95, 0xDB, 0x62, 0x2B, 0xFD, 0x45, 0x6D, 0xB0, 0xB4, 0x92, 0x2C, 0x0D, 0xF8, 0x17, 
            0xBB, 0x0C, 0xCB, 0x87, 0x77, 0xC2, 0xB3, 0x84, 0x99, 0x4B, 0x90, 0x01, 0x14, 0xF2, 0x77, 0x9A, 
            0x4C, 0xCB, 0xC7, 0xA1, 0x5C, 0x3C, 0xEC, 0xE5, 0x43, 0x98, 0x6F, 0x5B, 0x6A, 0xE5, 0x48, 0x25, 
            0x90, 0x36, 0x0C, 0x44, 0xED, 0x34, 0x53, 0x5C, 0x1F, 0xC9, 0x36, 0xC7, 0x30, 0xE6, 0x81, 0x08, 
            0x72, 0x99, 0xCD, 0x76, 0xB3, 0x0E, 0x7D, 0xF8, 0xFB, 0x09, 0x89, 0x0A, 0x0A, 0xBC, 0xFD, 0xDE, 
            0x57, 0xE5, 0x15, 0x83, 0xB5, 0x49, 0x4E, 0x17, 0x6C, 0x37, 0xC9, 0x5A, 0x6A, 0xB5, 0x17, 0x30, 
            0x3C, 0x3E, 0x15, 0x54, 0x82, 0x7E, 0xCB, 0x01, 0x95, 0x77, 0x66, 0xBE, 0xB5, 0x4B, 0x2A, 0xA4, 
            0xDE, 0xDA, 0x35, 0xA4, 0x35, 0x3F, 0x11, 0x23, 0x04, 0x41, 0xE7, 0x05, 0x0B, 0x53, 0x76, 0x85
        };
    }
}