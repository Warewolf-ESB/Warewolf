
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

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
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
        ScheduleRunStatus Success { get; }
        /// <summary>
        /// Windows event id
        /// </summary>
        string EventId { get; }


        string FailureReason { get; }


    }
}
