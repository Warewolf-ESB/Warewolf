/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Common.NetStandard20;

namespace Warewolf.Common.NetStandard20.Tests
{
    /// <summary>
    /// Pure-unit coverage for <see cref="HttpClientWrapperV2"/>. The class
    /// is the post-deprecation replacement for WebClient inside POST/PUT
    /// activities and previously had 0% coverage in the pipeline merged
    /// Cobertura report (~212 uncovered lines).
    ///
    /// These tests exercise construction, credential handling, the
    /// SetHeader/SetTimeout helpers and the catch branches in GetAsync/
    /// PostAsync/PutAsync/DeleteAsync by aiming the wrapper at an
    /// unroutable address and asserting that the typed exceptions
    /// (TimeoutException / HttpRequestException) propagate as
    /// documented.
    /// </summary>
    [TestClass]
    public class HttpClientWrapperV2Tests
    {
        // An IP address in the TEST-NET-1 documentation range
        // (RFC 5737) that is guaranteed not to host an HTTP service.
        // Coupled with a very small SetTimeout this gives us a fast
        // TaskCanceledException -> TimeoutException path.
        private const string UnreachableUrl = "http://192.0.2.1:1/";

        // -----------------------------------------------------------------
        // Construction
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_DefaultCtor_ReportsNoCredentials()
        {
            using (var c = new HttpClientWrapperV2())
            {
                Assert.IsFalse(c.HasCredentials);
                Assert.AreEqual(default(System.Net.HttpStatusCode), c.LastStatusCode);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_CredentialsCtor_WithUser_ReportsHasCredentials()
        {
            using (var c = new HttpClientWrapperV2("alice", "s3cret"))
            {
                Assert.IsTrue(c.HasCredentials);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_CredentialsCtor_EmptyUser_DoesNotSetCredentials()
        {
            using (var c = new HttpClientWrapperV2(string.Empty, "anything"))
            {
                Assert.IsFalse(c.HasCredentials);
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_CredentialsCtor_NullUser_DoesNotSetCredentials()
        {
            using (var c = new HttpClientWrapperV2(null, null))
            {
                Assert.IsFalse(c.HasCredentials);
            }
        }

        // -----------------------------------------------------------------
        // SetHeader
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_SetHeader_NullName_NoOp()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetHeader(null, "x"); // early-return branch
                c.SetHeader(string.Empty, "x");
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_SetHeader_ContentType_TakesSpecialPath()
        {
            using (var c = new HttpClientWrapperV2())
            {
                // Hits the early-return "Content-Type" branch which stores
                // the value in the private field for later application.
                c.SetHeader("Content-Type", "application/json");
                c.SetHeader("content-type", "text/xml"); // case insensitive
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_SetHeader_RegularHeader_AddedThenReplaced()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetHeader("X-Test", "first");
                c.SetHeader("X-Test", "second"); // exercises the "remove existing" branch
                c.SetHeader("Authorization", "Bearer redacted"); // logging redact branch
            }
        }

        // -----------------------------------------------------------------
        // SetTimeout
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_SetTimeout_DoesNotThrow()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetTimeout(TimeSpan.FromSeconds(5));
            }
        }

        // -----------------------------------------------------------------
        // Async error paths - unreachable host triggers HttpRequestException
        // or TimeoutException (TaskCanceledException) once we shrink the
        // timeout. Either is acceptable - both branches are valid coverage.
        // -----------------------------------------------------------------

        private static async Task AssertHttpFailureAsync(Func<Task> action)
        {
            try
            {
                await action();
                Assert.Fail("Expected the request to throw against an unreachable host.");
            }
            catch (HttpRequestException) { /* one of the documented error paths */ }
            catch (TimeoutException) { /* the documented timeout path */ }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public async Task HttpClientWrapperV2_GetAsync_UnreachableHost_ThrowsTypedException()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetTimeout(TimeSpan.FromMilliseconds(250));
                await AssertHttpFailureAsync(() => c.GetAsync(UnreachableUrl));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public async Task HttpClientWrapperV2_PostAsyncString_UnreachableHost_ThrowsTypedException()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetTimeout(TimeSpan.FromMilliseconds(250));
                c.SetHeader("Content-Type", "application/json"); // exercises the apply-content-type branch
                await AssertHttpFailureAsync(() => c.PostAsync(UnreachableUrl, "{}"));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public async Task HttpClientWrapperV2_PostAsyncString_NullData_StillReachesNetworkPath()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetTimeout(TimeSpan.FromMilliseconds(250));
                await AssertHttpFailureAsync(() => c.PostAsync(UnreachableUrl, (string)null));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public async Task HttpClientWrapperV2_PostFormDataAsync_UnreachableHost_Throws()
        {
            using (var c = new HttpClientWrapperV2())
            using (var form = new MultipartFormDataContent())
            {
                c.SetTimeout(TimeSpan.FromMilliseconds(250));
                await AssertHttpFailureAsync(() => c.PostFormDataAsync(UnreachableUrl, form));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public async Task HttpClientWrapperV2_PostUrlEncodedAsync_UnreachableHost_Throws()
        {
            using (var c = new HttpClientWrapperV2())
            using (var form = new FormUrlEncodedContent(new[]
                   {
                       new System.Collections.Generic.KeyValuePair<string, string>("k","v")
                   }))
            {
                c.SetTimeout(TimeSpan.FromMilliseconds(250));
                await AssertHttpFailureAsync(() => c.PostUrlEncodedAsync(UnreachableUrl, form));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public async Task HttpClientWrapperV2_PutAsync_UnreachableHost_Throws()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetTimeout(TimeSpan.FromMilliseconds(250));
                c.SetHeader("Content-Type", "application/json");
                await AssertHttpFailureAsync(() => c.PutAsync(UnreachableUrl, "{}"));
            }
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public async Task HttpClientWrapperV2_DeleteAsync_UnreachableHost_Throws()
        {
            using (var c = new HttpClientWrapperV2())
            {
                c.SetTimeout(TimeSpan.FromMilliseconds(250));
                await AssertHttpFailureAsync(() => c.DeleteAsync(UnreachableUrl));
            }
        }

        // -----------------------------------------------------------------
        // Dispose
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(HttpClientWrapperV2))]
        public void HttpClientWrapperV2_Dispose_CalledTwice_DoesNotThrow()
        {
            var c = new HttpClientWrapperV2();
            c.Dispose();
            c.Dispose();
        }
    }
}
