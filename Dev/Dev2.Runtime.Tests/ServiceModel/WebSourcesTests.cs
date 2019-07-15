/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
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
            var result = handler.Get(null,  Guid.Empty);

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
            WebSources.PerformMultipartWebRequest(source.Client, source.Address, "");
            var client = source.Client;
            var contentType = client.Headers["Content-Type"];
            Assert.IsNotNull(contentType);
            Assert.AreEqual("multipart/form-data",contentType);
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
            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("multipart/form-data",contentType);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        public void WebSources__Execute_Without_MultiPart()
        {
            var headerString = new List<string> { "a:x", "b:e" };
            var source = new WebSource { Address = "http://www.msn.com/", AuthenticationType = AuthenticationType.Anonymous };

            WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, headerString.ToArray());

            var client = source.Client;
            Assert.IsFalse(errors.HasErrors());
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
        }
    }
}
