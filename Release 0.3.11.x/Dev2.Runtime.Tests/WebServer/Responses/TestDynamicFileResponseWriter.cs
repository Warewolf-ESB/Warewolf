using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    public class TestDynamicFileResponseWriter : DynamicFileResponseWriter
    {
        readonly string _layoutFileContent;

        public TestDynamicFileResponseWriter(string layoutFileContent, string contentPathToken, string contentPath)
            : base("layout.htm", contentPathToken, contentPath)
        {
            _layoutFileContent = layoutFileContent;
        }

        protected override string ReadLayoutFile()
        {
            return _layoutFileContent;
        }
    }
}