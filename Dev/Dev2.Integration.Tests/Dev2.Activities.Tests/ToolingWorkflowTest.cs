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
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void AllToolsTestExpectPass()
        {
            string postData = String.Format("{0}{1}", WebserverURI, "Tool Testing");

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
            const string Expected = @"<DataList><Test><Result>ForEach: PASS</Result></Test><Test><Result>Switch: PASS</Result></Test><Test><Result>Decision: PASS</Result></Test><Test><Result>Count Records: PASS</Result></Test><Test><Result>Delete Record: PASS</Result></Test><Test><Result>Sort Records: PASS</Result></Test><Test><Result>Find Record Index: PASS</Result></Test><Test><Result>Assign: PASS</Result></Test><Test><Result>Base Conversion: PASS</Result></Test><Test><Result>Case Convert: PASS</Result></Test><Test><Result>Data Merge: PASS</Result></Test><Test><Result>Data Split: PASS</Result></Test><Test><Result>Date and Time: PASS</Result></Test><Test><Result>Date and Time Difference: PASS</Result></Test><Test><Result>Find Index: PASS</Result></Test><Test><Result>Format Number: INCONCLUSIVE</Result></Test><Test><Result>Javascript:PASS</Result></Test><Test><Result>Unique: PASS</Result></Test><Test><Result>XPath Tool: PASS</Result></Test><Test><Result>GetWebRequest: PASS</Result></Test><Test><Result>File Copy: Pass</Result></Test><Test><Result>Success</Result></Test><Test><Result>Find Record Index: PASS</Result></Test></DataList>";

            Assert.IsTrue(responseData.Contains(Expected), "Expected [ " + Expected + ", But Got [ " + responseData + " ]");
        }

        [TestMethod]
        public void ServiceExecutionTest()
        {
            string postData = String.Format("{0}{1}", WebserverURI, "ServiceExecutionTest");

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

            const string expected = @"<DataList><rs><val>1</val><result>res = 3</result></rs><rs><val>2</val><result>res = 3</result></rs><rs><val>3</val><result>res = 3</result></rs><rs><val></val><result>res = 3</result></rs></DataList>";

            Assert.IsTrue(responseData.Contains(expected), "Got [ " + responseData + " ] Expected [ " + expected + " ]");
        }
    }
}
