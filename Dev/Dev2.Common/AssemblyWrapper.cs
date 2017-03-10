using System.Reflection;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    public class AssemblyWrapper : IAssemblyWrapper
    {
        public Assembly Load( string assemblyString)
        {
            return Assembly.Load(assemblyString);
        }
    }
}
