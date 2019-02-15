using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Instrumentation.Factory.Tests
{
    [TestClass()]
    public class ApplicationTrackerFactoryTests
    {
        [TestMethod()]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ApplicationTrackerFactory))]
        public void ApplicationTrackerFactory_GetApplicationTrackerProviderTest()
        {
            Assert.IsNull(ApplicationTrackerFactory.ApplicationTracker);
            IApplicationTracker applicationTracker = ApplicationTrackerFactory.GetApplicationTrackerProvider();
            Assert.IsNotNull(applicationTracker, "Unable to get RevulyticsTracker");
            Assert.IsNotNull(ApplicationTrackerFactory.ApplicationTracker);
        }
    }
}