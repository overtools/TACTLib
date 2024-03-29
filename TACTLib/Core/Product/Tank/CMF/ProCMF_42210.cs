﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_42210 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[header.m_buildVersion & 511];
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
                kidx += header.m_buildVersion - kidx;
                buffer[i] ^= digest[SignedMod(i + kidx, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x99, 0x9D, 0x00, 0x78, 0xBA, 0x76, 0xED, 0xF1, 0xBB, 0xCA, 0x05, 0x72, 0xA5, 0x29, 0x7D, 0x87,
            0x11, 0x80, 0xD4, 0x52, 0x4A, 0x4D, 0x7F, 0x83, 0x9C, 0x05, 0xBB, 0x84, 0xCF, 0xC4, 0xC9, 0x98,
            0x25, 0x12, 0x08, 0x5B, 0x72, 0x47, 0xC9, 0x37, 0xBC, 0x5D, 0x76, 0x94, 0xB4, 0x10, 0x89, 0x69,
            0xB9, 0xFB, 0x81, 0xCE, 0xC3, 0xFD, 0x7A, 0xAD, 0xDB, 0x19, 0x3C, 0x75, 0x8B, 0x76, 0xEA, 0x34,
            0x8F, 0x6A, 0x31, 0xB0, 0x2C, 0x10, 0xAD, 0x13, 0x7F, 0xFA, 0xCB, 0x79, 0x4C, 0xC9, 0x64, 0x3E,
            0x40, 0xD5, 0x26, 0xD3, 0x88, 0xCE, 0x7A, 0x35, 0xB5, 0x78, 0xE9, 0x38, 0xC2, 0x66, 0xF7, 0x0D,
            0x99, 0x39, 0xB7, 0x6E, 0xCC, 0x4D, 0xA3, 0xA8, 0xB6, 0x72, 0xC2, 0x6F, 0x51, 0xBD, 0x7F, 0xC6,
            0x70, 0x5C, 0xB0, 0xE8, 0xE4, 0xA3, 0x65, 0xF2, 0x7F, 0x51, 0x3D, 0x0D, 0x3A, 0x66, 0xFD, 0x5F,
            0xC8, 0x13, 0x5A, 0x1A, 0xE8, 0x53, 0xA2, 0x4B, 0x6C, 0x27, 0x2A, 0xE7, 0xAB, 0x31, 0x52, 0x14,
            0x6A, 0x04, 0xC5, 0xDA, 0x4D, 0xFB, 0x40, 0xD4, 0xD1, 0x9D, 0xF0, 0x70, 0x30, 0x54, 0x85, 0x59,
            0xC5, 0x79, 0xE1, 0x2C, 0x51, 0x35, 0x43, 0xBE, 0x41, 0x5F, 0x42, 0x21, 0x3F, 0xFB, 0x1C, 0x30,
            0x96, 0x1A, 0xB7, 0x01, 0x3A, 0x0B, 0x46, 0x05, 0x82, 0xD9, 0xF3, 0xC0, 0x6F, 0xDD, 0x77, 0x86,
            0x38, 0xCC, 0x34, 0xA9, 0x45, 0x17, 0x01, 0x17, 0x9E, 0xA0, 0x1A, 0x9E, 0x6E, 0x1A, 0x70, 0xEE,
            0x47, 0xBF, 0x56, 0x3A, 0xAA, 0x01, 0xEB, 0x67, 0x75, 0x75, 0x39, 0x7B, 0x22, 0x30, 0x40, 0x3A,
            0xBC, 0xAA, 0x4B, 0xA9, 0x1D, 0xAB, 0xD1, 0x41, 0xEA, 0x46, 0x0C, 0x52, 0xEF, 0xD9, 0x5F, 0xFF,
            0x8A, 0x53, 0x5C, 0xAE, 0x1D, 0xCE, 0x61, 0x76, 0x10, 0xBA, 0x87, 0x07, 0xCE, 0x6C, 0x19, 0x04,
            0x3D, 0x85, 0x77, 0xF0, 0xDE, 0x60, 0x7F, 0xF7, 0x06, 0x2E, 0x91, 0xEB, 0x05, 0x59, 0x1C, 0x13,
            0xD0, 0xFB, 0xD2, 0x5A, 0xA7, 0x60, 0x39, 0x12, 0xF0, 0x51, 0xD7, 0x4C, 0x6D, 0x4E, 0x01, 0xB7,
            0x5C, 0x3F, 0xFA, 0x10, 0x69, 0x93, 0xEF, 0x05, 0xCA, 0x76, 0x61, 0x8A, 0x4B, 0x31, 0x18, 0x8F,
            0x6C, 0xF8, 0x02, 0x3A, 0x56, 0x5C, 0x72, 0xC1, 0x9C, 0x02, 0x29, 0xB6, 0xE9, 0x57, 0x3F, 0xC1,
            0xAB, 0x3F, 0x7C, 0xEC, 0xF4, 0xCD, 0x99, 0x01, 0x13, 0xCE, 0xD9, 0xE9, 0xEB, 0xDC, 0x9C, 0xE4,
            0x3B, 0x44, 0x72, 0xC2, 0x81, 0xB2, 0x10, 0xF5, 0xC3, 0x81, 0x7A, 0xF5, 0xF3, 0x3D, 0x03, 0x4E,
            0xD4, 0xAE, 0xD8, 0x41, 0xB7, 0xF7, 0x2B, 0x54, 0x51, 0x80, 0xD3, 0xD1, 0x19, 0x24, 0x6D, 0x0B,
            0xB1, 0x77, 0xBB, 0x1F, 0xBA, 0x23, 0x50, 0xE7, 0x18, 0x05, 0x6E, 0xC8, 0xBE, 0x0B, 0x09, 0x32,
            0x1C, 0xD4, 0xC0, 0x75, 0xB2, 0x45, 0x85, 0x3B, 0x3B, 0x97, 0x7B, 0x4A, 0x73, 0x3E, 0xAD, 0x90,
            0x9B, 0x27, 0x2C, 0x3A, 0xC9, 0xFA, 0x91, 0x45, 0x27, 0x20, 0x69, 0x44, 0x13, 0x06, 0x70, 0x12,
            0x5E, 0x0A, 0x72, 0xBC, 0x5C, 0x88, 0xF0, 0x9F, 0xF3, 0xCB, 0x8E, 0x54, 0xF2, 0x82, 0x5A, 0x85,
            0xC6, 0xAB, 0xF6, 0xED, 0x14, 0xAA, 0xFE, 0xA6, 0xD1, 0x01, 0xEA, 0xA9, 0x4F, 0xE4, 0x55, 0xBB,
            0xDC, 0xDB, 0xC5, 0x5E, 0x81, 0xF2, 0xC7, 0xDE, 0x3E, 0x66, 0x0F, 0x70, 0xD2, 0xF2, 0x98, 0xCB,
            0x0D, 0x1E, 0x42, 0x85, 0xF3, 0x1C, 0x52, 0x8D, 0x59, 0x83, 0xC0, 0x48, 0xFC, 0x31, 0xE3, 0xBB,
            0x44, 0x63, 0x0E, 0x66, 0xB8, 0x86, 0x9C, 0x81, 0x74, 0x05, 0xEB, 0x70, 0x59, 0x81, 0x04, 0x4F,
            0xEF, 0x12, 0x0B, 0x1B, 0xD7, 0xE9, 0x63, 0xBC, 0x3C, 0x52, 0xB0, 0x7A, 0xED, 0xB3, 0xFE, 0xD6
        };
    }
}