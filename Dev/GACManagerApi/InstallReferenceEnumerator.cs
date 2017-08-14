using System;
using System.Runtime.InteropServices;
using GACManagerApi.Fusion;




namespace GACManagerApi
{
    public class InstallReferenceEnumerator
    {
        public InstallReferenceEnumerator(String assemblyName)
        {
            IAssemblyName fusionName;

            int hr = FusionImports.CreateAssemblyNameObject(
                out fusionName,
                assemblyName,
                CREATE_ASM_NAME_OBJ_FLAGS.CANOF_PARSE_DISPLAY_NAME,
                IntPtr.Zero);

            if (hr >= 0)
            {
                hr = FusionImports.CreateInstallReferenceEnum(out refEnum, fusionName, 0, IntPtr.Zero);
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public InstallReferenceEnumerator(IAssemblyName assemblyName)
        {

            var hr = FusionImports.CreateInstallReferenceEnum(out refEnum, assemblyName, 0, IntPtr.Zero);

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        public FUSION_INSTALL_REFERENCE GetNextReference()
        {
            IInstallReferenceItem item;
            int hr = refEnum.GetNextInstallReferenceItem(out item, 0, IntPtr.Zero);
            if ((uint)hr == 0x80070103)
            {
                // ERROR_NO_MORE_ITEMS
                return null;
            }

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            IntPtr refData;
            FUSION_INSTALL_REFERENCE instRef = new FUSION_INSTALL_REFERENCE(Guid.Empty, String.Empty, String.Empty);

            hr = item.GetReference(out refData, 0, IntPtr.Zero);
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            Marshal.PtrToStructure(refData, instRef);
            return instRef;
        }

        private IInstallReferenceEnum refEnum;
    }
}
