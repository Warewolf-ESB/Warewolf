using System;
using System.IO;
using System.Reflection;
using Dev2.Common;
using Dev2.Util;
using RUISDK_5_1_0;

namespace Dev2.Instrumentation
{
    /// <summary>
    ///     Revulytics -User action tracking
    /// </summary>
    public class RevulyticsTracker : IApplicationTracker
    {
        private const int Protocol = 1;
        private const bool MultiSessionEnabled = true;

        private const bool ReachOutOnAutoSync = true;

        // current instance of the Revulytics
        private static RevulyticsTracker _revulyticsTrackerInstance;


        //to check if object is not in use
        private static readonly object SyncLock = new object();

        public string AesHexKey { get; set; }

        public string AppName { get; set; }

        public string ConfigFilePath { get; set; }

        public string ProductId { get; set; }

        public string ProductUrl { get; set; }
        public string SdkFilePath { get; set; }

        public RUISDK RuiSdk { get; set; }

        public string Username { get; set; }

        public string ProductVersion { get; set; }
        public RUIResult EnableApplicationResult { get; set; }

        /// <summary>
        ///     set the variables by reading config file
        /// </summary>
        public RevulyticsTracker()
        {
            SdkFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RUISDK)).Location);
            ConfigFilePath = AppSettings.ConfigFilePath;
            ProductId = AppSettings.ProductID;
            AppName = AppSettings.AppName;
            ProductUrl = AppSettings.ProductUrl;
            AesHexKey = AppSettings.AesHexKey;
        }

      

        /// <summary>
        ///     This function create config file for revulytics tracking.
        ///     And call the SetProductVersion and StartSession methods
        /// </summary>
        /// <param name="productVersion">current product version of the application</param>
        /// <param name="username">login user</param>
        public void EnableAppplicationTracker(string productVersion, string username)
        {
            ProductVersion = productVersion;
            Username = username;
            try
            {
                EnableApplicationResult = CreateRevulyticsConfig();
                if (EnableApplicationResult != RUIResult.ok)
                {
                    LogResult(EnableApplicationResult);
                }
                else
                {
                    var sdkResult = StartSdk();
                    if (sdkResult == RUIResult.ok)
                    {
                        LogErrorResult(SetProductVersion());
                        LogErrorResult(StartSession());
                    }
                    else
                    {
                        LogResult(sdkResult);
                    }
                }
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in initializing rui sdk", ex, "Revulytics sdk error");
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in config settings", e, "Revulytics sdk error");
            }

            catch (Exception e)
            {
                Dev2Logger.Error("Error in EnableAppplicationTracker method", e, "Revulytics sdk error");
            }
        }

        /// <summary>
        ///     This Function will stop the current session and action tracking.
        /// </summary>
        public void DisableAppplicationTracker()
        {
            LogErrorResult(StopSession());
            LogErrorResult(StopSdk());
        }

        /// <summary>
        ///     This function will log the event with event category in the revulytics log.
        ///     If CollectUsageStats set to true then only it will log to revulytics
        ///     Below is standard comment for TrackEventText method provided by revulytics.
        ///     * Conditioning: All leading white space is removed.
        ///     * Conditioning: All trailing white space is removed.
        ///     * Conditioning: All internal white spaces other than space characters(' ') are removed.
        ///     * Conditioning: Trimmed to a maximum of 128 UTF8 characters.
        ///     * Validation: eventCategory can be empty; eventName cannot be empty.
        /// </summary>
        /// <param name="category">Event Category</param>
        /// <param name="eventName">Action name</param>
        public void TrackEvent(string category, string eventName)
        {
            if (AppSettings.CollectUsageStats && RuiSdk != null)
            {
                var result = RuiSdk.TrackEvent(category, eventName, Username);
                LogErrorResult(result);
            }
        }


        /// <summary>
        ///     This function will log the event based on event category,event name in the revulytics log.
        ///     If CollectUsageStats set to true then only it will log to revulytics
        ///     Below is standard comment for TrackEventText method provided by revulytics.
        ///     * Conditioning: All leading white space is removed.
        ///     * Conditioning: All trailing white space is removed.
        ///     * Conditioning: All internal white spaces other than space characters(' ') are removed.
        ///     * Conditioning: Trimmed to a maximum of 128 UTF8 characters.
        ///     * Validation: eventCategory can be empty; eventName cannot be empty.
        /// </summary>
        /// <param name="eventCategory">Category of event like,item dragged, tab opened</param>
        /// <param name="eventName">name of the object/action</param>
        /// <param name="customValues">values of object ot variable</param>
        public void TrackCustomEvent(string eventCategory, string eventName, string customValues)
        {
            if (AppSettings.CollectUsageStats && RuiSdk != null)
            {
                var result = RuiSdk.TrackEventText(eventCategory, eventName, customValues, Username);
                LogErrorResult(result);
            }

        }

        /// <summary>
        ///     This function will return single instance of action tracking object
        ///     through out the application life cycle.
        /// </summary>
        /// <returns>RevulyticsTracker object</returns>
        public static RevulyticsTracker GetTrackerInstance()
        {
            if (_revulyticsTrackerInstance == null)
            {
                lock (SyncLock)
                {
                    if (_revulyticsTrackerInstance == null)
                        _revulyticsTrackerInstance = new RevulyticsTracker();
                }

                return _revulyticsTrackerInstance;
            }
            return _revulyticsTrackerInstance;
        }

   


        /// <summary>
        ///     This Function will create the config for revulytics action tracking
        /// </summary>
        /// <returns>RUIResult object</returns>
        public RUIResult CreateRevulyticsConfig()
        {
            var result = RUIResult.configNotCreated;
            if (string.IsNullOrEmpty(SdkFilePath))
            {
                throw new ArgumentNullException("SdkFilePath");
            }
            RuiSdk = new RUISDK(true, SdkFilePath);
            if (RuiSdk != null)
            {
                result = RuiSdk.CreateConfig(ConfigFilePath, ProductId, AppName, ProductUrl, Protocol, AesHexKey,
                    MultiSessionEnabled, ReachOutOnAutoSync);
               
            }
            return result;
        }

        public RUIResult StartSdk()
        {
            return RuiSdk.StartSDK();
        }
        public RUIResult StopSdk()
        {
            return RuiSdk.StopSDK(10);

        }

        /// <summary>
        ///     This function set the product version in revulytics.
        /// </summary>
        public RUIResult SetProductVersion()
        {
            return RuiSdk.SetProductVersion(ProductVersion);
        }

        /// <summary>
        ///     This function will start the revultics session for login user.
        /// </summary>
        public RUIResult StartSession()
        {
            return RuiSdk.StartSession(Username);
        }

        /// <summary>
        ///     This function will stop the revultics session for login user.
        /// </summary>
        public RUIResult StopSession()
        {
            return RuiSdk.StopSession(Username);
        }


        /// <summary>
        ///     This function will write into log if result is other than ok
        ///     while calling the revulytics method
        /// </summary>
        /// <param name="result">RUIResult object</param>
        private static void LogResult(RUIResult result)
        {
            var errormMessage = $"{DateTime.Now:g} :: Tracker Error -> {result}";

            Dev2Logger.Error(errormMessage, "Revulytics sdk error");
        }

        /// <summary>
        ///     This function will write into log if result is other than ok
        ///     while calling the revulytics method
        /// </summary>
        /// <param name="result">RUIResult object</param>
        private static void LogErrorResult(RUIResult result)
        {
            if (result!= RUIResult.ok)
            {
                var errormMessage = $"{DateTime.Now:g} :: Tracker Error -> {result}";
                Dev2Logger.Error(errormMessage, "Revulytics sdk error");
            }
        }
    }
}