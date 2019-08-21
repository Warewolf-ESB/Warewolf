/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class WebRequestForwarderTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebRequestForwarderBase))]
        public void WebRequestForwarder_ProcessMessage_Success()
        {
            //-----------------------------Arrange------------------------------
            var mockHttpClient = new Mock<IHttpClient>();

            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Connected") };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            mockHttpClient.Setup(o => o.GetAsync(It.IsAny<string>())).Returns(Task.Run(()=> response));

            var testWebRequestForwarder = new TestWebRequestForwarder(mockHttpClient.Object);

            //-----------------------------Act----------------------------------
            var result = testWebRequestForwarder.TestSendUrl("http:example.co.za");
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(response, result);
            mockHttpClient.Verify(o => o.GetAsync(It.IsAny<string>()), Times.Once);
        }

        private class TestWebRequestForwarder : WebRequestForwarderBase
        {
            public TestWebRequestForwarder(IHttpClient httpClient) : base(httpClient)
            {
            }

            public HttpResponseMessage TestSendUrl(string url)
            {
                return SendUrl(url).Result;
            }
        }
    }
}
