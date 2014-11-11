
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
using Dev2.Common.ExtMethods;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for OnErrorTest
    /// </summary>
    [TestClass]
    public class OnErrorTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("OnError_OnError")]
        public void OnError_OnError_WhenInvokingDifferentServiceTypes_ExpectPASS()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/OnError Test");
            const string Expected = @"<Result>PASS</Result>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData.Unescape(), Expected);
        }
    }
}
