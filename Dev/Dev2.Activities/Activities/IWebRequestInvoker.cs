using System;
using System.Collections.Generic;

namespace Dev2.Activities
{
    public interface IWebRequestInvoker
    {
        string ExecuteRequest(string method, string url);

        string ExecuteRequest(string method, string url, List<Tuple<string, string>> headers);

        string ExecuteRequest(string method, string url, string data, List<Tuple<string, string>> headers = null, Action<string> asyncCallback = null);
    }
}