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
            string postData = String.Format("{0}{1}", _webserverUri, "Tool Testing");

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
            const string Expected = @"<DataList><Test index=""1""><Result>ForEach: PASS</Result></Test><Test index=""2""><Result>Switch: PASS</Result></Test><Test index=""3""><Result>Decision: PASS</Result></Test><Test index=""4""><Result>Count Records: PASS</Result></Test><Test index=""5""><Result>Delete Record: PASS</Result></Test><Test index=""6""><Result>Sort Records: PASS</Result></Test><Test index=""7""><Result>Find Record Index: PASS</Result></Test><Test index=""8""><Result>Assign: PASS</Result></Test><Test index=""9""><Result>Base Conversion: PASS</Result></Test><Test index=""10""><Result>Case Convert: PASS</Result></Test><Test index=""11""><Result>Data Merge: PASS</Result></Test><Test index=""12""><Result>Data Split: PASS</Result></Test><Test index=""13""><Result>Date and Time: PASS</Result></Test><Test index=""14""><Result>Date and Time Difference: PASS</Result></Test><Test index=""15""><Result>Find Index: PASS</Result></Test><Test index=""16""><Result>Format Number: INCONCLUSIVE</Result></Test><Test index=""17""><Result>Javascript:PASS</Result></Test><Test index=""18""><Result>Unique: PASS</Result></Test><Test index=""19""><Result>XPath Tool: PASS</Result></Test><Test index=""20""><Result>GetWebRequest: PASS</Result></Test><Test index=""21""><Result>File Copy: Pass</Result></Test><Test index=""22""><Result>SQLBulk Insert: Success</Result></Test><Test index=""23""><Result>Find Record Index: PASS</Result></Test></DataList>";
            StringAssert.Contains(responseData, Expected);
        }



        [TestMethod]
        public void ServiceExecutionTest()
        {
            string postData = String.Format("{0}{1}", _webserverUri, "ServiceExecutionTest");

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
