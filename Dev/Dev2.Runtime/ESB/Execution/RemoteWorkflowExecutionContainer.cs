using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;

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
            var connection = GetConnection(DataObject.EnvironmentID);
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var expressionsEntry = dataListCompiler.Evaluate(DataObject.DataListID, enActionType.User, logUri, false, out errors);
            var itr = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionsEntry);
            while(itr.HasMoreRecords())
            {
                var cols = itr.FetchNextRowData();
                foreach(var c in cols)
                {
                    var buildGetWebRequest = BuildGetWebRequest(c.TheValue, connection.AuthenticationType, connection.UserName, connection.Password);
                    if(buildGetWebRequest == null)
                    {
                        throw new Exception("Invalid Url to execute for logging");
                    }
                    ExecuteWebRequestAsync(buildGetWebRequest);
                }
            }
        }

        protected virtual void ExecuteWebRequestAsync(WebRequest buildGetWebRequest)
        {
            if(buildGetWebRequest == null)
            {
                return;
            }
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
                result = ExecuteGetRequest(connection, serviceName, dataListFragment);
                IList<IDebugState> msg = FetchRemoteDebugItems(connection);
                DataObject.RemoteDebugItems = msg; // set them so they can be acted upon
            }
            catch(Exception e)
            {
                this.LogError(e);
                errors.AddError(e.Message);
            }

            // Create tmpDL
            var tmpId = dataListCompiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), result, DataObject.RemoteInvokeResultShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Merge Result into Local DL ;)
            Guid mergeOp = dataListCompiler.Merge(DataObject.DataListID, tmpId, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            if(mergeOp == DataObject.DataListID)
            {
                return mergeOp;
            }

            return Guid.Empty;
        }

        protected virtual IList<IDebugState> FetchRemoteDebugItems(Connection connection)
        {
            var data = ExecuteGetRequest(connection, "FetchRemoteDebugMessagesService", "InvokerID=" + DataObject.RemoteInvokerID);

            if(data != null)
            {
                IList<IDebugState> fetchRemoteDebugItems = RemoteDebugItemParser.ParseItems(data);
                fetchRemoteDebugItems.ForEach(state => state.SessionID = DataObject.DebugSessionID);
                return fetchRemoteDebugItems;
            }

            return null;
        }

        public virtual bool ServerIsUp()
        {
            var connection = GetConnection(DataObject.EnvironmentID);
            if(connection == null)
            {
                return false;
            }
            try
            {
                var returnData = ExecuteGetRequest(connection, "ping", "<DataList></DataList>");
                if(!string.IsNullOrEmpty(returnData))
                {
                    if(returnData.Contains("Pong"))
                    {
                        return true;
                    }
                }
            }
            catch(Exception)
            {
                return false;
            }
            return false;
        }

        protected virtual string ExecuteGetRequest(Connection connection, string serviceName, string payload)
        {
            var result = string.Empty;

            var requestUri = connection.WebAddress + "Services/" + serviceName + "?" + payload;
            var req = BuildGetWebRequest(requestUri, connection.AuthenticationType, connection.UserName, connection.Password);

            using(var response = req.GetResponse() as HttpWebResponse)
            {
                if(response != null)
                {
                    // ReSharper disable AssignNullToNotNullAttribute
                    using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                    // ReSharper restore AssignNullToNotNullAttribute
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }

            return result;
        }

        WebRequest BuildGetWebRequest(string requestUri, AuthenticationType authenticationType, string userName, string password)
        {
            try
            {
                var req = WebRequest.Create(requestUri);
                if(authenticationType == AuthenticationType.Windows)
                {
                    req.UseDefaultCredentials = true;
                }
                else
                {
                    req.UseDefaultCredentials = false;

                    // we to default to the hidden public user name of \, silly know but that is how to get around ntlm auth ;)
                    if(authenticationType == AuthenticationType.Public)
                    {
                        userName = GlobalConstants.PublicUsername;
                        password = string.Empty;
                    }

                    req.Credentials = new NetworkCredential(userName, password);
                }
                req.Method = "GET";

                // set header for server to know this is a remote invoke ;)
                var remoteInvokerId = DataObject.RemoteInvokerID;
                if(remoteInvokerId == Guid.Empty.ToString())
                {
                    throw new Exception("Remote Server ID Empty");
                }
                req.Headers.Add(HttpRequestHeader.From, remoteInvokerId); // Set to remote invoke ID ;)
                req.Headers.Add(HttpRequestHeader.Cookie, GlobalConstants.RemoteServerInvoke);
                return req;
            }
            catch(Exception)
            {
                return null;
            }
        }

        Connection GetConnection(Guid environmentId)
        {
            if(environmentId == Guid.Empty)
            {
                var localhostConnection = new Connection
                    {
                        Address = EnvironmentVariables.WebServerUri,
                        AuthenticationType = AuthenticationType.Windows
                    };
                return localhostConnection;
            }
            var xml = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, environmentId);

            if(xml == null || xml.Length == 0)
            {
                return null;
            }

            var xe = xml.ToXElement();
            return new Connection(xe);
        }

        public SerializableResource FetchRemoteResource(string serviceName)
        {
            var connection = GetConnection(DataObject.EnvironmentID);
            if(connection == null)
            {
                return null;
            }
            try
            {
                var returnData = ExecuteGetRequest(connection, "FindResourceService", string.Format("ResourceType={0}&ResourceName={1}", "TypeWorkflowService", serviceName));
                if(!string.IsNullOrEmpty(returnData))
                {
                    Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                    var serializableResources = serializer.Deserialize<IList<SerializableResource>>(returnData);
                    return serializableResources.FirstOrDefault(resource => resource.ResourceType == ResourceType.WorkflowService);
                }
            }
            catch(Exception)
            {
                return null;
            }
            return null;
        }
    }
}
