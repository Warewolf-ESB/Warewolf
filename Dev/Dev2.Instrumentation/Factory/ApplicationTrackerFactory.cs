/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using RUISDK_5_3_1;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Instrumentation.Factory
{
    /// <summary>
    /// Factory class to create instance of action tracking provider
    /// </summary>
   public static class ApplicationTrackerFactory
    {
        public static IApplicationTracker ApplicationTracker { get; private set; }

        /// <summary>
        /// This function will create instance of Application tracker object 
        /// based on the TrackerProvider value set in the userStudioSettings.config file.
        /// </summary>
        /// <returns> IApplicationTracker object</returns>
        public static IApplicationTracker  GetApplicationTrackerProvider()
        {
            // TODO: this should return a fake during debug and testing

            ApplicationTracker = null;
#if DEBUG
            ApplicationTracker = new DummyApplicationTracker();
#else
            ApplicationTracker = RevulyticsTracker.GetTrackerInstance();
#endif
            return ApplicationTracker;
        }

#if DEBUG
        [ExcludeFromCodeCoverage]
	    class DummyApplicationTracker : IApplicationTracker
        {
            public RUIResult EnableApplicationResultStatus { get; set; }

            public void DisableApplicationTracker()
            {
            }

            public void EnableApplicationTracker(string productVersion, string username)
            {
            }

            public void TrackCustomEvent(string category, string eventName, string customValues)
            {
            }

            public void TrackEvent(string category, string eventName)
            {
            }
        }
#endif
    }
}
