using System;
using System.Activities.Tracking;
using System.Activities;

namespace Dev2.Studio.Core.AppResources.Workflow {
    public class CustomTrackingEventArgs : System.EventArgs {

        #region Ctor
        public CustomTrackingEventArgs(TrackingRecord record, TimeSpan timeout, Activity trackingActivity) {
            this.Record = record;
            this.Timeout = timeout;
            this.TrackingActivity = trackingActivity;
        }
        #endregion


        #region Properties
        public TrackingRecord Record { get; set; }
        public TimeSpan Timeout { get; set; }
        public Activity TrackingActivity { get; set; }
        #endregion


    }
}
