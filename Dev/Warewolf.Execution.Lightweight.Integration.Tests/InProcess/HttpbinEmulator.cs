/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  In-process httpbin emulator for the Web GET/POST integration tests.
 *
 *  The TC and named Web-tool workflows reference two WebSource fixtures whose address has
 *  been repointed from https://httpbin.org to http://localhost:4000 (see
 *  Resources/tools/http {get,post}/httpbin.bite). This fixture stands up a WireMock.Net
 *  server on port 4000 that emulates the subset of httpbin.org's echo API the tests rely on,
 *  removing the external-network dependency and the associated flakiness.
 *
 *  Emulated endpoints: /get, /post, /anything (any method). The response mirrors httpbin's
 *  shape — args, headers, json, form, files, data, method, origin, url — echoing the request
 *  back. To satisfy the assertions (which were written against real httpbin), the echoed
 *  `headers.Host` and `url` host are hard-pinned to httpbin.org regardless of the real
 *  localhost:4000 endpoint.
 *
 *  Runs once per test assembly via [AssemblyInitialize]/[AssemblyCleanup].
 */

using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using WireMock;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests.InProcess
{
    /// <summary>
    /// Assembly-wide httpbin emulator listening on the port baked into the WebSource
    /// fixtures (<see cref="Port"/>).
    ///
    /// Lifecycle is driven by the single assembly fixture (<c>IntegrationTestAssemblyInit</c>),
    /// which calls <see cref="Start"/>/<see cref="Stop"/>. MSTest permits only one
    /// <c>[AssemblyInitialize]</c> per assembly, so this is a plain helper — not a fixture.
    /// </summary>
    internal static class HttpbinEmulator
    {
        /// <summary>Must match the port in the repointed httpbin.bite WebSource fixtures.</summary>
        public const int Port = 4000;

        private const string HttpbinHost = "httpbin.org";
        private const string HttpbinBase = "https://httpbin.org";

        private static WireMockServer? _server;

        public static void Start()
        {
            if (_server != null)
                return;

            _server = WireMockServer.Start(Port);

            // Single catch-all stub — the response factory inspects the request and
            // produces an httpbin-shaped echo for whichever path/method was called.
            _server
                .Given(Request.Create().WithPath(new WildcardMatcher("*")).UsingAnyMethod())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody(BuildHttpbinResponse));
        }

        public static void Stop()
        {
            _server?.Stop();
            _server = null;
        }

        // ── httpbin echo emulation ──────────────────────────────────────────────

        private static string BuildHttpbinResponse(IRequestMessage req)
        {
            var contentType = GetHeader(req, "Content-Type") ?? string.Empty;
            var result = new JObject();

            // args — query string parameters (values as strings, mirroring httpbin).
            var args = new JObject();
            if (req.Query != null)
            {
                foreach (var kv in req.Query)
                    args[kv.Key] = kv.Value?.LastOrDefault() ?? string.Empty;
            }
            result["args"] = args;

            // headers — echo every received header; Host is pinned to httpbin.org so
            // assertions that expect the real service host still pass.
            var headers = new JObject();
            if (req.Headers != null)
            {
                foreach (var kv in req.Headers)
                    headers[kv.Key] = string.Join(",", kv.Value);
            }
            headers["Host"] = HttpbinHost;
            result["headers"] = headers;

            result["origin"] = "127.0.0.1";
            result["method"] = req.Method;

            // url — reconstructed against the httpbin host (not the localhost mock).
            var pathAndQuery = new System.Uri(req.Url).PathAndQuery;
            result["url"] = HttpbinBase + pathAndQuery;

            // Body-dependent sections. WireMock surfaces text bodies (json / urlencoded)
            // via req.Body, but multipart/form-data is detected as binary and arrives in
            // req.BodyAsBytes with req.Body null — fall back to decoding the raw bytes.
            var body = req.Body;
            if (string.IsNullOrEmpty(body) && req.BodyAsBytes != null)
                body = System.Text.Encoding.UTF8.GetString(req.BodyAsBytes);
            body ??= string.Empty;
            result["data"] = string.Empty;
            result["json"] = JValue.CreateNull();
            var form = new JObject();
            var files = new JObject();

            if (contentType.Contains("application/json"))
            {
                result["data"] = body;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    try { result["json"] = JToken.Parse(body); }
                    catch { result["json"] = JValue.CreateNull(); }
                }
            }
            else if (contentType.Contains("application/x-www-form-urlencoded"))
            {
                foreach (var pair in body.Split('&', System.StringSplitOptions.RemoveEmptyEntries))
                {
                    var idx = pair.IndexOf('=');
                    var key = idx >= 0 ? System.Uri.UnescapeDataString(pair.Substring(0, idx)) : System.Uri.UnescapeDataString(pair);
                    var val = idx >= 0 ? System.Uri.UnescapeDataString(pair.Substring(idx + 1)) : string.Empty;
                    form[key] = val;
                }
            }
            else if (contentType.Contains("multipart/form-data"))
            {
                ParseMultipart(body, ExtractBoundary(contentType), form, files);
            }

            result["form"] = form;
            result["files"] = files;

            return result.ToString();
        }

        private static void ParseMultipart(string body, string? boundary, JObject form, JObject files)
        {
            if (string.IsNullOrEmpty(boundary) || string.IsNullOrEmpty(body))
                return;

            var delimiter = "--" + boundary;
            var sections = body.Split(new[] { delimiter }, System.StringSplitOptions.None);

            foreach (var raw in sections)
            {
                var part = raw.Trim('\r', '\n');
                if (part.Length == 0 || part == "--")
                    continue;

                var sep = part.IndexOf("\r\n\r\n", System.StringComparison.Ordinal);
                var sepLen = 4;
                if (sep < 0)
                {
                    sep = part.IndexOf("\n\n", System.StringComparison.Ordinal);
                    sepLen = 2;
                }
                if (sep < 0)
                    continue;

                var headerBlock = part.Substring(0, sep);
                var content = part.Substring(sep + sepLen).TrimEnd('\r', '\n');

                // Content-Disposition values may be quoted ("name") or bare (name) — the
                // Warewolf client emits them unquoted. \b ahead of the key prevents the
                // "name" pattern from matching inside "filename".
                var name = ExtractDispositionValue(headerBlock, "name");
                if (name == null)
                    continue;

                var filename = ExtractDispositionValue(headerBlock, "filename");
                if (filename != null)
                    files[name] = content;
                else
                    form[name] = content;
            }
        }

        private static string? ExtractDispositionValue(string headerBlock, string key)
        {
            var match = Regex.Match(
                headerBlock,
                "\\b" + key + "=(?:\"([^\"]*)\"|([^;\\r\\n]+))",
                RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            return (match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value).Trim();
        }

        private static string? ExtractBoundary(string contentType)
        {
            var match = Regex.Match(contentType, "boundary=(.+)$");
            return match.Success ? match.Groups[1].Value.Trim('"', ' ') : null;
        }

        private static string? GetHeader(IRequestMessage req, string name)
        {
            if (req.Headers == null)
                return null;

            foreach (var kv in req.Headers)
            {
                if (string.Equals(kv.Key, name, System.StringComparison.OrdinalIgnoreCase))
                    return kv.Value?.FirstOrDefault();
            }
            return null;
        }
    }
}
