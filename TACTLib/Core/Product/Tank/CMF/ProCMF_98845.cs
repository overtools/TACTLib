using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_98845 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
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
                kidx -= 43;
                buffer[i] ^= digest[SignedMod(kidx + header.m_dataCount, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x76, 0x7D, 0x00, 0xF5, 0x22, 0x05, 0x60, 0x8B, 0xCA, 0xA1, 0x99, 0xF0, 0xB0, 0xBB, 0x42, 0xB5, 
            0x19, 0xBA, 0x61, 0x60, 0x85, 0x84, 0x59, 0x77, 0xE2, 0x69, 0xBA, 0xFE, 0x9F, 0xF1, 0x61, 0x73, 
            0xC2, 0xEF, 0x81, 0x3C, 0x7B, 0x28, 0x75, 0xAF, 0x7B, 0xF3, 0x47, 0x2E, 0x37, 0xD4, 0x25, 0xD7, 
            0xDB, 0x90, 0x34, 0x91, 0x70, 0x5E, 0x42, 0x7D, 0x8C, 0x60, 0xD0, 0x7A, 0x30, 0xCE, 0xD1, 0x85, 
            0x00, 0x45, 0xBA, 0x4D, 0xF1, 0x41, 0x33, 0xF6, 0xA6, 0x47, 0x7B, 0x50, 0x7E, 0xEA, 0x7B, 0x07, 
            0xC0, 0xC6, 0xDD, 0x13, 0xD0, 0x0E, 0x23, 0x1B, 0xCB, 0xE2, 0x5A, 0xE9, 0xB1, 0x4C, 0x01, 0x90, 
            0x2B, 0x31, 0x6F, 0xA9, 0x50, 0x6D, 0xF4, 0xB7, 0x9F, 0xE5, 0xFC, 0x57, 0xE1, 0x8F, 0x5C, 0x86, 
            0x4E, 0x61, 0x81, 0xD2, 0x3B, 0x43, 0x40, 0x26, 0x88, 0x2D, 0x61, 0xD6, 0x51, 0x62, 0x43, 0xB1, 
            0x87, 0x11, 0x05, 0xED, 0x61, 0x0A, 0xA6, 0x09, 0x88, 0x7B, 0x7F, 0x82, 0x08, 0x6D, 0xDE, 0x80, 
            0x6E, 0x1D, 0x7A, 0x86, 0x01, 0x17, 0x69, 0xB7, 0x6E, 0xCC, 0x2E, 0x1B, 0x6A, 0x29, 0x69, 0xD2, 
            0x60, 0xF7, 0xF2, 0x8D, 0x36, 0x5D, 0x01, 0x27, 0xE4, 0xE4, 0xBA, 0xD8, 0xD7, 0xFF, 0x37, 0x59, 
            0x41, 0x35, 0x77, 0x9F, 0x1E, 0x5F, 0x97, 0x06, 0x8F, 0x1E, 0x47, 0xA8, 0xFF, 0x9D, 0xD4, 0x9E, 
            0x71, 0xA7, 0x3E, 0x05, 0x51, 0xBF, 0x24, 0x7D, 0xCC, 0x4D, 0xB4, 0x7D, 0xAB, 0x59, 0x01, 0x15, 
            0x7E, 0x36, 0xEB, 0x94, 0x1B, 0xE6, 0xBD, 0x82, 0xC7, 0x44, 0x89, 0xBA, 0x43, 0xBB, 0x14, 0x19, 
            0x06, 0xA8, 0xC4, 0xE0, 0x1D, 0xD5, 0x1B, 0x65, 0xE9, 0x74, 0x0D, 0x8F, 0x86, 0x52, 0x1D, 0xF1, 
            0xBB, 0xE1, 0x5D, 0x47, 0x0A, 0x08, 0x57, 0x07, 0xE7, 0xDF, 0xAF, 0xB1, 0xD2, 0xED, 0xE8, 0x8E, 
            0xB0, 0x54, 0xF7, 0x40, 0x75, 0xFE, 0x3E, 0xC3, 0x73, 0x0B, 0x0B, 0xF9, 0xEB, 0xC8, 0x1A, 0xA4, 
            0x7F, 0xB1, 0x27, 0x8D, 0xB4, 0x67, 0x31, 0xF0, 0xEF, 0xE1, 0xA0, 0x79, 0x4A, 0xEF, 0xBD, 0x06, 
            0x97, 0x3E, 0xC9, 0x18, 0x84, 0xBF, 0x50, 0x63, 0xE3, 0xBE, 0x1F, 0xC8, 0x69, 0xEE, 0x2F, 0xEE, 
            0xAF, 0x5C, 0x36, 0xF6, 0x65, 0xE9, 0xFD, 0x8C, 0xC6, 0xC8, 0xB7, 0xE5, 0x19, 0x50, 0x5F, 0xB6, 
            0x4D, 0x71, 0x6B, 0xC0, 0xDC, 0xF6, 0x09, 0x68, 0x94, 0xDB, 0xE0, 0xF8, 0xB9, 0x8C, 0x6C, 0x20, 
            0x5B, 0x13, 0x77, 0x1F, 0xED, 0x8B, 0x1F, 0x0B, 0xA4, 0xBA, 0x61, 0x5A, 0x6E, 0xCA, 0x96, 0xC3, 
            0x62, 0x86, 0x67, 0x04, 0x23, 0x53, 0x41, 0x0A, 0x06, 0x2A, 0xBC, 0x9A, 0x73, 0xE2, 0x6C, 0x86, 
            0xE6, 0xE7, 0xCC, 0xF3, 0x43, 0x88, 0xE9, 0x61, 0x92, 0x1E, 0x60, 0xAD, 0xEB, 0xCA, 0x03, 0xE1, 
            0x97, 0xFE, 0x16, 0xD6, 0x68, 0x40, 0xF5, 0xD7, 0x49, 0x87, 0xF8, 0x44, 0x69, 0xD4, 0xA6, 0xB0, 
            0x84, 0x19, 0x25, 0x7C, 0x58, 0x28, 0x6E, 0x61, 0x15, 0x1A, 0x10, 0x81, 0xF8, 0x2A, 0xBB, 0xF0, 
            0x07, 0xA7, 0x89, 0x65, 0xBB, 0x1A, 0x62, 0x63, 0xF2, 0x83, 0x9F, 0xA8, 0xC4, 0x48, 0xA5, 0xD5, 
            0x39, 0x6A, 0xE0, 0xF3, 0x2D, 0x26, 0xB3, 0x86, 0x38, 0x7B, 0x97, 0x96, 0xF9, 0xE7, 0xA6, 0x77, 
            0x42, 0x00, 0x7E, 0x54, 0xF2, 0x37, 0xC4, 0x66, 0x36, 0x2B, 0xD7, 0x67, 0x6E, 0x6D, 0xBE, 0xDA, 
            0x29, 0xD2, 0x80, 0xED, 0xEA, 0x5A, 0xA9, 0x42, 0xF8, 0xC7, 0x50, 0x13, 0x97, 0x0E, 0x3B, 0xC9, 
            0x78, 0x89, 0x85, 0x9A, 0x37, 0x0E, 0x7E, 0x9C, 0xE5, 0x47, 0xF0, 0x01, 0xF1, 0xF0, 0x29, 0x6D, 
            0xED, 0x99, 0x2A, 0x92, 0x9C, 0xAD, 0x69, 0x32, 0x8B, 0xC9, 0xEC, 0xFF, 0xB6, 0x45, 0xB6, 0xF8
        };
    }
}