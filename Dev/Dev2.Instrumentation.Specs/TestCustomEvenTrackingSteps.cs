using System;
using Dev2.Instrumentation.Factory;
using TechTalk.SpecFlow;

namespace Dev2.Instrumentation.SpecflowTests
{
    [Binding]
    public class TestCustomEvenTrackingSteps
    {
        private IApplicationTracker _applicationTracker;
        [Given(@"I have application Tracker instance")]
        public void GivenIHaveApplicationTrackerInstance()
        {
            _applicationTracker = ApplicationTrackerFactory.GetApplicationTrackerProvider();
        }
        
        [Given(@"I will enable application tracking by calling method")]
        public void GivenIWillEnableApplicationTrackingByCallingMethod()
        {
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            _applicationTracker.EnableAppplicationTracker(productVersion, username);
        }

        [When(@"I will call track custom event method")]
        public void ThenIwillcalltrackcustomeventmethod()
        {
            _applicationTracker.TrackCustomEvent("Test custom Event Category", "Testing Event","custom values");

        }

        [Then(@"I will disable application tracking by calling method")]
        public void ThenIWillDisableApplicationTrackingByCallingMethod()
        {
            _applicationTracker.DisableAppplicationTracker();
        }
    }
}
