using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;
using enActionType = Dev2.DataList.Contract.enActionType;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute a remote workflow ;)
    /// </summary>
    public class RemoteWorkflowExecutionContainer : EsbExecutionContainer
    {
        readonly IResourceCatalog _resourceCatalog;

        /// <summary>
        /// Need to add loc property to AbstractActivity ;)
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="dataObj"></param>
        /// <param name="workspace"></param>
        /// <param name="esbChannel"></param>
        public RemoteWorkflowExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel)
            : this(sa, dataObj, workspace, esbChannel, ResourceCatalog.Instance)
        {
        }

        public RemoteWorkflowExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, workspace, esbChannel)
        {
            if(resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        public void PerformLogExecution(string logUri)
        {
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var expressionsEntry = dataListCompiler.Evaluate(DataObject.DataListID, enActionType.User, logUri, false, out errors);
            var itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
            while(itr.HasMoreRecords())
            {
                var cols = itr.FetchNextRowData();
                foreach(var c in cols)
                {
                    var buildGetWebRequest = BuildGetWebRequest(c.TheValue);
                    ExecuteWebRequestAsync(buildGetWebRequest);
                }
            }
        }

        protected virtual void ExecuteWebRequestAsync(WebRequest buildGetWebRequest)
        {
            buildGetWebRequest.GetResponseAsync();
        }

        public override Guid Execute(out ErrorResultTO errors)
        {

            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            var serviceName = DataObject.ServiceName;

            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            // get data in a format we can send ;)
            var dataListFragment = dataListCompiler.ConvertFrom(DataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            string result = string.Empty;

            var connection = GetConnection(DataObject.EnvironmentID);
            if(connection == null)
            {
                errors.AddError("Server source not found.");
                return DataObject.DataListID;
            }

            try
            {
                // Invoke Remote WF Here ;)
                var remoteInvokeUri = connection.WebAddress;
                result = ExecuteGetRequest(remoteInvokeUri, serviceName, dataListFragment);
                IList<DebugState> msg = FetchRemoteDebugItems(remoteInvokeUri);
                DataObject.RemoteDebugItems = msg; // set them so they can be acted upon
            }
            catch(Exception e)
            {
                ServerLogger.LogError(e);
                errors.AddError(e.Message);
            }

            // Create tmpDL
            var tmpID = dataListCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), result, DataObject.RemoteInvokeResultShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Merge Result into Local DL ;)
            Guid mergeOp = dataListCompiler.Merge(DataObject.DataListID, tmpID, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // clean up ;)
            dataListCompiler.ForceDeleteDataListByID(tmpID);

            if(mergeOp == DataObject.DataListID)
            {
                return mergeOp;
            }

            return Guid.Empty;
        }

        protected virtual IList<DebugState> FetchRemoteDebugItems(string uri)
        {
            var data = ExecuteGetRequest(uri, "FetchRemoteDebugMessagesService", "InvokerID=" + DataObject.RemoteInvokerID);
            // Dev2System.ManagmentServicePayload

            if(data != null)
            {
                IList<DebugState> fetchRemoteDebugItems = RemoteDebugItemParser.ParseItems(data);
                fetchRemoteDebugItems.ForEach(state => state.SessionID=DataObject.DebugSessionID);
                return fetchRemoteDebugItems;
            }

            return null;
        }

        protected virtual string ExecuteGetRequest(string uri, string serviceName, string payload)
        {
            string result = string.Empty;

            var myURI = uri + "Services/" + serviceName + "?" + payload;
            var req = BuildGetWebRequest(myURI);

            // TODO : Start background worker to fetch messages ;)
            // FetchRemoteDebugMessagesService

            using(var response = req.GetResponse() as HttpWebResponse)
            {
                if(response != null)
                {
                    using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }

            return result;
        }

        WebRequest BuildGetWebRequest(string myURI)
        {
            WebRequest req = HttpWebRequest.Create(myURI);
            req.Method = "GET";

            // set header for server to know this is a remote invoke ;)
            if(DataObject.RemoteInvokerID == Guid.Empty.ToString())
            {
                throw new Exception("Remote Server ID Empty");
            }
            req.Headers.Add(HttpRequestHeader.From, DataObject.RemoteInvokerID); // Set to remote invoke ID ;)
            req.Headers.Add(HttpRequestHeader.Cookie, GlobalConstants.RemoteServerInvoke);
            return req;
        }

        Connection GetConnection(Guid environmentID)
        {
            var xml = _resourceCatalog.GetResourceContents(DataObject.WorkspaceID, environmentID);
            return string.IsNullOrEmpty(xml) ? null : new Connection(XElement.Parse(xml));
        }
    }
}
