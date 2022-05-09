using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_93519 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[length + 256];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 61;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(digest[7] + header.m_dataCount) & 511;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 29;
                buffer[i] ^= (byte)(digest[SignedMod(kidx + header.m_dataCount, SHA1_DIGESTSIZE)] + 1);
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xC2, 0xF8, 0x3A, 0xC4, 0x8F, 0x1C, 0x17, 0x12, 0x04, 0xA3, 0x2E, 0x5A, 0x03, 0xA0, 0xE6, 0xBF, 
            0x33, 0x6F, 0xF7, 0x42, 0xFB, 0x30, 0x9E, 0xF8, 0x65, 0x35, 0x4B, 0xCD, 0xCD, 0x9C, 0x9A, 0xC5, 
            0xA2, 0xB1, 0x30, 0x89, 0x52, 0x08, 0xB3, 0x2E, 0x57, 0x18, 0x79, 0xB7, 0x63, 0x4E, 0xB7, 0x6E, 
            0x70, 0x43, 0x6B, 0x55, 0xB8, 0x15, 0xF4, 0x53, 0x9A, 0xED, 0xAA, 0x08, 0xB8, 0xD9, 0xF1, 0xAB, 
            0xEC, 0x86, 0xF3, 0x48, 0x25, 0xDC, 0x00, 0xA8, 0x28, 0xD8, 0x2F, 0x9A, 0x4A, 0x3F, 0x75, 0xE0, 
            0x28, 0xD8, 0x7B, 0x06, 0x22, 0x34, 0x17, 0x18, 0x99, 0x27, 0xF1, 0x79, 0x97, 0xCC, 0xCD, 0x53, 
            0xE9, 0x59, 0x53, 0x41, 0xF4, 0x76, 0x93, 0x16, 0x57, 0xAE, 0x41, 0x5A, 0xED, 0xCF, 0x3F, 0x6F, 
            0x0F, 0x61, 0xD1, 0x4A, 0xBE, 0x63, 0x8A, 0xDD, 0xC9, 0x71, 0x2C, 0x83, 0xFC, 0xAD, 0x3C, 0xC4, 
            0x7A, 0x1E, 0xDB, 0xE1, 0x14, 0x99, 0x92, 0x40, 0xAD, 0x06, 0x64, 0x92, 0x71, 0x26, 0x14, 0x72, 
            0xE9, 0xCD, 0xA6, 0x89, 0xD1, 0x2D, 0x6F, 0xA5, 0x58, 0x05, 0x4A, 0x65, 0x21, 0x0D, 0x73, 0x70, 
            0xCF, 0x24, 0xD9, 0xF0, 0x93, 0xFD, 0x08, 0xFE, 0x3F, 0xCE, 0xAC, 0xE2, 0x9E, 0xAF, 0xCC, 0xE7, 
            0xB0, 0xD4, 0xF0, 0xD8, 0x27, 0xF1, 0x5C, 0x8D, 0xF0, 0xC1, 0x9D, 0xFD, 0x98, 0x02, 0xD5, 0xBE, 
            0xF3, 0x5F, 0x62, 0x5A, 0xBF, 0x5A, 0xC9, 0xAD, 0x91, 0xA9, 0x0D, 0xC0, 0x77, 0xA5, 0x62, 0xD5, 
            0x8F, 0x49, 0x36, 0x16, 0x8C, 0xA0, 0x69, 0x0D, 0x29, 0x6E, 0x52, 0x8C, 0x50, 0xC6, 0xE2, 0xFE, 
            0xA0, 0x80, 0xF9, 0x3F, 0xBF, 0x37, 0x3E, 0x3A, 0xD9, 0x22, 0xC5, 0xFD, 0x1A, 0xFC, 0x5A, 0xC4, 
            0x26, 0x72, 0x4E, 0x77, 0x68, 0x2E, 0xB3, 0x4B, 0x90, 0x30, 0x6C, 0xA6, 0xC4, 0xB6, 0x7C, 0xF4, 
            0x17, 0x44, 0x8F, 0x9A, 0x10, 0x13, 0xC9, 0x23, 0x26, 0x74, 0x01, 0xB5, 0x9C, 0xF2, 0x1D, 0x3D, 
            0x61, 0x06, 0xDB, 0x76, 0x7C, 0x63, 0x4D, 0x72, 0xAA, 0xD4, 0xC8, 0x13, 0xC3, 0xE6, 0x60, 0x74, 
            0x6E, 0x89, 0xB7, 0x5D, 0x40, 0x95, 0xC4, 0x88, 0xC4, 0x6A, 0x4B, 0x52, 0x40, 0x87, 0xA6, 0xEF, 
            0xED, 0xD2, 0x66, 0xFC, 0x8D, 0xEC, 0x86, 0x21, 0x54, 0x79, 0x7C, 0xBF, 0x19, 0x3D, 0xE7, 0x77, 
            0x42, 0xC9, 0xB5, 0xBE, 0xED, 0x13, 0xEF, 0xE4, 0x33, 0x0F, 0x87, 0x8F, 0x7D, 0xD1, 0xD2, 0x85, 
            0xA9, 0x72, 0x6D, 0x12, 0x34, 0x7B, 0x54, 0x3F, 0xB0, 0xEB, 0x30, 0x48, 0x25, 0x21, 0x67, 0xA1, 
            0xAD, 0x0C, 0xD6, 0xE4, 0xBF, 0x1C, 0x7C, 0x97, 0xF0, 0x63, 0x74, 0xDB, 0x72, 0x9E, 0x50, 0xFE, 
            0xF2, 0xFB, 0x85, 0x03, 0x88, 0xBF, 0x00, 0xF8, 0x34, 0x90, 0x30, 0x28, 0xC9, 0x1F, 0xE8, 0xC3, 
            0xD8, 0x3A, 0x2A, 0x65, 0x06, 0x7D, 0xCE, 0xE9, 0xE9, 0x61, 0xFB, 0x61, 0xD1, 0x95, 0xFE, 0xBB, 
            0x7D, 0xD4, 0x2F, 0x81, 0xAD, 0x66, 0xE6, 0xED, 0xB9, 0x5B, 0x0D, 0x16, 0x7A, 0xAE, 0xFE, 0xDF, 
            0x9D, 0xD4, 0x28, 0xDE, 0x5A, 0xFC, 0x3F, 0x83, 0x7D, 0xAA, 0xA1, 0xB5, 0xBC, 0xAF, 0xDE, 0xE4, 
            0x3E, 0xD6, 0x75, 0x97, 0x53, 0x18, 0xF5, 0x1B, 0x45, 0xD2, 0xE4, 0x16, 0x1A, 0xB6, 0x7D, 0x2F, 
            0x1B, 0x1A, 0x12, 0xF6, 0x80, 0x7A, 0xAE, 0x9E, 0xE1, 0xA7, 0xC4, 0x68, 0x18, 0xCE, 0xC1, 0x02, 
            0x19, 0x0C, 0x8F, 0x57, 0xD4, 0x5B, 0xD5, 0xBC, 0x66, 0x5B, 0x3E, 0x65, 0xDB, 0x3B, 0x00, 0x98, 
            0x64, 0xBE, 0x21, 0x0F, 0x08, 0xEF, 0x27, 0x7B, 0x7F, 0x93, 0x26, 0x86, 0x91, 0x6B, 0x1C, 0xA2, 
            0xC0, 0xAA, 0x28, 0x0C, 0xCE, 0xA4, 0xBB, 0xA9, 0x08, 0x5D, 0x9E, 0x6D, 0x57, 0x13, 0xBA, 0x2E
        };
    }
}