using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_126377 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[(length * Keytable[0]) & 511];
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
                kidx += (uint)header.m_dataCount + digest[SignedMod(header.m_dataCount, SHA1_DIGESTSIZE)];
                buffer[i] ^= digest[SignedMod(header.m_buildVersion + i, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x17, 0xCF, 0x8A, 0x42, 0x3B, 0x75, 0xB2, 0xC1, 0xDB, 0xDB, 0x1E, 0x6F, 0xDD, 0xCA, 0x14, 0x94, 
            0xB6, 0x2C, 0x10, 0x00, 0x8B, 0xD4, 0x68, 0x47, 0xF3, 0xE4, 0x8E, 0xA8, 0x57, 0xCA, 0x73, 0x85, 
            0xE2, 0x0D, 0xD3, 0xED, 0xAD, 0xDC, 0xCE, 0xF0, 0x8D, 0x3F, 0xF0, 0xB9, 0x1C, 0x49, 0x94, 0x70, 
            0xFB, 0xF9, 0xDD, 0x21, 0xFC, 0xB6, 0xE4, 0x14, 0x4A, 0xC2, 0xEA, 0x0E, 0x75, 0x96, 0xEF, 0x3D, 
            0xD0, 0x61, 0xF8, 0xD4, 0x32, 0x84, 0x30, 0xD6, 0xD1, 0x4D, 0x1D, 0x34, 0xDD, 0xB8, 0xCD, 0xAB, 
            0x94, 0x39, 0x64, 0x03, 0xAF, 0x81, 0xDD, 0x7C, 0x86, 0xCF, 0x4D, 0x50, 0x23, 0xE6, 0x8B, 0x8B, 
            0x4B, 0x3D, 0x5C, 0x56, 0xBD, 0x19, 0x20, 0xF3, 0x47, 0x44, 0xAE, 0x47, 0xFA, 0x27, 0xFC, 0x41, 
            0x66, 0xAD, 0x5E, 0x4C, 0x99, 0x90, 0x14, 0x3C, 0x9E, 0x60, 0xE8, 0x1C, 0x0F, 0x89, 0xF9, 0xC7, 
            0x87, 0x75, 0xC1, 0xA2, 0xF5, 0xF8, 0xAE, 0x4A, 0xAC, 0x21, 0xEC, 0x41, 0x5A, 0xD8, 0xD6, 0x30, 
            0x6C, 0xEE, 0x9F, 0x96, 0xBE, 0x85, 0x8B, 0x25, 0xF5, 0x57, 0xB3, 0xFB, 0x98, 0xC3, 0x58, 0x0C, 
            0x84, 0xDC, 0x6C, 0xCC, 0x6D, 0x77, 0x19, 0x94, 0x46, 0xA2, 0x44, 0x27, 0xD1, 0x59, 0x87, 0x2C, 
            0x3C, 0x37, 0x73, 0x7B, 0xD4, 0x5A, 0xC9, 0x17, 0x02, 0x40, 0xC4, 0x65, 0x36, 0xA2, 0x8C, 0x6B, 
            0xD9, 0xB4, 0xF3, 0x98, 0x97, 0x00, 0x0D, 0xAE, 0x79, 0x0D, 0xD5, 0x14, 0xB1, 0xB8, 0xA4, 0x7B, 
            0xF0, 0x04, 0x74, 0x35, 0xF2, 0xDE, 0x0F, 0xC5, 0x4A, 0x12, 0xB4, 0xF8, 0x36, 0xB7, 0xAF, 0x89, 
            0xE1, 0x9F, 0x09, 0xF2, 0x2B, 0xA8, 0x88, 0x98, 0x43, 0xC2, 0x56, 0xC0, 0x64, 0x98, 0x70, 0x6D, 
            0x55, 0x73, 0xE6, 0xEA, 0x4F, 0x41, 0xBE, 0x52, 0xA1, 0x3C, 0xC9, 0xAE, 0x7C, 0x5D, 0x3F, 0x06, 
            0xE8, 0x2A, 0xF3, 0x39, 0xD7, 0x7E, 0x19, 0x20, 0x75, 0xF3, 0xCD, 0xD9, 0xD1, 0x49, 0x16, 0x73, 
            0xBD, 0xA1, 0xAA, 0xB5, 0x2E, 0x5A, 0xD7, 0x69, 0x91, 0x58, 0x6A, 0x04, 0xA9, 0xC4, 0x51, 0xF0, 
            0x44, 0x16, 0x95, 0x06, 0x88, 0xE4, 0x14, 0xF7, 0x25, 0xA1, 0xF1, 0x38, 0xC0, 0x21, 0x61, 0xCE, 
            0xAA, 0x79, 0x83, 0x43, 0x3D, 0xCA, 0xA2, 0x51, 0x80, 0x3B, 0xDA, 0x5E, 0x45, 0x71, 0x95, 0x6D, 
            0xB2, 0x64, 0x39, 0x5C, 0xA2, 0x6D, 0x19, 0xD5, 0xD2, 0x9C, 0xD8, 0xB7, 0x96, 0x8C, 0x09, 0x47, 
            0x93, 0x21, 0x5B, 0x17, 0x4F, 0x0A, 0xC4, 0xF1, 0x52, 0x56, 0x95, 0xE7, 0xD8, 0xB7, 0xE5, 0x68, 
            0xE6, 0x0F, 0x74, 0xCE, 0x74, 0xF0, 0xEC, 0x9D, 0xEA, 0x6F, 0xB7, 0x93, 0xF5, 0xBA, 0xC6, 0xF3, 
            0xB2, 0xF1, 0x66, 0xC9, 0x1D, 0x59, 0xC9, 0x6B, 0xAC, 0x8B, 0x42, 0x13, 0x5D, 0x37, 0xDA, 0x20, 
            0xBA, 0x62, 0x63, 0x53, 0x21, 0x24, 0x95, 0xE4, 0x58, 0xB6, 0x38, 0x40, 0x96, 0x1A, 0xB3, 0x00, 
            0x68, 0xDF, 0xA5, 0xD9, 0x4A, 0x33, 0x47, 0x2B, 0x8A, 0xAE, 0xCC, 0xC8, 0x56, 0x99, 0x8D, 0x90, 
            0x82, 0xD1, 0xF4, 0x30, 0xDE, 0x5D, 0xDC, 0xC3, 0xBB, 0x6F, 0x73, 0x41, 0x27, 0xDF, 0x95, 0xD5, 
            0xAA, 0xEA, 0xAD, 0xC6, 0x17, 0xDA, 0x80, 0x5C, 0xC1, 0xD4, 0x17, 0x75, 0x08, 0xFF, 0x41, 0x5C, 
            0x0C, 0xAF, 0xDD, 0x58, 0x25, 0x43, 0xB0, 0xD3, 0xC7, 0xAA, 0xBD, 0x3E, 0x30, 0xD8, 0xDC, 0xE8, 
            0xBD, 0x04, 0x88, 0xBB, 0x5C, 0x50, 0xEF, 0xDE, 0x8C, 0x92, 0x97, 0xF6, 0xA7, 0x75, 0xE3, 0x76, 
            0xAE, 0xC8, 0x41, 0x87, 0x62, 0x37, 0xD8, 0x8D, 0x8F, 0x30, 0xD6, 0xF3, 0x87, 0xC8, 0x67, 0xAB, 
            0x13, 0xA9, 0x4A, 0x9D, 0x8B, 0x2B, 0xAC, 0x36, 0x28, 0xAB, 0xD0, 0x4E, 0xC9, 0xF1, 0xFF, 0x88
        };
    }
}