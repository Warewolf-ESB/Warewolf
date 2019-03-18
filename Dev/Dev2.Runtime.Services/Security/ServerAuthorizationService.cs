#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

namespace Dev2.Runtime.Security
{
    public class ServerAuthorizationService : AuthorizationServiceBase
    {
        static ConcurrentDictionary<Tuple<string, string, AuthorizationContext>, Tuple<bool, DateTime>> _cachedRequests = new ConcurrentDictionary<Tuple<string, string, AuthorizationContext>, Tuple<bool, DateTime>>();

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

        protected static void ClearCaches()
        {
            _cachedRequests = new ConcurrentDictionary<Tuple<string, string, AuthorizationContext>, Tuple<bool, DateTime>>();
        }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            bool authorized;

            VerifyArgument.IsNotNull("resource", resource);

            var user = Common.Utilities.OrginalExecutingUser ?? ClaimsPrincipal.Current;

            var requestKey = new Tuple<string, string,AuthorizationContext>(user.Identity.Name, resource,context);
            authorized = _cachedRequests.TryGetValue(requestKey, out Tuple<bool, DateTime> authorizedRequest) && DateTime.Now.Subtract(authorizedRequest.Item2) < _timeOutPeriod ? authorizedRequest.Item1 : IsAuthorized(user, context, resource);

            if (!authorized)
            {
                if (ResultsCache.Instance.ContainsPendingRequestForUser(user.Identity.Name))
                {
                    authorized = true;
                }
            }
            else
            {
                if (resource != Guid.Empty.ToString())
                {
                    authorizedRequest = new Tuple<bool, DateTime>(authorized, DateTime.Now);
                    _cachedRequests.AddOrUpdate(requestKey, authorizedRequest, (tuple, tuple1) => authorizedRequest);
                }
            }

            if (!authorized)
            {
                _perfCounter?.Increment();
            }
            return authorized;
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
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

                case WebServerRequestType.WebExecuteService:
                case WebServerRequestType.WebExecuteSecureWorkflow:
                case WebServerRequestType.WebExecutePublicWorkflow:
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

        static string GetResource(IAuthorizationRequest request)
        {
            var resource = request.QueryString["rid"];
            if (string.IsNullOrEmpty(resource))
            {
                switch (request.RequestType)
                {
                    case WebServerRequestType.WebExecuteService:
                        resource = GetWebExecuteName(request.Url.AbsolutePath);
                        break;

                    case WebServerRequestType.WebBookmarkWorkflow:
                        resource = GetWebBookmarkName(request.Url.AbsolutePath);
                        break;

                    case WebServerRequestType.WebExecuteInternalService:
                        resource = GetWebExecuteName(request.Url.AbsolutePath);
                        break;
                    case WebServerRequestType.Unknown:
                        break;
                    case WebServerRequestType.WebGetDecisions:
                        break;
                    case WebServerRequestType.WebGetDialogs:
                        break;
                    case WebServerRequestType.WebGetServices:
                        break;
                    case WebServerRequestType.WebGetSources:
                        break;
                    case WebServerRequestType.WebGetSwitch:
                        break;
                    case WebServerRequestType.WebGet:
                        break;
                    case WebServerRequestType.WebGetContent:
                        break;
                    case WebServerRequestType.WebGetImage:
                        break;
                    case WebServerRequestType.WebGetScript:
                        break;
                    case WebServerRequestType.WebGetView:
                        break;
                    case WebServerRequestType.WebInvokeService:
                        break;
                    case WebServerRequestType.WebExecuteSecureWorkflow:
                        break;
                    case WebServerRequestType.WebExecutePublicWorkflow:
                        break;
                    case WebServerRequestType.WebExecuteGetLogFile:
                        break;
                    case WebServerRequestType.WebExecuteGetRootLevelApisJson:
                        break;
                    case WebServerRequestType.WebExecuteGetApisJsonForFolder:
                        break;
                    case WebServerRequestType.HubConnect:
                        break;
                    case WebServerRequestType.EsbOnConnected:
                        break;
                    case WebServerRequestType.EsbOnDisconnected:
                        break;
                    case WebServerRequestType.EsbOnReconnected:
                        break;
                    case WebServerRequestType.EsbAddDebugWriter:
                        break;
                    case WebServerRequestType.EsbFetchExecutePayloadFragment:
                        break;
                    case WebServerRequestType.EsbExecuteCommand:
                        break;
                    case WebServerRequestType.EsbAddItemMessage:
                        break;
                    case WebServerRequestType.EsbSendMemo:
                        break;
                    case WebServerRequestType.EsbFetchResourcesAffectedMemo:
                        break;
                    case WebServerRequestType.EsbSendDebugState:
                        break;
                    case WebServerRequestType.EsbWrite:
                        break;
                    case WebServerRequestType.ResourcesSendMemo:
                        break;
                    default:
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
            if (startIndex.HasValue)
            {
                var endIndex = absolutePath.IndexOf("/instances/", startIndex.Value, StringComparison.InvariantCultureIgnoreCase);
                if (endIndex != -1)
                {
                    return HttpUtility.UrlDecode(absolutePath.Substring(startIndex.Value, endIndex - startIndex.Value));
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