using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_69939 : ITRGEncryptionProc
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
                kidx += (uint)header.m_packageCount + digest[SignedMod(header.m_packageCount, SHA1_DIGESTSIZE)];
                buffer[i] = digest[SignedMod(kidx, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x74, 0x7C, 0x6C, 0xB9, 0x79, 0x86, 0x32, 0x73, 0x14, 0x0E, 0xBB, 0x52, 0xD2, 0x2B, 0x9B, 0x40, 
            0x8A, 0xAE, 0x83, 0x13, 0x70, 0xA4, 0xC9, 0x1E, 0xDE, 0x96, 0x54, 0xDD, 0x54, 0x66, 0xBD, 0x5A, 
            0x3B, 0x10, 0xBB, 0x47, 0xA1, 0x0F, 0x06, 0x18, 0x48, 0x73, 0xF7, 0xEE, 0xFB, 0x6C, 0xDB, 0xB1, 
            0x7A, 0xB1, 0xFA, 0x0A, 0xC8, 0x3E, 0x58, 0x5F, 0x11, 0xB4, 0x09, 0x79, 0x6E, 0xF4, 0xF5, 0x09, 
            0x58, 0xE7, 0x8E, 0xB1, 0x86, 0x09, 0x24, 0x8A, 0x6A, 0xBC, 0x1E, 0x9D, 0x52, 0x03, 0x5F, 0x5D, 
            0x0F, 0x1E, 0xA8, 0x2C, 0x38, 0x02, 0x5D, 0xC8, 0x75, 0x35, 0x8F, 0x27, 0xBB, 0x29, 0x05, 0x94, 
            0xE4, 0x1E, 0xCB, 0xB4, 0xB3, 0x3E, 0xAD, 0x3A, 0x76, 0xD8, 0xFD, 0x2A, 0xBF, 0x6F, 0xB7, 0x42, 
            0x66, 0xFC, 0x6E, 0xB1, 0xDD, 0xCC, 0xE1, 0x52, 0x7A, 0xA2, 0xC2, 0x63, 0xB9, 0x36, 0x8F, 0x9A, 
            0xD7, 0x5B, 0x36, 0x2D, 0xBC, 0x57, 0xCC, 0x44, 0x4A, 0x57, 0xBA, 0x45, 0x1D, 0x8F, 0x57, 0x22, 
            0x7F, 0x27, 0x5B, 0x1A, 0xEE, 0xD6, 0xA7, 0x4E, 0x69, 0xCD, 0x23, 0xA7, 0xFD, 0x8C, 0x79, 0xC3, 
            0xB5, 0x5F, 0x75, 0x4C, 0xC1, 0x0F, 0x7C, 0xCB, 0xE7, 0xBA, 0x6B, 0xF6, 0x65, 0x38, 0x0C, 0x4D, 
            0x5D, 0x4F, 0xA9, 0x0A, 0x98, 0x9D, 0xBB, 0x01, 0x4D, 0xF6, 0xF8, 0xB0, 0x9E, 0x41, 0x36, 0x17, 
            0x4D, 0xD7, 0x44, 0x8F, 0xB3, 0x39, 0xF8, 0xE2, 0x52, 0x24, 0x14, 0x95, 0x3C, 0x44, 0x5A, 0x82, 
            0x5B, 0xA6, 0xD4, 0x8D, 0xB3, 0x1E, 0xC4, 0x52, 0x02, 0x26, 0xF0, 0xD7, 0xF8, 0x5A, 0x8C, 0xF9, 
            0xA8, 0x55, 0x81, 0x25, 0x1F, 0x6B, 0xFA, 0x7B, 0xB7, 0x11, 0x39, 0xF6, 0x18, 0xAD, 0x21, 0x94, 
            0x4C, 0x38, 0xE8, 0x09, 0xD4, 0xF4, 0x29, 0x28, 0x58, 0x9D, 0x3B, 0x13, 0x28, 0x5A, 0xB2, 0xAD, 
            0x33, 0x73, 0x56, 0x3A, 0x0D, 0x6A, 0x20, 0x79, 0xBC, 0x5F, 0x7D, 0xB9, 0xFE, 0xB1, 0x2E, 0x2B, 
            0xB2, 0x54, 0xA3, 0xF3, 0x54, 0xD9, 0x1F, 0x4E, 0xD1, 0x7A, 0x4A, 0x34, 0x2E, 0x72, 0xF6, 0xC0, 
            0xF1, 0xD0, 0x5F, 0x79, 0x68, 0xA6, 0xFA, 0x5C, 0x99, 0xB1, 0xF5, 0x85, 0xDE, 0xC6, 0xA1, 0xF6, 
            0x3E, 0x2D, 0x5E, 0xFE, 0x7F, 0x27, 0xA6, 0xF8, 0x00, 0x7F, 0x36, 0x20, 0xA6, 0xBC, 0x34, 0x21, 
            0xBD, 0xBA, 0xFD, 0x0E, 0x8A, 0x00, 0x41, 0x82, 0xA3, 0x46, 0xA4, 0x9B, 0xDC, 0x88, 0x4A, 0xD4, 
            0x97, 0x5F, 0xA4, 0xF7, 0xF3, 0xD9, 0x17, 0x51, 0x49, 0xC9, 0xB1, 0x01, 0x08, 0xC3, 0xE7, 0x08, 
            0x5F, 0x53, 0x53, 0xFA, 0x9F, 0x1E, 0x9B, 0xFA, 0x5C, 0x58, 0x08, 0x75, 0x33, 0xC2, 0x33, 0x97, 
            0xEF, 0x03, 0x4F, 0x4C, 0x67, 0xDC, 0x07, 0xCE, 0x36, 0xFD, 0x49, 0x03, 0x22, 0x21, 0x9D, 0x54, 
            0x53, 0xA0, 0x28, 0x4B, 0xB7, 0xC8, 0x7F, 0x46, 0x04, 0x67, 0x89, 0xBD, 0x94, 0xFC, 0xD0, 0x18, 
            0x7C, 0xB6, 0x0A, 0xC0, 0x6E, 0xA9, 0xF1, 0xEE, 0xB9, 0x60, 0x65, 0x03, 0x8E, 0xAF, 0x06, 0x03, 
            0x6D, 0x11, 0xA3, 0x01, 0xFF, 0xE0, 0x10, 0x46, 0x10, 0xF9, 0xF3, 0xA7, 0x4F, 0xFD, 0xD2, 0x42, 
            0x9B, 0xF2, 0x8C, 0xFB, 0xAD, 0xA6, 0xE0, 0x06, 0xF9, 0x8E, 0x0E, 0xD7, 0x92, 0xF3, 0x70, 0x00, 
            0xEB, 0xC1, 0x60, 0x8E, 0x86, 0xCB, 0x8D, 0xB9, 0x96, 0xF4, 0x78, 0x0A, 0xD6, 0x77, 0x98, 0x6F, 
            0x8C, 0xE5, 0xAF, 0xA1, 0x5B, 0x4B, 0x4C, 0x46, 0x15, 0xF3, 0x2A, 0x1A, 0x80, 0xEC, 0x7E, 0xCE, 
            0x62, 0x2A, 0xF3, 0xAF, 0xA9, 0xFD, 0xBB, 0x80, 0x88, 0x0C, 0xAB, 0xA5, 0x96, 0xCD, 0x88, 0x92, 
            0x9F, 0x08, 0x26, 0x6C, 0xAC, 0x98, 0x96, 0xC8, 0xB2, 0x7A, 0xC4, 0xE7, 0x4C, 0x08, 0x13, 0x93
        };
    }
}