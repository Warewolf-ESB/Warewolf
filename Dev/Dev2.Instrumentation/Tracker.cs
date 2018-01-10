/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Util;

#if ! DEBUG
using Dev2.Studio.Utils;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Trackerbird.Tracker;
#endif

namespace Dev2.Instrumentation
{
    // see http://docs.trackerbird.com/NET/
    public static class Tracker
    {
        public static void StartServer()
        {
#if !DEBUG
            Start("2386158864", "http://40589.tbnet1.com");
            TBApp.StartAutoSync(true);
#endif
        }

        public static void StartStudio()
        {
#if !DEBUG
            Start("2386158962", "http://94687.tbnet1.com");
#endif
        }

#if !DEBUG
        static void Start(string productId, string callHomeUrl)
        {
            Perform(() =>
            {
                var location = Assembly.GetExecutingAssembly().Location;
                var filePath = Path.GetDirectoryName(location);
                var fvi = VersionInfo.FetchVersionInfo();
                var productVersion = fvi;
                TBConfig.SetFilePath(filePath);
                TBConfig.CreateConfig(callHomeUrl, productId, productVersion, productVersion, false);
                return TBApp.Start();
            });
        }
#endif

        public static void Stop()
        {
#if ! DEBUG
            WriteError(TBApp.Stop());
#endif
        }

        public static void TrackEvent(TrackerEventGroup eventGroup, TrackerEventName eventName) => TrackEvent(eventGroup, eventName, null);
        public static void TrackEvent(TrackerEventGroup eventGroup, TrackerEventName eventName, string eventValue)
        {
#if ! DEBUG
            TrackEvent(eventGroup, eventName.ToString(), eventValue);
#endif
        }

        public static void TrackEvent(TrackerEventGroup eventGroup, string customText) => TrackEvent(eventGroup, customText, "");
        public static void TrackEvent(TrackerEventGroup eventGroup, string customText, string eventValue)
        {
#if ! DEBUG
            if (AppUsageStats.CollectUsageStats)
            {
                Perform(() => TBApp.EventTrackTxt(eventGroup.ToString(), customText, eventValue, null));
            }
#endif
        }

        public static void TrackException(string className, string methodName, Exception ex)
        {
#if !DEBUG
            if (AppUsageStats.CollectUsageStats)
            {
                var idx = className.LastIndexOf('.');
                var newClassName = className.Substring(idx + 1);
                newClassName = newClassName.Replace("`", "").Replace("1", "");
                TrackEvent(TrackerEventGroup.Exception, $"{newClassName}.{methodName}");
            }
#endif
        }

#if !DEBUG
        static void Perform(Func<GenericReturn> action, bool performAsync = false)
        {
            try
            {
                if (performAsync)
                {
                    Task.Run(action).ContinueWith(t => WriteError(t.Result));
                }
                else
                {
                    var result = action();
                    WriteError(result);
                }
            }            
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }
#endif

#if !DEBUG
        static void WriteError(GenericReturn result)
        {
            if(result != GenericReturn.OK)
            {
                var format = string.Format("{0} :: Tracker Error -> {1}", DateTime.Now.ToString("g"), result);
                Trace.WriteLine(format);
            }
        }
#endif

        public static void OverriddenTrackEvent(TrackerEventGroup eventGroup, TrackerEventName executed, string eventValue)
        {
#if !DEBUG
            Perform(() => TBApp.EventTrackTxt(eventGroup.ToString(), executed.ToString(), eventValue, null));
#endif
        }
    }
}
