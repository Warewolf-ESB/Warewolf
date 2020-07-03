#pragma warning disable
 /*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Dev2.Common;
using Dev2.Util;
using RUISDK_5_3_1;
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

        public string InformationalVersion { get; set; }

        public RUIResult EnableApplicationResultStatus { get; set; }

        public RevulyticsTracker()
        {
            SdkFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RUISDK)).Location);
            ConfigFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ProductId = AppUsageStats.ProductID;
            AppName = AppUsageStats.AppName;
            ProductUrl = AppUsageStats.ProductUrl;
            AesHexKey = AppUsageStats.AesHexKey;
        }

        public void EnableApplicationTracker(string productVersion, string informationalVersion, string username)
        {
            ProductVersion = productVersion;
            InformationalVersion = informationalVersion;
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
                        LogErrorResult("EnableApplicationTracker",SetProductVersion());
                        LogErrorResult("EnableApplicationTracker",SetInformationalVersion());
                        LogErrorResult("EnableApplicationTracker",StartSession());
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

        public void DisableApplicationTracker()
        {
            try
            {
                var result = StopSession();
                LogErrorResult("DisableApplicationTracker", result);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in DisableApplicationTracker method", e, Core.RevulyticsSdkError);
            }
        }

        public void TrackEvent(string category, string eventName)
        {
            if (AppUsageStats.CollectUsageStats && RuiSdk != null)
            {
                var result = RuiSdk.TrackEvent(category, eventName, Username);
                LogErrorResult("TrackEvent",result);
            }
        }

        public void TrackCustomEvent(string category, string eventName, string customValues)
        {
            if (AppUsageStats.CollectUsageStats && RuiSdk != null)
            {
                var result = RuiSdk.TrackEventText(category, eventName, customValues, Username);
                LogErrorResult("TrackCustomEvent",result);
            }
        }

        public static RevulyticsTracker GetTrackerInstance()
        {
            if (_revulyticsTrackerInstance == null)
            {
                lock (SyncLock)
                {
                    if (_revulyticsTrackerInstance == null)
                    {
                        _revulyticsTrackerInstance = new RevulyticsTracker();
                    }
                }

                return _revulyticsTrackerInstance;
            }
            return _revulyticsTrackerInstance;
        }

        public RUIResult CreateRevulyticsConfig()
        {
            var result = RUIResult.configNotCreated;
            try
            {
                if (string.IsNullOrEmpty(SdkFilePath))
                {
                    result = RUIResult.invalidConfigPath;
                    LogErrorResult("Error in CreateRevulyticsConfig: Sdk File Path is missing", result);
                    return result;
                }
                RuiSdk = new RUISDK(true, SdkFilePath,"RUISDK_5_3_1");
                if (RuiSdk != null)
                {
                    if (!Directory.Exists(ConfigFilePath) && !File.Exists(ConfigFilePath))
                    {
                        Directory.CreateDirectory(ConfigFilePath);
                    }
                    result = RuiSdk.CreateConfig(ConfigFilePath, ProductId, AppName, ProductUrl, Protocol, AesHexKey, MultiSessionEnabled, ReachOutOnAutoSync);
                    int timeout = 10;
                    while (RuiSdk.GetState() == RUIState.startedNewRegRunning && timeout > 0)
                    {
                        Thread.Sleep(100);
                    }
                }
                return result;
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in CreateRevulyticsConfig method initializing rui sdk", ex, Core.RevulyticsSdkError);
                return result;
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in CreateRevulyticsConfig Method", e, Core.RevulyticsSdkError);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in CreateRevulyticsConfig method", e, Core.RevulyticsSdkError);
                return result;
            }
        }

        public RUIResult StartSdk()
        {
            var result = RUIResult.sdkNotStarted;
            try
            {
                result = RuiSdk.StartSDK();
                return result;
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in StartSdk method (RUISDKCreationException)", ex, Core.RevulyticsSdkError);
                return result;
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in StartSdk method (ArgumentNullException)", e, Core.RevulyticsSdkError);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in StartSdk method", e, Core.RevulyticsSdkError);
                return result;
            }
        }
        public RUIResult StopSdk()
        {
            var result = RUIResult.sdkAlreadyStopped;
            try
            {
                result = RuiSdk.StopSDK(10);
                return result;
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in StartSdk method (RUISDKCreationException)", ex, Core.RevulyticsSdkError);
                return result;
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in StartSdk method (ArgumentNullException)", e, Core.RevulyticsSdkError);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in StartSdk method", e, Core.RevulyticsSdkError);
                return result;
            }
        }

        public RUIResult SetProductVersion()
        {
            var result = RUIResult.sdkSuspended; ///The RUI Server has instructed a temporary back-off.
            try
            {
                Dev2Logger.Warn($"Revulytics.SetProductVersion: {ProductVersion}", "");
                result = RuiSdk.SetProductVersion(ProductVersion);
                return result;
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in SetProductVersion method (RUISDKCreationException)", ex, Core.RevulyticsSdkError);
                return result;
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in SetProductVersion method (ArgumentNullException)", e, Core.RevulyticsSdkError);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in SetProductVersion method", e, Core.RevulyticsSdkError);
                return result;
            }
        }

        public RUIResult SetInformationalVersion()
        {
            var result = RUIResult.sdkSuspended;
            try
            {
                Dev2Logger.Warn($"Revulytics.SetInformationalVersion: {InformationalVersion}", "");
                result = RuiSdk.SetProductBuildNumber(InformationalVersion);
                return result;
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in SetInformationalVersion method (RUISDKCreationException)", ex, Core.RevulyticsSdkError);
                return result;
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in SetInformationalVersion method (ArgumentNullException)", e, Core.RevulyticsSdkError);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in SetInformationalVersion method", e, Core.RevulyticsSdkError);
                return result;
            }
        }

        public RUIResult StartSession()
        {
            var result = RUIResult.sdkSuspended;
            try
            {
                result = RuiSdk.StartSession(Username);
                return result;
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in StartSession method (RUISDKCreationException)", ex, Core.RevulyticsSdkError);
                return result;
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in StartSession method (ArgumentNullException)", e, Core.RevulyticsSdkError);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in StartSession method", e, Core.RevulyticsSdkError);
                return result;
            }
        }

        public RUIResult StopSession()
        {
            var result = RUIResult.sdkAlreadyStopped;
            try
            {
                result = RuiSdk.StopSession(Username);
                RuiSdk.StopSDK(0);
                return result;
            }
            catch (RUISDKCreationException ex)
            {
                Dev2Logger.Error("Error in StopSession method (RUISDKCreationException)", ex, Core.RevulyticsSdkError);
                return result;
            }
            catch (ArgumentNullException e)
            {
                Dev2Logger.Error("Error in StopSession method (ArgumentNullException)", e, Core.RevulyticsSdkError);
                return result;
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error in StopSession method", e, Core.RevulyticsSdkError);
                return result;
            }
        }

        static void LogResult(RUIResult result)
        {
            var errormMessage = $"{DateTime.Now:g} :: Tracker Error -> {result}";

            Dev2Logger.Error(errormMessage, Core.RevulyticsSdkError);
        }

        static void LogErrorResult(string method,RUIResult result)
        {
            if (result != RUIResult.ok)
            {
                var errormMessage = $"{DateTime.Now:g} :{method}: Tracker Error -> {result}";
                Dev2Logger.Error(errormMessage, Core.RevulyticsSdkError);
            }
        }
    }
}
