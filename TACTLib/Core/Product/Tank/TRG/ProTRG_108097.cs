using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_108097 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[(length * Keytable[0]) & 511];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx = header.m_buildVersion - kidx;
            }
            return buffer;
        }

        public byte[] IV(TRGHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
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
            0xD8, 0x43, 0xB1, 0x6F, 0x97, 0xD5, 0xEC, 0x97, 0x22, 0xE5, 0xCC, 0x6A, 0x7C, 0xA9, 0x00, 0x66, 
            0xEE, 0x2C, 0x4F, 0x87, 0xCE, 0x49, 0x21, 0x56, 0x18, 0xEE, 0x39, 0x94, 0x28, 0x55, 0xE8, 0xB6, 
            0x2A, 0x70, 0x4C, 0xCF, 0x83, 0x4E, 0xB8, 0x19, 0xB1, 0x05, 0x06, 0x34, 0x2B, 0x0B, 0x1F, 0x30, 
            0x59, 0x26, 0x2E, 0x4D, 0xE3, 0x04, 0xF4, 0x20, 0xE2, 0xB7, 0xDC, 0x6A, 0xEE, 0xFD, 0xCD, 0xC5, 
            0xB3, 0x03, 0x80, 0x1C, 0xF3, 0x28, 0x29, 0x42, 0x88, 0x8C, 0x63, 0xAA, 0x87, 0x37, 0x47, 0xDA, 
            0x8C, 0x91, 0x76, 0x93, 0xA3, 0x08, 0x71, 0x5B, 0x25, 0xAA, 0x42, 0x9E, 0x1B, 0x59, 0xBF, 0x7E, 
            0x03, 0x61, 0x6E, 0xB1, 0xD8, 0x61, 0x3C, 0x2E, 0xFC, 0x71, 0x25, 0x2E, 0x9A, 0xD3, 0x07, 0xF9, 
            0xDF, 0xCC, 0x0E, 0x27, 0x47, 0xFF, 0x4A, 0xE0, 0x4F, 0x34, 0x48, 0x3D, 0xA4, 0xD0, 0x6A, 0xC8, 
            0x77, 0xB3, 0xD2, 0x6C, 0xF2, 0x9F, 0x00, 0xB7, 0x08, 0x27, 0x34, 0x2A, 0x93, 0x52, 0xFF, 0xCB, 
            0xA0, 0xCB, 0x80, 0x58, 0xA7, 0x25, 0xD5, 0xE8, 0xC1, 0xCD, 0xFB, 0x9D, 0x0E, 0x0D, 0x79, 0xD5, 
            0x23, 0x2D, 0x64, 0x01, 0x3F, 0x29, 0x37, 0x47, 0x11, 0x59, 0x35, 0xF1, 0xD4, 0xC4, 0x2B, 0x01, 
            0x5B, 0x70, 0xD5, 0xAB, 0x85, 0xFE, 0x99, 0x16, 0x61, 0x05, 0x56, 0x8A, 0x80, 0x97, 0x44, 0x11, 
            0xAF, 0xF1, 0xAC, 0xDC, 0xDB, 0xB0, 0xCC, 0xCA, 0xC1, 0x95, 0x4A, 0xAE, 0xA9, 0x1A, 0x19, 0xFE, 
            0x00, 0x96, 0x6E, 0xDE, 0x96, 0xAC, 0x43, 0xD3, 0xDF, 0xBC, 0xD0, 0x54, 0x63, 0x37, 0xD6, 0xEB, 
            0x3E, 0x80, 0x0A, 0x2F, 0xCF, 0xC2, 0x10, 0x22, 0xA3, 0x23, 0xE6, 0xF0, 0x79, 0x46, 0x72, 0x41, 
            0x44, 0x61, 0x5A, 0xB3, 0xE1, 0xDD, 0x01, 0x7C, 0x6F, 0x11, 0x7A, 0x80, 0xF2, 0x16, 0x71, 0xA1, 
            0xBF, 0x9F, 0x54, 0xA2, 0xC2, 0xA6, 0x6F, 0x5C, 0x1F, 0x57, 0xE9, 0xC6, 0x2D, 0xEA, 0x3C, 0xA1, 
            0x21, 0x4E, 0x64, 0x34, 0x3E, 0xD5, 0x54, 0x04, 0x5A, 0x6B, 0x4C, 0x9E, 0xB9, 0x17, 0x5E, 0xC7, 
            0x9F, 0xB2, 0x86, 0x76, 0x6F, 0x95, 0xD6, 0xDB, 0x0A, 0x84, 0xE7, 0x77, 0xEF, 0x29, 0x26, 0xEA, 
            0x34, 0x2B, 0x4C, 0xE0, 0xEB, 0x9A, 0xF2, 0x94, 0x24, 0x53, 0x84, 0xBC, 0xAD, 0xD4, 0x66, 0x69, 
            0x8C, 0xFB, 0xBB, 0xE5, 0x41, 0x29, 0xB8, 0xB2, 0xD1, 0xBB, 0x04, 0xB4, 0xB6, 0xDB, 0xAF, 0x85, 
            0x75, 0xC7, 0x7C, 0x2D, 0x0F, 0xEA, 0x7B, 0xC9, 0x5F, 0x81, 0x64, 0xEE, 0x1D, 0x0C, 0x6B, 0x48, 
            0xD3, 0xA2, 0x3E, 0x56, 0x3C, 0xD0, 0x42, 0xFD, 0xF1, 0xBD, 0x03, 0x82, 0x93, 0x29, 0xD8, 0x3A, 
            0x51, 0xCD, 0x41, 0xDD, 0x27, 0x1B, 0x58, 0xB3, 0xD8, 0xB4, 0x28, 0xEE, 0x9F, 0x04, 0x55, 0x77, 
            0xF9, 0x6B, 0x95, 0xEF, 0x61, 0xAB, 0x91, 0xC5, 0xC0, 0xDB, 0xDE, 0xBF, 0xEE, 0x21, 0x32, 0x30, 
            0x6D, 0x26, 0x34, 0xF7, 0x84, 0x84, 0x12, 0x39, 0x42, 0xC3, 0xC8, 0x6C, 0xD9, 0xD9, 0xC3, 0xC4, 
            0x3B, 0x1D, 0x71, 0x8D, 0xC2, 0x53, 0xC8, 0x13, 0x99, 0x02, 0x42, 0xD0, 0x8A, 0x5B, 0xDB, 0xE8, 
            0x8D, 0x3B, 0xF1, 0xF2, 0x86, 0x59, 0x41, 0x18, 0x78, 0x36, 0x66, 0x64, 0xDB, 0x50, 0xE2, 0x22, 
            0xDE, 0x74, 0x0D, 0x91, 0x9E, 0xBA, 0xE9, 0x96, 0x35, 0xD0, 0x42, 0xA6, 0xF0, 0x89, 0xBA, 0x5D, 
            0x1E, 0xC2, 0x13, 0x27, 0x8C, 0x37, 0x5B, 0xC6, 0x18, 0xAF, 0xB3, 0xD8, 0xCB, 0x55, 0x8A, 0x33, 
            0x2D, 0x5D, 0x08, 0x12, 0xCA, 0x49, 0x93, 0x58, 0x1C, 0x0A, 0x91, 0xD5, 0xCE, 0x8C, 0x69, 0xDB, 
            0xAD, 0x47, 0x94, 0xE9, 0x41, 0x44, 0x3C, 0x91, 0x5E, 0x28, 0x79, 0x0D, 0x0D, 0x36, 0xF2, 0x44
        };
    }
}