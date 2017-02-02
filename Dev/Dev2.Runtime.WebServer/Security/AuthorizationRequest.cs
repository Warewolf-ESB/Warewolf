/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Principal;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hosting;

namespace Dev2.Runtime.WebServer.Security
{
    public class AuthorizationRequest : IAuthorizationRequest
    {
        public Tuple<string, string, AuthorizationContext> Key => new Tuple<string, string, AuthorizationContext>(User.Identity.Name, Url.OriginalString,AuthorizationContext.Any);
        public WebServerRequestType RequestType { get; set; }
        public IPrincipal User { get; set; }
        public Uri Url { get; set; }
        public INameValueCollection QueryString { get; set; }
    }
}
