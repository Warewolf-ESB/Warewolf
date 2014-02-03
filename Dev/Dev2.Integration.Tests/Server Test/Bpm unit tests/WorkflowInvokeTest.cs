using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Server_Test.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for WorkflowInvokeTest
    /// </summary>
    [TestClass]
    public class WorkflowInvokeTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InvokeWorkflow_ViaBrowser")]
        public void InvokeWorkflowViaBrowser_ViaBrowser_WhenInputIsName_ExpectValidResult()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "BUG_10995.xml?Name=Bob");

            const string expected = "<DataList><Result>Mr Bob</Result></DataList>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            //------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected);
        }
    }
}
