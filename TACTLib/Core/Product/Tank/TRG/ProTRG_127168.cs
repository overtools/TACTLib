using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_127168 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(length * header.m_buildVersion);
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 61;
            }
            return buffer;
        }

        public byte[] IV(TRGHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(digest[5] + header.m_skinCount) & 511;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 29;
                buffer[i] ^= (byte)(digest[SignedMod(kidx + header.m_packageCount, SHA1_DIGESTSIZE)] + 1);
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xAA, 0x4F, 0xEF, 0xC8, 0xFB, 0xCF, 0x64, 0x3D, 0xAD, 0xAB, 0x9D, 0xBE, 0x01, 0xE2, 0xC2, 0xB7, 
            0xB5, 0x19, 0x93, 0xB8, 0xCF, 0x50, 0x9D, 0x5D, 0x90, 0x62, 0x7B, 0x5E, 0x48, 0x95, 0xB7, 0xB8, 
            0x8E, 0x2D, 0x68, 0x04, 0x2A, 0x68, 0x34, 0x75, 0xAF, 0xF4, 0xB8, 0xC7, 0xE2, 0x52, 0xE7, 0xD2, 
            0xCD, 0xB8, 0x20, 0x45, 0x0A, 0xB5, 0x2B, 0xB1, 0xBE, 0x11, 0x1B, 0x2B, 0x02, 0xBD, 0xE3, 0xE0, 
            0xF0, 0x3D, 0x33, 0xED, 0x28, 0x2F, 0x2C, 0xC3, 0xFD, 0xBF, 0xB9, 0xDE, 0x4C, 0xCC, 0x8B, 0x70, 
            0x97, 0xD0, 0x48, 0xB6, 0x4D, 0xE9, 0x24, 0x52, 0x77, 0x48, 0xE5, 0x79, 0x12, 0x3B, 0xDA, 0xE3, 
            0x28, 0x68, 0x24, 0x29, 0xF9, 0xF3, 0x54, 0xA4, 0x66, 0x88, 0xA0, 0xD0, 0x6D, 0xCF, 0x8D, 0x30, 
            0x21, 0x90, 0x96, 0xB6, 0x6B, 0x44, 0x12, 0xF6, 0xA0, 0x2D, 0x15, 0x24, 0x77, 0xEE, 0x61, 0xDE, 
            0x45, 0x45, 0xCA, 0xEC, 0xA2, 0x33, 0x28, 0x0D, 0xA5, 0xE3, 0xAE, 0x8C, 0xBC, 0x9B, 0x41, 0x1F, 
            0xC2, 0x53, 0x1E, 0xDA, 0xA6, 0xEB, 0x23, 0xE8, 0x96, 0x65, 0x9A, 0x7C, 0x98, 0x4E, 0x3F, 0x17, 
            0x0F, 0x6E, 0x6C, 0x68, 0xCA, 0x95, 0x3C, 0x28, 0xA6, 0xDB, 0x07, 0x99, 0xEC, 0x5C, 0x18, 0x90, 
            0x90, 0x6A, 0x6E, 0x83, 0x57, 0xEF, 0x1E, 0x25, 0xC7, 0x7B, 0xCB, 0x46, 0x84, 0xED, 0x98, 0xF1, 
            0xF4, 0x8C, 0x7D, 0x5F, 0xBC, 0xCE, 0x57, 0x58, 0x55, 0x46, 0x1A, 0xE3, 0xBF, 0x7B, 0x60, 0x00, 
            0x31, 0x7A, 0x7E, 0xCD, 0x3B, 0x30, 0xD4, 0x8B, 0x96, 0x37, 0x9E, 0x2D, 0x73, 0xA4, 0x50, 0xB8, 
            0x55, 0x7C, 0x65, 0x99, 0x42, 0xF9, 0xFF, 0x48, 0x71, 0xB9, 0x9D, 0xAC, 0x57, 0x92, 0xFA, 0x85, 
            0x05, 0x46, 0xE1, 0x55, 0xCE, 0x95, 0x4C, 0x2E, 0x7E, 0x4B, 0xFA, 0x70, 0x6A, 0xBA, 0xAE, 0x87, 
            0x14, 0xB7, 0xD7, 0xA7, 0x67, 0x1F, 0x61, 0x5B, 0xDE, 0x29, 0x17, 0x56, 0x7B, 0x2F, 0x47, 0xA0, 
            0x3E, 0x54, 0xDB, 0x21, 0x55, 0x65, 0x6E, 0xF7, 0x52, 0x1D, 0xAE, 0x78, 0x6C, 0xF6, 0x40, 0x32, 
            0x1E, 0x20, 0x5C, 0x10, 0x7F, 0x65, 0x1B, 0x9E, 0x25, 0x73, 0x4D, 0xF4, 0xE4, 0x31, 0xD7, 0x7A, 
            0x3D, 0x1F, 0xA9, 0x38, 0x45, 0x77, 0xAA, 0x9A, 0x31, 0x3F, 0xEE, 0xC6, 0x1B, 0x71, 0xF2, 0xAC, 
            0x43, 0x82, 0xDD, 0xF7, 0x8A, 0x10, 0xCB, 0xCD, 0xFE, 0xD6, 0xCE, 0xDF, 0x3A, 0x41, 0x75, 0xE4, 
            0xFD, 0x8A, 0x05, 0x20, 0x4D, 0xD1, 0x1B, 0x03, 0xCA, 0xFE, 0xEC, 0x89, 0x90, 0x47, 0x3D, 0x02, 
            0x76, 0x16, 0xD9, 0xDA, 0xD1, 0x16, 0x86, 0x9D, 0xAF, 0x2A, 0x12, 0x22, 0x44, 0x2D, 0x62, 0x80, 
            0x06, 0xE9, 0xE8, 0x98, 0x0E, 0xE4, 0x8F, 0xDD, 0x57, 0x54, 0xA0, 0x3D, 0x6B, 0x92, 0x82, 0x20, 
            0x29, 0xCC, 0xB4, 0x48, 0x14, 0x63, 0x3D, 0xCE, 0xBC, 0xE1, 0xAD, 0x68, 0x45, 0x48, 0x0A, 0x6A, 
            0xC8, 0x19, 0xAA, 0x62, 0x00, 0x2A, 0x76, 0x81, 0x4E, 0xC0, 0x64, 0x11, 0xEB, 0xF9, 0x33, 0x67, 
            0x5B, 0x59, 0xA5, 0x58, 0xF6, 0x07, 0x4B, 0x7C, 0xB7, 0x97, 0x3C, 0xC7, 0x43, 0x95, 0xF0, 0x4D, 
            0x89, 0xF5, 0xA5, 0xBC, 0xE2, 0xD0, 0xCD, 0x54, 0x8A, 0xFE, 0x0A, 0xA4, 0x9A, 0x57, 0x1D, 0xFB, 
            0x0C, 0xC3, 0x37, 0xB1, 0xBC, 0xA6, 0x85, 0xF7, 0x4C, 0xBA, 0x2B, 0xEF, 0xC6, 0x10, 0x7D, 0xD7, 
            0x5D, 0x27, 0xC6, 0xBA, 0xF7, 0x46, 0xEA, 0x91, 0x5A, 0x5D, 0x37, 0xBC, 0xD7, 0x08, 0xFE, 0x72, 
            0x5A, 0x32, 0x5B, 0xAC, 0xF9, 0xDB, 0xE2, 0x15, 0xBA, 0x25, 0x2B, 0xD0, 0x11, 0x1A, 0x42, 0x13, 
            0x8C, 0xFD, 0x87, 0x40, 0xD0, 0x3F, 0x4F, 0x0D, 0x34, 0xE9, 0xB2, 0xA1, 0xD6, 0x12, 0xC5, 0xF0
        };
    }
}