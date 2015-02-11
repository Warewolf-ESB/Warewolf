
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Linq;
using System.Security.Principal;
using Dev2.Common.Interfaces.Services.Security;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hosting;
using Moq;

namespace Dev2.Tests.Runtime.Security
{
    public class TestAuthorizationRequest : AuthorizationRequest
    {
        bool _userIsInRole;
        public const string UserRole = "Test";
        public const string OtherRole = "TestOther";

        public TestAuthorizationRequest()
        {
        }

        public TestAuthorizationRequest(AuthorizationContext authorizationContext, WebServerRequestType requestType, string url, INameValueCollection queryString, string resource = null)
        {
            UserIsInRole = false;
            AuthorizationContext = authorizationContext;
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

        public AuthorizationContext AuthorizationContext { get; set; }
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
