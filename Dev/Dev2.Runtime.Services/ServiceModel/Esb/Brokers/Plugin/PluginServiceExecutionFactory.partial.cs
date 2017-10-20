using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;
using System.Collections.Generic;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    /// <summary>
    /// Used to execute plugins properly ;)
    /// INFO : http://stackoverflow.com/questions/2008691/pass-and-execute-delegate-in-separate-appdomain
    /// </summary>
    public static partial class PluginServiceExecutionFactory
    {
        #region Private Methods

        private static Isolated<PluginRuntimeHandler> CreateInvokeAppDomain()
        {
            Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>();
            return isolated;
        }

        #endregion

        #region Public Interface

        public static IOutputDescription TestPlugin(PluginInvokeArgs args, out string serializedResult)
        {

            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.Test(args, out serializedResult);
            }

        }

        public static object InvokePlugin(PluginInvokeArgs args)
        {

            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.Run(args);
            }

        }
        
        public static ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName)
        {
            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.ListMethods(assemblyLocation, assemblyName, fullName);
            }
        }

        public static List<NamespaceItem> GetNamespaces(PluginSource pluginSource)
        {
            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.FetchNamespaceListObject(pluginSource);
            }
        }

        #endregion
    }
}