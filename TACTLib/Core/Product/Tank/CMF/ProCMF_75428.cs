using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
	[ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
	public class ProCMF_75428 : ICMFEncryptionProc
	{
		public byte[] Key(CMFHeader header, int length)
		{
			byte[] buffer = new byte[length];
			uint kidx = (uint)(length * header.m_buildVersion);
			for (uint i = 0; i != length; ++i)
			{
				buffer[i] = Keytable[SignedMod(kidx, 512)];
				kidx = header.m_buildVersion - kidx;
			}
			return buffer;
		}

		public byte[] IV(CMFHeader header, byte[] digest, int length)
		{
			byte[] buffer = new byte[length];
			uint kidx = Keytable[header.m_buildVersion & 511];
			for (int i = 0; i != length; ++i)
			{
				buffer[i] = Keytable[SignedMod(kidx, 512)];
				kidx += ((uint)header.m_dataCount + digest[SignedMod(header.m_dataCount, SHA1_DIGESTSIZE)]) % 17;
				buffer[i] = digest[SignedMod(kidx, SHA1_DIGESTSIZE)];
			}
			return buffer;
		}

		private static readonly byte[] Keytable =
		{
			0x76, 0x36, 0xA8, 0xF1, 0x9D, 0xC4, 0x6E, 0x29, 0x1A, 0x59, 0x2D, 0x97, 0x3A, 0x55, 0x4C, 0xE1, 
			0x65, 0x45, 0x4D, 0x84, 0xC2, 0xC8, 0x12, 0xC7, 0x09, 0x09, 0x9D, 0x7B, 0x84, 0xD6, 0x94, 0xD0, 
			0xAA, 0x11, 0xAF, 0x79, 0xCE, 0x7E, 0x7A, 0x6B, 0xA4, 0x70, 0x0A, 0x45, 0xCD, 0xEC, 0x98, 0x79, 
			0x0C, 0x95, 0xEC, 0x2D, 0x5F, 0x76, 0xDA, 0x65, 0x50, 0x7E, 0x4F, 0x86, 0x2A, 0x29, 0xAF, 0xF4, 
			0x78, 0xF5, 0xD2, 0x50, 0xCF, 0xF4, 0x7B, 0x66, 0x15, 0xD1, 0x45, 0x48, 0xB4, 0x39, 0x62, 0x7C, 
			0x5E, 0xD6, 0x7C, 0x69, 0x1D, 0x41, 0x71, 0x46, 0x41, 0x4D, 0xC9, 0xE0, 0xAC, 0x99, 0xF7, 0xE9, 
			0x1E, 0xB2, 0x66, 0xB7, 0x97, 0xC6, 0x7F, 0xCD, 0xBF, 0xAC, 0xA8, 0x76, 0xBB, 0xD9, 0x19, 0x8F, 
			0xE3, 0x6F, 0x85, 0x4F, 0xB9, 0x0C, 0x43, 0x1B, 0x95, 0x71, 0x48, 0xAA, 0xBA, 0x8E, 0x7B, 0xF3, 
			0xA0, 0xB3, 0x4D, 0x06, 0x1C, 0x64, 0x48, 0xDA, 0x91, 0xF3, 0x34, 0x94, 0x20, 0x61, 0xFC, 0x61, 
			0x7A, 0x9B, 0xAE, 0x90, 0x64, 0xE6, 0x8B, 0x62, 0x39, 0xDE, 0x86, 0x08, 0x94, 0xE0, 0xAD, 0x40, 
			0xFE, 0x8F, 0x4D, 0x49, 0xAE, 0xCF, 0xF8, 0xA2, 0xD2, 0x57, 0x14, 0xC0, 0xBD, 0x3A, 0x1A, 0xBD, 
			0xAD, 0x41, 0x45, 0x08, 0xF4, 0xC8, 0x11, 0x0B, 0xD2, 0x06, 0x75, 0x9A, 0xC8, 0xD7, 0x0D, 0xBF, 
			0x4A, 0x0D, 0x38, 0xE6, 0x0B, 0xBF, 0x49, 0xA0, 0x83, 0x4C, 0xB4, 0xAD, 0xAA, 0x02, 0x34, 0x2D, 
			0x6A, 0x84, 0x0B, 0x13, 0x7C, 0xE2, 0x22, 0x0C, 0x77, 0x19, 0x02, 0x1A, 0x1E, 0x2E, 0x1B, 0x27, 
			0x2D, 0x04, 0x38, 0xCE, 0x9E, 0xED, 0x5C, 0x58, 0xCB, 0x71, 0x3E, 0x24, 0x13, 0x64, 0x28, 0xEB, 
			0x14, 0x68, 0x2E, 0x12, 0x64, 0x42, 0xE5, 0x74, 0xEF, 0xD2, 0x34, 0xE8, 0xF2, 0x12, 0x86, 0x2B, 
			0x6B, 0xEC, 0xA1, 0x01, 0x8A, 0x7B, 0xA3, 0x8C, 0x4E, 0xAB, 0x1D, 0x7D, 0x51, 0x6B, 0x64, 0x80, 
			0x1C, 0xCE, 0x08, 0x56, 0x41, 0xEB, 0x65, 0x86, 0x44, 0x42, 0x9B, 0x0A, 0x29, 0x0D, 0x78, 0x45, 
			0x41, 0x66, 0xD4, 0x33, 0xCE, 0xFF, 0x03, 0xA3, 0xD5, 0x89, 0xBF, 0x2D, 0xC4, 0x44, 0x88, 0x96, 
			0xC0, 0x43, 0x3E, 0xC5, 0x9F, 0x78, 0xBF, 0x16, 0x13, 0x1B, 0x1A, 0x5D, 0xC0, 0x83, 0x42, 0x99, 
			0x33, 0x6D, 0x2C, 0xCC, 0x36, 0x50, 0x7A, 0x57, 0x51, 0xF5, 0xC1, 0x6D, 0xC9, 0xD5, 0x0C, 0x21, 
			0x56, 0x1E, 0x91, 0xF6, 0x93, 0x1E, 0xFA, 0xD2, 0x3B, 0x9F, 0x9C, 0x23, 0x66, 0xB9, 0x7E, 0xF1, 
			0x02, 0x31, 0x80, 0x8A, 0xB2, 0xD7, 0x93, 0xA0, 0xDB, 0xF4, 0xAA, 0x1E, 0xBE, 0x9A, 0x50, 0x4B, 
			0xDD, 0x18, 0x9B, 0x8D, 0x37, 0x83, 0xBD, 0x80, 0x30, 0xDE, 0x84, 0x41, 0xCF, 0x2C, 0x96, 0x05, 
			0x0E, 0xB5, 0x8E, 0x80, 0xC8, 0x42, 0xEA, 0x01, 0xB0, 0xBB, 0xC6, 0x04, 0xFB, 0x59, 0xC4, 0xD0, 
			0xD9, 0xC6, 0x3F, 0x41, 0x45, 0xFB, 0x31, 0x00, 0x3B, 0x7A, 0x3A, 0xE1, 0xC8, 0x83, 0x01, 0xA3, 
			0xAB, 0x35, 0xA4, 0x3C, 0x10, 0x35, 0xFF, 0x2E, 0xC1, 0x6F, 0xA4, 0xC8, 0x7B, 0xD6, 0xA7, 0xE0, 
			0x22, 0xD2, 0x20, 0x78, 0xCC, 0x23, 0xF2, 0x23, 0x8C, 0x1E, 0x4B, 0x74, 0x45, 0xCE, 0x8A, 0x6A, 
			0x4A, 0xF1, 0x5E, 0x84, 0xF2, 0x4C, 0xCF, 0x15, 0x67, 0x59, 0x9C, 0xBA, 0x2A, 0x8A, 0x90, 0x4E, 
			0x2B, 0x22, 0xB3, 0x94, 0x95, 0xE9, 0x5E, 0xDA, 0xFB, 0xE4, 0xF9, 0x4A, 0xCD, 0x04, 0xFB, 0xB3, 
			0x9A, 0x31, 0x28, 0x57, 0x42, 0xFB, 0xEE, 0x67, 0x55, 0xA7, 0xBB, 0x45, 0x6E, 0xF0, 0x79, 0x8B, 
			0xE2, 0xDB, 0x9E, 0x28, 0xDB, 0x36, 0xB2, 0xD9, 0x99, 0x0E, 0xD9, 0xD6, 0x49, 0x0D, 0x23, 0x07
		};
	}
}