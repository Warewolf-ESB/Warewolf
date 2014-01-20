using System;
using System.Security.Principal;
using Microsoft.AspNet.SignalR.Hosting;

namespace Dev2.Services.Security
{
    public interface IAuthorizationRequest
    {
        Tuple<string, string> Key { get; }
        WebServerRequestType RequestType { get; }
        IPrincipal User { get; }
        Uri Url { get; }
        INameValueCollection QueryString { get; }
    }
}