using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Data.Settings.Security;
using Dev2.Runtime.ESB.Management.Services;

namespace Dev2.Runtime.WebServer.Security
{
    public class AuthorizationProvider : IAuthorizationProvider
    {
        readonly ISecurityConfigProvider _securityConfigProvider;
        static readonly string[] EmptyRoles = new string[0];

        readonly ConcurrentDictionary<Tuple<string, string>, bool> _requests = new ConcurrentDictionary<Tuple<string, string>, bool>();

        // Singleton instance - lazy initialization is used to ensure that the creation is threadsafe
        readonly static Lazy<AuthorizationProvider> TheInstance = new Lazy<AuthorizationProvider>(() => new AuthorizationProvider(new SecurityConfigProvider()));
        public static AuthorizationProvider Instance
        {
            get
            {
                return TheInstance.Value;
            }
        }

        protected AuthorizationProvider(ISecurityConfigProvider securityConfigProvider)
        {
            VerifyArgument.IsNotNull("securityConfigProvider", securityConfigProvider);
            _securityConfigProvider = securityConfigProvider;
            _securityConfigProvider.Changed += OnSecurityConfigProviderChanged;
        }

        void OnSecurityConfigProviderChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            _requests.Clear();
        }

        public bool IsAuthorized(IAuthorizationRequest request)
        {
            bool authorized;
            if(!_requests.TryGetValue(request.Key, out authorized))
            {
                var roles = GetRoles(request);
                authorized = roles.Any(request.User.IsInRole);
                _requests.TryAdd(request.Key, authorized);
            }
            return authorized;
        }

        IEnumerable<string> GetRoles(IAuthorizationRequest request)
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

            switch(request.RequestType)
            {
                case WebServerRequestType.WebGet:
                case WebServerRequestType.WebGetContent:
                case WebServerRequestType.WebGetImage:
                case WebServerRequestType.WebGetScript:
                case WebServerRequestType.WebGetView:
                    return _securityConfigProvider.Permissions.Where(p => (p.View || p.Contribute) && Matches(p, resource)).Select(p => p.WindowsGroup);

                case WebServerRequestType.WebInvokeService:
                    return _securityConfigProvider.Permissions.Where(p => (p.Contribute || p.Execute) && Matches(p, resource)).Select(p => p.WindowsGroup);

                case WebServerRequestType.WebExecuteWorkflow:
                    return _securityConfigProvider.Permissions.Where(p => (p.Contribute || p.Execute) && Matches(p, resource)).Select(p => p.WindowsGroup);

                case WebServerRequestType.WebBookmarkWorkflow:
                    return _securityConfigProvider.Permissions.Where(p => (p.Contribute || p.Execute) && Matches(p, resource)).Select(p => p.WindowsGroup);
            }
            return EmptyRoles;


            //return new List<string>
            //{
            //    "xxxx",
            //    "DnsAdmins",                
            //    WindowsGroupPermission.BuiltInAdministratorsText
            //};
        }

        static bool Matches(WindowsGroupPermission permission, string resource)
        {
            if(permission.IsServer)
            {
                return true;
            }

            if(string.IsNullOrEmpty(resource))
            {
                return false;
            }

            Guid resourceID;
            if(Guid.TryParse(resource, out resourceID))
            {
                return permission.ResourceID == resourceID;
            }

            // ResourceName is in the format: {categoryName}\{resourceName}
            return permission.ResourceName.EndsWith("\\" + resource);
        }

        static string GetWebExecuteName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            return startIndex.HasValue ? absolutePath.Substring(startIndex.Value, absolutePath.Length - startIndex.Value) : null;
        }

        static string GetWebBookmarkName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            if(startIndex.HasValue)
            {
                var endIndex = absolutePath.IndexOf("/instances/", startIndex.Value, StringComparison.InvariantCultureIgnoreCase);
                if(endIndex != -1)
                {
                    return absolutePath.Substring(startIndex.Value, endIndex - startIndex.Value);
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
    }
}
