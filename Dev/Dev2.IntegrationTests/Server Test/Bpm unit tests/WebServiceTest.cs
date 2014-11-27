
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for WebServiceTest
    /// </summary>
    [TestClass]
    public class WebServiceTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebService_Invoke")]
        // ReSharper disable InconsistentNaming
        public void WebService_Invoke_IntegrationTest_ExpectPass()
        {

            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/WebServiceTest");

            const string expected = @"<result>PASS</result>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(postData);

            ////------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected, " **** I expected { " + expected + " } but got { " + ResponseData + " }");
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebService_Invoke_IncorrectInput")]
        public void WebService_Invoke_IntegrationTest_ExpectCorrectErrorsForBadInputs()
        {
            var id = Guid.NewGuid();
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/11365_WebService");

            //------------Execute Test---------------------------
            TestHelper.PostDataToWebserverAsRemoteAgent(postData, id);
            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);
            ////------------Assert Results-------------------------

            Assert.AreEqual(debugItems.Count, 1);
            StringAssert.Contains(debugItems[0].ErrorMessage.Trim(), @"1 The '[' character, hexadecimal value 0x5B, cannot be included in a name. Line 5, position 4.
 2 Recordset index (**) contains invalid character(s)
 3 Invalid Recordset Index For { [[rec(**).A]] }
 4 Could not evaluate { [[rec(**).A]] }");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebService_Invoke_IncorrectInput")]
        public void WebService_Invoke_IntegrationTest_ExpectCorrectErrorsForBadInputsExtraBrackets()
        {
            var id = Guid.NewGuid();
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/11365AltSyntax");

            //------------Execute Test---------------------------
            TestHelper.PostDataToWebserverAsRemoteAgent(postData, id);
            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);
            ////------------Assert Results-------------------------

            Assert.AreEqual(debugItems.Count, 2);
            Assert.AreEqual(debugItems[1].ErrorMessage.Trim(), @"1 The '[' character, hexadecimal value 0x5B, cannot be included in a name. Line 5, position 4.");
        }
    }
    // ReSharper restore InconsistentNaming
}
