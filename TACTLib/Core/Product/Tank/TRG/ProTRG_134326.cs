using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_134326 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_skinCount & 511];
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
                switch (SignedMod(kidx, 3))
                {
                case 0:
                    kidx += 341;
                    break;
                case 1:
                    kidx = (uint)SignedMod(kidx * 7, header.m_buildVersion);
                    break;
                case 2:
                    kidx -= 13;
                    break;
                }
                buffer[i] ^= digest[SignedMod(kidx + header.m_buildVersion, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xFE, 0x68, 0x74, 0x66, 0x97, 0x6D, 0x16, 0x38, 0x5D, 0x9B, 0xFE, 0x81, 0xDA, 0x84, 0x9E, 0x89, 
            0x5D, 0x4B, 0x44, 0x2B, 0x9C, 0x89, 0xE2, 0x6C, 0xD3, 0x4B, 0x95, 0x0D, 0xA0, 0x91, 0x39, 0x2E, 
            0xC1, 0x4A, 0xB8, 0x1A, 0xE9, 0x9B, 0xCE, 0xAB, 0x87, 0xB2, 0x8E, 0xCA, 0x82, 0x82, 0xCD, 0xDA, 
            0x34, 0x94, 0x25, 0xBA, 0x8D, 0x38, 0x9F, 0xE3, 0x73, 0xF0, 0xC8, 0x19, 0x08, 0xB7, 0xE2, 0xF7, 
            0x48, 0x49, 0x76, 0x1D, 0x24, 0xDB, 0x91, 0x3A, 0x43, 0xC9, 0x6F, 0x83, 0x37, 0xA6, 0x69, 0xFB, 
            0x99, 0x38, 0x31, 0xD5, 0x1B, 0x30, 0x8E, 0x85, 0xEB, 0x4D, 0x4B, 0xC3, 0xCA, 0xAA, 0x0A, 0xAB, 
            0x67, 0x74, 0x00, 0x87, 0xF0, 0x09, 0xCF, 0xB9, 0x66, 0xD0, 0x8B, 0x27, 0xE1, 0xBB, 0x59, 0x73, 
            0xD9, 0x51, 0x53, 0x81, 0xC6, 0x89, 0xF8, 0x15, 0x32, 0x1A, 0x90, 0x1F, 0xB5, 0xCD, 0xD1, 0x52, 
            0x06, 0x1A, 0xED, 0x58, 0xB1, 0x05, 0x1A, 0x09, 0x08, 0x7A, 0xAD, 0x55, 0x10, 0x88, 0xC2, 0x00, 
            0x82, 0xB2, 0xB7, 0x1E, 0x13, 0xF8, 0xA9, 0x08, 0x28, 0x79, 0x57, 0x4E, 0x88, 0xF3, 0x36, 0x3A, 
            0x1D, 0x24, 0xBA, 0x23, 0x58, 0x0D, 0xA1, 0xDB, 0x76, 0x6A, 0x23, 0x1A, 0x22, 0xA6, 0x54, 0xC8, 
            0x20, 0x08, 0x0D, 0x4E, 0xAB, 0xCC, 0x28, 0x36, 0x42, 0xFC, 0xE9, 0x56, 0x3A, 0xAA, 0x08, 0xD6, 
            0x82, 0x01, 0x67, 0x85, 0x40, 0xD2, 0x1C, 0x87, 0xD8, 0x58, 0xD0, 0x20, 0x72, 0xB8, 0x07, 0xC3, 
            0x71, 0x5C, 0x29, 0x1C, 0xD0, 0xEA, 0x95, 0x66, 0xF9, 0xC0, 0x59, 0xDC, 0x2A, 0x51, 0xF1, 0x07, 
            0xFC, 0x54, 0x09, 0xD6, 0x63, 0x72, 0xE9, 0xF1, 0xB5, 0x54, 0x31, 0xB4, 0x14, 0x1E, 0x84, 0xC0, 
            0xC6, 0x78, 0x45, 0x2F, 0x96, 0x89, 0xB0, 0x04, 0x8A, 0x37, 0xC2, 0xCD, 0xAA, 0xD2, 0x45, 0xB2, 
            0x93, 0x2E, 0x09, 0x97, 0x0D, 0x08, 0x3F, 0xD2, 0xC6, 0x41, 0xCB, 0x34, 0xEE, 0x3D, 0xC7, 0x96, 
            0xED, 0x28, 0x3D, 0xE7, 0x72, 0x56, 0x42, 0xF9, 0x62, 0xD2, 0x14, 0xC4, 0xC6, 0xE4, 0x31, 0x7C, 
            0x38, 0x62, 0xCC, 0xBF, 0xE8, 0x6C, 0xD7, 0x2B, 0xC9, 0xC0, 0xA8, 0x71, 0xF3, 0x32, 0x61, 0x42, 
            0x02, 0x67, 0x13, 0xBC, 0x78, 0x35, 0xAB, 0xE8, 0x24, 0x18, 0xFA, 0x14, 0xA9, 0xBB, 0x56, 0xF9, 
            0xC9, 0xEC, 0xF5, 0x7A, 0xF1, 0x49, 0x3E, 0xA6, 0xEB, 0x6E, 0xC1, 0x57, 0x30, 0x12, 0x11, 0xDC, 
            0x57, 0xFF, 0xD9, 0x06, 0x51, 0x2E, 0xA2, 0x15, 0x8F, 0x9A, 0x78, 0x1D, 0xCB, 0x20, 0x0B, 0x7E, 
            0xF5, 0x51, 0xCB, 0x9B, 0x11, 0x90, 0x71, 0x35, 0xA5, 0x87, 0x1A, 0x9C, 0xE1, 0x0F, 0xE0, 0x16, 
            0x55, 0x19, 0x18, 0x04, 0xE2, 0x88, 0x97, 0x22, 0x80, 0x69, 0x04, 0x66, 0xCE, 0xC2, 0x98, 0x67, 
            0x0D, 0xCC, 0x46, 0x43, 0x4E, 0x76, 0xA3, 0x70, 0xDA, 0x24, 0xE6, 0x9A, 0xFE, 0x21, 0xBA, 0x44, 
            0x19, 0x13, 0x02, 0xF3, 0x49, 0x2F, 0xE1, 0x02, 0x3A, 0x5E, 0x25, 0x80, 0x07, 0xE5, 0x2F, 0xE5, 
            0xA2, 0x16, 0x52, 0x8E, 0x56, 0x31, 0x0D, 0xD4, 0x29, 0xC7, 0xF7, 0x20, 0xB5, 0xB9, 0xC1, 0xDF, 
            0x6D, 0xE1, 0x33, 0x74, 0xFB, 0x48, 0xA6, 0x39, 0x18, 0xBA, 0x3A, 0x84, 0x5C, 0xBD, 0xD1, 0xD4, 
            0x1A, 0x8D, 0xB6, 0xBD, 0x6D, 0x9E, 0x38, 0x15, 0xBF, 0xD2, 0xC2, 0x85, 0x1B, 0x49, 0xEE, 0x2C, 
            0x56, 0xA2, 0x04, 0x5B, 0x00, 0xBD, 0x6A, 0x0B, 0x9C, 0x18, 0x87, 0xC6, 0x4F, 0xC5, 0xDD, 0x90, 
            0x5B, 0x70, 0x97, 0x77, 0x83, 0x48, 0x95, 0x6C, 0x83, 0xF3, 0xAB, 0x11, 0x30, 0xF2, 0x7C, 0x4C, 
            0x19, 0x18, 0x1E, 0xE7, 0xE5, 0xE9, 0x8A, 0xF9, 0x47, 0x03, 0x8B, 0x57, 0x16, 0x1F, 0x5C, 0xAB
        };
    }
}