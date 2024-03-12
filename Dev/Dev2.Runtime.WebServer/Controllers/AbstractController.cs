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
using Dev2.Common;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;
using Warewolf.Security;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
#endif

namespace Dev2.Runtime.WebServer.Controllers
{
#if NETFRAMEWORK
    public abstract class AbstractController : ApiController
#else
    public abstract class AbstractController : ControllerBase
#endif
    {
        protected virtual HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables, bool isUrlWithTokenPrefix)
            where TRequestHandler : class, IRequestHandler, new()
        {
            var user = User;
#if !NETFRAMEWORK
            var requestMessage = Request.HttpContext.GetHttpRequestMessage();
#endif
            try
            {
                if (isUrlWithTokenPrefix)
                {
                    if (!TryOverrideByToken(ref user))
                    {
#if NETFRAMEWORK
                        return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.TOKEN_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisToken });
#else
                        return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.TOKEN_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisToken });
#endif
                    }
                }
                else
                {
                    if (!IsAuthenticated())
                    {
#if NETFRAMEWORK
                        return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
#else
                        return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
#endif
                    }
                }

#if NETFRAMEWORK
                var context = new WebServerContext(Request, requestVariables) {Request = {User = user}};
#else
                var context = new WebServerContext(requestMessage, requestVariables) { Request = { User = user } };
#endif
                var handler = CreateHandler<TRequestHandler>();
                handler.ProcessRequest(context);
                return context.ResponseMessage;
            }
            catch (Exception e)
            {
#if NETFRAMEWORK
                return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.InternalServerError, Title = GlobalConstants.INTERNAL_SERVER_ERROR, Message = e.Message });
#else
                return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.InternalServerError, Title = GlobalConstants.INTERNAL_SERVER_ERROR, Message = e.Message });
#endif
            }
        }

#if NETFRAMEWORK
        private bool TryOverrideByToken(ref IPrincipal user)
        {
            var token = Request.Headers.Authorization?.Parameter;
#else
        private bool TryOverrideByToken(ref System.Security.Claims.ClaimsPrincipal user)
        {
            var token = Request.Headers.Authorization.Count > 0 ? Request.Headers.Authorization[0] : string.Empty;
            if (string.IsNullOrEmpty(token)) return false;
#endif

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
#if NETFRAMEWORK
            if (!IsAuthenticated())
            {
                return Request.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
            }

            var context = new WebServerContext(Request) {Request = {User = User}};
#else
            var requestMessage = Request.HttpContext.GetHttpRequestMessage();

            if (!IsAuthenticated())
            {
                return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
            }

            var context = new WebServerContext(requestMessage) { Request = { User = User } };
#endif
            var handler = CreateHandler<TRequestHandler>();
            handler.ProcessRequest(context);

            return context.ResponseMessage;
        }

        protected virtual bool IsAuthenticated() => User.IsAuthenticated();

        protected virtual TRequestHandler CreateHandler<TRequestHandler>()
            where TRequestHandler : class, IRequestHandler, new() => Activator.CreateInstance<TRequestHandler>();
    }
}