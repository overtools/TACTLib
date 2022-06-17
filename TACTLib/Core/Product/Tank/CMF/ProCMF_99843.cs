using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_99843 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(length * header.m_buildVersion);
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx -= header.m_buildVersion & 511;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_dataCount & 511];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += okidx % 13;
                buffer[i] ^= digest[SignedMod(kidx - 73, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xD9, 0xE2, 0x13, 0x55, 0xB5, 0x00, 0x9E, 0x45, 0x1C, 0xF3, 0x34, 0x24, 0xF8, 0x64, 0x64, 0x2F, 
            0xBB, 0xAA, 0x77, 0x1E, 0x64, 0xAA, 0x7B, 0xEB, 0xDE, 0x8A, 0xBD, 0x95, 0xF2, 0x03, 0x2C, 0x47, 
            0xB6, 0xDC, 0x16, 0x90, 0x44, 0x1B, 0xE2, 0x92, 0x5D, 0x03, 0xA8, 0xF5, 0xF7, 0xAC, 0x77, 0x11, 
            0xBA, 0xCC, 0x49, 0xFB, 0xAF, 0xA5, 0x3E, 0x16, 0x96, 0xB2, 0x99, 0xE0, 0x35, 0xDF, 0x8E, 0x1A, 
            0xD4, 0x0F, 0x56, 0xDD, 0x23, 0x3F, 0x7B, 0x25, 0x72, 0x1F, 0xBA, 0xFD, 0x13, 0xF8, 0xA9, 0x02, 
            0x87, 0xD0, 0x1C, 0x3B, 0xB4, 0x70, 0x79, 0x20, 0x6B, 0x39, 0x7C, 0x01, 0xEC, 0x4B, 0x0D, 0xBC, 
            0xC0, 0x20, 0x1B, 0xBA, 0xF5, 0xF7, 0x26, 0xD9, 0xA9, 0x75, 0x61, 0xF7, 0x5E, 0x67, 0x38, 0x11, 
            0x73, 0x15, 0x5B, 0xDC, 0xF1, 0x17, 0x65, 0x7C, 0x57, 0x47, 0x18, 0x36, 0x77, 0x5C, 0x91, 0xCF, 
            0x9B, 0x48, 0x44, 0xE5, 0x52, 0x9D, 0xD9, 0xC2, 0x3F, 0xF3, 0x52, 0x1B, 0xB5, 0x33, 0xC7, 0xE0, 
            0x38, 0x47, 0x3D, 0xA1, 0xD8, 0x88, 0xFF, 0x70, 0x37, 0x15, 0x4D, 0xAB, 0xFD, 0x44, 0x79, 0xEA, 
            0x62, 0xD4, 0x2C, 0xCB, 0x1A, 0x54, 0x60, 0xEB, 0x0A, 0x92, 0xFD, 0x82, 0x0C, 0x00, 0xE7, 0xA0, 
            0x48, 0x6E, 0xB3, 0xFA, 0x42, 0x5F, 0x99, 0x09, 0x10, 0xC4, 0x00, 0xB9, 0x30, 0x9C, 0x39, 0x43, 
            0x0D, 0xCB, 0x93, 0xDA, 0xDF, 0x1A, 0x5E, 0xD0, 0xD8, 0xBA, 0x0C, 0x14, 0xF5, 0x13, 0xBB, 0x3B, 
            0xCB, 0x3A, 0x04, 0x35, 0x63, 0x5C, 0x68, 0xE9, 0x01, 0xB7, 0x94, 0x6C, 0x3A, 0x09, 0xE7, 0xB6, 
            0x99, 0x32, 0xAD, 0x48, 0x1D, 0xDD, 0xDA, 0x27, 0xA8, 0xC4, 0x2B, 0xF9, 0x26, 0x96, 0xCD, 0x25, 
            0xEC, 0xCC, 0xE8, 0xA3, 0xCA, 0xB0, 0xEE, 0x2F, 0x54, 0x87, 0x34, 0xF1, 0x9A, 0xB7, 0x1A, 0xFA, 
            0x8F, 0x14, 0x01, 0xDC, 0x02, 0x74, 0x88, 0x47, 0x23, 0xF3, 0xB6, 0xBD, 0xB3, 0xD7, 0x48, 0xE7, 
            0xF0, 0xC4, 0xC6, 0xB8, 0x14, 0xB5, 0x55, 0x57, 0x98, 0xBD, 0xA7, 0xD7, 0xC4, 0xBF, 0x77, 0xEB, 
            0x13, 0x8D, 0xFD, 0x97, 0x4A, 0x84, 0x6F, 0x01, 0x23, 0x7F, 0x5A, 0x4F, 0x94, 0x5D, 0xDF, 0x13, 
            0x28, 0xE2, 0x70, 0xC3, 0xA9, 0x08, 0x80, 0x60, 0x24, 0x2A, 0x49, 0xAD, 0xF5, 0x0D, 0x53, 0x54, 
            0x5A, 0xA9, 0x3D, 0xDD, 0x38, 0x0D, 0xC0, 0xE3, 0x53, 0x98, 0x7C, 0x07, 0x58, 0x67, 0x39, 0x58, 
            0x30, 0xD6, 0xC1, 0x57, 0x6C, 0x96, 0x15, 0x28, 0x90, 0xBB, 0x51, 0x31, 0x21, 0x94, 0x93, 0x3D, 
            0x75, 0xDE, 0x42, 0xD5, 0x24, 0x31, 0xF1, 0xE9, 0x73, 0xD0, 0xAD, 0x9C, 0xC8, 0xDD, 0x5D, 0xC0, 
            0x96, 0x1D, 0x70, 0xC2, 0xAF, 0x8A, 0x50, 0x29, 0x5B, 0x24, 0x3A, 0xD1, 0x90, 0x81, 0xAA, 0x53, 
            0xD1, 0x10, 0x1C, 0x2A, 0xFE, 0xCD, 0xA1, 0xF7, 0x7D, 0xE4, 0x52, 0x68, 0x80, 0xE3, 0x0A, 0x3D, 
            0x28, 0xBD, 0x75, 0x9B, 0xA7, 0x39, 0xB5, 0x5E, 0x46, 0x6C, 0x79, 0x6E, 0xC5, 0x48, 0x14, 0x75, 
            0x0D, 0x0F, 0x5D, 0x58, 0x7F, 0x19, 0x7B, 0xAE, 0x63, 0x5C, 0xBD, 0x49, 0xE5, 0x02, 0xA0, 0x17, 
            0xAE, 0x4B, 0xDF, 0xE8, 0x27, 0x05, 0x4C, 0x97, 0x87, 0x6B, 0x86, 0x28, 0xA4, 0x24, 0x55, 0xA2, 
            0xB1, 0x50, 0x92, 0x08, 0x88, 0xCC, 0x80, 0xDA, 0x84, 0x25, 0x0F, 0x88, 0xFB, 0x83, 0x1F, 0x2E, 
            0x89, 0x6F, 0x4B, 0xFC, 0x39, 0x00, 0xAC, 0x86, 0x36, 0xD6, 0x3F, 0x4E, 0x0E, 0x5E, 0x67, 0xC7, 
            0x6B, 0x30, 0x2B, 0x94, 0x19, 0xAA, 0x53, 0xF9, 0x68, 0x12, 0xAD, 0xDF, 0x8E, 0xCC, 0xC7, 0x8A, 
            0x2B, 0xCE, 0x0F, 0x34, 0xF9, 0xEE, 0x55, 0x1A, 0x17, 0x4F, 0x75, 0xBE, 0x1C, 0xE2, 0x86, 0xCC
        };
    }
}