using System;
using Dev2.Common.Interfaces.Infrastructure;

namespace Dev2.Providers.Errors
{
    public class ErrorInfo : IErrorInfo
    {
        public Guid InstanceID { get; set; }

        public string Message { get; set; }

        public string StackTrace { get; set; }

        public ErrorType ErrorType { get; set; }

        public FixType FixType { get; set; }

        public string FixData { get; set; }

    }
}
