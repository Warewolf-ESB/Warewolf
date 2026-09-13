/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.WebRequestInvokerTests
{
    /// <summary>
    /// Direct coverage of <see cref="WebRequestInvoker"/>, which had none before. Every caller
    /// reached it through a mocked <see cref="IWebRequestInvoker"/>, so the verb switch itself was
    /// never executed by a test and two defects shipped in it: PUT/DELETE fell through to
    /// <c>default</c> and returned an empty string without issuing any request, and POST passed a
    /// null body straight into <c>WebClient.UploadString</c>.
    ///
    /// <para>
    /// Requests go to a loopback <see cref="HttpListener"/> rather than a real host, so the tests
    /// are deterministic, need no network, and can assert what the server actually received — the
    /// only way to tell "issued a request that returned empty" apart from "issued nothing at all".
    /// </para>
    /// </summary>
    [TestClass]
    public class WebRequestInvokerTests
    {
        StubServer _server;

        [TestInitialize]
        public void Initialize() => _server = StubServer.Start();

        [TestCleanup]
        public void Cleanup() => _server?.Dispose();

        // PUT / DELETE: previously silent no-ops.

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_Put_IssuesPutAndReturnsResponse()
        {
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest("PUT", _server.Url, "put-body", null);

            Assert.AreEqual(StubServer.ResponseBody, result, "PUT returned no response body.");
            Assert.AreEqual("PUT", _server.LastMethod, "The stub server did not receive a PUT.");
            Assert.AreEqual("put-body", _server.LastBody);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_Delete_IssuesDeleteAndReturnsResponse()
        {
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest("DELETE", _server.Url, null, null);

            Assert.AreEqual(StubServer.ResponseBody, result, "DELETE returned no response body.");
            Assert.AreEqual("DELETE", _server.LastMethod, "The stub server did not receive a DELETE.");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_PutWithTimeoutOverload_IssuesPut()
        {
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest(60000, "PUT", _server.Url, "put-body", null);

            Assert.AreEqual(StubServer.ResponseBody, result);
            Assert.AreEqual("PUT", _server.LastMethod);
            Assert.AreEqual("put-body", _server.LastBody);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_DeleteWithTimeoutOverload_IssuesDelete()
        {
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest(60000, "DELETE", _server.Url, null, null);

            Assert.AreEqual(StubServer.ResponseBody, result);
            Assert.AreEqual("DELETE", _server.LastMethod);
        }

        // Null body: previously an ArgumentNullException out of WebClient.UploadString.

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_PostWithNullData_SendsEmptyBodyAndDoesNotThrow()
        {
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest("POST", _server.Url, null, null);

            Assert.AreEqual(StubServer.ResponseBody, result);
            Assert.AreEqual("POST", _server.LastMethod);
            Assert.AreEqual(string.Empty, _server.LastBody, "A null body should be sent as an empty body.");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_PostWithNullDataOnTimeoutOverload_SendsEmptyBody()
        {
            // This is the exact path DsfWebGetRequestWithTimeoutActivity takes.
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest(60000, "POST", _server.Url, null, null);

            Assert.AreEqual(StubServer.ResponseBody, result);
            Assert.AreEqual("POST", _server.LastMethod);
            Assert.AreEqual(string.Empty, _server.LastBody);
        }

        // Unknown verbs must fail loudly rather than report success having done nothing.

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_UnsupportedMethod_Throws()
        {
            var invoker = new WebRequestInvoker();

            var ex = Assert.ThrowsException<ArgumentException>(
                () => invoker.ExecuteRequest("TRACE", _server.Url, null, null));

            StringAssert.Contains(ex.Message, "TRACE");
            Assert.AreEqual(0, _server.RequestCount, "No request should have been issued for an unsupported verb.");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_UnsupportedMethodOnTimeoutOverload_Throws()
        {
            var invoker = new WebRequestInvoker();

            var ex = Assert.ThrowsException<ArgumentException>(
                () => invoker.ExecuteRequest(60000, "TRACE", _server.Url, null, null));

            StringAssert.Contains(ex.Message, "TRACE");
            Assert.AreEqual(0, _server.RequestCount);
        }

        // Verb normalisation: the verb is user-authored workflow data, not a constrained control.

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_LowerCaseVerbWithWhitespace_IsNormalised()
        {
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest(" put ", _server.Url, "body", null);

            Assert.AreEqual(StubServer.ResponseBody, result);
            Assert.AreEqual("PUT", _server.LastMethod);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(WebRequestInvoker))]
        public void WebRequestInvoker_ExecuteRequest_Get_StillIssuesGet()
        {
            var invoker = new WebRequestInvoker();

            var result = invoker.ExecuteRequest("GET", _server.Url, null, null);

            Assert.AreEqual(StubServer.ResponseBody, result);
            Assert.AreEqual("GET", _server.LastMethod);
        }

        /// <summary>
        /// Minimal loopback HTTP server that records the verb and body it was sent.
        /// </summary>
        sealed class StubServer : IDisposable
        {
            internal const string ResponseBody = "stub-response";

            readonly HttpListener _listener;
            volatile bool _stopping;

            public string Url { get; }
            public string LastMethod { get; private set; }
            public string LastBody { get; private set; }
            public int RequestCount { get; private set; }

            StubServer(HttpListener listener, string url)
            {
                _listener = listener;
                Url = url;
                var thread = new Thread(Loop) { IsBackground = true };
                thread.Start();
            }

            internal static StubServer Start()
            {
                // HttpListener does not support port 0, so probe the ephemeral range and retry on
                // collision rather than hard-coding a port another test could already own.
                var random = new Random();
                for (var attempt = 0; attempt < 20; attempt++)
                {
                    var port = random.Next(45000, 55000);
                    var url = "http://localhost:" + port + "/";
                    var listener = new HttpListener();
                    listener.Prefixes.Add(url);
                    try
                    {
                        listener.Start();
                        return new StubServer(listener, url);
                    }
                    catch (HttpListenerException)
                    {
                        ((IDisposable)listener).Dispose();
                    }
                }

                Assert.Inconclusive("Could not bind a loopback HttpListener port for the stub server.");
                return null;
            }

            void Loop()
            {
                while (!_stopping)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = _listener.GetContext();
                    }
                    catch (Exception)
                    {
                        return; // listener stopped
                    }

                    try
                    {
                        LastMethod = context.Request.HttpMethod;
                        using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                        {
                            LastBody = reader.ReadToEnd();
                        }
                        RequestCount++;

                        var buffer = Encoding.UTF8.GetBytes(ResponseBody);
                        context.Response.StatusCode = 200;
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // A listener torn down mid-request is not a test failure.
                    }
                }
            }

            public void Dispose()
            {
                _stopping = true;
                try
                {
                    _listener.Stop();
                    ((IDisposable)_listener).Dispose();
                }
                catch (Exception)
                {
                    // already disposed
                }
            }
        }
    }
}
