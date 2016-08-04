﻿using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace GACManagerApi.Fusion
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ASSEMBLY_INFO
    {
        public int cbAssemblyInfo; // size of this structure for future expansion
        public int assemblyFlags;
        public long assemblySizeInKB;
        [MarshalAs(UnmanagedType.LPWStr)]
        public String currentAssemblyPath;
        public int cchBuf; // size of path buf.
    }
}
