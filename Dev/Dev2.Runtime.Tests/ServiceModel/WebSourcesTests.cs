using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    [ExcludeFromCodeCoverage]
    public class WebSourcesTests
    {
        const string TestMethod = "GetCitiesByCountry";
        const string CountryName = "South%20Africa";
        const string TestAddress = "http://www.webservicex.net/globalweather.asmx";
        const string TestDefaultQuery = "/" + TestMethod + "?CountryName=" + CountryName;

        #region Ignored test methods

        //[TestMethod]
        //[Ignore]
        //// Ignore because this test may flicker but here for manual testing!
        //public void WebSourcesExecuteWithPostExpectedReturnsResult()
        //{
        //    var source = CreateWebSource();
        //    try
        //    {
        //        ErrorResultTO errors;
        //        var result = WebSources.Execute(source, WebRequestMethod.Post, "/" + TestMethod, "CountryName=" + CountryName, false, out errors);
        //        Assert.IsNotNull(result);
        //    }
        //    finally
        //    {
        //        source.Dispose();
        //    }
        //}

        //[TestMethod]
        //[Ignore]
        //// Ignore because this test may flicker but here for manual testing!
        //public void WebSourcesTestWithValidArgsExpectedValidValidationResult()
        //{
        //    var source = CreateWebSource();
        //    source.Address = source.Address;
        //    source.DefaultQuery = TestDefaultQuery;

        //    var handler = new WebSources();
        //    var result = handler.Test(source.ToString(), Guid.Empty, Guid.Empty);
        //    Assert.IsTrue(result.IsValid, result.ErrorMessage);
        //}

        //[TestMethod]
        //[Ignore]
        //// Ignore because this test may flicker but here for manual testing!
        //public void WebSourcesTestWithInvalidCredentialsExpectedInvalidValidationResult()
        //{
        //    var source = new WebSource
        //    {
        //        Address = "https://rsaklfsvrsbspdc.dev2.local/ews/services.wsdl",
        //        AuthenticationType = AuthenticationType.Anonymous
        //    }.ToString();

        //    var handler = new WebSources();
        //    var result = handler.Test(source, Guid.Empty, Guid.Empty);
        //    Assert.IsFalse(result.IsValid, result.ErrorMessage);
        //    Assert.AreEqual("The remote server returned an error: (401) Unauthorized.", result.ErrorMessage.Trim());
        //}


        //[TestMethod]
        //[Ignore]
        //// Ignore because this test may flicker but here for manual testing!
        //public void WebSourcesTestWithValidCredentialsExpectedReturnsResult()
        //{
        //    var source = new WebSource
        //    {
        //        Address = "https://rsaklfsvrsbspdc.dev2.local/ews/services.wsdl",
        //        AuthenticationType = AuthenticationType.User,
        //        UserName = "dev2test",
        //        Password = "Password1"
        //    }.ToString();

        //    var handler = new WebSources();
        //    var result = handler.Test(source, Guid.Empty, Guid.Empty);
        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.IsValid);
        //}

        #endregion

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

    }
}
