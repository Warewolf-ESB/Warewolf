using System.IO;

namespace Dev2.Runtime.WebServer
{
    public class HttpFileStream : HttpContentStream
    {
        readonly string _fileName;

        public HttpFileStream(string fileName, int chunkSize = 65536)
            : base(chunkSize)
        {
            _fileName = fileName;
        }

        protected override Stream OpenInputStream()
        {
            return File.Open(_fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
