using System;
using System.Security.Principal;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hosting;

namespace Dev2.Runtime.WebServer.Security
{
    public class AuthorizationRequest : IAuthorizationRequest
    {
        public Tuple<string, string> Key { get { return new Tuple<string, string>(User.Identity.Name, Url.OriginalString); } }
        public WebServerRequestType RequestType { get; set; }
        public IPrincipal User { get; set; }
        public Uri Url { get; set; }
        public INameValueCollection QueryString { get; set; }
    }
}