using System.IO;

namespace Dev2.Runtime.WebServer
{
    public class HttpTextStream : HttpContentStream
    {
        readonly string _text;

        public HttpTextStream(string text, int chunkSize = 65536)
            : base(chunkSize)
        {
            _text = text;
        }

        protected override Stream OpenInputStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(_text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}