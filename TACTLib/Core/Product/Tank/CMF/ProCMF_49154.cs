﻿using static TACTLib.Core.Product.Tank.CMFCryptHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF {
    [CMFMetadata(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
    public class ProCMF_49154 : ICMFEncryptionProc {
        public byte[] Key(CMFHeader header, int length) {
            byte[] buffer = new byte[length];

            uint kidx = Keytable[length + 256];
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += 3;
            }

            return buffer;
        }

        public byte[] IV(CMFHeader header, byte[] digest, int length) {
            byte[] buffer = new byte[length];

            uint kidx = (uint)length * header.BuildVersion;
            uint increment = (digest[6] & 1) == 0 ? 37 : (kidx % 61);
            for (int i = 0; i != length; ++i)
            {
                buffer[i] = Keytable[SignedMod(kidx, 512)];
                kidx += increment;
                buffer[i] ^= digest[SignedMod(kidx - i, SHA1_DIGESTSIZE)];
            }

            return buffer;
        }

        private static readonly byte[] Keytable = {
            0x35, 0xD4, 0x55, 0x9A, 0xC9, 0x98, 0xFF, 0xBF, 0x9C, 0x31, 0x1B, 0x75, 0x79, 0xFE, 0x4A, 0xA9, 
            0xDE, 0xEC, 0x4D, 0x2B, 0xD7, 0x54, 0x62, 0xE8, 0x6D, 0x20, 0xC9, 0xDD, 0x6F, 0x1E, 0x54, 0x99, 
            0x39, 0xA2, 0x1C, 0x53, 0x63, 0xDF, 0x1F, 0x67, 0x0D, 0xE2, 0x84, 0x2B, 0x70, 0xB9, 0x0C, 0x21, 
            0xBD, 0xBA, 0x0D, 0x6A, 0xF5, 0xB6, 0x75, 0x6C, 0xE4, 0x43, 0xCF, 0xB8, 0x6F, 0x59, 0x9D, 0xCF, 
            0xDC, 0x92, 0x0C, 0xB5, 0x32, 0xDD, 0x1B, 0x15, 0x7F, 0xA7, 0xA9, 0x9C, 0xA0, 0xDA, 0xF6, 0x5E, 
            0x83, 0x70, 0x6A, 0x3F, 0x96, 0x5E, 0x00, 0x75, 0x33, 0x3C, 0x23, 0xA4, 0x7B, 0x1E, 0x6E, 0xA8, 
            0x99, 0x43, 0x8C, 0x08, 0x3F, 0x75, 0x5B, 0xDF, 0x49, 0xEC, 0x4A, 0x99, 0x59, 0x5D, 0x69, 0x59, 
            0x2E, 0x47, 0x35, 0x5B, 0x9C, 0x09, 0xBD, 0x6B, 0x21, 0x55, 0x0D, 0x5B, 0xE7, 0xD2, 0x92, 0x36, 
            0x83, 0x97, 0x07, 0xAC, 0x2D, 0xD0, 0x44, 0xB5, 0x83, 0x73, 0x5D, 0xFB, 0x9A, 0x99, 0x77, 0x17, 
            0xDA, 0x7E, 0x0F, 0xA2, 0x9F, 0x10, 0x27, 0x64, 0x07, 0x1C, 0x88, 0xBB, 0xFB, 0x68, 0xE7, 0xD9, 
            0x37, 0x06, 0xD6, 0x6F, 0x0B, 0xD4, 0x42, 0x08, 0xE8, 0xB4, 0x10, 0x28, 0x03, 0x31, 0xB3, 0x59, 
            0xE3, 0x6F, 0x7C, 0xE2, 0xEF, 0x3E, 0x39, 0xAA, 0x3F, 0xC2, 0x51, 0x05, 0xC0, 0x6F, 0xCA, 0x75, 
            0x7E, 0x30, 0x64, 0xDA, 0xAB, 0x4D, 0x74, 0x4E, 0xF4, 0xF3, 0x6A, 0x8F, 0xE4, 0xEB, 0x49, 0xE0, 
            0x8C, 0x1A, 0xEF, 0x3B, 0x49, 0x40, 0xA4, 0xC4, 0xB5, 0x20, 0xDF, 0x1E, 0xCA, 0x4A, 0x4B, 0x5C, 
            0xD3, 0x1A, 0x4B, 0x31, 0x18, 0x70, 0x89, 0xC3, 0x38, 0x61, 0x53, 0x72, 0x1E, 0xA0, 0x67, 0x0D, 
            0x9F, 0x8B, 0x86, 0x76, 0x09, 0x89, 0x96, 0x0D, 0x7E, 0x52, 0xBB, 0x90, 0x11, 0xD3, 0x57, 0x43, 
            0x28, 0xE4, 0x3C, 0x67, 0xE5, 0xEC, 0x01, 0x5C, 0x93, 0x7B, 0xED, 0xD0, 0x75, 0x50, 0x2E, 0x94, 
            0x17, 0x1E, 0x31, 0x5C, 0x7D, 0x2F, 0xBC, 0x8C, 0xA2, 0x01, 0x74, 0x86, 0xF1, 0x06, 0xDA, 0x8E, 
            0xBE, 0x21, 0xA0, 0xB2, 0x55, 0x6F, 0xA5, 0xCF, 0x82, 0x2D, 0xEB, 0xE6, 0x3F, 0x21, 0x87, 0xF0, 
            0x78, 0xB8, 0x63, 0xEF, 0x01, 0x4E, 0x7E, 0x94, 0x4D, 0x53, 0x67, 0x7A, 0x5D, 0x53, 0x8D, 0x8F, 
            0x98, 0x6B, 0x50, 0x10, 0x62, 0x00, 0xEE, 0xC5, 0x4D, 0x36, 0xF0, 0x90, 0x50, 0xD2, 0xC0, 0xA4, 
            0x8E, 0xB1, 0xB3, 0xAF, 0x61, 0xCD, 0xA3, 0xAC, 0x35, 0x96, 0x82, 0x54, 0x48, 0x51, 0x8F, 0xB0, 
            0x95, 0x95, 0x2E, 0xB3, 0x44, 0xCE, 0x16, 0xF7, 0x8D, 0x65, 0x27, 0x17, 0x7E, 0xF1, 0x43, 0xB6, 
            0xD4, 0xD1, 0x83, 0x5D, 0xF3, 0xC0, 0xED, 0xD7, 0x76, 0xB3, 0x8C, 0xA4, 0xED, 0x75, 0xD2, 0xC5, 
            0x0B, 0x30, 0xFE, 0x52, 0x10, 0x36, 0x27, 0x2E, 0x82, 0xA5, 0xE9, 0xD0, 0xB1, 0x4D, 0x45, 0xC3, 
            0xF9, 0x13, 0x00, 0x6E, 0x5F, 0xFA, 0x6C, 0xC9, 0x44, 0x11, 0x84, 0xC2, 0x6C, 0xEE, 0x67, 0x55, 
            0x57, 0x62, 0x52, 0x05, 0xE0, 0x63, 0x4F, 0x58, 0xB0, 0xB8, 0x0E, 0xEE, 0x9B, 0x8B, 0x6A, 0x0C, 
            0x79, 0xB7, 0xB3, 0xC7, 0x55, 0x99, 0x4B, 0xE7, 0x17, 0x80, 0xAB, 0x51, 0x7D, 0x11, 0x2C, 0xD1, 
            0x0F, 0xF8, 0x54, 0x34, 0x0A, 0x33, 0xE4, 0x39, 0x8E, 0x8B, 0x9F, 0xA6, 0xB9, 0x6D, 0xDC, 0x6E, 
            0xE5, 0x70, 0xC7, 0x84, 0xE9, 0x4E, 0x24, 0xAE, 0x38, 0x7B, 0x63, 0xBD, 0xCA, 0x6B, 0x7C, 0x38, 
            0x38, 0xDE, 0xD8, 0x1C, 0x91, 0x0F, 0x79, 0xCF, 0x71, 0xCB, 0x01, 0x93, 0x8A, 0x07, 0x2D, 0x2F, 
            0x43, 0xC9, 0x92, 0x83, 0xAF, 0xB1, 0x00, 0x31, 0xDD, 0x8B, 0xAE, 0xC3, 0xD4, 0x35, 0x86, 0xED
        };
    }
}