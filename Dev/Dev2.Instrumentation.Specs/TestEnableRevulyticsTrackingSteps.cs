using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using RUISDK_5_1_0;
using TechTalk.SpecFlow;
using System.Configuration;

namespace Dev2.Instrumentation.SpecflowTests
{
    [Binding]
    public class TestEnableRevulyticsTrackingSteps
    {
        private RevulyticsTracker _tracker;
        [Given(@"I have revulytics instance")]
        public void GivenIHaveRevulyticsInstance()
        {
            _tracker = new RevulyticsTracker
            {
                SdkFilePath = "C:\\",
                ConfigFilePath = ConfigurationManager.AppSettings["ConfigFilePath"],
                ProductId = ConfigurationManager.AppSettings["ProductID"],
                AppName = ConfigurationManager.AppSettings["AppName"],
                ProductUrl = ConfigurationManager.AppSettings["ProductUrl"],
                AesHexKey = ConfigurationManager.AppSettings["AesHexKey"]
            };
        }
        
        [Given(@"I will call the EnableAppplicationTracker method")]
        public void GivenIWillCallTheEnableAppplicationTrackerMethod()
        {
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            _tracker.EnableAppplicationTracker(productVersion,username);
        }
        
        [Then(@"I will check the status of revulytics tracker")]
        public void ThenIWillCheckTheStatusOfRevulyticsTracker()
        {
            Assert.AreEqual(_tracker.EnableApplicationResultStatus, RUIResult.ok);
        }
    }
}
