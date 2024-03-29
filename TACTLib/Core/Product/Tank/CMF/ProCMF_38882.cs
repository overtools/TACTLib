﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_38882 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[length * Keytable[0] % 512];
            uint increment = header.m_buildVersion * (uint)header.m_dataCount % 7;
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

            uint kidx = Keytable[(digest[7] * Keytable[0]) & 511];
            uint increment = kidx % 29;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[kidx % 512];
                kidx += increment;
                buffer[i] ^= (byte)((digest[(kidx + header.m_entryCount) % SHA1_DIGESTSIZE] + 1) & 0xFF);
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x62, 0xBE, 0xC0, 0x8D, 0xF6, 0xE5, 0xCC, 0x26, 0x66, 0x57, 0xD5, 0x08, 0xC6, 0xC2, 0xFD, 0x3E,
            0xE7, 0x12, 0x9F, 0x1B, 0xC7, 0x78, 0x49, 0x8D, 0xF5, 0x7F, 0xBA, 0x4B, 0x77, 0xA3, 0xC7, 0xAB,
            0xA3, 0x6D, 0x2A, 0x85, 0x56, 0xEA, 0x07, 0x9A, 0xB2, 0x3B, 0x53, 0x1C, 0x65, 0x83, 0x87, 0x61,
            0x2D, 0xC3, 0x6E, 0x84, 0x50, 0x56, 0x6F, 0x4C, 0xCD, 0x2E, 0x68, 0xE7, 0x0B, 0xB4, 0x80, 0xE5,
            0xEA, 0x6C, 0xE8, 0xB9, 0x24, 0x82, 0xC6, 0xF6, 0x8A, 0xA5, 0xFF, 0x81, 0x16, 0x6E, 0x87, 0x6F,
            0x68, 0x29, 0x3C, 0x9F, 0x18, 0xB5, 0xAB, 0x9E, 0x1A, 0x5C, 0x9E, 0xCC, 0x89, 0xD1, 0x09, 0xE9,
            0x23, 0xBD, 0xFD, 0x02, 0xAB, 0xAA, 0x3A, 0xEA, 0xEF, 0x43, 0x6F, 0x2A, 0x3A, 0x76, 0x85, 0x1F,
            0x6B, 0x04, 0xA7, 0x9F, 0xB0, 0x61, 0x18, 0xFF, 0xB8, 0x4C, 0x45, 0xFA, 0xBC, 0xBA, 0x5B, 0x90,
            0x13, 0xEA, 0x2F, 0x39, 0x02, 0xB9, 0x62, 0x0C, 0x07, 0xAE, 0x64, 0x58, 0x0E, 0x21, 0x09, 0x2A,
            0xAA, 0x2A, 0x3F, 0x02, 0x49, 0xED, 0x5D, 0x53, 0xC4, 0x37, 0x44, 0x5F, 0x1F, 0xB6, 0x6D, 0x8B,
            0x2A, 0x4D, 0xF4, 0x98, 0x72, 0x81, 0x3A, 0x0E, 0xCA, 0x06, 0x8E, 0x11, 0x14, 0xEA, 0x48, 0x70,
            0xC9, 0x08, 0x44, 0xFC, 0xCC, 0xC8, 0x54, 0xED, 0x5D, 0x99, 0xC5, 0xA1, 0x01, 0x6F, 0x7F, 0x7E,
            0x55, 0x59, 0x6B, 0x20, 0x90, 0xA8, 0x33, 0xBD, 0x20, 0x17, 0xD5, 0x15, 0x0B, 0xB8, 0xDA, 0x49,
            0x1B, 0xCE, 0xD9, 0xD8, 0xC6, 0x96, 0x86, 0x06, 0xDA, 0x8E, 0xAF, 0x40, 0x21, 0x38, 0x4B, 0x4A,
            0x3B, 0xD6, 0x1B, 0xC0, 0x1E, 0x00, 0x92, 0xFB, 0xC1, 0x68, 0x4B, 0x7E, 0xF0, 0x7E, 0x35, 0xEA,
            0xDD, 0xEA, 0xEA, 0x52, 0x75, 0x3A, 0x42, 0xF2, 0xE2, 0xF5, 0x8F, 0xEE, 0x8F, 0xF4, 0x42, 0xC1,
            0x19, 0x80, 0xF8, 0xE9, 0xE7, 0xC3, 0x99, 0x9F, 0x03, 0x9B, 0xA7, 0x17, 0xDD, 0x8D, 0xAD, 0x9C,
            0x9B, 0x7F, 0xE1, 0x10, 0x5C, 0x4D, 0x08, 0x9E, 0x8F, 0xD3, 0x8C, 0x37, 0xE8, 0x1A, 0x9E, 0xAF,
            0xE7, 0x0C, 0x9B, 0xB1, 0x85, 0x27, 0x66, 0x97, 0xE4, 0xDB, 0x20, 0x14, 0xD2, 0xBF, 0x90, 0xD1,
            0x45, 0x72, 0xA2, 0xE9, 0x59, 0xEA, 0xEC, 0x31, 0x05, 0xB6, 0xE9, 0x80, 0x1F, 0xDE, 0xA8, 0x34,
            0xEF, 0x3D, 0xD9, 0xD7, 0xF7, 0x3B, 0xC2, 0x3C, 0xAD, 0x3D, 0x48, 0xB8, 0x9A, 0xD4, 0x01, 0x0B,
            0xF7, 0xB8, 0x54, 0x6F, 0xC8, 0x80, 0xC7, 0x72, 0x5C, 0xF2, 0xD9, 0xB9, 0x04, 0xC4, 0x9D, 0x37,
            0x76, 0x63, 0xB8, 0x47, 0xCD, 0x0E, 0x94, 0x87, 0x36, 0x7C, 0x9D, 0xBD, 0x9F, 0x50, 0x62, 0x17,
            0xBA, 0x08, 0xAD, 0x6B, 0xD2, 0x23, 0x45, 0x85, 0xDE, 0x0A, 0x28, 0xEB, 0x92, 0xEC, 0xC3, 0x5A,
            0xEB, 0x65, 0xFE, 0x7D, 0x23, 0x41, 0xC5, 0x34, 0x28, 0x49, 0x3C, 0x1F, 0xA2, 0x20, 0x9F, 0xEA,
            0x93, 0x22, 0x52, 0xEE, 0x9A, 0x92, 0x32, 0x98, 0xA9, 0xDF, 0x59, 0x1A, 0xD1, 0xBE, 0x68, 0x3F,
            0xE9, 0xE1, 0x45, 0xE5, 0x25, 0x39, 0x14, 0x5A, 0xBC, 0x38, 0x78, 0xDA, 0x39, 0x94, 0x0D, 0x69,
            0x13, 0x7A, 0x89, 0xC6, 0x2D, 0x90, 0x28, 0x23, 0x37, 0x67, 0xB7, 0x36, 0xC2, 0xCF, 0x40, 0x2F,
            0xDE, 0x10, 0xC8, 0xDA, 0x3E, 0x19, 0xA0, 0x9E, 0x22, 0xF8, 0x55, 0x4A, 0x4A, 0x20, 0xD8, 0xDD,
            0xE0, 0x21, 0x4A, 0x61, 0x18, 0xFC, 0xE4, 0x18, 0x91, 0x7C, 0x2B, 0x97, 0x5E, 0xC8, 0xEA, 0xF8,
            0x38, 0x87, 0x63, 0x3F, 0xDF, 0x5D, 0xD1, 0x91, 0x95, 0xE4, 0x63, 0x3C, 0xBE, 0x1B, 0xA2, 0x48,
            0x6A, 0xD9, 0x67, 0x3C, 0x2C, 0xD6, 0xF0, 0x3C, 0xEF, 0x84, 0xD4, 0x36, 0x7F, 0x38, 0x26, 0x8B
        };
    }
}