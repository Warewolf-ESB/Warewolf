/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Xml;
using Dev2.Common;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindResourceServiceTest
    /// </summary>
    [TestClass]
    public class FindResourceServiceTest
    {
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

            string path = "http://localhost:3142/services/" + "FindResourcesService?ResourceName=*&ResourceType=*&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExecutionWithNoStartNode_ExpectedInvalidValidResult()
        {

            string path = "http://localhost:3142/services/" + "Acceptance%20Testing%20Resources/WorkflowWithNoStartNodeConnected";

            string result = TestHelper.PostDataToWebserver(path);

            Assert.IsTrue(result.Contains("An internal error occurred while executing the service request"));
            Assert.IsTrue(result.Contains(GlobalConstants.NoStartNodeError));
        }

        [TestMethod]
        public void ExecutionWithSource_ExpectedValidXML()
        {

            string path = "http://localhost:3142/services/" + "FindResourcesService?ResourceName=*&ResourceType=Source&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExecutionWithWorkflowService_ExpectedValidXML()
        {

            string path = "http://localhost:3142/services/" + "FindResourcesService?ResourceName=*&ResourceType=WorkflowService&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ExecutionWithActivity_ExpectedValidXML()
        {

            string path = "http://localhost:3142/services/" + "FindResourcesService?ResourceName=*&ResourceType=Activity&Roles=*";

            string result = TestHelper.PostDataToWebserver(path);

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(result);
            // 1 == 1, else an error will be thrown
            Assert.IsTrue(true);
        }
    }
}
