﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_56957 : ICMFEncryptionProc
    {
        public byte[] Key(CMFHeader header, int length)
        {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[SignedMod(length * Keytable[0], 512)];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                switch (SignedMod(kidx, 3))
                {
                case 0:
                    kidx += 103;
                    break;
                case 1:
                    kidx = (uint)SignedMod(4 * kidx, header.m_buildVersion);
                    break;
                case 2:
                    --kidx;
                    break;
                }
            }

            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length)
        {
            byte[] buffer = new byte[length];

            int kidx = 2 * digest[5];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx -= 43;
                buffer[i] ^= digest[SignedMod(kidx + header.m_dataCount, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0xD2, 0x6B, 0xBE, 0xC4, 0x3E, 0x79, 0x3B, 0x91, 0x44, 0x52, 0x40, 0x07, 0xE9, 0xC7, 0x7B, 0xA0,
            0x4F, 0x70, 0xFC, 0x24, 0xBB, 0xA6, 0x54, 0x65, 0x87, 0xCB, 0x6E, 0x5F, 0xCD, 0x09, 0xEC, 0x2D,
            0xE2, 0xB2, 0x74, 0xCB, 0xDA, 0x50, 0x64, 0xD2, 0xBD, 0x4A, 0x20, 0x36, 0xE5, 0x74, 0x5E, 0x85,
            0x25, 0x01, 0xE4, 0x78, 0x92, 0x44, 0xEF, 0xC2, 0xE5, 0x2D, 0x7E, 0xF8, 0x91, 0xE0, 0x59, 0xA7,
            0xAB, 0xEF, 0x0A, 0x6D, 0x8A, 0x11, 0xC9, 0xB3, 0x17, 0xD9, 0x89, 0xFA, 0x04, 0x6E, 0xDE, 0xC3,
            0xEF, 0x78, 0xB9, 0x0A, 0xD7, 0x53, 0xCF, 0xC0, 0x93, 0xA2, 0x00, 0x4A, 0x3E, 0xC7, 0x03, 0x89,
            0x81, 0xEF, 0x1E, 0xE5, 0xF1, 0x43, 0x29, 0x07, 0x93, 0x14, 0x2D, 0x2D, 0xA0, 0xFD, 0xAE, 0x6C,
            0x30, 0x3F, 0xCC, 0x6A, 0x65, 0x52, 0x7F, 0x5F, 0xA8, 0x31, 0x8A, 0x92, 0xDC, 0xDB, 0x20, 0x80,
            0x5B, 0x82, 0xA8, 0x65, 0xC1, 0x59, 0xF8, 0xD6, 0x2F, 0x97, 0x85, 0xBD, 0xEE, 0xF1, 0x2A, 0xC6,
            0x8E, 0x28, 0x9C, 0xA6, 0xE1, 0x77, 0xF4, 0x72, 0xAA, 0xDE, 0x93, 0x38, 0xF3, 0xB2, 0xB1, 0xAE,
            0x92, 0x95, 0xB9, 0xA0, 0xAB, 0x75, 0x5C, 0xA5, 0x38, 0x61, 0xB6, 0x76, 0xA7, 0x2D, 0x1C, 0x0B,
            0xA7, 0x74, 0xF5, 0x24, 0xA6, 0xF3, 0x5D, 0xA6, 0x36, 0x0F, 0x2E, 0x57, 0xE2, 0x72, 0x75, 0x9A,
            0x42, 0x3A, 0x90, 0xB8, 0x2C, 0xAA, 0x57, 0xE5, 0xCE, 0x4C, 0xCD, 0x8D, 0xCF, 0xB6, 0x66, 0x0F,
            0x88, 0xE0, 0xDD, 0x92, 0x52, 0xB2, 0xB5, 0xC8, 0x1B, 0xC3, 0x41, 0xEF, 0x89, 0x78, 0xEA, 0x09,
            0xE5, 0x59, 0x66, 0x9D, 0x52, 0x2D, 0x06, 0xDF, 0xCE, 0x3F, 0xB0, 0x57, 0x19, 0x2C, 0xF8, 0x8A,
            0x27, 0x32, 0x67, 0x3F, 0x76, 0xF7, 0x52, 0xC1, 0xAF, 0x82, 0x1D, 0xF7, 0x4F, 0x78, 0x53, 0x9D,
            0x36, 0x40, 0x13, 0x70, 0xED, 0x67, 0x70, 0x25, 0xBE, 0xF7, 0x05, 0xDB, 0xA5, 0x27, 0xB8, 0xA8,
            0xD7, 0xF6, 0x9F, 0x11, 0x20, 0xAD, 0x98, 0x82, 0xD8, 0xDB, 0x4A, 0x8F, 0xE7, 0xA4, 0x45, 0xEE,
            0x39, 0x9C, 0x9E, 0xB7, 0xF5, 0xE6, 0xD3, 0x9D, 0x78, 0x80, 0xEB, 0xEA, 0xEF, 0x55, 0xCC, 0x9F,
            0xFD, 0x97, 0xF3, 0xF1, 0x1B, 0x92, 0x5E, 0x51, 0x12, 0x4E, 0xAD, 0x50, 0xCC, 0x2A, 0x4F, 0xE8,
            0x48, 0x96, 0x71, 0x70, 0xE5, 0x80, 0x6E, 0x26, 0x3A, 0x9B, 0x8F, 0x6A, 0xAF, 0xE4, 0x33, 0xF4,
            0x53, 0xD5, 0xC7, 0x1D, 0x30, 0xC2, 0x73, 0x38, 0x99, 0x05, 0x44, 0x38, 0x3D, 0x2B, 0xA1, 0x06,
            0x55, 0xCF, 0x0D, 0x12, 0xCC, 0xFF, 0xEA, 0x4E, 0xEB, 0x23, 0xA4, 0x8E, 0x5E, 0xAA, 0x6E, 0x5C,
            0x73, 0x79, 0x9E, 0x81, 0x19, 0x7B, 0xE0, 0xF9, 0x17, 0xA7, 0x2E, 0x25, 0xEA, 0x97, 0x93, 0x5D,
            0x5C, 0x41, 0x2C, 0xC3, 0xBA, 0xF5, 0x49, 0x64, 0x07, 0x9F, 0x15, 0x58, 0x35, 0x8D, 0xC1, 0x28,
            0x97, 0x33, 0x16, 0xF6, 0x32, 0x92, 0x86, 0xD6, 0x22, 0x77, 0x37, 0xDE, 0x99, 0x22, 0x50, 0x82,
            0x5E, 0x36, 0x6B, 0x1D, 0xDB, 0xE4, 0xDC, 0xDF, 0xD6, 0xB9, 0x4A, 0xD8, 0x2B, 0xBC, 0x36, 0x14,
            0xFF, 0xF6, 0xD0, 0x33, 0xA8, 0x64, 0xBA, 0xA7, 0x58, 0x1A, 0x45, 0x83, 0xB0, 0x4A, 0x4E, 0x0F,
            0xDF, 0x26, 0x1B, 0xC0, 0x95, 0x9B, 0xC2, 0x52, 0xB3, 0x21, 0x51, 0x38, 0xFD, 0x37, 0xF2, 0x4D,
            0x25, 0x4A, 0x8E, 0xD7, 0x2C, 0x98, 0xA4, 0x23, 0xBB, 0xAB, 0x76, 0x5A, 0x2E, 0xC8, 0xF4, 0xF1,
            0xF1, 0x15, 0xC5, 0x21, 0x39, 0x50, 0xFE, 0x7A, 0x5E, 0xBA, 0x3F, 0x90, 0x27, 0xAF, 0x55, 0xA3,
            0xF2, 0xF7, 0x4D, 0x2B, 0x92, 0xDC, 0xF6, 0x76, 0x77, 0xAD, 0xBD, 0x20, 0x23, 0x62, 0x84, 0xFA
        };
    }
}