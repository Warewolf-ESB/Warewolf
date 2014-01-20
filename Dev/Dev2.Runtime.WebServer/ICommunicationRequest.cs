using System;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace Dev2.Runtime.WebServer
{
    public interface ICommunicationRequest
    {
        string Method { get; }
        Uri Uri { get; }
        IPrincipal User { get; set; }
        int ContentLength { get; }
        string ContentType { get; }
        Encoding ContentEncoding { get; }
        Stream InputStream { get; }

        NameValueCollection QueryString { get; }
        NameValueCollection BoundVariables { get; }
    }
}
