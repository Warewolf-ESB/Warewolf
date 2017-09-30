using Dev2.Instrumentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dev2.Instrumentation.Factory
{
   public static class ApplicationTrackerFactory
    {       

        public static IApplicationTracker  GetApplicationTrackerProvider()
        {
          // IApplicationAnalytics = null; 

            if (ApplicationTrackerConstants.ProviderName== "revulytics")
            {
                return RevulyticsTracker.GetTrackerInstance();
                
            }

            //return null;
        }


    }
}
