
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
using System.IO;
using System.Threading;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
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
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/BUG_10995.xml?Name=Bob");

            const string expected = "<DataList><Result>Mr Bob</Result></DataList>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            //------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InvokeWorkflow_ViaBrowser")]
        public void InvokeWorkflowViaBrowser_ViaBrowser_WhenInputWithWorkspaceId_ExpectValidResult()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/ExecuteWithWorkspaceId.xml?<DataList><Cipher><char>a</char></Cipher><Cipher><char>b</char></Cipher><Cipher><char>c</char></Cipher></DataList>&wid=00000000-0000-0000-0000-000000000000");

            const string expected = "<DataList><a index=\"1\"><val>a</val></a><a index=\"2\"><val>b</val></a><a index=\"3\"><val>c</val></a></DataList>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            //------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("InvokeWorkflow_ViaBrowser")]
        public void InvokeWorkflow_ViaBrowser_HasWorkflowWithExecuteAsync_ShouldReturnImmediately()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/11536_FireForget.xml");
            const string pathForFileWrittenForTest = "C:\\Testing\\FireForgetText.txt";
            if(File.Exists(pathForFileWrittenForTest))
            {
                File.Delete(pathForFileWrittenForTest);
            }
            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(responseData, "<result>PASS</result><fireforgetres></fireforgetres>");
            //------------Assert Results-------------------------
            var exists = File.Exists(pathForFileWrittenForTest);
            Assert.IsFalse(exists);
            Thread.Sleep(4500); // 5 second wait for the async workflow to finish
            exists = File.Exists(pathForFileWrittenForTest);
            Assert.IsTrue(exists);
            var contentsOfFile = File.ReadAllText(pathForFileWrittenForTest);
            Assert.AreEqual("200", contentsOfFile);
            File.Delete(pathForFileWrittenForTest);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("InvokeWorkflow_ViaBrowser")]
        public void InvokeWorkflow_ViaBrowser_HasWorkflowWithExecuteAsync_WithError_ShouldReturnErrorImmediately()
        {
            //------------Setup for test--------------------------
            var PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/11536_FireForget_Error.xml");
            //------------Execute Test---------------------------
            var responseData = TestHelper.PostDataToWebserver(PostData);
            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, "Asynchronous execution failed: Resource not found");
        }
    }
}
