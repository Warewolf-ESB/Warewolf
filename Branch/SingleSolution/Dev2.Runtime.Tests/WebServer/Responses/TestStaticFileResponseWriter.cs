using System.IO;
using System.Text;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    public class TestStaticFileResponseWriter : StaticFileResponseWriter
    {
        readonly string _fileContent;

        public TestStaticFileResponseWriter(string fileContent, string contentType)
            : base("layout.htm", contentType)
        {
            _fileContent = fileContent;
        }

        protected override Stream OpenFileStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_fileContent));
        }
    }
}