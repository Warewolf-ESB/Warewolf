using System;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin
{
    /// <summary>
    /// Handle interaction with plugins ;)
    /// </summary>
    public class ComPluginBroker : ICOMPluginBroker<ComPluginSource, ComPluginService>
    {
        #region Implementation of IPluginBroker<in ComPluginSource,in ComPluginService>

        public NamespaceList GetNamespaces(ComPluginSource pluginSource)
        {
            try
            {
                return ComPluginServiceExecutionFactory.GetNamespaces(pluginSource);
            }
                
            catch (BadImageFormatException)
            {
                throw;
            }
        }

        public ServiceMethodList GetMethods(string clsId, bool is32Bit)
        {
            return ComPluginServiceExecutionFactory.GetMethods(clsId,is32Bit);
        }

        public IOutputDescription TestPlugin(ComPluginService pluginService)
        {
            ComPluginInvokeArgs args = new ComPluginInvokeArgs
            {
                Is32Bit = ((ComPluginSource)pluginService.Source).Is32Bit,
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