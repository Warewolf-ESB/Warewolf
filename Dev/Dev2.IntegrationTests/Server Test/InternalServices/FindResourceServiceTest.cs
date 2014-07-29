using System.Xml;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindResourceServiceTest
    /// </summary>
    [TestClass]
    public class FindResourceServiceTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ExecutionWithStar_ExpectedValidXML()
        {

            string path = ServerSettings.WebserverURI + "FindResourcesService?ResourceName=*&ResourceType=*&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExecutionWithSource_ExpectedValidXML()
        {

            string path = ServerSettings.WebserverURI + "FindResourcesService?ResourceName=*&ResourceType=Source&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExecutionWithWorkflowService_ExpectedValidXML()
        {

            string path = ServerSettings.WebserverURI + "FindResourcesService?ResourceName=*&ResourceType=WorkflowService&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExecutionWithActivity_ExpectedValidXML()
        {

            string path = ServerSettings.WebserverURI + "FindResourcesService?ResourceName=*&ResourceType=Activity&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }
    }
}
