using System;
using System.IO;
using static TACTLib.Helpers.Extensions;

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal class TNameIndexStruct
    {
        private byte[] NameFragments;
        private TSparseArray FragmentEnds;

        public int Count
        {
            get { return NameFragments.Length; }
        }

        public TNameIndexStruct(BinaryReader reader)
        {
            NameFragments = reader.ReadArray<byte>();
            FragmentEnds = new TSparseArray(reader);
        }

        public bool CheckAndCopyNameFragment(MNDXSearchResult pStruct1C, int dwFragOffs)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;

            if (FragmentEnds.TotalItemCount == 0)
            {
                string szSearchMask = pStruct1C.SearchMask;

                int startPos = dwFragOffs - pStruct40.CharIndex;

                // Keep copying as long as we don't reach the end of the search mask
                while (pStruct40.CharIndex < pStruct1C.SearchMask.Length)
                {
                    // HOTS: 195A5A0
                    if (NameFragments[startPos + pStruct40.CharIndex] != szSearchMask[pStruct40.CharIndex])
                        return false;

                    // HOTS: 195A5B7
                    pStruct40.Add(NameFragments[startPos + pStruct40.CharIndex]);
                    pStruct40.CharIndex++;

                    if (NameFragments[startPos + pStruct40.CharIndex] == 0)
                        return true;
                }

                // HOTS: 195A660
                // Now we need to copy the rest of the fragment
                while (NameFragments[startPos + pStruct40.CharIndex] != 0)
                {
                    pStruct40.Add(NameFragments[startPos + pStruct40.CharIndex]);
                    startPos++;
                }
            }
            else
            {
                // Get the offset of the fragment to compare
                // HOTS: 195A6B7
                string szSearchMask = pStruct1C.SearchMask;

                // Keep copying as long as we don't reach the end of the search mask
                while (dwFragOffs < pStruct1C.SearchMask.Length)
                {
                    if (NameFragments[dwFragOffs] != szSearchMask[pStruct40.CharIndex])
                        return false;

                    pStruct40.Add(NameFragments[dwFragOffs]);
                    pStruct40.CharIndex++;

                    // Keep going as long as the given bit is not set
                    if (FragmentEnds.Contains(dwFragOffs++))
                        return true;
                }

                // Now we need to copy the rest of the fragment
                while (!FragmentEnds.Contains(dwFragOffs))
                {
                    // HOTS: 195A7A6
                    pStruct40.Add(NameFragments[dwFragOffs]);
                    dwFragOffs++;
                }
            }

            return true;
        }

        public bool CheckNameFragment(MNDXSearchResult pStruct1C, int dwFragOffs)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;

            if (FragmentEnds.TotalItemCount == 0)
            {
                // Get the offset of the fragment to compare. For convenience with pStruct40->CharIndex,
                // subtract the CharIndex from the fragment offset
                string szSearchMask = pStruct1C.SearchMask;

                int startPos = dwFragOffs - pStruct40.CharIndex;

                // Keep searching as long as the name matches with the fragment
                while (NameFragments[startPos + pStruct40.CharIndex] == szSearchMask[pStruct40.CharIndex])
                {
                    // Move to the next character
                    pStruct40.CharIndex++;

                    // Is it the end of the fragment or end of the path?
                    if (NameFragments[startPos + pStruct40.CharIndex] == 0)
                        return true;

                    if (pStruct40.CharIndex >= pStruct1C.SearchMask.Length)
                        return false;
                }

                return false;
            }
            else
            {
                // Get the offset of the fragment to compare.
                string szSearchMask = pStruct1C.SearchMask;

                // Keep searching as long as the name matches with the fragment
                while (NameFragments[dwFragOffs] == szSearchMask[pStruct40.CharIndex])
                {
                    // Move to the next character
                    pStruct40.CharIndex++;

                    // Is it the end of the fragment or end of the path?
                    if (FragmentEnds.Contains(dwFragOffs++))
                        return true;

                    if (dwFragOffs >= pStruct1C.SearchMask.Length)
                        return false;
                }

                return false;
            }
        }

        public void CopyNameFragment(MNDXSearchResult pStruct1C, int dwFragOffs)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;

            if (FragmentEnds.TotalItemCount == 0)
            {
                while (NameFragments[dwFragOffs] != 0)
                {
                    pStruct40.Add(NameFragments[dwFragOffs++]);
                }
            }
            else
            {
                while (!FragmentEnds.Contains(dwFragOffs))
                {
                    pStruct40.Add(NameFragments[dwFragOffs++]);
                }
            }
        }
    }
}
