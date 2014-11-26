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
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface ITaskSettings : IDisposable, IWrappedObject<TaskSettings>
    {
        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the task can be started by using either the Run command or the
        ///     Context menu.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        [DefaultValue(true)]
        [XmlElement("AllowStartOnDemand")]
        [XmlIgnore]
        bool AllowDemandStart { get; set; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the task may be terminated by using TerminateProcess.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        [DefaultValue(true)]
        [XmlIgnore]
        bool AllowHardTerminate { get; set; }

        /// <summary>
        ///     Gets or sets the amount of time that the Task Scheduler will wait before deleting the task after it expires. If no
        ///     value is specified for this property, then the Task Scheduler service will not delete the task.
        /// </summary>
        /// <value>
        ///     Gets and sets the amount of time that the Task Scheduler will wait before deleting the task after it expires. For
        ///     Task Scheduler 1.0, this property will return a TimeSpan of 1 second if the task is set to delete when done. For
        ///     either version, TimeSpan.Zero will indicate that the task should not be deleted.
        /// </value>
        /// <remarks>
        ///     A task expires after the end boundary has been exceeded for all triggers associated with the task. The end boundary
        ///     for a trigger is specified by the <c>EndBoundary</c> property of all trigger types.
        /// </remarks>
        [DefaultValue(typeof (TimeSpan), "12:00:00")]
        TimeSpan DeleteExpiredTaskAfter { get; set; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the task will not be started if the task is triggered to run in a
        ///     Remote Applications Integrated Locally (RAIL) session.
        /// </summary>
        /// <exception cref="NotSupportedPriorToException">Property set for a task on a Task Scheduler version prior to 2.1.</exception>
        [DefaultValue(false)]
        [XmlIgnore]
        bool DisallowStartOnRemoteAppSession { get; set; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the task is enabled. The task can be performed only when this
        ///     setting is TRUE.
        /// </summary>
        [DefaultValue(true)]
        bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the amount of time that is allowed to complete the task. By default, a task will be stopped 72 hours
        ///     after it starts to run.
        /// </summary>
        /// <value>
        ///     The amount of time that is allowed to complete the task. When this parameter is set to <see cref="TimeSpan.Zero" />
        ///     , the execution time limit is infinite.
        /// </value>
        /// <remarks>
        ///     If a task is started on demand, the ExecutionTimeLimit setting is bypassed. Therefore, a task that is started on
        ///     demand will not be terminated if it exceeds the ExecutionTimeLimit.
        /// </remarks>
        [DefaultValue(typeof (TimeSpan), "72:00:00")]
        TimeSpan ExecutionTimeLimit { get; set; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the task will not be visible in the UI by default.
        /// </summary>
        [DefaultValue(false)]
        bool Hidden { get; set; }


        /// <summary>
        ///     Gets or sets the number of times that the Task Scheduler will attempt to restart the task.
        /// </summary>
        /// <value>
        ///     The number of times that the Task Scheduler will attempt to restart the task. If this property is set, the
        ///     <see
        ///         cref="RestartInterval" />
        ///     property must also be set.
        /// </value>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        [DefaultValue(0)]
        [XmlIgnore]
        int RestartCount { get; set; }

        /// <summary>
        ///     Gets or sets a value that specifies how long the Task Scheduler will attempt to restart the task.
        /// </summary>
        /// <value>
        ///     A value that specifies how long the Task Scheduler will attempt to restart the task. If this property is set, the
        ///     <see
        ///         cref="RestartCount" />
        ///     property must also be set. The maximum time allowed is 31 days, and the minimum time allowed is 1 minute.
        /// </value>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        [DefaultValue(typeof (TimeSpan), "00:00:00")]
        [XmlIgnore]
        TimeSpan RestartInterval { get; set; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the Task Scheduler can start the task at any time after its
        ///     scheduled time has passed.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        [DefaultValue(false)]
        [XmlIgnore]
        bool StartWhenAvailable { get; set; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the Task Scheduler will wake the computer when it is time to run
        ///     the task.
        /// </summary>
        [DefaultValue(false)]
        bool WakeToRun { get; set; }

        /// <summary>
        ///     Gets or sets the policy that defines how the Task Scheduler handles multiple instances of the task.
        /// </summary>
        [DefaultValue(typeof (TaskInstancesPolicy), "IgnoreNew")]
        TaskInstancesPolicy MultipleInstances { get; set; }
    }
}