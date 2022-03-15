using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_94717 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[(length * Keytable[0]) & 511];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx -= header.m_buildVersion & 511;
            }
            return buffer;
        }

        public byte[] IV(TRGHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(length * header.m_buildVersion);
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 13;
                buffer[i] ^= digest[SignedMod(kidx + header.GetNonEncryptedMagic(), SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xCA, 0x55, 0x57, 0xE8, 0x3F, 0x0A, 0x44, 0xB3, 0x8A, 0x1C, 0x91, 0x27, 0x75, 0x0D, 0xEA, 0x24, 
            0x45, 0x56, 0x26, 0xD0, 0x8D, 0xD1, 0x4D, 0xAD, 0x01, 0x64, 0x5D, 0xF4, 0xCA, 0xD9, 0x18, 0xA2, 
            0xCD, 0x32, 0xD3, 0xB6, 0x66, 0x30, 0x3C, 0xE7, 0x09, 0xB7, 0xDD, 0xF4, 0x5E, 0xA0, 0x15, 0x06, 
            0x2B, 0x8D, 0x32, 0xAA, 0x6A, 0x32, 0x51, 0x9C, 0x82, 0x32, 0xDD, 0xDD, 0x6B, 0x28, 0x67, 0xD0, 
            0xC5, 0x29, 0xFF, 0xDC, 0x00, 0x6A, 0x57, 0x57, 0xFC, 0x4A, 0xEE, 0x56, 0xA8, 0xAE, 0x6A, 0xB1, 
            0xDD, 0xCD, 0x04, 0x1A, 0x5B, 0x38, 0x49, 0x51, 0x3B, 0xF2, 0xC6, 0xE4, 0x25, 0xD6, 0x4C, 0xE9, 
            0x58, 0x73, 0xB8, 0x93, 0xC5, 0x0D, 0x2F, 0xFA, 0x22, 0x45, 0x96, 0x89, 0xA0, 0x14, 0x4F, 0x14, 
            0xD8, 0x55, 0x65, 0x12, 0x66, 0x0F, 0x82, 0xBF, 0xA0, 0xDE, 0x02, 0x78, 0xA5, 0xFE, 0x36, 0x01, 
            0xF3, 0x26, 0x85, 0xA5, 0x83, 0x33, 0xD6, 0xDE, 0xF5, 0xDC, 0x29, 0x3F, 0xA1, 0x6E, 0xF2, 0x76, 
            0xAC, 0x09, 0x44, 0x4D, 0xCC, 0x80, 0x10, 0xFF, 0xE1, 0xA2, 0x7F, 0xDC, 0x8B, 0x84, 0xA2, 0x67, 
            0xCC, 0xFA, 0xBF, 0x82, 0x72, 0x09, 0x2A, 0x3C, 0x21, 0xD4, 0xB4, 0x1B, 0xA2, 0x40, 0x56, 0xC7, 
            0x7E, 0x80, 0x17, 0x9D, 0x63, 0x19, 0x02, 0x3D, 0x64, 0x2F, 0x36, 0x69, 0x64, 0xB1, 0x72, 0x3B, 
            0x5C, 0xE5, 0xF4, 0x2E, 0xA1, 0x9F, 0xBD, 0xCE, 0x6A, 0x57, 0x5E, 0xA1, 0x30, 0x3C, 0x1A, 0x35, 
            0xC4, 0x0D, 0x21, 0x15, 0xE0, 0xE5, 0xFF, 0xCD, 0x5C, 0x83, 0x07, 0xB9, 0x3F, 0x79, 0x28, 0xC6, 
            0xAE, 0xF6, 0xC6, 0x18, 0xE8, 0xB5, 0xB8, 0x1F, 0x5B, 0x3A, 0x55, 0xD1, 0x41, 0xAE, 0x35, 0x1F, 
            0x81, 0x05, 0x40, 0x14, 0x11, 0xEB, 0x20, 0x41, 0x59, 0x10, 0x42, 0x1D, 0xEE, 0x17, 0xB8, 0x9D, 
            0x49, 0x65, 0x02, 0x93, 0x9B, 0xFC, 0x36, 0x3A, 0xA9, 0x09, 0x99, 0xD0, 0xD1, 0x91, 0xD0, 0x18, 
            0x58, 0x32, 0x0B, 0xFC, 0x8A, 0xD5, 0xF3, 0x91, 0x6D, 0xDA, 0x12, 0x85, 0xEC, 0x62, 0x01, 0x62, 
            0xBE, 0x9F, 0x8D, 0xEC, 0x1A, 0xEA, 0x5A, 0x5B, 0x10, 0x81, 0x44, 0xE5, 0x50, 0x0C, 0x6F, 0x90, 
            0x30, 0x31, 0x0B, 0xB1, 0x02, 0x1A, 0x21, 0x78, 0x7F, 0xA1, 0x6F, 0x57, 0x31, 0xDE, 0xC4, 0xD6, 
            0x37, 0xF9, 0x16, 0xAC, 0xAC, 0x59, 0x29, 0x93, 0x49, 0x0D, 0x15, 0x33, 0x70, 0xD2, 0xFD, 0x20, 
            0xA2, 0x6D, 0x75, 0x6D, 0xB1, 0xD6, 0xA9, 0x8E, 0x11, 0x56, 0xF7, 0x56, 0xFA, 0x8C, 0x2A, 0xC2, 
            0xC7, 0x78, 0x46, 0x37, 0x07, 0xD2, 0x74, 0xE2, 0x1B, 0x3B, 0x90, 0x19, 0x69, 0x55, 0x8E, 0xA6, 
            0x88, 0x55, 0xEA, 0xF8, 0x32, 0x5A, 0xD5, 0x09, 0x97, 0x03, 0x9B, 0x88, 0x9D, 0x7C, 0x65, 0xCA, 
            0x55, 0xFD, 0xE2, 0xB4, 0x33, 0xBB, 0x9F, 0xC1, 0x1D, 0x6A, 0xE2, 0x2E, 0xC2, 0xB5, 0x90, 0x75, 
            0x62, 0xB6, 0xD3, 0x63, 0x30, 0xDD, 0x63, 0x72, 0x00, 0x46, 0x18, 0x70, 0x6E, 0xDA, 0x58, 0x73, 
            0x9D, 0xDF, 0xFB, 0x92, 0x2E, 0xAF, 0x8E, 0xDF, 0xAF, 0xD5, 0x84, 0xB5, 0x26, 0x7F, 0x01, 0x22, 
            0x9A, 0xDF, 0x4F, 0x4F, 0x29, 0x07, 0x8C, 0x37, 0xC1, 0xB1, 0xDB, 0x29, 0x08, 0x17, 0x36, 0xCF, 
            0x48, 0x28, 0xCB, 0x62, 0xBB, 0x0F, 0x20, 0x71, 0x32, 0xA1, 0xD3, 0xE9, 0x1B, 0xD1, 0x4B, 0xF3, 
            0xE7, 0x72, 0x0B, 0x9E, 0x24, 0xB1, 0xD3, 0x41, 0xD9, 0xFD, 0xEA, 0xC3, 0x14, 0x28, 0x74, 0x7F, 
            0x06, 0xCB, 0xCB, 0x3A, 0x9F, 0x22, 0xFF, 0x14, 0x9A, 0xCC, 0xB4, 0xAD, 0x4E, 0x69, 0x6C, 0xE5, 
            0x60, 0x6D, 0x6D, 0x66, 0x5E, 0xC8, 0x20, 0x42, 0x22, 0x6B, 0x03, 0x2C, 0xEB, 0x59, 0xE3, 0x23
        };
    }
}
