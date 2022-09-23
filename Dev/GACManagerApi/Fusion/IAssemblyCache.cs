using System;
using System.Runtime.InteropServices;

namespace GACManagerApi.Fusion
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
    public interface IAssemblyCache
    {
        [PreserveSig()]
        int UninstallAssembly(
            int flags,
            [MarshalAs(UnmanagedType.LPWStr)] String assemblyName,
            FUSION_INSTALL_REFERENCE refData,
            out IASSEMBLYCACHE_UNINSTALL_DISPOSITION disposition);

        [PreserveSig()]
        int QueryAssemblyInfo(
            int flags,
            [MarshalAs(UnmanagedType.LPWStr)] String assemblyName,
            ref ASSEMBLY_INFO assemblyInfo);

        [PreserveSig()]
        int Reserved(
            int flags,
            IntPtr pvReserved,
            out Object ppAsmItem,
            [MarshalAs(UnmanagedType.LPWStr)] String assemblyName);

        [PreserveSig()]
        int Reserved(out Object ppAsmScavenger);

        [PreserveSig()]
        int InstallAssembly(
            int flags,
            [MarshalAs(UnmanagedType.LPWStr)] String assemblyFilePath,
            FUSION_INSTALL_REFERENCE refData);
    }
}