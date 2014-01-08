using System;
using System.Activities;
using System.Activities.Tracking;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Workflow
{
    public class CustomTrackingEventArgs : System.EventArgs
    {
        #region Ctor
        public CustomTrackingEventArgs(TrackingRecord record, TimeSpan timeout, Activity trackingActivity)
        {
            Record = record;
            Timeout = timeout;
            TrackingActivity = trackingActivity;
        }
        #endregion

        #region Properties
        public TrackingRecord Record { get; set; }
        public TimeSpan Timeout { get; set; }
        public Activity TrackingActivity { get; set; }
        #endregion
    }
}
