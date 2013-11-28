using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;
using System.Xml.Linq;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for ResourceUploadTest
    /// </summary>
    [TestClass]
    public class ResourceUploadTest
    {
        private const string _workflowName = "ResourceUpload";
        private const string _workflowCleanupName = _workflowName + "Test_Cleanup";

        private TestContext _testContext;

        public TestContext TestContext { get { return _testContext; } set { _testContext = value; } }

        public ResourceUploadTest()
        {
        }

        #region Test Class Initialization and Cleanup

        [TestCleanup]
        public void ResourceUploadTest_Cleanup() {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, _workflowCleanupName, TestResource.ResourceUpload_FromEditor_XmlString);
            TestHelper.PostDataToWebserver(postData);
        }

        #endregion Test Class Initialization and Cleanup

        #region Resource Upload Positive Test Cases

        [TestMethod]
        public void ResourceUpload_FromEditor_Expected_SucessfulResourceUpload()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, _workflowName, TestResource.ResourceUpload_FromEditor_XmlString);
            string actualResult = TestHelper.PostDataToWebserver(postData);
            XElement result = XElement.Parse(actualResult);

            IEnumerable<XElement> list = result.Descendants();

            XElement rNode = null;

            foreach (XElement element in list)
            {
                if (element.Name == "Result")
                {
                    rNode = element;
                    break;
                }
            }

            Assert.AreEqual(rNode.Value, "Successful");
        }

        #endregion Resource Upload Positive Test Cases

        #region Resource Upload Negative Test Cases

        #endregion Resource Upload Negative Test Cases
    }
}
