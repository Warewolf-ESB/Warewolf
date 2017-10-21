using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RUISDK_5_1_0;
using TechTalk.SpecFlow;

namespace Dev2.Instrumentation.SpecflowTests
{
    [Binding]
    public class TestRevultyicsWithCorrectConfigurationSteps
    {
        private RevulyticsTracker _tracker;
        private RUIResult _configResult;
        [Given(@"I have app config file I will read the revulytics app config keys")]
        public void GivenIHaveAppConfigFileIWillReadTheRevulyticsAppConfigKeys()
        {
            _tracker = new RevulyticsTracker
            {
                SdkFilePath = "D:\\",
                ConfigFilePath = ConfigurationManager.AppSettings["IConfigFilePath"],
                ProductId = ConfigurationManager.AppSettings["IProductID"],
                AppName = ConfigurationManager.AppSettings["IAppName"],
                ProductUrl = ConfigurationManager.AppSettings["IProductUrl"],
                AesHexKey = ConfigurationManager.AppSettings["IAesHexKey"]
            };


        }
        
        [Given(@"I will create revultyics configuration")]
        [ExpectedException(typeof(ArgumentNullException), "Error in config keys rui sdk")]
        public void GivenIWillCreateRevultyicsConfiguration()
        {
         _configResult=   _tracker.CreateRevulyticsConfig();
        }
        
        [Then(@"I will check the status of revulytics configuration")]
        public void ThenIWillCheckTheStatusOfRevulyticsConfiguration()
        {
            Assert.AreEqual(_configResult,RUIResult.ok,"Config created");
        }
    }
}
