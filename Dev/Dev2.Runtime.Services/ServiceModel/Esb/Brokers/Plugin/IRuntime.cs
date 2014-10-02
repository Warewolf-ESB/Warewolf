
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

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
