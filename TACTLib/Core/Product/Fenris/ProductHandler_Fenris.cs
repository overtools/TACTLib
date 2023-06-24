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
    public VFSFileTree VFS { get; }
    public VFSFileTree? BaseLocaleVFS { get; private set; }
    public VFSFileTree? SpeechLocaleVFS { get; private set; }
    public VFSFileTree? TextLocaleVFS { get; private set; }
    public EncryptedSnos EncryptedSnos { get; }
    public ReplacedSnos ReplacedSnos { get; }
    public CoreTOC TOC { get; }
    public SharedPayloadsMapping SharedPayloads { get; }
    public Locale BaseLocale { get; set; }
    public Locale SpeechLocale { get; set; }
    public Locale TextLocale { get; set; }
    private Locale LoadedBaseLocale { get; set; }
    private Locale LoadedSpeechLocale { get; set; }
    private Locale LoadedTextLocale { get; set; }
    public Dictionary<uint, Dictionary<string, uint>> Ids { get; }

    public ProductHandler_Fenris(ClientHandler client, IDisposable? stream) {
        stream?.Dispose();
        
        if (Enum.TryParse<Locale>(client.CreateArgs.TextLanguage, out var textLocale)) {
            TextLocale = textLocale;
        }
        
        if (Enum.TryParse<Locale>(client.CreateArgs.SpeechLanguage, out var speechLocale)) {
            SpeechLocale = speechLocale;
        }
        
        if (client.VFS == null) {
            throw new ArgumentException(null, nameof(client.VFS));
        }
        
        Client = client;

        using (var baseVfs = client.VFS.Open("base")) {
            VFS = new VFSFileTree(client, baseVfs);
        }

        using (var encryptedSno = VFS.Open("EncryptedSNOs.dat")) {
            EncryptedSnos = new EncryptedSnos(encryptedSno);
        }

        using (var toc = VFS.Open("CoreTOC.dat")) {
            TOC = new CoreTOC(toc, EncryptedSnos);
        }

        LoadLocaleVFS();

        using (var replaced = VFS.Open("CoreTOCReplacedSnosMapping.dat")) {
            ReplacedSnos = new ReplacedSnos(replaced);
        }
        
        using (var shared = VFS.Open("CoreTOCSharedPayloadsMapping.dat")) {
            SharedPayloads = new SharedPayloadsMapping(shared);
        }

        Ids = TOC.Files.GroupBy(x => x.Key.Group)
                 .ToDictionary(x => x.Key, 
                               x => x.ToDictionary(y => y.Value, 
                                                   y => y.Key.Id));
    }

    private void LoadLocaleVFS() {
        if (LoadedBaseLocale == BaseLocale && LoadedSpeechLocale == SpeechLocale && LoadedTextLocale == TextLocale) {
            return;
        }
        
        using var _ = new PerfCounter("ProductHandler_Fenris::LoadLocaleVFS");
        if (LoadedBaseLocale != BaseLocale) {
            LoadedBaseLocale = BaseLocale;
            if (Client.VFS!.Files.Contains($"{BaseLocale:G}_Base")) {
                using var locStream = Client.VFS.Open($"{BaseLocale:G}_Base");
                BaseLocaleVFS = new VFSFileTree(Client, locStream);
            } else {
                BaseLocaleVFS = null;
            }
        }
        
        
        if (LoadedSpeechLocale != SpeechLocale) {
            LoadedSpeechLocale = SpeechLocale;
            if (Client.VFS!.Files.Contains($"{SpeechLocale:G}_Speech")) {
                using var locStream = Client.VFS.Open($"{SpeechLocale:G}_Speech");
                SpeechLocaleVFS = new VFSFileTree(Client, locStream);
            } else {
                SpeechLocaleVFS = null;
            }
        }
        
        
        if (LoadedTextLocale != TextLocale) {
            LoadedTextLocale = TextLocale;
            if (Client.VFS!.Files.Contains($"{TextLocale:G}_Text")) {
                using var locStream = Client.VFS.Open($"{TextLocale:G}_Text");
                TextLocaleVFS = new VFSFileTree(Client, locStream);
            } else {
                TextLocaleVFS = null;
            }
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
        
        LoadLocaleVFS();

        if (BaseLocaleVFS?.Files.Contains(path) == true) {
            return BaseLocaleVFS.Open(path);
        }
        
        if (VFS.Files.Contains(path)) {
            return VFS.Open(path);
        }
        
        if (SpeechLocaleVFS?.Files.Contains(path) == true) {
            return SpeechLocaleVFS.Open(path);
        }
        
        if (TextLocaleVFS?.Files.Contains(path) == true) {
            return TextLocaleVFS.Open(path);
        }

        return type switch { // quality waterfall. payload = hires, paymid = without hires, paylow = tiny files 
                   SnoType.Payload => OpenFile(id, SnoType.Paymid, subId),
                   SnoType.Paymid  => OpenFile(id, SnoType.Paylow, subId),
                   _               => null,
               };
    }
}
