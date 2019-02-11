using RUISDK_5_3_1;

namespace Dev2.Instrumentation.Factory
{
   public static class ApplicationTrackerFactory
    {
        public static IApplicationTracker ApplicationTracker { get; private set; }
        
        public static IApplicationTracker  GetApplicationTrackerProvider()
        {
            ApplicationTracker = null;
#if !DEBUG
            ApplicationTracker = new DummyApplicationTracker();
#else
            ApplicationTracker = RevulyticsTracker.GetTrackerInstance();
#endif
            return ApplicationTracker;
        }

#if !DEBUG
	    class DummyApplicationTracker : IApplicationTracker
        {
            public RUIResult EnableApplicationResultStatus { get; set; }

            public void DisableApplicationTracker()
            {
            }

            public void EnableApplicationTracker(string productVersion, string informationalVersion, string username)
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
