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
using Microsoft.AspNetCore.Mvc;
using Warewolf.Resource.Errors;
using Warewolf.Security;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;

namespace Dev2.Runtime.WebServer.Controllers
{
    //[ApiController]
    //[Route("controller")]
    public abstract class AbstractController : ControllerBase
    {
        protected virtual HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables, bool isUrlWithTokenPrefix)
            where TRequestHandler : class, IRequestHandler, new()
        {
            var user = User;
            var requestMessage = Request.HttpContext.GetHttpRequestMessage();
            try
            {
                if (isUrlWithTokenPrefix)
                {
                    if (!TryOverrideByToken(ref user))
                    {
                        return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.TOKEN_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisToken });
                    }
                }
                else
                {
                    if (!IsAuthenticated())
                    {
                        return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
                    }
                }

                var context = new WebServerContext(requestMessage, requestVariables) { Request = { User = user } };
                var handler = CreateHandler<TRequestHandler>();
                handler.ProcessRequest(context);
                return context.ResponseMessage;
            }
            catch (Exception e)
            {
                return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.InternalServerError, Title = GlobalConstants.INTERNAL_SERVER_ERROR, Message = e.Message });
            }
        }

        private bool TryOverrideByToken(ref System.Security.Claims.ClaimsPrincipal user)
        {
            var token = Request.Headers.Authorization.Count > 0 ? Request.Headers.Authorization[0] : string.Empty;
            if (string.IsNullOrEmpty(token)) return false;

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
            var requestMessage = Request.HttpContext.GetHttpRequestMessage();

            if (!IsAuthenticated())
            {
                return requestMessage.CreateWarewolfErrorResponse(new WarewolfErrorResponseArgs { StatusCode = HttpStatusCode.Unauthorized, Title = GlobalConstants.USER_UNAUTHORIZED, Message = ErrorResource.AuthorizationDeniedForThisUser });
            }

            var context = new WebServerContext(requestMessage) { Request = { User = User } };
            var handler = CreateHandler<TRequestHandler>();
            handler.ProcessRequest(context);

            return context.ResponseMessage;
        }

        protected virtual bool IsAuthenticated() => User.IsAuthenticated();

        protected virtual TRequestHandler CreateHandler<TRequestHandler>()
            where TRequestHandler : class, IRequestHandler, new() => Activator.CreateInstance<TRequestHandler>();
    }
}