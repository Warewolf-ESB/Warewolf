using System.Collections.Generic;

namespace Dev2.Providers.Errors
{
    public interface IActionableErrorInfo : IErrorInfo
    {
        void Do();
        KeyValuePair<string, object> PropertyNameValuePair { get; set; }
    }
}