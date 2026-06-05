namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    internal static class TestConstants
    {
        internal const string AzureFunctionBaseUrl = "http://localhost:7071/public/tools/http%20post";
        // httpbin runs in a sidecar container started by the CI pipeline (see Dev/.azure/pipeline.yml,
        // job AzureFunctionsIntegrationTests) on http://localhost:4000.  For local dev, run:
        //   docker run -d --rm --name httpbin -p 4000:8080 ghcr.io/mccutchen/go-httpbin:latest
        // or use kennethreitz/httpbin (port 80).  Don't point this at the public https://httpbin.org —
        // it's rate-limited from CI and returns HTML 502 pages that break JSON parsing.
        internal const string HttpbinBaseUrl = "http://localhost:4000";
        internal const string HttpbinHost = "localhost:4000";
        internal const string HttpbinPostUrl = HttpbinBaseUrl + "/post";
        internal const string HttpbinAnythingUrl = HttpbinBaseUrl + "/anything";
        internal const string HttpbinGetBaseUrl = "http://localhost:7071/public/tools/http%20get";
        internal const string HttpbinGetUrl = HttpbinBaseUrl + "/get";
        internal const string SystemInfoBaseUrl = "http://localhost:7071/public/tools/system%20info";
    }
}
