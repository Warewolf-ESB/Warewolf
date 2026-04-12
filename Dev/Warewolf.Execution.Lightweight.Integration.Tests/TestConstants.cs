namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    internal static class TestConstants
    {
        internal const string AzureFunctionBaseUrl = "http://localhost:7071/public/tools/http%20post";
        internal const string HttpbinBaseUrl = "https://httpbin.org";
        internal const string HttpbinHost = "httpbin.org";
        internal const string HttpbinPostUrl = HttpbinBaseUrl + "/post";
        internal const string HttpbinAnythingUrl = HttpbinBaseUrl + "/anything";
        internal const string HttpbinGetBaseUrl = "http://localhost:7071/public/tools/http%20get";
        internal const string HttpbinGetUrl = HttpbinBaseUrl + "/get";
    }
}
