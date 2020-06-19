#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Services.Security;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Web;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Warewolf.Data;

namespace Dev2.Runtime.Security
{
    public class ServerAuthorizationService : AuthorizationServiceBase
    {
        static ConcurrentDictionary<AuthorizationRequestKey, Tuple<bool, DateTime>> _cachedRequests = new ConcurrentDictionary<AuthorizationRequestKey, Tuple<bool, DateTime>>();

        static Lazy<ServerAuthorizationService> _theInstance = new Lazy<ServerAuthorizationService>(() => new ServerAuthorizationService(new ServerSecurityService()));

        public static IAuthorizationService Instance
        {
            get
            {
                var serverAuthorizationService = _theInstance.Value;
                serverAuthorizationService.SecurityService.PermissionsChanged += (s, e) => ClearCaches();
                serverAuthorizationService.SecurityService.PermissionsModified += (s, e) => ClearCaches();
                return serverAuthorizationService;        
            }
        }

        readonly TimeSpan _timeOutPeriod;
        readonly IPerformanceCounter _perfCounter;
        private static readonly IResourceCatalog _resourceCatalog = ResourceCatalog.Instance;

        protected ServerAuthorizationService(ISecurityService securityService)
            : base(securityService, true)
        {
            _timeOutPeriod = securityService.TimeOutPeriod;            
            try
            {
                _perfCounter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Count of Not Authorised errors");
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        public int CachedRequestCount => _cachedRequests.Count;

        public static void ClearCaches()
        {
            _cachedRequests = new ConcurrentDictionary<AuthorizationRequestKey, Tuple<bool, DateTime>>();
        }

        public override bool IsAuthorized(AuthorizationContext context, IWarewolfResource resource)
        {
            return IsAuthorized(context, resource?.ResourceID ?? Guid.Empty);
        }

        public sealed override bool IsAuthorized(AuthorizationContext context, Guid resourceId)
        {
            bool authorized;

            VerifyArgument.IsNotNull("resourceId", resourceId);

            var user = Common.Utilities.OrginalExecutingUser ?? ClaimsPrincipal.Current;

            authorized = IsAuthorized(user, context, resourceId);

            if (!authorized)
            {
                if (ResultsCache.Instance.ContainsPendingRequestForUser(user.Identity.Name))
                {
                    authorized = true;
                }
            }

            if (!authorized)
            {
                _perfCounter?.Increment();
            }
            return authorized;
        }

        public sealed override bool IsAuthorized(IAuthorizationRequest request)
        {
            VerifyArgument.IsNotNull("request", request);
            bool authorized;
            authorized = _cachedRequests.TryGetValue(request.Key, out Tuple<bool, DateTime> authorizedRequest) && DateTime.Now.Subtract(authorizedRequest.Item2) < _timeOutPeriod ? authorizedRequest.Item1 : IsAuthorizedImpl(request);

            if (!authorized && (request.RequestType == WebServerRequestType.HubConnect || request.RequestType == WebServerRequestType.EsbFetchExecutePayloadFragment))
            {
                var identity = request.User.Identity;
                if (ResultsCache.Instance.ContainsPendingRequestForUser(identity.Name))
                {
                    authorized = true;
                }
            }
            else
            {
                authorizedRequest = new Tuple<bool, DateTime>(authorized, DateTime.Now);
                _cachedRequests.AddOrUpdate(request.Key, authorizedRequest, (tuple, tuple1) => authorizedRequest);
            }
            if (!authorized)
            {
                _perfCounter?.Increment();
            }
            return authorized;
        }

        bool IsAuthorizedImpl(IAuthorizationRequest request)
        {
            var result = false;
            switch (request.RequestType)
            {
                case WebServerRequestType.WebGetDecisions:
                case WebServerRequestType.WebGetDialogs:
                case WebServerRequestType.WebGetServices:
                case WebServerRequestType.WebGetSources:
                case WebServerRequestType.WebGetSwitch:
                    result = IsAuthorized(request.User, AuthorizationContext.View, GetResource(request));
                    break;

                case WebServerRequestType.WebGet:
                case WebServerRequestType.WebGetContent:
                case WebServerRequestType.WebGetImage:
                case WebServerRequestType.WebGetScript:
                case WebServerRequestType.WebGetView:
                    result = IsAuthorized(request.User, AuthorizationContext.Any, GetResource(request));
                    break;

                case WebServerRequestType.WebInvokeService:
                    var authorizationContext = IsWebInvokeServiceSave(request.Url.AbsolutePath) ? AuthorizationContext.Contribute : AuthorizationContext.View;
                    result = IsAuthorized(request.User, authorizationContext, GetResource(request));
                    break;

                case WebServerRequestType.WebExecuteLoginWorkflow:
                case WebServerRequestType.WebExecutePublicTokenWorkflow:
                    //TODO: Not sure what will happen here...
                    result = true;
                    break;

                case WebServerRequestType.WebExecuteSecureWorkflow:
                case WebServerRequestType.WebExecutePublicWorkflow:
                    result = IsAuthorized(request.User, AuthorizationContext.Execute, request);
                    break;
                case WebServerRequestType.WebExecuteService:
                case WebServerRequestType.WebBookmarkWorkflow:
                    result = IsAuthorized(request.User, AuthorizationContext.Execute, GetResource(request));
                    break;
                
                case WebServerRequestType.WebExecuteInternalService:
                    result = IsAuthorized(request.User, AuthorizationContext.Any, GetResource(request));
                    break;

                case WebServerRequestType.HubConnect:
                    result = IsAuthorizedToConnect(request.User);
                    break;

                case WebServerRequestType.WebExecuteGetLogFile:
                case WebServerRequestType.EsbSendMemo:
                case WebServerRequestType.EsbFetchResourcesAffectedMemo:
                case WebServerRequestType.EsbAddDebugWriter:
                case WebServerRequestType.EsbExecuteCommand:
                case WebServerRequestType.EsbSendDebugState:
                case WebServerRequestType.EsbWrite:
                case WebServerRequestType.EsbOnConnected:
                case WebServerRequestType.EsbFetchExecutePayloadFragment:
                case WebServerRequestType.ResourcesSendMemo:
                case WebServerRequestType.WebExecuteGetRootLevelApisJson:
                case WebServerRequestType.WebExecuteGetApisJsonForFolder:
                    result = IsAuthorizedToConnect(request.User);
                    break;
                case WebServerRequestType.Unknown:
                    break;
                case WebServerRequestType.EsbOnDisconnected:
                    break;
                case WebServerRequestType.EsbOnReconnected:
                    break;
                case WebServerRequestType.EsbAddItemMessage:
                    break;
                default:
                    break;
            }

            if (!result)
            {
                var user = "NULL USER";


                if (request.User.Identity != null)

                {
                    user = request.User.Identity.Name;
                    DumpPermissionsOnError(request.User);
                }
                Dev2Logger.Error("AUTH ERROR FOR USER : " + user, GlobalConstants.WarewolfError);
            }

            return result;
        }

        static WebName GetResource(IAuthorizationRequest request)
        {
            WebName resource = new WebNameResourceId(request.QueryString["rid"]);
            if (resource.IsValid)
            {
                return resource;
            }

            switch (request.RequestType)
            {
                case WebServerRequestType.WebExecuteService:
                    return GetWebExecuteName(request.Url.AbsolutePath);
                case WebServerRequestType.WebBookmarkWorkflow:
                    return GetWebBookmarkName(request.Url.AbsolutePath);
                case WebServerRequestType.WebExecuteInternalService:
                    return GetWebExecuteName(request.Url.AbsolutePath);
                case WebServerRequestType.Unknown:
                    return null;
                case WebServerRequestType.WebGetDecisions:
                    return null;
                case WebServerRequestType.WebGetDialogs:
                    return null;
                case WebServerRequestType.WebGetServices:
                    return null;
                case WebServerRequestType.WebGetSources:
                    return null;
                case WebServerRequestType.WebGetSwitch:
                    return null;
                case WebServerRequestType.WebGet:
                    return null;
                case WebServerRequestType.WebGetContent:
                    return null;
                case WebServerRequestType.WebGetImage:
                    return null;
                case WebServerRequestType.WebGetScript:
                    return null;
                case WebServerRequestType.WebGetView:
                    return null;
                case WebServerRequestType.WebInvokeService:
                    return null;
                case WebServerRequestType.WebExecuteSecureWorkflow:
                    return null;
                case WebServerRequestType.WebExecutePublicWorkflow:
                    return null;
                case WebServerRequestType.WebExecuteGetLogFile:
                    return null;
                case WebServerRequestType.WebExecuteGetRootLevelApisJson:
                    return null;
                case WebServerRequestType.WebExecuteGetApisJsonForFolder:
                    return null;
                case WebServerRequestType.HubConnect:
                    return null;
                case WebServerRequestType.EsbOnConnected:
                    return null;
                case WebServerRequestType.EsbOnDisconnected:
                    return null;
                case WebServerRequestType.EsbOnReconnected:
                    return null;
                case WebServerRequestType.EsbAddDebugWriter:
                    return null;
                case WebServerRequestType.EsbFetchExecutePayloadFragment:
                    return null;
                case WebServerRequestType.EsbExecuteCommand:
                    return null;
                case WebServerRequestType.EsbAddItemMessage:
                    return null;
                case WebServerRequestType.EsbSendMemo:
                    return null;
                case WebServerRequestType.EsbFetchResourcesAffectedMemo:
                    return null;
                case WebServerRequestType.EsbSendDebugState:
                    return null;
                case WebServerRequestType.EsbWrite:
                    return null;
                case WebServerRequestType.ResourcesSendMemo:
                    return null;
                default:
                    return null;
            }
        }

        protected override void RaisePermissionsChanged()
        {
            _cachedRequests.Clear();
            base.RaisePermissionsChanged();
        }

        static WebNameSimple GetWebExecuteName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            return new WebNameSimple(startIndex.HasValue ? HttpUtility.UrlDecode(absolutePath.Substring(startIndex.Value, absolutePath.Length - startIndex.Value)) : null);
        }

        class WebNameResourceId : WebName
        {
            string _resource;

            public WebNameResourceId(string resourceId)
            {
                _resource = resourceId;
            }
            public override bool IsValid { get => string.IsNullOrEmpty(_resource); }
            public override T Value<T>()
            {
                T tresource = default;
                if (_resource is T || _resource is null)
                {
                    return tresource;
                }
                throw new NotImplementedException("unsupported type for WebName");
            }
        }
        class BookMarkName : WebName
        {
            string _resource;

            public BookMarkName(string resource)
            {
                _resource = resource;
            }
            public override bool IsValid { get => string.IsNullOrEmpty(_resource); }
            public override T Value<T>()
            {
                if (_resource is T tresource)
                {
                    return tresource;
                }
                throw new NotImplementedException("unsupported type for WebName");
            }
        }
        static BookMarkName GetWebBookmarkName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            if (startIndex.HasValue)
            {
                var endIndex = absolutePath.IndexOf("/instances/", startIndex.Value, StringComparison.InvariantCultureIgnoreCase);
                if (endIndex != -1)
                {
                    return new BookMarkName(HttpUtility.UrlDecode(absolutePath.Substring(startIndex.Value, endIndex - startIndex.Value)));
                }
            }

            return null;
        }

        static int? GetNameStartIndex(string absolutePath)
        {
            var startIndex = absolutePath.IndexOf("services/", StringComparison.InvariantCultureIgnoreCase);
            if (startIndex == -1)
            {
                return startIndex;
            }

            startIndex += 9;
            return startIndex;
        }

        static bool IsWebInvokeServiceSave(string absolutePath) => absolutePath.EndsWith("/save", StringComparison.InvariantCultureIgnoreCase);

        protected override void OnDisposed()
        {
            SecurityService?.Dispose();
        }
    }
}