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
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using Dev2.Common;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;
using Warewolf.Security;

namespace Dev2.Runtime.WebServer.Controllers
{
    public abstract class AbstractController : ApiController
    {
        protected virtual HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables, bool isUrlWithTokenPrefix)
            where TRequestHandler : class, IRequestHandler, new()
        {
            var user = User;

            try
            {
                if (isUrlWithTokenPrefix)
                {
                    if (!TryOverrideByToken(ref user))
                    {
                        throw new HttpException(401,
                            Warewolf.Resource.Errors.ErrorResource.TokenNotAuthorizedToExecuteOuterWorkflowException);
                    }
                }
                else
                {
                    if (!IsAuthenticated())
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }
                }

                var context = new WebServerContext(Request, requestVariables) {Request = {User = user}};
                var handler = CreateHandler<TRequestHandler>();
                handler.ProcessRequest(context);
                return context.ResponseMessage;
            }
            catch (HttpException e)
            {
                var response  = Request.CreateResponse((HttpStatusCode)e.GetHttpCode());
                response.ReasonPhrase = e.Message;
                return response;
            }
        }

        private bool TryOverrideByToken(ref IPrincipal user)
        {
            var token = Request.Headers.Authorization?.Parameter;
            try
            {
                user = new JwtManager(new SecuritySettings()).BuildPrincipal(token);
                if (user is null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Warn($"failed to use authorization header: {e.Message}", GlobalConstants.WarewolfWarn);
            }

            return true;
        }

        protected HttpResponseMessage ProcessRequest<TRequestHandler>()
            where TRequestHandler : class, IRequestHandler, new()
        {
            if (!IsAuthenticated())
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var context = new WebServerContext(Request) {Request = {User = User}};
            var handler = CreateHandler<TRequestHandler>();
            handler.ProcessRequest(context);

            return context.ResponseMessage;
        }

        protected virtual bool IsAuthenticated() => User.IsAuthenticated();

        protected virtual TRequestHandler CreateHandler<TRequestHandler>()
            where TRequestHandler : class, IRequestHandler, new() => Activator.CreateInstance<TRequestHandler>();
    }
}