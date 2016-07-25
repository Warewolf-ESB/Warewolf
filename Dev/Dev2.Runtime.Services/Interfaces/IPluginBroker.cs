using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Interfaces
{
    public interface IPluginBroker<in T, in TS> where T : IResourceSource
        where TS : Service
    {
        NamespaceList GetNamespaces(T pluginSource);
        ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName);
        IOutputDescription TestPlugin(TS pluginService);
    }
}