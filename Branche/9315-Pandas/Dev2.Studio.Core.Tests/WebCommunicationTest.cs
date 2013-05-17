using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using Moq;
using System.Net;
using Dev2.Studio.Core;

namespace Dev2.Core.Tests {
    [TestClass]
    public class WebCommunicationTest {

        #region Get Tests

        //[TestMethod]
        //public void GetTest() {
        //    var webRequest = new WebCommunication();
        //    var response = new Mock<IWebCommunicationResponse>();
        //    response.Object.Content = "<result>Success</result>";
        //    var mockUri = "http://localhost:1234";
        //    webRequest.Setup(c => c.Get(mockUri)).Returns(response.Object); 
        //    IWebCommunicationResponse actual = webRequest.Object.Get(mockUri);
        //    IWebCommunicationResponse expected = response.Object;
        //    Assert.AreEqual(expected, actual);
        //}

        #endregion Get Tests

        #region Post Tests

        //[TestMethod]
        //public void PostTest() {
        //    var webRequest = new Mock<IWebCommunication>();
        //    var response = new Mock<IWebCommunicationResponse>();
        //    response.Object.Content = "<result>Success</result>";
        //    var mockUri = "http://localhost:1234";
        //    var mockData = "<info>This is data</info>";
        //    webRequest.Setup(c => c.Post(mockUri,mockData)).Returns(response.Object);
        //    IWebCommunicationResponse actual = webRequest.Object.Post(mockUri, mockData);
        //    IWebCommunicationResponse expected = response.Object;
        //    Assert.AreEqual(expected, actual);
        //}

        #endregion Post Tests
    }
}
