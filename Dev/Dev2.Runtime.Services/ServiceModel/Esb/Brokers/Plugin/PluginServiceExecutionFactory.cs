/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin
{
    /// <summary>
    /// Used to execute plugins properly ;)
    /// INFO : http://stackoverflow.com/questions/2008691/pass-and-execute-delegate-in-separate-appdomain
    /// </summary>
    public static partial class PluginServiceExecutionFactory
    {
        #region Private Methods


        #endregion

        #region Public Interface

        public static IDev2MethodInfo InvokePlugin(Isolated<PluginRuntimeHandler> appDomain, PluginExecutionDto dto,IDev2MethodInfo dev2MethodInfo,out string objString)
        {
            string objectString;
            var invokePlugin = appDomain.Value.Run(dev2MethodInfo,dto,out objectString);
            objString = objectString;
            return invokePlugin;
        }

        public static PluginExecutionDto ExecuteConstructor(Isolated<PluginRuntimeHandler> appDomain,PluginExecutionDto dto)
        {
            return appDomain.Value.ExecuteConstructor(dto);
        }

        public static Isolated<PluginRuntimeHandler> CreateAppDomain()
        {
            return CreateInvokeAppDomain();
            
        }


        /// <summary>
        /// Gets the Constructors.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public static ServiceConstructorList GetConstructors(string assemblyLocation, string assemblyName, string fullName)
        {
            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.ListConstructors(assemblyLocation, assemblyName, fullName);
            }
        }

        #endregion

        /// <summary>
        /// Gets the methods.
        /// </summary>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public static ServiceMethodList GetMethodsWithReturns(string assemblyLocation, string assemblyName, string fullName)
        {
            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.ListMethodsWithReturns(assemblyLocation, assemblyName, fullName);
            }
        }

        public static NamespaceList GetNamespacesWithJsonObjects(PluginSource pluginSource)
        {
            using (var runtime = CreateInvokeAppDomain())
            {
                return runtime.Value.FetchNamespaceListObjectWithJsonObjects(pluginSource);
            }
        }
        
    }
}
