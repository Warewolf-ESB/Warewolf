using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dev2
{
    interface IAssemblyLoader
    {
        AssemblyName[] assemblyNames(Assembly asm);
        Assembly LoadAndReturn(AssemblyName toLoad);
    }
    public class AssemblyLoader : IAssemblyLoader
    {
        public AssemblyName[] assemblyNames(Assembly asm)
        {
            return asm.GetReferencedAssemblies();
        }

        public Assembly LoadAndReturn(AssemblyName toLoad)
        {
            return AppDomain.CurrentDomain.Load(toLoad);
        }
    }
}
