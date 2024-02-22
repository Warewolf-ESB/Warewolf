using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common
{
    public class HttpClientException : Exception
    {
        public HttpResponseMessage Response { get; private set; }

        public HttpClientException()
        {
        }

        public HttpClientException(string message)
            : base(message)
        {
        }

        public HttpClientException(string message, Exception ex)
            : base(message, ex)
        {
        }

        public HttpClientException(HttpResponseMessage responseMessage)
            : this(GetExceptionMessage(responseMessage))
        {
            Response = responseMessage;
        }

        private static string GetExceptionMessage(HttpResponseMessage responseMessage)
        {
            if (responseMessage == null)
            {
                throw new ArgumentNullException(nameof(responseMessage));
            }

            return responseMessage.ToString();
        }
    }
}
