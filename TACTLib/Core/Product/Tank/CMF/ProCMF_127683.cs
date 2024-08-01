using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_127683 : ICMFEncryptionProc
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
                kidx += ((digest[6] & 1) != 0) ? 37 : (okidx % 61);
                buffer[i] ^= digest[SignedMod(kidx - i, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xFB, 0x84, 0x8E, 0xC8, 0x6E, 0xB7, 0x45, 0x75, 0x75, 0x2A, 0x06, 0x1F, 0xBA, 0x68, 0x90, 0x51, 
            0x78, 0xDB, 0xC7, 0xC1, 0x20, 0xCE, 0x09, 0x07, 0x61, 0x98, 0x5F, 0x60, 0xDF, 0x02, 0x61, 0x24, 
            0xDA, 0xA7, 0x90, 0x58, 0xCA, 0x22, 0x96, 0xC7, 0x2C, 0xEE, 0x9B, 0xB1, 0x15, 0x13, 0xDA, 0xFE, 
            0x85, 0x0E, 0x1C, 0x88, 0x88, 0x60, 0x86, 0x36, 0xDD, 0xEB, 0xE5, 0x6D, 0xB1, 0xCE, 0xFA, 0x25, 
            0xE5, 0x14, 0x10, 0x0D, 0xA9, 0xF9, 0x14, 0xDA, 0x24, 0x72, 0x1E, 0xC4, 0xE9, 0x5D, 0x4D, 0xDE, 
            0xC0, 0x1D, 0x33, 0x9C, 0x2D, 0x6A, 0xFB, 0x90, 0xDC, 0x2B, 0xBB, 0xEE, 0xB5, 0x28, 0xFE, 0x9A, 
            0xA1, 0xA1, 0x38, 0xF9, 0xB9, 0xAA, 0xCB, 0x0F, 0x57, 0x2A, 0xF8, 0xAE, 0xDD, 0x44, 0xCA, 0x29, 
            0x40, 0xAD, 0x3B, 0x20, 0x67, 0x7C, 0xFE, 0xF5, 0xE0, 0x12, 0xFB, 0xDD, 0x0D, 0xFC, 0x7B, 0xC9, 
            0xBD, 0x46, 0xE0, 0x29, 0xF6, 0x14, 0xD1, 0x30, 0xC7, 0x4E, 0x5D, 0xE3, 0x5B, 0xC9, 0xCA, 0x5E, 
            0xC6, 0xB2, 0xD1, 0x51, 0xB1, 0xBD, 0x99, 0x2F, 0x00, 0x4B, 0xB6, 0x0F, 0xA7, 0x63, 0x79, 0xDA, 
            0x63, 0xD0, 0x9E, 0x67, 0xB6, 0x48, 0x39, 0xEA, 0xE8, 0x58, 0xBE, 0x40, 0x09, 0xD7, 0x3E, 0x9E, 
            0x89, 0x3B, 0xCF, 0xF8, 0x91, 0xD8, 0x1D, 0xB5, 0x68, 0x62, 0x48, 0x09, 0x8F, 0xBB, 0xBC, 0x0A, 
            0x5F, 0xDA, 0xFF, 0xEA, 0xFF, 0x67, 0x2B, 0xEA, 0xB9, 0xD4, 0x62, 0xF7, 0xF6, 0x7C, 0xAE, 0x2F, 
            0x07, 0xA1, 0x4C, 0xA5, 0xD1, 0x22, 0xAA, 0xC1, 0x40, 0xBB, 0xB8, 0xF5, 0x32, 0x33, 0x2C, 0xBC, 
            0xA5, 0x23, 0x38, 0x7F, 0x9D, 0x4F, 0x74, 0x78, 0x49, 0xB1, 0x3D, 0xC9, 0x1A, 0xFC, 0x55, 0x22, 
            0x23, 0x51, 0x53, 0x17, 0xA0, 0x0C, 0x0C, 0xAC, 0xCC, 0xAF, 0x4B, 0x03, 0x97, 0x3B, 0xBC, 0xDB, 
            0xDB, 0x22, 0x38, 0x3A, 0x9D, 0x56, 0xCA, 0x6B, 0x62, 0x70, 0x85, 0x81, 0x2D, 0x96, 0x3B, 0xEA, 
            0x4D, 0xAF, 0xEA, 0xC4, 0x21, 0x1F, 0xEB, 0x05, 0x91, 0x8A, 0xBC, 0x0A, 0x9F, 0xD9, 0x0C, 0x43, 
            0x8B, 0x47, 0xA7, 0x65, 0x97, 0x37, 0xC8, 0x39, 0x64, 0x00, 0x93, 0x57, 0x52, 0x5A, 0xB6, 0x41, 
            0xD5, 0x7B, 0x78, 0xB1, 0xD3, 0xF2, 0x20, 0xCE, 0x28, 0x6D, 0xA2, 0x78, 0xF4, 0x89, 0xBD, 0x48, 
            0x18, 0x10, 0xB0, 0xCD, 0xBE, 0x1E, 0xF7, 0xDC, 0x42, 0x30, 0x20, 0x2B, 0x4D, 0xC4, 0xEC, 0x40, 
            0x57, 0x8E, 0x25, 0xF6, 0x92, 0xFE, 0xA2, 0x07, 0x03, 0xC5, 0xF9, 0x41, 0x06, 0xA3, 0x39, 0x4D, 
            0xFE, 0x50, 0x71, 0x0F, 0xC4, 0x8A, 0xE9, 0xBE, 0xDB, 0xF2, 0x96, 0x04, 0xBB, 0x44, 0xB9, 0x62, 
            0xC8, 0xF5, 0x43, 0xF9, 0xB3, 0xBA, 0xAD, 0x05, 0x92, 0xF3, 0xB5, 0x2C, 0xD8, 0x43, 0x09, 0xF4, 
            0xAC, 0xD6, 0xF5, 0x65, 0x00, 0xEB, 0x53, 0x60, 0x49, 0x12, 0xBC, 0x96, 0x1D, 0xEE, 0x81, 0x37, 
            0x87, 0x9B, 0xEB, 0x58, 0x5F, 0xFA, 0x32, 0xE0, 0x0E, 0xD3, 0x9B, 0x6B, 0x05, 0x58, 0xCA, 0x2F, 
            0xF5, 0x95, 0xEF, 0x33, 0xD3, 0x8C, 0xBE, 0x2C, 0x06, 0x6F, 0x48, 0x45, 0x7A, 0xE3, 0x0C, 0x7A, 
            0x6A, 0x50, 0xC9, 0xFB, 0x0E, 0xC9, 0xA2, 0x0A, 0x66, 0xBE, 0xB6, 0xA2, 0x88, 0x77, 0x65, 0xF4, 
            0xF3, 0x5E, 0xCB, 0xB8, 0xF6, 0x20, 0xAA, 0xD7, 0xC5, 0xC9, 0xB2, 0xFD, 0x10, 0xF4, 0xAA, 0x60, 
            0x79, 0xB9, 0x11, 0xA8, 0x9A, 0xBA, 0x95, 0x3C, 0x69, 0x7B, 0xAF, 0xD3, 0xB0, 0xB4, 0x73, 0xFB, 
            0x0F, 0x13, 0x0E, 0xFC, 0xAB, 0x6C, 0x07, 0x09, 0xB7, 0x03, 0x98, 0x2C, 0xB1, 0x89, 0xCC, 0xD3, 
            0xF9, 0x46, 0x68, 0x25, 0x04, 0x08, 0x48, 0xCF, 0xB4, 0x9F, 0xBE, 0x3F, 0x67, 0x96, 0x31, 0xCB
        };
    }
}