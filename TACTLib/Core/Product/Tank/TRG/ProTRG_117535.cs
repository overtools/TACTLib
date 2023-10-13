using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_117535 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
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
            0x1F, 0x43, 0x36, 0x0D, 0x98, 0x60, 0xD4, 0x56, 0xA1, 0xFB, 0x65, 0xF1, 0xCA, 0x5A, 0x30, 0xF3, 
            0x7A, 0x6A, 0x11, 0xDD, 0x74, 0x6B, 0xB2, 0xBB, 0x22, 0xAB, 0x53, 0xDE, 0xA3, 0x85, 0xD0, 0x0A, 
            0x65, 0xBA, 0x88, 0xB8, 0x54, 0x1E, 0xA6, 0xD0, 0x01, 0x29, 0xAA, 0x1B, 0xD0, 0x6C, 0x7F, 0xAE, 
            0x19, 0x2E, 0x48, 0x43, 0x07, 0x6D, 0x27, 0xAF, 0xF9, 0x09, 0x32, 0x5F, 0x77, 0x44, 0x3D, 0x35, 
            0xB3, 0xFF, 0x53, 0xDD, 0xC7, 0x8D, 0x7D, 0xBE, 0x8C, 0x7D, 0x51, 0xA8, 0xF6, 0x31, 0x56, 0x32, 
            0xD1, 0x1B, 0x84, 0x65, 0x9A, 0x5F, 0x85, 0xF2, 0x73, 0x37, 0x29, 0x07, 0x3E, 0x71, 0xA3, 0x4A, 
            0x43, 0x3E, 0x2D, 0x48, 0xBE, 0x45, 0x64, 0x27, 0xA5, 0x0C, 0xD7, 0xC4, 0x35, 0x6C, 0xE9, 0x7E, 
            0x6C, 0x53, 0xF2, 0xC3, 0x3D, 0xA0, 0x51, 0x25, 0xAC, 0x9D, 0x2E, 0x01, 0x02, 0x83, 0x57, 0x27, 
            0x36, 0x48, 0xDC, 0xE0, 0xBE, 0x42, 0x2B, 0x91, 0x40, 0x0B, 0x22, 0xE9, 0xB8, 0xAB, 0x99, 0x35, 
            0xA1, 0x37, 0xB1, 0x81, 0x38, 0xC3, 0x19, 0x13, 0xB1, 0x6F, 0xFD, 0x79, 0x45, 0x84, 0x8E, 0x67, 
            0x98, 0x90, 0x83, 0xAD, 0x95, 0xE2, 0xCE, 0x7F, 0xA0, 0x23, 0x04, 0xD9, 0x22, 0x1E, 0x7B, 0xC4, 
            0xAA, 0xAC, 0x90, 0xAA, 0x4C, 0xEE, 0xAD, 0x4E, 0xF6, 0x11, 0x72, 0xD9, 0x57, 0xB2, 0xD0, 0x96, 
            0x3E, 0x24, 0x7F, 0x2C, 0x8C, 0xF9, 0x13, 0x39, 0x67, 0x91, 0x69, 0x50, 0xCE, 0xCC, 0x3A, 0xA4, 
            0x4B, 0x43, 0x4D, 0xFA, 0x59, 0x12, 0x82, 0x70, 0x56, 0x2B, 0x46, 0x04, 0x42, 0x54, 0x9B, 0x1F, 
            0xCB, 0x08, 0xAE, 0x5B, 0xEA, 0x2F, 0x7C, 0xE5, 0x02, 0xF2, 0xB3, 0x16, 0x4F, 0xE0, 0x5C, 0x08, 
            0xAB, 0x12, 0xCC, 0xF6, 0xBE, 0x99, 0x1A, 0x1C, 0x99, 0xA8, 0x67, 0xD8, 0x79, 0x86, 0x0C, 0x6B, 
            0x7F, 0xE3, 0x8A, 0x92, 0x1A, 0x88, 0x89, 0xFE, 0xB0, 0x67, 0xAD, 0x2E, 0x95, 0xE4, 0xCE, 0xA1, 
            0xA1, 0x9A, 0x86, 0xD5, 0x01, 0x36, 0x0F, 0x51, 0x2B, 0x15, 0x79, 0x5A, 0xE9, 0xAD, 0x13, 0x30, 
            0x32, 0x9F, 0x58, 0xE5, 0x95, 0x5F, 0x4C, 0x37, 0x20, 0x3D, 0xE9, 0x82, 0xA4, 0x4E, 0xBB, 0x08, 
            0xEE, 0x04, 0xAB, 0xA4, 0x14, 0xB1, 0x14, 0x1A, 0xCA, 0xE7, 0x6B, 0xC5, 0xA2, 0xD1, 0x25, 0xBA, 
            0x81, 0xDE, 0xC3, 0x4F, 0xEF, 0xF0, 0x58, 0x53, 0x29, 0xBC, 0xDE, 0x89, 0x6A, 0xFD, 0x39, 0xD1, 
            0x00, 0x47, 0x76, 0xBA, 0x7A, 0xD6, 0x99, 0xEA, 0x21, 0xBB, 0x63, 0xEC, 0xA0, 0x77, 0x66, 0x4C, 
            0xCE, 0x8D, 0x00, 0xF7, 0xD8, 0xBE, 0xDD, 0xCE, 0x6F, 0xF5, 0xA5, 0x11, 0x95, 0x85, 0x96, 0xBD, 
            0xCD, 0x3C, 0x38, 0x4F, 0xF1, 0x5E, 0xCA, 0x71, 0x31, 0x4E, 0x09, 0xD6, 0x64, 0x17, 0xBA, 0x09, 
            0x75, 0x22, 0x53, 0xA8, 0x5F, 0xCA, 0xA0, 0xC8, 0xE9, 0x9C, 0x1E, 0xE5, 0xFB, 0x9C, 0x95, 0xA7, 
            0xC9, 0xFE, 0xAF, 0x0E, 0x85, 0xD3, 0x0E, 0x53, 0xF6, 0x76, 0xBC, 0x22, 0x5F, 0xE3, 0xC2, 0x75, 
            0x05, 0x38, 0xA8, 0x4F, 0x7E, 0x11, 0x0A, 0x9D, 0xE9, 0x7E, 0x60, 0x4E, 0xCB, 0xCA, 0xB8, 0xCF, 
            0x74, 0x63, 0x25, 0x98, 0x51, 0xE9, 0xBC, 0xAB, 0xF0, 0x86, 0x9D, 0x33, 0xB4, 0x56, 0x57, 0x22, 
            0x15, 0xEC, 0xDB, 0xFE, 0x19, 0x4E, 0x35, 0x92, 0xAB, 0x38, 0x14, 0x68, 0xE1, 0xE8, 0xF3, 0xAF, 
            0x16, 0x05, 0xAF, 0x3C, 0x08, 0xDC, 0x84, 0x64, 0x66, 0xC6, 0x09, 0x9B, 0xB2, 0x86, 0x29, 0xA6, 
            0x3B, 0x23, 0xC6, 0x19, 0xF0, 0x44, 0x49, 0x83, 0x9A, 0x9C, 0xA3, 0x9E, 0xD8, 0x6A, 0xB2, 0xA0, 
            0xC0, 0xA7, 0xB6, 0x89, 0x9D, 0xF6, 0xB4, 0xBC, 0x8E, 0xC7, 0x76, 0x10, 0x65, 0xB2, 0xA9, 0xF9
        };
    }
}