using TACTLib;
using TACTLib.Container;

namespace TACTView.Api.Models {
    public interface IFileEntry : IDirectoryEntry {
        public long Size { get; init; }
        public Locale Locale { get; init; }
        public CKey Key { get; init; }
    }
}
