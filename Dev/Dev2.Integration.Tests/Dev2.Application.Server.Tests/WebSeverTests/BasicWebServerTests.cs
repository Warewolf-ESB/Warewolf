using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.WebSeverTests
{
    /// <summary>
    /// Summary description for BasicWebServerTests
    /// </summary>
    [TestClass]
    public class BasicWebServerTests
    {
        string WebserverURI = ServerSettings.WebserverURI;
        private string WebserverHttpsURI = ServerSettings.WebserverHttpsURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region List Service Tests


        [TestMethod]
        public void WebserverRequestWhenLargeDataShouldAddDownloadHeaders()
        {
            string path = WebserverURI + "ABC";

            HttpWebResponse result = TestHelper.GetResponseFromServer(path);

            var allKeys = result.Headers.AllKeys;
            const string ContentType = "Content-Type";
            const string ContentDisposition = "Content-Disposition";
            CollectionAssert.Contains(allKeys, ContentType);
            CollectionAssert.DoesNotContain(allKeys, ContentDisposition);

            var contentTypeValue = result.Headers.Get(ContentType);

            Assert.AreNotEqual("application/force-download",contentTypeValue);
        }

        [TestMethod]
        public void WebserverRequestWhenNotLargeDataShouldNotAddDownloadHeaders()
        {

            string path = ServerSettings.WebserverURI + "LargeDataTest";

            HttpWebResponse result = TestHelper.GetResponseFromServer(path);

            var allKeys = result.Headers.AllKeys;
            const string ContentType = "Content-Type";
            const string ContentDisposition = "Content-Disposition";
            CollectionAssert.Contains(allKeys, ContentType);
            CollectionAssert.Contains(allKeys, ContentDisposition);

            var contentTypeValue = result.Headers.Get(ContentType);
            var contentDispositionValue = result.Headers.Get(ContentDisposition);

            Assert.AreEqual("application/force-download",contentTypeValue);
            Assert.AreEqual("attachment; filename=Output.xml", contentDispositionValue);
        }

        #endregion List Service Tests

        [TestMethod]
        public void WebServerExecuteWhereDbServiceCalledExpectErrorResult()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverURI, "CaseSP");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, "<InnerError>Can only execute workflows from web browser</InnerError>");
        }

        // -- Trav New -- //
        [TestMethod]
        public void CanWebServerReturnErrorsResultAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverURI, "CaseSP.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------

            var expected = "{ \"FatalError\": \"An internal error occured while executing the service request\",\"errors\": [ \"Can only execute workflows from web browser\"]}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void CanWebServerExecuteNonExistingServiceWithJsonExtInternalErrorsAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverURI, "BugXXXX.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "{ \"FatalError\": \"An internal error occured while executing the service request\",\"errors\": [ \"Service [ BugXXXX ] not found.\"]}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServerExecuteNonExistingServiceWithXMLExtInternalErrorAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverURI, "BugXXXX.xml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<FatalError> <Message> An internal error occured while executing the service request </Message><InnerError>Service [ BugXXXX ] not found.</InnerError></FatalError>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServerExecuteServiceWithJsonExtExpectResultAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverURI, "Bug9139.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "{\"result\":\"PASS\"}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServerExecuteServiceWithXmlExtExpectResultAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverURI, "Bug9139.xml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServerExecuteServiceWithBadExtExpectResultAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverURI, "Bug9139.ml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebServer_WhenHTTPS")]
        public void WebServer_WhenHTTPS_InvokeOnSSLPort_ExpectValidResultViaSSL()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", WebserverHttpsURI, "Bug9139");
            //------------Execute Test---------------------------
            bool wasHTTPS;
            string responseData = TestHelper.PostDataToWebserver(postData, out wasHTTPS);
            
            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.IsTrue(wasHTTPS);
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

    }
}
