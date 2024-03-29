﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_38170 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[header.m_buildVersion & 511];
            uint increment = kidx % 61;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[kidx % 512];
                kidx += increment;
            }

            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = (uint)(digest[7] + (ushort)header.m_dataCount) & 511;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[kidx % 512];
                kidx += 3;
                buffer[i] ^= digest[(kidx - i) % SHA1_DIGESTSIZE];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x78, 0x3F, 0xEC, 0x15, 0xEB, 0xDA, 0xF2, 0xB0, 0x62, 0xD7, 0x0D, 0xCD, 0x15, 0x06, 0x62, 0x94,
            0xA3, 0x25, 0x6E, 0x17, 0x0C, 0x6F, 0x80, 0xCB, 0xEA, 0x6D, 0x88, 0x1E, 0x28, 0xA5, 0x9D, 0xD5,
            0x33, 0x0C, 0x0C, 0xC3, 0x3D, 0x1F, 0x26, 0x0F, 0x65, 0x7C, 0x43, 0xEA, 0x1F, 0xAE, 0x1F, 0x14,
            0xE8, 0x6B, 0xEE, 0x04, 0xEE, 0x62, 0xB7, 0xDA, 0xDF, 0x8C, 0x9C, 0x34, 0x2F, 0x93, 0x77, 0xD6,
            0xAE, 0x8C, 0x13, 0x3A, 0x74, 0x4E, 0xB4, 0x26, 0xBA, 0x89, 0xA2, 0x53, 0xA9, 0x33, 0xCF, 0x50,
            0x51, 0x9E, 0xC4, 0x01, 0xB6, 0x7C, 0x77, 0x7B, 0x9B, 0x31, 0x90, 0xA5, 0x1B, 0xF0, 0x1D, 0x29,
            0xEE, 0x35, 0x38, 0xE7, 0x9F, 0x13, 0xD5, 0x10, 0x2C, 0xF3, 0x95, 0xE4, 0xB0, 0xAA, 0xD0, 0x76,
            0x64, 0x3A, 0xDB, 0x5A, 0x37, 0xF4, 0xEB, 0x8E, 0x81, 0x7B, 0x24, 0x35, 0x81, 0x30, 0xD7, 0x43,
            0x5B, 0x0C, 0xAD, 0xCB, 0x78, 0x50, 0xFD, 0xAF, 0xE0, 0x51, 0xB2, 0xB4, 0x4A, 0x72, 0xC0, 0x3B,
            0xF8, 0xA3, 0x5A, 0x38, 0x49, 0x5E, 0x04, 0x25, 0xC8, 0xB8, 0xA5, 0xA4, 0x30, 0x0F, 0x7E, 0x47,
            0xFF, 0xA2, 0x19, 0x9A, 0xD0, 0x69, 0xC5, 0x98, 0xD8, 0xAC, 0xE3, 0xB7, 0x22, 0x96, 0x92, 0xF1,
            0x0E, 0x00, 0x9F, 0x23, 0xCD, 0x4B, 0x5A, 0xA8, 0xC5, 0x36, 0x2B, 0x77, 0x21, 0x57, 0xEC, 0x9A,
            0x48, 0xC4, 0xED, 0x13, 0x13, 0x06, 0x09, 0xE2, 0x08, 0x56, 0x38, 0x98, 0xA5, 0x50, 0x9B, 0x68,
            0x3D, 0xA4, 0x9A, 0x18, 0xCE, 0x99, 0x83, 0x69, 0xE9, 0x3D, 0xC9, 0xE8, 0x42, 0x60, 0xF8, 0x10,
            0xA8, 0xBB, 0x49, 0xE2, 0x6A, 0x85, 0x5B, 0x42, 0x36, 0x5D, 0x07, 0xFA, 0xED, 0xFE, 0x59, 0xDB,
            0x80, 0x11, 0x7F, 0xE0, 0x7B, 0xF3, 0x4A, 0x28, 0xDA, 0xF9, 0x8C, 0x7B, 0x4A, 0x33, 0x42, 0xB3,
            0xFC, 0xC8, 0xB0, 0x4F, 0x74, 0xB8, 0x3D, 0xFE, 0xED, 0x65, 0x1B, 0x9D, 0x89, 0xB2, 0x68, 0x94,
            0x48, 0x86, 0x3D, 0x16, 0x9F, 0xE6, 0x4C, 0x80, 0xB9, 0x6A, 0xA6, 0xDA, 0xA1, 0x8A, 0x7D, 0x8D,
            0xB6, 0x3B, 0x7D, 0x81, 0x38, 0xEE, 0xEB, 0x95, 0xBF, 0x57, 0x4A, 0xCB, 0xE6, 0x8A, 0x34, 0x2A,
            0xAB, 0x05, 0xE5, 0xB8, 0x23, 0xF2, 0x62, 0x55, 0x0D, 0xDE, 0xF0, 0x81, 0x1C, 0xF3, 0xAF, 0xE3,
            0x04, 0x65, 0xBD, 0xA4, 0xA1, 0xA2, 0x52, 0x00, 0x38, 0x54, 0xE4, 0x8D, 0xFB, 0x61, 0x4C, 0x52,
            0xCB, 0x1E, 0xAE, 0x22, 0xB6, 0xB7, 0x77, 0x40, 0xA4, 0xA4, 0xAA, 0xC8, 0x93, 0xA9, 0xDB, 0xF7,
            0xF3, 0xF3, 0x61, 0xB7, 0xAA, 0x58, 0x0A, 0x42, 0xC2, 0x29, 0x12, 0x1B, 0xC6, 0xE1, 0xD6, 0x27,
            0xF8, 0xB7, 0x83, 0x4C, 0x6D, 0xBD, 0xB2, 0xC2, 0xA1, 0xCE, 0x29, 0xFC, 0xE5, 0xE2, 0xC5, 0xE0,
            0x6D, 0x39, 0x26, 0x8C, 0x16, 0xB4, 0x69, 0xA8, 0xEA, 0xE1, 0xC7, 0x45, 0x0E, 0x0D, 0xF4, 0xDB,
            0x9C, 0xAC, 0x3E, 0xB9, 0x0D, 0x08, 0x75, 0x8E, 0x5F, 0x8A, 0xC5, 0x18, 0xA8, 0xD5, 0x34, 0xB2,
            0x00, 0xC3, 0x27, 0x56, 0x49, 0xA2, 0x9E, 0x46, 0x48, 0x25, 0xFE, 0x27, 0x13, 0x8A, 0x7C, 0x64,
            0x94, 0x25, 0xCD, 0xA9, 0xEC, 0xDB, 0xE6, 0x4B, 0x39, 0x8D, 0xAE, 0x1F, 0x31, 0x2C, 0x5F, 0x37,
            0xE6, 0x08, 0x24, 0xC6, 0x43, 0xDD, 0xF8, 0x48, 0x5D, 0x87, 0x1A, 0x20, 0xBD, 0x12, 0xFE, 0x57,
            0x7E, 0x12, 0x1D, 0x91, 0x90, 0x7C, 0x90, 0xFB, 0x80, 0x3D, 0xCA, 0x37, 0xF9, 0xDA, 0x7A, 0x75,
            0xC6, 0x30, 0x2B, 0x62, 0x61, 0xCA, 0xE1, 0xA9, 0xB8, 0x03, 0xB1, 0xF5, 0xBD, 0xA1, 0x76, 0xA0,
            0x77, 0xDE, 0xEA, 0xDC, 0xEC, 0x80, 0x69, 0x27, 0x52, 0xC2, 0x34, 0x33, 0x17, 0xDC, 0x1E, 0x97
        };
    }
}