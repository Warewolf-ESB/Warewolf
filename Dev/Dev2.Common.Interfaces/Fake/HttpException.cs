using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Interfaces
{
    public class HttpException : Exception
    {
        private string message;
        private int internalServerError;
        private string v;

        public HttpException(string message)
        {
            this.message = message;
        }

        public HttpException(int internalServerError, string v)
        {
            this.internalServerError = internalServerError;
            this.v = v;
        }
    }
}
