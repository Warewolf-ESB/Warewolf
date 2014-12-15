
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfCalculateActivityTest
    /// </summary>
    [TestClass]
    public class DsfCalculateActivityTest
    {
        private readonly string _webServerUri = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Calculate Static Values Tests

        [TestMethod] // - OK
        // ReSharper disable InconsistentNaming
        public void Calculate_Activity_StaticValueSum_Expected_CorrectlySummed()
        // ReSharper restore InconsistentNaming
        {
            string postData = string.Format("{0}{1}", _webServerUri, "Acceptance Testing Resources/Calculate_ScalarDataListItems_Multiplication");

            const string Expected = @"57.4456264653803";

            string actual = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(actual.Contains(Expected), actual + " does not contain " + Expected);
        }

        #endregion Calculate Static Values Tests

        #region Calculate RecordSet Tests

        [TestMethod] // - OK      
        // ReSharper disable InconsistentNaming
        public void Calculate_Activity_RecordSetEvaluation_Expected_RecordSetResolutionCorrectlyEvaluated()
        // ReSharper restore InconsistentNaming
        {
            string postData = string.Format("{0}{1}", _webServerUri, "Acceptance Testing Resources/Calculate_RecordSet_Subtract");
            const string Expected = @"<result>920</result>";

            string actual = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(actual.Contains(Expected), "Expected " + Expected + " but got " + actual);

        }

        #endregion Calculate RecordSet Tests
    }
}
