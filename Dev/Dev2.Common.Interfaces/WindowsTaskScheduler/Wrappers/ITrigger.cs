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
    public interface ITrigger : IDisposable, IWrappedObject<Trigger>
    {
        /// <summary>
        ///     Gets or sets a Boolean value that indicates whether the trigger is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        ///     Gets or sets the date and time when the trigger is deactivated. The trigger cannot start the task after it is
        ///     deactivated.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Version 1 (1.1 on all systems prior to Vista) of the native library only allows for the Day, Month and Year
        ///         values of the
        ///         <see
        ///             cref="DateTime" />
        ///         structure.
        ///     </para>
        ///     <para>
        ///         Version 2 (1.2 or higher) of the native library only allows for both date and time and all
        ///         <see
        ///             cref="DateTime.Kind" />
        ///         values. However, the user interface and <see cref="Trigger.ToString()" /> methods
        ///         will always show the time translated to local time. The library makes every attempt to maintain the Kind value.
        ///         When using the UI elements provided in the TaskSchedulerEditor
        ///         library, the "Synchronize across time zones" checkbox will be checked if the Kind is Local or Utc. If the Kind
        ///         is Unspecified and the user selects the checkbox, the Kind will
        ///         be changed to Utc and the time adjusted from the value displayed as the local time.
        ///     </para>
        /// </remarks>
        [DefaultValue(typeof (DateTime), "9999-12-31T23:59:59.9999999")]
        DateTime EndBoundary { get; set; }

        /// <summary>
        ///     Gets or sets the maximum amount of time that the task launched by this trigger is allowed to run. Not available
        ///     with Task Scheduler 1.0.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        [DefaultValue(typeof (TimeSpan), "00:00:00")]
        [XmlIgnore]
        TimeSpan ExecutionTimeLimit { get; }

        /// <summary>
        ///     Gets or sets the identifier for the trigger. Cannot set with Task Scheduler 1.0.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        [DefaultValue(null)]
        [XmlIgnore]
        string Id { get; set; }

        /// <summary>
        ///     Gets a <see cref="RepetitionPattern" /> instance that indicates how often the task is run and how long the
        ///     repetition pattern is repeated after the task is started.
        /// </summary>
        IRepetitionPattern Repetition { get; }

        /// <summary>
        ///     Gets or sets the date and time when the trigger is activated.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Version 1 (1.1 on all systems prior to Vista) of the native library only allows for <see cref="DateTime" />
        ///         values where the
        ///         <see
        ///             cref="DateTime.Kind" />
        ///         is unspecified.
        ///         If the DateTime value Kind is <see cref="DateTimeKind.Local" /> then it will be used as is. If the DateTime
        ///         value Kind is
        ///         <see
        ///             cref="DateTimeKind.Utc" />
        ///         then it will be
        ///         converted to the local time and then used.
        ///     </para>
        ///     <para>
        ///         Version 2 (1.2 or higher) of the native library only allows for all <see cref="DateTime.Kind" /> values.
        ///         However, the user interface and
        ///         <see
        ///             cref="Trigger.ToString()" />
        ///         methods
        ///         will always show the time translated to local time. The library makes every attempt to maintain the Kind value.
        ///         When using the UI elements provided in the TaskSchedulerEditor
        ///         library, the "Synchronize across time zones" checkbox will be checked if the Kind is Local or Utc. If the Kind
        ///         is Unspecified and the user selects the checkbox, the Kind will
        ///         be changed to Utc and the time adjusted from the value displayed as the local time.
        ///     </para>
        ///     <para>
        ///         Under Version 2, when converting the string used in the native library for this value (ITrigger.Startboundary)
        ///         this library will behave as follows:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>YYYY-MM-DDTHH:MM:SS format uses DateTimeKind.Unspecified and the time specified.</description>
        ///             </item>
        ///             <item>
        ///                 <description>YYYY-MM-DDTHH:MM:SSZ format uses DateTimeKind.Utc and the time specified as the GMT time.</description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     YYYY-MM-DDTHH:MM:SS±HH:MM format uses DateTimeKind.Local and the time specified in that
        ///                     time zone.
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        DateTime StartBoundary { get; set; }

        /// <summary>
        ///     Gets the type of the trigger.
        /// </summary>
        /// <value>
        ///     The <see cref="TaskTriggerType" /> of the trigger.
        /// </value>
        [XmlIgnore]
        TaskTriggerType TriggerType { get; }
    }
}