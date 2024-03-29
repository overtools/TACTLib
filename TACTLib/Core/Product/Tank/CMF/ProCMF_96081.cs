using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_96081 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_dataCount & 511];
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
                kidx += okidx % 13;
                buffer[i] ^= digest[SignedMod(kidx - 73, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xDC, 0xBB, 0x3F, 0xBE, 0x48, 0x7B, 0xE4, 0x59, 0x31, 0xA9, 0x5F, 0x0D, 0x30, 0x9F, 0xD8, 0x78, 
            0xF4, 0x15, 0xAC, 0x71, 0x9C, 0x91, 0x5F, 0xCB, 0xD0, 0x41, 0xD0, 0x4B, 0x9D, 0xDB, 0x2F, 0xAC, 
            0x9A, 0x47, 0x75, 0x8D, 0xEE, 0xF9, 0x7D, 0x8E, 0xC4, 0xCC, 0x64, 0x7E, 0x51, 0xCB, 0x96, 0xC5, 
            0x24, 0x89, 0x39, 0xEA, 0x94, 0x3A, 0x73, 0x4F, 0x17, 0xD5, 0x2C, 0x65, 0xD2, 0xE5, 0x70, 0xA6, 
            0x71, 0x8C, 0xD8, 0xB4, 0x62, 0xD9, 0xB4, 0x3B, 0x2F, 0x33, 0x7E, 0x23, 0x54, 0x7D, 0xBA, 0x11, 
            0x59, 0xA8, 0x07, 0x94, 0x47, 0x7F, 0x2D, 0x98, 0xB8, 0xBE, 0x96, 0x34, 0x65, 0x53, 0x2D, 0x1D, 
            0x6D, 0x01, 0x74, 0xBA, 0xD8, 0x76, 0xA8, 0x1F, 0x66, 0x12, 0xF4, 0x05, 0x1E, 0x75, 0x5C, 0x27, 
            0x0E, 0xCD, 0xA2, 0x0F, 0xD9, 0x57, 0x33, 0x7F, 0xAD, 0xE3, 0xF7, 0x2B, 0x0F, 0x79, 0x67, 0x22, 
            0x5D, 0x62, 0x6D, 0x07, 0x74, 0xE3, 0xE3, 0x77, 0x78, 0x15, 0x81, 0x72, 0x00, 0x4D, 0x50, 0xD4, 
            0x9F, 0xEB, 0x2D, 0xC6, 0xFE, 0x42, 0xCD, 0x77, 0x04, 0x7A, 0x11, 0x7A, 0x77, 0xB1, 0x85, 0x1D, 
            0xD8, 0x8A, 0x7B, 0xE9, 0x00, 0x24, 0x08, 0x8C, 0xD2, 0x52, 0x56, 0xD7, 0x71, 0x30, 0xB6, 0x77, 
            0xDF, 0x56, 0x0B, 0x26, 0xFA, 0x51, 0x87, 0x47, 0x2A, 0x6E, 0xFE, 0xFD, 0x6A, 0x95, 0x39, 0x5C, 
            0x67, 0x17, 0x74, 0xB8, 0xE0, 0x62, 0xCF, 0x04, 0xD0, 0x8B, 0x78, 0xA0, 0xC6, 0x43, 0x2D, 0x6F, 
            0x76, 0x0B, 0x7B, 0x42, 0x04, 0xD2, 0xE3, 0x80, 0x60, 0x13, 0xE0, 0xA3, 0x57, 0x9B, 0xE3, 0x81, 
            0xC8, 0x56, 0x40, 0x45, 0xCE, 0x42, 0xAA, 0x35, 0x31, 0xC8, 0x8E, 0x9E, 0x31, 0x8F, 0xA4, 0x26, 
            0x2B, 0x04, 0xF3, 0xD2, 0x79, 0xB2, 0xC5, 0xE8, 0xB1, 0xDF, 0xF3, 0x73, 0x20, 0x33, 0x58, 0x04, 
            0x41, 0x0A, 0xD9, 0xC8, 0xBA, 0x34, 0xD8, 0x8E, 0x27, 0x3B, 0xF6, 0xB3, 0xB5, 0x4D, 0x2A, 0x0B, 
            0xD3, 0xEB, 0x6E, 0x61, 0xC4, 0xAF, 0x78, 0x0C, 0x11, 0xE0, 0x0E, 0xF7, 0xB0, 0x9A, 0xFE, 0xD5, 
            0xBE, 0x42, 0xC9, 0x88, 0xE1, 0x01, 0x26, 0xD6, 0xF3, 0x0F, 0x09, 0x38, 0xAC, 0x64, 0xEA, 0xA7, 
            0xD4, 0xAB, 0x8D, 0xAB, 0xBC, 0xBC, 0x5A, 0xB4, 0xEA, 0xF1, 0x81, 0x51, 0x2C, 0x4C, 0x59, 0x73, 
            0x99, 0xCD, 0x86, 0x60, 0xE6, 0xBC, 0x71, 0x74, 0x3D, 0xE9, 0x4A, 0x67, 0x41, 0x99, 0x28, 0xA8, 
            0x0C, 0xE0, 0xB9, 0xAD, 0x28, 0x0E, 0x78, 0x5E, 0xFE, 0x71, 0x66, 0x3C, 0xA8, 0x0F, 0xFB, 0x42, 
            0xA4, 0xBC, 0x96, 0x7C, 0x1E, 0x7A, 0x61, 0x27, 0xF3, 0xB2, 0xB3, 0xEC, 0xFB, 0xE3, 0xBE, 0x9B, 
            0xD2, 0xBB, 0x6C, 0xB1, 0x85, 0x40, 0xE1, 0x45, 0x58, 0x01, 0x94, 0x7A, 0x21, 0xAD, 0x79, 0xC2, 
            0xA1, 0xCA, 0xD9, 0x5C, 0x0C, 0x6C, 0xFD, 0xE4, 0xBE, 0xBC, 0xC8, 0x81, 0x93, 0x8F, 0x49, 0x99, 
            0xD8, 0xDC, 0x1B, 0xF5, 0x27, 0xD0, 0xF3, 0xD1, 0xDA, 0x0E, 0x70, 0x1E, 0xBF, 0x7E, 0xFB, 0xEF, 
            0x02, 0xDA, 0x9E, 0x07, 0xE2, 0x12, 0x92, 0xDB, 0xC0, 0x85, 0xD4, 0xBF, 0xD6, 0xB7, 0x59, 0x6F, 
            0x07, 0xEB, 0x42, 0x7D, 0x4D, 0x7B, 0xE4, 0x48, 0xF7, 0xA7, 0x4E, 0x86, 0x2F, 0xE4, 0x36, 0x9B, 
            0xE3, 0x59, 0x63, 0xB7, 0x3F, 0x80, 0xF0, 0xE6, 0x0B, 0xAB, 0x89, 0xDD, 0x34, 0x11, 0x2D, 0x6F, 
            0x74, 0x39, 0xC6, 0xCA, 0x1F, 0x3B, 0x6E, 0x0F, 0x9E, 0xFC, 0x86, 0x50, 0xAB, 0x12, 0x8C, 0x7B, 
            0xA6, 0xBD, 0xAE, 0xD4, 0xBC, 0xFC, 0xFC, 0x80, 0x5A, 0x6A, 0xC0, 0x1B, 0xD1, 0x5D, 0x5E, 0x82, 
            0xAC, 0xCC, 0x67, 0xDD, 0xFA, 0x1C, 0xBD, 0x93, 0xDC, 0x3C, 0x82, 0x40, 0xC7, 0x1C, 0xD4, 0xC3
        };
    }
}