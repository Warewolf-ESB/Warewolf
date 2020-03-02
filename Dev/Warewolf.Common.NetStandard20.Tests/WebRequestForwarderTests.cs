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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Web;

namespace Warewolf.Web.Tests
{
    [TestClass]
    public class WebRequestForwarderTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IHttpClientFactory))]
        public void HttpClientFactory_ProcessMessage_Success()
        {
            //-----------------------------Arrange------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var headers = new Headers();
            headers.CustomTransactionID = "customTransactionID";
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Connected") };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            mockHttpClient.Setup(o => o.GetAsync(It.IsAny<string>())).Returns(Task.Run(() => response));

            mockHttpClientFactory.Setup(o => o.New(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(),headers)).Returns(mockHttpClient.Object);
            //-----------------------------Act----------------------------------
            var factory = mockHttpClientFactory.Object;
          
            var client = factory.New(new Uri("http://warewolf.io"),"","",headers);
            client.GetAsync("/person/1");
            //-----------------------------Assert-------------------------------
            mockHttpClient.Verify(o => o.GetAsync(It.IsAny<string>()), Times.Once);
            mockHttpClientFactory.Verify(o => o.New(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(),headers), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(IHttpClientFactory))]
        public void HttpClientFactory_New_String_With_Username_Password_Success()
        {
            //-----------------------------Arrange------------------------------
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var headers = new Headers();
            headers.CustomTransactionID = "customTransactionID";
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),headers));
            var factory = mockHttpClientFactory.Object;
            //-----------------------------Act----------------------------------

            var client = factory.New("http://warewolf.io", "Bob", "TheBuilder",headers);
            //-----------------------------Assert-------------------------------
            mockHttpClientFactory.Verify(o => o.New("http://warewolf.io", "Bob", "TheBuilder",headers), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(IHttpClientFactory))]
        public void HttpClientFactory_New_Uri_With_Username_Password_Success()
        {
            //-----------------------------Arrange------------------------------
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var headers = new Headers();
            headers.CustomTransactionID = "customTransactionID";
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(),headers));
            var factory = mockHttpClientFactory.Object;
            var uri = new Uri("http://warewolf.io");
            //-----------------------------Act----------------------------------
           
            var client = factory.New(uri, "Bob", "TheBuilder",headers);
            //-----------------------------Assert-------------------------------
            mockHttpClientFactory.Verify(o => o.New(uri, "Bob", "TheBuilder",headers), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(IHttpClientFactory))]
        public void HttpClientFactory_New_String_With_Username_Password_HasCredentails_True()
        {
            //-----------------------------Arrange------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            mockHttpClient.SetupAllProperties();
            var factory = new HttpClientFactory();

           //-----------------------------Act----------------------------------
           var headers = new Headers();
           headers.CustomTransactionID = "customTransactionID";
            var client = factory.New("http://warewolf.io", "Bob", "TheBuilder",headers);
            //-----------------------------Assert-------------------------------
            Assert.IsTrue(client.HasCredentials);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(IHttpClientFactory))]
        public void HttpClientFactory_New_Uri_With_Username_Password_HasCredentails_True()
        {
            //-----------------------------Arrange------------------------------
            var factory = new HttpClientFactory();

            var uri = new Uri("http://warewolf.io");
            //-----------------------------Act----------------------------------
            var headers = new Headers();
            headers.CustomTransactionID = "customTransactionID";
            var client = factory.New(uri, "Bob", "TheBuilder",headers);
            //-----------------------------Assert-------------------------------
            Assert.IsTrue(client.HasCredentials);
        }
    }
}
