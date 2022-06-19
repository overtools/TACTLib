using System;
using System.Diagnostics.CodeAnalysis;

namespace TACTLib.Core.Product {
    [Flags]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Locale : uint {
        All = 0xFFFFFFFF,
        None = 0,
        Internal = 0x1,
        enUS = 0x2,
        koKR = 0x4,
        Unused = 0x8,
        frFR = 0x10,
        deDE = 0x20,
        zhCN = 0x40,
        esES = 0x80,
        zhTW = 0x100,
        enGB = 0x200,
        enCN = 0x400,
        enTW = 0x800,
        esMX = 0x1000,
        ruRU = 0x2000,
        ptBR = 0x4000,
        itIT = 0x8000,
        ptPT = 0x10000,
        jaJP = 0x20000
    }
}
