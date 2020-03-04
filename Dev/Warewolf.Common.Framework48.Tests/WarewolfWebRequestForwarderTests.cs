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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Core;
using Warewolf.Data;
using Warewolf.Streams;
using Warewolf.Web;

namespace Warewolf.Common.Tests
{
    [TestClass]
    public class WarewolfWebRequestForwarderTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfWebRequestForwarder))]
        public void WarewolfWebRequestForwarder_Consume_Success()
        {
            //---------------------------------Arrange--------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task<HttpResponseMessage>.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>())).Returns(mockHttpClient.Object);
            
            var testUrl = "http://warewolf.io:0420/test/url";
            var customTransactionID = new Guid().ToString();
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object,new Mock<IPublisher>().Object, testUrl, "","", new List<IServiceInputBase>(), false);
            //---------------------------------Act------------------------------------
            var result = WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes("This is a message"),customTransactionID).Result;

            //---------------------------------Assert---------------------------------
            Assert.AreEqual(ConsumerResult.Success, result);
            mockHttpClientFactory.Verify(o => o.New(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WarewolfWebRequestForwarder))]
        public void WarewolfWebRequestForwarder_Consume_Should_CallPostAsync()
        {
            //---------------------------------Arrange--------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task<HttpResponseMessage>.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";
            var customTransactionID = new Guid().ToString();
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, new Mock<IPublisher>().Object, testUrl, "","", new List<IServiceInputBase>(), false);
            //---------------------------------Act------------------------------------
            var result = WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes("This is a message"),customTransactionID).Result;

            //---------------------------------Assert---------------------------------
            Assert.AreEqual(ConsumerResult.Success, result);
            mockHttpClient.Verify(o => o.PostAsync(It.IsAny<string>(),It.IsAny<string>()), Times.Once);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WarewolfWebRequestForwarder))]
        public void WarewolfWebRequestForwarder_Consume_GivenMappableInputsAndXmlData_ShouldCreateCorrectPostBody()
        {
            //---------------------------------Arrange--------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var postBody = "";
            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string o, string p) => { postBody = p; }).Returns(Task<HttpResponseMessage>.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";
            var inputs = new List<IServiceInputBase>
            {
                new ServiceInput("Name","FirstName")
            };
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, new Mock<IPublisher>().Object, testUrl, "","", inputs, false);
            var expectedPostBody = "{\"Name\":\"My Name\"}";
            var customTransactionID = new Guid().ToString();
            //---------------------------------Act------------------------------------
            string message = "<Root><FirstName>My Name</FirstName></Root>";
            var result = WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes(message),customTransactionID).Result;

            //---------------------------------Assert---------------------------------
            Assert.AreEqual(ConsumerResult.Success, result);
            Assert.AreEqual(expectedPostBody, postBody);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WarewolfWebRequestForwarder))]
        public void WarewolfWebRequestForwarder_Consume_GivenMappableInputsAndJsonData_ShouldCreateCorrectPostBody()
        {
            //---------------------------------Arrange--------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var postBody = "";
            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string o, string p) => { postBody = p; }).Returns(Task<HttpResponseMessage>.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";
            var inputs = new List<IServiceInputBase>
            {
                new ServiceInput("Name","FirstName")
            };
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, new Mock<IPublisher>().Object, testUrl, "","", inputs, false);
            var expectedPostBody = "{\"Name\":\"My Name\"}";
            var customTransactionID = new Guid().ToString();
            //---------------------------------Act------------------------------------
            string message = "{\"FirstName\":\"My Name\"}";
            var result = WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes(message),customTransactionID).Result;

            //---------------------------------Assert---------------------------------
            Assert.AreEqual(ConsumerResult.Success, result);
            Assert.AreEqual(expectedPostBody, postBody);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WarewolfWebRequestForwarder))]
        public async Task WarewolfWebRequestForwarder_Consume_GivenNonSuccessResponse_ShouldPublishMessageToDeadLetterQueueAsync()
        {
            //---------------------------------Arrange--------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockPublisher = new Mock<IPublisher>();
            mockPublisher.Setup(p => p.Publish(It.IsAny<byte[]>())).Verifiable();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task<HttpResponseMessage>.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)));
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";
            var inputs = new List<IServiceInputBase>();
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, mockPublisher.Object, testUrl, "","", inputs, false);
            //---------------------------------Act------------------------------------
            string message = "{\"FirstName\":\"My Name\"}";
            var customTransactionID = new Guid().ToString();
            var result = await WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes(message), customTransactionID);

            //---------------------------------Assert---------------------------------
            Assert.AreEqual(ConsumerResult.Failed, result);
            mockPublisher.Verify(p => p.Publish(It.IsAny<byte[]>()),Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WarewolfWebRequestForwarder))]
        public async Task WarewolfWebRequestForwarder_Consume_GivenSuccessResponse_ShouldNotPublishMessageToDeadLetterQueueAsync()
        {
            //---------------------------------Arrange--------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockPublisher = new Mock<IPublisher>();
            mockPublisher.Setup(p => p.Publish(It.IsAny<byte[]>())).Verifiable();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task<HttpResponseMessage>.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";
            var inputs = new List<IServiceInputBase>();
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, mockPublisher.Object, testUrl, "","", inputs, false);
            //---------------------------------Act------------------------------------
            string message = "{\"FirstName\":\"My Name\"}";
            var customTransactionID = new Guid().ToString();
            var result = await WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes(message),customTransactionID);

            //---------------------------------Assert---------------------------------
            Assert.AreEqual(ConsumerResult.Success, result);
            mockPublisher.Verify(p => p.Publish(It.IsAny<byte[]>()), Times.Never);
        }
    }
}
