using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_136696 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_dataCount & 511];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += 3;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(length * header.m_buildVersion);
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx -= 43;
                buffer[i] ^= digest[SignedMod(kidx + header.m_dataCount, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x93, 0xF8, 0xBD, 0x20, 0x5A, 0x82, 0x13, 0x0E, 0x9E, 0x01, 0x3E, 0xC1, 0x72, 0x20, 0x90, 0x23, 
            0x19, 0xC8, 0x8D, 0x0A, 0xFD, 0x4C, 0x1D, 0xC6, 0x55, 0x28, 0xB2, 0x1D, 0x10, 0x66, 0x1D, 0xDE, 
            0xD2, 0xC5, 0x1E, 0xAB, 0x75, 0xAF, 0xB8, 0x06, 0xC8, 0x2D, 0x25, 0x00, 0xCD, 0x63, 0xF7, 0x0A, 
            0x0B, 0x42, 0x81, 0xA0, 0x55, 0xD9, 0x7C, 0xBF, 0x3E, 0x15, 0xF5, 0xF8, 0x45, 0xEA, 0x90, 0xBB, 
            0x6C, 0x1C, 0xEA, 0x1B, 0x19, 0xB5, 0x24, 0xB6, 0xF2, 0x4E, 0xE1, 0x31, 0xC7, 0x2D, 0x39, 0xB5, 
            0x42, 0x29, 0x8D, 0x20, 0xFC, 0xC9, 0x23, 0xC0, 0x02, 0xBD, 0xB6, 0x45, 0xEB, 0xA9, 0x95, 0x47, 
            0xB3, 0xA7, 0x9F, 0x4E, 0xC5, 0x6B, 0x3F, 0x32, 0x36, 0x38, 0xE0, 0xD9, 0xB4, 0x27, 0xFD, 0x74, 
            0x31, 0x0C, 0x9D, 0x6E, 0xD7, 0x51, 0x56, 0x2F, 0xE4, 0xEF, 0x68, 0x59, 0xF4, 0xCA, 0xD0, 0x0C, 
            0x47, 0x93, 0x5D, 0xD0, 0x3A, 0xAD, 0x06, 0xF0, 0x7D, 0x62, 0xA1, 0x4C, 0x00, 0x3F, 0x62, 0x6C, 
            0x22, 0x16, 0x8A, 0x2A, 0x09, 0x7B, 0xA8, 0x50, 0x37, 0x36, 0x87, 0xCF, 0xEE, 0x28, 0x70, 0xCE, 
            0x58, 0x1B, 0xDA, 0x8D, 0xD8, 0x40, 0x01, 0x3E, 0x34, 0x6F, 0xC9, 0x00, 0x7A, 0x4D, 0x8F, 0x1E, 
            0xF1, 0xF2, 0xC7, 0x55, 0xEB, 0xE9, 0x3F, 0xB4, 0xAE, 0x94, 0xBD, 0x76, 0x77, 0xE1, 0xF5, 0xEB, 
            0x77, 0x4B, 0x1C, 0x93, 0xCE, 0xE2, 0x62, 0x9D, 0x85, 0xA1, 0xC8, 0xD7, 0x3B, 0x4F, 0x1B, 0xBD, 
            0xF9, 0x99, 0xB6, 0xED, 0x1A, 0x88, 0x83, 0xBB, 0xE8, 0x65, 0x60, 0xC8, 0x34, 0x14, 0x3E, 0x69, 
            0x87, 0xF5, 0x07, 0x00, 0x24, 0xDF, 0xC1, 0xF7, 0x6B, 0xCC, 0x0E, 0x76, 0x68, 0xA5, 0x1D, 0xCA, 
            0x46, 0xC5, 0x9D, 0x71, 0x6E, 0xEE, 0x1A, 0x28, 0x9B, 0x06, 0x78, 0xDA, 0xA0, 0xE4, 0xE8, 0x0E, 
            0x8A, 0xC7, 0x34, 0xC4, 0x03, 0x79, 0x92, 0x02, 0xDA, 0xFB, 0x1F, 0x3C, 0x3F, 0xB9, 0xEA, 0x09, 
            0xD6, 0x32, 0x0D, 0x6F, 0x28, 0xD0, 0x82, 0x5F, 0x47, 0xD5, 0xFE, 0x48, 0x75, 0xF1, 0x42, 0x35, 
            0x97, 0x77, 0xF2, 0xF8, 0x35, 0x2B, 0xF8, 0x39, 0x32, 0xFC, 0x62, 0xFC, 0x73, 0x32, 0xD8, 0xFD, 
            0x89, 0x66, 0xF2, 0x40, 0x86, 0xAE, 0x7C, 0xB4, 0xAC, 0x22, 0x83, 0x3E, 0x6B, 0xDB, 0xD0, 0x88, 
            0x2D, 0x6A, 0x79, 0xB5, 0x25, 0xF1, 0xA2, 0x5D, 0xE2, 0x9B, 0x8A, 0x64, 0x76, 0xEA, 0xFE, 0xDF, 
            0xD1, 0x93, 0xBB, 0x74, 0xA0, 0xC0, 0x96, 0xF4, 0xAE, 0xC9, 0x5B, 0xEB, 0xDE, 0xE5, 0x4D, 0x30, 
            0x26, 0x9B, 0x5B, 0x89, 0xA1, 0x1D, 0x64, 0x23, 0x8D, 0x56, 0xA5, 0x42, 0x59, 0x5A, 0x87, 0xB3, 
            0xEE, 0x68, 0x14, 0x3C, 0xB3, 0x21, 0x28, 0x91, 0xD9, 0xD7, 0x49, 0xFA, 0x09, 0xFC, 0x22, 0x1E, 
            0xE1, 0x55, 0xD8, 0x49, 0xE3, 0x87, 0x5D, 0xD4, 0x0F, 0x5A, 0x76, 0xCD, 0x74, 0x58, 0x52, 0xFC, 
            0x83, 0x0A, 0xEC, 0x69, 0x45, 0xE8, 0xB7, 0x5D, 0x6A, 0x64, 0x19, 0x09, 0x30, 0x3A, 0xB3, 0xEC, 
            0x60, 0x8B, 0xA3, 0x7F, 0x26, 0xCE, 0xBF, 0x13, 0x15, 0x25, 0x5E, 0x62, 0xB3, 0xCF, 0x1E, 0xC3, 
            0x6D, 0xD2, 0xAF, 0x85, 0x04, 0x1A, 0x73, 0xC7, 0x1E, 0xEA, 0x7B, 0x40, 0x4F, 0xEF, 0xE0, 0x72, 
            0x11, 0x56, 0x0D, 0xA8, 0x13, 0x19, 0x89, 0x21, 0xCF, 0x63, 0x61, 0xBF, 0x60, 0x0E, 0xB3, 0x5D, 
            0x98, 0x7A, 0xE5, 0xC5, 0x41, 0x7B, 0xDC, 0x5C, 0x6D, 0xCE, 0xFC, 0xCE, 0xDA, 0xD0, 0x3E, 0x2F, 
            0xDE, 0x60, 0xDF, 0x48, 0xA5, 0x54, 0x4A, 0xC5, 0x35, 0x1F, 0xFF, 0x8B, 0x8A, 0xCC, 0x0F, 0x50, 
            0xB5, 0x7A, 0x87, 0x62, 0x4D, 0x77, 0x2D, 0x4F, 0xCF, 0x6E, 0x6E, 0x63, 0xCB, 0x1F, 0x28, 0x97
        };
    }
}