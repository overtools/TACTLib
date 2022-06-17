using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_98576 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];
            uint kidx, okidx;
            kidx = okidx = Keytable[header.m_dataCount & 511];
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
                buffer[i] ^= digest[SignedMod(kidx + header.m_buildVersion, SHA1_DIGESTSIZE)];
            }
            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x1D, 0x0A, 0x9C, 0xCA, 0x37, 0x56, 0x47, 0x24, 0x94, 0x23, 0xA7, 0xBA, 0xB6, 0x1B, 0xC8, 0x06, 
            0x3D, 0xE6, 0x02, 0x98, 0xCC, 0x36, 0xCD, 0xCE, 0x10, 0x21, 0x2F, 0x80, 0x87, 0x28, 0xE7, 0xD2, 
            0x71, 0x17, 0x85, 0x66, 0xB3, 0x0D, 0xBC, 0xBE, 0x1B, 0x4B, 0x2F, 0x48, 0xE2, 0xD9, 0x7C, 0x7C, 
            0x05, 0x1B, 0x94, 0x20, 0xE9, 0x00, 0x63, 0x63, 0xAF, 0xEF, 0x19, 0x25, 0xCF, 0x0B, 0x37, 0x3A, 
            0x7E, 0x67, 0xFE, 0x64, 0x7F, 0x17, 0x38, 0xCB, 0x84, 0x08, 0x96, 0x0D, 0xEE, 0x8E, 0xCF, 0xB2, 
            0xA5, 0xFE, 0x74, 0x2E, 0x45, 0xDF, 0x37, 0xAE, 0xBB, 0xA3, 0xAD, 0xFD, 0x14, 0xE3, 0xD0, 0x21, 
            0xC0, 0x87, 0xC7, 0x6F, 0x7A, 0x16, 0xC7, 0x8F, 0x3B, 0x77, 0x62, 0x52, 0xD8, 0x3A, 0x61, 0x09, 
            0x1F, 0x73, 0x23, 0x33, 0xC2, 0x60, 0x27, 0x4A, 0x52, 0x89, 0xD8, 0xFC, 0x87, 0xF9, 0xE9, 0x5C, 
            0x72, 0x57, 0xE0, 0xB3, 0x3D, 0x24, 0x4F, 0xC8, 0xCF, 0x92, 0x42, 0x12, 0x22, 0xEF, 0x25, 0xFE, 
            0x9A, 0x4E, 0x7A, 0xC4, 0xEB, 0xFC, 0x78, 0xD0, 0x9F, 0x83, 0x00, 0xE7, 0x7E, 0x1D, 0x9E, 0x12, 
            0x99, 0xFD, 0x0A, 0xD2, 0xFD, 0x4E, 0xE0, 0x53, 0xF8, 0xFF, 0x69, 0x91, 0x61, 0x56, 0x1C, 0xB2, 
            0x17, 0xA8, 0x7A, 0x0C, 0x63, 0xD6, 0x07, 0xDA, 0x53, 0x7B, 0xBB, 0x3B, 0xCC, 0x63, 0xB1, 0x4C, 
            0x92, 0xF3, 0x31, 0x36, 0x92, 0x68, 0x7C, 0x2C, 0xC2, 0x71, 0x5C, 0xFD, 0x30, 0x53, 0xCC, 0xA1, 
            0xEB, 0x92, 0x25, 0xEA, 0x3B, 0x8A, 0xA1, 0xB8, 0x79, 0x22, 0x2E, 0x42, 0x5E, 0xA9, 0x71, 0x90, 
            0xDC, 0x09, 0x50, 0x52, 0x48, 0xFD, 0x4B, 0x37, 0x32, 0x0B, 0x3E, 0x15, 0x04, 0x7F, 0xB7, 0xEB, 
            0x66, 0x20, 0xB3, 0x23, 0xC3, 0x29, 0x3F, 0xB8, 0xB4, 0xA2, 0x1F, 0x98, 0x24, 0x10, 0xE3, 0xF6, 
            0xA5, 0xB3, 0x5C, 0x6B, 0xD6, 0x0B, 0xFB, 0x72, 0x2B, 0xC0, 0x76, 0x26, 0xA2, 0x07, 0xDB, 0x1A, 
            0x4F, 0x38, 0x21, 0x01, 0x73, 0x86, 0x56, 0xB9, 0xED, 0xDC, 0xDF, 0x3F, 0x96, 0x74, 0x2F, 0x76, 
            0x5A, 0x01, 0x6F, 0xCE, 0x70, 0x37, 0x29, 0x42, 0xC7, 0xFC, 0x2E, 0x69, 0x12, 0xF8, 0xC8, 0xA4, 
            0x97, 0xFE, 0x7B, 0x3B, 0x87, 0x46, 0x5E, 0xE5, 0x6A, 0x5E, 0x7F, 0xB0, 0x1F, 0x33, 0x50, 0x9C, 
            0x04, 0x3D, 0xFB, 0x32, 0x83, 0xE9, 0xFD, 0xCD, 0x74, 0x27, 0x6E, 0x51, 0xC6, 0xCF, 0xBB, 0x79, 
            0xD4, 0x63, 0x71, 0x10, 0xF2, 0xDB, 0x29, 0x22, 0x7F, 0x04, 0xCF, 0xEF, 0xBF, 0xBD, 0x26, 0x65, 
            0xE0, 0x7C, 0x72, 0xBC, 0xBD, 0xC3, 0x91, 0xEF, 0x4E, 0xC5, 0xC7, 0x67, 0xBC, 0x47, 0x68, 0xE1, 
            0x7F, 0xBA, 0x4A, 0x56, 0xF4, 0x5F, 0xDE, 0x13, 0x48, 0xB5, 0x89, 0x3F, 0xF8, 0xBA, 0xAB, 0x8F, 
            0xAE, 0x02, 0x25, 0x63, 0xBA, 0x86, 0x45, 0xA1, 0x40, 0xF8, 0xF4, 0xDC, 0x96, 0x7B, 0xED, 0xF8, 
            0x99, 0xA5, 0x4D, 0xB2, 0x99, 0x7C, 0xF0, 0x4B, 0x6C, 0x5D, 0x91, 0x3A, 0xC3, 0x7B, 0x32, 0x41, 
            0x26, 0x54, 0x08, 0xA7, 0x09, 0x9D, 0x95, 0x9E, 0x41, 0xB4, 0xC6, 0xC8, 0x65, 0x89, 0xF7, 0xBF, 
            0xC0, 0x73, 0x10, 0x8F, 0xD4, 0xCE, 0x69, 0x05, 0x16, 0x8C, 0x32, 0xCA, 0x72, 0x7B, 0xB9, 0xB1, 
            0xF9, 0x69, 0x99, 0xAF, 0x46, 0x40, 0x3D, 0x91, 0x32, 0xA0, 0xD6, 0x1B, 0xBE, 0x69, 0x38, 0x8A, 
            0x50, 0x07, 0x6B, 0x7B, 0x1A, 0x06, 0xB2, 0x0F, 0x81, 0xA7, 0x62, 0xF3, 0x1D, 0xE2, 0xC0, 0x9B, 
            0xBF, 0xFB, 0xEE, 0xD1, 0x62, 0xF3, 0x30, 0xEB, 0xD1, 0x59, 0x71, 0xC1, 0xB0, 0xEC, 0x4B, 0x67, 
            0x8A, 0x13, 0x7A, 0xAE, 0x51, 0x00, 0x2C, 0xD2, 0x22, 0x8C, 0x01, 0x7D, 0x6E, 0xE7, 0x16, 0x26
        };
    }
}