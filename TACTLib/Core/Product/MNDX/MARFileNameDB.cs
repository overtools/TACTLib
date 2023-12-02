using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

// https://raw.githubusercontent.com/WoW-Tools/CascLib/master/CascLib/RootHandlers/MNDXRootHandler.cs

namespace TACTLib.Core.Product.MNDX
{
    internal class MARFileNameDB
    {
        private const int CASC_MAR_SIGNATURE = 0x0052414d;           // 'MAR\0'

        private TSparseArray Struct68_00;
        private TSparseArray FileNameIndexes;
        private TSparseArray Struct68_D0;
        private byte[] FrgmDist_LoBits;
        private TBitEntryArray FrgmDist_HiBits;
        private TNameIndexStruct IndexStruct_174;
        private MARFileNameDB? NextDB;
        private NAME_FRAG[] NameFragTable;
        private int NameFragIndexMask;
        private int field_214;

        public int NumFiles { get { return FileNameIndexes.ValidItemCount; } }

        private byte[] table_1BA1818 =
        {
            0x07, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x05, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x06, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x05, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x07, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x05, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x06, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x05, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x04, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00,
            0x07, 0x07, 0x07, 0x01, 0x07, 0x02, 0x02, 0x01, 0x07, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x05, 0x05, 0x01, 0x05, 0x02, 0x02, 0x01, 0x05, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x05, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x06, 0x06, 0x01, 0x06, 0x02, 0x02, 0x01, 0x06, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x06, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x06, 0x05, 0x05, 0x01, 0x05, 0x02, 0x02, 0x01, 0x05, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x05, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x07, 0x07, 0x01, 0x07, 0x02, 0x02, 0x01, 0x07, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x05, 0x05, 0x01, 0x05, 0x02, 0x02, 0x01, 0x05, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x05, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x06, 0x06, 0x01, 0x06, 0x02, 0x02, 0x01, 0x06, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x06, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x06, 0x05, 0x05, 0x01, 0x05, 0x02, 0x02, 0x01, 0x05, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x05, 0x04, 0x04, 0x01, 0x04, 0x02, 0x02, 0x01, 0x04, 0x03, 0x03, 0x01, 0x03, 0x02, 0x02, 0x01,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x02, 0x07, 0x07, 0x07, 0x03, 0x07, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x04, 0x07, 0x04, 0x04, 0x02, 0x07, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x02, 0x07, 0x05, 0x05, 0x03, 0x05, 0x03, 0x03, 0x02,
            0x07, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x02, 0x05, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x02, 0x07, 0x06, 0x06, 0x03, 0x06, 0x03, 0x03, 0x02,
            0x07, 0x06, 0x06, 0x04, 0x06, 0x04, 0x04, 0x02, 0x06, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x02, 0x06, 0x05, 0x05, 0x03, 0x05, 0x03, 0x03, 0x02,
            0x06, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x02, 0x05, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x02, 0x07, 0x07, 0x07, 0x03, 0x07, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x04, 0x07, 0x04, 0x04, 0x02, 0x07, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x02, 0x07, 0x05, 0x05, 0x03, 0x05, 0x03, 0x03, 0x02,
            0x07, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x02, 0x05, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x02, 0x07, 0x06, 0x06, 0x03, 0x06, 0x03, 0x03, 0x02,
            0x07, 0x06, 0x06, 0x04, 0x06, 0x04, 0x04, 0x02, 0x06, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x02, 0x06, 0x05, 0x05, 0x03, 0x05, 0x03, 0x03, 0x02,
            0x06, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x02, 0x05, 0x04, 0x04, 0x03, 0x04, 0x03, 0x03, 0x02,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x04, 0x07, 0x07, 0x07, 0x04, 0x07, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05, 0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x03,
            0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x04, 0x07, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x03,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x04, 0x07, 0x06, 0x06, 0x04, 0x06, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05, 0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x03,
            0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x04, 0x06, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x04, 0x07, 0x07, 0x07, 0x04, 0x07, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05, 0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x03,
            0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x04, 0x07, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x03,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x04, 0x07, 0x06, 0x06, 0x04, 0x06, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05, 0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x03,
            0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x04, 0x06, 0x05, 0x05, 0x04, 0x05, 0x04, 0x04, 0x03,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05, 0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05, 0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05, 0x07, 0x07, 0x07, 0x05, 0x07, 0x05, 0x05, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05,
            0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05, 0x07, 0x06, 0x06, 0x05, 0x06, 0x05, 0x05, 0x04,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x05,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x06, 0x06, 0x05,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
            0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07
        };

