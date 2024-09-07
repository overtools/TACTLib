using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_129218 : ITRGEncryptionProc
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
                kidx += (uint)header.m_packageCount + digest[SignedMod(header.m_packageCount, SHA1_DIGESTSIZE)];
                buffer[i] = digest[SignedMod(kidx, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xCF, 0x9B, 0xD5, 0x85, 0x1B, 0xF2, 0x91, 0xCD, 0x8B, 0x91, 0x3A, 0x69, 0xF7, 0x6A, 0x59, 0x46, 
            0xFC, 0x7F, 0x82, 0x2D, 0x68, 0x46, 0xC4, 0xC5, 0x5D, 0xBE, 0x5B, 0x69, 0x8F, 0xBD, 0x39, 0xCF, 
            0xB7, 0x37, 0xA5, 0x09, 0x7E, 0x29, 0x79, 0x08, 0x6D, 0xB9, 0x64, 0xEC, 0xB1, 0xF8, 0x96, 0xA5, 
            0x12, 0x6C, 0x21, 0xE9, 0x48, 0xDC, 0x08, 0x45, 0x1E, 0x05, 0xC9, 0x13, 0x12, 0x35, 0xF1, 0x3D, 
            0xDE, 0x49, 0x47, 0x82, 0x96, 0x28, 0xAE, 0x2C, 0x8D, 0x9E, 0x5F, 0xB1, 0xA5, 0xB3, 0x43, 0x8B, 
            0x7F, 0x76, 0xF6, 0x39, 0x5D, 0x7A, 0x90, 0x6E, 0xEF, 0x0A, 0x9D, 0x0E, 0xDF, 0x80, 0x0E, 0x15, 
            0xC3, 0x90, 0xDD, 0x3D, 0xDA, 0xEB, 0x7A, 0xD2, 0xF8, 0x67, 0x5B, 0x0A, 0x79, 0x70, 0x6C, 0xAB, 
            0xDF, 0xE2, 0x62, 0xE9, 0x46, 0xA3, 0xFD, 0x23, 0x3F, 0xDA, 0x19, 0xBF, 0x51, 0x5C, 0x13, 0xD7, 
            0x6C, 0xC2, 0x65, 0xD6, 0xA5, 0xEB, 0x98, 0xC2, 0x1A, 0x1F, 0xF1, 0xF6, 0xA8, 0x60, 0x31, 0xC2, 
            0x4E, 0x9B, 0x61, 0x3F, 0xC1, 0x9A, 0x38, 0xEB, 0x8D, 0xAC, 0x0E, 0x90, 0x05, 0x43, 0xEC, 0x43, 
            0x80, 0xED, 0x88, 0x58, 0xCA, 0xEC, 0x40, 0x1E, 0x95, 0x72, 0xA6, 0x4D, 0xB4, 0x8F, 0x1E, 0x82, 
            0xA7, 0xEE, 0xEE, 0x4F, 0xD5, 0x58, 0x24, 0x85, 0x83, 0x81, 0x16, 0x56, 0xE9, 0x7E, 0xA3, 0x60, 
            0x25, 0x80, 0xB2, 0xCF, 0x19, 0x34, 0x59, 0xB3, 0x8A, 0xAE, 0x35, 0xE9, 0xDC, 0xC2, 0x93, 0x8A, 
            0x61, 0x7C, 0x6E, 0x1A, 0x68, 0x4F, 0x94, 0x8C, 0xBE, 0xB5, 0x1C, 0x83, 0xEE, 0x1C, 0x34, 0x1A, 
            0x5B, 0xD7, 0xFB, 0x87, 0x89, 0x50, 0x47, 0x72, 0x56, 0x3C, 0x93, 0xA4, 0xEA, 0xAC, 0xD3, 0xEA, 
            0x8B, 0x93, 0xCE, 0xC4, 0x2A, 0x84, 0xEB, 0x9A, 0x77, 0x58, 0x51, 0x7E, 0xEE, 0x7E, 0x0B, 0x33, 
            0x32, 0x02, 0xBA, 0x54, 0xEA, 0x5F, 0x87, 0x4B, 0x18, 0xA4, 0xBB, 0x36, 0xE0, 0xCE, 0xDE, 0xD9, 
            0x4D, 0x7C, 0xF3, 0xF7, 0x88, 0x6F, 0xD2, 0x64, 0xF4, 0x8E, 0xA4, 0x37, 0xFB, 0x22, 0x4A, 0x85, 
            0xAF, 0xB1, 0xDC, 0x09, 0x12, 0x4F, 0xCE, 0xF3, 0x93, 0x9C, 0x79, 0x8E, 0xCB, 0x25, 0x4E, 0x66, 
            0xD4, 0x41, 0x2D, 0x09, 0x05, 0x72, 0x12, 0x18, 0x51, 0xF3, 0xBE, 0xEC, 0x7A, 0x9C, 0x55, 0x59, 
            0xE4, 0xF6, 0x8E, 0x1F, 0xBB, 0x0D, 0x03, 0x19, 0xFB, 0x1A, 0xEA, 0x8A, 0xC6, 0x8B, 0xF2, 0x4B, 
            0x5F, 0xEF, 0xED, 0x74, 0x2E, 0x28, 0x50, 0x33, 0x58, 0xE2, 0xBB, 0x40, 0x9D, 0xA2, 0x6C, 0x54, 
            0xA8, 0x6D, 0x83, 0xCA, 0x4C, 0x00, 0x3E, 0x97, 0x84, 0x6E, 0x7F, 0x95, 0xBB, 0xAE, 0x04, 0x8F, 
            0x8C, 0x07, 0x9E, 0x98, 0xED, 0x8D, 0xA5, 0xBC, 0x18, 0xC5, 0x76, 0x71, 0x35, 0xE1, 0x0A, 0xE0, 
            0x6F, 0xFB, 0x30, 0xDD, 0xDC, 0x8A, 0xF7, 0x66, 0xEA, 0x51, 0x20, 0xEF, 0xA5, 0xBD, 0xF1, 0x8D, 
            0x9D, 0xE9, 0x4B, 0x12, 0xE0, 0x36, 0x52, 0x17, 0x59, 0x94, 0x72, 0x03, 0x67, 0xB6, 0x52, 0x09, 
            0xFB, 0xEE, 0x4E, 0x11, 0x38, 0xC5, 0x33, 0xC1, 0xE7, 0x6F, 0x05, 0x05, 0x3C, 0x17, 0x57, 0x7C, 
            0xBA, 0x3D, 0xF1, 0xD0, 0xF5, 0xE7, 0x42, 0x6E, 0x33, 0xDB, 0xF3, 0xAE, 0x34, 0x88, 0x58, 0x94, 
            0x72, 0xBD, 0x04, 0xD1, 0xEE, 0x9C, 0xE9, 0x72, 0x41, 0xA0, 0x69, 0x9E, 0xD0, 0x41, 0x6D, 0x45, 
            0xB2, 0x2B, 0xF2, 0x7E, 0x26, 0x9D, 0x60, 0x1E, 0xF2, 0xF8, 0x68, 0x00, 0xE4, 0xC1, 0x54, 0x42, 
            0x95, 0x59, 0xC3, 0x12, 0x74, 0xF9, 0x96, 0xC6, 0xA6, 0xDC, 0x99, 0x9D, 0xA2, 0x15, 0x9C, 0x54, 
            0x57, 0x98, 0x55, 0x60, 0x16, 0xC0, 0x78, 0x4E, 0x54, 0xB6, 0x49, 0xCE, 0x04, 0x73, 0x82, 0xF2
        };
    }
}