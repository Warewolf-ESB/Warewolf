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
using Trackerbird.Tracker;

namespace Dev2.Instrumentation
{

 public class RevulyticsTracker : IApplicationTracker
    {
        // current instance of the Revuulytics
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
        private static object _syncLock = new object();


        // private object AppSettings;

        protected RevulyticsTracker()
        {
            this._sdkFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RUISDK)).Location);
            this._configFilePath = AppSettings.ConfigFilePath;

            this._productId = AppSettings.ProductID;
            this._appName = AppSettings.AppName;
            this._productUrl = AppSettings.ProductUrl;
            this._aesHexKey = AppSettings.AesHexKey;
            // this._configFilePath = AppSettings.ConfigFilePath;
            //RevulyticsCreateConfig();
        }

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
        public void DisableAppplicationTracker()
        {
            RUIResult stopSessionResult = _ruiSdk.StopSession(this.Username);
            WriteError(stopSessionResult);

            RUIResult result= _ruiSdk.StopSDK(10);
            WriteError(result);
            //  throw new NotImplementedException();
        }

        public void EnableAppplicationTracker(string productVersion, string username)
        {
            // throw new NotImplementedException();

            this.ProductVersion = productVersion;
            this.Username = username;

            RUIResult result = CreateRevulyticsConfig();

            WriteError(result);

            RUIResult versionResult = _ruiSdk.SetProductVersion(this.ProductVersion);
            WriteError(versionResult);

            RUIResult startSessionResult = _ruiSdk.StartSession(this.Username);
            WriteError(startSessionResult);
        }
        /// <summary>
        /// * Conditioning: All leading white space is removed.
        //* Conditioning: All trailing white space is removed.
        //* Conditioning: All internal white spaces other than space characters(' ') are removed.
        //* Conditioning: Trimmed to a maximum of 128 UTF8 characters.
        //* Validation: eventCategory can be empty; eventName cannot be empty.
        /// </summary>
        /// <param name="eventName"></param>
        public void TrackApplicationEvent(string eventName)
        {
            RUIResult result = _ruiSdk.TrackEventText(ApplicationTrackerConstants.TrackerEventGroup.MainMenuClicked, eventName, eventName, this.Username);

            WriteError(result);
        }

        public void TrackCustomEvent(string eventCategory, string eventName, string customValues)
        {
            RUIResult result = _ruiSdk.TrackEventText(eventCategory, eventName, customValues, this.Username);

           WriteError(result);
        }

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

        void WriteError(RUIResult result)
        {
            if (result != RUIResult.ok)
            {
                var errormMessage = string.Format("{0} :: Tracker Error -> {1}", DateTime.Now.ToString("g"), result);
                //  Trace.WriteLine(format);

                Dev2Logger.Error(errormMessage, "Revulytics sdk error");
            }
        }

    }
}
