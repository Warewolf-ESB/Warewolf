using System;
using System.Threading.Tasks;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
//using Microsoft.AspNet.SignalR;
//using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;

namespace Dev2.Runtime.WebServer.Security
{
    public class CustomHubFilter : IHubFilter
    {

        public async ValueTask<object> InvokeMethodAsync(
            HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            var httpContext = invocationContext.Context.GetHttpContext();
            if (httpContext != null && httpContext.User.IsAuthenticated())
            {
                var request = httpContext.Request;
                var url = GetUri(request);
                var auth = new AuthorizationRequest
                {
                    Url = url,
                    User = httpContext.User,
                    QueryString = request.Query,
                    RequestType = WebServerRequestType.HubConnect
                };

                if (ServerAuthorizationService.Instance.IsAuthorized(auth))
                {
                    var handler = next;
                    if (handler != null)
                        return await handler(invocationContext);
                    else
                        return default(ValueTask);
                }
            }

            return ValueTask.FromException(new HubException("Unauthorized"));
        }

        public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            return next?.Invoke(context);
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
