using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TACTLib.Client;
using TACTLib.Client.HandlerArgs;
using TACTLib.Core.VFS;

namespace TACTLib.Core.Product.Fenris;

[ProductHandler(TACTProduct.Diablo4)]
public class ProductHandler_Fenris : IProductHandler {
    public ClientHandler Client { get; }
    public int LogLevel { get; } = Debugger.IsAttached ? 2 : 1;

#region SNO Lists

    public EncryptedSnos EncryptedSnos { get; }
    public ReplacedSnos ReplacedSnos { get; }
    public SharedPayloadsMapping SharedPayloads { get; }
    public CoreTOC TOC { get; }
    public Dictionary<ulong, EncryptedNameDict> EncryptedNameDicts = new();

#endregion

#region Manfiests

    public VFSFileTree?[] CoreVFS { get; } = new VFSFileTree[4];
    public SnoManifest?[] CoreManifest { get; } = new SnoManifest[4];
    public Locale BaseLocale { get; set; } = Locale.Unused;
    public Locale SpeechLocale { get; set; } = Locale.Unused;
    public Locale TextLocale { get; set; } = Locale.Unused;

#endregion

    public ProductHandler_Fenris(ClientHandler client, IDisposable? stream) {
        stream?.Dispose();

        var clientArgs = client.CreateArgs.HandlerArgs as ClientCreateArgs_Fenris ?? new ClientCreateArgs_Fenris();

        if (client.VFS == null) {
            throw new ArgumentException(null, nameof(client.VFS));
        }

        if (Enum.TryParse<Locale>(client.CreateArgs.TextLanguage, out var textLocale)) {
            TextLocale = textLocale;
        }

        if (Enum.TryParse<Locale>(client.CreateArgs.SpeechLanguage, out var speechLocale)) {
            SpeechLocale = speechLocale;
        }

        if (Enum.TryParse<Locale>(clientArgs.BaseLanguage, out var baseLocale)) {
            BaseLocale = baseLocale;
        }

        Client = client;

        var core = CoreVFS[(int) SnoManifestRole.Core] = LoadVFS("base") ?? throw new InvalidOperationException();
        CoreManifest[(int) SnoManifestRole.Core] = LoadManifest("Base", Locale.All, SnoManifestRole.Core) ?? throw new InvalidOperationException();

        using (var encryptedSno = core.Open("EncryptedSNOs.dat")) {
            EncryptedSnos = new EncryptedSnos(encryptedSno);
        }

        using (var toc = core.Open("CoreTOC.dat")) {
            TOC = new CoreTOC(toc, EncryptedSnos);
        }

        foreach (var key in client.ConfigHandler.Keyring.Keys.Keys) {
            try {
                using var dict = core.Open($"EncryptedNameDict-0x{BinaryPrimitives.ReverseEndianness(key):x16}.dat");
                if (dict != null) {
                    var end = new EncryptedNameDict(dict);
                    EncryptedNameDicts[key] = end;
                    foreach (var (id, name) in end.Files) {
                        TOC.Files[id] = name;
                    }
                }
            } catch (FileNotFoundException) {
                // ignored
            }
        }

        using (var replaced = core.Open("CoreTOCReplacedSnosMapping.dat")) {
            ReplacedSnos = new ReplacedSnos(replaced);
        }

        using (var shared = core.Open("CoreTOCSharedPayloadsMapping.dat")) {
            SharedPayloads = new SharedPayloadsMapping(shared);
        }

        LoadLocale();
    }

    private VFSFileTree? LoadVFS(string path) {
        if (Client.VFS!.Files.Contains(path)) {
            using var locStream = Client.VFS.Open(path);
            return new VFSFileTree(Client, locStream!);
        }

        return null;
    }

    private SnoManifest? LoadManifest(string path, Locale locale, SnoManifestRole role) {
        if (Client.VFS!.Files.Contains(path)) {
            using var locStream = Client.VFS.Open(path);
            return new SnoManifest(locStream) {
                Locale = locale,
                Role = role,
            };
        }

        return null;
    }

