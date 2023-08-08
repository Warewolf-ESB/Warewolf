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

        public CustomHubFilter(Dev2.Services.Security.IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("AuthorizationService", authorizationService);
            Service = authorizationService;
        }

        public IAuthorizationService Service { get; private set; }


        public Task OnConnectedAsync(HubLifetimeContext hubLifeTimeContext, Func<HubLifetimeContext, Task> next)
        {
            if (AuthorizeHubConnection(hubLifeTimeContext))
                return next?.Invoke(hubLifeTimeContext);

            return Task.FromResult(false);
        }
      

        public async ValueTask<object> InvokeMethodAsync(
            HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            if (AuthorizeHubMethodInvocation(invocationContext))
            {
                if (next != null)
                    return await next(invocationContext);
                else
                    return default(ValueTask);
            }

            return ValueTask.FromException(new HubException("Unauthorized"));
        }

        public bool AuthorizeHubConnection(HubLifetimeContext hubLifeTimeContext)
        {
            VerifyArgument.IsNotNull(nameof(hubLifeTimeContext), hubLifeTimeContext);

            var httpContext = hubLifeTimeContext.Context.GetHttpContext();

            return (httpContext.User.IsAuthenticated() || httpContext.User.IsAnonymous()) && Service.IsAuthorized(hubLifeTimeContext.GetAuthorizationRequest());
        }

        public bool AuthorizeHubMethodInvocation(HubInvocationContext invocationContext)
        {
            VerifyArgument.IsNotNull(nameof(invocationContext), invocationContext);

            var httpContext = invocationContext.Context.GetHttpContext();
            
            VerifyArgument.IsNotNull(nameof(httpContext), httpContext);

            return httpContext != null && (httpContext.User.IsAuthenticated() || httpContext.User.IsAnonymous()) && Service.IsAuthorized(invocationContext.GetAuthorizationRequest());

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
