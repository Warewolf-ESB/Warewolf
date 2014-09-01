using System;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface ITaskEvent
    {
        /// <summary>
        ///     Gets the activity id.
        /// </summary>
        Guid? ActivityId { get; }

        /// <summary>
        ///     Gets the event id.
        /// </summary>
        int EventId { get; }

        /// <summary>
        ///     Gets the task category.
        /// </summary>
        string TaskCategory { get; }

        /// <summary>
        ///     Gets the time created.
        /// </summary>
        DateTime? TimeCreated { get; }

        /// <summary>
        ///     correlation id
        /// </summary>
        string Correlation { get; }

        /// <summary>
        ///     the user that the task ran as
        /// </summary>
        string UserId { get; }
    }
}