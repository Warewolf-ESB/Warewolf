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
            ApplicationTracker = null;
            ApplicationTracker = RevulyticsTracker.GetTrackerInstance();
            return ApplicationTracker;
        }


    }
}
