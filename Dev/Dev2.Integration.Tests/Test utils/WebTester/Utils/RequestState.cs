using System;
using System.IO;
using System.Net;
using System.Text;

namespace Dev2.Integration.Tests.MEF.WebTester
{
    public class RequestState
    {
        const int _bufferSize = 1024;
        public StringBuilder _requestData;
        public byte[] _bufferRead;
        public WebRequest _request;
        public Stream _responseStream;

        public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

        public RequestState()
        {
            _bufferRead = new byte[_bufferSize];
            _requestData = new StringBuilder(String.Empty);
            _request = null;
            _responseStream = null;
        }

        public RequestState(WebRequest request)
        {
            _bufferRead = new byte[_bufferSize];
            _requestData = new StringBuilder(String.Empty);
            _request = request;
        }
    }
}
