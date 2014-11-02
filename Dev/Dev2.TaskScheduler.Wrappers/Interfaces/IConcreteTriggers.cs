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
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public interface ITimeTrigger : ITriggerDelay, ITrigger, IWrappedObject<TimeTrigger>
    {
        /// <summary>
        ///     Gets or sets a delay time that is randomly added to the start time of the trigger.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        TimeSpan RandomDelay { get; set; }
    }

    public interface IBootTrigger : ITriggerDelay, ITrigger, IWrappedObject<BootTrigger>
    {
    }

    public class Dev2BootTrigger : Dev2Trigger, IBootTrigger
    {
        public Dev2BootTrigger(ITaskServiceConvertorFactory _taskServiceConvertorFactory, BootTrigger instance)
            : base(_taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return Instance.Delay; }
            set { Instance.Delay = value; }
        }

        public new BootTrigger Instance
        {
            get { return (BootTrigger) base.Instance; }
        }
    }


    public interface IWeeklyTrigger : ICalendarTrigger, ITriggerDelay, ITrigger, IWrappedObject<WeeklyTrigger>
    {
        /// <summary>
        ///     Gets or sets the days of the week on which the task runs.
        /// </summary>
        DaysOfTheWeek DaysOfWeek { get; }

        /// <summary>
        ///     Gets or sets a delay time that is randomly added to the start time of the trigger.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        TimeSpan RandomDelay { get; }

        /// <summary>
        ///     Gets or sets the interval between the weeks in the schedule.
        /// </summary>
        short WeeksInterval { get; }
    }


    public interface IDailyTrigger : ICalendarTrigger, IWrappedObject<DailyTrigger>
    {
        /// <summary>
        ///     Sets or retrieves the interval between the days in the schedule.
        /// </summary>
        short DaysInterval { get; set; }

        /// <summary>
        ///     Gets or sets a delay time that is randomly added to the start time of the trigger.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        TimeSpan RandomDelay { get; set; }
    }


    public interface IEventTrigger : ITriggerDelay, ITrigger, IWrappedObject<EventTrigger>
    {
        /// <summary>
        ///     Gets or sets the XPath query string that identifies the event that fires the trigger.
        /// </summary>
        string Subscription { get; set; }

        /// <summary>
        ///     Gets a collection of named XPath queries. Each query in the collection is applied to the last matching event XML
        ///     returned from the subscription query specified in the Subscription property. The name of the query can be used as a
        ///     variable in the message of a
        ///     <see
        ///         cref="ShowMessageAction" />
        ///     action.
        /// </summary>
        NamedValueCollection ValueQueries { get; }

        /// <summary>
        ///     Gets basic event information.
        /// </summary>
        /// <param name="log">The event's log.</param>
        /// <param name="source">
        ///     The event's source. Can be <c>null</c>.
        /// </param>
        /// <param name="eventId">
        ///     The event's id. Can be <c>null</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if subscription represents a basic event, <c>false</c> if not.
        /// </returns>
        bool GetBasic(out string log, out string source, out int? eventId);

        /// <summary>
        ///     Sets the subscription for a basic event. This will replace the contents of the <see cref="Subscription" /> property
        ///     and clear all entries in the
        ///     <see
        ///         cref="ValueQueries" />
        ///     property.
        /// </summary>
        /// <param name="log">The event's log.</param>
        /// <param name="source">
        ///     The event's source. Can be <c>null</c>.
        /// </param>
        /// <param name="eventId">
        ///     The event's id. Can be <c>null</c>.
        /// </param>
        void SetBasic(string log, string source, int? eventId);
    }


    public interface IIdleTrigger : ITrigger, IWrappedObject<IdleTrigger>
    {
    }

    public interface ILogonTrigger : ITriggerDelay, ITriggerUserId, ITrigger, IWrappedObject<LogonTrigger>
    {
    }

    public interface IMonthlyDOWTrigger : ICalendarTrigger, ITriggerDelay, ITrigger, IWrappedObject<MonthlyDOWTrigger>
    {
        /// <summary>
        ///     Gets or sets the days of the week during which the task runs.
        /// </summary>
        DaysOfTheWeek DaysOfWeek { get; }

        /// <summary>
        ///     Gets or sets the months of the year during which the task runs.
        /// </summary>
        MonthsOfTheYear MonthsOfYear { get; }

        /// <summary>
        ///     Gets or sets a delay time that is randomly added to the start time of the trigger.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        TimeSpan RandomDelay { get; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the task runs on the last week of the month.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        bool RunOnLastWeekOfMonth { get; }

        /// <summary>
        ///     Gets or sets the weeks of the month during which the task runs.
        /// </summary>
        WhichWeek WeeksOfMonth { get; }
    }


    public interface IMonthlyTrigger : ICalendarTrigger, ITriggerDelay, IWrappedObject<MonthlyTrigger>
    {
        /// <summary>
        ///     Gets or sets the days of the month during which the task runs.
        /// </summary>
        int[] DaysOfMonth { get; }

        /// <summary>
        ///     Gets or sets the months of the year during which the task runs.
        /// </summary>
        MonthsOfTheYear MonthsOfYear { get; }

        /// <summary>
        ///     Gets or sets a delay time that is randomly added to the start time of the trigger.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        TimeSpan RandomDelay { get; }

        /// <summary>
        ///     Gets or sets a Boolean value that indicates that the task runs on the last day of the month.
        /// </summary>
        /// <exception cref="NotV1SupportedException">Not supported under Task Scheduler 1.0.</exception>
        bool RunOnLastDayOfMonth { get; }
    }

    public interface IRegistrationTrigger : ITrigger, ITriggerDelay, IWrappedObject<RegistrationTrigger>
    {
    }
}