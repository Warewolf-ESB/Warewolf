using System;
using System.IO;
using System.Reflection;
using Dev2.Common;
using Dev2.Util;
using RUISDK_5_1_0;
using Warewolf.Studio.Resources.Languages;
namespace Dev2.Instrumentation
{
    public class RevulyticsTracker : IApplicationTracker
    {
        const int Protocol = 1;
        const bool MultiSessionEnabled = true;

        const bool ReachOutOnAutoSync = true;

        static RevulyticsTracker _revulyticsTrackerInstance;

        static readonly object SyncLock = new object();

        public string AesHexKey { get; set; }

        public string AppName { get; set; }

        public string ConfigFilePath { get; set; }

        public string ProductId { get; set; }

        public string ProductUrl { get; set; }

        public string SdkFilePath { get; set; }

        public RUISDK RuiSdk { get; set; }

        public string Username { get; set; }

        public string ProductVersion { get; set; }

        public RUIResult EnableApplicationResultStatus { get; set; }
        
        public RevulyticsTracker()
        {
            SdkFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RUISDK)).Location);
            ConfigFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ProductId = AppSettings.ProductID;
            AppName = AppSettings.AppName;
            ProductUrl = AppSettings.ProductUrl;
            AesHexKey = AppSettings.AesHexKey;
        }

        public void EnableAppplicationTracker(string productVersion, string username)
        {
            ProductVersion = productVersion;
            Username = username;
            try
            {
                EnableApplicationResultStatus = CreateRevulyticsConfig();
                if (EnableApplicationResultStatus != RUIResult.ok)
                {
                    LogResult(EnableApplicationResultStatus);
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
                Dev2Logger.Error("Error in initializing rui sdk", ex, Core.RevulyticsSdkError);
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in config settings", e, Core.RevulyticsSdkError);
            }

            catch (Exception e)
            {
                Dev2Logger.Error("Error in EnableAppplicationTracker method", e, Core.RevulyticsSdkError);
            }
        }

        public void DisableAppplicationTracker()
        {
            try
            {
                LogErrorResult(StopSession());
                LogErrorResult(StopSdk());
            }

            catch (Exception e)
            {
                Dev2Logger.Error("Error in DisableAppplicationTracker method", e, Core.RevulyticsSdkError);
            }
        }

        public void TrackEvent(string category, string eventName)
        {
            if (AppSettings.CollectUsageStats && RuiSdk != null)
            {
                var result = RuiSdk.TrackEvent(category, eventName, Username);
                LogErrorResult(result);
            }
        }

        public void TrackCustomEvent(string eventCategory, string eventName, string customValues)
        {
            if (AppSettings.CollectUsageStats && RuiSdk != null)
            {
                var result = RuiSdk.TrackEventText(eventCategory, eventName, customValues, Username);
                LogErrorResult(result);
            }

        }

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

        public RUIResult CreateRevulyticsConfig()
        {
            var result = RUIResult.configNotCreated;
            if (string.IsNullOrEmpty(SdkFilePath))
            {
                throw new ArgumentNullException($"Sdk File Path is missing");
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

        public RUIResult SetProductVersion()
        {
            return RuiSdk.SetProductVersion(ProductVersion);
        }

        public RUIResult StartSession()
        {
            return RuiSdk.StartSession(Username);
        }

        public RUIResult StopSession()
        {
            return RuiSdk.StopSession(Username);
        }

        static void LogResult(RUIResult result)
        {
            var errormMessage = $"{DateTime.Now:g} :: Tracker Error -> {result}";

            Dev2Logger.Error(errormMessage, Core.RevulyticsSdkError);
        }

        static void LogErrorResult(RUIResult result)
        {
            if (result != RUIResult.ok)
            {
                var errormMessage = $"{DateTime.Now:g} :: Tracker Error -> {result}";
                Dev2Logger.Error(errormMessage, Core.RevulyticsSdkError);
            }
        }
    }
}