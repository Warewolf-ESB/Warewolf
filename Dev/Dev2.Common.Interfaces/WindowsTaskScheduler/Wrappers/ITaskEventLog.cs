using System.Collections.Generic;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface ITaskEventLog : IEnumerable<ITaskEvent>
    {
        /// <summary>
        ///     Gets the total number of events for this task.
        /// </summary>
        long Count { get; }
    }
}