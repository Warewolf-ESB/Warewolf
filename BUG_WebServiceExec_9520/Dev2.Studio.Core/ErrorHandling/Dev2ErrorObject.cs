using System;

namespace Dev2.Studio.Core.ErrorHandling
{
    public class Dev2ErrorObject
    {
        public string UserErrorMessage { get; set; }

        public string StackTrace { get; set; }

        public enErrorType ErrorType { get; set; }

        public Action FixAction { get; set; }

        public Dev2ErrorObject()
        {

        }
    }


}
