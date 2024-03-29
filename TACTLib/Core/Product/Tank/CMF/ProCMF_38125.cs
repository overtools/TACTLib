﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_38125 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[header.m_buildVersion & 511];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[kidx % 512];
                kidx -= header.m_buildVersion;
            }

            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[header.m_buildVersion & 511];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[kidx % 512];
                kidx -= 43;
                buffer[i] ^= (byte)(digest[(kidx + header.m_dataCount) % SHA1_DIGESTSIZE] % 0xFF);
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x38, 0xBD, 0xA8, 0x9E, 0x49, 0xC2, 0xBD, 0x0F, 0x21, 0x9E, 0x44, 0x7A, 0x8E, 0xD2, 0x22, 0x4E,
            0x36, 0x75, 0xEC, 0x49, 0x91, 0xEB, 0x73, 0x17, 0x3C, 0x02, 0x1D, 0x14, 0x9B, 0x18, 0xDF, 0x23,
            0xD5, 0x70, 0x41, 0x12, 0x0C, 0x98, 0x9B, 0x3E, 0x63, 0x5B, 0x67, 0x26, 0x5F, 0x4C, 0x95, 0x25,
            0x46, 0x24, 0x07, 0x48, 0x4E, 0xEF, 0x82, 0x6D, 0xBA, 0xDA, 0xCD, 0x0A, 0xEA, 0xF0, 0x43, 0x60,
            0x24, 0xEA, 0xDD, 0x39, 0x86, 0x0E, 0x2C, 0xB2, 0xE1, 0xB3, 0x59, 0xA2, 0xB6, 0x4B, 0xC6, 0x9F,
            0xB6, 0x18, 0xF2, 0xFC, 0xEE, 0xA2, 0x42, 0x72, 0x4C, 0x82, 0xCA, 0xC2, 0x6D, 0x55, 0xEF, 0x05,
            0x2D, 0x0C, 0x6B, 0x66, 0x51, 0xFC, 0xB3, 0xC9, 0x92, 0xB3, 0x19, 0xDE, 0xAA, 0x4E, 0x3D, 0x26,
            0xE3, 0x1A, 0x2C, 0xA1, 0xA0, 0x7B, 0x5F, 0xF4, 0xF5, 0xF8, 0x43, 0x61, 0x10, 0x31, 0xDE, 0x45,
            0xEE, 0xA4, 0xA9, 0x92, 0x9E, 0x89, 0x66, 0x1C, 0x4F, 0x52, 0x03, 0x21, 0xA5, 0x81, 0x7C, 0x66,
            0x4A, 0x28, 0xF4, 0x22, 0x2C, 0xC6, 0xBC, 0x0C, 0xFF, 0x26, 0x15, 0x19, 0xBC, 0x88, 0xC9, 0x60,
            0x52, 0x71, 0x89, 0x1B, 0xFD, 0xB0, 0x59, 0x37, 0x7B, 0xE7, 0xD5, 0xCD, 0x78, 0x4B, 0x76, 0xF7,
            0x56, 0x81, 0x64, 0x22, 0x64, 0xA7, 0xFA, 0x18, 0x13, 0x50, 0x54, 0x03, 0x71, 0xD2, 0x58, 0xE4,
            0x7E, 0xA6, 0x86, 0x44, 0x5E, 0x94, 0x6D, 0x86, 0xD2, 0xCC, 0x10, 0xC8, 0xE3, 0xF4, 0x23, 0xA5,
            0x53, 0x26, 0x35, 0x68, 0xD4, 0x67, 0x05, 0xC1, 0x69, 0x4D, 0x68, 0x8F, 0x6B, 0x8D, 0x7F, 0x23,
            0xB8, 0x3A, 0x85, 0x8A, 0x19, 0x36, 0x17, 0xCE, 0xC8, 0xBA, 0x1D, 0x84, 0x2F, 0xBD, 0x4E, 0xCF,
            0x10, 0x3B, 0x16, 0x35, 0x13, 0xD2, 0x31, 0x15, 0x0E, 0x68, 0x0C, 0x27, 0xE5, 0x68, 0x43, 0x1A,
            0x6A, 0x45, 0x43, 0x08, 0x63, 0xF8, 0x6F, 0xB7, 0x7D, 0x56, 0xD0, 0x48, 0x87, 0xAF, 0xE8, 0xDE,
            0xAE, 0x57, 0x86, 0x87, 0x66, 0x2C, 0xC2, 0xD2, 0xBA, 0xFB, 0x47, 0x99, 0x64, 0xD6, 0x8A, 0x9D,
            0xFD, 0x59, 0x5C, 0x5D, 0x9A, 0xC7, 0xB8, 0xB4, 0xB2, 0x5D, 0x16, 0x39, 0x02, 0x6B, 0x58, 0x1E,
            0x7C, 0x35, 0xC9, 0x2A, 0xBB, 0xF0, 0xCE, 0x1D, 0x03, 0x15, 0x16, 0xE4, 0x76, 0x8E, 0x1F, 0xE9,
            0xC3, 0x87, 0x5D, 0xC0, 0x3A, 0x4F, 0x71, 0x24, 0xF6, 0xA3, 0xBA, 0xA6, 0x11, 0xDC, 0x2E, 0x84,
            0x52, 0xFF, 0x1A, 0x5F, 0x22, 0x22, 0x79, 0x0A, 0x71, 0xD1, 0x75, 0xD5, 0x3D, 0xA1, 0x5C, 0x53,
            0x76, 0x6D, 0x5F, 0x32, 0xCC, 0xB6, 0x01, 0x1F, 0xD2, 0x54, 0x9F, 0xB3, 0xB6, 0x9D, 0x1F, 0x1D,
            0x28, 0xBD, 0x10, 0xE1, 0x4C, 0x56, 0x4F, 0x12, 0xFB, 0x1A, 0xA5, 0x5C, 0xAA, 0x04, 0x84, 0x3C,
            0xB1, 0x56, 0x5C, 0xD7, 0xB4, 0xDF, 0x9C, 0xEC, 0xD1, 0x22, 0x58, 0x34, 0x88, 0x03, 0x44, 0x61,
            0x21, 0x6A, 0xD2, 0xB5, 0xDE, 0xBD, 0x73, 0x3C, 0xBB, 0xC6, 0x60, 0x33, 0x94, 0x71, 0x86, 0x37,
            0xC2, 0xF5, 0x50, 0x33, 0xE0, 0x5C, 0xBD, 0x4D, 0xE5, 0xA1, 0xF0, 0x09, 0xF0, 0xEC, 0x34, 0x1A,
            0xAB, 0xA2, 0x2A, 0x08, 0xF4, 0xA8, 0x66, 0xAD, 0x07, 0xB4, 0x59, 0x00, 0xA4, 0x2A, 0x4A, 0x02,
            0x15, 0x47, 0x96, 0x9C, 0xFE, 0x21, 0xA1, 0xB2, 0xD8, 0xE1, 0x93, 0x39, 0xD1, 0x7C, 0x65, 0x7A,
            0xB1, 0x46, 0x20, 0x25, 0xCB, 0xD8, 0xEC, 0xF1, 0x2D, 0x3C, 0x02, 0x43, 0x0D, 0xC8, 0xF0, 0xA0,
            0xEF, 0xDA, 0x81, 0x50, 0xAD, 0x56, 0x7B, 0x5C, 0x00, 0x13, 0x2A, 0xE5, 0x72, 0xBC, 0x6A, 0xDE,
            0x31, 0xB9, 0x06, 0xE5, 0x44, 0x87, 0xE9, 0x14, 0xD6, 0xD2, 0xA1, 0x7A, 0x80, 0x32, 0xC7, 0x19
        };
    }
}