
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dev2.Studio.Core.Configuration
{
    public interface IRuntimeConfigurationAssemblyRepository
    {
        IEnumerable<string> AllHashes();
        Assembly Load(string hash);
        void Add(string hash, byte[] assemblyData);
        UserControl GetUserControlForAssembly(string hash);
        Dictionary<string, UserControl> UserControlCache { get; }
    }
}
