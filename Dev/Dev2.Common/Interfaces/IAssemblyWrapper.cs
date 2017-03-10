using System.Reflection;

namespace Dev2.Common.Interfaces
{
    public interface IAssemblyWrapper
    {
        Assembly Load( string assemblyString);
    }
}