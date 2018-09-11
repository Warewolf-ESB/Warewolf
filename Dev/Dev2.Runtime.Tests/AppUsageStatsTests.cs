using System;
using Dev2.Common;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class AppUsageStatsTests
    {
        /// <summary>
        /// This test checks that CollectUsageStats is set to False on develop
        /// </summary>
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("RevulyticsCollectUsageStats")]
        public void RevulyticsCollectUsageStatsForServerIsFalseTest()
        {
            Assert.AreEqual(false, AppUsageStats.CollectUsageStats);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("RevulyticsCollectUsageStats")]
        public void RevulyticsCollectUsageStats_WhenNoConfigSetting_ShouldUseGlobalConstantValue()
        {
            GlobalConstants.CollectUsageStats = "True";
            Assert.AreEqual(true, AppUsageStats.CollectUsageStats);
            GlobalConstants.CollectUsageStats = null;
        }
    }
}
