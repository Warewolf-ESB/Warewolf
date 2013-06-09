using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using Dev2.Common;
using System.Xml;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute a remote workflow ;)
    /// </summary>
    public class RemoteWorkflowExecutionContainer : EsbExecutionContainer
    {

        /// <summary>
        /// Need to add loc property to AbstractActivity ;)
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="dataObj"></param>
        /// <param name="workspace"></param>
        /// <param name="esbChannel"></param>
        public RemoteWorkflowExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel)
            : base(sa, dataObj, workspace, esbChannel)
        {
        }

        public override Guid Execute(out ErrorResultTO errors)
        {
            
            // TODO : Add URI to Activity and DataObject - Populated from Studio on drop
            // TODO : Get service action to respect RemoteWorkflow enActionType!
            // TODO : Use test utils to invoke ?!

            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            var serviceName = DataObject.ServiceName;

            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;

            // get data in a format we can send ;)
            var dataListFragment = dataListCompiler.ConvertFrom(DataObject.DataListID,DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            string result = string.Empty;

            try
            {
                // Invoke Remote WF Here ;)
                result = ExecuteGetRequest(DataObject.RemoteInvokeUri, serviceName, dataListFragment);
                IList<DebugState> msg = FetchRemoteDebugItems();
                DataObject.RemoteDebugItems = msg; // set them so they can be acted upon
            }
            catch (Exception e)
            {
                ServerLogger.LogError(e);
            }

            // Create tmpDL
            var tmpID = dataListCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), result, DataObject.RemoteInvokeResultShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Merge Result into Local DL ;)
            Guid mergeOp = dataListCompiler.Merge(DataObject.DataListID, tmpID, enDataListMergeTypes.Union, enTranslationDepth.Data_With_Blank_OverWrite, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // clean up ;)
            dataListCompiler.ForceDeleteDataListByID(tmpID);

            if (mergeOp == DataObject.DataListID)
            {
                return mergeOp;
            }
            
            throw new Exception("Failed to invoke remote workflow");
            
        }

        /// <summary>
        /// Fetches the remote debug items.
        /// </summary>
        /// <returns></returns>
        private IList<DebugState> FetchRemoteDebugItems()
        {
            var data = ExecuteGetRequest(DataObject.RemoteInvokeUri, "FetchRemoteDebugMessagesService", "InvokerID=" + DataObject.RemoteInvokerID);
            // Dev2System.ManagmentServicePayload

            if (data != null)
            {
                return RemoteDebugItemParser.ParseItems(data);
            }

            return null;
        }

        public string ExecuteGetRequest(string uri, string serviceName, string payload)
        {
            string result = string.Empty;

            var myURI = uri + "Services/" + serviceName + "?" + payload;
            WebRequest req = HttpWebRequest.Create(myURI);
            req.Method = "GET";

            // set header for server to know this is a remote invoke ;)
            if (DataObject.RemoteInvokerID == Guid.Empty.ToString())
            {
                throw new Exception("Remote Server ID Empty");
            }
            req.Headers.Add(HttpRequestHeader.From, DataObject.RemoteInvokerID); // Set to remote invoke ID ;)
            req.Headers.Add(HttpRequestHeader.Cookie, GlobalConstants.RemoteServerInvoke);

            // TODO : Start background worker to fetch messages ;)
            // FetchRemoteDebugMessagesService

            using (var response = req.GetResponse() as HttpWebResponse)
            {
                if (response != null)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }

            return result;
        }
    }
}
