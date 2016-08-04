using System;
using System.Runtime.InteropServices;

namespace GACManagerApi.Fusion
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
    public interface IAssemblyCache
    {
        [PreserveSig()]
        int QueryAssemblyInfo(
            int flags,
            [MarshalAs(UnmanagedType.LPWStr)] String assemblyName,
            ref ASSEMBLY_INFO assemblyInfo);
    }
}