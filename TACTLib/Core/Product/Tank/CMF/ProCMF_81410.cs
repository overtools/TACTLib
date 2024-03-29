using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_81410 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx;
            kidx = Keytable[header.m_buildVersion & 511];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += 3;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx;
            kidx = (uint)(length * header.m_buildVersion);
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
            0xB3, 0x59, 0x6F, 0x0B, 0xE5, 0xA9, 0x6C, 0x21, 0xCD, 0x91, 0x44, 0x6E, 0x60, 0xBB, 0x13, 0xA0,
            0x74, 0x25, 0xEC, 0x52, 0x04, 0xBD, 0xA3, 0x61, 0x8B, 0x95, 0x29, 0xFE, 0x91, 0x5C, 0x31, 0x2A,
            0xA8, 0xCC, 0xEA, 0x5E, 0x46, 0x5D, 0xD1, 0xD9, 0xE6, 0x77, 0x11, 0xEB, 0x7D, 0x94, 0xF3, 0x2C,
            0xD0, 0xA8, 0x66, 0x23, 0xEB, 0xC1, 0x96, 0x0F, 0xA2, 0xEB, 0x83, 0x21, 0x56, 0x30, 0x80, 0x8B,
            0xEB, 0xEB, 0x1F, 0xA5, 0xC9, 0x6D, 0x89, 0x19, 0xA1, 0xFC, 0x8A, 0x09, 0x24, 0xA5, 0x47, 0xC3,
            0x65, 0x30, 0x59, 0xB1, 0x30, 0x64, 0x02, 0xB8, 0xD9, 0x32, 0x2F, 0x6B, 0x56, 0x39, 0xB9, 0x08,
            0xF5, 0x8F, 0x90, 0xFC, 0x69, 0x1E, 0x55, 0x1A, 0x89, 0x19, 0x50, 0xAD, 0xD5, 0xC2, 0x3B, 0xFB,
            0xC1, 0xFF, 0xA7, 0xBF, 0xFF, 0x93, 0x95, 0x44, 0xE3, 0x82, 0x28, 0x8B, 0x86, 0x3D, 0x45, 0x31,
            0x71, 0xFC, 0xC2, 0x43, 0x5A, 0xA2, 0x3E, 0x30, 0x1B, 0xA1, 0x35, 0xA4, 0x6F, 0x62, 0x2A, 0x96,
            0x6D, 0xE8, 0xA0, 0x6D, 0x06, 0xF3, 0xB0, 0xA0, 0xFA, 0x53, 0x85, 0x95, 0x53, 0x84, 0xCE, 0x74,
            0x61, 0xA7, 0x0D, 0x29, 0x63, 0x90, 0x39, 0x59, 0x48, 0x32, 0x07, 0x57, 0x1A, 0x33, 0x73, 0x91,
            0x0B, 0x9F, 0x3D, 0x2C, 0xE0, 0xF2, 0x22, 0x78, 0x88, 0x6E, 0x36, 0x12, 0x4F, 0x4B, 0x4C, 0x7F,
            0x58, 0x0C, 0xAD, 0x3B, 0xB5, 0x57, 0x90, 0x76, 0x36, 0x1C, 0x1A, 0x09, 0xA7, 0x2A, 0xDD, 0xE7,
            0xE3, 0x2D, 0x97, 0xFC, 0xA0, 0x04, 0x99, 0xC1, 0xA6, 0xA6, 0xB2, 0x61, 0xF4, 0x67, 0xDF, 0x6E,
            0x02, 0x7B, 0xB1, 0xC8, 0xF9, 0x76, 0x72, 0x1D, 0xC9, 0xA7, 0xBA, 0x57, 0x45, 0xBC, 0x56, 0xFF,
            0x6B, 0xC8, 0x67, 0xFA, 0x26, 0x44, 0x72, 0x3A, 0x9D, 0xD9, 0x12, 0x36, 0xBC, 0xFB, 0x8D, 0x59,
            0x0F, 0x45, 0x88, 0x71, 0x23, 0x4B, 0x60, 0x88, 0x1F, 0x93, 0x80, 0xD7, 0x55, 0x2E, 0x0D, 0x81,
            0xC8, 0x56, 0x4A, 0x93, 0x1C, 0xE7, 0x85, 0x41, 0x36, 0x37, 0xC7, 0xD5, 0xB0, 0x9F, 0x1C, 0xF3,
            0x67, 0x0C, 0xD7, 0x3E, 0x18, 0x9F, 0xB9, 0xB9, 0x3E, 0x54, 0x03, 0x4D, 0x23, 0xE9, 0x19, 0x18,
            0xF9, 0x59, 0x8E, 0x42, 0x38, 0x89, 0xDC, 0xD7, 0x66, 0xB1, 0x2F, 0x23, 0x7D, 0x6C, 0x36, 0xA6,
            0x04, 0x19, 0x75, 0xB8, 0xD5, 0xB9, 0x45, 0x5D, 0x36, 0xA5, 0xA4, 0x80, 0x96, 0x27, 0xDB, 0x14,
            0xBC, 0xFF, 0x28, 0x6B, 0xE8, 0x6E, 0xD9, 0xC3, 0xCE, 0x85, 0x42, 0x0F, 0xEB, 0x05, 0x47, 0x0E,
            0xE7, 0x37, 0x76, 0x04, 0x69, 0xB5, 0x4C, 0xDC, 0xA1, 0x76, 0x3E, 0x31, 0x9F, 0x5D, 0xCB, 0x7F,
            0x51, 0xD9, 0x17, 0x22, 0xD3, 0x9E, 0xA0, 0x9A, 0xE6, 0x2B, 0xF1, 0x76, 0x85, 0xE4, 0x20, 0x7F,
            0xCC, 0xF0, 0x2D, 0x48, 0xB9, 0x50, 0x33, 0x93, 0x16, 0x58, 0xBF, 0x62, 0xBC, 0xF9, 0xBF, 0x04,
            0xDE, 0x53, 0xA9, 0x8F, 0xA9, 0x27, 0xFE, 0x00, 0x03, 0xDC, 0x43, 0xAA, 0x1D, 0xDF, 0x75, 0x6A,
            0x56, 0xB5, 0xC1, 0x46, 0xF5, 0x3D, 0x31, 0x2B, 0x97, 0x29, 0x64, 0x62, 0x14, 0xCF, 0xCE, 0x76,
            0xCC, 0xF8, 0x39, 0x2F, 0x07, 0x62, 0x9F, 0xCC, 0x4F, 0xC7, 0x13, 0x86, 0x60, 0x8F, 0x3C, 0xBE,
            0x6F, 0xEA, 0x69, 0xA9, 0x2A, 0xDD, 0x26, 0xAA, 0x9C, 0x1D, 0x6E, 0xCE, 0x86, 0x1D, 0x91, 0x16,
            0x8C, 0x50, 0xD4, 0x4F, 0x4F, 0x84, 0x7B, 0x5B, 0xE4, 0x34, 0x02, 0x41, 0x3B, 0xBE, 0x71, 0x0E,
            0x31, 0xC6, 0x9A, 0xC1, 0xCB, 0x4D, 0xDE, 0x1D, 0xC8, 0x3E, 0x5C, 0x9E, 0xBC, 0x21, 0x16, 0x06,
            0x38, 0xB1, 0x42, 0x70, 0xFC, 0x30, 0xBF, 0xF0, 0x02, 0xB5, 0x2C, 0x3E, 0xC8, 0x02, 0x21, 0x53
        };
    }
}