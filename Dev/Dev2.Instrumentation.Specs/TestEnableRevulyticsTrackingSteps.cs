using Microsoft.VisualStudio.TestTools.UnitTesting;
using RUISDK_5_3_1;
using TechTalk.SpecFlow;

namespace Dev2.Instrumentation.SpecflowTests
{
    [Binding]
    public class TestEnableRevulyticsTrackingSteps
    {
        private RevulyticsTracker _tracker;
        [Given(@"I have revulytics instance")]
        public void GivenIHaveRevulyticsInstance()
        {
             _tracker = RevulyticsTracker.GetTrackerInstance();
        }

        [Given(@"I will call the EnableAppplicationTracker method")]
        public void GivenIWillCallTheEnableAppplicationTrackerMethod()
        {
            const string productVersion = "1.0.0.0";
            const string username = "windows\\raju";
            _tracker.EnableApplicationTracker(productVersion, username);
        }

        [Then(@"I will check the status of revulytics tracker")]
        public void ThenIWillCheckTheStatusOfRevulyticsTracker()
        {
            Assert.AreEqual(_tracker.EnableApplicationResultStatus, RUIResult.ok);
        }
    }
}
