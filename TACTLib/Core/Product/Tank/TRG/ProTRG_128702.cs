using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_128702 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[length + 256];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += 3;
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
                kidx -= header.m_buildVersion & 511;
                buffer[i] ^= digest[SignedMod(kidx + header.m_buildVersion, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xB9, 0xC4, 0x06, 0xA1, 0x5E, 0xC4, 0x25, 0x93, 0xAB, 0x04, 0xE5, 0xFA, 0x41, 0x10, 0x4E, 0xD9, 
            0xD3, 0x6D, 0x10, 0xDA, 0x9C, 0x16, 0x7D, 0x33, 0x90, 0x8C, 0x55, 0x73, 0x4A, 0xA5, 0xB6, 0xFB, 
            0x09, 0xB2, 0xD9, 0x31, 0x14, 0x6B, 0x11, 0x00, 0x7B, 0x62, 0xC0, 0xE1, 0x83, 0xC8, 0x1A, 0x41, 
            0xA7, 0x58, 0xE1, 0x10, 0xF0, 0x89, 0xC8, 0x90, 0xA4, 0x02, 0xF2, 0x24, 0xBB, 0xB9, 0x2C, 0x9D, 
            0x64, 0x06, 0xF3, 0xC1, 0x07, 0xA8, 0x16, 0x90, 0x63, 0x3D, 0x3C, 0x68, 0x85, 0xD4, 0x84, 0x62, 
            0x55, 0xA3, 0x42, 0x5E, 0xE6, 0xAA, 0xFD, 0xF6, 0x76, 0xDF, 0xE1, 0x1B, 0xED, 0xB9, 0xB6, 0x28, 
            0xDD, 0xA3, 0x81, 0x47, 0x12, 0x53, 0x66, 0xD2, 0x3C, 0x4F, 0x99, 0xB8, 0x97, 0x85, 0x0C, 0x51, 
            0x53, 0x47, 0x36, 0x59, 0x9C, 0xD2, 0x7D, 0x73, 0x34, 0xBD, 0x78, 0x0F, 0xE1, 0xA8, 0x3B, 0x24, 
            0xAD, 0xE4, 0xFE, 0x0B, 0x21, 0x98, 0x90, 0x6D, 0x6D, 0x55, 0xCC, 0xAB, 0xB5, 0xE0, 0x06, 0x4E, 
            0x3F, 0x1D, 0x72, 0x46, 0xA6, 0x0E, 0xC6, 0x2D, 0x5F, 0xF5, 0x95, 0xC6, 0x01, 0xC0, 0x89, 0xE4, 
            0x45, 0x36, 0xF4, 0x2C, 0x78, 0x8C, 0x0E, 0xFA, 0xF4, 0xD8, 0x28, 0xB8, 0xA3, 0x16, 0x29, 0xB8, 
            0xC5, 0x75, 0x29, 0xF7, 0x2A, 0xF6, 0x1C, 0xA5, 0xCE, 0x57, 0xCC, 0xA4, 0x3B, 0x5D, 0x1F, 0xCE, 
            0xB1, 0x10, 0x7C, 0xBC, 0x47, 0x8E, 0xE3, 0x8A, 0xFE, 0x10, 0x51, 0x5C, 0x13, 0x6B, 0xB3, 0x52, 
            0xCD, 0x1D, 0xB5, 0x7C, 0xB5, 0xC4, 0x75, 0xC9, 0x63, 0x6F, 0x2A, 0x51, 0xF9, 0x62, 0xF5, 0x96, 
            0xC8, 0x7C, 0xC3, 0xAF, 0xAC, 0x31, 0x3E, 0x53, 0xAD, 0xC8, 0xD1, 0xFD, 0x0A, 0xB8, 0x1B, 0x9B, 
            0xC2, 0xE7, 0xFC, 0x8F, 0xB9, 0xA3, 0x43, 0x8E, 0xEE, 0x90, 0xC5, 0x24, 0x55, 0xF6, 0x7A, 0x22, 
            0x7D, 0x8A, 0x95, 0x3E, 0xEE, 0xC9, 0x20, 0xB7, 0xF8, 0x3C, 0x7F, 0x49, 0xBC, 0x94, 0x27, 0xE0, 
            0x3B, 0x1F, 0x5B, 0x40, 0x81, 0x37, 0x99, 0xE5, 0xEE, 0x7D, 0x6F, 0xED, 0xE9, 0xCD, 0x03, 0x66, 
            0x0A, 0xBC, 0xD8, 0xCE, 0xD0, 0xF9, 0xE9, 0x03, 0xDC, 0x01, 0x38, 0xE8, 0x3A, 0xB1, 0xBA, 0xE2, 
            0xE6, 0xEB, 0xD9, 0xB1, 0xA3, 0xB4, 0x04, 0x55, 0x59, 0xAF, 0x8B, 0x78, 0x0E, 0x37, 0xBF, 0x67, 
            0xA0, 0x02, 0xC3, 0x38, 0x19, 0x32, 0xD1, 0x54, 0x50, 0x2E, 0xCB, 0xF0, 0x1F, 0x5F, 0xC5, 0x72, 
            0x67, 0xEC, 0x51, 0xD4, 0xB3, 0x9A, 0x09, 0xA7, 0x5D, 0xD6, 0x95, 0x8A, 0x3D, 0xF5, 0x9C, 0x82, 
            0xBF, 0xE7, 0x5F, 0xF6, 0xE0, 0x61, 0x2D, 0x95, 0x04, 0x31, 0x84, 0x19, 0xC3, 0xAA, 0xC7, 0xEF, 
            0x57, 0xD7, 0x97, 0x27, 0x4C, 0xA6, 0xAF, 0x90, 0x9B, 0x63, 0xD5, 0x95, 0xFF, 0x8C, 0x23, 0xE2, 
            0x4E, 0x53, 0x61, 0xE4, 0x3A, 0x3E, 0x16, 0xFC, 0xC8, 0xF2, 0xE1, 0xE6, 0x79, 0x49, 0x9B, 0x8A, 
            0x39, 0x74, 0xD0, 0xFF, 0x08, 0x77, 0xA3, 0xCD, 0x92, 0xCD, 0x35, 0x21, 0x0D, 0x8E, 0xC5, 0x80, 
            0x27, 0xD6, 0xC5, 0x6F, 0xDB, 0xE5, 0x77, 0x81, 0x71, 0x76, 0xFB, 0x5C, 0x3A, 0xAF, 0x4C, 0xFC, 
            0x42, 0x79, 0xCE, 0x7F, 0x60, 0xAC, 0x45, 0x86, 0x3C, 0x32, 0xBC, 0xFF, 0xED, 0xDD, 0x38, 0xC3, 
            0x1E, 0x0F, 0xD9, 0xE1, 0xDE, 0xA0, 0x10, 0xA1, 0x2A, 0x1D, 0xFC, 0x79, 0x94, 0xD6, 0x9B, 0xE1, 
            0x54, 0xD1, 0x61, 0x12, 0x9C, 0xF5, 0x75, 0xCF, 0x03, 0x81, 0x47, 0x73, 0x0A, 0xC0, 0x60, 0x33, 
            0xC2, 0x45, 0xA3, 0xF9, 0x46, 0x2A, 0x32, 0xE6, 0x89, 0xCF, 0xAA, 0x2A, 0x01, 0xCF, 0xB5, 0xD3, 
            0x88, 0x72, 0x4D, 0x72, 0x93, 0xB4, 0x7F, 0x60, 0xC9, 0xDA, 0x1B, 0xAD, 0x52, 0x0E, 0x08, 0xB0
        };
    }
}