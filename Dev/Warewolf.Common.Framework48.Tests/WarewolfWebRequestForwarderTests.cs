/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Text;
using Warewolf.Core;
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

            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>())).Returns(new Mock<IHttpClient>().Object);

            var testUrl = "http://warewolf.io:0420/test/url";

            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, testUrl, new List<IServiceInput>());
            //---------------------------------Act------------------------------------
            WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes("this is a test message"));

            //---------------------------------Assert---------------------------------
            mockHttpClientFactory.Verify(o => o.New(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(WarewolfWebRequestForwarder))]
        public void WarewolfWebRequestForwarder_Consume_Should_CallPostAsync()
        {
            //---------------------------------Arrange--------------------------------
            var mockHttpClient = new Mock<IHttpClient>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";

            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, testUrl, new List<IServiceInput>());
            //---------------------------------Act------------------------------------
            WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes("this is a test message"));

            //---------------------------------Assert---------------------------------
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
            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string o, string p) => { postBody = p; });
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";
            var inputs = new List<IServiceInput>
            {
                new ServiceInput("Name","FirstName")
            };
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, testUrl, inputs);
            var expectedPostBody = "{\"Name\":\"My Name\"}";
            //---------------------------------Act------------------------------------
            string message = "<Root><FirstName>My Name</FirstName></Root>";
            WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes(message));

            //---------------------------------Assert---------------------------------
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
            mockHttpClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<string>())).Callback((string o, string p) => { postBody = p; });
            mockHttpClientFactory.Setup(o => o.New(It.IsAny<string>())).Returns(mockHttpClient.Object);

            var testUrl = "http://warewolf.io:0420/test/url";
            var inputs = new List<IServiceInput>
            {
                new ServiceInput("Name","FirstName")
            };
            var WarewolfWebRequestForwarder = new WarewolfWebRequestForwarder(mockHttpClientFactory.Object, testUrl, inputs);
            var expectedPostBody = "{\"Name\":\"My Name\"}";
            //---------------------------------Act------------------------------------
            string message = "{\"FirstName\":\"My Name\"}";
            WarewolfWebRequestForwarder.Consume(Encoding.UTF8.GetBytes(message));

            //---------------------------------Assert---------------------------------
            Assert.AreEqual(expectedPostBody, postBody);
        }
    }
}
