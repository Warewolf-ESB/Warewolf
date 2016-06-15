
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel
{
    // PBI 953 - 2013.05.16 - TWR - Created
    [TestClass]    
    public class WebSourcesTests
    {
        // ReSharper disable InconsistentNaming
        const string TestMethod = "GetCitiesByCountry";
        const string CountryName = "South%20Africa";
        const string TestAddress = "http://www.webservicex.net/globalweather.asmx";
        const string TestDefaultQuery = "/" + TestMethod + "?CountryName=" + CountryName;

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSourcesConstructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
#pragma warning disable 168
            // ReSharper disable UnusedVariable
            var handler = new WebSources(null);
            // ReSharper restore UnusedVariable
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

            WebSources.EnsureWebClient(source, new List<string>());

            var client = source.Client;
            var agent = client.Headers["user-agent"];
            Assert.IsNotNull(agent);
            Assert.AreEqual(agent,GlobalConstants.UserAgentString);
        }
        [TestMethod]
        public void WebSourcesAssertUserAgentHeaderSet_SetsOtherHeaders()
        {
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.Anonymous };

            WebSources.EnsureWebClient(source, new List<string> { "a:x", "b:e" });

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
            var source = new WebSource { Address = "www.foo.bar", AuthenticationType = AuthenticationType.User,UserName = "User",Password = "pwd"};

            WebSources.EnsureWebClient(source, new List<string> { "a:x", "b:e" });

            var client = source.Client;
            // ReSharper disable PossibleNullReferenceException
            Assert.IsTrue((client.Credentials as NetworkCredential).UserName == "User");
          
            Assert.IsTrue((client.Credentials as NetworkCredential).Password == "pwd");
            // ReSharper restore PossibleNullReferenceException
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
        public void WebSourcesGetWithValidArgsExpectedReturnsSource()
        {
            var expected = CreateWebSource();
            var saveArgs = expected.ToString();

            var workspaceID = Guid.NewGuid();
            var workspacePath = EnvironmentVariables.GetWorkspacePath(workspaceID);
            try
            {
                var handler = new WebSources();
                handler.Save(saveArgs, workspaceID, Guid.Empty);

                var actual = handler.Get(expected.ResourceID.ToString(), workspaceID, Guid.Empty);

                VerifySource(actual, expected);
            }
            finally
            {
                try
                {
                    if(Directory.Exists(workspacePath))
                    {
                        DirectoryHelper.CleanUp(workspacePath);
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch(Exception)
                // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }
        }

        #endregion

        #region Save

        [TestMethod]
        public void WebSourcesSaveWithInValidArgsExpectedInvalidValidationResult()
        {
            var handler = new WebSources();
            var jsonResult = handler.Save("root:'hello'", Guid.Empty, Guid.Empty);
            var result = JsonConvert.DeserializeObject<ValidationResult>(jsonResult);
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void WebSourcesSaveWithValidArgsExpectedInvokesResourceCatalogSave()
        {
            var expected = CreateWebSource();

            var catalog = new Mock<IResourceCatalog>();
            catalog.Setup(c => c.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            var handler = new WebSources(catalog.Object);
            var jsonResult = handler.Save(expected.ToString(), Guid.Empty, Guid.Empty);
            var actual = JsonConvert.DeserializeObject<WebSource>(jsonResult);

            catalog.Verify(c => c.SaveResource(It.IsAny<Guid>(), It.IsAny<IResource>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            VerifySource(expected, actual);
        }

        #endregion

        #region VerifySource

        static void VerifySource(WebSource actual, WebSource expected)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.ResourceID, actual.ResourceID);
            Assert.AreEqual(expected.ResourceName, actual.ResourceName);
            Assert.AreEqual(expected.ResourcePath, actual.ResourcePath);
            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.ResourceType, actual.ResourceType);

            Assert.AreEqual(expected.Address, actual.Address);
            Assert.AreEqual(expected.DefaultQuery, actual.DefaultQuery);
            Assert.AreEqual(expected.AuthenticationType, actual.AuthenticationType);
            Assert.AreEqual(expected.UserName, actual.UserName);
            Assert.AreEqual(expected.Password, actual.Password);
            Assert.IsNull(actual.Response);
        }

        #endregion

        #region CreateWebSource

        static WebSource CreateWebSource()
        {
            return new WebSource
            {
                Address = TestAddress,
                DefaultQuery = TestDefaultQuery,
                AuthenticationType = AuthenticationType.Anonymous,
                UserName = "",
                Password = "",

                ResourceID = Guid.NewGuid(),
                ResourceName = "TestWeather",
                ResourcePath = "Testing\\TestWeather",
            };
        }

        #endregion
        // ReSharper restore InconsistentNaming
    }
}
