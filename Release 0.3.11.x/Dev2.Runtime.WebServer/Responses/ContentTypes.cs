using System.Net.Http.Headers;

namespace Dev2.Runtime.WebServer.Responses
{
    public static class ContentTypes
    {
        public static readonly MediaTypeHeaderValue Html = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
        public static readonly MediaTypeHeaderValue Xml = MediaTypeHeaderValue.Parse("text/xml");
        public static readonly MediaTypeHeaderValue Plain = MediaTypeHeaderValue.Parse("text/plain");
        public static readonly MediaTypeHeaderValue Json = MediaTypeHeaderValue.Parse("application/json");

        public static readonly MediaTypeHeaderValue ForceDownload = MediaTypeHeaderValue.Parse("application/force-download; charset=utf-8");
    }
}