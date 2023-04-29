using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_111774 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[length + 256];
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
                kidx = header.m_buildVersion - kidx;
                buffer[i] ^= digest[SignedMod(kidx + i, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x35, 0xB0, 0x92, 0x41, 0x5B, 0xDD, 0x58, 0x1F, 0x99, 0x02, 0xAE, 0x16, 0xCD, 0x20, 0xB8, 0x6E, 
            0xBD, 0xFE, 0x10, 0x32, 0xD3, 0x9D, 0xC1, 0xE5, 0x1F, 0xAD, 0x0C, 0xDC, 0xDF, 0xAA, 0x90, 0xE7, 
            0x2F, 0x66, 0x3F, 0xF3, 0xD8, 0x9A, 0xC3, 0x70, 0x50, 0xB9, 0xB7, 0x19, 0x9E, 0x43, 0x46, 0xF0, 
            0x3D, 0x7B, 0xA0, 0x69, 0xE3, 0x9B, 0x4C, 0xF9, 0x7D, 0x28, 0x2B, 0x7D, 0xAE, 0x4B, 0x9E, 0x93, 
            0x6B, 0x45, 0xAA, 0x3E, 0xF3, 0x66, 0xA5, 0x84, 0x60, 0xAF, 0x5B, 0xA0, 0x85, 0x81, 0x3C, 0xCC, 
            0xAC, 0xE8, 0x14, 0x12, 0xF9, 0x32, 0xE0, 0x43, 0x5F, 0x71, 0x2E, 0x0A, 0x20, 0xE1, 0x74, 0x65, 
            0x58, 0xF5, 0xD1, 0xA6, 0x01, 0x7D, 0x06, 0xD3, 0xB2, 0x58, 0x9E, 0x4D, 0x63, 0xCB, 0x72, 0xA5, 
            0xF5, 0x03, 0x55, 0x9B, 0xD7, 0x87, 0x30, 0x9D, 0x71, 0x48, 0xD6, 0xC3, 0xC8, 0xDF, 0x9D, 0xF3, 
            0x8A, 0xD9, 0xAA, 0xBD, 0x72, 0xB7, 0x80, 0xB9, 0x49, 0x05, 0xB9, 0x51, 0x8F, 0xA9, 0x2A, 0x47, 
            0xD5, 0x70, 0xD3, 0xCF, 0xF1, 0x68, 0x73, 0xC3, 0x3D, 0xDA, 0x68, 0xD3, 0x32, 0xF5, 0x5F, 0xD6, 
            0x4C, 0x52, 0x44, 0x1C, 0x66, 0xD9, 0xFD, 0x70, 0xC2, 0xDB, 0x7C, 0xBC, 0x58, 0x10, 0x75, 0x83, 
            0xC2, 0xFE, 0xAE, 0x42, 0x2D, 0x5C, 0x38, 0x34, 0xB6, 0xAA, 0x41, 0xA8, 0xC5, 0x14, 0xC1, 0x92, 
            0xC0, 0x25, 0x5B, 0xAE, 0x06, 0xFC, 0xC9, 0x16, 0xF3, 0x22, 0xC6, 0xA3, 0x8C, 0x45, 0x2F, 0xA2, 
            0xC9, 0x65, 0x10, 0x7C, 0x91, 0x73, 0x1A, 0xC5, 0x59, 0xBD, 0x28, 0x93, 0x35, 0x64, 0xC3, 0x6A, 
            0x5B, 0x71, 0x70, 0xC3, 0xED, 0xF5, 0x51, 0x53, 0x1D, 0xBA, 0x91, 0x66, 0x45, 0x8F, 0xDD, 0x9D, 
            0xBA, 0x57, 0x52, 0xD0, 0x13, 0xB5, 0xF4, 0x45, 0x37, 0x83, 0x9E, 0x55, 0x9E, 0x98, 0x25, 0x1F, 
            0x1B, 0x71, 0x78, 0x57, 0xB8, 0xA8, 0x76, 0xB3, 0xFC, 0x0F, 0xFF, 0xAB, 0xF7, 0x89, 0xA7, 0x63, 
            0x31, 0x29, 0xDE, 0xBD, 0xFE, 0xF4, 0x4B, 0x0B, 0xDE, 0x57, 0xDB, 0x22, 0x9B, 0x89, 0x0B, 0xD2, 
            0x03, 0x98, 0x0E, 0xFC, 0x83, 0x53, 0x5A, 0x84, 0x62, 0xAE, 0xD2, 0x07, 0xFD, 0x91, 0x17, 0xB4, 
            0x7B, 0x43, 0xEF, 0x7E, 0xC1, 0x45, 0x33, 0x0B, 0xDF, 0x53, 0xF4, 0xC7, 0x33, 0x96, 0xB1, 0xC1, 
            0xBF, 0x18, 0x2C, 0x2E, 0x77, 0x5B, 0x86, 0x81, 0x69, 0x56, 0x48, 0xFD, 0xAA, 0xB8, 0xEA, 0x1B, 
            0xF0, 0xD2, 0xB5, 0x65, 0x0D, 0xDF, 0xA9, 0x74, 0x84, 0xB1, 0x89, 0x04, 0xAB, 0x4C, 0xE0, 0xA4, 
            0x25, 0x40, 0x03, 0x4C, 0x1D, 0x71, 0x35, 0xB7, 0xE1, 0x95, 0x72, 0x6D, 0x9A, 0x9C, 0x71, 0x22, 
            0xC1, 0xE8, 0xF7, 0x6A, 0x4D, 0xB1, 0x18, 0xDF, 0xB9, 0x22, 0x90, 0x87, 0xAA, 0xFE, 0x04, 0xEA, 
            0x23, 0x0E, 0x1E, 0x6C, 0xE8, 0xBC, 0x7F, 0xF2, 0x3F, 0xAE, 0xBA, 0x39, 0xB9, 0xBC, 0x72, 0x67, 
            0x4A, 0x21, 0x37, 0xFC, 0xB0, 0x17, 0xA2, 0xCD, 0x7F, 0x7A, 0xCC, 0xBF, 0x58, 0x2D, 0xEC, 0xC0, 
            0x90, 0x93, 0x3F, 0x07, 0x4B, 0x77, 0x71, 0x57, 0x2F, 0x3D, 0xF7, 0x87, 0x03, 0xD2, 0xCB, 0xAB, 
            0xAD, 0xB4, 0x48, 0x24, 0xDD, 0xEE, 0xB7, 0xC3, 0xE7, 0xF1, 0xEC, 0x59, 0xE9, 0x89, 0xA4, 0x8C, 
            0x9C, 0x86, 0xAF, 0x61, 0xD4, 0xC0, 0xBA, 0x26, 0xB2, 0xF4, 0xCC, 0xE6, 0x42, 0x17, 0x97, 0x22, 
            0x8D, 0x93, 0x2A, 0x8E, 0xE8, 0x33, 0xD4, 0x71, 0x47, 0x98, 0x80, 0xAD, 0x02, 0x35, 0x52, 0x54, 
            0x67, 0x97, 0x7B, 0xFB, 0x66, 0x5B, 0x68, 0x06, 0xCE, 0xA8, 0x6A, 0x40, 0xCA, 0x6E, 0xC1, 0x26, 
            0x12, 0xD2, 0xA5, 0x63, 0xD0, 0x9E, 0xE7, 0x76, 0x95, 0xCE, 0x32, 0x93, 0x51, 0xF9, 0xBA, 0xA0
        };
    }
}