﻿using Dev2.Instrumentation.Factory;
using TechTalk.SpecFlow;

namespace Dev2.Instrumentation.SpecflowTests
{
    [Binding]
    public class TestRevulyticsApplicationEventTestSteps
    {
        private IApplicationTracker _applicationTracker;
        [Given(@"I have Application Tracker instance")]
        public void GivenIHaveApplicationTrackerInstance()
        {
            _applicationTracker = ApplicationTrackerFactory.GetApplicationTrackerProvider();
        }

        [Given(@"I will enable application tracking")]
        public void GivenIWillEnableApplicationTracking()
        {
            const string productVersion = "1.0.0.0";
            const string username = "windows\\raju";
            _applicationTracker.EnableApplicationTracker(productVersion, username);
        }

        [When(@"I call the track event method with event category and event name")]
        public void WhenICallTheTrackEventMethodWithEventCategoryAndEventName()
        {
            _applicationTracker.TrackEvent("Test Application Event Category", "Testing Event");
        }

        [Then(@"I will disable application tracking")]
        public void ThenIWillDisableApplicationTracking()
        {
            _applicationTracker.DisableApplicationTracker();
        }
    }
}
