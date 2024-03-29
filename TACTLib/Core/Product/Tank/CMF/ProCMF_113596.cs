using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_113596 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_dataCount & 511];
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
            uint kidx, okidx;
            kidx = okidx = (uint)(length * header.m_buildVersion);
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
            0x85, 0x6F, 0xD4, 0x8C, 0xB1, 0xE1, 0x5F, 0x01, 0x2E, 0x36, 0x35, 0xB3, 0xDD, 0xDE, 0xD1, 0x23, 
            0xA6, 0x0C, 0x58, 0x89, 0x5C, 0x3F, 0xBD, 0x9B, 0x99, 0xA8, 0x2F, 0xB7, 0x8A, 0xB7, 0x67, 0xAD, 
            0x5E, 0xFF, 0x0E, 0x22, 0x2D, 0x3E, 0x49, 0xA4, 0x8E, 0x19, 0x50, 0x0D, 0x45, 0xCF, 0x16, 0xA1, 
            0x79, 0xC3, 0x04, 0x22, 0x78, 0x4F, 0xEC, 0x7D, 0x74, 0x97, 0xBC, 0x78, 0x16, 0x97, 0x7A, 0xB5, 
            0x44, 0x24, 0x3A, 0xCD, 0xD3, 0x85, 0x5E, 0x65, 0xD0, 0xAB, 0x7C, 0x16, 0x70, 0xEE, 0xBD, 0x55, 
            0x3C, 0x36, 0xD7, 0xB3, 0xF3, 0x5F, 0x08, 0xA8, 0xBB, 0xB3, 0xBA, 0xDE, 0xB0, 0x13, 0x41, 0x22, 
            0xF8, 0x03, 0x18, 0x23, 0xEF, 0xE5, 0x10, 0x4E, 0xA0, 0x34, 0xE2, 0x8A, 0x0E, 0x22, 0xB2, 0xC8, 
            0x98, 0xE6, 0x3F, 0x92, 0x6E, 0x4E, 0x70, 0x9B, 0xAD, 0x44, 0xD8, 0x1F, 0xD3, 0x8D, 0xBE, 0x44, 
            0xBE, 0x21, 0x08, 0xA1, 0xD2, 0x70, 0x78, 0x13, 0xF5, 0x79, 0xE8, 0xF9, 0xCE, 0xFD, 0xA8, 0x7F, 
            0x72, 0xA9, 0xF9, 0xC4, 0x08, 0x52, 0xA7, 0x81, 0xF3, 0xAE, 0xCD, 0xA9, 0xF4, 0x7D, 0x62, 0xF2, 
            0xFE, 0xDA, 0xCE, 0x3A, 0xA7, 0xC7, 0x46, 0x19, 0x41, 0x8D, 0xEA, 0xDA, 0x8E, 0x65, 0xE7, 0x06, 
            0x33, 0x34, 0xA1, 0x4F, 0x5D, 0x37, 0x1C, 0xC3, 0x27, 0xF8, 0xC5, 0x04, 0x80, 0x0A, 0x54, 0xC8, 
            0x25, 0x43, 0x81, 0x21, 0x1C, 0x78, 0x01, 0x50, 0x57, 0x98, 0x69, 0xD5, 0x84, 0xEE, 0x05, 0xE2, 
            0xCD, 0xDA, 0x1A, 0x75, 0x31, 0xD8, 0x91, 0x17, 0xC9, 0x5F, 0xC0, 0xBD, 0x4C, 0x13, 0x56, 0x6F, 
            0x8C, 0x6C, 0xA0, 0xB2, 0x74, 0xB9, 0xD1, 0xD4, 0xCF, 0xDB, 0xD3, 0x10, 0xE0, 0x46, 0x3A, 0x9A, 
            0x45, 0x65, 0x0D, 0xB9, 0x5A, 0xA0, 0x9F, 0x1C, 0xDC, 0x2C, 0x69, 0xAD, 0x52, 0x00, 0xE8, 0xBF, 
            0x85, 0x47, 0xB7, 0x0D, 0xEC, 0xB2, 0x9E, 0xF9, 0xA5, 0x33, 0x0E, 0x01, 0x3E, 0xE8, 0xCB, 0x66, 
            0x15, 0x42, 0x76, 0x5B, 0xF3, 0xA8, 0x60, 0x05, 0x3D, 0x2C, 0x02, 0x13, 0x6A, 0xD5, 0x70, 0x56, 
            0x2D, 0x7C, 0xB2, 0xDF, 0xDF, 0xC0, 0xF8, 0x6E, 0x7C, 0xC6, 0x78, 0xA2, 0xAD, 0x7B, 0x2A, 0x82, 
            0x51, 0xC8, 0x90, 0x13, 0x68, 0xF9, 0xBC, 0x8B, 0x34, 0x5E, 0x35, 0xDB, 0x20, 0xD6, 0x48, 0x8D, 
            0xE1, 0xB5, 0x9E, 0x3D, 0xFD, 0x5E, 0x97, 0x2C, 0xE4, 0x0C, 0x28, 0xB2, 0x7C, 0x83, 0x9A, 0x82, 
            0x5C, 0xED, 0xEC, 0x85, 0x58, 0x15, 0x87, 0x78, 0x1D, 0xC3, 0x08, 0xAF, 0xBE, 0x56, 0x6C, 0xBA, 
            0x68, 0x64, 0x0C, 0x1C, 0x25, 0x57, 0xB1, 0x69, 0x75, 0x18, 0x29, 0x84, 0x73, 0xB1, 0x60, 0x96, 
            0xF9, 0x2C, 0xC4, 0x58, 0xFB, 0xF3, 0x50, 0xFA, 0xAF, 0x9A, 0x4B, 0x73, 0x73, 0xA1, 0xB2, 0x25, 
            0x79, 0xDF, 0xB2, 0x47, 0xF4, 0x1A, 0x4E, 0x71, 0x63, 0x4C, 0xE2, 0xD5, 0x98, 0x51, 0x05, 0xC7, 
            0x7D, 0x9F, 0xC7, 0x19, 0x0B, 0x53, 0x8E, 0x54, 0xEE, 0x43, 0xAB, 0xEE, 0x69, 0x8F, 0x6A, 0xBD, 
            0xD1, 0x5C, 0xBA, 0xD0, 0x45, 0xA8, 0x1E, 0xD9, 0x74, 0x9E, 0x3D, 0x76, 0xC8, 0x71, 0xCC, 0x33, 
            0x31, 0x49, 0xC0, 0x3A, 0x64, 0x67, 0x6C, 0xB3, 0x03, 0x1B, 0xFF, 0x87, 0x7A, 0x84, 0xB0, 0xC7, 
            0x51, 0x06, 0x30, 0xDD, 0x40, 0xF9, 0x17, 0xC5, 0xBD, 0xE5, 0x06, 0xC6, 0x54, 0xBB, 0x2F, 0x4C, 
            0xE1, 0x40, 0xB7, 0x2D, 0x6D, 0x26, 0x80, 0x17, 0x87, 0xBA, 0x57, 0x12, 0x41, 0xA9, 0xC0, 0x91, 
            0x9A, 0xCA, 0xD5, 0xEF, 0x0A, 0x75, 0xE3, 0xD1, 0xE0, 0xE9, 0xDC, 0xC1, 0xCC, 0x9A, 0x71, 0x31, 
            0xE0, 0xB5, 0xC6, 0x6E, 0x97, 0x35, 0x65, 0x64, 0x99, 0x40, 0x9B, 0x09, 0x38, 0x7D, 0x1C, 0xC4
        };
    }
}