using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TACTLib.Client;
using TACTLib.Container;
using static TACTLib.Helpers.Extensions;

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    public class MNDXEntry
    {
        public MNDXRootEntry Info { get; set; }
        public string Path { get; set; }
        public Locale Locale { get; set; }
    }

    [ProductHandler(TACTProduct.HeroesOfTheStorm), ProductHandler(TACTProduct.StarCraft2)]
    public class ProductHandler_MNDX : IProductHandler
    {
        private const int CASC_MNDX_SIGNATURE = 0x58444E4D;          // 'MNDX'
        private const int CASC_MAX_MAR_FILES = 3;

        //[0] - package names
        //[1] - file names stripped off package names
        //[2] - complete file names
        private MARFileNameDB[] MarFiles = new MARFileNameDB[CASC_MAX_MAR_FILES];

        private Dictionary<int, MNDXRootEntry> mndxRootEntries = new Dictionary<int, MNDXRootEntry>();
        private Dictionary<int, MNDXRootEntry> mndxRootEntriesValid;

        private Dictionary<int, string> Packages = new Dictionary<int, string>();
        private Dictionary<string, int> PackagesValue = new Dictionary<string, int>();

        public List<MNDXEntry> Entries = new List<MNDXEntry>();

        private ClientHandler Handler { get; }

        public ProductHandler_MNDX(ClientHandler client, Stream stream)
        {
            Handler = client;
            using (var reader = new BinaryReader(stream))
            {
                var header = reader.Read<MNDXHeader>();

                if (header.Signature != CASC_MNDX_SIGNATURE || header.FormatVersion > 2 || header.FormatVersion < 1)
                    throw new Exception("invalid root file");

                if (header.HeaderVersion == 2)
                {
                    var build1 = reader.ReadInt32(); // build number
                    var build2 = reader.ReadInt32(); // build number
                }

                int MarInfoOffset = reader.ReadInt32();                            // Offset of the first MAR entry info
                int MarInfoCount = reader.ReadInt32();                             // Number of the MAR info entries
                int MarInfoSize = reader.ReadInt32();                              // Size of the MAR info entry
                int MndxEntriesOffset = reader.ReadInt32();
                int MndxEntriesTotal = reader.ReadInt32();                         // Total number of MNDX root entries
                int MndxEntriesValid = reader.ReadInt32();                         // Number of valid MNDX root entries
                int MndxEntrySize = reader.ReadInt32();                            // Size of one MNDX root entry

                if (MarInfoCount > CASC_MAX_MAR_FILES || MarInfoSize != Marshal.SizeOf<MARInfo>())
                    throw new Exception("invalid root file (1)");

                for (int i = 0; i < MarInfoCount; i++)
                {
                    stream.Position = MarInfoOffset + (MarInfoSize * i);

                    MARInfo marInfo = reader.Read<MARInfo>();

                    stream.Position = marInfo.MarDataOffset;

                    MarFiles[i] = new MARFileNameDB(reader);

                    if (stream.Position != marInfo.MarDataOffset + marInfo.MarDataSize)
                        throw new Exception("MAR parsing error!");
                }

                stream.Position = MndxEntriesOffset;

                MNDXRootEntry prevEntry = null;

                for (int i = 0; i < MndxEntriesTotal; i++)
                {
                    MNDXRootEntry entry = new MNDXRootEntry();

                    if (prevEntry != null)
                        prevEntry.Next = entry;

                    prevEntry = entry;
                    entry.Flags = reader.ReadInt32();
                    entry.Key = reader.Read<CKey>();
                    entry.FileSize = reader.ReadInt32();
                    mndxRootEntries.Add(i, entry);
                }

                mndxRootEntriesValid = new Dictionary<int, MNDXRootEntry>();

                int ValidEntryCount = 1; // edx
                int index = 0;

                mndxRootEntriesValid[index++] = mndxRootEntries[0];

                for (int i = 0; i < MndxEntriesTotal; i++)
                {
                    if (ValidEntryCount >= MndxEntriesValid)
                        break;

                    if ((mndxRootEntries[i].Flags & 0x80000000) != 0)
                    {
                        mndxRootEntriesValid[index++] = mndxRootEntries[i + 1];

                        ValidEntryCount++;
                    }
                }


                Logger.Info("MNDX", "loading file names...");

                Regex regex = new Regex("\\w{4}(?=\\.(storm|sc2)(data|assets))", RegexOptions.Compiled);
                var packagesLocale = new Dictionary<int, Locale>();
                foreach (var result in MarFiles[0].EnumerateFiles())
                {
                    Packages.Add(result.FileNameIndex, result.FoundPath);

                    Match match = regex.Match(result.FoundPath);

                    if (match.Success)
                    {
                        var localeStr = match.Value;

                        if (!Enum.TryParse(localeStr, true, out Locale locale))
                            locale = Locale.All;

                        packagesLocale.Add(result.FileNameIndex, locale);
                    }
                    else
                        packagesLocale.Add(result.FileNameIndex, Locale.All);
                }

                Packages = Packages.OrderByDescending(x => x.Value.Length).ToDictionary(x => x.Key, x => x.Value);
                PackagesValue = Packages.ToDictionary(x => x.Value, x => x.Key);
                
                foreach (var result in MarFiles[2].EnumerateFiles())
                {
                    var file = result.FoundPath;
                    var package = FindMNDXPackage(file);
                    var info = FindMNDXInfo(file, package);
                    var entry = new MNDXEntry
                    {
                        Path = file,
                        Locale = packagesLocale[package],
                        Info = info
                    };
                    Entries.Add(entry);
                }
            }
        }

        private int FindMNDXPackage(string fileName)
        {
            var index = fileName.LastIndexOf('/');
            if (index == -1) return -1;
            while(index > -1)
            {
                if (PackagesValue.TryGetValue(fileName.Substring(0, index), out var packageId))
                {
                    return packageId;
                }

                index = fileName.LastIndexOf('/', index - 1);
            }

            return -1;
        }

        private MNDXRootEntry FindMNDXInfo(string path, int dwPackage)
        {
            MNDXSearchResult result = new MNDXSearchResult()
            {
                SearchMask = path.Substring(Packages[dwPackage].Length + 1).ToLower()
            };
            MARFileNameDB marFile1 = MarFiles[1];

            if (marFile1.FindFileInDatabase(result))
            {
                var pRootEntry = mndxRootEntriesValid[result.FileNameIndex];

                while ((pRootEntry.Flags & 0x00FFFFFF) != dwPackage)
                {
                    // The highest bit serves as a terminator if set
                    if ((pRootEntry.Flags & 0x80000000) != 0)
                        throw new Exception("File not found!");

                    pRootEntry = pRootEntry.Next;
                }

                // Give the root entry pointer to the caller
                return pRootEntry;
            }

            throw new Exception("File not found!");
        }

        private MNDXRootEntry FindMNDXInfo2(string path, int dwPackage)
        {
            MNDXSearchResult result = new MNDXSearchResult()
            {
                SearchMask = path
            };
            MARFileNameDB marFile2 = MarFiles[2];

            if (marFile2.FindFileInDatabase(result))
            {
                var pRootEntry = mndxRootEntries[result.FileNameIndex];

                while ((pRootEntry.Flags & 0x00FFFFFF) != dwPackage)
                {
                    // The highest bit serves as a terminator if set
                    //if ((pRootEntry.Flags & 0x80000000) != 0)
                    //    throw new Exception("File not found!");

                    pRootEntry = pRootEntry.Next;
                }

                // Give the root entry pointer to the caller
                return pRootEntry;
            }

            throw new Exception("File not found!");
        }

        public Stream OpenFile(object key)
        {
            switch (key)
            {
                case MNDXEntry entry:
                    return Handler.OpenCKey(entry.Info.Key);
                case CKey ckey:
                    return Handler.OpenCKey(ckey);
                case EKey ekey:
                    return Handler.OpenEKey(ekey);
                default:
                    throw new ArgumentException(nameof(key));
            }
        }
    }
}
