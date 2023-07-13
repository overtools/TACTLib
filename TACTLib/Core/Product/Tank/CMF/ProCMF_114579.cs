using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_114579 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[length + 256];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx = header.m_buildVersion - kidx;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx = header.m_buildVersion - kidx;
                buffer[i] ^= digest[SignedMod(kidx + i, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x4E, 0xF1, 0x56, 0xDA, 0x1C, 0x82, 0xE7, 0x94, 0x2E, 0xF3, 0x8C, 0x0A, 0x79, 0x9E, 0xDC, 0xFE, 
            0x89, 0xCB, 0x7F, 0x74, 0xD0, 0xDC, 0x96, 0x90, 0xE0, 0x57, 0x87, 0x5C, 0x7A, 0xBB, 0x24, 0x4A, 
            0xEB, 0x09, 0x89, 0x90, 0x1E, 0x9B, 0x14, 0x4F, 0xAE, 0xE4, 0x0E, 0xAF, 0x40, 0x42, 0x68, 0x43, 
            0xD7, 0x41, 0x16, 0xDB, 0x9F, 0xDF, 0x11, 0x69, 0x0C, 0x90, 0xDF, 0x13, 0x94, 0xE6, 0xAB, 0x56, 
            0x75, 0xF8, 0xE2, 0xE1, 0xA1, 0xE9, 0xAA, 0x05, 0x27, 0x47, 0x62, 0x94, 0x19, 0x08, 0xE6, 0xDD, 
            0x3E, 0xAE, 0x25, 0x47, 0xFA, 0xE6, 0x95, 0x93, 0x30, 0x3F, 0x0C, 0x13, 0x08, 0x0B, 0xA7, 0x4F, 
            0x14, 0xCC, 0x00, 0xF0, 0xB9, 0x4F, 0xD6, 0x13, 0xF1, 0x32, 0x72, 0xA9, 0x0E, 0x56, 0x70, 0xC1, 
            0x97, 0x72, 0xB0, 0x7D, 0x37, 0x7A, 0x2D, 0x3B, 0x25, 0x51, 0x91, 0xFE, 0x09, 0xA6, 0xA0, 0x99, 
            0xCB, 0x46, 0x0B, 0x4F, 0xE6, 0x46, 0x99, 0xE6, 0x29, 0xD7, 0x80, 0xB6, 0xA3, 0x78, 0x7E, 0xDB, 
            0x05, 0x9D, 0x9C, 0x6B, 0x4B, 0x6A, 0x14, 0x89, 0xA0, 0x35, 0x7C, 0x2D, 0x15, 0x5A, 0xA8, 0x2E, 
            0xF4, 0xDF, 0x35, 0x9C, 0x82, 0x27, 0x4F, 0xC9, 0xB8, 0x59, 0xA9, 0x4B, 0x7A, 0x07, 0xC2, 0x85, 
            0xAA, 0xC8, 0x3F, 0x30, 0x6A, 0x58, 0xC5, 0x7C, 0x5D, 0xED, 0xF9, 0x1A, 0x5B, 0x73, 0x33, 0x35, 
            0xD1, 0x74, 0x0B, 0x82, 0xFE, 0x37, 0x3E, 0xA3, 0xB3, 0xC4, 0x29, 0xC0, 0x5C, 0xC1, 0x29, 0x47, 
            0xBA, 0xCA, 0x41, 0x1A, 0x6E, 0x34, 0x09, 0x36, 0xB0, 0xD2, 0xA1, 0xBA, 0xE9, 0xA8, 0xE8, 0x29, 
            0x1D, 0x2B, 0x1C, 0xBC, 0x22, 0x01, 0x4C, 0xC7, 0xEC, 0x5E, 0xE4, 0xF8, 0xB9, 0xE1, 0x4D, 0x4E, 
            0xD4, 0x12, 0x79, 0xF0, 0xBA, 0xE2, 0x34, 0x32, 0x50, 0x4F, 0x41, 0x5C, 0xAD, 0x8E, 0xA4, 0x44, 
            0xDD, 0xC6, 0x1D, 0x3E, 0x8C, 0x9C, 0xC1, 0x0A, 0x9A, 0x39, 0x7F, 0x0E, 0xD4, 0x11, 0xDD, 0x18, 
            0x52, 0x15, 0xCE, 0x89, 0x8E, 0x19, 0x40, 0xBA, 0xB4, 0x77, 0x55, 0xB5, 0xF6, 0xCB, 0x51, 0x85, 
            0xA9, 0x2D, 0x6B, 0x38, 0x83, 0xBD, 0x47, 0x75, 0x2D, 0xBF, 0xE3, 0xA5, 0x10, 0xA2, 0xEE, 0x2C, 
            0x4E, 0x0C, 0x03, 0xE5, 0x87, 0xC4, 0x8A, 0x6F, 0x22, 0x4A, 0xA0, 0xE6, 0x8B, 0x4F, 0x63, 0x92, 
            0xF1, 0x91, 0x27, 0xC7, 0xAC, 0x70, 0x35, 0x1F, 0xCD, 0xA9, 0x4A, 0x77, 0x0B, 0x5A, 0x3B, 0xFA, 
            0x30, 0xB6, 0x1E, 0x9B, 0x3D, 0xF2, 0x56, 0xFA, 0x16, 0x13, 0xAC, 0xDD, 0x42, 0x89, 0x79, 0xEA, 
            0x45, 0x60, 0x34, 0x1B, 0x12, 0x86, 0xDA, 0xBC, 0xFC, 0x24, 0xC0, 0x4A, 0x89, 0x81, 0x83, 0xFF, 
            0x38, 0x81, 0xB2, 0xB5, 0xDF, 0xA8, 0x49, 0x8F, 0xE9, 0x55, 0xA4, 0x5F, 0xDF, 0x4F, 0xA9, 0x68, 
            0x9E, 0x33, 0x1B, 0xA0, 0x3C, 0xD7, 0x13, 0x76, 0x28, 0x1D, 0xB0, 0xBA, 0x09, 0x31, 0x74, 0x63, 
            0x7A, 0x08, 0x62, 0x07, 0x37, 0x08, 0x19, 0x82, 0x6F, 0xD0, 0x99, 0x40, 0x84, 0x8C, 0x2F, 0xB6, 
            0x13, 0x36, 0x0E, 0xC4, 0xD0, 0x9F, 0xCF, 0xF4, 0x89, 0x11, 0x0F, 0xB0, 0xDA, 0x8D, 0x12, 0x69, 
            0xF4, 0x97, 0x3F, 0x88, 0x9F, 0x88, 0xD4, 0x59, 0xB1, 0xD2, 0x78, 0x11, 0x71, 0x25, 0x33, 0x70, 
            0x95, 0xDF, 0xAB, 0x0C, 0x03, 0xF1, 0xD0, 0xB7, 0x33, 0xED, 0xDC, 0x18, 0x8C, 0x4C, 0xA9, 0x56, 
            0x47, 0xC3, 0xFE, 0xBD, 0x92, 0xD0, 0xA2, 0xC7, 0x61, 0xE5, 0xB6, 0x5C, 0xD9, 0xA2, 0x4E, 0xCA, 
            0x08, 0x35, 0x4C, 0xF1, 0xFE, 0x12, 0xC4, 0x6C, 0xE3, 0x43, 0xBE, 0xC1, 0xE7, 0x70, 0x00, 0x1C, 
            0xDC, 0xC4, 0x65, 0xCD, 0x0A, 0xCC, 0x00, 0x88, 0x58, 0xB7, 0xBD, 0xA5, 0xAD, 0x30, 0x6F, 0x91
        };
    }
}