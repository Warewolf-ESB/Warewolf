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
using Dev2.Data.TO;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Text;
using Warewolf.Common.Interfaces.NetStandard20;
using Warewolf.Common.NetStandard20;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [DoNotParallelize]
    public class WebSourcesTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_ConstructorWithNullResourceCatalog_ExpectedThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(()=> new WebSources(null));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Constructor_With_NotNullResourceCatalog_ExpectedSuccess()
        {
            const string xmlString = @"<Source ID=""1a82a341-b678-4992-a25a-39cdd57198d4"" Name=""Example Redis Source"" ResourceType=""RedisSource"" IsValid=""false"" 
                                        ConnectionString=""HostName=localhost;Port=6379;UserName=warewolf;Password=;AuthenticationType=Anonymous"" Type=""RedisSource"" ServerVersion=""1.4.1.27"" ServerID=""693ca20d-fb17-4044-985a-df3051d6bac7"">
                                          <DisplayName>Example Redis Source</DisplayName>
                                          <AuthorRoles>
                                          </AuthorRoles>
                                          <ErrorMessages />
                                          <TypeOf>RedisSource</TypeOf>
                                          <VersionInfo DateTimeStamp=""2017-05-26T14:21:24.3247847+02:00"" Reason="""" User=""NT AUTHORITY\SYSTEM"" VersionNumber=""3"" ResourceId=""1a82a341-b678-4992-a25a-39cdd57198d4"" VersionId=""b1a6de00-3cac-41cd-b0ed-9fac9bb61266"" />
                                        </Source>";

            var resourceId = Guid.Parse("1a82a341-b678-4992-a25a-39cdd57198d4");
            
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetResourceContents(Guid.Empty, resourceId))
                .Returns(new StringBuilder(xmlString));

            var sut = new WebSources(mockResourceCatalog.Object);

            var result = sut.Get(resourceId.ToString(), Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(resourceId, result.ResourceID);
            Assert.AreEqual("Example Redis Source", result.ResourceName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Test_WithInValidArgs_ExpectedInvalidValidationResult()
        {
            var handler = new WebSources();
            var result = handler.Test("root:'hello'");
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Test_WithInValidArgs_ExpectedValidValidationResult()
        {
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();
            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });

            var source = new WebSource
            {
                Client = mockWebClientWrapper.Object
            };

            var handler = new WebSources();
            var result = handler.Test(source);
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Test_WithInValidArgs_And_WebRequestFails_ExpectedValidInvalidationResult()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.DownloadData(It.IsAny<string>()))
                .Throws(new WebException("test: false webexception"));

            var source = new WebSource
            {
                Client = mockWebClientWrapper.Object
            };

            var handler = new WebSources();
            var result = handler.Test(source);
            Assert.IsFalse(result.IsValid, result.ErrorMessage);
            Assert.AreEqual("test: false webexception", result.ErrorMessage.Trim());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Test_WithInValidArgs_And_SourceExecuteFails_ExpectedValidInvalidationResult()
        {
            var mockWebSource = new Mock<IWebSource>();
            mockWebSource.Setup(o => o.Client)
                .Throws(new Exception("test: false exception"));

            var handler = new WebSources();
            var result = handler.Test(mockWebSource.Object);
            Assert.IsFalse(result.IsValid, result.ErrorMessage);
            Assert.AreEqual("test: false exception", result.ErrorMessage.Trim());

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Test_WithInvalidAddress_ExpectedInvalidValidationResult()
        {
            var source = new WebSource { AuthenticationType = AuthenticationType.Anonymous }.ToString();

            var handler = new WebSources();
            var result = handler.Test(source);
            Assert.IsFalse(result.IsValid, result.ErrorMessage);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_AssertUserAgentHeaderSet()
        {
            var source = new WebSource 
            { 
                Address = "www.foo.bar", 
                AuthenticationType = AuthenticationType.Anonymous,
                Client = new WebClientWrapper
                {
                    Headers = new WebHeaderCollection
                    {
                        "a:x", 
                        "b:e"
                    }
                }
            };

            _= WebSources.Execute(source, WebRequestMethod.Get, "http://consoto.com", "test data", false, out ErrorResultTO error);

            var client = source.Client;
            var agent = client.Headers["user-agent"];
            Assert.AreEqual(agent, GlobalConstants.UserAgentString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_AssertUserAgentHeaderSet_SetsOtherHeaders()
        {
            var source = new WebSource
            {
                Address = "www.foo.bar",
                AuthenticationType = AuthenticationType.User,
                UserName = "User",
                Password = "pwd",
                Client = new WebClientWrapper
                {
                    Headers = new WebHeaderCollection
                    {
                        "a:x"
                    }
                }
            };

            _= WebSources.Execute(source, WebRequestMethod.Get, "http://consoto.com", "test data", false, out ErrorResultTO error, new string[] { "b:e" });

            var client = source.Client;
            var agent = client.Headers["user-agent"];
            Assert.AreEqual(agent, GlobalConstants.UserAgentString);
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_AssertUserAgentHeaderSet_SetsUserNameAndPassword()
        {
            var source = new WebSource
            {
                Address = "www.foo.bar",
                AuthenticationType = AuthenticationType.User,
                UserName = "User",
                Password = "pwd",
                Client = new WebClientWrapper
                {
                    Headers = new WebHeaderCollection
                    {
                        "a:x",
                        "b:e"
                    }
                }
            };

            _ = WebSources.Execute(source, WebRequestMethod.Get, "http://consoto.com", "test data", false, out ErrorResultTO error);

            var client = source.Client;
            Assert.IsTrue((client.Credentials as NetworkCredential).UserName == "User");
            Assert.IsTrue((client.Credentials as NetworkCredential).Password == "pwd");

        }

        [TestMethod]
        [TestCategory(nameof(WebSources))]
        public void WebSources_GetWithNullArgsExpectedReturnsNewSource()
        {
            var handler = new WebSources();
            var result = handler.Get(null, Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        [TestCategory(nameof(WebSources))]
        public void WebSources_GetWithInvalidArgsExpectedReturnsNewSource()
        {
            var handler = new WebSources();
            var result = handler.Get("xxxxx", Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        [TestCategory(nameof(WebSources))]
        public void WebSources_GetWithInvalidArgsExpectedReturnsNewSource1()
        {
            var handler = new WebSources();
            var result = handler.Get("xxxxx", Guid.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(Guid.Empty, result.ResourceID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(WebSources))]
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
        [TestCategory(nameof(WebSources))]
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_PerformMultipartWebRequest_SetsContentTypeHeaders()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = new WebClientWrapper 
                {
                    Headers = new WebHeaderCollection 
                    {
                        "a:x", 
                        "b:e", 
                        "Content-Type: multipart/form-data"
                    }
                }
            };

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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_SetsContentTypeHeaders_multipart()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e", "Content-Type: multipart/form-data" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            _= WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, new string[] { });

            var client = source.Client;
            var contentType = client.Headers["Content-Type"];
            Assert.IsNotNull(contentType);
            Assert.AreEqual("multipart/form-data", contentType);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_Without_MultiPart()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            _= WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, new string[] { });

            var client = source.Client;
            Assert.IsFalse(errors.HasErrors(), "On executing without multipart form data web client source returned at least one error: " + (errors.HasErrors()?errors.FetchErrors()[0]:""));
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_HttpNewLine()
        {
            const string data = 
                @"Accept-Language: en-US,en;q=0.5
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
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Get_ExpectExpectBase64String()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };
            var result = WebSources.Execute(source, WebRequestMethod.Get, "http://www.msn.com/", "", false, out var errors, new string[] { });

            Assert.IsTrue(IsBase64(result));

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
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var result = WebSources.Execute(source, WebRequestMethod.Put, "http://www.msn.com/", "", false, out var errors, new string[] { });

            Assert.IsTrue(IsBase64(result));
            Assert.AreEqual(result, Convert.ToBase64String(responseFromWeb));

            var client = source.Client;
            Assert.IsFalse(errors.HasErrors(), "On executing without multipart form data web client source returned at least one error: " + (errors.HasErrors() ? errors.FetchErrors()[0] : ""));
            Assert.IsTrue(client.Headers.AllKeys.Contains("a"));
            Assert.IsTrue(client.Headers.AllKeys.Contains("b"));
            
            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_ExpectExpectBase64String()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))//(address+query, method.ToString().ToUpperInvariant(), putData.ToBytesArray()))
                .Returns(responseFromWeb);

            var source = new WebSource 
            { 
                Address = "http://www.msn.com/", 
                AuthenticationType = AuthenticationType.Anonymous, 
                Client = mockWebClientWrapper.Object 
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, new string[] { });

            Assert.IsTrue(IsBase64(result));
            Assert.AreEqual(result, Convert.ToBase64String(responseFromWeb));

            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }


        private static bool IsBase64(string payload)
        {
            var result = false;
            try
            {
                Convert.FromBase64String(payload);
                result = true;
            }
            catch (Exception)
            {
                // if error is thrown we know it is not a valid base64 string
            }

            return result;
        }

    }
}
