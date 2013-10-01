using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Activities
{
    /// <summary>
    /// Please place any bug integration test not specific to tooling here ;)
    /// </summary>
    [TestClass]
    public class GeneralWorkflowTest
    {
        readonly string _webserverUri = ServerSettings.WebserverURI;

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RecordsetMapping_NestedWorkflows")]
        // Ensure we can map portions of a recordset as input and other portionas as output
        public void RecordsetMapping_NestedWorkflows_MixedInputAndOutput_ExpectValidResult()
        {

            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", _webserverUri, "Bug_10247_Outter");
            string expected = @"<rs><result>2</result></rs><rs><result>3</result></rs>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------

            // Standardise the outputs (Remove newlines, etc)
            expected = TestHelper.CleanUp(expected);
            responseData = TestHelper.CleanUp(responseData);
            StringAssert.Contains(responseData, expected);
  
        }

    }
}
