/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;
using System.Collections.Generic;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin
{
    public static class ComPluginServiceExecutionFactory
    {
        #region Private Methods

        private static ComPluginRuntimeHandler CreateInvokeAppDomain()
        {
            ComPluginRuntimeHandler isolated = new ComPluginRuntimeHandler();
            return isolated;
        }

        #endregion

        #region Public Interface

        public static IOutputDescription TestComPlugin(ComPluginInvokeArgs args,out string serializedResult)
        {
            var runtime = CreateInvokeAppDomain();
            return runtime.Test(args, out serializedResult);
        }

        public static object InvokeComPlugin(ComPluginInvokeArgs args)
        {
            var runtime = CreateInvokeAppDomain();
            return runtime.Run(args);
        }

        public static ServiceMethodList GetMethods(string classId,bool is32Bit)
        {
            var runtime = CreateInvokeAppDomain();
            return runtime.ListMethods(classId, is32Bit);
        }

        public static List<NamespaceItem> GetNamespaces(ComPluginSource pluginSource)
        {
            var runtime = CreateInvokeAppDomain();
            return runtime.FetchNamespaceListObject(pluginSource);
        }

        #endregion

    }
}
