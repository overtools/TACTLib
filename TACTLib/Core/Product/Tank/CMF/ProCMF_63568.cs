using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_63568 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx = (uint)(header.m_buildVersion * length);
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += (uint)header.m_entryCount;
            }

            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx = Keytable[SignedMod((2 * digest[13]) - length, 512)];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx -= 43;
                buffer[i] ^= digest[SignedMod(kidx + header.m_dataCount, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x69, 0x3A, 0xD8, 0xEF, 0x4E, 0x52, 0xD6, 0xB3, 0x65, 0x70, 0x91, 0x4C, 0x14, 0x67, 0xAC, 0x74,
            0x57, 0x81, 0x0C, 0x43, 0xE4, 0xCA, 0xD3, 0x39, 0xFA, 0x6A, 0xE6, 0xC8, 0xDA, 0x49, 0xAC, 0x39,
            0x48, 0xFD, 0x52, 0x6D, 0x5A, 0x88, 0xD7, 0x24, 0x63, 0x9E, 0x78, 0x9D, 0x72, 0x26, 0x8A, 0xAB,
            0x29, 0x03, 0x9B, 0xDA, 0xF4, 0x96, 0xA9, 0x16, 0x04, 0x58, 0x7F, 0x65, 0x1D, 0x33, 0xA7, 0x1D,
            0xFC, 0x4B, 0x1C, 0x66, 0x31, 0x99, 0xE4, 0x18, 0x42, 0x01, 0x93, 0x18, 0x31, 0xFF, 0x5F, 0x88,
            0xD5, 0x72, 0x2A, 0x07, 0x40, 0x25, 0x1E, 0xB1, 0x12, 0x93, 0x37, 0xDC, 0xB0, 0xDA, 0xF2, 0x04,
            0x8E, 0x61, 0x14, 0xA6, 0x76, 0x42, 0xA7, 0xB6, 0x26, 0xE8, 0xBB, 0x32, 0x66, 0xC7, 0xDB, 0x39,
            0xA6, 0x57, 0x88, 0x66, 0xA9, 0x18, 0x87, 0x93, 0xE5, 0x27, 0x89, 0x13, 0x46, 0xCA, 0xF8, 0xDB,
            0xA6, 0x55, 0xF3, 0x28, 0x4C, 0x8B, 0x75, 0x50, 0x14, 0x8A, 0x65, 0x53, 0x4E, 0x32, 0x74, 0x7E,
            0x63, 0x71, 0xC3, 0xE2, 0x05, 0x8C, 0x9A, 0x1E, 0x73, 0xBB, 0x8F, 0xE4, 0x12, 0x7E, 0x4A, 0x1F,
            0x56, 0x40, 0xAC, 0x8A, 0x96, 0x05, 0x11, 0xD5, 0x02, 0x23, 0x7F, 0x39, 0x2E, 0xF7, 0x56, 0xBE,
            0xF9, 0xED, 0x0C, 0xA4, 0x61, 0xC7, 0xD1, 0x9E, 0x31, 0x70, 0x78, 0xA8, 0x0C, 0x8B, 0x35, 0x6C,
            0x77, 0x4F, 0xF7, 0x0F, 0xCD, 0xA7, 0x10, 0xAB, 0x3E, 0x97, 0xC8, 0xE1, 0x61, 0xC7, 0x8C, 0xBF,
            0xAF, 0x59, 0x48, 0x14, 0x9A, 0x1D, 0xA7, 0x49, 0xD6, 0x50, 0xD3, 0xA0, 0x63, 0xF4, 0x7A, 0x06,
            0x06, 0xF8, 0x98, 0xCA, 0x51, 0x40, 0xB4, 0x0A, 0x0C, 0x29, 0x56, 0x76, 0x09, 0x2D, 0x65, 0xF0,
            0x13, 0x61, 0x13, 0xA2, 0xC5, 0xB2, 0xED, 0xA2, 0x58, 0x6C, 0x58, 0x9C, 0xCF, 0xE9, 0x3C, 0xB3,
            0x9D, 0xFA, 0x36, 0x35, 0x6E, 0x06, 0x8F, 0x05, 0xE3, 0x2D, 0x95, 0x71, 0x31, 0x87, 0x12, 0xF1,
            0xB2, 0x7A, 0x33, 0x5B, 0xE2, 0xB7, 0x61, 0xDA, 0x90, 0xC3, 0x9B, 0x56, 0xCA, 0xF8, 0x0B, 0xE8,
            0x62, 0xD2, 0x46, 0x33, 0x1F, 0x7E, 0x9F, 0x10, 0x52, 0x75, 0x05, 0x88, 0xB1, 0xC0, 0x4D, 0x7A,
            0x07, 0x2A, 0x68, 0x94, 0x53, 0xA3, 0x37, 0x80, 0xC8, 0x8F, 0x53, 0xF8, 0xB8, 0x0F, 0x22, 0xD2,
            0xE9, 0x06, 0x41, 0x3E, 0x4F, 0x21, 0x23, 0x4D, 0x9E, 0x74, 0x0F, 0x38, 0x6A, 0xF4, 0x0B, 0x0F,
            0x79, 0x20, 0x01, 0xF2, 0x54, 0xB9, 0x42, 0x78, 0xB3, 0x77, 0xBD, 0x71, 0xB6, 0xA4, 0xD0, 0x01,
            0xB5, 0xFA, 0x4E, 0xEC, 0x8A, 0x51, 0x57, 0x24, 0xED, 0xC1, 0x23, 0x80, 0x6C, 0xAB, 0x0D, 0x85,
            0xCC, 0xD7, 0xE4, 0xE2, 0x40, 0x99, 0xEB, 0xB3, 0xCE, 0x58, 0x21, 0x5A, 0xC8, 0xD8, 0xC4, 0x47,
            0x69, 0xB4, 0xDC, 0x90, 0xAA, 0xEA, 0x83, 0xCD, 0xE8, 0xE9, 0x89, 0xAA, 0x80, 0x87, 0xCB, 0x5B,
            0xF4, 0x83, 0x3A, 0xF3, 0x07, 0x6F, 0x27, 0xCF, 0x4A, 0xEF, 0x67, 0x18, 0xAD, 0x00, 0xF3, 0xAD,
            0x36, 0xD7, 0x79, 0xB0, 0xE5, 0x85, 0x20, 0x6E, 0xAA, 0x6C, 0xB0, 0x5B, 0x20, 0xD9, 0xF0, 0xBA,
            0xC0, 0x59, 0x9F, 0xEC, 0x69, 0xD8, 0x0A, 0xB1, 0x01, 0xEF, 0xD3, 0x17, 0x55, 0xF5, 0x18, 0xC0,
            0x3F, 0xB1, 0x9F, 0xD1, 0xEE, 0xBB, 0x9A, 0x4E, 0x30, 0xB9, 0xF2, 0x6C, 0x13, 0xED, 0x3D, 0x47,
            0xF5, 0x04, 0xBE, 0xCB, 0xAF, 0x86, 0x16, 0xEC, 0x21, 0x2D, 0xAA, 0x56, 0xDB, 0x1E, 0xFC, 0x5D,
            0x72, 0xF9, 0xBB, 0xB3, 0x96, 0x9B, 0x67, 0x24, 0x1F, 0x49, 0xBD, 0x66, 0xFB, 0xEE, 0x60, 0xC3,
            0x10, 0xE3, 0x4D, 0x98, 0xA8, 0xBB, 0xB8, 0x20, 0x33, 0x03, 0x36, 0xE1, 0xF8, 0x59, 0x74, 0xD4
        };
    }
}