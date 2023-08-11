using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ResourceGraph;

namespace TACTLib.Core.Product.Tank.TRG
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProTRG_114357 : ITRGEncryptionProc
    {
        public byte[] Key(TRGHeader header, int length)
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
            0x6C, 0xF2, 0xCE, 0xF9, 0x61, 0x62, 0x37, 0xC7, 0xBF, 0x82, 0xAD, 0x53, 0xB3, 0xAF, 0xDA, 0xB5, 
            0xBE, 0x81, 0x2F, 0xFB, 0x09, 0x4C, 0xC8, 0x85, 0x8B, 0x57, 0xBA, 0xD7, 0x88, 0x52, 0x55, 0xB4, 
            0x08, 0xEE, 0x4D, 0x4B, 0xD5, 0x56, 0x8C, 0xF8, 0xB0, 0x40, 0xA1, 0xE2, 0x80, 0x55, 0x31, 0x73, 
            0xF2, 0x55, 0x7F, 0x77, 0x9A, 0x27, 0x45, 0xA6, 0x52, 0xF9, 0x3D, 0xB6, 0x9D, 0xD6, 0x48, 0xA3, 
            0xCE, 0x34, 0x6D, 0x03, 0x67, 0x10, 0xE5, 0x48, 0x56, 0xE3, 0xAA, 0xAB, 0xCD, 0x63, 0x9E, 0x45, 
            0x4C, 0x44, 0xAA, 0x2B, 0x3F, 0x77, 0xC5, 0x9C, 0x38, 0x88, 0xC2, 0xBA, 0x75, 0x7A, 0xF9, 0x67, 
            0x94, 0x33, 0xBD, 0x4B, 0x6F, 0x08, 0x68, 0xD2, 0x38, 0x62, 0x6C, 0xC8, 0xD7, 0x16, 0xA5, 0x19, 
            0x07, 0xF1, 0x2A, 0xC3, 0x2F, 0x2C, 0x6D, 0xA7, 0xE4, 0x87, 0xF0, 0xD5, 0x31, 0xDB, 0xF1, 0xB7, 
            0x01, 0xD8, 0xD9, 0xBC, 0xB6, 0xAD, 0x18, 0xDB, 0x42, 0x15, 0xD1, 0x50, 0x8D, 0x5F, 0x1F, 0xEF, 
            0x6B, 0x0D, 0x20, 0x95, 0xD9, 0x7B, 0x84, 0x76, 0x49, 0xF7, 0x21, 0xD8, 0xD3, 0x07, 0x41, 0x1F, 
            0xA2, 0x7A, 0xC5, 0xFE, 0xD9, 0xB5, 0x27, 0x52, 0x5E, 0x07, 0x50, 0xA7, 0x81, 0xB4, 0xAE, 0x32, 
            0x4F, 0x35, 0x05, 0xF5, 0x0E, 0xCA, 0xC3, 0x9A, 0xA9, 0x35, 0x61, 0x64, 0xE2, 0x44, 0xAF, 0x78, 
            0x87, 0x06, 0xD9, 0x74, 0x51, 0xBE, 0xD1, 0x30, 0xF2, 0xD0, 0x2C, 0x7B, 0x74, 0x98, 0xCB, 0x9D, 
            0x60, 0x3A, 0x2E, 0x0E, 0x22, 0x40, 0xCB, 0x8F, 0x5D, 0xBA, 0x17, 0xB6, 0xC4, 0xB8, 0x89, 0x63, 
            0xED, 0xA3, 0x61, 0x32, 0x4F, 0xE2, 0x1E, 0x71, 0xD8, 0x41, 0xF2, 0xA7, 0xB6, 0xCF, 0xC8, 0x3B, 
            0xA6, 0xD0, 0xAE, 0x42, 0xD5, 0x73, 0xDB, 0x99, 0x82, 0xE1, 0xBA, 0x23, 0x25, 0x3E, 0x7A, 0x8E, 
            0x1D, 0x3D, 0x78, 0x7B, 0x27, 0x83, 0xAB, 0x91, 0x7C, 0xA4, 0x7A, 0x7C, 0x00, 0x48, 0xAD, 0x2B, 
            0x17, 0x34, 0x3C, 0x88, 0xC4, 0xCA, 0xC5, 0xC7, 0x78, 0x21, 0x62, 0x8F, 0x0B, 0x30, 0xC3, 0x6F, 
            0x0C, 0x29, 0x25, 0x07, 0x38, 0xE3, 0x2F, 0x0A, 0x3F, 0x9A, 0xF1, 0xD7, 0x35, 0xD5, 0x5C, 0xDF, 
            0xFA, 0xF3, 0x46, 0x00, 0x07, 0xBC, 0xCB, 0xD0, 0x54, 0xA6, 0x43, 0x27, 0x00, 0x1F, 0x73, 0x6F, 
            0xD1, 0xD7, 0x1A, 0xF4, 0x83, 0xB2, 0x91, 0xBC, 0x85, 0x43, 0x33, 0xF2, 0xF3, 0xC9, 0x3B, 0xB9, 
            0xE2, 0x60, 0xB9, 0xA6, 0xAB, 0xDF, 0x28, 0xED, 0xD3, 0x79, 0x47, 0x74, 0x9A, 0x33, 0x88, 0x7D, 
            0x7D, 0x25, 0xD3, 0x26, 0xE6, 0x9B, 0xD2, 0x40, 0xCE, 0xC9, 0x7E, 0x8B, 0x21, 0xBC, 0xB0, 0x75, 
            0x44, 0x81, 0xD3, 0x98, 0x5C, 0x29, 0x9B, 0xA8, 0xA9, 0x7A, 0x82, 0x62, 0x3F, 0x44, 0x34, 0xF6, 
            0xB2, 0x0B, 0x45, 0x28, 0xE7, 0xA5, 0x97, 0x51, 0x9F, 0x0D, 0x7C, 0x1A, 0x5A, 0xF3, 0x20, 0x92, 
            0x2C, 0x1D, 0xC6, 0x2E, 0x3E, 0x18, 0x82, 0xBF, 0xF1, 0xA1, 0x00, 0x1E, 0xB4, 0x56, 0xC5, 0xF5, 
            0x97, 0x4A, 0x27, 0x5F, 0xF2, 0x66, 0x13, 0xCE, 0x6F, 0xBE, 0x28, 0x51, 0x3E, 0x4C, 0xC7, 0x42, 
            0xA0, 0x33, 0x65, 0xE4, 0xEA, 0xC9, 0xFF, 0x6C, 0xAA, 0x45, 0x54, 0x81, 0xBE, 0xF6, 0xCB, 0x2C, 
            0x1E, 0x6F, 0xEB, 0x52, 0x1D, 0x2C, 0xE4, 0x10, 0xB0, 0x75, 0x53, 0x78, 0x75, 0xE1, 0xD6, 0x4C, 
            0x7C, 0x3D, 0x9C, 0x77, 0xCC, 0xC8, 0xBD, 0xDD, 0x4D, 0x59, 0x11, 0x2B, 0x17, 0x9A, 0xBB, 0x2E, 
            0x35, 0xE2, 0xF4, 0x9A, 0x10, 0xEB, 0xA1, 0xF0, 0x35, 0xF9, 0x42, 0xFD, 0xCD, 0x98, 0x4E, 0xCE, 
            0xD1, 0xED, 0x1B, 0xEA, 0x8C, 0xBB, 0xDE, 0xFA, 0xC9, 0xB3, 0xFF, 0x5E, 0x26, 0x4D, 0x33, 0xC4
        };
    }
}