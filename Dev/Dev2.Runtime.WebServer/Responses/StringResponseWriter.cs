using System.Text;
using Dev2.Runtime.WebServer.Controllers;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StringResponseWriter : ResponseWriter
    {
        readonly string _text;
        readonly string _contentType;

        public StringResponseWriter(string text, string contentType)
        {
            _text = text;
            _contentType = contentType;
        }

        public override void Write(ICommunicationContext context)
        {
            var buffer = Encoding.UTF8.GetBytes(_text);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.ContentType = _contentType;
            context.Response.ContentLength = buffer.Length;
        }

        public override void Write(WebServerContext context)
        {
            var stream = new HttpTextStream(_text);
            context.ResponseMessage.Content = stream.CreatePushStreamContent(_contentType);
        }
    }
}