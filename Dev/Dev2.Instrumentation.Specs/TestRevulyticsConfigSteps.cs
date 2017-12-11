using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RUISDK_5_1_0;
using TechTalk.SpecFlow;

namespace Dev2.Instrumentation.SpecflowTests
{
    [Binding]
    public class TestRevulyticsConfigSteps
    {
        private RevulyticsTracker _tracker;
        private RUIResult _configResult;

        /// <summary>
        /// This method will read the keys from config file
        /// IProductID is left blank
        /// </summary>
        [Given(@"I have revulytics config and i will read the keys")]
        public void GivenIHaveRevulyticsConfigAndIWillReadTheKeys()
        {
            _tracker = new RevulyticsTracker
            {
                SdkFilePath = "C:\\ru\\",
                ConfigFilePath = ConfigurationManager.AppSettings["IConfigFilePath"],
                ProductId = ConfigurationManager.AppSettings["IProductID"],
                AppName = ConfigurationManager.AppSettings["IAppName"],
                ProductUrl = ConfigurationManager.AppSettings["IProductUrl"],
                AesHexKey = ConfigurationManager.AppSettings["IAesHexKey"]
            };


        }
        
        [Given(@"I will create revulytics configuration")]
        [ExpectedException(typeof(ArgumentNullException), "Error in config keys rui sdk")]
        public void GivenIWillCreateRevulyticsConfiguration()
        {
            _configResult = _tracker.CreateRevulyticsConfig();
        }
        
        [Then(@"I will check whether result is equal to ok")]
        public void ThenIWillCheckWhetherResultIsEqualToOk()
        {
            Assert.AreEqual(_configResult, RUIResult.invalidProductID);
        }
    }
}
