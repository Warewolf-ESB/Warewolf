
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
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Workflow
{
    public class CustomTrackingParticipant : TrackingParticipant
    {
        #region Properties
        public Dictionary<string, Activity> ActivityIdToWorkflowElementMap { get; set; }
        #endregion

        #region Events
        public event EventHandler<CustomTrackingEventArgs> TrackingRecordReceived;
        #endregion

        #region Protected Methods
        protected void OnTrackingRecordReceived(TrackingRecord record, TimeSpan timeout)
        {
            ActivityStateRecord activityStateRecord = record as ActivityStateRecord;
            if(TrackingRecordReceived != null)
            {
                if(activityStateRecord != null && !activityStateRecord.Activity.TypeName.Contains("System.Activities.Expressions"))
                {
                    if(ActivityIdToWorkflowElementMap.ContainsKey(activityStateRecord.Activity.Id))
                    {
                        TrackingRecordReceived(this, new CustomTrackingEventArgs(record, timeout, ActivityIdToWorkflowElementMap[activityStateRecord.Activity.Id]));
                    }
                }
            }
            else
            {
                TrackingRecordReceived(this, new CustomTrackingEventArgs(record, timeout, null));
            }
        }

        #region TrackingParticipant Member
        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            OnTrackingRecordReceived(record, timeout);
        }
        #endregion

        #endregion



    }
}
