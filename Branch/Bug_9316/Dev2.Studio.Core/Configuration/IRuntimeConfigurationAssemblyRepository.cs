
using System.Collections.Generic;
using System.Reflection;

namespace Dev2.Studio.Core.Configuration
{
    public interface IRuntimeConfigurationAssemblyRepository
    {
        IEnumerable<string> AllHashes();
        Assembly Load(string hash);
        void Add(string hash, byte[] assemblyData);
    }
}
