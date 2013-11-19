using System;
using System.Net;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2_Application_Server_Tests.WebServerTests
{
    /// <summary>
    /// Summary description for BasicWebServerTests
    /// </summary>
    [TestClass]
    public class WebServerTests
    {
        readonly static string ServicesUri = ServerSettings.WebserverURI;
        readonly static string ServicesHttpsUri = ServerSettings.WebserverHttpsURI;
        readonly static string WebsiteUri = ServerSettings.WebsiteServerUri;


        [TestMethod]
        public void WebServer_ServicesRequest_DataIsNotLarge_DownloadHeadersNotAdded()
        {
            string path = ServicesUri + "ABC";

            HttpWebResponse result = TestHelper.GetResponseFromServer(path);

            var allKeys = result.Headers.AllKeys;
            const string ContentType = "Content-Type";
            const string ContentDisposition = "Content-Disposition";
            CollectionAssert.Contains(allKeys, ContentType);
            CollectionAssert.DoesNotContain(allKeys, ContentDisposition);

            var contentTypeValue = result.Headers.Get(ContentType);

            Assert.AreNotEqual("application/force-download", contentTypeValue);
        }

        [TestMethod]
        public void WebServer_ServicesRequest_DataIsLarge_DownloadHeadersAdded()
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

            Assert.AreEqual("application/force-download", contentTypeValue);
            Assert.AreEqual("attachment; filename=Output.xml", contentDispositionValue);
        }

        [TestMethod]
        public void WebServer_ServicesRequest_Service_CannotExecuteServiceError()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "CaseSP");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, "<InnerError>Can only execute workflows from web browser</InnerError>");
        }

        // -- Trav New -- //
        [TestMethod]
        public void WebServer_ServicesRequest_ServiceAsJson_CannotExecuteServiceErrorAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "CaseSP.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------

            var expected = "{ \"FatalError\": \"An internal error occured while executing the service request\",\"errors\": [ \"Can only execute workflows from web browser\"]}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServer_ServicesRequest_NonExistingServiceAsJson_InternalErrorsAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "BugXXXX.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "{ \"FatalError\": \"An internal error occured while executing the service request\",\"errors\": [ \"Service [ BugXXXX ] not found.\"]}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServer_ServicesRequest_NonExistingServiceAsXml_InternalErrorAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "BugXXXX.xml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<FatalError> <Message> An internal error occured while executing the service request </Message><InnerError>Service [ BugXXXX ] not found.</InnerError></FatalError>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServer_ServicesRequest_WorkflowAsJson_ResultAsJson()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "Bug9139.json");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "{\"result\":\"PASS\"}";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServer_ServicesRequest_WorkflowAsXml_ResultAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "Bug9139.xml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        public void WebServer_ServicesRequest_WorkflowAsBadExtension_ResultAsXml()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesUri, "Bug9139.ml");
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            var expected = "<DataList><result>PASS</result></DataList>";
            Assert.AreEqual(expected, responseData, "Expected [ " + expected + "] but got [ " + responseData + " ]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebServer_HTTPS")]
        public void WebServer_ServicesRequest_OnSSLPort_ValidResultViaSSL()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServicesHttpsUri, "Bug9139");
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
