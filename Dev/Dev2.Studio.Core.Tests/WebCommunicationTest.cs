//using System;
//using System.Diagnostics.CodeAnalysis;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using Caliburn.Micro;
//using Dev2.Core.Tests.Network;
//using Dev2.Data.ServiceModel;
//using Dev2.Studio.Core.Network;
//using Dev2.Studio.Webs;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Diagnostics.CodeAnalysis;
//using Dev2.Studio.Core.Interfaces;
//using System.ComponentModel.Composition;
//using Moq;
//using System.Net;
//using Dev2.Studio.Core;
//
//namespace Dev2.Core.Tests {
//    [TestClass]
//    [ExcludeFromCodeCoverage]
//    public class WebCommunicationTest {
//
//        #region Get Tests
//
//        //[TestMethod]
//        //public void GetTest() {
//        //    var webRequest = new WebCommunication();
//        //    var response = new Mock<IWebCommunicationResponse>();
//        //    response.Object.Content = "<result>Success</result>";
//        //    var mockUri = "http://localhost:1234";
//        //    webRequest.Setup(c => c.Get(mockUri)).Returns(response.Object); 
//        //    IWebCommunicationResponse actual = webRequest.Object.Get(mockUri);
//        //    IWebCommunicationResponse expected = response.Object;
//        //    Assert.AreEqual(expected, actual);
//        //}
//
//        #endregion Get Tests
//
//        #region Post Tests
//
//        //[TestMethod]
//        //public void PostTest() {
//        //    var webRequest = new Mock<IWebCommunication>();
//        //    var response = new Mock<IWebCommunicationResponse>();
//        //    response.Object.Content = "<result>Success</result>";
//        //    var mockUri = "http://localhost:1234";
//        //    var mockData = "<info>This is data</info>";
//        //    webRequest.Setup(c => c.Post(mockUri,mockData)).Returns(response.Object);
//        //    IWebCommunicationResponse actual = webRequest.Object.Post(mockUri, mockData);
//        //    IWebCommunicationResponse expected = response.Object;
//        //    Assert.AreEqual(expected, actual);
//        //}
//
//        #endregion Post Tests
//
//        #region Encode Uri Param
//
//        [TestMethod]
//        public void FullyEncodeServerDetailsExpectedBothFullstopsAndUriSensitiveChars()
//        {
//            Uri uri;
//            Uri.TryCreate("http://127.0.0.1:77/dsf", UriKind.Absolute, out uri); 
//            IEnvironmentConnection testConn = new TcpConnection(new MockSecurityProvider(string.Empty), uri, 77);
//            testConn.DisplayName = "Localhost";
//            Assert.AreEqual("Localhost+(http%3a%2f%2f127%252E0%252E0%252E1%3a77%2fdsf)", RootWebSite.FullyEncodeServerDetails(testConn));
//        }
//
//        #endregion
//
//        #region Exception Test
//
//        [TestMethod]
//        [TestCategory("RootWebSite,UnitTest")]
//        [Description("Ensure the RootWebsite can detect a null DsfChannel property on the Enviroment")]
//        [Owner("Travis")]
//        public void CanShowDialogHandleNullDsfChannelProperly()
//        {
//            var model = new Mock<IEnvironmentModel>();
//            Assert.IsFalse(RootWebSite.ShowDialog(model.Object, ResourceType.Server, null, null, Guid.NewGuid()), "Show Server Dialog Displayed With No DsfChannel");
//        }
//
//        [TestMethod]
//        [ExpectedException(typeof(ArgumentNullException), "Failed to properly detect null Enviroment")]
//        [TestCategory("RootWebSite,UnitTest")]
//        [Description("Ensure the RootWebsite can detect a null Enviroment properly")]
//        [Owner("Travis")]
//        public void CanShowDialogHandleNullEnviromentProperly()
//        {
//            RootWebSite.ShowDialog(null, ResourceType.Server, null, null, Guid.NewGuid());
//        }
//
//        #endregion
//    }
//}
