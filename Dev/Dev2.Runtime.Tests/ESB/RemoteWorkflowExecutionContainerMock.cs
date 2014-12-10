
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
using System.Net;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class RemoteWorkflowExecutionContainerMock : RemoteWorkflowExecutionContainer
    {
        public RemoteWorkflowExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, theWorkspace, esbChannel, resourceCatalog)
        {
        }

        public string GetRequestRespsonse { get; set; }
        public string GetRequestUri { get; set; }
        public string FetchRemoteDebugItemsUri { get; set; }
        public string LogExecutionUrl { get; private set; }
        public WebRequest LogExecutionWebRequest { get; private set; }

        protected override string ExecuteGetRequest(Connection connection, string serviceName, string payload)
        {
            GetRequestUri = connection.Address;
            return GetRequestRespsonse;
        }

        #region Overrides of RemoteWorkflowExecutionContainer

        protected override void ExecuteWebRequestAsync(WebRequest buildGetWebRequest)
        {
            LogExecutionUrl = buildGetWebRequest.RequestUri.ToString();
            LogExecutionWebRequest = buildGetWebRequest;
        }

        #endregion

        protected override IList<IDebugState> FetchRemoteDebugItems(Connection connection)
        {
            FetchRemoteDebugItemsUri = connection.Address;
            return new List<IDebugState>();
        }

    }
}
