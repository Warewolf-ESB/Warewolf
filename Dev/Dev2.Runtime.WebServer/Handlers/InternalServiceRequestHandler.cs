#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.TO;
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
        public IPrincipal ExecutingUser { private get; set; }

        public InternalServiceRequestHandler()
            : this(ResourceCatalog.Instance, ServerAuthorizationService.Instance)
        {
        }
        public InternalServiceRequestHandler(IResourceCatalog catalog, IAuthorizationService authorizationService)
        {
            _catalog = catalog;
            _authorizationService = authorizationService;
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var serviceName = GetServiceName(ctx);
            var instanceId = GetInstanceID(ctx);
            var bookmark = GetBookmark(ctx);
            GetDataListID(ctx);
            var workspaceId = GetWorkspaceID(ctx);
            var formData = new WebRequestTO();

            var xml = GetPostData(ctx);

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

        string BuildStudioUrl(string payLoad)
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
            var isDebug = false;
            if (request.Args != null && request.Args.ContainsKey("IsDebug"))
            {
                var debugString = request.Args["IsDebug"].ToString();
                if (!bool.TryParse(debugString, out isDebug))
                {
                    isDebug = false;
                }
            }
            var serializer = new Dev2JsonSerializer();
            IDSFDataObject dataObject = new DsfDataObject(xmlData, dataListId);
            if (!dataObject.ExecutionID.HasValue)
            {
                dataObject.ExecutionID = Guid.NewGuid();
            }            
            dataObject.QueryString = queryString;

            if (isDebug)
            {
                dataObject.IsDebug = true;
            }
            dataObject.StartTime = DateTime.Now;
            dataObject.EsbChannel = channel;
            dataObject.ServiceName = request.ServiceName;

            var resource = request.ResourceID != Guid.Empty ? _catalog.GetResource(workspaceId, request.ResourceID) : _catalog.GetResource(workspaceId, request.ServiceName);
            var isManagementResource = false;
            if (!string.IsNullOrEmpty(request.TestName))
            {
                dataObject.TestName = request.TestName;
                dataObject.IsServiceTestExecution = true;
            }
            if (resource != null)
            {
                dataObject.ResourceID = resource.ResourceID;
                dataObject.SourceResourceID = resource.ResourceID;
                isManagementResource = _catalog.ManagementServices.ContainsKey(resource.ResourceID);
            }
            else
            {
                if (request.ResourceID != Guid.Empty)
                {
                    dataObject.ResourceID = request.ResourceID;
                }
            }

            dataObject.ClientID = Guid.Parse(connectionId);
            Common.Utilities.OrginalExecutingUser = ExecutingUser;
            dataObject.ExecutingUser = ExecutingUser;
            if (!dataObject.Environment.HasErrors())
            {
                return ProcessRequest(request, workspaceId, channel, dataObject, isManagementResource);
            }

            var msg = new ExecuteMessage { HasError = true };
            msg.SetMessage(string.Join(Environment.NewLine, dataObject.Environment.Errors));

            return serializer.SerializeToBuilder(msg);
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

                    TryExecuteRequest(request, workspaceId, channel, dataObject);
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

        private void TryExecuteRequest(EsbExecuteRequest request, Guid workspaceId, EsbServicesEndpoint channel, IDSFDataObject dataObject)
        {
            channel.ExecuteRequest(dataObject, request, workspaceId, out ErrorResultTO errors);
        }

        void IsAuthorizedForServiceTestRun(IDSFDataObject dataObject)
        {
            if (dataObject.IsServiceTestExecution && _authorizationService != null)
            {
                var authorizationService = _authorizationService;
                var hasContribute = authorizationService.IsAuthorized(AuthorizationContext.Contribute, Guid.Empty.ToString());
                if (!hasContribute)
                {
                    throw new UnauthorizedAccessException("The user does not have permission to execute tests.");
                }
            }
        }
    }
}
