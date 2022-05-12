/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
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
#if NETFRAMEWORK
using System.Web.Http;
#endif
using Dev2.Common;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;
using Warewolf.Security;

namespace Dev2.Runtime.WebServer.Controllers
{
#if NETFRAMEWORK
    public abstract class AbstractController : ApiController
#else
    public abstract class AbstractController : System.Web.Mvc.Controller
#endif
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
                        return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.TOKEN_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisToken });
                    }
                }
                else
                {
                    if (!IsAuthenticated())
                    {
                        return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
                    }
                }

                var context = new WebServerContext(Request, requestVariables) {Request = {User = user}};
                var handler = CreateHandler<TRequestHandler>();
                handler.ProcessRequest(context);
                return context.ResponseMessage;
            }
            catch (Exception e)
            {
                return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.InternalServerError, Title = GlobalConstants.INTERNAL_SERVER_ERROR, Message = e.Message });
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
                return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
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