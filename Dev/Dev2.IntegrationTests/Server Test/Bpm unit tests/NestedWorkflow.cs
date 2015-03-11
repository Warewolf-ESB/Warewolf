
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Xml.Linq;
using Dev2.Integration.Tests.Dev2.Application.Server.Tests.Workspace.XML;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for DBServiceTest
    /// </summary>
    [TestClass]
    public class NestedWorkflow
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        readonly string WebserverURI = ServerSettings.WebserverURI;

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkflowService_Invoke")]
        public void WorkflowService_Invoke_WithNestedWorkflowHavingForEachDataListNotInSequence_ShouldStillMapCorrectly()
        {
            // 24-04-2014 
            // Some flipping joker keeps changing the data in this table. 
            // I have fixed the null lat and long ;)

            //------------Setup for test--------------------------
            var expectedXML = XmlResource.Fetch("Bug_10528_Result.xml");
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/Bug_10528");
            string expected = expectedXML.ToString(SaveOptions.None);
            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, XElement.Parse(ResponseData).ToString(SaveOptions.None));
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowService_Invoke")]
        public void WorkflowService_Invoke_WithNestedWorkflowNestedForEachAllSameRecordset_ShouldStillMapCorrectly()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/Inner Foreach Execution Type Test");
            string expected = "<Result>PASS</Result>";
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(PostData);
            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, expected);
        }

    }
}
