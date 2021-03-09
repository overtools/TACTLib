using static TACTLib.Core.Product.Tank.ManifestCryptoHandler;
using static TACTLib.Core.Product.Tank.ContentManifestFile;

namespace TACTLib.Core.Product.Tank.CMF
{
	[ManifestCrypto(AutoDetectVersion = true, Product = TACTProduct.Overwatch)]
	public class ProCMF_79605 : ICMFEncryptionProc
	{
		public byte[] Key(CMFHeader header, int length)
		{
			byte[] buffer = new byte[length];
			uint kidx, okidx;
			kidx = okidx = Keytable[header.m_buildVersion & 511];
			for (uint i = 0; i != length; ++i)
			{
				buffer[i] = Keytable[SignedMod(kidx, 512)];
				kidx += (uint)header.m_dataCount;
			}
			return buffer;
		}

		public byte[] IV(CMFHeader header, byte[] digest, int length)
		{
			byte[] buffer = new byte[length];
			uint kidx, okidx;
			kidx = okidx = Keytable[((2 * digest[13]) - length) & 511];
			for (int i = 0; i != length; ++i)
			{
				buffer[i] = Keytable[SignedMod(kidx, 512)];
				kidx += (uint)header.m_dataCount + digest[SignedMod(header.m_dataCount, SHA1_DIGESTSIZE)];
				buffer[i] ^= digest[SignedMod(header.m_buildVersion + i, SHA1_DIGESTSIZE)];
			}
			return buffer;
		}

