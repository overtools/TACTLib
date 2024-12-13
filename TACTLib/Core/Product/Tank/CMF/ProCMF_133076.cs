using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_133076 : ICMFEncryptionProc
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
                kidx += (uint)header.m_dataCount + digest[SignedMod(header.m_dataCount, SHA1_DIGESTSIZE)];
                buffer[i] ^= digest[SignedMod(header.m_buildVersion + i, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xCF, 0x02, 0xA7, 0xB9, 0x74, 0x0F, 0x59, 0x11, 0x78, 0x73, 0xC4, 0xA0, 0x53, 0xE5, 0xE9, 0x13, 
            0x9D, 0xCE, 0x0C, 0xCB, 0xDD, 0x18, 0x3B, 0x0A, 0x5D, 0x22, 0xC6, 0x78, 0xAB, 0x19, 0x7A, 0x21, 
            0x6F, 0xBA, 0x00, 0x72, 0xD7, 0x37, 0x5E, 0x35, 0xB7, 0xF9, 0x63, 0x30, 0x19, 0xDB, 0xC0, 0x0C, 
            0xB2, 0x76, 0xC2, 0x18, 0xA5, 0xB9, 0x9C, 0xEB, 0xC6, 0xC5, 0x84, 0x18, 0x2B, 0x85, 0x2E, 0x01, 
            0x3B, 0x84, 0xD2, 0x92, 0x6D, 0x71, 0x7C, 0x72, 0x8D, 0x87, 0x60, 0x5A, 0x66, 0x9B, 0x4B, 0xDB, 
            0xCC, 0x3C, 0x33, 0x49, 0x87, 0x40, 0xA9, 0xE7, 0xF7, 0x33, 0x32, 0xDA, 0x26, 0xC8, 0x70, 0xAA, 
            0xCD, 0x4D, 0xF6, 0x90, 0x45, 0x83, 0x6F, 0xC3, 0x37, 0x14, 0x2E, 0x4B, 0xBC, 0x66, 0x42, 0x40, 
            0x95, 0x5E, 0xBA, 0x84, 0x2B, 0x92, 0x6C, 0xC1, 0xE8, 0x05, 0x47, 0x70, 0x6E, 0xC7, 0xDD, 0x7E, 
            0x3A, 0x29, 0x8A, 0x12, 0xE0, 0x5E, 0x35, 0x40, 0x5F, 0x85, 0x21, 0x2E, 0x7E, 0x2D, 0xC2, 0x5F, 
            0x54, 0x21, 0xAA, 0xBB, 0xB5, 0xD4, 0xBA, 0x79, 0x85, 0x9F, 0x8D, 0x39, 0x11, 0x41, 0xA4, 0x75, 
            0x82, 0xB9, 0xE5, 0xF6, 0x3D, 0xDF, 0x21, 0x95, 0x48, 0x72, 0xAA, 0x80, 0x8C, 0xCC, 0xE5, 0x99, 
            0xED, 0x76, 0x09, 0x69, 0xE0, 0x3C, 0xEB, 0xDB, 0x67, 0x0A, 0x45, 0xE5, 0x0B, 0x7E, 0xAB, 0x56, 
            0x80, 0xEB, 0xA8, 0xF0, 0xB8, 0xC6, 0x6A, 0x2B, 0xE7, 0xB0, 0x47, 0xCB, 0x23, 0x59, 0x70, 0x31, 
            0x08, 0xEF, 0x12, 0xC7, 0xC8, 0x22, 0xC5, 0x36, 0xC5, 0x2C, 0x71, 0xFE, 0x69, 0x0B, 0xAD, 0x03, 
            0xBD, 0x50, 0x19, 0x4F, 0xE1, 0x23, 0x41, 0x03, 0x08, 0x1B, 0xF3, 0xCF, 0xC0, 0xDE, 0xE5, 0x35, 
            0x42, 0xE7, 0xAE, 0x7F, 0x61, 0x91, 0x3C, 0x6A, 0xB2, 0x0A, 0x0D, 0xA1, 0x85, 0x48, 0x08, 0x09, 
            0xC3, 0xE2, 0x30, 0xB1, 0xE2, 0xF7, 0xBF, 0x39, 0xD4, 0xEA, 0x50, 0x85, 0xAA, 0xD1, 0x35, 0x8C, 
            0x4F, 0x25, 0xAC, 0xEB, 0x01, 0x21, 0x50, 0xFA, 0x7B, 0x94, 0xFD, 0x47, 0x5B, 0x02, 0x05, 0xA2, 
            0xB9, 0xE0, 0xD4, 0x83, 0x07, 0x9F, 0x71, 0x47, 0xB4, 0x87, 0x98, 0x9B, 0x99, 0x63, 0x16, 0x6A, 
            0xC8, 0xC1, 0x26, 0x69, 0x00, 0xD6, 0xE6, 0xE0, 0x70, 0xFC, 0xD9, 0xD1, 0xCB, 0x48, 0x30, 0x68, 
            0xFE, 0x30, 0xE4, 0x2D, 0x37, 0x3E, 0x67, 0x8E, 0x70, 0x20, 0xA1, 0x9E, 0x0D, 0xE6, 0xC8, 0xEC, 
            0x06, 0xC5, 0x02, 0x75, 0x0F, 0x21, 0x44, 0xA6, 0x52, 0x18, 0x4D, 0xB2, 0x17, 0x04, 0xE5, 0x44, 
            0x24, 0x58, 0xD9, 0x4C, 0xC0, 0xF2, 0x3C, 0xEA, 0xDD, 0xBF, 0xDD, 0xB2, 0x90, 0x7D, 0x50, 0x46, 
            0x69, 0xEC, 0x56, 0xF0, 0xBF, 0x35, 0x70, 0x7C, 0xC9, 0xFC, 0x1D, 0xBB, 0x7E, 0xC5, 0xB1, 0x9E, 
            0x51, 0x4F, 0xD1, 0x6E, 0x97, 0x7D, 0x0C, 0x75, 0x86, 0x63, 0x1A, 0xC5, 0x81, 0x26, 0x4B, 0xE1, 
            0x22, 0x4A, 0xF6, 0xE3, 0x7B, 0xFD, 0x81, 0xB4, 0x6B, 0x21, 0xD1, 0x94, 0x69, 0x12, 0x11, 0xFB, 
            0x47, 0xA7, 0x55, 0x23, 0xE1, 0xB9, 0x2F, 0x85, 0x47, 0x7D, 0x72, 0x8F, 0x56, 0xCD, 0xF2, 0x0E, 
            0xDE, 0xCA, 0x60, 0xB4, 0xD5, 0x13, 0x2C, 0x6D, 0xDD, 0xD4, 0x12, 0x8A, 0x7A, 0x58, 0xAA, 0xEB, 
            0x57, 0xEF, 0xEA, 0x2B, 0xD4, 0xFF, 0x81, 0x3B, 0x0E, 0xF8, 0xAB, 0x51, 0x33, 0xDD, 0x06, 0xA7, 
            0xD7, 0xB5, 0x47, 0x00, 0xBE, 0x7B, 0x41, 0x9D, 0x56, 0x57, 0x93, 0xD6, 0x87, 0xAF, 0x5C, 0x56, 
            0x08, 0x2C, 0x47, 0x2F, 0xCE, 0x06, 0x41, 0xED, 0x56, 0xA4, 0x63, 0xFB, 0x20, 0x3C, 0x3E, 0x08, 
            0x13, 0x35, 0x47, 0xFD, 0xEB, 0xB6, 0x69, 0xB9, 0xFA, 0xA9, 0x56, 0xFE, 0x05, 0x17, 0x3F, 0x0A
        };
    }
}