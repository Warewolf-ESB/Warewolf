/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Trackerbird.Tracker;

// ReSharper disable RedundantUsingDirective

// ReSharper restore RedundantUsingDirective

#if ! DEBUG
using Dev2.Studio.Utils;
#endif

namespace Dev2.Instrumentation
{
    // see http://docs.trackerbird.com/NET/

    /// <summary>
    ///     Tracks feature and event usage.
    /// </summary>
    public static class Tracker
    {
        /// <summary>
        ///     This signals that Server has started.
        ///     This should be placed before calling any other <see cref="Tracker" /> method.
        /// </summary>
        public static void StartServer()
        {
#if ! DEBUG
    // RELEASE
            Start("2386158864", "http://40589.tbnet1.com");
            TBApp.StartAutoSync(true);
#endif
        }

        /// <summary>
        ///     This signals that Studio has started.
        ///     This should be placed before calling any other <see cref="Tracker" /> method.
        /// </summary>
        public static void StartStudio()
        {
#if ! DEBUG
    // RELEASE
            Start("2386158962", "http://94687.tbnet1.com");
#endif
        }

        // ReSharper disable UnusedMember.Local
        private static void Start(string productId, string callHomeUrl)
            // ReSharper restore UnusedMember.Local
        {
            Perform(() =>
            {
                string location = Assembly.GetExecutingAssembly().Location;
                string filePath = Path.GetDirectoryName(location);
#if ! DEBUG && ! TEST
                var fvi = VersionInfo.FetchVersionInfo();
                var productVersion = fvi;
#else
                // ReSharper disable ConvertToConstant.Local
                string productVersion = "0.0.9999.0";
                // ReSharper restore ConvertToConstant.Local
#endif
                TBConfig.SetFilePath(filePath);
                TBConfig.CreateConfig(callHomeUrl, productId, productVersion, productVersion, false);
                return TBApp.Start();
            });
        }

        /// <summary>
        ///     This method should be called when your application is exiting.
        ///     It will signal <see cref="Tracker" /> to log the event and to attempt to Sync with the Servers.
        ///     After calling this Method, <see cref="Tracker.Start" /> must be called again to start using <see cref="Tracker" />.
        /// </summary>
        public static void Stop()
        {
#if ! DEBUG
            WriteError(TBApp.Stop());
#endif
        }

        /// <summary>
        ///     Track events being used from within your application
        /// </summary>
        /// <param name="eventGroup">
        ///     The text by which to group your event. If the length of this string and the 'eventName'
        ///     parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used
        ///     inside this parameter.
        /// </param>
        /// <param name="eventName">
        ///     The text used to describe the feature. If the length of this string and the 'eventGroup'
        ///     parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used
        ///     inside this parameter.
        /// </param>
        /// <param name="eventValue">An optional value which is related to your event and you would like to store.</param>
        public static void TrackEvent(TrackerEventGroup eventGroup, TrackerEventName eventName, string eventValue = null)
        {
#if ! DEBUG
            TrackEvent(eventGroup, eventName.ToString(), eventValue);
#endif
        }

        /// <summary>
        ///     Track events being used from within your application
        /// </summary>
        /// <param name="eventGroup">
        ///     The text by which to group your event. If the length of this string and the 'eventName'
        ///     parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used
        ///     inside this parameter.
        /// </param>
        /// <param name="customText">
        ///     The text used to describe the feature. If the length of this string and the 'eventGroup'
        ///     parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used
        ///     inside this parameter.
        /// </param>
        /// <param name="eventValue">An optional value which is related to your event and you would like to store.</param>
        public static void TrackEvent(TrackerEventGroup eventGroup, string customText, string eventValue = "")
        {
#if ! DEBUG
            Perform(() => TBApp.EventTrackTxt(eventGroup.ToString(), customText, eventValue, null));
#endif
        }

        /// <summary>
        ///     Tracks and logs exceptions from within your code.
        /// </summary>
        /// <param name="className">
        ///     The class name from which the error originated. If the length of the string is greater than 50
        ///     it will be truncated.
        /// </param>
        /// <param name="methodName">
        ///     The method name from which the error originated. If the length of the string is greater than
        ///     50 it will be truncated.
        /// </param>
        /// <param name="ex">The handled exception.</param>
        public static void TrackException(string className, string methodName, Exception ex)
        {
            int idx = className.LastIndexOf('.');
            string newClassName = className.Substring(idx + 1);
            newClassName = newClassName.Replace("`", "").Replace("1", "");
            TrackEvent(TrackerEventGroup.Exception, string.Format("{0}.{1}", newClassName, methodName));
        }

        private static void Perform(Func<GenericReturn> action, bool async = false)
        {
            try
            {
                if (async)
                {
                    Task.Run(action).ContinueWith(t => WriteError(t.Result));
                }
                else
                {
                    GenericReturn result = action();
                    WriteError(result);
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
                // this is a tracker issue ;(
            }
        }

        private static void WriteError(GenericReturn result)
        {
            if (result != GenericReturn.OK)
            {
                string format = string.Format("{0} :: Tracker Error -> {1}", DateTime.Now.ToString("g"), result);
                Trace.WriteLine(format);
            }
        }
    }
}