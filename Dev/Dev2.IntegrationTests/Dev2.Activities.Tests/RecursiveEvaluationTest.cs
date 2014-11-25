
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

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for RecursiveEvaluationTest
    /// </summary>
    [TestClass]
    public class RecursiveEvaluationTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RecursiveEvaluation_WhenUsingDataMergeAndDataSplit")]
        // ReSharper disable InconsistentNaming
        public void RecursiveEvaluation_WhenUsingDataMergeAndDataSplit_WhenSpecFlowHasBugsCheckExection_ExpectPass()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/Bug_11889");

            //------------Execute Test---------------------------
            var result = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "PASS");

        }

    }
}
