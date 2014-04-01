using System.Collections.Generic;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public interface ITaskEventLog : IEnumerable<ITaskEvent>
    {
        /// <summary>
        ///     Gets the total number of events for this task.
        /// </summary>
        long Count { get; }
    }
}