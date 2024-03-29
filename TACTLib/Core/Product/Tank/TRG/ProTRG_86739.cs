using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_86739 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[length + 256];
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
            kidx = okidx = Keytable[header.m_skinCount & 511];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += (header.m_buildVersion * (uint)header.m_skinCount) % 7;
                buffer[i] ^= digest[SignedMod(kidx - 73, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xB8, 0xD6, 0x31, 0x12, 0x82, 0xD8, 0x3C, 0x78, 0x1C, 0xB1, 0x14, 0x13, 0xBB, 0xCC, 0xFD, 0x96, 
            0x25, 0x69, 0x9A, 0x6A, 0x0A, 0xB4, 0x2F, 0x55, 0x49, 0x95, 0x27, 0xDB, 0x3D, 0x06, 0x70, 0xAF, 
            0xDB, 0xFC, 0x56, 0x81, 0x9E, 0xFA, 0xA2, 0x6F, 0xB4, 0xE8, 0x4E, 0xE6, 0xAC, 0x96, 0xD5, 0x76, 
            0x17, 0x09, 0x6E, 0xDF, 0x05, 0x8A, 0xE2, 0x3F, 0x1F, 0x99, 0x45, 0x76, 0xE2, 0x48, 0x7B, 0x3F, 
            0xAA, 0x00, 0x16, 0xA0, 0x1D, 0x99, 0xED, 0x54, 0x9E, 0xCA, 0xE0, 0x7E, 0xBE, 0xFA, 0x83, 0xA2, 
            0x1E, 0xA7, 0x4D, 0x39, 0xF7, 0x3D, 0xCF, 0x8E, 0x35, 0x51, 0x2E, 0xF1, 0x0F, 0xDE, 0xF7, 0x33, 
            0x56, 0x2F, 0xD0, 0xB8, 0x52, 0xDC, 0x53, 0x20, 0x3F, 0x23, 0x0C, 0xA0, 0xE4, 0x36, 0x32, 0x37, 
            0xBB, 0xF7, 0xE0, 0x97, 0x62, 0x99, 0x88, 0x83, 0xD0, 0x35, 0x57, 0x60, 0xB2, 0xAE, 0xA0, 0x4A, 
            0xB4, 0x16, 0x7B, 0x33, 0x7C, 0x9F, 0x7C, 0xAB, 0xA9, 0x59, 0xA6, 0xB5, 0x68, 0x0E, 0xAF, 0x21, 
            0x71, 0xB5, 0xFF, 0x61, 0x0E, 0x27, 0xA9, 0x6D, 0xF2, 0x36, 0xA2, 0x79, 0x74, 0xC2, 0x04, 0xE2, 
            0xFE, 0xBE, 0x02, 0x17, 0xA7, 0x7F, 0x7A, 0x18, 0xDE, 0xB0, 0xEE, 0x5B, 0x4A, 0xF1, 0x77, 0x82, 
            0x00, 0x4F, 0xD7, 0x06, 0xC7, 0x1D, 0x19, 0x62, 0xE4, 0x5E, 0x6E, 0xB5, 0xC9, 0x03, 0x28, 0x5B, 
            0x6C, 0xC6, 0x00, 0x4A, 0x75, 0xA0, 0x39, 0x4B, 0x05, 0xB7, 0xEB, 0x17, 0x6F, 0x79, 0x2E, 0x3B, 
            0x52, 0x43, 0x7F, 0x6A, 0x2D, 0x09, 0x10, 0x0A, 0x6A, 0xBE, 0xAF, 0x88, 0xFD, 0x68, 0x05, 0x6E, 
            0xEF, 0xB0, 0x13, 0xAD, 0xA6, 0xF6, 0x31, 0xA9, 0xCE, 0x51, 0xEA, 0xBD, 0x4E, 0x81, 0x43, 0x9D, 
            0xB5, 0xEF, 0xD6, 0xEF, 0xB5, 0x42, 0x2B, 0x5F, 0xFD, 0x19, 0xF5, 0xFC, 0x9C, 0xB0, 0xEA, 0x59, 
            0x0B, 0x2C, 0xA0, 0x6F, 0x89, 0x96, 0x95, 0xBE, 0x7F, 0xA3, 0x92, 0x36, 0xA1, 0x3D, 0x30, 0xFC, 
            0xC1, 0x31, 0x4A, 0x19, 0xB3, 0x98, 0xF3, 0x7B, 0xF0, 0x7E, 0x15, 0x0D, 0x01, 0xE0, 0x4D, 0x6A, 
            0xE4, 0x82, 0x66, 0x4E, 0x48, 0xB0, 0x49, 0x0A, 0xB2, 0x45, 0xAA, 0xCD, 0xD1, 0xA3, 0xE9, 0x29, 
            0xA0, 0x48, 0x84, 0xE4, 0xDB, 0xB9, 0x10, 0x22, 0x7D, 0x19, 0x77, 0x91, 0x56, 0xF7, 0x2C, 0xF1, 
            0x9A, 0xB0, 0x2F, 0xAE, 0xBF, 0x79, 0x98, 0xDC, 0x4B, 0xA0, 0xAF, 0x4C, 0x73, 0x49, 0x63, 0x2A, 
            0xC2, 0x34, 0xE8, 0x0E, 0xD0, 0xBA, 0x3C, 0x95, 0x29, 0x97, 0x13, 0x35, 0x81, 0x21, 0x3B, 0x22, 
            0xAF, 0x76, 0x3D, 0x37, 0x89, 0x9E, 0xB0, 0x1F, 0xBC, 0x26, 0xE2, 0x44, 0x07, 0x11, 0x0B, 0xF0, 
            0xF3, 0x31, 0x18, 0x91, 0x75, 0x59, 0xA2, 0xB0, 0x0A, 0x34, 0xC0, 0x50, 0xC1, 0x06, 0xA6, 0xC3, 
            0x32, 0xEA, 0xA8, 0xC1, 0x37, 0xED, 0xCE, 0x4F, 0x59, 0xDB, 0x8F, 0x8B, 0xBB, 0x1C, 0xD0, 0x0F, 
            0x35, 0x0B, 0x63, 0x3B, 0x87, 0x3F, 0x39, 0xB4, 0xF8, 0xF7, 0x47, 0x34, 0x3B, 0x74, 0x5D, 0xBD, 
            0x9A, 0x89, 0xCD, 0xB1, 0x33, 0x39, 0x4F, 0x45, 0x08, 0x6C, 0x01, 0x6C, 0xBB, 0x79, 0x7F, 0xB0, 
            0xFF, 0x54, 0xA5, 0xBE, 0x97, 0xC1, 0xF3, 0xBB, 0x13, 0xE3, 0x53, 0x48, 0x28, 0xBE, 0x08, 0xE0, 
            0x04, 0x96, 0x56, 0xE6, 0x44, 0xF3, 0x46, 0x0B, 0x50, 0x1D, 0xED, 0xF6, 0x74, 0xB5, 0xF9, 0x3C, 
            0xC3, 0xB2, 0x5B, 0x77, 0x66, 0xF4, 0x3C, 0xDB, 0xDE, 0x80, 0x45, 0x14, 0x84, 0xA7, 0x84, 0x0B, 
            0x05, 0x1E, 0xF9, 0x6C, 0x7F, 0x15, 0xF8, 0x5A, 0x7A, 0xAA, 0xC6, 0xA4, 0x8B, 0x69, 0x85, 0x4D, 
            0x4E, 0x12, 0x7D, 0xC8, 0x68, 0xFE, 0x2F, 0xA7, 0xCB, 0x18, 0x98, 0x53, 0x5C, 0x00, 0x55, 0xFF
        };
    }
}
