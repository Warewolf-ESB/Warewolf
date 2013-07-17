
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    /// <summary>
    /// Summary description for DynamicServicesHostTest
    /// </summary>
    [TestClass]
    public class DynamicServicesHostTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get;
            set;
        }

        #region EmptyToNull Test

        [TestMethod]
        public void TestDBNullInsert_Expected_clientID()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=insert");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue((result.IndexOf("<userID>") > 0));
        }

        [TestMethod]
        [TestCategory("WebURI, DB")]
        public void TestDBNullLogicNullValue_Expected_ZZZ_10Times()
        {
            // ensure we get the same result 10 times ;)
            for (int i = 0; i < 10; i++)
            {
                string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI,
                                                "IntegrationTestDBEmptyToNull", "testType=logic&nullLogicValue=");
                string result = TestHelper.PostDataToWebserver(postData);

                Assert.IsTrue((result.IndexOf("<result>ZZZ</result>") > 0), "Failed to pass null for empty");
            }
        }

        [TestMethod]
        public void TestDBNullLogicNotNullValue_Expected_AAA()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=logic&nullLogicValue=dummy");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue((result.IndexOf("<result>AAA</result>") > 0), "Failed to assign non-null value");
        }

        [TestMethod]
        public void TestDBNullLogicEmptyNullConvertOffValue_Expected_AAA()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=nullActive&nullLogicValue=");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue((result.IndexOf("<result>AAA</result>") > 0),"Assigned null, when it should have been empty");
        }

        [TestMethod]
        [Ignore]
        public void TestPluginNull_Expected_AnonymousSend()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestPluginEmptyToNull", "testType=nullActive&sender=");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue((result.IndexOf("Anonymous email sent") > 0));
        }

        [TestMethod]
        [Ignore]
        public void TestPluginNonNull_Expected_FromInResult()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestPluginEmptyToNull", "testType=nullActive&sender=test@domain.local");
            string result = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue((result.IndexOf("from test@domain.local") > 0));
        }

        #endregion EmptyToNull Test
    }
}
