
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
