using System;
using System.Threading.Tasks;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
//using Microsoft.AspNet.SignalR;
//using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dev2.Runtime.WebServer.Security
{
    public class CustomHubFilter : IHubFilter
    {
        [ActivatorUtilitiesConstructor]
        public CustomHubFilter()
               : this(ServerAuthorizationService.Instance)
        {

        }

        public CustomHubFilter(Services.Security.IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("AuthorizationService", authorizationService);
            Service = authorizationService;
        }

        public IAuthorizationService Service { get; private set; }


        public Task OnConnectedAsync(HubLifetimeContext hubLifeTimeContext, Func<HubLifetimeContext, Task> next)
        {
            VerifyArgument.IsNotNull(nameof(hubLifeTimeContext), hubLifeTimeContext);

            var httpContext = hubLifeTimeContext.Context.GetHttpContext();

            if (httpContext != null && httpContext.User.IsAuthenticated() && Service.IsAuthorized(hubLifeTimeContext.GetAuthorizationRequest()))
                return next?.Invoke(hubLifeTimeContext);

            return Task.FromResult(false);
        }

        public async ValueTask<object> InvokeMethodAsync(
            HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            VerifyArgument.IsNotNull(nameof(invocationContext), invocationContext);

            var httpContext = invocationContext.Context.GetHttpContext();
            VerifyArgument.IsNotNull(nameof(httpContext), httpContext);

            var request = invocationContext.GetAuthorizationRequest();
            var result = httpContext.User.IsAuthenticated() && Service.IsAuthorized(request);
            //var result = AuthorizeHubMethodInvocation(httpContext, httpContext.getre);
            if (result)
            {
                var handler = next;
                if (handler != null)
                    return await handler(invocationContext);
                else
                    return default(ValueTask);
            }

            return ValueTask.FromException(new HubException("Unauthorized"));
        }

        public bool AuthorizeHubMethodInvocation(HttpContext httpContext, WebServerRequestType requestType)
        {
            if (httpContext != null && httpContext.User.IsAuthenticated())
            {
                var request = httpContext.Request;
                var url = GetUri(request);
                var auth = new AuthorizationRequest
                {
                    Url = url,
                    User = httpContext.User,
                    QueryString = request.Query,
                    RequestType = requestType
                };

                return Service.IsAuthorized(auth);
            }
            return false;
        }

      

        public Task OnDisconnectedAsync(
            HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
        {
            return next?.Invoke(context, exception);
        }

        private static Uri GetUri(HttpRequest request)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Port = request.Host.Port.GetValueOrDefault(80),
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };
            return uriBuilder.Uri;
        }

    }
}
