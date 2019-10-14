/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Web;
using TestBase;

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

            var path = "http://localhost:3142/services/" + "FindResourceService?ResourceName=*&ResourceType=*&Roles=*";

            var result = TestHelper.PostDataToWebserver(path);

            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(result);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void ExecutionWithNoStartNode_ExpectedInvalidValidResult()
        {

            var path = "http://localhost:3142/services/" + "Acceptance%20Testing%20Resources/WorkflowWithNoStartNodeConnected";

            string result = "";
            try
            {
                result = TestHelper.PostDataToWebserver(path);
            } catch (WebException e)
            {
                var errorContent = new StreamReader(e.Response.GetResponseStream());
                result = errorContent.ReadToEnd();
            }

            Assert.IsTrue(result.Contains("The workflow must have at least one service or activity connected to the Start Node."));
            Assert.IsTrue(result.Contains(GlobalConstants.NoStartNodeError));
        }

        [TestMethod]
        public void ExecutionWithSource_ExpectedValidXML()
        {

            var path = "http://localhost:3142/services/" + "FindResourceService?ResourceName=*&ResourceType=Source&Roles=*";

            var result = TestHelper.PostDataToWebserver(path);

            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(result);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void ExecutionWithWorkflowService_ExpectedValidXML()
        {

            var path = "http://localhost:3142/services/" + "FindResourceService?ResourceName=*&ResourceType=WorkflowService&Roles=*";

            var result = TestHelper.PostDataToWebserver(path);

            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(result);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void ExecutionWithActivity_ExpectedValidXML()
        {

            var path = "http://localhost:3142/services/" + "FindResourceService?ResourceName=*&ResourceType=Activity&Roles=*";

            var result = TestHelper.PostDataToWebserver(path);

            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(result);
            Assert.IsNotNull(json);
        }
    }
}
