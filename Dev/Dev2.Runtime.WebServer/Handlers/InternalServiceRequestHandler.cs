/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.TransferObjects;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class InternalServiceRequestHandler : AbstractWebRequestHandler
    {
        public IPrincipal ExecutingUser { get; set; }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var serviceName = GetServiceName(ctx);
            var instanceId = GetInstanceID(ctx);
            var bookmark = GetBookmark(ctx);
            GetDataListID(ctx);
            var workspaceID = GetWorkspaceID(ctx);
            var formData = new WebRequestTO();

            var xml = GetPostData(ctx);

            if(!String.IsNullOrEmpty(xml))
            {
                formData.RawRequestPayload = xml;
            }

            formData.ServiceName = serviceName;
            formData.InstanceID = instanceId;
            formData.Bookmark = bookmark;
            formData.WebServerUrl = ctx.Request.Uri.ToString();
            formData.Dev2WebServer = String.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);

            if(ExecutingUser == null)
            {
                throw new Exception(ErrorResource.NullExecutingUser);
            }

            try
            {
                // Execute in its own thread to give proper context ;)
                var t = new Thread(() =>
                {
                    Thread.CurrentPrincipal = ExecutingUser;

                    var responseWriter = CreateForm(formData, serviceName, workspaceID, ctx.FetchHeaders(),ctx.Request.User);
                    ctx.Send(responseWriter);
                });

                t.Start();

                t.Join();
            }
            catch(Exception e)
            {
                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Error(this, e);
                // ReSharper restore InvokeAsExtensionMethod
            }
        }

        public StringBuilder ProcessRequest(EsbExecuteRequest request, Guid workspaceID, Guid dataListID, string connectionId)
        {
            var channel = new EsbServicesEndpoint();
            var xmlData = string.Empty;
            if(request.Args != null && request.Args.ContainsKey("DebugPayload"))
            {
                xmlData = request.Args["DebugPayload"].ToString();
                xmlData = xmlData.Replace("<DataList>", "<XmlData>").Replace("</DataList>", "</XmlData>");
            }

            if(string.IsNullOrEmpty(xmlData))
            {
                xmlData = "<DataList></DataList>";
            }

            IDSFDataObject dataObject = new DsfDataObject(xmlData, dataListID);
            dataObject.StartTime = DateTime.Now;
            dataObject.EsbChannel = channel;
            dataObject.ServiceName = request.ServiceName;
           
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, request.ServiceName);
            var isManagementResource = false;
            if(resource != null)
            {
                dataObject.ResourceID = resource.ResourceID;
                isManagementResource =  ResourceCatalog.Instance.ManagementServices.ContainsKey(resource.ResourceID);
            }
            dataObject.ClientID = Guid.Parse(connectionId);
            Common.Utilities.OrginalExecutingUser = ExecutingUser;
            dataObject.ExecutingUser = ExecutingUser;
            // we need to assign new ThreadID to request coming from here, because it is a fixed connection and will not change ID on its own ;)
            if(!dataObject.Environment.HasErrors())
            {
                ErrorResultTO errors;

                if(ExecutingUser == null)
                {
                    throw new Exception(ErrorResource.NullExecutingUser);
                }

                try
                {
                    // Execute in its own thread to give proper context ;)
                    var t = new Thread(() =>
                    {
                        Thread.CurrentPrincipal = ExecutingUser;
                        if(isManagementResource)
                        {
                            Thread.CurrentPrincipal = Common.Utilities.ServerUser;
                            ExecutingUser = Common.Utilities.ServerUser;
                            dataObject.ExecutingUser = Common.Utilities.ServerUser;
                        }
                        channel.ExecuteRequest(dataObject, request, workspaceID, out errors);
                    });

                    t.Start();

                    t.Join();
                }
                catch(Exception e)
                {
                    Dev2Logger.Error(e.Message,e);
                }



                if(request.ExecuteResult.Length > 0)
                {
                    return request.ExecuteResult;
                }

                return new StringBuilder();
            }

            ExecuteMessage msg = new ExecuteMessage { HasError = true };
            msg.SetMessage(String.Join(Environment.NewLine, dataObject.Environment.Errors));

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(msg);
        }
    }
}
