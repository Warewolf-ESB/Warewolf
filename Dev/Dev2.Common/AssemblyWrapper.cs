using System;
using System.Reflection;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    public class AssemblyWrapper : IAssemblyWrapper
    {
        public Assembly Load(string assemblyString)
        {
            return Assembly.Load(assemblyString);
        }

        public Assembly LoadFrom(string assemblyString)
        {
            return Assembly.LoadFrom(assemblyString);
        }

        public Assembly Load(AssemblyName toLoad)
        {
            return Assembly.Load(toLoad);
        }

        public Assembly UnsafeLoadFrom(string assemblyLocation)
        {
            return Assembly.UnsafeLoadFrom(assemblyLocation);
        }

        public Assembly GetAssembly(Type getType)
        {
            return Assembly.GetAssembly(getType);
        }

        public AssemblyName[] GetReferencedAssemblies(Assembly asm)
        {
            return asm.GetReferencedAssemblies();
        }
    }
}
