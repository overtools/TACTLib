using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_90193 : ICMFEncryptionProc
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
            0x29, 0x20, 0x7E, 0x3F, 0xD7, 0xD0, 0xC0, 0xF7, 0x3C, 0x57, 0x0C, 0x9E, 0xEE, 0x74, 0xD2, 0x7A,
            0x32, 0xC5, 0xB4, 0x82, 0x90, 0xF1, 0x5A, 0xD1, 0xF3, 0x82, 0x87, 0x79, 0x33, 0x6A, 0xAB, 0xE9,
            0xD0, 0xE1, 0x83, 0x91, 0xAF, 0x63, 0x00, 0x8F, 0x24, 0xCB, 0xE5, 0xD2, 0x29, 0x6E, 0xC1, 0x24,
            0xA3, 0xFF, 0xB9, 0xEE, 0xFF, 0x70, 0x89, 0x4C, 0xC9, 0xCE, 0x29, 0x9B, 0x7F, 0x5C, 0x68, 0x15,
            0x98, 0x06, 0x79, 0xFA, 0x43, 0x5B, 0x12, 0x49, 0xF0, 0x6C, 0x67, 0xA0, 0xA8, 0x64, 0x6B, 0x39,
            0x0E, 0x34, 0xB2, 0x2B, 0x3F, 0xF3, 0xD8, 0x3C, 0xD2, 0xD9, 0xAE, 0x3E, 0xA1, 0x2E, 0x4C, 0x7E,
            0xB9, 0x41, 0xF1, 0x51, 0xA5, 0x0B, 0x98, 0x9B, 0xC7, 0x52, 0xAE, 0x11, 0xFD, 0xC9, 0xC5, 0x0D,
            0xFD, 0xC2, 0xB3, 0x11, 0x40, 0x94, 0x36, 0x20, 0x80, 0xA6, 0xE6, 0x9F, 0x52, 0xC9, 0x24, 0xBC,
            0x74, 0xC2, 0x6C, 0x45, 0xA4, 0x5C, 0x9A, 0x4C, 0x23, 0x9C, 0x64, 0x71, 0x55, 0x3E, 0x1D, 0x86,
            0xB9, 0x42, 0x8A, 0x83, 0x13, 0x40, 0xD8, 0xC9, 0x65, 0x5E, 0x48, 0x1E, 0x80, 0x98, 0xCB, 0x82,
            0x08, 0x06, 0x6E, 0xAB, 0xFA, 0xA2, 0xFE, 0x2E, 0x56, 0xBA, 0x85, 0x43, 0xC1, 0x40, 0xF7, 0x31,
            0xB8, 0x06, 0x6C, 0xAC, 0x41, 0x68, 0xF4, 0x45, 0x81, 0x99, 0x89, 0xEC, 0xBF, 0xC8, 0x94, 0x5D,
            0x22, 0x3F, 0x76, 0xA6, 0x84, 0x05, 0xB2, 0x2F, 0xC9, 0x98, 0xF8, 0x75, 0x24, 0x8D, 0x42, 0x63,
            0x4D, 0x91, 0xD8, 0x90, 0xF7, 0x42, 0x7C, 0xB4, 0x33, 0x8B, 0xF6, 0xEC, 0x93, 0x50, 0xB0, 0xB7,
            0x19, 0xD5, 0xF6, 0xB0, 0x60, 0x81, 0x87, 0xA0, 0xC1, 0x0F, 0xC4, 0xE4, 0x47, 0xC8, 0x4F, 0xB0,
            0x90, 0x02, 0x9A, 0xB6, 0xBB, 0x70, 0x07, 0x90, 0xEC, 0xA8, 0xA8, 0x06, 0x58, 0xF5, 0xF7, 0x46,
            0xA1, 0xB7, 0x4E, 0xF8, 0x18, 0x93, 0x0D, 0xBE, 0x5B, 0xCE, 0x58, 0x9F, 0xA1, 0x28, 0xA7, 0x93,
            0x4B, 0xCD, 0x65, 0x3A, 0x41, 0x2D, 0xB1, 0x7B, 0x3B, 0x14, 0xAF, 0xFE, 0x2D, 0x37, 0xB1, 0x5C,
            0x13, 0xCF, 0x39, 0x7C, 0x82, 0x6D, 0xFB, 0x80, 0x69, 0x55, 0xAD, 0x63, 0xB2, 0x73, 0x06, 0xB9,
            0x0A, 0x00, 0xA3, 0x66, 0xD0, 0x77, 0xD3, 0x46, 0x7B, 0x5F, 0x0C, 0x9A, 0x76, 0x81, 0x7D, 0x3B,
            0x2F, 0x03, 0xE0, 0xC9, 0x15, 0x16, 0x36, 0xFD, 0x30, 0xF6, 0xC3, 0x26, 0x14, 0x88, 0xDB, 0x70,
            0x78, 0x05, 0x69, 0x15, 0xB2, 0xDD, 0x44, 0x49, 0x65, 0xFB, 0x65, 0xC4, 0xB0, 0xE5, 0xBD, 0x16,
            0x2A, 0xBE, 0x78, 0xE3, 0x40, 0x1F, 0x65, 0x5A, 0x63, 0x5B, 0x5D, 0xDA, 0x44, 0x72, 0x1B, 0x31,
            0x3B, 0x4C, 0x04, 0xBC, 0x39, 0x20, 0xBD, 0x82, 0x53, 0xC6, 0x4C, 0xC3, 0x52, 0xB5, 0x62, 0xEF,
            0xE4, 0x39, 0x3C, 0x9F, 0x1A, 0xE4, 0xF1, 0xE6, 0x96, 0x0F, 0x7A, 0xDE, 0xE7, 0xE5, 0xEC, 0x60,
            0x67, 0xD2, 0x5D, 0xA7, 0xEB, 0xC5, 0xBC, 0x97, 0xE3, 0x77, 0x59, 0xEE, 0xD0, 0x34, 0x0C, 0xAE,
            0x21, 0x1D, 0xD2, 0xC6, 0xDA, 0x3B, 0x1D, 0x82, 0x8E, 0x4A, 0xFD, 0xD0, 0xF9, 0xF9, 0xCE, 0xC4,
            0x63, 0x33, 0x56, 0x9D, 0x70, 0x10, 0x7C, 0x7F, 0x72, 0xB0, 0x78, 0x5B, 0x1F, 0x72, 0xE1, 0x7B,
            0x90, 0x58, 0xBD, 0xA9, 0x38, 0xD4, 0xF8, 0x1E, 0xB6, 0xD0, 0x92, 0xD9, 0x80, 0xB0, 0xE5, 0xFD,
            0x4E, 0x30, 0x49, 0xB9, 0x68, 0x25, 0x11, 0x6D, 0x8E, 0xDF, 0x0D, 0x35, 0xFD, 0x46, 0x1F, 0x45,
            0xB5, 0xB7, 0xE8, 0xA9, 0x7C, 0x64, 0xFB, 0x75, 0xF4, 0x81, 0xB8, 0x3B, 0xCB, 0xE3, 0x91, 0x19,
            0x14, 0xB4, 0x29, 0xD4, 0xB9, 0xCE, 0x22, 0x13, 0x7C, 0xE5, 0x16, 0x0F, 0x73, 0xAA, 0xCD, 0xEE
        };
    }
}