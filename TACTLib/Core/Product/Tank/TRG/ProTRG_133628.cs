using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_133628 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(length * header.m_buildVersion);
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                switch (SignedMod(kidx, 3))
                {
                case 0:
                    kidx += 1273;
                    break;
                case 1:
                    kidx = (uint)SignedMod(kidx * 4, header.m_buildVersion);
                    break;
                case 2:
                    kidx -= 17;
                    break;
                }
            }
            return buffer;
        }

        public byte[] IV(TRGHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(2 * digest[7]);
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
            0xDD, 0x26, 0x31, 0x53, 0x84, 0xC3, 0xE7, 0x3B, 0x85, 0x2C, 0xB4, 0x17, 0x82, 0xB3, 0xB4, 0x3D, 
            0xB3, 0xB7, 0x74, 0x1F, 0x92, 0xEA, 0xC5, 0xB6, 0xA4, 0x73, 0x87, 0x8C, 0x67, 0x1A, 0x3F, 0xD1, 
            0x4F, 0x5B, 0x40, 0x03, 0x83, 0xE8, 0x7F, 0x2F, 0x31, 0x0B, 0xFB, 0xF9, 0xFC, 0x19, 0x77, 0x49, 
            0x5F, 0x24, 0xC2, 0x6E, 0x3D, 0xAA, 0x01, 0x74, 0x67, 0xF2, 0xD4, 0x2E, 0xAE, 0x95, 0xE7, 0xA9, 
            0x05, 0x8B, 0x46, 0x5C, 0x8C, 0x11, 0x60, 0x35, 0xA2, 0x9A, 0x3C, 0x66, 0xE9, 0xDC, 0xEE, 0xB8, 
            0xFA, 0x97, 0xC7, 0x85, 0xDD, 0x64, 0x25, 0x81, 0x12, 0xDA, 0xD2, 0xB9, 0x67, 0xCC, 0xBF, 0xF9, 
            0x58, 0xCD, 0x79, 0xB5, 0x4D, 0x19, 0xCC, 0x45, 0x6E, 0x23, 0xE4, 0xD6, 0xCF, 0x37, 0xBB, 0xC3, 
            0x80, 0x7B, 0xA4, 0x7A, 0xB4, 0x5B, 0x7B, 0x87, 0x26, 0xFE, 0x6C, 0xB4, 0x26, 0xDE, 0x85, 0x18, 
            0x3F, 0x65, 0x5D, 0x51, 0x05, 0x3A, 0x74, 0x8F, 0xFD, 0x7B, 0xE5, 0xC8, 0xBF, 0xC0, 0x39, 0xA8, 
            0xDD, 0x8B, 0xD9, 0xA3, 0x76, 0x6B, 0x0B, 0xAE, 0x38, 0x1B, 0x54, 0x0D, 0x74, 0x4F, 0xEA, 0xBC, 
            0x04, 0x2E, 0x7A, 0x79, 0x7A, 0x20, 0xEA, 0xB3, 0xA6, 0x74, 0xDD, 0xD3, 0x5E, 0x0A, 0xB5, 0x65, 
            0x7C, 0x09, 0x0F, 0x17, 0x9F, 0x11, 0x48, 0x08, 0xF4, 0x77, 0x3C, 0x8A, 0x95, 0xBB, 0x9B, 0x40, 
            0x61, 0x98, 0x96, 0xF2, 0xB6, 0xF8, 0x80, 0x8F, 0x7F, 0x86, 0x4B, 0xE2, 0xC0, 0xE0, 0x55, 0xDB, 
            0x7E, 0x9C, 0x3A, 0xEB, 0xA9, 0x8F, 0xBC, 0x17, 0x72, 0x21, 0x0B, 0x9A, 0x0B, 0xE8, 0x5D, 0x3A, 
            0x15, 0x46, 0xCC, 0xA0, 0x45, 0xBC, 0x8C, 0xEB, 0x77, 0xD0, 0xA6, 0x54, 0xB0, 0x14, 0xFD, 0x2E, 
            0xFA, 0x59, 0x36, 0x2C, 0xEE, 0xC7, 0x4D, 0x8E, 0x10, 0x94, 0xC6, 0x35, 0x0A, 0x6D, 0xC6, 0x3E, 
            0xE3, 0x97, 0x02, 0xA9, 0x4E, 0xB5, 0x59, 0xC0, 0x2C, 0xA0, 0xA5, 0xE1, 0x50, 0xE1, 0x62, 0x2E, 
            0xE0, 0xB8, 0x0B, 0xF5, 0x90, 0xCA, 0x2B, 0x8D, 0xE6, 0x70, 0x6F, 0xDA, 0x37, 0xD4, 0x41, 0xDC, 
            0x45, 0x01, 0x33, 0xB7, 0xF1, 0x53, 0x2B, 0xF7, 0xB2, 0x4E, 0xD2, 0x25, 0xEF, 0xFC, 0xFF, 0x7B, 
            0x97, 0xF9, 0x66, 0xF2, 0x13, 0x49, 0x37, 0x60, 0x32, 0x43, 0x62, 0xAA, 0x67, 0x2E, 0x44, 0x9A, 
            0x5B, 0x98, 0x34, 0xE9, 0x53, 0x26, 0x31, 0x05, 0x08, 0x5D, 0xF2, 0xE5, 0x66, 0x40, 0x2A, 0xB7, 
            0xA9, 0xE3, 0xC0, 0x90, 0xEC, 0xA1, 0x13, 0x8D, 0xCC, 0x04, 0x79, 0x64, 0xBE, 0x7C, 0x96, 0x16, 
            0xB1, 0x81, 0x26, 0x04, 0x31, 0x95, 0x3B, 0xC4, 0xDD, 0x64, 0x58, 0xBF, 0x92, 0x61, 0x3D, 0xB2, 
            0xF0, 0xB0, 0x07, 0xC1, 0x35, 0x6D, 0x7F, 0xA6, 0xFF, 0xEC, 0xFA, 0x1B, 0x5E, 0xF9, 0x55, 0xE8, 
            0x62, 0x38, 0x14, 0xF8, 0x30, 0xC9, 0xEF, 0x85, 0x0E, 0xF5, 0x4B, 0xFD, 0x18, 0x2F, 0x36, 0x4B, 
            0x9F, 0xF7, 0x7E, 0xC5, 0xCF, 0x27, 0x3C, 0x85, 0x77, 0x6E, 0xA3, 0xB0, 0x59, 0xFD, 0x2E, 0x29, 
            0x51, 0xAE, 0xF4, 0x70, 0x8A, 0xAF, 0x11, 0x67, 0xEB, 0x58, 0x3B, 0x70, 0x15, 0x7E, 0x22, 0xA1, 
            0xE1, 0xF2, 0xDA, 0x06, 0xD4, 0x4F, 0x26, 0xC3, 0x66, 0xA8, 0x71, 0xAC, 0xCD, 0x03, 0x32, 0x93, 
            0x47, 0x3D, 0x8A, 0x8F, 0xD8, 0x3F, 0x1E, 0x3E, 0xC9, 0xDB, 0x21, 0xA9, 0x77, 0xCC, 0x48, 0xE5, 
            0x5F, 0xB9, 0x8A, 0x4E, 0xF8, 0xD2, 0x69, 0xCF, 0xD6, 0x79, 0x54, 0x9D, 0xCF, 0xE4, 0xDB, 0x8F, 
            0xF8, 0x85, 0xF4, 0x59, 0x5D, 0x3F, 0x9C, 0xAA, 0xDB, 0xC8, 0x9D, 0x12, 0xB6, 0x21, 0x85, 0xD7, 
            0x82, 0xC4, 0xA5, 0xF9, 0x2A, 0x8D, 0xF9, 0xAB, 0xC2, 0x54, 0x3A, 0x71, 0x4E, 0x1D, 0x50, 0x28
        };
    }
}