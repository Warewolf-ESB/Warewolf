/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;
using Warewolf.Execution;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Execution
{
    public interface IWebRequest
    {
        string Method { get; set; }
        string ContentType { get; set; }
        long ContentLength { get; set; }
        bool UseDefaultCredentials { get; set; }
        WebHeaderCollection Headers { get; set; }
        ICredentials Credentials { get; set; }
        Uri RequestUri { get; }
        Stream GetRequestStream();
        WebResponse GetResponse();
        Task<WebResponse> GetResponseAsync();
    }
    public interface IWebRequestFactory
    {
        IWebRequest New(string escapeUriString);
    }

    public class WebRequestWrapper : IWebRequest
    {
        private WebRequest _request;
        public WebRequestWrapper(string escapeUriString)
        {
            _request = WebRequest.Create(escapeUriString);
        }
        public string Method {
            get => _request.Method;
            set => _request.Method = value;
        }
        public string ContentType {
            get => _request.ContentType;
            set => _request.ContentType = value;
        }
        public long ContentLength {
            get => _request.ContentLength;
            set => _request.ContentLength = value;
        }
        public bool UseDefaultCredentials {
            get => _request.UseDefaultCredentials;
            set => _request.UseDefaultCredentials = value;
        }
        public WebHeaderCollection Headers {
            get => _request.Headers;
            set => _request.Headers = value;
        }

        public ICredentials Credentials
        {
            get => _request.Credentials;
            set => _request.Credentials = value;
        }

        public Uri RequestUri => _request.RequestUri;

        public Stream GetRequestStream()
        {
            return _request.GetRequestStream();
        }

        public WebResponse GetResponse()
        {
            return _request.GetResponse();
        }

        public Task<WebResponse> GetResponseAsync()
        {
            return _request.GetResponseAsync();
        }
    }
    public class WebRequestFactory : IWebRequestFactory
    {
        public IWebRequest New(string escapeUriString)
        {
            return new WebRequestWrapper(escapeUriString);
        }
    }

    /// <summary>
    /// Execute a remote workflow ;)
    /// </summary>
    public class RemoteWorkflowExecutionContainer : EsbExecutionContainer
    {
        readonly IResourceCatalog _resourceCatalog;
        private IWebRequestFactory _webRequestFactory;

        /// <summary>
        /// Need to add loc property to AbstractActivity ;)
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="dataObj"></param>
        /// <param name="workspace"></param>
        /// <param name="esbChannel"></param>
        public RemoteWorkflowExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel)
            : this(sa, dataObj, workspace, esbChannel, ResourceCatalog.Instance, new WebRequestFactory())
        {
        }

        protected RemoteWorkflowExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel, IResourceCatalog resourceCatalog, IWebRequestFactory webRequestFactory)
            : base(sa, dataObj, workspace, esbChannel)
        {
            _resourceCatalog = resourceCatalog ?? throw new ArgumentNullException(nameof(resourceCatalog));
            _webRequestFactory = webRequestFactory;
        }

        public void PerformLogExecution(string logUri, int update)
        {

            var expressionsEntry = DataObject.Environment.Eval(logUri, update);
            var itr = new WarewolfIterator(expressionsEntry);
            while (itr.HasMoreData())
            {
                var val = itr.GetNextValue();
                {
                    var buildGetWebRequest = BuildSimpleGetWebRequest(val);
                    if (buildGetWebRequest == null)
                    {
                        throw new Exception(ErrorResource.InvalidUrl);
                    }
                    buildGetWebRequest.UseDefaultCredentials = true;
                    ExecuteWebRequestAsync(buildGetWebRequest);
                }
            }
        }

        protected virtual void ExecuteWebRequestAsync(IWebRequest buildGetWebRequest)
        {
            _ = buildGetWebRequest?.GetResponseAsync();
        }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
#pragma warning disable CC0021
            Dev2Logger.Info($"Starting Remote Execution. Service Name:{DataObject.ServiceName} Resource Id:{DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}", GlobalConstants.WarewolfInfo);
