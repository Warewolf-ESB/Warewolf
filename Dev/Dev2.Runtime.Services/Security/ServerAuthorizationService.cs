using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Web;
using Dev2.Common;
using Dev2.Services.Security;

namespace Dev2.Runtime.Security
{
    public class ServerAuthorizationService : AuthorizationServiceBase
    {
        readonly ConcurrentDictionary<Tuple<string, string>, Tuple<bool, DateTime>> _cachedRequests = new ConcurrentDictionary<Tuple<string, string>, Tuple<bool, DateTime>>();

        // Singleton instance - lazy initialization is used to ensure that the creation is thread-safe
        static readonly Lazy<ServerAuthorizationService> TheInstance = new Lazy<ServerAuthorizationService>(() => new ServerAuthorizationService(new ServerSecurityService()));
        public static IAuthorizationService Instance { get { return TheInstance.Value; } }

        readonly TimeSpan _timeOutPeriod;

        protected ServerAuthorizationService(ISecurityService securityService)
            : base(securityService, true)
        {
            _timeOutPeriod = securityService.TimeOutPeriod;
            securityService.Read();
        }

        public int CachedRequestCount { get { return _cachedRequests.Count; } }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(ClaimsPrincipal.Current, context, resource);
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
        {
            ServerLogger.LogTrace("IsAuthorized Request Entered");

            VerifyArgument.IsNotNull("request", request);
            bool authorized;
            Tuple<bool, DateTime> authorizedRequest;
            if(_cachedRequests.TryGetValue(request.Key, out authorizedRequest) && DateTime.Now.Subtract(authorizedRequest.Item2) < _timeOutPeriod)
            {
                authorized = authorizedRequest.Item1;
            }
            else
            {
                authorized = IsAuthorizedImpl(request);
            }
            authorizedRequest = new Tuple<bool, DateTime>(authorized, DateTime.Now);
            _cachedRequests.AddOrUpdate(request.Key, authorizedRequest, (tuple, tuple1) => authorizedRequest);
            return authorized;
        }

        bool IsAuthorizedImpl(IAuthorizationRequest request)
        {
            switch(request.RequestType)
            {
                case WebServerRequestType.WebGetDecisions:
                case WebServerRequestType.WebGetDialogs:
                case WebServerRequestType.WebGetServices:
                case WebServerRequestType.WebGetSources:
                case WebServerRequestType.WebGetSwitch:
                    return IsAuthorized(request.User, AuthorizationContext.View, GetResource(request));

                case WebServerRequestType.WebGet:
                case WebServerRequestType.WebGetContent:
                case WebServerRequestType.WebGetImage:
                case WebServerRequestType.WebGetScript:
                case WebServerRequestType.WebGetView:
                    return IsAuthorized(request.User, AuthorizationContext.Any, GetResource(request));

                case WebServerRequestType.WebInvokeService:
                    var authorizationContext = IsWebInvokeServiceSave(request.Url.AbsolutePath) ? AuthorizationContext.Contribute : AuthorizationContext.View;
                    return IsAuthorized(request.User, authorizationContext, GetResource(request));

                case WebServerRequestType.WebExecuteWorkflow:
                case WebServerRequestType.WebBookmarkWorkflow:
                    return IsAuthorized(request.User, AuthorizationContext.Execute, GetResource(request));

                case WebServerRequestType.WebExecuteInternalService:
                    return IsAuthorized(request.User, AuthorizationContext.Any, GetResource(request));
                case WebServerRequestType.HubConnect:
                    var result = IsAuthorizedToConnect(request.User);
                    if(!result)
                    {
                        this.LogError("AUTH ERROR FOR USER : " + request.User.Identity.Name);
                        DumpPermissionsOnError(request.User);
                    }
                    return result;
                case WebServerRequestType.EsbSendMemo:
                case WebServerRequestType.EsbAddDebugWriter:
                case WebServerRequestType.EsbExecuteCommand:
                case WebServerRequestType.EsbSendDebugState:
                case WebServerRequestType.EsbWrite:
                case WebServerRequestType.EsbOnConnected:
                case WebServerRequestType.EsbFetchExecutePayloadFragment:
                case WebServerRequestType.ResourcesSendMemo:
                    return IsAuthorizedToConnect(request.User);
            }

            return false;
        }

        static string GetResource(IAuthorizationRequest request)
        {
            var resource = request.QueryString["rid"];
            if(string.IsNullOrEmpty(resource))
            {
                switch(request.RequestType)
                {
                    case WebServerRequestType.WebExecuteWorkflow:
                        resource = GetWebExecuteName(request.Url.AbsolutePath);
                        break;

                    case WebServerRequestType.WebBookmarkWorkflow:
                        resource = GetWebBookmarkName(request.Url.AbsolutePath);
                        break;

                    case WebServerRequestType.WebExecuteInternalService:
                        resource = GetWebExecuteName(request.Url.AbsolutePath);
                        break;
                }
            }
            return string.IsNullOrEmpty(resource) ? null : resource;
        }

        protected override void RaisePermissionsChanged()
        {
            _cachedRequests.Clear();
            base.RaisePermissionsChanged();
        }

        static string GetWebExecuteName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            return startIndex.HasValue ? HttpUtility.UrlDecode(absolutePath.Substring(startIndex.Value, absolutePath.Length - startIndex.Value)) : null;
        }

        static string GetWebBookmarkName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            if(startIndex.HasValue)
            {
                var endIndex = absolutePath.IndexOf("/instances/", startIndex.Value, StringComparison.InvariantCultureIgnoreCase);
                if(endIndex != -1)
                {
                    return HttpUtility.UrlDecode(absolutePath.Substring(startIndex.Value, endIndex - startIndex.Value));
                }
            }

            return null;
        }

        static int? GetNameStartIndex(string absolutePath)
        {
            var startIndex = absolutePath.IndexOf("services/", StringComparison.InvariantCultureIgnoreCase);
            if(startIndex == -1)
            {
                return startIndex;
            }

            startIndex += 9;
            return startIndex;
        }

        static bool IsWebInvokeServiceSave(string absolutePath)
        {
            return absolutePath.EndsWith("/save", StringComparison.InvariantCultureIgnoreCase);
        }

        protected override void OnDisposed()
        {
        }
    }
}
