using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using TACTLib.Client;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Tank {
    public class ContentManifestFile {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HashData : IComparable<HashData> { // version 25?
            public ulong GUID;
            public uint Size;
            public byte Unknown;
            public CKey ContentKey;

            public int CompareTo(HashData other) {
                return GUID.CompareTo(other.GUID);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HashData24 { // version 24?
            public ulong GUID;
            public uint Size;
            public CKey ContentKey;

            public HashData ToHashData() {
                return new HashData() {
                    GUID = GUID,
                    Size = Size,
                    Unknown = 0,
                    ContentKey = ContentKey
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct CMFHeader25 { // 1.22+
            public uint m_buildVersion;
            public uint m_unk04;
            public uint m_unk08;
            public uint m_unk0C;
            public uint m_unk10;
            public uint m_unk14;
            public int m_dataCount;
            public uint m_unk1C;
            public int m_entryCount;
            public uint m_magic;

            public CMFHeader Upgrade() {
                return new CMFHeader {
                    m_buildVersion = m_buildVersion,
                    m_dataCount = m_dataCount,
                    m_entryCount = m_entryCount,
                    m_magic = m_magic
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct CMFHeader { // 1.48+, version 26
            public uint m_buildVersion;
            public uint m_unk04;
            public uint m_unk08;
            public uint m_unk0C;
            public uint m_unk10;
            public uint m_unk14;
            public uint m_unk18;
            public int m_dataPatchRecordCount;
            public int m_dataCount;
            public int m_entryPatchRecordCount;
            public int m_entryCount;
            public uint m_magic;

            public uint GetNonEncryptedMagic() {
                return (uint) (0x00666D63u | (GetVersion() << 24));
            }

            public byte GetVersion() {
                return IsEncrypted() ? (byte) (m_magic & 0x000000FF) : (byte) ((m_magic & 0xFF000000) >> 24);
            }

            public bool IsEncrypted() {
                return (m_magic >> 8) == ENCRYPTED_MAGIC;
            }
        }

        public string m_name;
        public CMFHeader m_header;

        public AssetPackageManifest.Entry[] m_entries = null!;
        public HashData[] m_hashList = null!;

        // ReSharper disable once InconsistentNaming
        public const int ENCRYPTED_MAGIC = 0x636D66; // todo: use the thingy again?

        public ContentManifestFile(ClientHandler client, Stream stream, string name) {
            m_name = name;
            using (BinaryReader reader = new BinaryReader(stream)) {
                m_header = reader.Read<CMFHeader>();
                if (m_header.m_buildVersion < ProductHandler_Tank.VERSION_148_PTR) { // before 1.48
                    stream.Position = 0;
                    m_header = reader.Read<CMFHeader25>().Upgrade();
                }

                if (m_header.m_buildVersion >= 12923648 || m_header.m_buildVersion < 52320) {
                    throw new NotSupportedException("Overwatch 1.29 or earlier is not supported");
                }

                if (m_header.IsEncrypted()) {
                    using (var decryptedReader = ManifestCryptoHandler.GetDecryptedReader(name, "CMF", m_header, m_header.m_buildVersion, client.Product, stream))
                        ParseEntries(decryptedReader);
                } else {
                    ParseEntries(reader);
                }
            }
        }

        private void ParseEntries(BinaryReader reader) {
            m_entries = reader.ReadArray<AssetPackageManifest.Entry>(m_header.m_entryCount);
            if (m_header.m_buildVersion >= 57230) { // 1.35+
                m_hashList = reader.ReadArray<HashData>(m_header.m_dataCount);
            } else {
                m_hashList = reader.ReadArray<HashData24>(m_header.m_dataCount).Select(x => x.ToHashData()).ToArray();
            }

            if (m_entries.Length >= 1 && m_entries[0].m_index != 1) {
                Logger.Warn("CMF", "CMF Crypto using invalid IV. This can probably be ignored.");
            }
        }

        public bool TryGet(ulong guid, out HashData hashData) {
            var speculativeEntry = new HashData {
                GUID = guid
            };

            var index = Array.BinarySearch(m_hashList, speculativeEntry);
            if (index < 0 || index >= m_hashList.Length) {
                hashData = default;
                return false;
            }

            hashData = m_hashList[index];
            return true;
        }

        public bool Exists(ulong guid) {
            return TryGet(guid, out _);
        }

        public HashData GetHashData(ulong guid) {
            if (TryGet(guid, out var data)) {
                return data;
            }

            throw new FileNotFoundException($"{guid:X16}");
        }

        public Stream? OpenFile(ClientHandler client, ulong guid) {
            var data = GetHashData(guid);
            return client.OpenCKey(data.ContentKey);
        }
    }
}