﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_45752 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[SignedMod(length * Keytable[0], 512)];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += 3;
            }

            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = (uint)length * header.m_buildVersion;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx = header.m_buildVersion - kidx;
                buffer[i] ^= digest[SignedMod(i + kidx, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xF5, 0xD0, 0xE7, 0x8D, 0x86, 0xD6, 0x22, 0xA2, 0xA6, 0xEC, 0x4B, 0xCD, 0x72, 0x4A, 0xD4, 0x8B, 
            0xC7, 0xE4, 0x5B, 0x55, 0x25, 0xF6, 0xEE, 0xBE, 0xA5, 0x24, 0x1B, 0xFA, 0x50, 0xEF, 0x0C, 0xE7, 
            0xA4, 0xA8, 0xE4, 0xF5, 0x97, 0x2B, 0xCF, 0xF6, 0xBB, 0x06, 0x75, 0x6A, 0xF4, 0x61, 0xE4, 0xF2, 
            0xCA, 0xBC, 0x7B, 0x23, 0x87, 0x49, 0x99, 0x83, 0x28, 0xFC, 0x88, 0xB8, 0x23, 0x9C, 0xFF, 0xB9, 
            0x8B, 0x89, 0x7F, 0x08, 0x42, 0xF5, 0x06, 0xED, 0x5D, 0x7C, 0x98, 0x6B, 0x2E, 0x37, 0xB2, 0xDE, 
            0xBD, 0xAB, 0x65, 0x79, 0xAA, 0xDA, 0x70, 0x83, 0x30, 0xA0, 0xA4, 0x4D, 0x68, 0xFF, 0x4B, 0x32, 
            0x0F, 0x97, 0x0A, 0x73, 0xDC, 0x77, 0x67, 0x10, 0x89, 0xB6, 0xE7, 0xBE, 0x96, 0xD0, 0x15, 0x24, 
            0x13, 0xB6, 0x7F, 0xA8, 0x18, 0xEF, 0x53, 0x83, 0xA0, 0x9E, 0x50, 0x29, 0xE7, 0x5E, 0x5F, 0xAC, 
            0x25, 0x11, 0xA2, 0xA2, 0x6D, 0xE2, 0xBB, 0xE6, 0x60, 0x14, 0x25, 0x07, 0xC5, 0xB9, 0xB1, 0x37, 
            0x2C, 0x34, 0x6B, 0xC6, 0x95, 0x04, 0x73, 0xE5, 0xD7, 0x01, 0x78, 0x8E, 0x0E, 0xDF, 0x5D, 0x30, 
            0xFE, 0x1D, 0x81, 0xC3, 0x53, 0x17, 0xB7, 0x6F, 0xF7, 0x99, 0x6E, 0xB5, 0x19, 0x12, 0xD6, 0x1F, 
            0x04, 0x48, 0x82, 0x07, 0x22, 0x71, 0x39, 0x0A, 0x79, 0x1B, 0x5F, 0x6A, 0x4C, 0xED, 0xF4, 0x3E, 
            0xC7, 0x84, 0x45, 0x41, 0x5F, 0xAF, 0x78, 0x13, 0xB0, 0xB4, 0x82, 0xD7, 0x21, 0x9C, 0x33, 0x24, 
            0x8A, 0x54, 0xF9, 0xC4, 0xC9, 0x1A, 0x1E, 0x56, 0x21, 0x93, 0xEF, 0xFC, 0x1A, 0x5E, 0x2F, 0x68, 
            0x74, 0x27, 0x64, 0xEC, 0xBD, 0x1B, 0x0F, 0x94, 0xFD, 0xD7, 0x85, 0x0F, 0x83, 0x6B, 0x6F, 0xAC, 
            0xD3, 0x64, 0xFC, 0xD0, 0x6B, 0x4E, 0xB8, 0x86, 0x8A, 0x45, 0xAE, 0x31, 0x55, 0xBA, 0xAF, 0xB6, 
            0x50, 0x5C, 0x4E, 0xE4, 0xF4, 0x1A, 0xE8, 0xFC, 0xAD, 0xB6, 0xE6, 0xBF, 0x5F, 0x35, 0x63, 0x1A, 
            0xBD, 0xC2, 0x6C, 0xCD, 0x28, 0xEE, 0xB6, 0xA2, 0x6B, 0x7A, 0xD7, 0xBC, 0xF3, 0xC4, 0x9F, 0xD6, 
            0x2D, 0xC4, 0xB7, 0xBF, 0xEF, 0x44, 0xFA, 0x3D, 0xA9, 0x70, 0x19, 0x34, 0x32, 0x88, 0xD9, 0x37, 
            0x4D, 0x78, 0x2E, 0xCB, 0x7A, 0x11, 0xEF, 0xCD, 0x17, 0xDB, 0xD6, 0x87, 0x3F, 0x75, 0x77, 0xEA, 
            0x0A, 0xE1, 0x65, 0x02, 0x91, 0xD1, 0x4C, 0x11, 0x75, 0xF0, 0x35, 0x87, 0xC6, 0xF9, 0x22, 0x47, 
            0x15, 0x96, 0xB3, 0x8D, 0xF9, 0xD5, 0x0E, 0x87, 0x7F, 0x47, 0x33, 0x18, 0x1E, 0x75, 0xA3, 0xC1, 
            0xDB, 0x32, 0x63, 0x9F, 0xA4, 0xA1, 0x46, 0x4C, 0xB3, 0x3D, 0x44, 0x11, 0x64, 0x34, 0x27, 0x01, 
            0xE5, 0x44, 0x0C, 0x16, 0x6A, 0xC7, 0x77, 0xB4, 0x31, 0xF2, 0x44, 0x2A, 0xFD, 0x7B, 0x04, 0xF7, 
            0xD4, 0x3D, 0x3C, 0x80, 0x0A, 0x25, 0xC4, 0xBF, 0xE4, 0x34, 0x65, 0x17, 0x18, 0x02, 0x54, 0xA3, 
            0xA7, 0x00, 0x40, 0xFE, 0x10, 0xDB, 0x24, 0x69, 0xFD, 0xF8, 0x2E, 0x53, 0xB1, 0x80, 0x5A, 0xF9, 
            0x59, 0x2D, 0xA7, 0x51, 0x66, 0xD1, 0x66, 0xFC, 0x7D, 0x15, 0xCD, 0x88, 0x45, 0x9D, 0x66, 0x63, 
            0x79, 0xE7, 0x18, 0x10, 0x0B, 0x57, 0x73, 0x1D, 0xD5, 0xDC, 0x3E, 0xA6, 0x84, 0x36, 0xB2, 0xD0, 
            0x25, 0x52, 0xBE, 0xFE, 0xE1, 0x77, 0xF0, 0x0B, 0x77, 0x7F, 0xF2, 0xED, 0xB6, 0xD8, 0x9A, 0x32, 
            0xA1, 0xA8, 0xAB, 0xC3, 0xEC, 0xE9, 0xFD, 0x5B, 0xF0, 0x4E, 0xCD, 0x3D, 0xA2, 0xBF, 0x71, 0xAC, 
            0x09, 0x67, 0x5E, 0x6B, 0x44, 0xA5, 0xC5, 0xEC, 0xBC, 0xC1, 0x23, 0x0B, 0x0C, 0x70, 0x5D, 0xF2, 
            0x69, 0x59, 0x7E, 0x6E, 0xED, 0xEF, 0x92, 0xA9, 0x52, 0xBC, 0xCC, 0xEA, 0x97, 0xFD, 0xB4, 0xDD
        };
    }
}