using System;
using System.Reflection;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    public class AssemblyWrapper : IAssemblyWrapper
    {
        public Assembly Load(string assemblyString) => Assembly.Load(assemblyString);

        public Assembly LoadFrom(string assemblyString) => Assembly.LoadFrom(assemblyString);

        public Assembly Load(AssemblyName toLoad) => Assembly.Load(toLoad);

        public Assembly UnsafeLoadFrom(string assemblyLocation) => Assembly.UnsafeLoadFrom(assemblyLocation);

        public Assembly GetAssembly(Type getType) => Assembly.GetAssembly(getType);

        public AssemblyName[] GetReferencedAssemblies(Assembly asm) => asm.GetReferencedAssemblies();
    }
}
