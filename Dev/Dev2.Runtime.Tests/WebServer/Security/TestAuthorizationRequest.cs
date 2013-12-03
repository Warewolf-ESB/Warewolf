using System;
using System.Linq;
using System.Security.Principal;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hosting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer.Security
{
    public class TestAuthorizationRequest : AuthorizationRequest
    {
        bool _userIsInRole;
        public const string UserRole = "Test";
        public const string OtherRole = "TestOther";

        public TestAuthorizationRequest()
        {
        }

        public TestAuthorizationRequest(Permissions allowedPermissions, WebServerRequestType requestType, string url, INameValueCollection queryString, string resource = null)
        {
            UserIsInRole = false;
            AllowedPermissions = allowedPermissions;
            Resource = resource;
            RequestType = requestType;
            Url = new Uri(url);
            QueryString = queryString;

            var principal = new Mock<IPrincipal>();
            principal.Setup(p => p.Identity.Name).Returns("User");
            principal.Setup(p => p.IsInRole(It.Is<string>(role => UserRoles.Contains(role)))).Returns(true);
            principal.Setup(p => p.IsInRole(It.Is<string>(role => !UserRoles.Contains(role)))).Returns(false);

            User = principal.Object;
        }

        public string Resource { get; set; }

        public Permissions AllowedPermissions { get; set; }
        public string[] UserRoles { get; private set; }
        public bool UserIsInRole
        {
            get
            {
                return _userIsInRole;
            }
            set
            {
                UserRoles = value ? new[] { UserRole } : new[] { OtherRole };
                _userIsInRole = value;
            }
        }

    }
}