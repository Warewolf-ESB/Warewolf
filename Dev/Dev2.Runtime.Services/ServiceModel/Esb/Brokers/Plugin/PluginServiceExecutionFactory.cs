/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    /// <summary>
    /// Used to execute plugins properly ;)
    /// INFO : http://stackoverflow.com/questions/2008691/pass-and-execute-delegate-in-separate-appdomain
    /// </summary>
    public static class PluginServiceExecutionFactory
    {
        #region Private Methods

        private static Isolated<PluginRuntimeHandler> CreateInvokeAppDomain()
        {
            Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>();
            return isolated;
        }

        #endregion

        #region Public Interface

        public static IOutputDescription TestPlugin(PluginInvokeArgs args,out string serializedResult)
        {

            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.Test(args,out serializedResult);
            }

        }

        public static object InvokePlugin(PluginInvokeArgs args)
        {

            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.Run(args);
            }
           
        }

        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public static ServiceMethodList GetMethods(string assemblyLocation, string assemblyName, string fullName)
        {
            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.ListMethods(assemblyLocation, assemblyName, fullName);
            }           
        }

        public static NamespaceList GetNamespaces(PluginSource pluginSource)
        {
            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.FetchNamespaceListObject(pluginSource);
            }            
        }

        #endregion

    }
}