        public MARFileNameDB(BinaryReader reader, bool next = false)
        {
            if (!next && reader.ReadInt32() != CASC_MAR_SIGNATURE)
                throw new Exception("invalid MAR file");

            Struct68_00 = new TSparseArray(reader);
            FileNameIndexes = new TSparseArray(reader);
            Struct68_D0 = new TSparseArray(reader);
            FrgmDist_LoBits = reader.ReadArray<byte>();
            FrgmDist_HiBits = new TBitEntryArray(reader);
            IndexStruct_174 = new TNameIndexStruct(reader);

            if (Struct68_D0.ValidItemCount != 0 && IndexStruct_174.Count == 0)
            {
                NextDB = new MARFileNameDB(reader, true);
            }

            NameFragTable = reader.ReadArray<NAME_FRAG>();

            NameFragIndexMask = NameFragTable.Length - 1;

            field_214 = reader.ReadInt32();

            var dwBitMask = reader.ReadInt32();
        }

        private int sub_1959CB0(int dwItemIndex)
        {
            TRIPLET pTriplet;
            var dwKeyShifted = (dwItemIndex >> 9);
            int eax, ebx, ecx, edx, esi, edi;

            // If lower 9 is zero
            edx = dwItemIndex;
            if ((edx & 0x1FF) == 0)
                return Struct68_00.GetArrayValue_38(dwKeyShifted);

            eax = Struct68_00.GetArrayValue_38(dwKeyShifted) >> 9;
            esi = (Struct68_00.GetArrayValue_38(dwKeyShifted + 1) + 0x1FF) >> 9;
            dwItemIndex = esi;

            if ((eax + 0x0A) >= esi)
            {
                // HOTS: 1959CF7
                var i = eax + 1;
                pTriplet = Struct68_00.GetBaseValue(i);
                i++;
                edi = (eax << 0x09);
                ebx = edi - pTriplet.BaseValue + 0x200;
                while (edx >= ebx)
                {
                    // HOTS: 1959D14
                    edi += 0x200;
                    pTriplet = Struct68_00.GetBaseValue(i);

                    ebx = edi - pTriplet.BaseValue + 0x200;
                    eax++;
                    i++;
                }
            }
            else
            {
                // HOTS: 1959D2E
                while ((eax + 1) < esi)
                {
                    // HOTS: 1959D38
                    // ecx = Struct68_00.BaseValues.TripletArray;
                    esi = (esi + eax) >> 1;
                    ebx = (esi << 0x09) - Struct68_00.GetBaseValue(esi).BaseValue;
                    if (edx < ebx)
                    {
                        // HOTS: 01959D4B
                        dwItemIndex = esi;
                    }
                    else
                    {
                        // HOTS: 1959D50
                        eax = esi;
                        esi = dwItemIndex;
                    }
                }
            }

            // HOTS: 1959D5F
            pTriplet = Struct68_00.GetBaseValue(eax);
            edx += pTriplet.BaseValue - (eax << 0x09);
            edi = (eax << 4);

            eax = pTriplet.Value2;
            ecx = (eax >> 0x17);
            ebx = 0x100 - ecx;
            if (edx < ebx)
            {
                // HOTS: 1959D8C
                ecx = ((eax >> 0x07) & 0xFF);
                esi = 0x80 - ecx;
                if (edx < esi)
                {
                    // HOTS: 01959DA2
                    eax = eax & 0x7F;
                    ecx = 0x40 - eax;
                    if (edx >= ecx)
                    {
                        // HOTS: 01959DB7
                        edi += 2;
                        edx = edx + eax - 0x40;
                    }
                }
                else
                {
                    // HOTS: 1959DC0
                    eax = (eax >> 0x0F) & 0xFF;
                    esi = 0xC0 - eax;
                    if (edx < esi)
                    {
                        // HOTS: 1959DD3
                        edi += 4;
                        edx = edx + ecx - 0x80;
                    }
                    else
                    {
                        // HOTS: 1959DD3
                        edi += 6;
                        edx = edx + eax - 0xC0;
                    }
                }
            }
            else
            {
                // HOTS: 1959DE8
                esi = pTriplet.Value3;
                eax = ((esi >> 0x09) & 0x1FF);
                ebx = 0x180 - eax;
                if (edx < ebx)
                {
                    // HOTS: 01959E00
                    esi = esi & 0x1FF;
                    eax = (0x140 - esi);
                    if (edx < eax)
                    {
                        // HOTS: 1959E11
                        edi = edi + 8;
                        edx = edx + ecx - 0x100;
                    }
                    else
                    {
                        // HOTS: 1959E1D
                        edi = edi + 0x0A;
                        edx = edx + esi - 0x140;
                    }
                }
                else
                {
                    // HOTS: 1959E29
                    esi = (esi >> 0x12) & 0x1FF;
                    ecx = (0x1C0 - esi);
                    if (edx < ecx)
                    {
                        // HOTS: 1959E3D
                        edi = edi + 0x0C;
                        edx = edx + eax - 0x180;
                    }
                    else
                    {
                        // HOTS: 1959E49
                        edi = edi + 0x0E;
                        edx = edx + esi - 0x1C0;
                    }
                }
            }

            // HOTS: 1959E53:
            // Calculate the number of bits set in the value of "ecx"
            ecx = ~Struct68_00.GetItemBit(edi);
            eax = GetNumberOfSetBits(ecx);
            esi = eax >> 0x18;

            if (edx >= esi)
            {
                // HOTS: 1959ea4
                ecx = ~Struct68_00.GetItemBit(++edi);
                edx = edx - esi;
                eax = GetNumberOfSetBits(ecx);
            }

            // HOTS: 1959eea 
            // ESI gets the number of set bits in the lower 16 bits of ECX
            esi = (eax >> 0x08) & 0xFF;
            edi = edi << 0x05;
            if (edx < esi)
            {
                // HOTS: 1959EFC
                eax = eax & 0xFF;
                if (edx >= eax)
                {
                    // HOTS: 1959F05
                    ecx >>= 0x08;
                    edi += 0x08;
                    edx -= eax;
                }
            }
            else
            {
                // HOTS: 1959F0D
                eax = (eax >> 0x10) & 0xFF;
                if (edx < eax)
                {
                    // HOTS: 1959F19
                    ecx >>= 0x10;
                    edi += 0x10;
                    edx -= esi;
                }
                else
                {
                    // HOTS: 1959F23
                    ecx >>= 0x18;
                    edi += 0x18;
                    edx -= eax;
                }
            }

            // HOTS: 1959f2b
            edx = edx << 0x08;
            ecx = ecx & 0xFF;

            // BUGBUG: Possible buffer overflow here. Happens when dwItemIndex >= 0x9C.
            // The same happens in Heroes of the Storm (build 29049), so I am not sure
            // if this is a bug or a case that never happens
            Debug.Assert((ecx + edx) < table_1BA1818.Length);
            return table_1BA1818[ecx + edx] + edi;
        }

