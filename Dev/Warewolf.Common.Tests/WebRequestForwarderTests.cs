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
using Warewolf.Web;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class WebRequestForwarderTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IHttpClientFactory))]
        public void WebRequestForwarder_ProcessMessage_Success()
        {
            //-----------------------------Arrange------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Connected") };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            mockHttpClient.Setup(o => o.GetAsync(It.IsAny<string>())).Returns(Task.Run(() => response));

            mockHttpClientFactory.Setup(o => o.New(It.IsAny<Uri>())).Returns(mockHttpClient.Object);
            //-----------------------------Act----------------------------------
            var factory = mockHttpClientFactory.Object;
            var client = factory.New(new Uri("http://warewolf.io"));
            client.GetAsync("/person/1");
            //-----------------------------Assert-------------------------------
            mockHttpClient.Verify(o => o.GetAsync(It.IsAny<string>()), Times.Once);
            mockHttpClientFactory.Verify(o => o.New(It.IsAny<Uri>()), Times.Once);
        }
    }
}
