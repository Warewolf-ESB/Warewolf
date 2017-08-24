/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ServiceModel
{
    // PBI 953 - 2013.05.16 - TWR - Created
    [TestClass]
    public class WebSourcesTests
    {
        
        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSourcesConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
#pragma warning disable 168
            
            var handler = new WebSources(null);
            
#pragma warning restore 168
        }

        #endregion

        #region Test

        [TestMethod]
        public void WebSourcesTestWithInValidArgsExpectedInvalidValidationResult()
        {
            var handler = new WebSources();
            var result = handler.Test("root:'hello'", Guid.Empty, Guid.Empty);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void WebSourcesTestWithInvalidAddressExpectedInvalidValidationResult()
        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.Anonymous }.ToString();

            var handler = new WebSources();
            var result = handler.Test(source, Guid.Empty, Guid.Empty);
            Assert.IsFalse(result.IsValid, result.ErrorMessage);
        }

        [TestMethod]
        public void WebSourcesAssertUserAgentHeaderSet()
        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.Anonymous };

            WebSources.CreateWebClient(source, new List<string>());

            var client = source.Client;
            var agent = client.Headers["user-agent"];
            Assert.IsNotNull(agent);
            Assert.AreEqual(agent, GlobalConstants.UserAgentString);
        }
        [TestMethod]
        public void WebSourcesAssertUserAgentHeaderSet_SetsOtherHeaders()
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

        public void WebSourcesAssertUserAgentHeaderSet_SetsUserNameAndPassword()

        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.User, UserName = "User", Password = "pwd" };

            WebSources.CreateWebClient(source, new List<string> { "a:x", "b:e" });

            var client = source.Client;
            
            Assert.IsTrue((client.Credentials as NetworkCredential).UserName == "User");

            Assert.IsTrue((client.Credentials as NetworkCredential).Password == "pwd");
            
        }

        #endregion

        #region Get

        [TestMethod]
        public void WebSourcesGetWithNullArgsExpectedReturnsNewSource()
        {
            var handler = new WebSources();
            var result = handler.Get(null, Guid.Empty, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        public void WebSourcesGetWithInvalidArgsExpectedReturnsNewSource()
        {
            var handler = new WebSources();
            var result = handler.Get("xxxxx", Guid.Empty, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetAddress_Given_Null_Source_And_NoRelativeUri_Should_Return_relativeUri()
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
        public void GetAddress_Given_Null_Source_And_relativeUri_Should_Return_relativeUri()
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

        #endregion
    }
}
