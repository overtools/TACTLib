using System;
using System.Windows.Controls;
using TACTLib;
using TACTLib.Core.Product;
using TACTView.Api.Models;

namespace TACTView.Api {
    public interface IFilePreview {
        public TACTProduct[]? GetValidProducts();
        public bool IsValidFor(IFileEntry entry, IProductHandler handler);
        public Control GetPreviewControl(IFileEntry entry, Span<byte> data, IProductHandler handler);
    }
}
