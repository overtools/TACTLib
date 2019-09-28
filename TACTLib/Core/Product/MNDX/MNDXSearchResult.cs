using System;

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal class MNDXSearchResult
    {
        private string szSearchMask;
        public string SearchMask        // Search mask without wildcards
        {
            get { return szSearchMask; }
            set
            {
                Buffer.Init();

                szSearchMask = value ?? throw new ArgumentNullException(nameof(SearchMask));
            }
        }
        public string FoundPath { get; private set; }       // Found path name
        public int FileNameIndex { get; private set; }      // Index of the file name
        public SearchBuffer Buffer { get; private set; }

        public MNDXSearchResult()
        {
            Buffer = new SearchBuffer();

            SearchMask = string.Empty;
        }

        public void SetFindResult(string szFoundPath, int dwFileNameIndex)
        {
            FoundPath = szFoundPath;
            FileNameIndex = dwFileNameIndex;
        }
    }
}
