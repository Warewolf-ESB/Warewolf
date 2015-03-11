
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Common.ExtMethods;
using Dev2.Integration.Tests;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Load.Tests
{
    /// <summary>
    /// Summary description for ActivityLoadTest
    /// </summary>
    [TestClass]
    public class ActivityLoadTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Plugins_PrimitiveTypes")]
        public void ForEach_Load_WhenSelfLoopedExecutionCausesMultipleForeachExecutions_ExpectPASS()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Load Test Resources/ForEachLoadTest?exeLoopCount=3");
            const string expected = @"<Result>PASS</Result>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(responseData.Unescape(), expected);
        }
    }
}
