using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Dev2.Runtime.WebServer
{
    public interface ICommunicationRequest
    {
        string Method { get; }
        Uri Uri { get; }

        int ContentLength { get; }
        string ContentType { get; }
        Encoding ContentEncoding { get; }
        Stream InputStream { get; }

        NameValueCollection QueryString { get; }
        NameValueCollection BoundVariables { get; }
    }
}
