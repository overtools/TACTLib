using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_60993 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx = (uint)length;
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
            uint kidx = Keytable[(uint)header.m_dataCount & 511];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += (uint)header.m_entryCount + digest[SignedMod(header.m_entryCount, SHA1_DIGESTSIZE)];
                buffer[i] ^= digest[SignedMod(header.m_buildVersion + i, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x6C, 0xC3, 0x9E, 0xC8, 0x2C, 0xD9, 0xFB, 0x34, 0x4E, 0xF2, 0xBE, 0xAB, 0xA9, 0xBD, 0x44, 0x22,
            0x9E, 0x6E, 0x71, 0xC9, 0x9B, 0xB9, 0x33, 0x1D, 0x8D, 0xE4, 0xDE, 0x32, 0x43, 0xBA, 0xAC, 0x23,
            0xEE, 0x45, 0x14, 0xDC, 0x6F, 0x55, 0x8A, 0xDE, 0xA5, 0x3B, 0xDB, 0x34, 0x1E, 0xE6, 0xF9, 0x1E,
            0x67, 0xAD, 0x5A, 0x33, 0x27, 0x16, 0xF4, 0xE1, 0x28, 0xDD, 0x88, 0x4F, 0x88, 0xC5, 0x7D, 0x15,
            0x56, 0x86, 0xFA, 0x30, 0x7D, 0x73, 0xB2, 0xFD, 0xAE, 0xE3, 0xB9, 0xAC, 0x50, 0x78, 0x61, 0x57,
            0x25, 0xC3, 0x69, 0xCF, 0x73, 0xC3, 0x58, 0xAA, 0xB9, 0xCE, 0xCB, 0x22, 0x68, 0x1F, 0x3F, 0x83,
            0x00, 0x44, 0xE2, 0xA9, 0xDA, 0xC5, 0x55, 0x45, 0xDC, 0xC0, 0xEA, 0x37, 0x49, 0x3D, 0x86, 0xAE,
            0x9D, 0x7B, 0xFD, 0xF1, 0xBD, 0x68, 0xDF, 0x89, 0xA2, 0x3D, 0x22, 0xFA, 0x6F, 0xFE, 0x14, 0x8E,
            0x0E, 0xD4, 0xD1, 0x85, 0x61, 0x6F, 0xAC, 0x46, 0x76, 0x0A, 0x95, 0xBA, 0xEC, 0xC1, 0xC7, 0x24,
            0xBC, 0x73, 0x0A, 0xA0, 0xDA, 0xBC, 0x9F, 0x79, 0xCA, 0x37, 0x87, 0xEE, 0xDF, 0x95, 0xDA, 0xDA,
            0x7A, 0x32, 0x80, 0x1A, 0x45, 0x74, 0xD5, 0xDE, 0xF6, 0x88, 0x13, 0x05, 0x70, 0x36, 0x7C, 0xBA,
            0x28, 0xE5, 0xEA, 0x52, 0xCC, 0xB1, 0xC6, 0x35, 0x6B, 0xC3, 0x41, 0x3F, 0xCB, 0x5B, 0xAA, 0x96,
            0xA1, 0x67, 0x5A, 0xA1, 0xA8, 0xE1, 0x8B, 0xF6, 0x96, 0xB4, 0x74, 0x86, 0x1E, 0xF0, 0x29, 0xE9,
            0x25, 0x63, 0xB4, 0xEB, 0xFD, 0xA0, 0x1D, 0x06, 0x66, 0x65, 0xDE, 0x7D, 0xA0, 0x50, 0x3B, 0x3B,
            0xD9, 0xEE, 0xC8, 0xE6, 0x43, 0xA7, 0x09, 0x60, 0xEB, 0x86, 0x55, 0xAE, 0xDE, 0x61, 0x0D, 0xFD,
            0x44, 0x40, 0x3B, 0x82, 0xFA, 0xDB, 0x1F, 0x57, 0xE4, 0x64, 0xCC, 0x00, 0x71, 0xB0, 0x41, 0xCC,
            0x10, 0x02, 0x12, 0x8F, 0x96, 0xC8, 0x04, 0x30, 0x20, 0xBD, 0x27, 0x16, 0xCC, 0x4E, 0xD3, 0x98,
            0x16, 0xF5, 0xBA, 0xEE, 0x3C, 0x23, 0x25, 0x8C, 0xAC, 0xE9, 0x36, 0xA7, 0xEA, 0x5C, 0x1A, 0xA5,
            0x3F, 0x40, 0x5C, 0x5A, 0x7C, 0xE6, 0x9E, 0x1A, 0x40, 0xE5, 0x44, 0x05, 0xC7, 0x6F, 0xED, 0x57,
            0x67, 0xBF, 0x66, 0x0F, 0xE3, 0x2A, 0xF1, 0xD7, 0x75, 0xFA, 0xD1, 0xF5, 0x65, 0x06, 0x16, 0xB6,
            0x61, 0x8D, 0x9F, 0xF7, 0x5A, 0x51, 0xD2, 0x6C, 0xC3, 0xCF, 0x9E, 0xA6, 0xB6, 0xED, 0xBE, 0xDF,
            0x34, 0xC3, 0xEE, 0xC8, 0x7F, 0x66, 0x7D, 0xA3, 0xFC, 0xCC, 0xE6, 0x8F, 0xC8, 0xB3, 0x88, 0xE2,
            0x8D, 0x28, 0x4D, 0x80, 0x54, 0x2B, 0x47, 0xC5, 0x91, 0x6F, 0x98, 0x35, 0x44, 0x16, 0x61, 0xA8,
            0xF4, 0x5C, 0x7C, 0x41, 0x69, 0x50, 0xC8, 0x3F, 0xD1, 0x55, 0x02, 0xA6, 0x12, 0x9B, 0x3A, 0x44,
            0xFB, 0xF4, 0x45, 0x31, 0x27, 0x20, 0x37, 0x1C, 0x4D, 0x8A, 0x61, 0xDB, 0xE5, 0x98, 0x71, 0x9A,
            0x43, 0xFC, 0x93, 0x93, 0x7E, 0xF4, 0x8A, 0x31, 0x1B, 0xDF, 0x4A, 0xE3, 0x6A, 0x00, 0x2C, 0x96,
            0x1F, 0x69, 0xE8, 0xE0, 0x94, 0x74, 0x7D, 0xC5, 0x07, 0xC1, 0x2F, 0x86, 0x4D, 0x44, 0xC8, 0x81,
            0x8D, 0xFC, 0xED, 0xFC, 0xAB, 0xAB, 0xF9, 0x75, 0x1F, 0x8D, 0x51, 0xE7, 0xE0, 0xB4, 0x78, 0x28,
            0x9F, 0x78, 0xBE, 0xAC, 0x3A, 0xD2, 0x72, 0x14, 0x73, 0x7C, 0x50, 0xAF, 0x19, 0xCB, 0xC7, 0x77,
            0x7F, 0x66, 0xFE, 0x48, 0x39, 0x8A, 0x8A, 0x4A, 0x4B, 0x1E, 0x90, 0x9B, 0xC2, 0x38, 0x56, 0x8D,
            0xFA, 0x01, 0xF2, 0xC9, 0xE4, 0x5D, 0x8C, 0xAD, 0x03, 0x62, 0x98, 0xD5, 0x7E, 0xF4, 0x90, 0x32,
            0x09, 0x32, 0xFD, 0xC2, 0xEF, 0xA4, 0x0A, 0x77, 0x6D, 0x27, 0x15, 0xB1, 0x36, 0x1A, 0x9F, 0x73
        };
    }
}