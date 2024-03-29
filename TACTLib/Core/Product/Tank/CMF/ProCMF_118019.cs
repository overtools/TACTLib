using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_118019 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[length + 256];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 61;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(digest[7] + header.m_dataCount) & 511;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += 3;
                buffer[i] ^= digest[SignedMod(kidx - i, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x25, 0xB4, 0x52, 0x63, 0x43, 0xBB, 0x4A, 0xC3, 0xB0, 0x94, 0xC5, 0xCA, 0xC9, 0x7E, 0x78, 0x59, 
            0xD5, 0x2C, 0xBF, 0xCE, 0xA0, 0x4D, 0x1F, 0xE5, 0x4B, 0x75, 0x59, 0xE5, 0xD1, 0x93, 0x9B, 0xA6, 
            0x74, 0xE7, 0x39, 0xC6, 0x94, 0x36, 0x89, 0x73, 0xA5, 0x12, 0x3E, 0xE9, 0x85, 0x0D, 0x38, 0xF0, 
            0xEE, 0xE4, 0x07, 0xAD, 0x76, 0x77, 0x16, 0xE2, 0xAB, 0x26, 0x4B, 0x5E, 0x2D, 0x1D, 0xE0, 0xBD, 
            0xFB, 0x50, 0xDF, 0x71, 0x95, 0xD7, 0x7C, 0xF1, 0x3E, 0x22, 0x85, 0x29, 0xD5, 0x95, 0x9D, 0xCB, 
            0x17, 0x87, 0xAD, 0x5B, 0xFE, 0xF1, 0x44, 0x17, 0xD8, 0x6F, 0x01, 0x26, 0x3B, 0x8F, 0xF1, 0xE5, 
            0x13, 0xB3, 0x9B, 0xC8, 0xDB, 0x71, 0x6D, 0xB8, 0x25, 0xB1, 0xFC, 0x1F, 0xA8, 0xFA, 0x70, 0x6A, 
            0x38, 0x26, 0x92, 0xB3, 0x28, 0x13, 0x7F, 0x70, 0x12, 0xBE, 0x71, 0x1E, 0x81, 0x85, 0x39, 0xA0, 
            0xB9, 0x99, 0x1E, 0x1A, 0x5D, 0x11, 0xFD, 0x3F, 0xD3, 0x73, 0x54, 0xF6, 0x91, 0xA1, 0x48, 0xDA, 
            0xA3, 0x6A, 0x19, 0xF9, 0xC1, 0x01, 0xBC, 0x04, 0x62, 0x83, 0x04, 0x78, 0xA1, 0x85, 0x30, 0x9F, 
            0x28, 0xC1, 0x54, 0x66, 0x08, 0xF9, 0xB5, 0x18, 0x46, 0x72, 0x7F, 0x81, 0x2F, 0xF4, 0xAB, 0xCC, 
            0xA8, 0x56, 0x14, 0x4F, 0x4D, 0xD6, 0xF2, 0xF7, 0x75, 0xD3, 0xDA, 0x45, 0x4A, 0x76, 0xCE, 0x73, 
            0xE6, 0xCD, 0xC4, 0xA0, 0x99, 0x85, 0xC3, 0xEA, 0x9A, 0xD0, 0x8C, 0x37, 0x08, 0xF8, 0x3C, 0xE9, 
            0x83, 0x69, 0x11, 0xE8, 0x7F, 0xDB, 0xB0, 0x01, 0xBF, 0xE0, 0x83, 0xE2, 0x8F, 0xDF, 0x08, 0xF6, 
            0x4C, 0x04, 0xAA, 0xE6, 0xB9, 0x00, 0x33, 0x3F, 0x23, 0x9A, 0xB8, 0x43, 0x52, 0x5D, 0xB9, 0xFC, 
            0xAF, 0x37, 0x2F, 0x6B, 0xAB, 0xDD, 0xC1, 0xAF, 0x16, 0xDE, 0xFA, 0x99, 0x85, 0x35, 0xCF, 0x16, 
            0x76, 0x01, 0x6D, 0x64, 0x7F, 0x11, 0x0F, 0xEA, 0x91, 0x5E, 0x9F, 0x44, 0xE9, 0xFA, 0xA2, 0xB2, 
            0x02, 0x82, 0x04, 0x63, 0xD4, 0x10, 0xAA, 0xE6, 0xA2, 0x69, 0xDF, 0x67, 0x5F, 0xB7, 0xA3, 0xA8, 
            0x45, 0x10, 0xD3, 0xE2, 0x99, 0xC4, 0x80, 0x12, 0xD6, 0xC5, 0xCF, 0xAE, 0x72, 0xD0, 0x54, 0x22, 
            0x01, 0x41, 0x37, 0x62, 0xCD, 0x1C, 0xA5, 0xD6, 0x2F, 0x57, 0x27, 0x8C, 0x6B, 0xE2, 0xFF, 0x10, 
            0x2B, 0xE5, 0x47, 0xB7, 0xA3, 0x2A, 0x36, 0xF3, 0xBE, 0x0E, 0x97, 0x16, 0xCD, 0xE9, 0x71, 0xAD, 
            0x63, 0x67, 0x28, 0x1E, 0x2C, 0xD2, 0x65, 0xE9, 0x3A, 0xD2, 0x67, 0x31, 0x1A, 0xA1, 0x51, 0x18, 
            0x9A, 0x34, 0xDB, 0x99, 0x1A, 0x99, 0x91, 0xE6, 0x49, 0xC0, 0xEE, 0xD1, 0x85, 0xDD, 0x20, 0x86, 
            0xCD, 0x22, 0xB9, 0xCC, 0x95, 0xCF, 0xAF, 0x1E, 0xA1, 0x2C, 0x84, 0xC5, 0xD0, 0xA3, 0x38, 0x62, 
            0x6A, 0x0C, 0x8B, 0x24, 0xD0, 0x71, 0x68, 0x6B, 0x7C, 0x80, 0x85, 0x16, 0xBC, 0x1D, 0x44, 0x26, 
            0x16, 0x8A, 0x22, 0x4D, 0x0D, 0x62, 0x0A, 0x55, 0x23, 0xEF, 0xBA, 0xBE, 0xF2, 0x6C, 0xA6, 0xBF, 
            0x76, 0xAA, 0xBC, 0x34, 0xCA, 0x83, 0x82, 0x01, 0xA6, 0xD8, 0x4C, 0x90, 0x63, 0xF2, 0xEA, 0x9C, 
            0xE3, 0x0C, 0x93, 0x35, 0xDD, 0xD4, 0xA9, 0xCC, 0xF1, 0xE4, 0xDE, 0xB3, 0x4B, 0x36, 0x0F, 0x27, 
            0xB9, 0xFA, 0x33, 0x85, 0xC5, 0xA8, 0x41, 0x87, 0x67, 0xBE, 0x72, 0x1A, 0xD5, 0xB9, 0x9C, 0x5D, 
            0x8F, 0x1D, 0x9E, 0x63, 0x59, 0x0F, 0xD1, 0x6E, 0x2C, 0x35, 0xB2, 0x81, 0x82, 0x82, 0x40, 0x4F, 
            0x9A, 0x3B, 0x06, 0x6F, 0x72, 0x03, 0x7C, 0xE8, 0xC8, 0x24, 0x2F, 0x27, 0x3C, 0xA5, 0xAA, 0x9A, 
            0x86, 0xBC, 0x8A, 0x78, 0xCD, 0xC5, 0x72, 0x96, 0x16, 0xCB, 0x16, 0x15, 0xAD, 0xB4, 0xF8, 0x73
        };
    }
}