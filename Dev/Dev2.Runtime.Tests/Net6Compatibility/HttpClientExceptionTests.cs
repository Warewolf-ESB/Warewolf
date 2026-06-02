/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Net;
using System.Net.Http;
using Dev2.Net6.Compatibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Net6Compatibility
{
    /// <summary>
    /// Unit tests for <see cref="HttpClientException"/> covering every constructor and the
    /// private <c>GetExceptionMessage</c> helper invoked via the response-message ctor.
    /// </summary>
    [TestClass]
    public class HttpClientExceptionTests
    {
        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(HttpClientException))]
        public void HttpClientException_DefaultConstructor_HasNoMessageOrResponse()
        {
            var ex = new HttpClientException();
            Assert.IsNotNull(ex.Message); // base default message is non-null
            Assert.IsNull(ex.Response);
            Assert.IsNull(ex.InnerException);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(HttpClientException))]
        public void HttpClientException_MessageConstructor_StoresMessage()
        {
            var ex = new HttpClientException("boom");
            Assert.AreEqual("boom", ex.Message);
            Assert.IsNull(ex.Response);
            Assert.IsNull(ex.InnerException);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(HttpClientException))]
        public void HttpClientException_MessageAndInnerConstructor_StoresBoth()
        {
            var inner = new InvalidOperationException("cause");
            var ex = new HttpClientException("outer", inner);
            Assert.AreEqual("outer", ex.Message);
            Assert.AreSame(inner, ex.InnerException);
            Assert.IsNull(ex.Response);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(HttpClientException))]
        public void HttpClientException_ResponseMessageConstructor_DerivesMessageFromResponse()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                ReasonPhrase = "Not Found"
            };

            var ex = new HttpClientException(response);

            Assert.AreSame(response, ex.Response);
            // The implementation uses HttpResponseMessage.ToString() which always contains
            // the StatusCode value in its formatted output.
            StringAssert.Contains(ex.Message, "404");
            StringAssert.Contains(ex.Message, "Not Found");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(HttpClientException))]
        public void HttpClientException_ResponseMessageConstructor_NullResponse_ThrowsArgumentNullException()
        {
            // GetExceptionMessage throws when responseMessage is null.
            Assert.ThrowsException<ArgumentNullException>(
                () => new HttpClientException((HttpResponseMessage)null));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(HttpClientException))]
        public void HttpClientException_IsAnException()
        {
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(typeof(HttpClientException)));
        }
    }
}
