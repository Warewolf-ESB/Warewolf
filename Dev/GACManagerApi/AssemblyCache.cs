using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GACManagerApi.Fusion;

namespace GACManagerApi
{
    /// <summary>
    /// AssemblyMustBeStronglyNamedException.
    /// </summary>
    public class AssemblyMustBeStronglyNamedException : Exception
    {

    }

    /// <summary>
    /// The AssemblyCache class is a managed wrapper around the Fusion IAssemblyCache COM interface.
    /// </summary>
    [ComVisible(false)]
    public static class AssemblyCache
    {
        public static void InstallAssembly(String assemblyPath, FUSION_INSTALL_REFERENCE reference, AssemblyCommitFlags flags)
        {
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidInstallGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid reference guid.", "guid");
            }

            IAssemblyCache ac = null;

            int hr = 0;

            hr = FusionImports.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.InstallAssembly((int)flags, assemblyPath, reference);
            }

            if (hr < 0)
            {
                if (hr == -2146234300 /*0x80131044*/)
                    throw new AssemblyMustBeStronglyNamedException();
                else
                    Marshal.ThrowExceptionForHR(hr);
            }
        }

        // assemblyName has to be fully specified name. 
        // A.k.a, for v1.0/v1.1 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx".
        // For v2.0 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx, ProcessorArchitecture=xx".
        // If assemblyName is not fully specified, a random matching assembly will be uninstalled. 
        public static void UninstallAssembly(String assemblyName, FUSION_INSTALL_REFERENCE reference,
                                             out IASSEMBLYCACHE_UNINSTALL_DISPOSITION disp)
        {
            if (reference != null)
            {
                if (!InstallReferenceGuid.IsValidUninstallGuidScheme(reference.GuidScheme))
                    throw new ArgumentException("Invalid reference guid.", "guid");
            }

            //  Create an assembly cache objet.
            IAssemblyCache ac = null;
            int hr = FusionImports.CreateAssemblyCache(out ac, 0);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);

            //  Uninstall the assembly.
            hr = ac.UninstallAssembly(0, assemblyName, reference, out disp);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
        }

        // See comments in UninstallAssembly
        public static String QueryAssemblyInfo(String assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentException("Invalid name", "assemblyName");
            }

            var aInfo = new ASSEMBLY_INFO();

            aInfo.cchBuf = 1024;
            // Get a string with the desired length
            aInfo.currentAssemblyPath = new String('\0', aInfo.cchBuf);

            IAssemblyCache ac = null;
            int hr = FusionImports.CreateAssemblyCache(out ac, 0);
            if (hr >= 0)
            {
                hr = ac.QueryAssemblyInfo(0, assemblyName, ref aInfo);
            }
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return aInfo.currentAssemblyPath;
        }
    }
}
