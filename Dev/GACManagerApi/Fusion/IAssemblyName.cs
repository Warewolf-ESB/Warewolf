using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GACManagerApi.Fusion
{
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
    public interface IAssemblyName
    {
        [PreserveSig()]
        int SetProperty(
            ASM_NAME PropertyId,
            IntPtr pvProperty,
            int cbProperty);

        [PreserveSig()]
        int GetProperty(ASM_NAME PropertyId, IntPtr pvProperty, ref uint pcbProperty);

        [PreserveSig()]
        
#pragma warning disable 465
        int Finalize();
#pragma warning restore 465

        [PreserveSig()]
        int GetDisplayName(
            StringBuilder pDisplayName,
            ref int pccDisplayName,
            ASM_DISPLAY_FLAGS displayFlags);

        [PreserveSig()]
        int Reserved(ref Guid guid,
                     Object obj1,
                     Object obj2,
                     String string1,
                     Int64 llFlags,
                     IntPtr pvReserved,
                     int cbReserved,
                     out IntPtr ppv);

        [PreserveSig()]
        int GetName(
            ref int pccBuffer,
            StringBuilder pwzName);

        [PreserveSig()]
        int GetVersion(
            out int versionHi,
            out int versionLow);

        [PreserveSig()]
        int IsEqual(
            IAssemblyName pAsmName,
            int cmpFlags);

        [PreserveSig()]
        int Clone(out IAssemblyName pAsmName);
    }
}