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
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "INTEGRATION TEST SERVICES/OnError Test");
            const string Expected = @"<Result>PASS</Result>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData.Unescape(), Expected);
        }
    }
}
