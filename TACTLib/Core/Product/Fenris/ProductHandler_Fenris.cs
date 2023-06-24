using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TACTLib.Client;
using TACTLib.Core.VFS;
using TACTLib.Helpers;

namespace TACTLib.Core.Product.Fenris;

[ProductHandler(TACTProduct.Diablo4)]
public class ProductHandler_Fenris : IProductHandler {
    public ClientHandler Client { get; }

#region SNO Lists

    public EncryptedSnos EncryptedSnos { get; }
    public ReplacedSnos ReplacedSnos { get; }
    public SharedPayloadsMapping SharedPayloads { get; }
    public CoreTOC TOC { get; }

#endregion

#region VFS

    public VFSFileTree CoreVFS { get; }
    public VFSFileTree? BaseVFS { get; private set; }
    public VFSFileTree? SpeechVFS { get; private set; }
    public VFSFileTree? TextVFS { get; private set; }

#endregion

#region Manifests

    public SnoManifest CoreManifest { get; }
    public SnoManifest? BaseManifest { get; private set; }
    public SnoManifest? SpeechManifest { get; private set; }
    public SnoManifest? TextManifest { get; private set; }

#endregion

#region Locale

    public Locale BaseLocale { get; set; }
    public Locale SpeechLocale { get; set; }
    public Locale TextLocale { get; set; }
    private Locale LoadedBaseLocale { get; set; }
    private Locale LoadedSpeechLocale { get; set; }
    private Locale LoadedTextLocale { get; set; }

#endregion

    public Dictionary<uint, Dictionary<string, uint>> Ids { get; }

    public ProductHandler_Fenris(ClientHandler client, IDisposable? stream) {
        stream?.Dispose();

        if (client.VFS == null) {
            throw new ArgumentException(null, nameof(client.VFS));
        }

        if (Enum.TryParse<Locale>(client.CreateArgs.TextLanguage, out var textLocale)) {
            TextLocale = textLocale;
        }

        if (Enum.TryParse<Locale>(client.CreateArgs.SpeechLanguage, out var speechLocale)) {
            SpeechLocale = speechLocale;
        }

        Client = client;

        CoreVFS = LoadVFS("base") ?? throw new InvalidOperationException();
        CoreManifest = LoadManifest("Base") ?? throw new InvalidOperationException();

        using (var encryptedSno = CoreVFS.Open("EncryptedSNOs.dat")) {
            EncryptedSnos = new EncryptedSnos(encryptedSno);
        }

        using (var toc = CoreVFS.Open("CoreTOC.dat")) {
            TOC = new CoreTOC(toc, EncryptedSnos);
        }

        LoadLocale();

        using (var replaced = CoreVFS.Open("CoreTOCReplacedSnosMapping.dat")) {
            ReplacedSnos = new ReplacedSnos(replaced);
        }

        using (var shared = CoreVFS.Open("CoreTOCSharedPayloadsMapping.dat")) {
            SharedPayloads = new SharedPayloadsMapping(shared);
        }

        Ids = TOC.Files.GroupBy(x => x.Key.Group)
                 .ToDictionary(x => x.Key,
                               x => x.ToDictionary(y => y.Value,
                                                   y => y.Key.Id));
    }

    private VFSFileTree? LoadVFS(string path) {
        if (Client.VFS!.Files.Contains(path)) {
            using var locStream = Client.VFS.Open(path);
            return new VFSFileTree(Client, locStream);
        }

        return null;
    }

    private SnoManifest? LoadManifest(string path) {
        if (Client.VFS!.Files.Contains(path)) {
            using var locStream = Client.VFS.Open(path);
            return new SnoManifest(locStream);
        }

        return null;
    }

    private void LoadLocale() {
        if (LoadedBaseLocale == BaseLocale && LoadedSpeechLocale == SpeechLocale && LoadedTextLocale == TextLocale) {
            return;
        }

        using var _ = new PerfCounter("ProductHandler_Fenris::LoadLocaleVFS");
        if (LoadedBaseLocale != BaseLocale) {
            LoadedBaseLocale = BaseLocale;
            BaseVFS = LoadVFS($"{BaseLocale:G}_Base");
            BaseManifest = LoadManifest($"{BaseLocale:G}.base");
        }


        if (LoadedSpeechLocale != SpeechLocale) {
            LoadedSpeechLocale = SpeechLocale;
            SpeechVFS = LoadVFS($"{SpeechLocale:G}_Speech");
            SpeechManifest = LoadManifest($"{SpeechLocale:G}.speech");
        }


        if (LoadedTextLocale != TextLocale) {
            LoadedTextLocale = TextLocale;
            TextVFS = LoadVFS($"{TextLocale:G}_Text");
            TextManifest = LoadManifest($"{TextLocale:G}.text");
        }
    }

    public Stream? OpenFile(object? key) {
        if (key is SnoFile fileReq) {
            return OpenFile(fileReq.Id, fileReq.Type, fileReq.SubId);
        }

        return null;
    }

    public Stream? OpenFile(uint id, SnoType type, uint subId = 0) {
        if (id == 0) { // 0 = null
            return null;
        }

        // i assume if replacedId is zero then it's deleted, but we don't care.
        if (ReplacedSnos.Lookup.TryGetValue(id, out var replacedId) && replacedId > 0) {
            id = replacedId;
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

        if (BaseVFS?.Files.Contains(path) == true) {
            return BaseVFS.Open(path);
        }

        if (CoreVFS.Files.Contains(path)) {
            return CoreVFS.Open(path);
        }

        if (SpeechVFS?.Files.Contains(path) == true) {
            return SpeechVFS.Open(path);
        }

        if (TextVFS?.Files.Contains(path) == true) {
            return TextVFS.Open(path);
        }

        return type switch { // quality waterfall. payload = hires, paymid = without hires, paylow = tiny files 
                   SnoType.Payload => OpenFile(id, SnoType.Paymid, subId),
                   SnoType.Paymid  => OpenFile(id, SnoType.Paylow, subId),
                   _               => null,
               };
    }
}
