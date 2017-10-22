using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using RUISDK_5_1_0;
using System.Configuration;

namespace Dev2.Instrumentation.Tests
{
    [TestClass()]
    public class RevulyticsTrackerTests
    {
        /// <summary>
        /// This method is used to read correct config needed in test cases
        /// </summary>
        /// <returns>RevulyticsTracker object</returns>
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

        /// <summary>
        /// This method is used to read incorrect config needed in test cases
        /// </summary>
        /// <returns>RevulyticsTracker object</returns>
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

        /// <summary>
        /// This exception occcur when require revulytics dll is not present or version is different
        /// in the running application directory below are the dlls.
        /// ruiSDK_5.1.0.x86.dll ruiSDK_5.1.0.x64.dll
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(RUISDKCreationException), "Error in initializing rui sdk")]
        public void CreateRevulyticsConfigTestSdkException()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            tracker.CreateRevulyticsConfig();
        }

        /// <summary>
        /// This exception occur whenever paramerter required for the revulytics config is 
        /// null or empty.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException), "Error in config keys rui sdk")]
        public void CreateRevulyticsConfigTestArgumentNullException()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            tracker.CreateRevulyticsConfig();
        }

        /// <summary>
        /// This test check to revulytics config is valid or not
        /// </summary>
        [TestMethod()]
        public void CreateRevulyticsConfigTest()
        {
            var tracker = GetRevulyticsTracker();
            var configResult = tracker.CreateRevulyticsConfig();
            Assert.AreEqual(configResult, RUIResult.ok, "Revultyics config created");
        }

        /// <summary>
        /// This Test case check the status of StartSdk method to ensure revultyics 
        /// sdk is started.
        /// Test case will fail when status in other than ok. 
        /// </summary>
        [TestMethod()]
        public void StartSdkTest()
        {
            var tracker = GetRevulyticsTracker();
            tracker.CreateRevulyticsConfig();
            var startSdkResult = tracker.StartSdk();
            Assert.AreEqual(startSdkResult, RUIResult.ok, "Error in starting sdk");
            
        }

        /// <summary>
        /// This Test case check the status of StopSdk method to ensure revultyics 
        /// started sdk is stopped.
        /// Test case will fail when status in other than ok. 
        /// </summary>
        [TestMethod()]
        public void StopSdkTest()
        {
            var tracker = GetRevulyticsTracker();
            tracker.CreateRevulyticsConfig();
            tracker.StartSdk();
            var stopResult = tracker.StopSdk();
            Assert.AreEqual(stopResult, RUIResult.ok, "Error in stopping sdk");
        }

        /// <summary>
        /// This Test case check the status of StartSession method to ensure revultyics 
        /// session is started.
        /// Test case will fail when status in other than ok. 
        /// </summary>
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

        /// <summary>
        /// This Test case check the status of StopSession method to ensure revultyics 
        /// started session is stopped.
        /// Test case will fail when status in other than ok. 
        /// </summary>
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

        /// <summary>
        /// Before logging any events to revulytics,product version need to be set.
        /// This test set the product version and check the status.
        /// Test case will fail when status in other than ok. 
        /// </summary>

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

        /// <summary>
        /// This exception occur whenever paramerter required for the revulytics config is 
        /// null or empty.
        /// </summary>
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

        /// <summary>
        /// Revultyics config must be created and sdk must be started before logging any event.
        /// This Test will check the status of revultics with correct config provided.
        /// The outcome must be running to validate the test case.
        /// </summary>
        [TestMethod()]
        public void EnableAppplicationTrackerTestWithCorrectonfig()
        {
            var tracker = GetRevulyticsTracker();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            Assert.AreEqual(tracker.RuiSdk.GetState(), RUIState.running, "Revulytics Started");

        }
        /// <summary>
        /// Revultyics config must be created and sdk must be started before logging any event.
        /// This Test will check the status of revultics config creation with 
        /// in correct config provided.
        /// </summary>
        [TestMethod()]
        public void EnableAppplicationTrackerTestWithInCorrectonfig()
        {
            var tracker = GetRevulyticsTrackerWithIncorrectConfig();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            Assert.AreNotEqual(tracker.EnableApplicationResultStatus, RUIResult.ok, "Config is not created");
        }

        /// <summary>
        /// This test is to track application event
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

        /// <summary>
        /// This test is to track custom event
        /// </summary>
        [TestMethod()]
        public void TrackCustomEventTest()
        {
            var tracker = RevulyticsTracker.GetTrackerInstance();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            tracker.TrackCustomEvent("Test Event", "Unit Test", "custom values");
        }

        /// <summary>
        /// Revultyics logging must be stopped once application closed.
        /// This Test will check the status of revultics logging to stopped.
        /// </summary>
        [TestMethod()]
        public void DisableAppplicationTrackerTest()
        {
            var tracker = GetRevulyticsTracker();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion,username);
            tracker.DisableAppplicationTracker();
            Assert.AreEqual(tracker.RuiSdk.GetState(), RUIState.stopped, "Revulytics stopped");
        }

        /// <summary>
        /// Revultyics logging must be stopped once application closed.
        /// This Test will ensure exception is handled.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(Exception))]
        public void DisableAppplicationTrackerExceptionTest()
        {
            var tracker = GetRevulyticsTracker();
            string productVersion = "1.0.0.0";
            string username = "windows\\raju";
            tracker.EnableAppplicationTracker(productVersion, username);
            tracker.RuiSdk = null;
            tracker.DisableAppplicationTracker();
           
        }
    }
}