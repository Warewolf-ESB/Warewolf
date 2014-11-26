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