﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_42539 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[length + 256];
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
                kidx -= 43;
                buffer[i] ^= digest[SignedMod(kidx + header.m_buildVersion, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x7F, 0x81, 0xB1, 0x26, 0x5F, 0xCC, 0x7B, 0x70, 0xD6, 0x8F, 0x1E, 0x4E, 0x37, 0xFF, 0x9C, 0x31,
            0xBD, 0xC7, 0x03, 0x04, 0x16, 0x14, 0x8E, 0xA0, 0x1A, 0x66, 0xE0, 0x23, 0xF8, 0xAB, 0x3C, 0xEC,
            0x99, 0x0D, 0x9C, 0x35, 0x52, 0xB2, 0x76, 0x6A, 0x7A, 0x66, 0x89, 0xA2, 0xFF, 0x1D, 0xC1, 0x90,
            0xD7, 0x22, 0x2C, 0x64, 0xD9, 0x58, 0x36, 0x96, 0x10, 0xD2, 0x26, 0x36, 0xFB, 0xB1, 0x74, 0x41,
            0x9C, 0x6C, 0x2E, 0x30, 0x58, 0x32, 0x99, 0xCC, 0x8C, 0x42, 0x7B, 0xFF, 0x93, 0x66, 0x45, 0x5C,
            0x86, 0xF4, 0x68, 0x6F, 0xAD, 0x47, 0xFF, 0xA4, 0xBA, 0x63, 0xF4, 0xED, 0xCF, 0x1E, 0x77, 0x2C,
            0x2B, 0x6B, 0x64, 0x2B, 0x30, 0xEC, 0x14, 0x32, 0x9D, 0xAF, 0xF7, 0xB6, 0x51, 0xAF, 0x0A, 0xA0,
            0xF3, 0x1E, 0x30, 0x23, 0xBD, 0x69, 0x09, 0x86, 0x17, 0xE4, 0xCC, 0xF0, 0x18, 0x06, 0xC3, 0x7D,
            0x2D, 0x6D, 0x46, 0xE8, 0xA4, 0xDE, 0x77, 0x51, 0xAE, 0xEC, 0x2F, 0x98, 0x4C, 0x7A, 0x79, 0x21,
            0x1D, 0x94, 0x11, 0x01, 0x68, 0xD9, 0x59, 0x21, 0xE3, 0x73, 0x4C, 0xD3, 0xA4, 0xC7, 0x23, 0x7C,
            0x11, 0x67, 0x2F, 0x3B, 0x50, 0xB1, 0x7C, 0x00, 0xD2, 0x65, 0xDB, 0xFA, 0x0B, 0x3F, 0x3D, 0xF7,
            0xB1, 0x46, 0x23, 0x7F, 0x35, 0x29, 0x06, 0xB4, 0x09, 0xA8, 0xA5, 0x44, 0x68, 0x1A, 0x5D, 0x20,
            0xD7, 0x0A, 0x9A, 0x87, 0xBC, 0x4B, 0x38, 0x23, 0x56, 0x1D, 0xF5, 0x04, 0x83, 0xC4, 0xDF, 0x0F,
            0x74, 0xF3, 0xF7, 0x1B, 0xCA, 0x17, 0xE3, 0x45, 0xFD, 0x6A, 0x41, 0xCF, 0x16, 0x71, 0x95, 0x08,
            0x2E, 0xBD, 0x00, 0x04, 0x73, 0xC0, 0xEC, 0xCD, 0x5F, 0x63, 0xC4, 0xA5, 0xAE, 0x42, 0xEA, 0x33,
            0x81, 0x3B, 0x17, 0xB0, 0x93, 0xAB, 0x6F, 0x25, 0xB7, 0xE8, 0x5F, 0xAA, 0xF3, 0x57, 0xB9, 0xF9,
            0x4D, 0xA0, 0xCC, 0x6E, 0x9F, 0x89, 0x9F, 0x53, 0xE1, 0xB5, 0xE3, 0xB4, 0xCC, 0x91, 0xAE, 0x88,
            0x66, 0x98, 0xF7, 0xD6, 0x2C, 0x24, 0x26, 0x41, 0x75, 0xD0, 0x6F, 0xAB, 0xAC, 0xA2, 0x85, 0xD6,
            0x25, 0xC6, 0x06, 0xEE, 0xE9, 0x49, 0xDC, 0xE2, 0x56, 0x97, 0x6B, 0xB3, 0x11, 0x8C, 0xCC, 0x3F,
            0x8B, 0x44, 0x91, 0x34, 0x07, 0x25, 0x14, 0x56, 0x78, 0x4F, 0x22, 0xC1, 0x00, 0x90, 0x47, 0xC1,
            0xC2, 0xBD, 0xF7, 0x74, 0xE8, 0x84, 0x59, 0x43, 0x2D, 0xCB, 0xDF, 0xDA, 0x56, 0x6D, 0xC0, 0xEA,
            0xE3, 0x93, 0xC6, 0x4B, 0x02, 0xBA, 0xB9, 0x1A, 0xBC, 0xD4, 0x18, 0x07, 0xDF, 0x42, 0x99, 0xAF,
            0xD3, 0xB9, 0x47, 0x6B, 0x15, 0x15, 0x0F, 0x74, 0x57, 0xF8, 0x9A, 0x8D, 0x89, 0x99, 0x4D, 0x07,
            0xDD, 0x1E, 0x52, 0xE8, 0xC0, 0x68, 0x4B, 0x22, 0x8E, 0xA0, 0x4A, 0x57, 0x1E, 0x2D, 0x18, 0xDA,
            0x93, 0xB7, 0xFE, 0x36, 0xF7, 0x28, 0x86, 0x2B, 0x7E, 0xBD, 0x7C, 0x0C, 0xE9, 0xDA, 0x2F, 0x2A,
            0xD7, 0x1C, 0x55, 0xC3, 0xD1, 0x4C, 0x96, 0xD2, 0x07, 0x5D, 0x8D, 0x1A, 0xB7, 0x3D, 0x7E, 0x8E,
            0xD2, 0x5D, 0xC6, 0x90, 0x00, 0xF3, 0x23, 0xDE, 0x36, 0xCE, 0xA6, 0x88, 0x1C, 0x77, 0xAD, 0x25,
            0xCD, 0x92, 0x46, 0x7D, 0x97, 0x7A, 0xB6, 0x97, 0x36, 0xF9, 0xBC, 0x0B, 0xD3, 0x6A, 0x01, 0x0D,
            0x7D, 0x0B, 0x49, 0x84, 0xA4, 0x29, 0x34, 0x68, 0x09, 0x31, 0x1C, 0x3C, 0xEB, 0xCA, 0x2A, 0x25,
            0x2E, 0xCB, 0xD0, 0xAE, 0x35, 0x9C, 0xBA, 0xEA, 0x8A, 0x87, 0x6C, 0x9B, 0x8B, 0xC8, 0x68, 0xAA,
            0x87, 0x4B, 0xFF, 0xEC, 0xAB, 0x0B, 0x6F, 0xD6, 0x94, 0x95, 0xC4, 0x61, 0x48, 0x65, 0x62, 0x56,
            0x1E, 0x4D, 0x25, 0x5F, 0xB4, 0x34, 0xFA, 0x86, 0xDC, 0x43, 0x2C, 0x1F, 0x77, 0xF6, 0x53, 0x6D
        };
    }
}