using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for WorkflowInputMapping
    /// </summary>
    [TestClass]
    public class WorkflowInputMapping
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private readonly string WebserverURI = ServerSettings.WebserverURI;

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowsViaWeb_EnsureInputsAreRespected")]
        public void WorkflowsViaWeb_EnsureInputsAreRespected_ExpectFail()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", WebserverURI, "Bug_10685.xml?val=10");
            const string expected = @"<result>PASS</result>";
            //------------Execute Test---------------------------

            string actual = TestHelper.PostDataToWebserver(PostData);

            //------------Assert Results-------------------------
            StringAssert.Contains(actual, expected);

        }

    }
}
