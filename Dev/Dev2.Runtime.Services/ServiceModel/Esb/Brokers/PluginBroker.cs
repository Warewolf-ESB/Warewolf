using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    public interface IPluginBroker
    {
        NamespaceList GetNamespaces(PluginSource pluginSource);
        ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName);
        IOutputDescription TestPlugin(PluginService pluginService);
        bool ValidatePlugin(string toLoad, out string error);
    }

    /// <summary>
    /// Handle interaction with plugins ;)
    /// </summary>
    public class PluginBroker : IPluginBroker
    {
        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        /// <param name="pluginSource">The plugin source.</param>
        /// <returns></returns>
        public NamespaceList GetNamespaces(PluginSource pluginSource)
        {
            return PluginServiceExecutionFactory.GetNamespaces(pluginSource);
        }

        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName)
        {
            return PluginServiceExecutionFactory.GetMethods(assemblyLocation, assemblyName, fullName);
        }

        public bool ValidatePlugin(string toLoad, out string error)
        {
            error = PluginServiceExecutionFactory.ValidatePlugin(toLoad);

            return (error == string.Empty);
        }

        /// <summary>
        /// Tests the plugin.
        /// </summary>
        /// <param name="pluginService">The plugin service.</param>
        /// <returns></returns>
        public IOutputDescription TestPlugin(PluginService pluginService)
        {
            PluginInvokeArgs args = new PluginInvokeArgs
                                    {
                                        AssemblyLocation = ((PluginSource)pluginService.Source).AssemblyLocation,
                                        AssemblyName = ((PluginSource)pluginService.Source).AssemblyName,
                                        Method = pluginService.Method.Name,
                                        Fullname = pluginService.Namespace,
                                        Parameters = pluginService.Method.Parameters
                                    };

            var pluginResult = PluginServiceExecutionFactory.TestPlugin(args);

            return pluginResult;
        }
    }
}