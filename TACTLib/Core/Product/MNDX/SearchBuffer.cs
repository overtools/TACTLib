using System.Collections.Generic;
using System.Text;

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal class SearchBuffer
    {
        private List<byte> SearchResult = new List<byte>();
        private List<PATH_STOP> PathStops = new List<PATH_STOP>();   // Array of path checkpoints

        public int ItemIndex { get; set; } = 0;// Current name fragment: Index to various tables
        public int CharIndex { get; set; } = 0;
        public int ItemCount { get; set; } = 0;
        public CASCSearchPhase SearchPhase { get; private set; } = CASCSearchPhase.Initializing; // 0 = initializing, 2 = searching, 4 = finished

        public string Result
        {
            get { return Encoding.ASCII.GetString(SearchResult.ToArray()); }
        }

        public int NumBytesFound
        {
            get { return SearchResult.Count; }
        }

        public int NumPathStops
        {
            get { return PathStops.Count; }
        }

        public void Add(byte value)
        {
            SearchResult.Add(value);
        }

        public void RemoveRange(int index)
        {
            SearchResult.RemoveRange(index, SearchResult.Count - index);
        }

        public void AddPathStop(PATH_STOP item)
        {
            PathStops.Add(item);
        }

        public PATH_STOP GetPathStop(int index)
        {
            return PathStops[index];
        }

        public void Init()
        {
            SearchPhase = CASCSearchPhase.Initializing;
        }

        public void Finish()
        {
            SearchPhase = CASCSearchPhase.Finished;
        }

        public void InitSearchBuffers()
        {
            SearchResult.Clear();
            PathStops.Clear();

            ItemIndex = 0;
            CharIndex = 0;
            ItemCount = 0;
            SearchPhase = CASCSearchPhase.Searching;
        }
    }
}
