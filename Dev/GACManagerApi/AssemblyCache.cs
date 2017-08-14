using System;
using System.Runtime.InteropServices;
using GACManagerApi.Fusion;


namespace GACManagerApi
{
    /// <summary>
    /// The AssemblyCache class is a managed wrapper around the Fusion IAssemblyCache COM interface.
    /// </summary>
    [ComVisible(false)]
    public static class AssemblyCache
    {
        // assemblyName has to be fully specified name. 
        // A.k.a, for v1.0/v1.1 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx".
        // For v2.0 assemblies, it should be "name, Version=xx, Culture=xx, PublicKeyToken=xx, ProcessorArchitecture=xx".
        // If assemblyName is not fully specified, a random matching assembly will be uninstalled. 

        // See comments in UninstallAssembly
        public static String QueryAssemblyInfo(String assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentException("Invalid name", nameof(assemblyName));
            }

            var aInfo = new ASSEMBLY_INFO { cchBuf = 1024 };

            // Get a string with the desired length
            aInfo.currentAssemblyPath = new String('\0', aInfo.cchBuf);

            IAssemblyCache ac;
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
