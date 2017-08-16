using System;
using System.Runtime.InteropServices;
using GACManagerApi.Fusion;


namespace GACManagerApi
{
    /// <summary>
    /// The AssemblyCacheEnumerator is an object that can be used to enumerate all assemblies in the GAC.
    /// </summary>
    [ComVisible(false)]
    public class AssemblyCacheEnumerator
    {
        public AssemblyCacheEnumerator()
        {
            Initialise(null);
        }

        public AssemblyCacheEnumerator(string assemblyName)
        {
            Initialise(assemblyName);
        }

        private void Initialise(string assemblyName)
        {
            IAssemblyName fusionName = null;
            int hr;

            //  If we have an assembly name, create the assembly name object.
            if (assemblyName != null)
            {
                hr = FusionImports.CreateAssemblyNameObject(out fusionName, assemblyName,
                    CREATE_ASM_NAME_OBJ_FLAGS.CANOF_PARSE_DISPLAY_NAME, IntPtr.Zero);

                //  Check the result.
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);
            }

            //  Create the assembly enumerator.
            hr = FusionImports.CreateAssemblyEnum(out assemblyEnumerator, IntPtr.Zero,
                fusionName, ASM_CACHE_FLAGS.ASM_CACHE_GAC, IntPtr.Zero);
            
            //  Check the result.
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Gets the next assembly.
        /// </summary>
        /// <returns>The next assembly, or null of all assemblies have been enumerated.</returns>
        public IAssemblyName GetNextAssembly()
        {
            int hr;
            IAssemblyName fusionName;

            if (done)
            {
                return null;
            }

            // Now get next IAssemblyName from m_AssemblyEnum
            hr = assemblyEnumerator.GetNextAssembly((IntPtr)0, out fusionName, 0);

            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            //  If we haven't got a fusion object, we're done.
            if (fusionName == null)
            {
                done = true;
                return null;
            }

            return fusionName;
        }

        private IAssemblyEnum assemblyEnumerator = null;
        private bool done;
    }
}
