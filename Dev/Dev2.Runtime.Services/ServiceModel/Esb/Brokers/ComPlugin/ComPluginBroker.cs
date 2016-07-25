using System;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin
{
    /// <summary>
    /// Handle interaction with plugins ;)
    /// </summary>
    public class ComPluginBroker : IPluginBroker<ComPluginSource, ComPluginService>
    {
        #region Implementation of IPluginBroker<in ComPluginSource,in ComPluginService>

        public NamespaceList GetNamespaces(ComPluginSource pluginSource)
        {
            try
            {
                return ComPluginServiceExecutionFactory.GetNamespaces(pluginSource);
            }
                // ReSharper disable once RedundantCatchClause
            catch (BadImageFormatException)
            {
                throw;
            }
        }

        public ServiceMethodList GetMethods(string clsId, string assemblyName, string fullName)
        {
            return ComPluginServiceExecutionFactory.GetMethods(clsId);
        }

        public IOutputDescription TestPlugin(ComPluginService pluginService)
        {
            ComPluginInvokeArgs args = new ComPluginInvokeArgs
            {
                ProgId = ((ComPluginSource)pluginService.Source).ProgId,
                ClsId = ((ComPluginSource)pluginService.Source).ClsId,
                Method = pluginService.Method.Name,
                Fullname = pluginService.Namespace,
                Parameters = pluginService.Method.Parameters
            };

            string serializedResult;
            var pluginResult = ComPluginServiceExecutionFactory.TestComPlugin(args, out serializedResult);
            pluginService.SerializedResult = serializedResult;
            return pluginResult;
        }

        #endregion
    }
}