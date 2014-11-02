/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using Dev2.Common;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebsiteServiceHandler : AbstractWebRequestHandler
    {
        private readonly ServiceInvoker _serviceInvoker = new ServiceInvoker();

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            // Read post data which is expected to be JSON
            string args;
            using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
            {
                args = reader.ReadToEnd();
            }

            string className = GetClassName(ctx);
            string methodName = GetMethodName(ctx);
            string dataListID = GetDataListID(ctx);
            string workspaceID = GetWorkspaceID(ctx);
            dynamic result;
            try
            {
                Guid workspaceGuid;
                Guid.TryParse(workspaceID, out workspaceGuid);

                Guid dataListGuid;
                Guid.TryParse(dataListID, out dataListGuid);

                //
                // NOTE: result.ToString() MUST return JSON
                //

                // Ensure we execute as the correct user ;)
                IPrincipal userPrinciple = ctx.Request.User;
                if (userPrinciple != null)
                {
                    Thread.CurrentPrincipal = userPrinciple;
                    Dev2Logger.Log.Info("WEB EXECUTION USER CONTEXT [ " + userPrinciple.Identity.Name + " ]");
                }

                result = _serviceInvoker.Invoke(className, methodName, args, workspaceGuid, dataListGuid);
            }
            catch (Exception ex)
            {
                result = new ValidationResult
                {
                    ErrorMessage = ex.Message
                };
            }
            ctx.Send(new StringResponseWriter(result.ToString(), ContentTypes.Json));
        }
    }
}