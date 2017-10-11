using Dev2.Common;
using Dev2.Util;
using RUISDK_5_1_0;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Dev2.Instrumentation
{
    /// <summary>
    /// Revulytics -User action tracking 
    /// </summary>
    public class RevulyticsTracker : IApplicationTracker
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

        //to check if object is not in use
        private static object _syncLock = new object();
     
        /// <summary>
        /// set the variables by reading config file
        /// </summary>
        protected RevulyticsTracker()
        {
            this._sdkFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RUISDK)).Location);
            this._configFilePath = AppSettings.ConfigFilePath;
            this._productId = AppSettings.ProductID;
            this._appName = AppSettings.AppName;
            this._productUrl = AppSettings.ProductUrl;
            this._aesHexKey = AppSettings.AesHexKey;
           
        }

        /// <summary>
        /// This function will return single instance of action tracking object
        /// through out the application life cycle.
        /// </summary>
        /// <returns>RevulyticsTracker object</returns>
        public static RevulyticsTracker GetTrackerInstance()
        {

            if (_revulyticsTrackerInstance == null)
            {
                lock (_syncLock)
                {
                    if (_revulyticsTrackerInstance == null)
                    {
                        _revulyticsTrackerInstance = new RevulyticsTracker();
                    }
                }
            }

            return _revulyticsTrackerInstance;

        }

        /// <summary>
        /// This function create config file for revulytics tracking.       
        /// And call the SetProductVersion and StartSession methods
        /// </summary>
        /// <param name="productVersion">current product version of the application</param>
        /// <param name="username">login user</param>
        public void EnableAppplicationTracker(string productVersion, string username)
        {
            this.ProductVersion = productVersion;
            this.Username = username;

            RUIResult result = CreateRevulyticsConfig();
            WriteError(result);

            SetProductVersion();
            StartSession();

        }

      
        /// <summary>
        /// This function set the product version in revulytics.
        /// </summary>
        private void SetProductVersion()
        {
            RUIResult versionResult = _ruiSdk.SetProductVersion(this.ProductVersion);
            WriteError(versionResult);
        }

        /// <summary>
        /// This function will start the revultics session for login user.
        /// </summary>
        private void StartSession()
        {
            RUIResult startSessionResult = _ruiSdk.StartSession(this.Username);
            WriteError(startSessionResult);
        }

        /// <summary>
        /// This Function will stop the current session and action tracking.
        /// </summary>
        public void DisableAppplicationTracker()
        {
            StopSession();

            RUIResult result = _ruiSdk.StopSDK(10);
            WriteError(result);

        }

        /// <summary>
        /// This function will stop the revultics session for login user.
        /// </summary>
        private void StopSession()
        {
            RUIResult stopSessionResult = _ruiSdk.StopSession(this.Username);
            WriteError(stopSessionResult);
        }


        /// <summary>
        /// This function will log the Main menu event category in the revulytics log.
        /// If CollectUsageStats set to true then only it will log to revulytics
        ///Below is standard comment for TrackEventText method provided by revulytics.
        ///* Conditioning: All leading white space is removed.
        ///* Conditioning: All trailing white space is removed.
        ///* Conditioning: All internal white spaces other than space characters(' ') are removed.
        ///* Conditioning: Trimmed to a maximum of 128 UTF8 characters.
        ///* Validation: eventCategory can be empty; eventName cannot be empty.
        /// </summary>
        /// <param name="eventName">Action name</param>
        public void TrackApplicationEvent(string eventName)
        {
            if (AppSettings.CollectUsageStats)
            {
                RUIResult result = _ruiSdk.TrackEventText(ApplicationTrackerConstants.TrackerEventGroup.MainMenuClicked, eventName, eventName, this.Username);

                WriteError(result);
            }
        }

        /// <summary>
        /// This function will log the event based on event category,event name in the revulytics log.
        /// If CollectUsageStats set to true then only it will log to revulytics
        ///Below is standard comment for TrackEventText method provided by revulytics.
        /// * Conditioning: All leading white space is removed.
        ///* Conditioning: All trailing white space is removed.
        ///* Conditioning: All internal white spaces other than space characters(' ') are removed.
        ///* Conditioning: Trimmed to a maximum of 128 UTF8 characters.
        ///* Validation: eventCategory can be empty; eventName cannot be empty.
        /// </summary>
        /// <param name="eventCategory">Category of event like,item dragged, tab opened</param>
        /// <param name="eventName">name of the object/action</param>
        /// <param name="customValues">values of object ot variable</param>

        public void TrackCustomEvent(string eventCategory, string eventName, string customValues)
        {
            if (AppSettings.CollectUsageStats)
            {
                RUIResult result = _ruiSdk.TrackEventText(eventCategory, eventName, customValues, this.Username);

                WriteError(result);
            }
        }

        /// <summary>
        /// This Function will create the config for revulytics action tracking
        /// </summary>
        /// <returns>RUIResult object</returns>
        private RUIResult CreateRevulyticsConfig()
        {
            _ruiSdk = new RUISDK(true, this._sdkFilePath);
            var result = _ruiSdk.CreateConfig(_configFilePath, _productId, _appName, _productUrl, _protocol, _aesHexKey, _multiSessionEnabled, _reachOutOnAutoSync);

            if (result == RUIResult.ok)
            {
                result = _ruiSdk.StartSDK();
            }

            return result;

        }

        /// <summary>
        /// This function will write into log if result is other than ok
        /// while calling the revulytics method
        /// </summary>
        /// <param name="result">RUIResult object</param>

        void WriteError(RUIResult result)
        {
            if (result != RUIResult.ok)
            {
                var errormMessage = string.Format("{0} :: Tracker Error -> {1}", DateTime.Now.ToString("g"), result);             

                Dev2Logger.Error(errormMessage, "Revulytics sdk error");
            }
        }

    }
}