        private int sub_1959F50(int arg_0)
        {
            TRIPLET pTriplet;
            int eax, ebx, ecx, edx, esi, edi;

            edx = arg_0;
            eax = arg_0 >> 0x09;
            if ((arg_0 & 0x1FF) == 0)
                return Struct68_00.GetArrayValue_50(eax);

            var item0 = Struct68_00.GetArrayValue_50(eax);
            var item1 = Struct68_00.GetArrayValue_50(eax + 1);
            eax = (item0 >> 0x09);
            edi = (item1 + 0x1FF) >> 0x09;

            if ((eax + 0x0A) > edi)
            {
                // HOTS: 01959F94
                var i = eax + 1;
                pTriplet = Struct68_00.GetBaseValue(i);
                i++;
                while (edx >= pTriplet.BaseValue)
                {
                    // HOTS: 1959FA3
                    pTriplet = Struct68_00.GetBaseValue(i);
                    eax++;
                    i++;
                }
            }
            else
            {
                // Binary search
                // HOTS: 1959FAD
                if (eax + 1 < edi)
                {
                    // HOTS: 1959FB4
                    esi = (edi + eax) >> 1;
                    if (edx < Struct68_00.GetBaseValue(esi).BaseValue)
                    {
                        // HOTS: 1959FC4
                        edi = esi;
                    }
                    else
                    {
                        // HOTS: 1959FC8
                        eax = esi;
                    }
                }
            }

            // HOTS: 1959FD4
            pTriplet = Struct68_00.GetBaseValue(eax);
            edx = edx - pTriplet.BaseValue;
            edi = eax << 0x04;
            eax = pTriplet.Value2;
            ebx = (eax >> 0x17);
            if (edx < ebx)
            {
                // HOTS: 1959FF1
                esi = (eax >> 0x07) & 0xFF;
                if (edx < esi)
                {
                    // HOTS: 0195A000
                    eax = eax & 0x7F;
                    if (edx >= eax)
                    {
                        // HOTS: 195A007
                        edi = edi + 2;
                        edx = edx - eax;
                    }
                }
                else
                {
                    // HOTS: 195A00E
                    eax = (eax >> 0x0F) & 0xFF;
                    if (edx < eax)
                    {
                        // HOTS: 195A01A
                        edi += 4;
                        edx = edx - esi;
                    }
                    else
                    {
                        // HOTS: 195A01F
                        edi += 6;
                        edx = edx - eax;
                    }
                }
            }
            else
            {
                // HOTS: 195A026
                esi = pTriplet.Value3;
                eax = (pTriplet.Value3 >> 0x09) & 0x1FF;
                if (edx < eax)
                {
                    // HOTS: 195A037
                    esi = esi & 0x1FF;
                    if (edx < esi)
                    {
                        // HOTS: 195A041
                        edi = edi + 8;
                        edx = edx - ebx;
                    }
                    else
                    {
                        // HOTS: 195A048
                        edi = edi + 0x0A;
                        edx = edx - esi;
                    }
                }
                else
                {
                    // HOTS: 195A04D
                    esi = (esi >> 0x12) & 0x1FF;
                    if (edx < esi)
                    {
                        // HOTS: 195A05A
                        edi = edi + 0x0C;
                        edx = edx - eax;
                    }
                    else
                    {
                        // HOTS: 195A061
                        edi = edi + 0x0E;
                        edx = edx - esi;
                    }
                }
            }

            // HOTS: 195A066
            esi = Struct68_00.GetItemBit(edi);
            eax = GetNumberOfSetBits(esi);
            ecx = eax >> 0x18;

            if (edx >= ecx)
            {
                // HOTS: 195A0B2
                esi = Struct68_00.GetItemBit(++edi);
                edx = edx - ecx;
                eax = GetNumberOfSetBits(esi);
            }

            // HOTS: 195A0F6
            ecx = (eax >> 0x08) & 0xFF;

            edi = (edi << 0x05);
            if (edx < ecx)
            {
                // HOTS: 195A111
                eax = eax & 0xFF;
                if (edx >= eax)
                {
                    // HOTS: 195A111
                    edi = edi + 0x08;
                    esi = esi >> 0x08;
                    edx = edx - eax;
                }
            }
            else
            {
                // HOTS: 195A119
                eax = (eax >> 0x10) & 0xFF;
                if (edx < eax)
                {
                    // HOTS: 195A125
                    esi = esi >> 0x10;
                    edi = edi + 0x10;
                    edx = edx - ecx;
                }
                else
                {
                    // HOTS: 195A12F
                    esi = esi >> 0x18;
                    edi = edi + 0x18;
                    edx = edx - eax;
                }
            }

            esi = esi & 0xFF;
            edx = edx << 0x08;

            // BUGBUG: Potential buffer overflow
            // Happens in Heroes of the Storm when arg_0 == 0x5B
            Debug.Assert((esi + edx) < table_1BA1818.Length);
            return table_1BA1818[esi + edx] + edi;
        }