#pragma warning restore CC0021

            var serviceName = DataObject.ServiceName;

            errors = new ErrorResultTO();

            Dev2Logger.Debug("Creating DataList fragment for remote execute", GlobalConstants.WarewolfDebug);
            var dataListFragment = ExecutionEnvironmentUtils.GetXmlInputFromEnvironment(DataObject, DataObject.RemoteInvokeResultShape.ToString(), update);

            var result = string.Empty;

            var connection = GetConnection(DataObject.EnvironmentID);
            if (connection == null)
            {
                errors.AddError(ErrorResource.ServiceNotFound);
                return DataObject.DataListID;
            }

            try
            {
                result = ExecutePostRequest(connection, serviceName, dataListFragment);
                var msg = DataObject.IsDebug ? FetchRemoteDebugItems(connection) : new List<IDebugState>();
                DataObject.RemoteDebugItems = msg; // set them so they can be acted upon
            }
            catch (Exception e)
            {
                var errorMessage = e.Message.Contains("Forbidden") ? "Executing a service requires Execute permissions" : e.Message;
                DataObject.Environment.Errors.Add(errorMessage);
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }

            // Create tmpDL
            ExecutionEnvironmentUtils.UpdateEnvironmentFromOutputPayload(DataObject, result.ToStringBuilder(), DataObject.RemoteInvokeResultShape.ToString());
#pragma warning disable CC0021
            Dev2Logger.Info($"Completed Remote Execution. Service Name:{DataObject.ServiceName} Resource Id:{DataObject.ResourceID} Mode:{(DataObject.IsDebug ? "Debug" : "Execute")}", GlobalConstants.WarewolfInfo);
