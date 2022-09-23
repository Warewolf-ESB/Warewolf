using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GACManagerApi.Fusion
{
    public static class FusionImports
    {
        internal const string FusionDll = "fusion.dll";

        [DllImport(FusionDll)]
        public static extern int CreateAssemblyEnum(
            out IAssemblyEnum ppEnum,
            IntPtr pUnkReserved,
            IAssemblyName pName,
            ASM_CACHE_FLAGS flags,
            IntPtr pvReserved);

        [DllImport(FusionDll)]
        public static extern int CreateAssemblyNameObject(
            out IAssemblyName ppAssemblyNameObj,
            [MarshalAs(UnmanagedType.LPWStr)] String szAssemblyName,
            CREATE_ASM_NAME_OBJ_FLAGS flags,
            IntPtr pvReserved);

        [DllImport(FusionDll)]
        public static extern int CreateAssemblyCache(
            out IAssemblyCache ppAsmCache,
            int reserved);

        [DllImport(FusionDll)]
        public static extern int CreateInstallReferenceEnum(
            out IInstallReferenceEnum ppRefEnum,
            IAssemblyName pName,
            int dwFlags,
            IntPtr pvReserved);
    }
}