        bool CheckNextPathFragment(MNDXSearchResult pStruct1C)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;
            int CollisionIndex;
            int NameFragIndex;
            int SaveCharIndex;
            int HiBitsIndex;
            int FragOffs;

            // Calculate index of the next name fragment in the name fragment table
            NameFragIndex = ((pStruct40.ItemIndex << 0x05) ^ pStruct40.ItemIndex ^ pStruct1C.SearchMask[pStruct40.CharIndex]) & NameFragIndexMask;

            // Does the hash value match?
            if (NameFragTable[NameFragIndex].ItemIndex == pStruct40.ItemIndex)
            {
                // Check if there is single character match
                if (IsSingleCharMatch(NameFragTable, NameFragIndex))
                {
                    pStruct40.ItemIndex = NameFragTable[NameFragIndex].NextIndex;
                    pStruct40.CharIndex++;
                    return true;
                }

                // Check if there is a name fragment match
                if (NextDB != null)
                {
                    if (!NextDB.sub_1957B80(pStruct1C, NameFragTable[NameFragIndex].FragOffs))
                        return false;
                }
                else
                {
                    if (!IndexStruct_174.CheckNameFragment(pStruct1C, NameFragTable[NameFragIndex].FragOffs))
                        return false;
                }

                pStruct40.ItemIndex = NameFragTable[NameFragIndex].NextIndex;
                return true;
            }

            //
            // Conflict: Multiple hashes give the same table index
            //

            // HOTS: 1957A0E
            CollisionIndex = sub_1959CB0(pStruct40.ItemIndex) + 1;
            if (!Struct68_00.Contains(CollisionIndex))
                return false;

            pStruct40.ItemIndex = (CollisionIndex - pStruct40.ItemIndex - 1);
            HiBitsIndex = -1;

