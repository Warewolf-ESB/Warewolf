using System.IO;
using System.Net.Http;

namespace Dev2.Runtime.WebServer.Responses
{
    public class FileResponseWriter : IResponseWriter
    {
        readonly string _fileName;

       
        public FileResponseWriter(string fileName)
        {
            _fileName = fileName;
        }

        public void Write(WebServerContext context)
        {
            context.ResponseMessage.Content = new StreamContent(File.Open(_fileName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
        }
    }
}