		private static readonly byte[] Keytable =
		{
			0x1D, 0xF0, 0xF4, 0x8D, 0xDF, 0xDC, 0xE5, 0x87, 0x99, 0x3A, 0xC1, 0x0A, 0x0E, 0x93, 0x5C, 0x86, 
			0x2E, 0x7B, 0x08, 0x34, 0x7B, 0x87, 0xD0, 0xCA, 0x7D, 0x40, 0xF3, 0x17, 0x8F, 0x15, 0xE6, 0x67, 
			0xB4, 0x0F, 0xA5, 0xB5, 0x80, 0xD4, 0xCC, 0x63, 0x15, 0xBF, 0xD1, 0xFA, 0x1C, 0x69, 0xD0, 0x24, 
			0xC9, 0x21, 0x5F, 0xC9, 0x38, 0x06, 0xAE, 0x4F, 0xF6, 0x6F, 0xB9, 0xA7, 0x4A, 0x4C, 0x1F, 0xBD, 
			0xEF, 0xD5, 0x84, 0x11, 0xE1, 0x7A, 0x1C, 0x3A, 0x44, 0xBA, 0x1D, 0xAE, 0x31, 0xA8, 0x1B, 0xE5, 
			0x8B, 0x46, 0xAA, 0x8B, 0x8D, 0xCF, 0x3E, 0x43, 0x33, 0xD7, 0xE9, 0x05, 0x10, 0xEB, 0xD0, 0x30, 
			0x55, 0x1A, 0x5B, 0x7E, 0x77, 0x27, 0xBA, 0x88, 0xCE, 0xD0, 0xA8, 0x03, 0x96, 0xA1, 0x5B, 0x88, 
			0x85, 0xB4, 0x49, 0x5D, 0xC2, 0xE6, 0xA0, 0xD1, 0xBB, 0xAE, 0xC3, 0x7B, 0x9C, 0x9D, 0x73, 0x4B, 
			0x49, 0xAF, 0x3F, 0x9E, 0x77, 0xE4, 0x04, 0xC6, 0x9B, 0x7F, 0xC9, 0x0C, 0x44, 0xF2, 0xBB, 0xF7, 
			0x22, 0xC6, 0x0D, 0x65, 0x81, 0x4E, 0x14, 0x24, 0x52, 0x8A, 0xEB, 0xBF, 0x5E, 0x11, 0xED, 0x41, 
			0xB4, 0x93, 0x87, 0xE1, 0xD7, 0xD8, 0x89, 0xD5, 0xE5, 0x38, 0x79, 0x72, 0x4D, 0xB5, 0xB7, 0x3F, 
			0x73, 0xEF, 0xE6, 0x21, 0xC1, 0x95, 0xBD, 0x64, 0x8F, 0xA6, 0x7C, 0x13, 0xBE, 0x52, 0x43, 0x1B, 
			0xE9, 0x29, 0x33, 0x63, 0x81, 0x75, 0xED, 0x36, 0x70, 0x4B, 0x45, 0x2D, 0xE4, 0x6D, 0x1E, 0xE9, 
			0xFC, 0x62, 0xFC, 0x2F, 0x49, 0x6D, 0xF6, 0x9F, 0xAA, 0xB4, 0xD4, 0x25, 0x8C, 0x27, 0xCD, 0x59, 
			0x4F, 0x1D, 0xE4, 0xCA, 0x1A, 0x0F, 0xBB, 0xE4, 0xFF, 0x88, 0x27, 0x66, 0xDB, 0x59, 0xBB, 0x63, 
			0xA2, 0xC1, 0xAE, 0xD9, 0xAF, 0xC4, 0x1D, 0xDB, 0x43, 0xB2, 0x96, 0x9D, 0x9B, 0xDF, 0xB8, 0x28, 
			0x0B, 0x74, 0xDB, 0x30, 0xD0, 0xF6, 0x66, 0x76, 0xCF, 0x8D, 0x03, 0x7A, 0xA2, 0x76, 0x43, 0x57, 
			0x79, 0x5A, 0x7D, 0x23, 0x7B, 0xD3, 0x46, 0x3B, 0xA6, 0xCE, 0x71, 0x82, 0xF8, 0xD2, 0x85, 0xDB, 
			0x29, 0x66, 0x5A, 0xBE, 0xC2, 0xAA, 0x23, 0xE5, 0x27, 0xC9, 0xE1, 0x1D, 0x99, 0x20, 0xDC, 0x68, 
			0xEB, 0x63, 0x65, 0x18, 0x21, 0x4F, 0x2C, 0x58, 0xDE, 0x81, 0x5B, 0xBE, 0x7B, 0xBB, 0x85, 0x16, 
			0x43, 0x81, 0x29, 0x44, 0xA3, 0xEC, 0xE5, 0xDF, 0xBB, 0x08, 0x3E, 0xF5, 0xC0, 0x5A, 0xB2, 0x82, 
			0xAD, 0xAF, 0x4C, 0x78, 0x93, 0xA3, 0x45, 0x92, 0x20, 0x5A, 0xB4, 0xE2, 0x4E, 0x64, 0xA0, 0x45, 
			0xB5, 0x71, 0x17, 0x2D, 0x64, 0x90, 0xBF, 0xDE, 0xD0, 0x30, 0xB1, 0xE8, 0x9C, 0x25, 0xB7, 0x71, 
			0xBE, 0x0B, 0x45, 0x38, 0x6F, 0x4C, 0x8F, 0xF2, 0x65, 0xBD, 0x94, 0xCB, 0xF5, 0x46, 0x09, 0xE8, 
			0xD3, 0x8A, 0xF9, 0xE8, 0x87, 0xB0, 0x74, 0x7F, 0xA5, 0x40, 0x0D, 0x6D, 0x73, 0xC1, 0xD0, 0xE0, 
			0xF8, 0x58, 0x40, 0xB2, 0x4E, 0x27, 0xF2, 0xF7, 0xB5, 0x96, 0xC2, 0xFD, 0xD5, 0xB5, 0xB0, 0x3A, 
			0x29, 0x8A, 0x21, 0xCE, 0x39, 0xE2, 0xF1, 0x03, 0x07, 0x4F, 0xBC, 0x52, 0x7B, 0xE0, 0xB8, 0x5E, 
			0x1A, 0x08, 0xF1, 0xF5, 0x53, 0x77, 0x5A, 0xBC, 0x62, 0x8B, 0x8A, 0x6A, 0xAD, 0x63, 0x9E, 0xF8, 
			0x01, 0x56, 0xA8, 0xAB, 0x88, 0x7D, 0x13, 0xC5, 0x58, 0x3C, 0x81, 0xEB, 0x92, 0x9A, 0x41, 0xB6, 
			0xD0, 0xCC, 0x53, 0xDD, 0x33, 0xF8, 0x4E, 0xE0, 0xE7, 0xD6, 0x13, 0x99, 0xFB, 0xA9, 0x62, 0xF3, 
			0xC9, 0x02, 0xC4, 0x20, 0xB6, 0x90, 0x11, 0x58, 0x06, 0xFB, 0x3C, 0x0A, 0xD8, 0xDA, 0x9E, 0x9D, 
			0x26, 0x6D, 0x88, 0x01, 0x88, 0x19, 0x0D, 0x74, 0xE4, 0xC7, 0xE1, 0x74, 0xC2, 0x0A, 0x7F, 0x87
		};
	}
}
