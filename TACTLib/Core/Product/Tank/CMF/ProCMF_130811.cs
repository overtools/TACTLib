using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_130811 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_dataCount & 511];
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
                kidx -= 43;
                buffer[i] ^= digest[SignedMod(kidx + header.m_dataCount, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x6B, 0xA5, 0x33, 0x14, 0x35, 0x5E, 0xC8, 0x79, 0x64, 0x32, 0x8A, 0xB7, 0x7E, 0xEC, 0x8D, 0x63, 
            0x0F, 0x04, 0x36, 0xBC, 0x67, 0x44, 0x00, 0x1F, 0x4B, 0x2B, 0x19, 0x15, 0xC4, 0x9A, 0xF6, 0x8E, 
            0xFB, 0xF1, 0x79, 0xB3, 0xC5, 0xC0, 0xC9, 0xB4, 0xF6, 0x3D, 0xCF, 0x01, 0x5C, 0x0C, 0x07, 0x13, 
            0x8E, 0x34, 0x9B, 0xC0, 0xBE, 0x85, 0x09, 0xB9, 0x5C, 0x83, 0x69, 0x86, 0xDF, 0xE5, 0x25, 0x3A, 
            0xD0, 0x3C, 0x2D, 0xF0, 0x64, 0x4E, 0xE9, 0x7D, 0x0B, 0x7F, 0x5C, 0xDD, 0x8A, 0xBC, 0x1E, 0xFC, 
            0x96, 0xA9, 0xDC, 0xA3, 0x78, 0xED, 0x1A, 0x88, 0xFC, 0xAE, 0x06, 0x03, 0x86, 0x47, 0x23, 0xC2, 
            0xA5, 0x61, 0xDA, 0xFB, 0x79, 0x1F, 0x0D, 0x2F, 0x14, 0xC0, 0x58, 0x43, 0x3B, 0xB2, 0xCF, 0x64, 
            0x54, 0x47, 0x28, 0xBE, 0xA4, 0x1E, 0x6B, 0xE8, 0x3C, 0xFD, 0x05, 0x6E, 0x8C, 0x2F, 0x3D, 0x10, 
            0x10, 0xC3, 0x6C, 0xA2, 0x42, 0x90, 0x8A, 0x06, 0xA2, 0x5C, 0x3D, 0xCF, 0xC4, 0xFB, 0x23, 0xF6, 
            0xC7, 0xB1, 0x94, 0x50, 0xA5, 0xDC, 0xB6, 0x85, 0xC7, 0x18, 0x9C, 0xAB, 0x08, 0x23, 0xDC, 0x46, 
            0x5D, 0xBE, 0x7B, 0x6D, 0x10, 0xEB, 0xC5, 0x06, 0x15, 0xDA, 0xBD, 0xD5, 0x59, 0x7E, 0x4D, 0xD0, 
            0xEC, 0x6F, 0x70, 0x5A, 0xDB, 0xFB, 0xB5, 0x77, 0x9F, 0x65, 0x7B, 0xFD, 0x17, 0x32, 0x90, 0x1B, 
            0x45, 0x55, 0xAD, 0x51, 0x20, 0xA5, 0xD6, 0x2A, 0xB3, 0xF5, 0xB4, 0x00, 0xB3, 0x7A, 0xA7, 0xEC, 
            0xF0, 0x61, 0x30, 0x0C, 0x04, 0xE7, 0x97, 0x3D, 0x75, 0x6F, 0xD2, 0xE7, 0xEE, 0x6A, 0xD6, 0x3C, 
            0xD5, 0x5A, 0x75, 0x21, 0x8F, 0x73, 0xF6, 0xE3, 0xE1, 0xA5, 0x69, 0x6D, 0xE7, 0x8D, 0xB9, 0xDF, 
            0x94, 0x96, 0x69, 0xE0, 0xA0, 0xC8, 0x06, 0x5A, 0x02, 0x84, 0x06, 0xEB, 0x19, 0x18, 0x37, 0x83, 
            0xE7, 0x26, 0xB2, 0xC8, 0x89, 0xEB, 0x44, 0xD4, 0x12, 0xFE, 0x15, 0x11, 0x16, 0xF1, 0xC5, 0x3F, 
            0xA1, 0x41, 0x48, 0xE9, 0x67, 0x3B, 0x4E, 0x9D, 0x39, 0x09, 0xEB, 0x16, 0x76, 0x1D, 0xF0, 0x5B, 
            0xCF, 0xE8, 0x69, 0xED, 0x8A, 0x3D, 0xA4, 0x70, 0x85, 0x4C, 0x6D, 0x56, 0x15, 0xA5, 0xBD, 0x32, 
            0x0A, 0x72, 0xF3, 0xA6, 0xEF, 0xA7, 0xF3, 0x00, 0x97, 0xCE, 0xBB, 0xE5, 0xCA, 0x34, 0x56, 0x6C, 
            0x70, 0x98, 0x90, 0x9E, 0x46, 0x6B, 0x82, 0xC9, 0x5F, 0x4D, 0x5E, 0xAD, 0x04, 0x24, 0xCA, 0xD2, 
            0x47, 0xCE, 0x3E, 0x22, 0xAE, 0x0D, 0x16, 0x02, 0xC6, 0x8F, 0xB7, 0x66, 0xA8, 0xA0, 0x7C, 0x5D, 
            0x6D, 0x3A, 0x61, 0x47, 0xEA, 0x7F, 0xB9, 0x45, 0x4F, 0x05, 0xE0, 0xCA, 0x34, 0x38, 0x67, 0x22, 
            0x9A, 0x7D, 0x21, 0x0D, 0x25, 0xB7, 0x74, 0x25, 0x00, 0x4F, 0x98, 0xE0, 0xF8, 0xE7, 0x38, 0x9C, 
            0x46, 0x82, 0x6D, 0x71, 0xE4, 0xB9, 0xC8, 0x2E, 0x65, 0xE1, 0xE5, 0x28, 0x1B, 0x9F, 0xB2, 0x80, 
            0xCE, 0x02, 0xF0, 0xEA, 0x13, 0x7B, 0x78, 0x4E, 0xF7, 0x2E, 0x3B, 0xA3, 0x6F, 0x02, 0xE0, 0x73, 
            0xE7, 0xFD, 0x61, 0xE4, 0x44, 0xBD, 0x17, 0x93, 0xE2, 0x37, 0x86, 0xC3, 0x65, 0xBD, 0x2B, 0xFE, 
            0xDF, 0x01, 0x95, 0xC8, 0x5D, 0xAD, 0x5F, 0xC1, 0x6C, 0xF4, 0x6F, 0x9D, 0x21, 0x5E, 0x0C, 0x57, 
            0xCA, 0x4D, 0x7A, 0xD5, 0x70, 0x41, 0x0A, 0x56, 0x36, 0x5B, 0x2F, 0x11, 0x83, 0x1B, 0x2D, 0xB6, 
            0x7D, 0xA7, 0x4B, 0x85, 0xF1, 0x3F, 0xBC, 0xFD, 0xDB, 0xE3, 0xB2, 0x05, 0x47, 0xA7, 0x18, 0xF7, 
            0x91, 0x46, 0x01, 0x46, 0x8E, 0x4D, 0xA0, 0x3B, 0xE4, 0x6C, 0x9E, 0xA1, 0x4E, 0x5E, 0xF1, 0x39, 
            0x63, 0x36, 0x7F, 0x0C, 0xC8, 0x82, 0xA2, 0x05, 0xF4, 0x55, 0x19, 0x89, 0xF2, 0x7D, 0x83, 0x29
        };
    }
}