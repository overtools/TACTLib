﻿using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
    [ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_46510 : ICMFEncryptionProc
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

            uint kidx = 2u * digest[1];
            uint increment = header.m_buildVersion * (uint)header.m_dataCount % 7;
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += increment;
                buffer[i] ^= digest[SignedMod(kidx - 73, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable =
        {
            0x44, 0xFF, 0x01, 0xB0, 0xC1, 0xD5, 0xC9, 0x0B, 0x36, 0x2D, 0xE9, 0x6B, 0x03, 0x7F, 0x0A, 0x5A, 
            0x68, 0xB3, 0x1C, 0x67, 0xE6, 0x42, 0xB0, 0x47, 0x57, 0xA0, 0x7D, 0x7D, 0xD4, 0x06, 0x32, 0x04, 
            0xB4, 0xF7, 0xB2, 0x88, 0xE0, 0x02, 0x2E, 0xBC, 0x33, 0xFC, 0x84, 0x73, 0xCA, 0x92, 0x85, 0x96, 
            0x9C, 0x87, 0x71, 0x01, 0x98, 0x34, 0xC3, 0x32, 0x4D, 0x06, 0x8E, 0x43, 0x65, 0x58, 0xA8, 0x30, 
            0x87, 0x1E, 0xDD, 0x68, 0xE9, 0x02, 0xC3, 0xE4, 0x16, 0x2F, 0xEF, 0xB6, 0x41, 0xCB, 0x3E, 0x54, 
            0xC9, 0x1A, 0xAB, 0x7D, 0x57, 0x9E, 0xA2, 0xB8, 0x38, 0xEC, 0x08, 0x84, 0xB8, 0x65, 0x7E, 0x77, 
            0xB2, 0x4E, 0xB9, 0x84, 0xD3, 0xD3, 0x29, 0x98, 0x16, 0x80, 0x24, 0x55, 0xDC, 0x47, 0x90, 0x39, 
            0x02, 0x52, 0x25, 0x9E, 0x84, 0xEC, 0x2B, 0xB4, 0x54, 0xE2, 0xA1, 0x95, 0x9D, 0xD4, 0xE6, 0x73, 
            0x10, 0x8E, 0x57, 0x1E, 0xB2, 0xB7, 0x01, 0xC6, 0xDE, 0xA5, 0x89, 0x7D, 0x9E, 0xC0, 0x1A, 0x69, 
            0x38, 0xC9, 0xED, 0xBE, 0x24, 0x1D, 0xB3, 0x45, 0x2D, 0x1B, 0xE0, 0x4B, 0x4C, 0x8D, 0x07, 0x2C, 
            0x73, 0xEE, 0x2F, 0x5B, 0x2B, 0x74, 0x21, 0x80, 0x84, 0x69, 0xFB, 0x7D, 0x01, 0x8C, 0x95, 0xA5, 
            0x6F, 0x7D, 0x09, 0xE0, 0xF8, 0xD1, 0x5B, 0xED, 0x8B, 0x07, 0x77, 0x64, 0x27, 0xCA, 0xA6, 0xD8, 
            0x82, 0xB0, 0xD8, 0xA9, 0xAC, 0x93, 0x42, 0xCC, 0xB4, 0xB9, 0xC9, 0xD4, 0x97, 0xDC, 0x95, 0xF6, 
            0x3B, 0x8C, 0xB3, 0xA6, 0x7C, 0xA5, 0xD4, 0x91, 0x6B, 0x3D, 0xE7, 0xA2, 0xC5, 0xA5, 0xF9, 0x45, 
            0xC7, 0x23, 0xF0, 0x65, 0x27, 0xDF, 0x37, 0x87, 0xCF, 0xC8, 0xAB, 0x57, 0x64, 0xA7, 0xC4, 0x8B, 
            0x4C, 0xBA, 0x2E, 0x0F, 0xFF, 0x44, 0xB5, 0xBA, 0xF0, 0x0B, 0x43, 0x1E, 0xF7, 0x8A, 0x68, 0xDC, 
            0x5D, 0xB6, 0x83, 0x27, 0x30, 0xF2, 0xAC, 0x25, 0xCA, 0x07, 0xBB, 0x25, 0x2B, 0xC1, 0x2D, 0xE0, 
            0x00, 0x8A, 0xF1, 0x46, 0xE1, 0x6F, 0x41, 0xF2, 0x62, 0x8F, 0xA3, 0x88, 0x55, 0xF7, 0x0A, 0x93, 
            0xF7, 0x7C, 0x04, 0x1E, 0x26, 0xD6, 0x20, 0x5F, 0x7B, 0xD7, 0x43, 0x70, 0xB2, 0x35, 0xD4, 0x67, 
            0xAD, 0x37, 0x58, 0x1F, 0x5F, 0xA9, 0x86, 0xCF, 0x11, 0x23, 0xE0, 0x52, 0xA3, 0x5B, 0x21, 0x96, 
            0xF5, 0xF6, 0xE7, 0x14, 0x35, 0x3B, 0xFF, 0x81, 0x2E, 0x26, 0x92, 0x1C, 0x18, 0x69, 0x8F, 0x2B, 
            0x31, 0xA4, 0x56, 0x34, 0xE1, 0xEA, 0x1F, 0x52, 0x21, 0x2F, 0x23, 0xDF, 0x52, 0xC8, 0x39, 0x00, 
            0xB3, 0x31, 0x9E, 0x2A, 0x52, 0x1E, 0xE7, 0xE6, 0xE3, 0x79, 0xAB, 0x42, 0x16, 0x27, 0x73, 0xBC, 
            0x79, 0x4A, 0x4E, 0xB8, 0x61, 0x01, 0xCD, 0xA1, 0xAC, 0x36, 0x26, 0x1E, 0x45, 0x3D, 0x12, 0x4D, 
            0x85, 0xE8, 0x6E, 0x20, 0xAF, 0x71, 0xB3, 0x7F, 0xFB, 0xFF, 0xD4, 0x45, 0x60, 0x4A, 0x4A, 0x52, 
            0x6A, 0xE4, 0xD7, 0x32, 0xF4, 0x2C, 0xCA, 0xC3, 0x63, 0x44, 0x75, 0xE3, 0x90, 0x76, 0x30, 0xE9, 
            0xE7, 0x6D, 0x03, 0xD9, 0x16, 0xA1, 0x96, 0xC0, 0x8D, 0xE7, 0xE6, 0x59, 0xBC, 0xC2, 0xB1, 0x02, 
            0xE5, 0xBC, 0x4E, 0xF7, 0x14, 0xEB, 0x34, 0x58, 0x95, 0xFD, 0xA0, 0xD1, 0x3B, 0x56, 0x0A, 0x61, 
            0x84, 0x07, 0x6A, 0x26, 0x24, 0x0A, 0xF7, 0x72, 0xA9, 0x73, 0xB7, 0x5F, 0x2E, 0xAD, 0x8E, 0x0F, 
            0x6E, 0x27, 0x66, 0x2C, 0xDC, 0x86, 0x04, 0xDD, 0xC8, 0x28, 0xED, 0xA3, 0x42, 0xD7, 0x8D, 0xD3, 
            0x68, 0x35, 0x67, 0x28, 0x50, 0xCF, 0x31, 0xB9, 0x5F, 0x7D, 0xA3, 0xB7, 0xCD, 0xA9, 0x38, 0x9D, 
            0x5E, 0xE7, 0x85, 0xBD, 0xBA, 0xD9, 0x52, 0xE3, 0x1B, 0x21, 0xBF, 0x96, 0xE5, 0xC5, 0x8B, 0x2F
        };
    }
}