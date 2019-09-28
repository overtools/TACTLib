// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal class PATH_STOP
    {
        public int ItemIndex { get; set; }
        public int Field_4 { get; set; }
        public int Field_8 { get; set; }
        public int Field_C { get; set; }
        public int Field_10 { get; set; }

        public PATH_STOP()
        {
            ItemIndex = 0;
            Field_4 = 0;
            Field_8 = 0;
            Field_C = -1;
            Field_10 = -1;
        }
    }
}
