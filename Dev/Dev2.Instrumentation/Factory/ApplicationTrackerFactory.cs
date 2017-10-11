using Dev2.Instrumentation;
using Dev2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dev2.Instrumentation.Factory
{
    /// <summary>
    /// Factory class to create instance of action tracking provider
    /// </summary>
   public static class ApplicationTrackerFactory
    {
        public static IApplicationTracker _applicationTracker { get; private set; }

        /// <summary>
        /// This function will create instance of Application tracker object 
        /// based on the TrackerProvider value set in the userStudioSettings.config file.
        /// </summary>
        /// <returns> IApplicationTracker object</returns>
        public static IApplicationTracker  GetApplicationTrackerProvider()
        {
            _applicationTracker = null;

            switch (AppSettings.TrackerProvider.ToLower())
            {
              case  "revulytics":
                    _applicationTracker = RevulyticsTracker.GetTrackerInstance();
                    break; 
            default:
                    _applicationTracker = RevulyticsTracker.GetTrackerInstance();
                    break;

            }

            return _applicationTracker;
        }


    }
}
