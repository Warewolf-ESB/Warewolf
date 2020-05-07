using Dev2.Common;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Tests.Runtime
{
    [TestClass]
    public class AppUsageStatsTests
    {
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
