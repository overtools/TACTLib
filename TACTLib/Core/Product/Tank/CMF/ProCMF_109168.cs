using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_109168 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = (uint)(length * header.m_buildVersion);
            for (uint i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx = header.m_buildVersion - kidx;
            }
            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_buildVersion & 511];
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
            0xCB, 0x08, 0x59, 0x42, 0x68, 0xE7, 0xC9, 0xE7, 0x34, 0x4A, 0x62, 0xB2, 0xD2, 0x66, 0x22, 0x49, 
            0x05, 0x93, 0x09, 0x3E, 0x32, 0x32, 0xE8, 0x82, 0x62, 0x34, 0xDD, 0xA9, 0x8B, 0x4C, 0x3D, 0xF9, 
            0xFF, 0xF2, 0x42, 0x3A, 0x3B, 0xC7, 0x8B, 0x0B, 0xDC, 0x73, 0x9F, 0x1A, 0x28, 0x1C, 0x01, 0xF8, 
            0xAA, 0x7A, 0xFD, 0x0C, 0xC5, 0x38, 0x98, 0xE6, 0x37, 0x39, 0xE0, 0xC6, 0xCA, 0xB6, 0x58, 0x2D, 
            0xB6, 0x56, 0xFE, 0x7A, 0x11, 0x22, 0xB5, 0xDA, 0xDE, 0x35, 0xF2, 0xE9, 0xE9, 0x0A, 0x45, 0x70, 
            0xE2, 0x8E, 0x1E, 0xED, 0xAE, 0x49, 0x0D, 0x91, 0x9F, 0x87, 0xC6, 0x3F, 0x17, 0xB9, 0x15, 0x47, 
            0xC8, 0x96, 0x7A, 0xA9, 0x48, 0x9F, 0xD5, 0x31, 0x0F, 0xEE, 0xB8, 0xDE, 0xFA, 0x47, 0x06, 0xFA, 
            0xA4, 0x34, 0x26, 0xB4, 0x72, 0x84, 0xBD, 0x74, 0x61, 0x2B, 0x81, 0xB5, 0x81, 0x0E, 0x1D, 0x2F, 
            0x70, 0xBF, 0xBF, 0x28, 0x10, 0x9C, 0xBE, 0x08, 0x09, 0xC4, 0x74, 0x55, 0x38, 0x68, 0xB8, 0x2B, 
            0xAC, 0x43, 0x1D, 0xEE, 0xD9, 0x1B, 0xB9, 0xDE, 0xE7, 0xFF, 0xDB, 0xF3, 0x4E, 0x04, 0x8B, 0x70, 
            0x5F, 0xFE, 0x2D, 0xC8, 0x4A, 0x82, 0xAD, 0x1D, 0xF0, 0x68, 0xE3, 0x13, 0x05, 0x32, 0x8E, 0x8C, 
            0x4B, 0x77, 0x89, 0x69, 0xAF, 0xFC, 0x85, 0xA6, 0xE3, 0x56, 0x70, 0xC7, 0x66, 0xEC, 0xCB, 0xE0, 
            0x39, 0x59, 0x06, 0xBF, 0xBE, 0x6F, 0x8B, 0x83, 0xA2, 0xF8, 0xE3, 0xE7, 0x74, 0x78, 0x30, 0xD9, 
            0xA5, 0x96, 0xAD, 0x73, 0x74, 0x45, 0xB4, 0x07, 0xC3, 0x68, 0xE8, 0x81, 0xB9, 0xEC, 0xF6, 0xED, 
            0x12, 0x0F, 0xC9, 0x88, 0x2A, 0xCE, 0xF1, 0x26, 0xAB, 0xFA, 0x04, 0xBE, 0xE7, 0x7C, 0x4A, 0x64, 
            0xF4, 0x90, 0xB8, 0xF2, 0x9C, 0xF4, 0x8C, 0x61, 0xDA, 0x15, 0x66, 0xDF, 0x7C, 0xDE, 0x28, 0x2E, 
            0xE6, 0x02, 0x7C, 0x36, 0xFA, 0xD4, 0xD5, 0xE8, 0x01, 0x7D, 0x4B, 0xDF, 0x5F, 0x37, 0x9F, 0x51, 
            0xC5, 0x16, 0xEE, 0x30, 0xA4, 0x3A, 0xB9, 0xFC, 0x27, 0x96, 0x4C, 0xB8, 0xC0, 0x2D, 0xF8, 0x42, 
            0x36, 0x3A, 0x99, 0x52, 0xFE, 0x1F, 0xCB, 0x1A, 0xB8, 0x3C, 0xD8, 0x3A, 0x49, 0x05, 0xC8, 0x0E, 
            0x49, 0x38, 0x34, 0x2E, 0x13, 0x2D, 0x51, 0x1B, 0x7C, 0xAC, 0xC9, 0x38, 0x10, 0x84, 0xF6, 0x4E, 
            0xAD, 0x45, 0xB6, 0x87, 0x1D, 0x6A, 0xC2, 0xF1, 0x14, 0xDE, 0xA3, 0x94, 0xD0, 0xF2, 0x1D, 0x8C, 
            0x45, 0x2A, 0xFD, 0x5E, 0x8F, 0x3A, 0x1E, 0x68, 0x04, 0x26, 0xBE, 0x8C, 0x79, 0x7F, 0x46, 0x4A, 
            0xB6, 0x6B, 0xE2, 0x99, 0xB4, 0x11, 0x6F, 0x1D, 0xCE, 0x3D, 0xF9, 0xDD, 0x00, 0x69, 0x63, 0xD4, 
            0x0E, 0x74, 0x74, 0x99, 0x66, 0x5F, 0x28, 0xF1, 0xF9, 0x11, 0x83, 0x00, 0x75, 0x8C, 0x2B, 0x67, 
            0x78, 0xB0, 0x63, 0xA5, 0x23, 0x9B, 0xDF, 0x07, 0xD5, 0xEA, 0xAB, 0xFB, 0xA0, 0xBD, 0xC4, 0x41, 
            0xBB, 0xE5, 0xC8, 0xC2, 0x12, 0x8D, 0x3F, 0x43, 0x31, 0x59, 0x01, 0x78, 0x93, 0xEC, 0xB9, 0x46, 
            0x32, 0xFF, 0x90, 0x1D, 0xEE, 0xA9, 0x1B, 0x53, 0x61, 0x75, 0x24, 0x67, 0x1B, 0x69, 0x05, 0x77, 
            0x24, 0x85, 0x22, 0xFD, 0x8E, 0x5E, 0xA6, 0x69, 0xFA, 0x94, 0x10, 0xFC, 0xC1, 0x65, 0xA9, 0x95, 
            0x6B, 0xC0, 0x0B, 0xDE, 0x0B, 0x03, 0x76, 0x5D, 0x01, 0xF9, 0x9D, 0x77, 0x20, 0xDA, 0x36, 0x87, 
            0x6E, 0xDE, 0x9E, 0x35, 0xE3, 0xAD, 0xEE, 0x8E, 0x22, 0xD2, 0xB4, 0x44, 0xDB, 0x46, 0x73, 0xFB, 
            0x17, 0x51, 0x69, 0x9D, 0xDF, 0x20, 0x64, 0xBC, 0xD1, 0x54, 0x69, 0xA4, 0x6B, 0x8F, 0xA4, 0xDB, 
            0x0D, 0x35, 0xAE, 0x5C, 0xF2, 0x35, 0x19, 0x9A, 0xE4, 0xA9, 0x61, 0xDB, 0x04, 0xEB, 0x06, 0xD2
        };
    }
}