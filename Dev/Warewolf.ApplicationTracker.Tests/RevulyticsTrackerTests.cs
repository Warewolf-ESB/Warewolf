using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Instrumentation;
using RUISDK_5_1_0;
using System.IO;
using System.Reflection;
using System.Configuration;
using System;


namespace Warewolf.ApplicationTracker.Tests
{
    [TestClass()]
    public class RevulyticsTrackerTests
    {

        // current instance of the Revulytics
        private static RevulyticsTracker _revulyticsTrackerInstance;
        RUISDK _ruiSdk;

        string _sdkFilePath;

        string _configFilePath;

        string _productId;

        string _appName;

        string _productUrl;

        int _protocol = 1;

        string _aesHexKey;

        bool _multiSessionEnabled = true;

        bool _reachOutOnAutoSync = true;

        string Username { get; set; }

        string ProductVersion { get; set; }


        public RevulyticsTrackerTests()
        {
            this._sdkFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RUISDK)).Location);
            this._configFilePath = ConfigurationManager.AppSettings["ConfigFilePath"];
            this._productId = ConfigurationManager.AppSettings["ProductID"];
            this._appName = ConfigurationManager.AppSettings["AppName"];
            this._productUrl = ConfigurationManager.AppSettings["ProductUrl"];
            this._aesHexKey = ConfigurationManager.AppSettings["AesHexKey"];


        }


        [TestMethod()]
        //[ExpectedException(typeof(RUISDKCreationException), "Error in intialzing rui sdk")]
        public void EnableAppplicationTrackerTest()
        {
            this.ProductVersion = "0.0.0.0";
            this.Username = "windows\\raju";
            RUIResult result = CreateRevulyticsConfig();

            Assert.AreEqual(result, RUIResult.ok, "Error in creating revulytics configuration");

            SetProductVersion();
            StartSession();

        }

        /// <summary>
        /// This Function will create the config for revulytics action tracking
        /// </summary>
        /// <returns>RUIResult object</returns>
        /// 
        private RUIResult CreateRevulyticsConfig()
        {
            var result = RUIResult.configNotCreated;
            _ruiSdk = new RUISDK(true, this._sdkFilePath);
            result = _ruiSdk.CreateConfig(_configFilePath, _productId, _appName, _productUrl, _protocol, _aesHexKey, _multiSessionEnabled, _reachOutOnAutoSync);

            if (result == RUIResult.ok)
            {
                result = _ruiSdk.StartSDK();
            }

            return result;

        }

        /// <summary>
        /// This function set the product version in revulytics.
        /// </summary>
        private void SetProductVersion()
        {
            RUIResult versionResult = _ruiSdk.SetProductVersion(this.ProductVersion);
            Assert.AreEqual(versionResult, RUIResult.ok, "Error in setting product version");

        }


        /// <summary>
        /// This function will start the revultics session for login user.
        /// </summary>
        private void StartSession()
        {
            RUIResult startSessionResult = _ruiSdk.StartSession(this.Username);
            Assert.AreEqual(startSessionResult, RUIResult.ok, "Error in starting session");
        }



        [TestMethod()]
        public void DisableAppplicationTrackerTest()
        {
            EnableAppplicationTrackerTest();
            StopSession();
            RUIResult result = _ruiSdk.StopSDK(10);
            Assert.AreEqual(result, RUIResult.ok, "Error in stopping sdk");
        }

        /// <summary>
        /// This function will stop the revultics session for login user.
        /// </summary>
        private void StopSession()
        {
            RUIResult stopSessionResult = _ruiSdk.StopSession(this.Username);
            Assert.AreEqual(stopSessionResult, RUIResult.ok, "Error in stopping session");
        }

        [TestMethod()]
        public void TrackEventTest()
        {
            EnableAppplicationTrackerTest();
            string category = "Main Menu Events";
            string eventName = "Test TrackEvent Methods";
            RUIResult result = _ruiSdk.TrackEvent(category, eventName, this.Username);

            Assert.AreEqual(result, RUIResult.ok, "Error in logging events in revulytics");
           
        }

        [TestMethod()]
        public void TrackCustomEventTest()
        {
            EnableAppplicationTrackerTest();
            string category = "Track Custom Event Test";
            string eventName = "Test Custom Methods with custom value";
            string customValue = "Test CustomEvent";
            RUIResult result = _ruiSdk.TrackEventText(category, eventName, customValue, this.Username);

            Assert.AreEqual(result, RUIResult.ok, "Error in logging events in revulytics");
        }

        [TestMethod()]
        public void GetTrackerInstanceTest()
        {
            Assert.Fail();
        }

        



    }
}