using System;
using System.Reflection;

namespace Dev2.Common.Interfaces
{
    public interface IAssemblyWrapper
    {
        Assembly Load( string assemblyString);
        Assembly LoadFrom(string assemblyString);
        Assembly Load(AssemblyName toLoad);
        Assembly UnsafeLoadFrom(string assemblyLocation);
        Assembly GetAssembly(Type getType);
        AssemblyName[] GetReferencedAssemblies(Assembly asm);
    }
}