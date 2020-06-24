#pragma warning disable
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
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class InternalServiceRequestHandler : AbstractWebRequestHandler
    {
        readonly IResourceCatalog _catalog;
        readonly IAuthorizationService _authorizationService;
        IDSFDataObject DsfDataObject { get; set; }
        public IPrincipal ExecutingUser { private get; set; }

        public InternalServiceRequestHandler()
            : this(ResourceCatalog.Instance, ServerAuthorizationService.Instance)
        {
        }

        public InternalServiceRequestHandler(IResourceCatalog catalog, IAuthorizationService authorizationService)
            : base(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance, new DefaultEsbChannelFactory(), new SecuritySettings())
        {
            _catalog = catalog;
            _authorizationService = authorizationService;
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var serviceName = ctx.GetServiceName();
            var instanceId = ctx.GetInstanceID();
            var bookmark = ctx.GetBookmark();
            ctx.GetDataListID();
            var workspaceId = ctx.GetWorkspaceID();
            var formData = new WebRequestTO();

            var xml = SubmittedData.GetPostData(ctx);

            if (!string.IsNullOrEmpty(xml))
            {
                formData.RawRequestPayload = xml;
            }

            formData.ServiceName = serviceName;
            formData.InstanceID = instanceId;
            formData.Bookmark = bookmark;
            formData.WebServerUrl = ctx.Request.Uri.ToString();
            formData.Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}";

            if (ExecutingUser == null)
            {
                throw new Exception(ErrorResource.NullExecutingUser);
            }

            try
            {
                // Execute in its own thread to give proper context
                var t = new Thread(() =>
                {
                    Thread.CurrentPrincipal = ExecutingUser;

                    var responseWriter = CreateForm(formData, serviceName, workspaceId, ctx.FetchHeaders(), ctx.Request.User);
                    ctx.Send(responseWriter);
                });

                t.Start();
                t.Join();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(this, e, GlobalConstants.WarewolfError);
            }
        }

        static string BuildStudioUrl(string payLoad)
        {
            try
            {
                var xElement = XDocument.Parse(payLoad);
                xElement.Descendants().Where(e => e.Name == "BDSDebugMode" || e.Name == "DebugSessionID" || e.Name == "EnvironmentID").Remove();
                var s = xElement.ToString(SaveOptions.DisableFormatting);
                var buildStudioUrl = s.Replace(Environment.NewLine, string.Empty).Replace(" ", "%20");
                return buildStudioUrl;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, "BuildStudioUrl(string payLoad)");
                return string.Empty;
            }
        }

        public StringBuilder ProcessRequest(EsbExecuteRequest request, Guid workspaceId, Guid dataListId, string connectionId)
        {
            var channel = new EsbServicesEndpoint();

            var isManagementResource = ProcessDsfDataObject(request, workspaceId, dataListId, connectionId, channel);
            if (!DsfDataObject.Environment.HasErrors())
            {
                return ProcessRequest(request, workspaceId, channel, DsfDataObject, isManagementResource);
            }

            var msg = new ExecuteMessage { HasError = true };
            msg.SetMessage(string.Join(Environment.NewLine, DsfDataObject.Environment.Errors));

            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(msg);
        }

        private static (string xmlData, string queryString) FormatXmlData(EsbExecuteRequest request)
        {
            var xmlData = string.Empty;
            var queryString = "";
            if (request.Args != null && request.Args.ContainsKey("DebugPayload"))
            {
                xmlData = request.Args["DebugPayload"].ToString();
                queryString = BuildStudioUrl(xmlData);
                xmlData = xmlData.Replace("<DataList>", "<XmlData>").Replace("</DataList>", "</XmlData>");
            }

            if (string.IsNullOrEmpty(xmlData))
            {
                xmlData = "<DataList></DataList>";
            }
            return (xmlData, queryString);
        }

        private bool ProcessDsfDataObject(EsbExecuteRequest request, Guid workspaceId, Guid dataListId, string connectionId, EsbServicesEndpoint channel)
        {
            var (xmlData, queryString) = FormatXmlData(request);

            DsfDataObject = new DsfDataObject(xmlData, dataListId);
            if (!DsfDataObject.ExecutionID.HasValue)
            {
                DsfDataObject.ExecutionID = Guid.NewGuid();
            }
            DsfDataObject.QueryString = queryString;

            if (IsDebugRequest(request))
            {
                DsfDataObject.IsDebug = true;
            }
            DsfDataObject.StartTime = DateTime.Now;
            DsfDataObject.EsbChannel = channel;
            DsfDataObject.ServiceName = request.ServiceName;
            DsfDataObject.Settings = new Dev2WorkflowSettingsTO
            {
                EnableDetailedLogging = true,
                LoggerType = LoggerType.JSON,
                KeepLogsForDays = 2,
                CompressOldLogFiles = true
            };

            var resource = request.ResourceID != Guid.Empty ? _catalog.GetResource(workspaceId, request.ResourceID) : _catalog.GetResource(workspaceId, request.ServiceName);
            var isManagementResource = false;
            if (!string.IsNullOrEmpty(request.TestName))
            {
                DsfDataObject.TestName = request.TestName;
                DsfDataObject.IsServiceTestExecution = true;
            }
            if (resource != null)
            {
                DsfDataObject.ResourceID = resource.ResourceID;
                DsfDataObject.SourceResourceID = resource.ResourceID;
                isManagementResource = _catalog.ManagementServices.ContainsKey(resource.ResourceID);
            }
            else
            {
                if (request.ResourceID != Guid.Empty)
                {
                    DsfDataObject.ResourceID = request.ResourceID;
                }
            }

            DsfDataObject.ClientID = Guid.Parse(connectionId);
            Common.Utilities.OrginalExecutingUser = ExecutingUser;
            DsfDataObject.ExecutingUser = ExecutingUser;
            return isManagementResource;
        }

        private static bool IsDebugRequest(EsbExecuteRequest request)
        {
            var isDebug = false;
            if (request.Args != null && request.Args.ContainsKey("IsDebug"))
            {
                var debugString = request.Args["IsDebug"].ToString();
                if (!bool.TryParse(debugString, out isDebug))
                {
                    isDebug = false;
                }
            }

            return isDebug;
        }

        private StringBuilder ProcessRequest(EsbExecuteRequest request, Guid workspaceId, EsbServicesEndpoint channel, IDSFDataObject dataObject, bool isManagementResource)
        {
            if (ExecutingUser == null)
            {
                throw new Exception(ErrorResource.NullExecutingUser);
            }
            try
            {
                // Execute in its own thread to give proper context
                var t = new Thread(() =>
                {
                    try
                    {
                        Thread.CurrentPrincipal = ExecutingUser;
                        if (isManagementResource)
                        {
                            Thread.CurrentPrincipal = Common.Utilities.ServerUser;
                            ExecutingUser = Common.Utilities.ServerUser;
                            dataObject.ExecutingUser = Common.Utilities.ServerUser;
                        }
                        else
                        {
                            IsAuthorizedForServiceTestRun(dataObject);
                        }

                        channel.ExecuteRequest(dataObject, request, workspaceId, out var errors);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                    }
                });

                t.Start();
                t.Join();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
            }

            if (request.ExecuteResult.Length > 0)
            {
                return request.ExecuteResult;
            }

            return new StringBuilder();
        }

        void IsAuthorizedForServiceTestRun(IDSFDataObject dataObject)
        {
            var isAuthorized = dataObject.IsServiceTestExecution;
            isAuthorized &= _authorizationService != null;
            if (isAuthorized)
            {
                var authorizationService = _authorizationService;
                var hasContribute = authorizationService.IsAuthorized(AuthorizationContext.Contribute, Guid.Empty);
                if (!hasContribute)
                {
                    throw new UnauthorizedAccessException("The user does not have permission to execute tests.");
                }
            }
        }
    }
}