            // HOTS: 1957A41:
            do
            {
                // HOTS: 1957A41
                // Check if the low 8 bits if the fragment offset contain a single character
                // or an offset to a name fragment 
                if (Struct68_D0.Contains(pStruct40.ItemIndex))
                {
                    if (HiBitsIndex == -1)
                    {
                        // HOTS: 1957A6C
                        HiBitsIndex = Struct68_D0.GetItemValue(pStruct40.ItemIndex);
                    }
                    else
                    {
                        // HOTS: 1957A7F
                        HiBitsIndex++;
                    }

                    // HOTS: 1957A83
                    SaveCharIndex = pStruct40.CharIndex;

                    // Get the name fragment offset as combined value from lower 8 bits and upper bits
                    FragOffs = GetNameFragmentOffsetEx(pStruct40.ItemIndex, HiBitsIndex);

                    // Compare the string with the fragment name database
                    if (NextDB != null)
                    {
                        // HOTS: 1957AEC
                        if (NextDB.sub_1957B80(pStruct1C, FragOffs))
                            return true;
                    }
                    else
                    {
                        // HOTS: 1957AF7
                        if (IndexStruct_174.CheckNameFragment(pStruct1C, FragOffs))
                            return true;
                    }

                    // HOTS: 1957B0E
                    // If there was partial match with the fragment, end the search
                    if (pStruct40.CharIndex != SaveCharIndex)
                        return false;
                }
                else
                {
                    // HOTS: 1957B1C
                    if (FrgmDist_LoBits[pStruct40.ItemIndex] == pStruct1C.SearchMask[pStruct40.CharIndex])
                    {
                        pStruct40.CharIndex++;
                        return true;
                    }
                }

                // HOTS: 1957B32
                pStruct40.ItemIndex++;
                CollisionIndex++;
            }
            while (Struct68_00.Contains(CollisionIndex));
            return false;
        }

        private bool sub_1957B80(MNDXSearchResult pStruct1C, int dwKey)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;
            NAME_FRAG pNameEntry;
            int FragOffs;
            int eax, edi;

            edi = dwKey;

