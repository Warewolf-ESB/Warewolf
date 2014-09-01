using System;
using System.ComponentModel;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IRepetitionPattern : IDisposable, IWrappedObject<RepetitionPattern>
    {
        /// <summary>
        ///     Gets or sets how long the pattern is repeated.
        /// </summary>
        /// <value>
        ///     The duration that the pattern is repeated. The minimum time allowed is one minute. If <c>TimeSpan.Zero</c> is specified, the pattern is repeated indefinitely.
        /// </value>
        /// <remarks>If you specify a repetition duration for a task, you must also specify the repetition interval.</remarks>
        [DefaultValue(typeof (TimeSpan), "00:00:00")]
        TimeSpan Duration { get; set; }

        /// <summary>
        ///     Gets or sets the amount of time between each restart of the task.
        /// </summary>
        /// <value>
        ///     The amount of time between each restart of the task. The maximum time allowed is 31 days, and the minimum time allowed is 1 minute.
        /// </value>
        /// <remarks>If you specify a repetition duration for a task, you must also specify the repetition interval.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">The maximum time allowed is 31 days, and the minimum time allowed is 1 minute.</exception>
        [DefaultValue(typeof (TimeSpan), "00:00:00")]
        TimeSpan Interval { get; set; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates if a running instance of the task is stopped at the end of repetition pattern duration.
        /// </summary>
        bool StopAtDurationEnd { get; set; }

        /// <summary>
        ///     Determines whether any properties for this <see cref="RepetitionPattern" /> have been set.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if properties have been set; otherwise, <c>false</c>.
        /// </returns>
        bool IsSet();
    }
}