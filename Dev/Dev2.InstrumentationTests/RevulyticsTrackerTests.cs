using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Instrumentation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RUISDK_5_1_0;
using System.Configuration;

namespace Dev2.Instrumentation.Tests
{
    [TestClass()]
    public class RevulyticsTrackerTests
    {





        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        public void TrackEventTest()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            tracker.TrackEvent("Test Event", "Unit Test");

        }

        [TestMethod()]
        public void TrackCustomEventTest()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            tracker.TrackCustomEvent("Test Event", "Unit Test", "custom values");

        }

        [TestMethod()]
        [ExpectedException(typeof(RUISDKCreationException), "Error in initializing rui sdk")]
        public void CreateRevulyticsConfigTestSdkException()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            tracker.CreateRevulyticsConfig();

        }
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException), "Error in config keys rui sdk")]
        public void CreateRevulyticsConfigTestArgumentNullException()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            tracker.CreateRevulyticsConfig();

        }

        [TestMethod()]
        public void CreateRevulyticsConfigTest()
        {
            var tracker = GetRevulyticsTracker();
            var configResult = tracker.CreateRevulyticsConfig();
            Assert.AreEqual(configResult, RUIResult.ok, "Revultyics config created");
        }

        private RevulyticsTracker GetRevulyticsTracker()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            tracker.SdkFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RUISDK)).Location);
            tracker.ConfigFilePath = ConfigurationManager.AppSettings["ConfigFilePath"];
            tracker.ProductId = ConfigurationManager.AppSettings["ProductID"];
            tracker.AppName = ConfigurationManager.AppSettings["AppName"];
            tracker.ProductUrl = ConfigurationManager.AppSettings["ProductUrl"];
            tracker.AesHexKey = ConfigurationManager.AppSettings["AesHexKey"];

            return tracker;
        }

        private RevulyticsTracker GetRevulyticsTrackerWithIncorrectConfig()
        {
            var tracker = new RevulyticsTracker();
            tracker.SdkFilePath = "D:\\";
            tracker.ConfigFilePath = ConfigurationManager.AppSettings["IConfigFilePath"];
            tracker.ProductId = ConfigurationManager.AppSettings["IProductID"];
            tracker.AppName = ConfigurationManager.AppSettings["IAppName"];
            tracker.ProductUrl = ConfigurationManager.AppSettings["IProductUrl"];
            tracker.AesHexKey = ConfigurationManager.AppSettings["IAesHexKey"];

            return tracker;
        }

        [TestMethod()]
        public void StartSdkTest()
        {
            var tracker = GetRevulyticsTracker();
            tracker.CreateRevulyticsConfig();
            var startSdkResult = tracker.StartSdk();
            Assert.AreEqual(startSdkResult, RUIResult.ok, "Error in starting sdk");
        }

        [TestMethod()]
        public void StopSdkTest()
        {
            var tracker = GetRevulyticsTracker();
            tracker.CreateRevulyticsConfig();
            tracker.StartSdk();
            var stopResult = tracker.StopSdk();
            Assert.AreEqual(stopResult, RUIResult.ok, "Error in stopping sdk");

        }

        [TestMethod()]
        public void StartSessionTest()
        {
            var tracker = GetRevulyticsTracker();
            tracker.CreateRevulyticsConfig();
            tracker.StartSdk();
            tracker.Username = "windows\\raju";
            var startSessionResult = tracker.StartSession();
            Assert.AreEqual(startSessionResult, RUIResult.ok, "Error in starting session sdk");
        }

        [TestMethod()]
        public void StopSessionTest()
        {
            var tracker = GetRevulyticsTracker();
            tracker.CreateRevulyticsConfig();
            tracker.StartSdk();
            tracker.Username = "windows\\raju";
            tracker.StartSession();

            var stopSessionResult = tracker.StopSession();
            Assert.AreEqual(stopSessionResult, RUIResult.ok, "Error in stopping session sdk");
        }

        [TestMethod()]
        public void SetProductVersionTest()
        {
            var tracker = GetRevulyticsTracker();
            tracker.CreateRevulyticsConfig();
            tracker.StartSdk();
            tracker.ProductVersion = "1.0.0.0";
            var result = tracker.SetProductVersion();
            Assert.AreEqual(result, RUIResult.ok, "Error in setting product version");
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(Exception), "Exceptions handled")]
        public void EnableAppplicationTrackerException()
        {
            var tracker = GetRevulyticsTrackerWithIncorrectConfig();

            string productVersion = "1.0.0.0";
            string username ="windows\\raju";
            
            tracker.EnableAppplicationTracker(productVersion, username);

        }


        /// <summary>
        /// This exception occcur when require revulytics dll is not present or version is different
        /// in the running application directory below are the dlls.
        /// ruiSDK_5.1.0.x86.dll ruiSDK_5.1.0.x64.dll
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(RUISDKCreationException), "RUISDKCreation Exception handled")]
        public void EnableAppplicationTrackerSdkException()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException), "Argument null exception handled")]
        public void EnableAppplicationTrackerArgumentNullException()
        {
            var tracker = GetRevulyticsTrackerWithIncorrectConfig();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.SdkFilePath = "";
            tracker.EnableAppplicationTracker(productVersion, username);

        }

        [TestMethod()]
        public void EnableAppplicationTrackerTestWithCorrectonfig()
        {
            var tracker = GetRevulyticsTracker();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            Assert.AreEqual(tracker.RuiSdk.GetState(), RUIState.running, "Revulytics Started");

        }

        [TestMethod()]
        public void EnableAppplicationTrackerTestWithInCorrectonfig()
        {
            var tracker = GetRevulyticsTrackerWithIncorrectConfig();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            Assert.AreNotEqual(tracker.EnableApplicationResult, RUIResult.ok, "Config is not created");

        }

        [TestMethod()]
        public void DisableAppplicationTrackerTest()
        {
            var tracker = GetRevulyticsTracker();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion,username);
            tracker.DisableAppplicationTracker();
            Assert.AreEqual(tracker.RuiSdk.GetState(), RUIState.stopped, "Revulytics stopped");
            //  Assert.Fail();
        }

        [TestMethod()]
        public void EnableAppplicationTrackerSdkStartTest()
        {
            var tracker = GetRevulyticsTracker();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.CreateRevulyticsConfig();
            tracker.StartSdk();
            tracker.EnableAppplicationTracker(productVersion, username);
            Assert.AreEqual(tracker.EnableApplicationResult, RUIResult.ok, "Config created");

        }
    }
}