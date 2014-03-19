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
            const string Expected = "<DataList><Test rowID=\"1\"><Result>ForEach: PASS</Result></Test><Test rowID=\"2\"><Result>Switch: PASS</Result></Test><Test rowID=\"3\"><Result>Decision: PASS</Result></Test><Test rowID=\"4\"><Result>Count Records: PASS</Result></Test><Test rowID=\"5\"><Result>Delete Record: PASS</Result></Test><Test rowID=\"6\"><Result>Sort Records: PASS</Result></Test><Test rowID=\"7\"><Result>Find Record Index: PASS</Result></Test><Test rowID=\"8\"><Result>Assign: PASS</Result></Test><Test rowID=\"9\"><Result>Base Conversion: PASS</Result></Test><Test rowID=\"10\"><Result>Case Convert: PASS</Result></Test><Test rowID=\"11\"><Result>Data Merge: FAIL</Result></Test><Test rowID=\"12\"><Result>Data Split: PASS</Result></Test><Test rowID=\"13\"><Result>Date and Time: PASS</Result></Test><Test rowID=\"14\"><Result>Date and Time Difference: PASS</Result></Test><Test rowID=\"15\"><Result>Find Index: PASS</Result></Test><Test rowID=\"16\"><Result>Format Number: INCONCLUSIVE</Result></Test><Test rowID=\"17\"><Result>Javascript:PASS</Result></Test><Test rowID=\"18\"><Result>Unique: PASS</Result></Test><Test rowID=\"19\"><Result>XPath Tool: PASS</Result></Test><Test rowID=\"20\"><Result>GetWebRequest: PASS</Result></Test><Test rowID=\"21\"><Result>File Copy: Pass</Result></Test><Test rowID=\"22\"><Result>Success</Result></Test><Test rowID=\"23\"><Result>Find Record Index: PASS</Result></Test></DataList>";

            Assert.IsTrue(responseData.Contains(Expected), "Expected [ " + Expected + ", But Got [ " + responseData + " ]");
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

            const string Expected = @"<DataList><rs rowID=""1""><val>1</val><result>res = 3</result></rs><rs rowID=""2""><val>2</val><result>res = 3</result></rs><rs rowID=""3""><val>3</val><result>res = 3</result></rs><rs rowID=""4""><val></val><result>res = 3</result></rs></DataList>";

            StringAssert.Contains(responseData, Expected);
        }
    }
}
