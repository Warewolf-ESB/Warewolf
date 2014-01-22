using System;
using System.Globalization;
using System.Reflection;
using Trackerbird.Tracker;

namespace Dev2.Instrumentation
{
    public static class Tracker
    {
        /// <summary>
        /// This signals that Server has started. 
        /// This should be placed before calling any other <see cref="Tracker"/> method.
        /// </summary>
        public static void StartServer()
        {
            Start("2385158467", "http://27504.tbnet1.com");
        }

        /// <summary>
        /// This signals that Studio has started. 
        /// This should be placed before calling any other <see cref="Tracker"/> method.
        /// </summary>
        public static void StartStudio()
        {
            Start("2385158467", "http://27504.tbnet1.com");
        }

        static void Start(string productID, string callHomeUrl)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var productVersion = version.ToString();
            var productBuildNumber = version.Build.ToString(CultureInfo.InvariantCulture);

            var config = new TBConfig(callHomeUrl, productID, productVersion, productBuildNumber, false);
            App.Start(config);
        }

        /// <summary>
        /// This method should be called when your application is exiting. 
        /// It will signal <see cref="Tracker"/> to log the event and to attempt to Sync with the Servers.  
        /// After calling this Method, <see cref="Tracker.Start"/> must be called again to start using <see cref="Tracker"/>.
        /// </summary>
        public static void Stop()
        {
            App.Stop();
        }

        /// <summary>
        /// Track events being used from within your application
        /// </summary>
        /// <param name="eventGroup">The text by which to group your event. If the length of this string and the 'eventName' parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used inside this parameter.</param>
        /// <param name="eventName">The text used to describe the feature. If the length of this string and the 'eventGroup' parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used inside this parameter.</param>
        /// <param name="eventValue">An optional value which is related to your event and you would like to store.</param>
        public static void TrackEvent(TrackerEventGroup eventGroup, TrackerEventName eventName, double? eventValue = null)
        {
            TrackEvent(eventGroup, eventName.ToString(), eventValue);
        }

        /// <summary>
        /// Track events being used from within your application
        /// </summary>
        /// <param name="eventGroup">The text by which to group your event. If the length of this string and the 'eventName' parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used inside this parameter.</param>
        /// <param name="customText">The text used to describe the feature. If the length of this string and the 'eventGroup' parameter is greater than 40 it will be truncated. Also ';' (semicolons) and '|' (pipeline) are not to be used inside this parameter.</param>
        /// <param name="eventValue">An optional value which is related to your event and you would like to store.</param>
        public static void TrackEvent(TrackerEventGroup eventGroup, string customText, double? eventValue = null)
        {
            App.EventTrack(eventGroup.ToString(), customText, eventValue);
        }

        /// <summary>
        /// Tracks and logs exceptions from within your code.
        /// </summary>
        /// <param name="className">The class name from which the error originated. If the length of the string is greater than 50 it will be truncated.</param>
        /// <param name="methodName">The method name from which the error originated. If the length of the string is greater than 50 it will be truncated.</param>
        /// <param name="ex">The handled exception.</param>
        public static void TrackException(string className, string methodName, Exception ex)
        {
            App.ExceptionTrack(className, methodName, ex);
        }
    }
}
