/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Warewolf.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [DoNotParallelize]
    public class WebSourcesTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSources_ConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
#pragma warning disable 168

            var handler = new WebSources(null);

#pragma warning restore 168
        }

        [TestMethod]
        public void WebSources_TestWithInValidArgsExpectedInvalidValidationResult()
        {
            var handler = new WebSources();
            var result = handler.Test("root:'hello'");
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void WebSources_TestWithInvalidAddressExpectedInvalidValidationResult()
        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.Anonymous }.ToString();

            var handler = new WebSources();
            var result = handler.Test(source);
            Assert.IsFalse(result.IsValid, result.ErrorMessage);
        }

        [TestMethod]
        public void WebSources_AssertUserAgentHeaderSet()
        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.Anonymous };

            WebSources.CreateWebClient(source, new List<string>());

            var client = source.Client;
            var agent = client.Headers["user-agent"];
            Assert.IsNotNull(agent);
            Assert.AreEqual(agent, GlobalConstants.UserAgentString);
        }
        [TestMethod]
        public void WebSources_AssertUserAgentHeaderSet_SetsOtherHeaders()
        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.Anonymous };

            WebSources.CreateWebClient(source, new List<string> { "a:x", "b:e" });

            var client = source.Client;
            var agent = client.Headers["user-agent"];
            Assert.IsNotNull(agent);
            Assert.AreEqual(agent, GlobalConstants.UserAgentString);
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
        }
        [TestMethod]
        public void WebSources_AssertUserAgentHeaderSet_SetsUserNameAndPassword()

        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.User, UserName = "User", Password = "pwd" };

            WebSources.CreateWebClient(source, new List<string> { "a:x", "b:e" });

            var client = source.Client;

            Assert.IsTrue((client.Credentials as NetworkCredential).UserName == "User");

            Assert.IsTrue((client.Credentials as NetworkCredential).Password == "pwd");

        }

        [TestMethod]
        public void WebSources_GetWithNullArgsExpectedReturnsNewSource()
        {
            var handler = new WebSources();
            var result = handler.Get(null, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        public void WebSources_GetWithInvalidArgsExpectedReturnsNewSource()
        {
            var handler = new WebSources();
            var result = handler.Get("xxxxx", Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WebSources_GetAddress_Given_Null_Source_And_NoRelativeUri_Should_Return_relativeUri()
        {
            //------------Setup for test-------------------------
            var webSource = new PrivateType(typeof(WebSources));
            //------------Execute Test---------------------------
            object[] args = { null, string.Empty };
            var invokeStaticResults = webSource.InvokeStatic("GetAddress", args);
            //------------Assert Results-------------------------
            Assert.IsNotNull(invokeStaticResults);
            var results = invokeStaticResults as string;
            Assert.IsNotNull(results);
            Assert.IsTrue(string.IsNullOrEmpty(results));
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void WebSources_GetAddress_Given_Null_Source_And_relativeUri_Should_Return_relativeUri()
        {
            //------------Setup for test-------------------------
            var webSource = new PrivateType(typeof(WebSources));
            //------------Execute Test---------------------------
            object[] args = { null, "some url" };
            var invokeStaticResults = webSource.InvokeStatic("GetAddress", args);
            //------------Assert Results-------------------------
            Assert.IsNotNull(invokeStaticResults);
            var results = invokeStaticResults as string;
            Assert.IsNotNull(results);
            Assert.AreEqual("some url", results);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        public void WebSources_PerformMultipartWebRequest_SetsContentTypeHeaders()
        {
            var source = new WebSource { Address = "http://www.msn.com/", AuthenticationType = AuthenticationType.Anonymous };
            WebSources.CreateWebClient(source, new List<string> { "a:x", "b:e", "Content-Type: multipart/form-data" });
            try
            {
                WebSources.PerformMultipartWebRequest(source.Client, source.Address, "");
            }
            catch(WebException e)
            {
                Assert.Fail("Error connecting to " + source.Address + "\n" + e.Message);
            }
            var client = source.Client;
            var contentType = client.Headers["Content-Type"];
            Assert.IsNotNull(contentType);
            Assert.AreEqual("multipart/form-data", contentType);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        public void WebSources_Execute_SetsContentTypeHeaders_multipart()
        {
            var headerString = new List<string> { "a:x", "b:e", "Content-Type: multipart/form-data" };
            var source = new WebSource { Address = "http://www.msn.com/", AuthenticationType = AuthenticationType.Anonymous };

            WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, headerString.ToArray());

            var client = source.Client;
            var contentType = client.Headers["Content-Type"];
            Assert.IsNotNull(contentType);
            Assert.IsFalse(errors.HasErrors(), "On executing with multipart form data web client source returned at least one error: " + (errors.HasErrors()?errors.FetchErrors()[0]:""));
            Assert.AreEqual("multipart/form-data", contentType);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        public void WebSources__Execute_Without_MultiPart()
        {
            var headerString = new List<string> { "a:x", "b:e" };
            var source = new WebSource { Address = "http://www.msn.com/", AuthenticationType = AuthenticationType.Anonymous };

            WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, headerString.ToArray());

            var client = source.Client;
            Assert.IsFalse(errors.HasErrors(), "On executing without multipart form data web client source returned at least one error: " + (errors.HasErrors()?errors.FetchErrors()[0]:""));
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        public void WebSources_HttpNewLine()
        {
            const string data = @"Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate
Cookie: __atuvc=34%7C7; permanent=0; _gitlab_session=226ad8a0be43681acf38c2fab9497240; __profilin=p%3Dt; request_method=GET
Connection: keep-alive
Content-Type: multipart/form-data; boundary=---------------------------9051914041544843365972754266
Content-Length: 554

-----------------------------9051914041544843365972754266
Content-Disposition: form-data; name=""text""

text default
-----------------------------9051914041544843365972754266
Content-Disposition: form-data; name=""file1""; filename=""a.txt""
Content-Type: text/plain

Content of a.txt.

-----------------------------9051914041544843365972754266
Content-Disposition: form-data; name=""file2""; filename=""a.html""
Content-Type: text/html

<!DOCTYPE html><title>Content of a.html.</title>

-----------------------------9051914041544843365972754266--";
            var rData = data;
            var byteData = WebSources.ConvertToHttpNewLine(ref rData);

            string expected = data;
            string result = Encoding.UTF8.GetString(byteData);

            Assert.AreEqual(expected, result);

            rData = data.Replace("\r\n", "\n");
            byteData = WebSources.ConvertToHttpNewLine(ref rData);

            result = Encoding.UTF8.GetString(byteData);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void WebSources_Execute_WebRequestMethod_Get_ExpectExpectBase64String()
        {
            var headerString = new List<string> { "a:x", "b:e" };
            var source = new WebSource { Address = "http://www.msn.com/", AuthenticationType = AuthenticationType.Anonymous };

            var result = WebSources.Execute(source, WebRequestMethod.Get, "http://www.msn.com/", "", false, out var errors, headerString.ToArray());

            Assert.IsTrue(result.IsBase64());

            var client = source.Client;
            Assert.IsFalse(errors.HasErrors(), "On executing without multipart form data web client source returned at least one error: " + (errors.HasErrors() ? errors.FetchErrors()[0] : ""));
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Put_ExpectExpectBase64String()
        {
            var headerString = new List<string> { "a:x", "b:e" };
            var source = new WebSource { Address = "http://www.msn.com/", AuthenticationType = AuthenticationType.Anonymous };

            var result = WebSources.Execute(source, WebRequestMethod.Put, "http://www.msn.com/", "", false, out var errors, headerString.ToArray());

            Assert.IsTrue(result.IsBase64());

            var client = source.Client;
            Assert.IsFalse(errors.HasErrors(), "On executing without multipart form data web client source returned at least one error: " + (errors.HasErrors() ? errors.FetchErrors()[0] : ""));
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
        }
    }
}
