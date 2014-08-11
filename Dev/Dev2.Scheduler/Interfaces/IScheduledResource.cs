using System;
using Dev2.DataList.Contract;

namespace Dev2.Scheduler.Interfaces
{
    public interface IScheduledResource
    {

  
        /// <summary>
        /// Property to check if the scheduled resouce is saved
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        ///     Schedule Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Represents the old name of the task
        /// </summary>
        string OldName { get; set; }

        /// <summary>
        ///     Schedule Status
        /// </summary>
        SchedulerStatus Status { get; set; }

        /// <summary>
        ///     The next time that this schedule will run
        /// </summary>
        DateTime NextRunDate { get; set; }

        /// <summary>
        ///     Trigger
        /// </summary>
        IScheduleTrigger Trigger { get; set; }

        /// <summary>
        /// NumberOfHistoryToKeep
        /// </summary>
        int NumberOfHistoryToKeep { get; set; }

        /// <summary>
        /// The workflow that we will run
        /// </summary>
        string WorkflowName { get; set; }


        /// <summary>
        /// The workflow that we will run
        /// </summary>
        Guid ResourceId { get; set; }

        /// <summary>
        /// If a schedule is missed execute as soon as possible
        /// </summary>
        bool RunAsapIfScheduleMissed { get; set; }

        // Allow more than once Instance of this task to be run at te same time
        bool AllowMultipleIstances { get; set; }

        /// <summary>
        /// The task UserName
        /// </summary>
        string UserName { get; set; }

        //The task password
        string Password{ get; set; }

        /// <summary>
        /// validation errors
        /// </summary>

        ErrorResultTO Errors { get; set; }
        bool IsNew { get; set; }
    }
}