            // HOTS: 1957B95
            for (; ; )
            {
                pNameEntry = NameFragTable[(edi & NameFragIndexMask)];
                if (edi == pNameEntry.NextIndex)
                {
                    // HOTS: 01957BB4
                    if ((pNameEntry.FragOffs & 0xFFFFFF00) != 0xFFFFFF00)
                    {
                        // HOTS: 1957BC7
                        if (NextDB != null)
                        {
                            // HOTS: 1957BD3
                            if (!NextDB.sub_1957B80(pStruct1C, pNameEntry.FragOffs))
                                return false;
                        }
                        else
                        {
                            // HOTS: 1957BE0
                            if (!IndexStruct_174.CheckNameFragment(pStruct1C, pNameEntry.FragOffs))
                                return false;
                        }
                    }
                    else
                    {
                        // HOTS: 1957BEE
                        if (pStruct1C.SearchMask[pStruct40.CharIndex] != (byte)pNameEntry.FragOffs)
                            return false;
                        pStruct40.CharIndex++;
                    }

                    // HOTS: 1957C05
                    edi = pNameEntry.ItemIndex;
                    if (edi == 0)
                        return true;

                    if (pStruct40.CharIndex >= pStruct1C.SearchMask.Length)
                        return false;
                }
                else
                {
                    // HOTS: 1957C30
                    if (Struct68_D0.Contains(edi))
                    {
                        // HOTS: 1957C4C
                        if (NextDB != null)
                        {
                            // HOTS: 1957C58
                            FragOffs = GetNameFragmentOffset(edi);
                            if (!NextDB.sub_1957B80(pStruct1C, FragOffs))
                                return false;
                        }
                        else
                        {
                            // HOTS: 1957350
                            FragOffs = GetNameFragmentOffset(edi);
                            if (!IndexStruct_174.CheckNameFragment(pStruct1C, FragOffs))
                                return false;
                        }
                    }
                    else
                    {
                        // HOTS: 1957C8E
                        if (FrgmDist_LoBits[edi] != pStruct1C.SearchMask[pStruct40.CharIndex])
                            return false;

                        pStruct40.CharIndex++;
                    }

                    // HOTS: 1957CB2
                    if (edi <= field_214)
                        return true;

                    if (pStruct40.CharIndex >= pStruct1C.SearchMask.Length)
                        return false;

                    eax = sub_1959F50(edi);
                    edi = (eax - edi - 1);
                }
            }
        }

        private void sub_1958D70(MNDXSearchResult pStruct1C, int arg_4)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;
            NAME_FRAG pNameEntry;

            // HOTS: 1958D84
            for (; ; )
            {
                pNameEntry = NameFragTable[(arg_4 & NameFragIndexMask)];
                if (arg_4 == pNameEntry.NextIndex)
                {
                    // HOTS: 1958DA6
                    if ((pNameEntry.FragOffs & 0xFFFFFF00) != 0xFFFFFF00)
                    {
                        // HOTS: 1958DBA
                        if (NextDB != null)
                        {
                            NextDB.sub_1958D70(pStruct1C, pNameEntry.FragOffs);
                        }
                        else
                        {
                            IndexStruct_174.CopyNameFragment(pStruct1C, pNameEntry.FragOffs);
                        }
                    }
                    else
                    {
                        // HOTS: 1958DE7
                        // Insert the low 8 bits to the path being built
                        pStruct40.Add((byte)(pNameEntry.FragOffs & 0xFF));
                    }

                    // HOTS: 1958E71
                    arg_4 = pNameEntry.ItemIndex;
                    if (arg_4 == 0)
                        return;
                }
                else
                {
                    // HOTS: 1958E8E
                    if (Struct68_D0.Contains(arg_4))
                    {
                        int FragOffs;

                        // HOTS: 1958EAF
                        FragOffs = GetNameFragmentOffset(arg_4);
                        if (NextDB != null)
                        {
                            NextDB.sub_1958D70(pStruct1C, FragOffs);
                        }
                        else
                        {
                            IndexStruct_174.CopyNameFragment(pStruct1C, FragOffs);
                        }
                    }
                    else
                    {
                        // HOTS: 1958F50
                        // Insert one character to the path being built
                        pStruct40.Add(FrgmDist_LoBits[arg_4]);
                    }

                    // HOTS: 1958FDE
                    if (arg_4 <= field_214)
                        return;

                    arg_4 = -1 - arg_4 + sub_1959F50(arg_4);
                }
            }
        }

        private bool sub_1959010(MNDXSearchResult pStruct1C, int arg_4)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;
            NAME_FRAG pNameEntry;

            // HOTS: 1959024
            for (; ; )
            {
                pNameEntry = NameFragTable[(arg_4 & NameFragIndexMask)];
                if (arg_4 == pNameEntry.NextIndex)
                {
                    // HOTS: 1959047
                    if ((pNameEntry.FragOffs & 0xFFFFFF00) != 0xFFFFFF00)
                    {
                        // HOTS: 195905A
                        if (NextDB != null)
                        {
                            if (!NextDB.sub_1959010(pStruct1C, pNameEntry.FragOffs))
                                return false;
                        }
                        else
                        {
                            if (!IndexStruct_174.CheckAndCopyNameFragment(pStruct1C, pNameEntry.FragOffs))
                                return false;
                        }
                    }
                    else
                    {
                        // HOTS: 1959092
                        if ((byte)(pNameEntry.FragOffs & 0xFF) != pStruct1C.SearchMask[pStruct40.CharIndex])
                            return false;

                        // Insert the low 8 bits to the path being built
                        pStruct40.Add((byte)(pNameEntry.FragOffs & 0xFF));
                        pStruct40.CharIndex++;
                    }

                    // HOTS: 195912E
                    arg_4 = pNameEntry.ItemIndex;
                    if (arg_4 == 0)
                        return true;
                }
                else
                {
                    // HOTS: 1959147
                    if (Struct68_D0.Contains(arg_4))
                    {
                        int FragOffs;

                        // HOTS: 195917C
                        FragOffs = GetNameFragmentOffset(arg_4);
                        if (NextDB != null)
                        {
                            if (!NextDB.sub_1959010(pStruct1C, FragOffs))
                                return false;
                        }
                        else
                        {
                            if (!IndexStruct_174.CheckAndCopyNameFragment(pStruct1C, FragOffs))
                                return false;
                        }
                    }
                    else
                    {
                        // HOTS: 195920E
                        if (FrgmDist_LoBits[arg_4] != pStruct1C.SearchMask[pStruct40.CharIndex])
                            return false;

                        // Insert one character to the path being built
                        pStruct40.Add(FrgmDist_LoBits[arg_4]);
                        pStruct40.CharIndex++;
                    }

                    // HOTS: 19592B6
                    if (arg_4 <= field_214)
                        return true;

                    arg_4 = -1 - arg_4 + sub_1959F50(arg_4);
                }

                // HOTS: 19592D5
                if (pStruct40.CharIndex >= pStruct1C.SearchMask.Length)
                    break;
            }

            sub_1958D70(pStruct1C, arg_4);
            return true;
        }

        private bool EnumerateFiles(MNDXSearchResult pStruct1C)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;

            if (pStruct40.SearchPhase == CASCSearchPhase.Finished)
                return false;

            if (pStruct40.SearchPhase != CASCSearchPhase.Searching)
            {
                // HOTS: 1959489
                pStruct40.InitSearchBuffers();

                // If the caller passed a part of the search path, we need to find that one
                while (pStruct40.CharIndex < pStruct1C.SearchMask.Length)
                {
                    if (!sub_1958B00(pStruct1C))
                    {
                        pStruct40.Finish();
                        return false;
                    }
                }

                // HOTS: 19594b0
                PATH_STOP PathStop = new PATH_STOP()
                {
                    ItemIndex = pStruct40.ItemIndex,
                    Field_4 = 0,
                    Field_8 = pStruct40.NumBytesFound,
                    Field_C = -1,
                    Field_10 = -1
                };
                pStruct40.AddPathStop(PathStop);
                pStruct40.ItemCount = 1;

                if (FileNameIndexes.Contains(pStruct40.ItemIndex))
                {
                    pStruct1C.SetFindResult(pStruct40.Result, FileNameIndexes.GetItemValue(pStruct40.ItemIndex));
                    return true;
                }
            }

            // HOTS: 1959522
            for (; ; )
            {
                // HOTS: 1959530
                if (pStruct40.ItemCount == pStruct40.NumPathStops)
                {
                    PATH_STOP pLastStop;
                    int CollisionIndex;

                    pLastStop = pStruct40.GetPathStop(pStruct40.NumPathStops - 1);
                    CollisionIndex = sub_1959CB0(pLastStop.ItemIndex) + 1;

                    // Insert a new structure
                    PATH_STOP PathStop = new PATH_STOP()
                    {
                        ItemIndex = CollisionIndex - pLastStop.ItemIndex - 1,
                        Field_4 = CollisionIndex,
                        Field_8 = 0,
                        Field_C = -1,
                        Field_10 = -1
                    };
                    pStruct40.AddPathStop(PathStop);
                }

                // HOTS: 19595BD
                PATH_STOP pPathStop = pStruct40.GetPathStop(pStruct40.ItemCount);

                // HOTS: 19595CC
                if (Struct68_00.Contains(pPathStop.Field_4++))
                {
                    // HOTS: 19595F2
                    pStruct40.ItemCount++;

                    if (Struct68_D0.Contains(pPathStop.ItemIndex))
                    {
                        // HOTS: 1959617
                        if (pPathStop.Field_C == -1)
                            pPathStop.Field_C = Struct68_D0.GetItemValue(pPathStop.ItemIndex);
                        else
                            pPathStop.Field_C++;

                        // HOTS: 1959630
                        var FragOffs = GetNameFragmentOffsetEx(pPathStop.ItemIndex, pPathStop.Field_C);
                        if (NextDB != null)
                        {
                            // HOTS: 1959649
                            NextDB.sub_1958D70(pStruct1C, FragOffs);
                        }
                        else
                        {
                            // HOTS: 1959654
                            IndexStruct_174.CopyNameFragment(pStruct1C, FragOffs);
                        }
                    }
                    else
                    {
                        // HOTS: 1959665
                        // Insert one character to the path being built
                        pStruct40.Add(FrgmDist_LoBits[pPathStop.ItemIndex]);
                    }

                    // HOTS: 19596AE
                    pPathStop.Field_8 = pStruct40.NumBytesFound;

                    // HOTS: 19596b6
                    if (FileNameIndexes.Contains(pPathStop.ItemIndex))
                    {
                        // HOTS: 19596D1
                        if (pPathStop.Field_10 == -1)
                        {
                            // HOTS: 19596D9
                            pPathStop.Field_10 = FileNameIndexes.GetItemValue(pPathStop.ItemIndex);
                        }
                        else
                        {
                            pPathStop.Field_10++;
                        }

                        // HOTS: 1959755
                        pStruct1C.SetFindResult(pStruct40.Result, pPathStop.Field_10);
                        return true;
                    }
                }
                else
                {
                    // HOTS: 19596E9
                    if (pStruct40.ItemCount == 1)
                    {
                        pStruct40.Finish();
                        return false;
                    }

                    // HOTS: 19596F5
                    pPathStop = pStruct40.GetPathStop(pStruct40.ItemCount - 1);
                    pPathStop.ItemIndex++;

                    pPathStop = pStruct40.GetPathStop(pStruct40.ItemCount - 2);
                    var edi = pPathStop.Field_8;

                    // HOTS: 1959749
                    pStruct40.RemoveRange(edi);
                    pStruct40.ItemCount--;
                }
            }
        }

        private bool sub_1958B00(MNDXSearchResult pStruct1C)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;
            byte[] pbPathName = Encoding.ASCII.GetBytes(pStruct1C.SearchMask);
            int CollisionIndex;
            int FragmentOffset;
            int SaveCharIndex;
            int ItemIndex;
            int FragOffs;
            int var_4;

            ItemIndex = pbPathName[pStruct40.CharIndex] ^ (pStruct40.ItemIndex << 0x05) ^ pStruct40.ItemIndex;
            ItemIndex = ItemIndex & NameFragIndexMask;
            if (pStruct40.ItemIndex == NameFragTable[ItemIndex].ItemIndex)
            {
                // HOTS: 1958B45
                FragmentOffset = NameFragTable[ItemIndex].FragOffs;
                if ((FragmentOffset & 0xFFFFFF00) == 0xFFFFFF00)
                {
                    // HOTS: 1958B88
                    pStruct40.Add((byte)FragmentOffset);
                    pStruct40.ItemIndex = NameFragTable[ItemIndex].NextIndex;
                    pStruct40.CharIndex++;
                    return true;
                }

                // HOTS: 1958B59
                if (NextDB != null)
                {
                    if (!NextDB.sub_1959010(pStruct1C, FragmentOffset))
                        return false;
                }
                else
                {
                    if (!IndexStruct_174.CheckAndCopyNameFragment(pStruct1C, FragmentOffset))
                        return false;
                }

                // HOTS: 1958BCA
                pStruct40.ItemIndex = NameFragTable[ItemIndex].NextIndex;
                return true;
            }

            // HOTS: 1958BE5
            CollisionIndex = sub_1959CB0(pStruct40.ItemIndex) + 1;
            if (!Struct68_00.Contains(CollisionIndex))
                return false;

            pStruct40.ItemIndex = (CollisionIndex - pStruct40.ItemIndex - 1);
            var_4 = -1;

            // HOTS: 1958C20
            for (; ; )
            {
                if (Struct68_D0.Contains(pStruct40.ItemIndex))
                {
                    // HOTS: 1958C0E
                    if (var_4 == -1)
                    {
                        // HOTS: 1958C4B
                        var_4 = Struct68_D0.GetItemValue(pStruct40.ItemIndex);
                    }
                    else
                    {
                        var_4++;
                    }

                    // HOTS: 1958C62
                    SaveCharIndex = pStruct40.CharIndex;

                    FragOffs = GetNameFragmentOffsetEx(pStruct40.ItemIndex, var_4);
                    if (NextDB != null)
                    {
                        // HOTS: 1958CCB
                        if (NextDB.sub_1959010(pStruct1C, FragOffs))
                            return true;
                    }
                    else
                    {
                        // HOTS: 1958CD6
                        if (IndexStruct_174.CheckAndCopyNameFragment(pStruct1C, FragOffs))
                            return true;
                    }

                    // HOTS: 1958CED
                    if (SaveCharIndex != pStruct40.CharIndex)
                        return false;
                }
                else
                {
                    // HOTS: 1958CFB
                    if (FrgmDist_LoBits[pStruct40.ItemIndex] == pStruct1C.SearchMask[pStruct40.CharIndex])
                    {
                        // HOTS: 1958D11
                        pStruct40.Add(FrgmDist_LoBits[pStruct40.ItemIndex]);
                        pStruct40.CharIndex++;
                        return true;
                    }
                }

                // HOTS: 1958D11
                pStruct40.ItemIndex++;
                CollisionIndex++;

                if (!Struct68_00.Contains(CollisionIndex))
                    break;
            }

            return false;
        }

        public bool FindFileInDatabase(MNDXSearchResult pStruct1C)
        {
            SearchBuffer pStruct40 = pStruct1C.Buffer;

            pStruct40.ItemIndex = 0;
            pStruct40.CharIndex = 0;
            pStruct40.Init();

            if (pStruct1C.SearchMask.Length > 0)
            {
                while (pStruct40.CharIndex < pStruct1C.SearchMask.Length)
                {
                    // HOTS: 01957F12
                    if (!CheckNextPathFragment(pStruct1C))
                        return false;
                }
            }

            // HOTS: 1957F26
            if (!FileNameIndexes.Contains(pStruct40.ItemIndex))
                return false;

            pStruct1C.SetFindResult(pStruct1C.SearchMask, FileNameIndexes.GetItemValue(pStruct40.ItemIndex));
            return true;
        }

        public IEnumerable<MNDXSearchResult> EnumerateFiles()
        {
            MNDXSearchResult pStruct1C = new MNDXSearchResult();

            while (EnumerateFiles(pStruct1C))
                yield return pStruct1C;
        }

        private int GetNameFragmentOffsetEx(int LoBitsIndex, int HiBitsIndex)
        {
            return (FrgmDist_HiBits[HiBitsIndex] << 0x08) | FrgmDist_LoBits[LoBitsIndex];
        }

        private int GetNameFragmentOffset(int LoBitsIndex)
        {
            return GetNameFragmentOffsetEx(LoBitsIndex, Struct68_D0.GetItemValue(LoBitsIndex));
        }

        private bool IsSingleCharMatch(NAME_FRAG[] Table, int ItemIndex)
        {
            return ((Table[ItemIndex].FragOffs & 0xFFFFFF00) == 0xFFFFFF00);
        }

        private int GetNumberOfSetBits(int Value32)
        {
            Value32 = ((Value32 >> 1) & 0x55555555) + (Value32 & 0x55555555);
            Value32 = ((Value32 >> 2) & 0x33333333) + (Value32 & 0x33333333);
            Value32 = ((Value32 >> 4) & 0x0F0F0F0F) + (Value32 & 0x0F0F0F0F);

            return (Value32 * 0x01010101);
        }
    }
}
