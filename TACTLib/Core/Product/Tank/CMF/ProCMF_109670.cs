using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_109670 : ICMFEncryptionProc
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
                kidx += 3;
                buffer[i] ^= digest[SignedMod(kidx - i, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xB1, 0xEF, 0x1F, 0x4A, 0x2A, 0x5F, 0xBC, 0x94, 0x43, 0x2D, 0x12, 0x2B, 0xD3, 0x1A, 0xDD, 0x05, 
            0x71, 0x69, 0x85, 0x50, 0x7B, 0x47, 0xCD, 0xC7, 0x06, 0x7C, 0xAF, 0x75, 0x19, 0x08, 0x12, 0x7A, 
            0x58, 0x5B, 0x4D, 0x04, 0xB5, 0xAA, 0xCE, 0x50, 0x98, 0x8A, 0xC3, 0x7D, 0xB1, 0xE0, 0x31, 0xDF, 
            0x8F, 0x87, 0x18, 0xA0, 0x8A, 0x49, 0x70, 0x91, 0x76, 0x27, 0xD8, 0x54, 0x81, 0x0E, 0x71, 0xDD, 
            0x6B, 0x94, 0x4C, 0x46, 0x08, 0xBD, 0xD5, 0x07, 0xC4, 0x36, 0xB1, 0x58, 0xC6, 0x4A, 0x28, 0x00, 
            0xDA, 0xFB, 0x11, 0x68, 0x98, 0xF5, 0xF9, 0x28, 0xE7, 0xE1, 0x69, 0xF9, 0xC6, 0x2A, 0x03, 0x75, 
            0x4D, 0x54, 0x9B, 0x39, 0xC4, 0x8C, 0x80, 0xED, 0x91, 0xE7, 0xDA, 0xDE, 0xEB, 0x81, 0xDB, 0x37, 
            0x92, 0xAB, 0x79, 0xC3, 0x85, 0x20, 0x01, 0x5F, 0x7D, 0x33, 0x20, 0xC4, 0xF0, 0xF5, 0x62, 0x0C, 
            0xD7, 0x4B, 0x59, 0xC9, 0xD5, 0xAE, 0x6C, 0x67, 0x9C, 0x96, 0x23, 0xA9, 0x09, 0x84, 0x78, 0x06, 
            0x00, 0x4A, 0xF2, 0x3C, 0xA4, 0x8B, 0xD7, 0x33, 0x5A, 0x7A, 0xFB, 0x6C, 0xDE, 0xBA, 0xF1, 0x85, 
            0x7B, 0xDE, 0x97, 0x0D, 0xD1, 0x70, 0xD3, 0xB5, 0x56, 0x7C, 0xDB, 0x8B, 0x87, 0x54, 0x71, 0x5D, 
            0x56, 0x7B, 0x03, 0xAD, 0xD9, 0xB9, 0xA3, 0x26, 0x94, 0x86, 0x2E, 0xDB, 0xBE, 0xD1, 0x45, 0xAC, 
            0xFE, 0xA0, 0x84, 0x57, 0xA4, 0x9F, 0x03, 0x72, 0x38, 0x02, 0x8B, 0x47, 0x37, 0x37, 0xB3, 0x0F, 
            0x4D, 0x86, 0x2B, 0xED, 0xEE, 0x2A, 0x5B, 0xAD, 0x0C, 0x12, 0x5B, 0xC7, 0xF7, 0x3A, 0x5B, 0xA3, 
            0x5B, 0xDA, 0xFC, 0x9B, 0xBE, 0x4A, 0x18, 0xD4, 0x45, 0x94, 0xD5, 0xFA, 0xD8, 0x75, 0xDF, 0xAF, 
            0x78, 0x0F, 0xFC, 0xD7, 0xAD, 0xE7, 0xA7, 0xB5, 0x32, 0x00, 0x4A, 0x98, 0x13, 0x9F, 0x83, 0x47, 
            0x59, 0xE3, 0xA1, 0x92, 0xC7, 0xEA, 0xBF, 0x3F, 0xDD, 0xD9, 0x4E, 0xCC, 0x50, 0x10, 0xB7, 0xF8, 
            0xF1, 0x6B, 0x33, 0x14, 0x98, 0xD7, 0xEC, 0xE4, 0x4A, 0x23, 0xFA, 0xFE, 0x76, 0x26, 0x36, 0x1A, 
            0x78, 0x32, 0x5A, 0x82, 0xDE, 0x48, 0xFB, 0x61, 0x80, 0x45, 0xE1, 0x17, 0xB7, 0x84, 0xD7, 0xFD, 
            0xFC, 0x64, 0xBF, 0x0A, 0xA1, 0xF1, 0x59, 0xDB, 0x03, 0x95, 0xD6, 0x61, 0xE6, 0x1D, 0xDA, 0xB3, 
            0x5B, 0x76, 0x37, 0xA6, 0x21, 0xE6, 0x7D, 0xDA, 0x20, 0x5C, 0xDA, 0x0B, 0xDC, 0x19, 0x8E, 0xC9, 
            0x2B, 0xE8, 0xD9, 0x7B, 0x2C, 0xF6, 0xE7, 0x99, 0xBC, 0x6A, 0x46, 0x56, 0xF3, 0x13, 0xD0, 0xE0, 
            0x3D, 0x4D, 0x07, 0x52, 0xC3, 0xDD, 0xD0, 0x63, 0x55, 0x35, 0x28, 0x86, 0x5E, 0xB1, 0x2A, 0x20, 
            0x6D, 0xA5, 0x8D, 0x06, 0x09, 0x8A, 0x98, 0x90, 0x8C, 0x9B, 0xC2, 0x62, 0x03, 0x04, 0xB2, 0xF1, 
            0x56, 0xF7, 0x57, 0xFF, 0x7C, 0x7D, 0xA6, 0x2C, 0x71, 0x5B, 0xF6, 0xFB, 0x7E, 0x2C, 0x48, 0x83, 
            0x5F, 0xD1, 0x80, 0x9B, 0x6F, 0x46, 0x7D, 0x70, 0x84, 0x85, 0x7C, 0xD2, 0x20, 0xC5, 0xAD, 0x9F, 
            0x29, 0xAE, 0x32, 0x3F, 0x77, 0x2B, 0x11, 0xCC, 0x64, 0x4F, 0x46, 0x5C, 0xE7, 0x50, 0x2D, 0xE2, 
            0xCA, 0xFD, 0x6F, 0x0F, 0x16, 0xF6, 0x2C, 0x74, 0x2F, 0xA9, 0x2A, 0x61, 0xF0, 0x9B, 0xD7, 0xCC, 
            0xB6, 0x08, 0x5E, 0xC7, 0x38, 0x9F, 0xD1, 0x1A, 0x15, 0x38, 0x73, 0x10, 0x22, 0xF1, 0x02, 0x76, 
            0xF4, 0x44, 0x4B, 0x35, 0xB5, 0x2A, 0xB8, 0x5A, 0xC3, 0xD6, 0x44, 0x76, 0xD6, 0xCF, 0x0A, 0x95, 
            0x82, 0x42, 0x61, 0x41, 0xBF, 0x92, 0x7C, 0x99, 0xA2, 0x32, 0x43, 0x4F, 0x85, 0xAC, 0x53, 0xA9, 
            0x99, 0x26, 0x40, 0x3E, 0x8C, 0x0E, 0x7D, 0x81, 0x3D, 0x55, 0x98, 0xF0, 0xD6, 0x47, 0x88, 0x19
        };
    }
}