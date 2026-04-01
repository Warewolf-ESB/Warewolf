namespace Warewolf.Execution.Lightweight.Tests
{
    internal static class TestConstants
    {
        internal const string AzureFunctionBaseUrl = "http://localhost:7071/public/tools/http%20post";
        internal const string HttpbinBaseUrl = "https://httpbin.org";//"http://localhost:4000";
        internal const string HttpbinHost = "httpbin.org";//"localhost:4000";
        internal const string HttpbinPostUrl = HttpbinBaseUrl + "/post";
        internal const string HttpbinAnythingUrl = HttpbinBaseUrl + "/anything";
        internal const string HttpbinGetBaseUrl = "http://localhost:7071/public/tools/http%20get";
        internal const string HttpbinGetUrl = HttpbinBaseUrl + "/get";
    }
}
