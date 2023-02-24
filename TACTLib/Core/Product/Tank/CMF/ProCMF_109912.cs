using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_109912 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[(length * Keytable[0]) & 511];
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                switch (SignedMod(kidx, 3))
                {
                case 0:
                    kidx += 103;
                    break;
                case 1:
                    kidx = (uint)SignedMod(kidx * 4, header.m_buildVersion);
                    break;
                case 2:
                    --kidx;
                    break;
                }
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(2 * digest[5]);
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
            0x5B, 0xBE, 0x9D, 0x1B, 0xD4, 0x42, 0xBF, 0xAB, 0xB9, 0x0D, 0xE0, 0xC1, 0xE3, 0xDD, 0xD1, 0x9E, 
            0x8C, 0x9F, 0x9D, 0xE3, 0x97, 0x6A, 0x7E, 0x3F, 0xC7, 0x83, 0x2F, 0x78, 0xE9, 0x43, 0x62, 0x88, 
            0x2A, 0x96, 0xA6, 0x44, 0x6D, 0xD2, 0x1E, 0x5A, 0x68, 0x47, 0x05, 0x1C, 0x67, 0xFE, 0x8B, 0xE6, 
            0xF5, 0xDB, 0x6C, 0x57, 0x6C, 0xB8, 0x2C, 0xBA, 0xF7, 0x32, 0xF6, 0xBD, 0xD2, 0xDD, 0x03, 0xCD, 
            0x67, 0xA0, 0xCF, 0x53, 0x91, 0x7B, 0x10, 0x3D, 0x34, 0x40, 0xE8, 0x7A, 0xEA, 0xA9, 0x49, 0x20, 
            0x10, 0x13, 0x6C, 0x31, 0x21, 0x52, 0x54, 0x08, 0xF2, 0x40, 0xE2, 0xED, 0xBC, 0xB5, 0xB1, 0xFB, 
            0x37, 0xEF, 0x35, 0xDA, 0xDE, 0x2E, 0x46, 0x9E, 0x4A, 0x89, 0x12, 0xC3, 0x89, 0xA8, 0xD3, 0x56, 
            0xBA, 0x5C, 0xAD, 0xAD, 0xF3, 0xD5, 0xBB, 0x19, 0x93, 0x9E, 0x36, 0x7F, 0x48, 0x3B, 0xDA, 0x93, 
            0xC4, 0xBD, 0x13, 0x96, 0x5B, 0x51, 0xE5, 0x5F, 0x8C, 0xCD, 0x76, 0xBD, 0xF4, 0x88, 0x15, 0xB9, 
            0x09, 0xFE, 0x0C, 0x27, 0x34, 0x27, 0xBE, 0xC6, 0x7F, 0x03, 0x2C, 0xE2, 0x24, 0x6D, 0x1D, 0xF3, 
            0x64, 0xAD, 0x81, 0x5A, 0xFA, 0x66, 0x35, 0x8C, 0x03, 0x8D, 0xC1, 0x42, 0x4A, 0x90, 0x7F, 0x5D, 
            0x44, 0xC5, 0x91, 0x1C, 0xB8, 0x2A, 0x4B, 0x88, 0x6E, 0x76, 0x7F, 0xEA, 0x44, 0x45, 0x4F, 0xCC, 
            0x38, 0xD2, 0x78, 0x0F, 0x6B, 0x1D, 0x91, 0xCC, 0x1D, 0x51, 0x2F, 0x46, 0x12, 0x2B, 0x78, 0x23, 
            0xEB, 0xC2, 0xE1, 0x0A, 0x1B, 0x5D, 0xED, 0xF5, 0x22, 0x28, 0x81, 0x9C, 0x20, 0x12, 0xBC, 0xDE, 
            0x94, 0x8B, 0xBE, 0x2C, 0x66, 0xAA, 0x82, 0x81, 0x27, 0xC5, 0x31, 0x3A, 0x52, 0xCC, 0x87, 0x1B, 
            0x85, 0x45, 0x4D, 0x75, 0x34, 0xBE, 0xAF, 0x81, 0xD5, 0x97, 0x2C, 0xD3, 0xB2, 0xF7, 0xF6, 0xE1, 
            0x21, 0x23, 0xCC, 0x37, 0xFF, 0xDE, 0x36, 0x95, 0x19, 0x86, 0xD6, 0x82, 0x16, 0x70, 0x43, 0xCF, 
            0xEF, 0x61, 0x22, 0x21, 0x0A, 0x8E, 0xEA, 0xF5, 0x0B, 0x77, 0xAA, 0x3F, 0xC1, 0xFB, 0x72, 0xF1, 
            0xE6, 0x81, 0xED, 0x21, 0x6B, 0x01, 0xFD, 0xB8, 0x20, 0x14, 0xD2, 0x36, 0xF6, 0x81, 0x4B, 0xAF, 
            0x64, 0x55, 0x7C, 0x70, 0xD3, 0xCD, 0x66, 0xE9, 0x8D, 0xEF, 0xD5, 0xB2, 0xDD, 0x58, 0x39, 0xDC, 
            0x14, 0x80, 0x6D, 0x45, 0x87, 0xC8, 0xA5, 0x92, 0xCD, 0xD8, 0xEB, 0xAA, 0x15, 0x72, 0xFD, 0xDC, 
            0x85, 0x9C, 0x89, 0xE9, 0xD4, 0x44, 0xCB, 0x2B, 0x0B, 0x17, 0x65, 0xA1, 0xD7, 0x02, 0xD4, 0x0C, 
            0xE5, 0x14, 0xAC, 0x71, 0x0F, 0xE2, 0xE2, 0xB2, 0x92, 0x54, 0xF9, 0x6A, 0x9A, 0x57, 0x63, 0x9C, 
            0xA1, 0xDE, 0x19, 0x3B, 0x36, 0x52, 0x8E, 0x2D, 0xBC, 0xE1, 0x19, 0x1E, 0xCD, 0x45, 0x5E, 0x9C, 
            0x65, 0xC3, 0x81, 0x77, 0xC7, 0xA7, 0xA1, 0x21, 0x0E, 0x47, 0x51, 0xA0, 0x53, 0x22, 0x5A, 0xF4, 
            0x12, 0x2D, 0x8C, 0xCC, 0x3F, 0x70, 0xC8, 0x58, 0x7A, 0xB3, 0x30, 0x9C, 0x5F, 0xF2, 0xF6, 0xF1, 
            0x4A, 0xAE, 0xF6, 0x9C, 0x9E, 0x0D, 0x35, 0x47, 0xCF, 0x91, 0xF7, 0x77, 0x1A, 0x1B, 0xE6, 0x45, 
            0xD3, 0x31, 0xAD, 0x15, 0xC3, 0x15, 0x3D, 0x1C, 0xCD, 0x7D, 0x13, 0xB1, 0xBD, 0xBB, 0x7B, 0x1E, 
            0x73, 0x08, 0xDB, 0xF2, 0x3D, 0xFD, 0x80, 0xA9, 0x54, 0xB2, 0x71, 0xB4, 0xDC, 0x42, 0x03, 0x5B, 
            0xDB, 0x9F, 0x34, 0x9A, 0xC7, 0xC4, 0x69, 0x31, 0x6F, 0x22, 0xFF, 0xFD, 0x66, 0x09, 0x22, 0x2A, 
            0x79, 0x33, 0x78, 0x90, 0xA1, 0xB8, 0x23, 0x07, 0xA1, 0x54, 0x2A, 0x21, 0x86, 0xAC, 0xE7, 0xA4, 
            0x94, 0xB4, 0x5B, 0x2E, 0x25, 0xD7, 0x09, 0x43, 0x7E, 0xA3, 0xB5, 0x8E, 0xDE, 0x0E, 0x00, 0x28
        };
    }
}