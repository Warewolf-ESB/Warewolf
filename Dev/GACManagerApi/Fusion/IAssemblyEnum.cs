using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GACManagerApi.Fusion
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
    public interface IAssemblyEnum
    {
        [PreserveSig()]
        int GetNextAssembly(
            IntPtr pvReserved,
            out IAssemblyName ppName,
            int flags);

        [PreserveSig()]
        int Reset();

        [PreserveSig()]
        int Clone(out IAssemblyEnum ppEnum);
    }
}