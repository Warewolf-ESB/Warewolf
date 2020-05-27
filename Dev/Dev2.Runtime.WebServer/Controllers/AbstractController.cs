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
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer.Controllers
{
    public abstract class AbstractController : ApiController
    {
        public WebServerContext Context { get; set; }


        protected virtual HttpResponseMessage ProcessTokenRequest<TRequestHandler>(NameValueCollection requestVariables)
            where TRequestHandler : class, IRequestHandler, new()
        {
            var context = new WebServerContext(Request, requestVariables) {Request = {User = User}};
            var handler = CreateHandler<TRequestHandler>();
            handler.ProcessRequest(context);
            return context.ResponseMessage;
        }

        protected virtual HttpResponseMessage ProcessRequest<TRequestHandler>(NameValueCollection requestVariables)
            where TRequestHandler : class, IRequestHandler, new()
        {
            var user = User;

            var isTokenRequest = requestVariables["isToken"];
            if (isTokenRequest == "False")
            {
                if (!IsAuthenticated())
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }

            if (isTokenRequest == "True")
            {
                var token = Request.Headers.Authorization?.Parameter;
                if (!string.IsNullOrWhiteSpace(token))
                {
                   user = new JwtManager(new SecuritySettings()).BuildPrincipal(token);
                }
            }

            var context = new WebServerContext(Request, requestVariables) { Request = { User = user } };
            var handler = CreateHandler<TRequestHandler>();


            handler.ProcessRequest(context);

            return context.ResponseMessage;
        }

        protected virtual HttpResponseMessage ProcessRequest<TRequestHandler>()
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