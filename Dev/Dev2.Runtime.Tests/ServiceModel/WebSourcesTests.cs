/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Warewolf.Common.Interfaces.NetStandard20;
using Warewolf.Common.NetStandard20;
using Warewolf.Data.Options;
using Warewolf.Options;

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class WebSourcesTests
    {
        const string postData =
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
                Address = "http://sample.com",
                DefaultQuery = "/default",
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
                Address = "http://example.com",
                DefaultQuery = "/default",
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
            mockWebSource.Setup(o => o.Address)
                .Returns("http://localhost:2121");
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

            _= WebSources.Execute(source, WebRequestMethod.Get, "http://consoto.com", "test data", false, out ErrorResultTO error, new[] { "b:e" });

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
            var clientCredentials = client.Credentials as NetworkCredential;
            Assert.IsNotNull(clientCredentials);
            Assert.IsTrue(clientCredentials.UserName == "User");
            Assert.IsTrue(clientCredentials.Password == "pwd");
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
        public void WebSources_PerformMultipartWebRequest_SetsContentTypeHeaders_And_ShouldReturnApplicationException()
        {
            var address = "http://www.msn.com/";
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.BadRequest);

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(new MemoryStream());
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var source = new WebSource
            {
                Address = address,
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
                Assert.ThrowsException<ApplicationException>(()=> WebSources.PerformMultipartWebRequest(mockWebRequestFactory.Object, source.Client, source.Address, postData.ToBytesArray()), "Error while upload files. Server status code: " + HttpStatusCode.BadRequest);
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
        public void WebSources_PerformMultipartWebRequest_SetsContentTypeHeaders_And_ShouldReturnNull()
        {
            var address = "http://www.msn.com/";
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.OK);

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(new MemoryStream());
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var source = new WebSource
            {
                Address = address,
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

            var result = string.Empty;
            try
            {
                result = WebSources.PerformMultipartWebRequest(mockWebRequestFactory.Object, source.Client, source.Address, postData.ToBytesArray());
            }
            catch (WebException e)
            {
                Assert.Fail("Error connecting to " + source.Address + "\n" + e.Message);
            }
            var client = source.Client;
            var contentType = client.Headers["Content-Type"];
            Assert.IsNotNull(contentType);
            Assert.AreEqual("multipart/form-data", contentType);
            
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_PerformMultipartWebRequest_SetsContentTypeHeaders_And_ShouldReturnNullWebResponse()
        {
            var address = "http://www.msn.com/";
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "a:x", "b:e" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.OK);
            mockWebResponseWrapper.Setup(o => o.GetResponseStream())
                .Returns(new MemoryStream(responseFromWeb));

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(new MemoryStream());
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var source = new WebSource
            {
                Address = address,
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

            var result = string.Empty;
            try
            {
                result = WebSources.PerformMultipartWebRequest(mockWebRequestFactory.Object, source.Client, source.Address, postData.ToBytesArray());
            }
            catch (WebException e)
            {
                Assert.Fail("Error connecting to " + source.Address + "\n" + e.Message);
            }
            var client = source.Client;
            var contentType = client.Headers["Content-Type"];
            Assert.IsNotNull(contentType);
            Assert.AreEqual("multipart/form-data", contentType);
           
            Assert.AreEqual("response from web request", result);
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_ToTest_ConvertToHttpNewLine_ExpectedRequestData()
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

            var relativeUri = string.Empty;
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");

            var address = "http://www.msn.com/";
            var requestStream = new MemoryStream();
            var responseStrem = new MemoryStream(responseFromWeb);

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.OK);
            mockWebResponseWrapper.Setup(o => o.GetResponseStream())
                .Returns(responseStrem);

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(requestStream);
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "Content-Type:multipart/form-data" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            _ = WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, string.Empty, isNoneChecked: true, isFormDataChecked: false, data, true, out var errors, new List<IFormDataParameters>(), mockWebRequestFactory.Object);

            var bytes = requestStream.GetBuffer();
            using (var memoryStream = new MemoryStream(bytes))
            {
                var streamReader = new StreamReader(memoryStream);
                var expectedRequestPayload = streamReader.ReadToEnd();

                Assert.AreEqual(data, expectedRequestPayload); 
            }
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
        public void WebSources_Execute_WebRequestMethod_Post_Classic_ExpectNonBase64String()
        {
            var responseFromWeb = "response from web request";
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { });
            mockWebClientWrapper.Setup(o => o.UploadString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(responseFromWeb);

            var source = new WebSource 
            { 
                Address = "http://www.msn.com/", 
                AuthenticationType = AuthenticationType.Anonymous, 
                Client = mockWebClientWrapper.Object 
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, new[] { "Content-Type:application/json" });

            Assert.IsFalse(IsBase64(result));
            Assert.AreEqual(result, responseFromWeb);

            var client = source.Client;

            Assert.AreEqual(client.Headers[HttpRequestHeader.ContentType], "application/json");
            mockWebClientWrapper.Verify(o => o.UploadString("http://www.msn.com/", "POST", string.Empty), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Delete_Classic_ExpectNonBase64String()
        {
            var responseFromWeb = "response from web request";
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { });
            mockWebClientWrapper.Setup(o => o.UploadString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var result = WebSources.Execute(source, WebRequestMethod.Delete, "http://www.msn.com/", "", false, out var errors, new string[] { });

            Assert.IsFalse(IsBase64(result));
            Assert.AreEqual(result, responseFromWeb);

            mockWebClientWrapper.Verify(o => o.UploadString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_Given_NoContentType_And_ThrowError_False_ExpectErrorMessage()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, "http://www.msn.com/", isNoneChecked: false, isFormDataChecked: true, "", throwError: false, out var errors, new List<FormDataParameters> { });

            Assert.IsTrue(errors.HasErrors(), "This error should now happen, the handling of the Content-Type for form-data request should no longer be done on the backend, the user should be able to edd his own");
            Assert.AreEqual("The argument must not be null or empty and must contain non-whitespace characters must\r\nParameter name: Content-Type", errors.MakeDisplayReady());
            
            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_Given_WithCorrectContentType_And_ThrowError_False_ExpectSuccess()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");

            var address = "http://www.msn.com/";
            var requestStream = new MemoryStream();
            var responseStrem = new MemoryStream(responseFromWeb);

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.OK);
            mockWebResponseWrapper.Setup(o => o.GetResponseStream())
                .Returns(responseStrem);

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(requestStream);
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();
            mockWebClientWrapper.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                { 
                    { 
                        HttpRequestHeader.ContentType, "multipart/form-data; boundry=------thisshoulbesentinbythecaller" 
                    } 
                });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, "http://www.msn.com/", isNoneChecked: false, isFormDataChecked: true, "", throwError: false, out var errors, new List<FormDataParameters> { }, mockWebRequestFactory.Object);

            Assert.IsFalse(errors.HasErrors(), "This error should not happen, the Content-Type for form-data request is overriden if there are any misspells");
            
            //make sure the data sent is as expected
            var bytes = requestStream.GetBuffer();
            using (var memoryStream = new MemoryStream(bytes))
            {
                var streamReader = new StreamReader(memoryStream);
                var expectedRequestPayload = streamReader.ReadToEnd();
                StringAssert.Contains(expectedRequestPayload, "\r\n--------thisshoulbesentinbythecaller--");
            }
            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_Given_NoContentType_And_ThrowError_True_ExpectArgumentException()
        {
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");
            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            Assert.ThrowsException<ArgumentNullException>(()=> WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, "http://www.msn.com/", isNoneChecked: false, isFormDataChecked: true, "", throwError: true, out var errors, new List<FormDataParameters> { }));

            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_Given_FormData_With_File_MatchType_ExpectApplicationException()
        {
            var relativeUri = string.Empty;
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");

            var address = "http://www.msn.com/";


            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.BadRequest);

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(new MemoryStream());
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "Content-Type:multipart/form-data" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var formDataParameters = new List<IFormDataParameters>
            {
                new FormDataConditionExpression
                {
                    Key = "[[textKey]]",
                    Cond = new FormDataConditionFile
                    {
                        TableType = enFormDataTableType.File,
                        FileBase64 = "VGhpcyBpcyBzb21lIHRleHQgaW4gdGhlIGZpbGUu",
                        FileName = "test file name"
                    }
                }.ToFormDataParameter()
            };

            Assert.ThrowsException<ApplicationException>(() => WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, relativeUri,isNoneChecked: false, isFormDataChecked: true, string.Empty, true, out var errors, formDataParameters, mockWebRequestFactory.Object), "Error while upload files. Server status code: BadRequest");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_Given_FormData_With_File_MatchType_ExpectSuccess()
        {
            var relativeUri = string.Empty;
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");

            var address = "http://www.msn.com/";

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.OK);
            mockWebResponseWrapper.Setup(o => o.GetResponseStream())
                .Returns(new MemoryStream(responseFromWeb));

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(new MemoryStream());
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "Content-Type:multipart/form-data" });
         
            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var formDataParameters = new List<IFormDataParameters> 
            {
                new FormDataConditionExpression
                {
                    Key = "[[textKey]]",
                    Cond = new FormDataConditionFile
                    {
                        TableType = enFormDataTableType.File,
                        FileBase64 = "VGhpcyBpcyBzb21lIHRleHQgaW4gdGhlIGZpbGUu",
                        FileName = "test file name"
                    }
                }.ToFormDataParameter()
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, relativeUri, isNoneChecked: false, isFormDataChecked: true, string.Empty, true, out var errors, formDataParameters, mockWebRequestFactory.Object);
            
            Assert.AreEqual("response from web request", result);

            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_Given_FormData_With_File_MatchType_GetResponseStream_ExpectNull()
        {
            var relativeUri = string.Empty;
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");

            var address = "http://www.msn.com/";

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.OK);

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(new MemoryStream());
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "Content-Type:multipart/form-data" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var formDataParameters = new List<IFormDataParameters>
            {
                new FormDataConditionExpression
                {
                    Key = "[[textKey]]",
                    Cond = new FormDataConditionFile
                    {
                        TableType = enFormDataTableType.File,
                        FileBase64 = "VGhpcyBpcyBzb21lIHRleHQgaW4gdGhlIGZpbGUu",
                        FileName = "test file name"
                    }
                }.ToFormDataParameter()
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, relativeUri, isNoneChecked: false, isFormDataChecked: true, string.Empty, true, out var errors, formDataParameters, mockWebRequestFactory.Object);

            Assert.IsFalse(IsBase64(result));

            Assert.IsNull(result);

            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_WebRequestMethod_Post_Given_FormData_With_File_MatchType_GetResponseStream_NotNull_ExpectSuccess()
        {
            var relativeUri = string.Empty;
            var responseFromWeb = Encoding.ASCII.GetBytes("response from web request");

            var address = "http://www.msn.com/";
            var requestStream = new MemoryStream();
            var responseStrem = new MemoryStream(responseFromWeb);

            var mockWebResponseWrapper = new Mock<HttpWebResponse>();
            mockWebResponseWrapper.Setup(o => o.StatusCode)
                .Returns(HttpStatusCode.OK);
            mockWebResponseWrapper.Setup(o => o.GetResponseStream())
                .Returns(responseStrem);

            var mockWebRequest = new Mock<IWebRequest>();
            mockWebRequest.Setup(o => o.Headers)
                .Returns(new WebHeaderCollection
                {
                    "Authorization:bear: sdfsfff",
                });
            mockWebRequest.Setup(o => o.ContentType)
                .Returns("Content-Type: multipart/form-data");
            mockWebRequest.Setup(o => o.ContentLength)
                .Returns(Encoding.ASCII.GetBytes(postData).Length);
            mockWebRequest.Setup(o => o.Method)
                .Returns("POST");
            mockWebRequest.Setup(o => o.GetRequestStream())
                .Returns(requestStream);
            mockWebRequest.Setup(o => o.GetResponse())
                .Returns(mockWebResponseWrapper.Object);

            var mockWebRequestFactory = new Mock<IWebRequestFactory>();
            mockWebRequestFactory.Setup(o => o.New(address))
                .Returns(mockWebRequest.Object);

            var mockWebClientWrapper = new Mock<IWebClientWrapper>();

            mockWebClientWrapper.Setup(o => o.Headers).Returns(new WebHeaderCollection { "Content-Type:multipart/form-data" });
            mockWebClientWrapper.Setup(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(responseFromWeb);

            var source = new WebSource
            {
                Address = "http://www.msn.com/",
                AuthenticationType = AuthenticationType.Anonymous,
                Client = mockWebClientWrapper.Object
            };

            var formDataParameters = new List<IFormDataParameters>
            {
                new FormDataConditionExpression
                {
                    Key = "[[textKey]]",
                    Cond = new FormDataConditionFile
                    {
                        TableType = enFormDataTableType.File,
                        FileBase64 = "VGhpcyBpcyBzb21lIHRleHQgaW4gdGhlIGZpbGUu",
                        FileName = "test file name.txt"
                    }
                }.ToFormDataParameter()
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, headers: new string[] { }, relativeUri, isNoneChecked: false, isFormDataChecked: true, string.Empty, true, out var errors, formDataParameters, mockWebRequestFactory.Object);

            //make sure the data sent is as expected
            var bytes = requestStream.GetBuffer();
            using (var memoryStream = new MemoryStream(bytes))
            {
                var streamReader = new StreamReader(memoryStream);
                var expectedRequestPayload = streamReader.ReadToEnd();
                Assert.IsTrue(expectedRequestPayload.Contains("Content-Disposition: form-data; name=\"[[textKey]]\";"));
                Assert.IsTrue(expectedRequestPayload.Contains("filename=\"test file name.txt"));
                Assert.IsTrue(expectedRequestPayload.Contains("Content-Type: application/octet-stream"));
                Assert.IsTrue(expectedRequestPayload.Contains("\r\n\r\nThis is some text in the file."));
            }

            Assert.IsFalse(IsBase64(result));

            Assert.IsNotNull(result);
            Assert.AreEqual("response from web request", result);

            mockWebClientWrapper.Verify(o => o.UploadData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        private string GetReadToEnd(byte[] bytes)
        {
            var text = string.Empty;
            using (var stream = new StreamReader(new MemoryStream(bytes)))
            {
                text = stream.ReadToEnd();
            }
            return text;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_Null_WebSource_Expect_Correct_ErrorMessage()
        {
            var result = WebSources.Execute(null, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, new string[] { });

            Assert.AreEqual("", result);
            var fetchErrors = errors.FetchErrors();
            Assert.AreEqual(1, fetchErrors.Count);
            Assert.AreEqual("The web source has an incomplete web address.", fetchErrors[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebSources))]
        public void WebSources_Execute_Incomplete_WebSource_Expect_Correct_ErrorMessage()
        {
            var source = new WebSource
            {
                Address = "",
            };

            var result = WebSources.Execute(source, WebRequestMethod.Post, "http://www.msn.com/", "", false, out var errors, new string[] { });

            Assert.AreEqual("", result);
            var fetchErrors = errors.FetchErrors();
            Assert.AreEqual(1, fetchErrors.Count);
            Assert.AreEqual("The web source has an incomplete web address.", fetchErrors[0]);
        }

        private static bool IsBase64(string payload)
        {
            try
            {
                _ = Convert.FromBase64String(payload);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
