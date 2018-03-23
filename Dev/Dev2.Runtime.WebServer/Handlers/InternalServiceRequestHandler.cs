/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
                // Execute in its own thread to give proper context ;)
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
                if (ExecutingUser == null)
                {
                    throw new Exception(ErrorResource.NullExecutingUser);
                }
                try
                {
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
                            IsAuthorized(dataObject);
                        }

                        channel.ExecuteRequest(dataObject, request, workspaceId, out ErrorResultTO errors);
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

            var msg = new ExecuteMessage { HasError = true };
            msg.SetMessage(string.Join(Environment.NewLine, dataObject.Environment.Errors));

            return serializer.SerializeToBuilder(msg);
        }

        private void IsAuthorized(IDSFDataObject dataObject)
        {
            if (dataObject.IsServiceTestExecution && _authorizationService != null)
            {
                var authorizationService = _authorizationService;
                var hasContribute =
                    authorizationService.IsAuthorized(AuthorizationContext.Contribute,
                        Guid.Empty.ToString());
                if (!hasContribute)
                {
                    throw new UnauthorizedAccessException(
                        "The user does not have permission to execute tests.");
                }
            }
        }
    }
}
