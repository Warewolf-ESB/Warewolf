using Dev2.Common;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class AppUsageStatsTests
    {
        /// <summary>
        /// This test checks that CollectUsageStats is set to False on develop
        /// </summary>
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("RevulyticsCollectUsageStats")]
        public void RevulyticsCollectUsageStatsForServerIsFalseTest()
        {
            Assert.AreEqual(false, AppUsageStats.CollectUsageStats);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("RevulyticsCollectUsageStats")]
        public void RevulyticsCollectUsageStats_WhenNoConfigSetting_ShouldUseGlobalConstantValue()
        {
            var oldValue = ConfigurationManager.AppSettings["CollectUsageStats"];
            try
            {
                //setup for test
                ConfigurationManager.AppSettings["CollectUsageStats"] = null;
                GlobalConstants.CollectUsageStats = "True";
                //test
                Assert.AreEqual(true, AppUsageStats.CollectUsageStats);
            }
            finally
            {
                //cleanup
                ConfigurationManager.AppSettings["CollectUsageStats"] = oldValue;
                GlobalConstants.CollectUsageStats = null;
            }
        }
    }
}
