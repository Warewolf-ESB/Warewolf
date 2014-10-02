
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
using System.Net;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for ToolingWorkflowTest
    /// </summary>
    [TestClass]
    public class ToolingWorkflowTest
    {
        readonly string _webserverUri = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void AllToolsTestExpectPass()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "TEST/Tool Testing");

            var responseData = string.Empty;
            try
            {
                responseData = TestHelper.PostDataToWebserver(postData);
            }
            catch(WebException e)
            {
                if(e.Message.Contains("timed out"))
                {
                    Assert.Inconclusive("Tools test workflow took to long to run");
                }
            }

            // Find Record Index: PASS
            const string Expected = @"<DataList><Test index=""1""><Result>ForEach: PASS</Result></Test><Test index=""2""><Result>Switch: PASS</Result></Test><Test index=""3""><Result>Decision: PASS</Result></Test><Test index=""4""><Result>Count Records: PASS</Result></Test><Test index=""5""><Result>Delete Record: PASS</Result></Test><Test index=""6""><Result>Sort Records: PASS</Result></Test>";
            StringAssert.Contains(responseData, Expected);
        }



        [TestMethod]
        public void ServiceExecutionTest()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "TestCategory/ServiceExecutionTest");

            var responseData = string.Empty;
            try
            {
                responseData = TestHelper.PostDataToWebserver(postData);
            }
            catch(WebException e)
            {
                if(e.Message.Contains("timed out"))
                {
                    Assert.Inconclusive("Service execution test workflow took to long to run");
                }
            }

            const string Expected = @"<DataList><rs index=""1""><val>1</val><result>res = 3</result></rs><rs index=""2""><val>2</val><result>res = 3</result></rs><rs index=""3""><val>3</val><result>res = 3</result></rs><rs index=""4""><val></val><result>res = 3</result></rs></DataList>";

            StringAssert.Contains(responseData, Expected);
        }
    }
}