    private void LoadLocale() {
        var loadedBase = CoreManifest[(int) SnoManifestRole.Base]?.Locale ?? Locale.Unused;
        var loadedSpeech = CoreManifest[(int) SnoManifestRole.Speech]?.Locale ?? Locale.Unused;
        var loadedText = CoreManifest[(int) SnoManifestRole.Text]?.Locale ?? Locale.Unused;

        if (loadedBase != BaseLocale) {
            if (BaseLocale is Locale.Unused) {
                CoreVFS[(int) SnoManifestRole.Base] = null;
                CoreManifest[(int) SnoManifestRole.Base] = null;
            } else {
                CoreVFS[(int) SnoManifestRole.Base] = LoadVFS($"{BaseLocale:G}_Base");
                CoreManifest[(int) SnoManifestRole.Base] = LoadManifest($"{BaseLocale:G}.base", BaseLocale, SnoManifestRole.Base);
            }

            BaseLocale = CoreManifest[(int) SnoManifestRole.Base]?.Locale ?? Locale.Unused;
        }

        if (loadedSpeech != SpeechLocale) {
            if (SpeechLocale is Locale.Unused) {
                CoreVFS[(int) SnoManifestRole.Speech] = null;
                CoreManifest[(int) SnoManifestRole.Speech] = null;
            } else {
                CoreVFS[(int) SnoManifestRole.Speech] = LoadVFS($"{SpeechLocale:G}_Speech");
                CoreManifest[(int) SnoManifestRole.Speech] = LoadManifest($"{SpeechLocale:G}.speech", SpeechLocale, SnoManifestRole.Speech);
            }

            SpeechLocale = CoreManifest[(int) SnoManifestRole.Speech]?.Locale ?? Locale.Unused;
        }

        if (loadedText != TextLocale) {
            if (TextLocale is Locale.Unused) {
                CoreVFS[(int) SnoManifestRole.Text] = null;
                CoreManifest[(int) SnoManifestRole.Text] = null;
            } else {
                CoreVFS[(int) SnoManifestRole.Text] = LoadVFS($"{TextLocale:G}_Text");
                CoreManifest[(int) SnoManifestRole.Text] = LoadManifest($"{TextLocale:G}.text", TextLocale, SnoManifestRole.Text);
            }

            TextLocale = CoreManifest[(int) SnoManifestRole.Text]?.Locale ?? Locale.Unused;
        }
    }

    public Stream? OpenFile(object? key) {
        if (key is SnoFile fileReq) {
            return OpenFile(fileReq.Id, fileReq.Type, fileReq.SubId);
        }

        return null;
    }

    // logLevel 2 logs reads, logLevel 1 logs missing.
    public Stream? OpenFile(uint id, SnoType type, uint subId = 0, bool waterfall = true) {
        // i assume if replacedId is zero then it's deleted
        if (ReplacedSnos.Lookup.TryGetValue(id, out var replacedId)) {
            id = replacedId;
        }

        if (id is 0 or uint.MaxValue) { // 0 = null
            if (LogLevel >= 1) {
                Logger.Debug("Fenris", $"{type} {id} {subId} is deleted");
            }

            return null;
        }

        switch (type) {
            case SnoType.Child when SharedPayloads.SharedChildren.TryGetValue(new SnoChild {
                SnoId = id,
                SubId = subId,
            }, out var sharedChild):
                id = sharedChild.SnoId;
                subId = sharedChild.SubId;
                break;
            case >= SnoType.Payload when SharedPayloads.SharedPayloads.TryGetValue(id, out var sharedId):
                id = sharedId;
                break;
        }

        var path = $"{type.ToString("G").ToLower()}/{id}";

        if (type is SnoType.Child) {
            path += $"-{subId}";
        }

        LoadLocale();

        Stream? value;
        foreach (var (vfs, manifest) in CoreVFS.Zip(CoreManifest)) {
            if (vfs == null || manifest == null) {
                continue;
            }

            if (type is SnoType.Child ? manifest.ContainsChild(id, subId) : manifest.Contains(id)) {
                value = vfs.Open(path);
                if (value is null) {
                    if (LogLevel >= 1 && (!waterfall || type is not (SnoType.Payload or SnoType.Paymid))) {
                        Logger.Debug("Fenris", $"{id}{(type is SnoType.Child ? "-" + subId : "")} found in {manifest.Locale:G} ({manifest.Role}) but VFS returned null for type {type:G}");
                    }

                    continue;
                }

                if (LogLevel >= 2) {
                    Logger.Debug("Fenris", $"{id}{(type is SnoType.Child ? "-" + subId : "")} ({type:G}) found in {manifest.Locale:G} ({manifest.Role})");
                }

                return value;
            }
        }

        value = type switch { // quality waterfall. payload = hires, paymid = without hires, paylow = tiny files
                    SnoType.Payload when waterfall => OpenFile(id, SnoType.Paymid, subId),
                    SnoType.Paymid when waterfall  => OpenFile(id, SnoType.Paylow, subId),
                    _                              => null,
                };

        if (value is null && LogLevel >= 1) {
            Logger.Debug("Fenris", $"{id}{(type is SnoType.Child ? "-" + subId : "")} {type:G} not found in any locale ({BaseLocale:G}, {SpeechLocale:G}, {TextLocale:G})");
        }

        return value;
    }
}