#pragma warning restore CC0021
            return Guid.Empty;
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext) => true;
        string ExecutePostRequest(Connection connection, string serviceName, string payload, bool isDebugMode = true)
        {
            var result = string.Empty;

            var serviceToExecute = GetServiceToExecute(connection, serviceName);

            var req = BuildPostRequest(serviceToExecute, payload, connection.AuthenticationType, connection.UserName, connection.Password, isDebugMode);
            Dev2Logger.Debug("Executing the remote request.", GlobalConstants.WarewolfDebug);
            if (req != null)
            {
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
            }

            return result;
        }

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;

        protected virtual IList<IDebugState> FetchRemoteDebugItems(Connection connection)
        {
            var data = ExecutePostRequest(connection, "FetchRemoteDebugMessagesService", "InvokerID=" + DataObject.RemoteInvokerID);

            if (data != null)
            {
                var fetchRemoteDebugItems = RemoteDebugItemParser.ParseItems(data);
                fetchRemoteDebugItems.ForEach(state => state.SessionID = DataObject.DebugSessionID);
                return fetchRemoteDebugItems;
            }

            return null;
        }

        internal bool ServerIsUp()
        {
            var connection = GetConnection(DataObject.EnvironmentID);
            if (connection == null)
            {
                return false;
            }
            try
            {
                var returnData = ExecuteGetRequest(connection, "ping", "<DataList></DataList>", false);
                if (!string.IsNullOrEmpty(returnData) && returnData.Contains("Pong"))
                {
                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        protected virtual string ExecuteGetRequest(Connection connection, string serviceName, string payload) => ExecuteGetRequest(connection, serviceName, payload, true);

        protected virtual string ExecuteGetRequest(Connection connection, string serviceName, string payload, bool isDebugMode)
        {
            var result = string.Empty;

            var serviceToExecute = GetServiceToExecute(connection, serviceName);
            var requestUri = serviceToExecute + "?" + payload;
            var req = BuildGetWebRequest(requestUri, connection.AuthenticationType, connection.UserName, connection.Password, isDebugMode)
                      ?? BuildPostRequest(serviceToExecute, payload, connection.AuthenticationType, connection.UserName, connection.Password, isDebugMode);

            Dev2Logger.Debug("Executing the remote request.", GlobalConstants.WarewolfDebug);
            if (req != null)
            {
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
            }

            return result;
        }

        static string GetServiceToExecute(Connection connection, string serviceName) => connection.WebAddress + "Secure/" + serviceName + ".json";

        IWebRequest BuildPostRequest(string serviceToExecute, string payload, AuthenticationType authenticationType, string userName, string password, bool isDebug)
        {
            var escapeUriString = Uri.EscapeUriString(serviceToExecute);
            var req = _webRequestFactory.New(escapeUriString);
            req.Method = "POST";
            UpdateRequest(authenticationType, userName, password, isDebug, req);

            var data = Encoding.ASCII.GetBytes(payload);

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.Headers.Add("Warewolf-Execution-Id", DataObject.ExecutionID.ToString());

            using (Stream requestStream = req.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }
            return req;
        }

        void UpdateRequest(AuthenticationType authenticationType, string userName, string password, bool isDebug, IWebRequest req)
        {
            if (authenticationType == AuthenticationType.Windows)
            {
                req.UseDefaultCredentials = true;
            }
            else
            {
                req.UseDefaultCredentials = false;

                // we to default to the hidden public user name of \, silly know but that is how to get around ntlm auth ;)
                if (authenticationType == AuthenticationType.Public)
                {
                    userName = GlobalConstants.PublicUsername;
                    password = string.Empty;
                }

                req.Credentials = new NetworkCredential(userName, password);
            }
            var remoteInvokerId = DataObject.RemoteInvokerID;
            if (remoteInvokerId == Guid.Empty.ToString())
            {
                throw new Exception(ErrorResource.RemoteServerIDNull);
            }
            req.Headers.Add(HttpRequestHeader.From, remoteInvokerId); // Set to remote invoke ID ;)
            req.Headers.Add(HttpRequestHeader.Cookie, isDebug ? GlobalConstants.RemoteServerInvoke : GlobalConstants.RemoteDebugServerInvoke);
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        IWebRequest BuildGetWebRequest(string requestUri, AuthenticationType authenticationType, string userName, string password, bool isdebug)
        {
            try
            {
                var req = BuildSimpleGetWebRequest(requestUri);
                UpdateRequest(authenticationType, userName, password, isdebug, req);
                return req;
            }
            catch (Exception)
            {
                return null;
            }
        }

        IWebRequest BuildSimpleGetWebRequest(string requestUri)
        {
            try
            {
                var escapeUriString = Uri.EscapeUriString(requestUri);
                var req = _webRequestFactory.New(escapeUriString);
                req.Method = "GET";
                return req;
            }
            catch (Exception)
            {
                return null;
            }
        }

        Connection GetConnection(Guid environmentId)
        {
            if (environmentId == Guid.Empty)
            {
                var localhostConnection = new Connection
                {
                    Address = EnvironmentVariables.WebServerUri,
                    AuthenticationType = AuthenticationType.Windows
                };
                return localhostConnection;
            }
            var xml = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, environmentId);

            if (xml == null || xml.Length == 0)
            {
                return null;
            }

            var xe = xml.ToXElement();
            return new Connection(xe);
        }

        public override SerializableResource FetchRemoteResource(Guid serviceId, string serviceName, bool isDebugMode)
        {
            var connection = GetConnection(DataObject.EnvironmentID);
            if (connection == null)
            {
                return null;
            }
            try
            {
                var returnData = ExecuteGetRequest(connection, "FindResourceService", $"ResourceType=TypeWorkflowService&ResourceName={serviceName}&ResourceId={serviceId}", isDebugMode);
                if (!string.IsNullOrEmpty(returnData))
                {
                    var serializer = new Dev2JsonSerializer();
                    var serializableResources = serializer.Deserialize<IList<SerializableResource>>(returnData);
                    return serializableResources.FirstOrDefault(resource => resource.ResourceType == "WorkflowService");
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
    }
}
