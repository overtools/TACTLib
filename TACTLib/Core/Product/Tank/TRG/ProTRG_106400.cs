using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_106400 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
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
                kidx += okidx % 29;
                buffer[i] ^= (byte)(digest[SignedMod(kidx + header.m_packageCount, SHA1_DIGESTSIZE)] + 1);
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xCA, 0x56, 0xF8, 0x7F, 0x62, 0xBB, 0x07, 0x4D, 0xE6, 0x6D, 0x6B, 0x15, 0xA4, 0xF5, 0x23, 0x37, 
            0xF9, 0xDE, 0x77, 0x41, 0x6D, 0xC2, 0x6B, 0x9B, 0xD6, 0x17, 0x38, 0xCB, 0x8C, 0x33, 0x1C, 0xEC, 
            0x83, 0x93, 0xFA, 0xB3, 0xA1, 0xC2, 0xE9, 0x42, 0xED, 0xD7, 0x3B, 0x52, 0x81, 0x29, 0x48, 0x01, 
            0xC5, 0xD5, 0xB0, 0x6A, 0x5E, 0x5D, 0xDE, 0x09, 0xF5, 0x64, 0x49, 0x56, 0x3F, 0x41, 0x9E, 0x15, 
            0x3E, 0xE4, 0x96, 0xD3, 0xE0, 0xE6, 0x41, 0x02, 0x3D, 0x56, 0x9E, 0x33, 0x0E, 0x24, 0x2F, 0x8C, 
            0x6D, 0xA0, 0x60, 0xE3, 0x03, 0x87, 0x6B, 0x81, 0x9D, 0x22, 0x2D, 0xC9, 0xF8, 0x86, 0xAF, 0x29, 
            0x53, 0x2A, 0x9E, 0xCD, 0x99, 0x39, 0x36, 0x2C, 0xEB, 0xCF, 0x0F, 0x42, 0xB5, 0xE6, 0x11, 0x85, 
            0x63, 0x08, 0xBF, 0x71, 0x6C, 0xB0, 0xE3, 0xF8, 0x8D, 0x68, 0x40, 0x37, 0x94, 0xFA, 0x1B, 0xC5, 
            0x2B, 0x25, 0x5E, 0x60, 0xE2, 0xF3, 0x81, 0x3F, 0x12, 0xF1, 0x5F, 0x66, 0x4C, 0xE4, 0xC6, 0xC3, 
            0x60, 0x21, 0xB0, 0xDD, 0xFD, 0xD3, 0xBF, 0x72, 0x88, 0x17, 0xD2, 0xC8, 0xA3, 0x70, 0x35, 0x15, 
            0x79, 0xFF, 0xB6, 0x5A, 0x05, 0x40, 0x6B, 0xCE, 0x9E, 0xE8, 0x8E, 0x1F, 0x82, 0x3A, 0xDA, 0xC7, 
            0x25, 0xA0, 0x65, 0x71, 0x60, 0xDE, 0x47, 0xBE, 0x8D, 0xCA, 0xA7, 0xF7, 0x9F, 0xE5, 0x33, 0x87, 
            0x68, 0xD8, 0xFE, 0xFE, 0xCF, 0x6E, 0xC7, 0xCA, 0xC6, 0x5A, 0xD1, 0xFA, 0x5F, 0xEC, 0x4E, 0x34, 
            0x85, 0xF9, 0x70, 0x6C, 0x2D, 0x9F, 0x3B, 0xEF, 0x0B, 0x51, 0x44, 0x6E, 0xBE, 0x23, 0x40, 0x84, 
            0x89, 0xBF, 0xE2, 0x1C, 0x46, 0x87, 0xE2, 0xA8, 0x99, 0xE8, 0x49, 0x79, 0x33, 0x31, 0x6F, 0xA7, 
            0x68, 0xB2, 0x67, 0xF6, 0x9E, 0xC6, 0x07, 0xB8, 0xE5, 0x2A, 0x18, 0x7E, 0xBD, 0x25, 0x38, 0x1C, 
            0xAE, 0xC2, 0x27, 0xA3, 0x0C, 0x47, 0x5F, 0x87, 0xFD, 0x97, 0x3F, 0x4E, 0x8B, 0xCE, 0x86, 0x81, 
            0x9D, 0x3B, 0x57, 0x1F, 0xDB, 0x5A, 0x1B, 0xA5, 0x97, 0xCA, 0x48, 0xC1, 0x46, 0xC4, 0x81, 0x61, 
            0xE0, 0x00, 0x6A, 0x1C, 0x13, 0x73, 0x2E, 0x3A, 0x90, 0xD6, 0xAE, 0x9F, 0xD1, 0x04, 0xDF, 0x3F, 
            0x88, 0x5D, 0x3B, 0x30, 0xDF, 0x6F, 0xBB, 0xE8, 0x0E, 0xBB, 0x1C, 0xD5, 0xE5, 0x85, 0x04, 0x58, 
            0x76, 0x87, 0x6E, 0x9D, 0x7C, 0xBE, 0x97, 0x5C, 0x08, 0x53, 0x85, 0x36, 0x14, 0x1D, 0xDE, 0x28, 
            0x3A, 0xDB, 0x55, 0x26, 0xA4, 0x26, 0x9A, 0x62, 0x90, 0x49, 0x31, 0xA6, 0x3E, 0x62, 0x4C, 0xEF, 
            0x47, 0x0D, 0xF9, 0xFF, 0xBF, 0x44, 0x14, 0x30, 0x70, 0xA8, 0x83, 0x7E, 0xFC, 0xB5, 0x37, 0xFD, 
            0xD4, 0x14, 0x87, 0x71, 0x9E, 0x64, 0xBC, 0xD5, 0xCC, 0x39, 0x05, 0x67, 0xBC, 0x2B, 0xF0, 0xDF, 
            0x20, 0x86, 0xC1, 0x12, 0xE9, 0x6B, 0x8B, 0x2B, 0x16, 0xDC, 0x42, 0x8F, 0x4D, 0xC5, 0x69, 0xC2, 
            0xF5, 0xC3, 0xF1, 0xBB, 0x92, 0x1C, 0x10, 0x27, 0x17, 0x37, 0x6C, 0x02, 0xF7, 0xEB, 0x2C, 0x22, 
            0x9F, 0xF2, 0xC3, 0x70, 0x09, 0x6D, 0xD7, 0xFC, 0x8B, 0x36, 0x80, 0x02, 0x83, 0xC3, 0x6D, 0xC8, 
            0x32, 0xDF, 0xF0, 0x8E, 0x54, 0x19, 0xAC, 0x6F, 0x57, 0xA3, 0x16, 0xD2, 0x52, 0xBB, 0xFA, 0x37, 
            0x09, 0xCA, 0xBE, 0x17, 0xFE, 0xF0, 0xC4, 0x9A, 0x1D, 0x65, 0xB4, 0x62, 0x08, 0xA3, 0xD6, 0x72, 
            0xD6, 0x9B, 0xFA, 0x84, 0x1C, 0xCD, 0x2A, 0x74, 0x26, 0xA9, 0xCD, 0x1E, 0x89, 0xB4, 0x3D, 0xA5, 
            0x8B, 0x94, 0x5D, 0x87, 0x0F, 0x6B, 0x85, 0xAA, 0xF6, 0xD4, 0xEB, 0xA0, 0xF9, 0x1E, 0x97, 0x42, 
            0x29, 0x6C, 0xE4, 0xE3, 0x06, 0xC0, 0xAD, 0x1E, 0x5F, 0x00, 0xBE, 0x71, 0xF7, 0xBB, 0x06, 0xAD
        };
    }
}