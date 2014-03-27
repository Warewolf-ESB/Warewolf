using System.Collections.Generic;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    /// <summary>
    /// Interface for Plugin Invoke
    /// </summary>
    public interface IRuntime
    {
        object Run(PluginInvokeArgs setupInfo);

        IOutputDescription Test(PluginInvokeArgs setupInfo);

        IEnumerable<string> ListNamespaces(string assemblyLocation, string assemblyName);

        ServiceMethodList ListMethods(string assemblyLocation, string assemblyName, string fullName);

        string ValidatePlugin(string toLoad);

        NamespaceList FetchNamespaceListObject(PluginSource pluginSource);
    }
}
