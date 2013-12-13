using System;
using System.Collections.Concurrent;
using System.Web;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer.Security
{
    public class AuthorizationService : AuthorizationServiceBase, IAuthorizationService
    {
        readonly ConcurrentDictionary<Tuple<string, string>, bool> _cachedRequests = new ConcurrentDictionary<Tuple<string, string>, bool>();

        // Singleton instance - lazy initialization is used to ensure that the creation is threadsafe
        // new ServerSecurityService()
        static readonly Lazy<AuthorizationService> TheInstance = new Lazy<AuthorizationService>(() => new AuthorizationService(null));
        public static AuthorizationService Instance { get { return TheInstance.Value; } }

        protected AuthorizationService(ISecurityService securityService)
            : base(securityService)
        {
            Load();
        }

        public int CachedRequestCount { get { return _cachedRequests.Count; } }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return false;
        }

        public bool IsAuthorized(IAuthorizationRequest request)
        {
            VerifyArgument.IsNotNull("request", request);
            bool authorized;
            if(!_cachedRequests.TryGetValue(request.Key, out authorized))
            {                
                authorized = IsAuthorized(request.User, GetContext(request), GetResource(request));
                _cachedRequests.TryAdd(request.Key, authorized);
            }
            return authorized;
        }

        static AuthorizationContext GetContext(IAuthorizationRequest request)
        {
            switch(request.RequestType)
            {
                case WebServerRequestType.WebGet:
                case WebServerRequestType.WebGetContent:
                case WebServerRequestType.WebGetImage:
                case WebServerRequestType.WebGetScript:
                case WebServerRequestType.WebGetView:
                    return AuthorizationContext.View;

                case WebServerRequestType.WebInvokeService:
                    return IsWebInvokeServiceSave(request.Url.AbsolutePath)
                        ? AuthorizationContext.Contribute
                        : AuthorizationContext.View;

                case WebServerRequestType.WebExecuteWorkflow:
                case WebServerRequestType.WebBookmarkWorkflow:
                    return AuthorizationContext.Execute;

                case WebServerRequestType.HubConnect:
                case WebServerRequestType.EsbSendMemo:
                case WebServerRequestType.ResourcesSend:
                case WebServerRequestType.ResourcesSendMemo:
                case WebServerRequestType.ResourcesSave:
                    // TODO: Fix hub permissions
                    return AuthorizationContext.Contribute;
            }
            return AuthorizationContext.None;
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
    }
}
