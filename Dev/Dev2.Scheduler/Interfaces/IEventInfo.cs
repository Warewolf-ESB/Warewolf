using System;

namespace Dev2.Scheduler.Interfaces
{
    public interface IEventInfo
    {
        /// <summary>
        /// Run date as per windows
        /// </summary>
        DateTime StartDate { get; }
        /// <summary>
        /// Total Duration
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// Completion date
        /// </summary>
        DateTime EndDate { get; }
        /// <summary>
        /// State
        /// </summary>
        bool Success { get; }
        /// <summary>
        /// Windows event id
        /// </summary>
        string EventId { get; }


        string FailureReason { get; }


    }
}