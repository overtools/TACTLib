using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_115603 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
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
                kidx += (header.m_buildVersion * (uint)header.m_dataCount) % 7;
                buffer[i] ^= digest[SignedMod(kidx - 73, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xD3, 0x14, 0x8D, 0x63, 0x91, 0xC4, 0xA9, 0x78, 0x2F, 0x3F, 0xA6, 0x76, 0x4B, 0xCB, 0x51, 0x2F, 
            0xBF, 0xEC, 0x37, 0x02, 0xE6, 0x26, 0x3C, 0xCF, 0xCE, 0xF3, 0x24, 0x7A, 0x94, 0xE7, 0xED, 0xF1, 
            0xA7, 0xFA, 0xE7, 0x33, 0xE3, 0xD5, 0x8A, 0xB8, 0xBA, 0x7D, 0xCE, 0x82, 0xF0, 0x87, 0x02, 0xFF, 
            0x2D, 0x7B, 0x2F, 0x4D, 0x48, 0xF7, 0x18, 0xC8, 0x63, 0xA3, 0x24, 0xBD, 0xE2, 0x92, 0xC7, 0xAD, 
            0xED, 0xE6, 0xBC, 0x51, 0x0F, 0x4F, 0xD3, 0xDC, 0x52, 0xE8, 0xEA, 0xFF, 0x0D, 0x5E, 0x64, 0xF8, 
            0xE6, 0x2A, 0x2D, 0xC0, 0x83, 0xC3, 0x4E, 0xF0, 0xCE, 0x21, 0x85, 0x28, 0x61, 0xAE, 0xDA, 0x91, 
            0x2B, 0x87, 0xB3, 0xE5, 0x90, 0x39, 0x5E, 0x04, 0xC0, 0xF7, 0xA0, 0x22, 0xDB, 0xBD, 0x78, 0xC9, 
            0xAD, 0x34, 0x01, 0x8F, 0x7F, 0x3D, 0xA1, 0x2F, 0xF9, 0x78, 0x82, 0x47, 0x5C, 0x40, 0x20, 0xB9, 
            0xA8, 0x9B, 0x6C, 0xC2, 0x22, 0xE7, 0x97, 0xAC, 0x43, 0xD0, 0x6F, 0xCE, 0x14, 0x5C, 0x76, 0xAD, 
            0xB0, 0x58, 0xC4, 0xE9, 0x87, 0x05, 0x57, 0x5E, 0xAC, 0xDD, 0x7C, 0xC6, 0xC4, 0x81, 0x06, 0xA8, 
            0x11, 0xBA, 0x2F, 0xCB, 0xC1, 0xFC, 0x38, 0xF7, 0x20, 0xF4, 0x32, 0x7F, 0x07, 0x21, 0x31, 0x55, 
            0x3D, 0x53, 0x08, 0x38, 0x9E, 0x89, 0x5F, 0x34, 0x3C, 0xA0, 0x25, 0x37, 0x75, 0xE1, 0x39, 0xDC, 
            0x53, 0xCF, 0xC3, 0x11, 0xD7, 0xD8, 0x30, 0xFA, 0x3A, 0x4F, 0xFB, 0x47, 0xEE, 0xC1, 0xDA, 0xE1, 
            0x26, 0xF5, 0x2C, 0xE1, 0x25, 0x71, 0xC2, 0x72, 0xA0, 0x02, 0x54, 0x67, 0x5B, 0xF3, 0x12, 0xD1, 
            0xAC, 0x7E, 0x89, 0xC3, 0xBA, 0x67, 0x15, 0x00, 0x81, 0x26, 0xFB, 0xD8, 0x15, 0x11, 0xDE, 0xD9, 
            0x3A, 0xCD, 0xA8, 0xF0, 0x98, 0x74, 0xC1, 0xDD, 0x43, 0x52, 0x2F, 0x5D, 0x84, 0x5F, 0x56, 0xCE, 
            0xFA, 0x22, 0x87, 0x57, 0x3C, 0xC0, 0xCC, 0xAB, 0x61, 0x9C, 0xF6, 0xC5, 0x7C, 0xB5, 0x71, 0x80, 
            0xC5, 0x82, 0xEE, 0x7A, 0xBF, 0x4B, 0xC8, 0x7E, 0x71, 0xEE, 0x56, 0x62, 0xB3, 0x9A, 0x5A, 0x5F, 
            0x3F, 0xD0, 0x03, 0x3A, 0xC5, 0x4D, 0x6B, 0xCE, 0xDA, 0x3C, 0x19, 0xB7, 0xE9, 0x61, 0x87, 0xB2, 
            0x2C, 0xBE, 0xE5, 0x77, 0x0B, 0xC6, 0x4A, 0x1A, 0x9B, 0x7B, 0x54, 0xAA, 0xEF, 0x85, 0x54, 0xBE, 
            0xA5, 0xFB, 0x9D, 0x60, 0xC9, 0xB2, 0xFB, 0x66, 0x36, 0x1D, 0xE9, 0x31, 0x76, 0x29, 0xE9, 0xC5, 
            0xBB, 0xEE, 0x96, 0x9A, 0x1A, 0x48, 0xD8, 0x28, 0xAF, 0xE7, 0xC3, 0x1E, 0x78, 0x63, 0x94, 0x6A, 
            0xC1, 0xC5, 0x74, 0x20, 0x0E, 0x90, 0x76, 0x0D, 0x87, 0x94, 0xD5, 0xCA, 0xF7, 0x8A, 0x72, 0xFF, 
            0x2C, 0x32, 0x68, 0x4A, 0x37, 0xEF, 0xA8, 0xFE, 0x8E, 0xA4, 0x68, 0x11, 0xED, 0x5B, 0x58, 0x16, 
            0x88, 0x2A, 0x5C, 0x9F, 0x9D, 0xBE, 0xAF, 0x4B, 0x31, 0x9E, 0xDF, 0x7B, 0x29, 0xD2, 0x1A, 0x87, 
            0x9E, 0x8E, 0x55, 0xC6, 0x5A, 0x7D, 0x40, 0xEB, 0xC6, 0x5B, 0x8B, 0xEB, 0x3E, 0x75, 0x62, 0xE5, 
            0xE4, 0x56, 0xF6, 0xA6, 0x98, 0xF2, 0x85, 0x06, 0x19, 0x20, 0x8F, 0x74, 0xCE, 0x39, 0x4A, 0xFB, 
            0x0D, 0x5D, 0x86, 0xC1, 0x01, 0xC4, 0xD7, 0x30, 0x62, 0xB8, 0x16, 0xDF, 0x91, 0x66, 0xFE, 0x65, 
            0xC7, 0xD6, 0x9C, 0x2A, 0x85, 0xF2, 0xC0, 0x86, 0x75, 0x03, 0xE4, 0xE9, 0xC6, 0xE4, 0xD1, 0x40, 
            0x93, 0x36, 0x76, 0xF7, 0x49, 0xB1, 0x47, 0x36, 0xD2, 0xA7, 0x9C, 0x85, 0x5B, 0x1D, 0x9A, 0x8A, 
            0x20, 0x09, 0x88, 0x2A, 0x6D, 0xD5, 0x5D, 0x4B, 0x2C, 0x62, 0xA5, 0xB7, 0x1D, 0xDC, 0x02, 0xBB, 
            0x12, 0x1E, 0xFB, 0x4E, 0x99, 0xD7, 0x95, 0x26, 0x62, 0x55, 0xB2, 0x6C, 0x08, 0xB7, 0x6D, 0x4F
        };
    }
}