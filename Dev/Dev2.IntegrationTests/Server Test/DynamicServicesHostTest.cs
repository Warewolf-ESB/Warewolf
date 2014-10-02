
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
// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    /// <summary>
    /// Summary description for DynamicServicesHostTest
    /// </summary>
    [TestClass]
    public class DynamicServicesHostTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get;
            set;
        }

        [TestMethod]
        public void TestPluginNull_Expected_AnonymousSend()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "ML-Testing/IntegrationTestPluginEmptyToNull", "testType=nullActive");
            string result = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(result, "Anonymous email sent");
        }

        [TestMethod]
        public void TestPluginNonNull_Expected_FromInResult()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "ML-Testing/IntegrationTestPluginEmptyToNull", "testType=nullActive&sender=test@domain.local");
            string result = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(result, "from test@domain.local");

        }


        [TestMethod]
        public void WorkflowWithPluginActivity_Integration_ExpectedReturnsPluginData()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "PBI9135PluginServiceTest", "");
            //------------Execute Test---------------------------
            string result = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            const string expectedReturnValue = @"<Name>Dev2</Name><Departments index=""1""><Name>Dev</Name></Departments><Departments index=""2""><Name>Accounts</Name></Departments><Departments_Employees index=""1""><Name>Brendon</Name></Departments_Employees><Departments_Employees index=""2""><Name>Jayd</Name></Departments_Employees><Departments_Employees index=""3""><Name>Bob</Name></Departments_Employees><Departments_Employees index=""4""><Name>Jo</Name></Departments_Employees>";

            StringAssert.Contains(result, expectedReturnValue);
        }
    }
}
