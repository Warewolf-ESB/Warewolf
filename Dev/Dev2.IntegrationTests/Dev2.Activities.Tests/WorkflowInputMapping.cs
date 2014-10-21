
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
    // ReSharper disable InconsistentNaming

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
            string PostData = String.Format("{0}{1}", WebserverURI, "Integration Test Resources/Bug_10685.xml?val=10");
            const string expected = @"<result>PASS</result>";
            //------------Execute Test---------------------------

            string actual = TestHelper.PostDataToWebserver(PostData);

            //------------Assert Results-------------------------
            StringAssert.Contains(actual, expected);

        }

    }
}
