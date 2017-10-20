/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using System.Collections.Generic;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    public class PluginBroker : IPluginBroker<PluginSource, PluginService>
    {
        #region Implementation of IPluginBroker<in PluginSource>

        public List<NamespaceItem> GetNamespaces(PluginSource pluginSource)
        {
            try
            {
                return PluginServiceExecutionFactory.GetNamespaces(pluginSource);
            }
            
            catch (BadImageFormatException)
            {
                throw;
            }
        }

        public List<NamespaceItem> GetNamespacesWithJsonObjects(PluginSource pluginSource)
        {
            try
            {
                return PluginServiceExecutionFactory.GetNamespacesWithJsonObjects(pluginSource);
            }
            
            catch (BadImageFormatException)
            {
                throw;
            }
        }

        public ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName)
        {
            return PluginServiceExecutionFactory.GetMethods(assemblyLocation, assemblyName, fullName);
        }

        public ServiceMethodList GetMethodsWithReturns(string assemblyLocation, string assemblyName, string fullName)
        {
            return PluginServiceExecutionFactory.GetMethodsWithReturns(assemblyLocation, assemblyName, fullName);
        }

        public ServiceConstructorList GetConstructors(string assemblyLocation, string assemblyName, string fullName)
        {
            return PluginServiceExecutionFactory.GetConstructors(assemblyLocation, assemblyName, fullName); 
        }

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

            var pluginResult = PluginServiceExecutionFactory.TestPlugin(args, out string serializedResult);
            pluginService.SerializedResult = serializedResult;
            return pluginResult;
        }

        #endregion
    }
}
