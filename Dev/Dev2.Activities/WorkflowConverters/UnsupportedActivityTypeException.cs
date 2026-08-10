/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Activities.WF
{
    /// <summary>
    /// Thrown by <see cref="X6ToWorkflowConverter"/> when an X6 graph node's <c>data.type</c>
    /// does not match any activity type the converter knows how to instantiate. Replaces the
    /// previous silent fallback to a no-op <c>WriteLine</c> activity, which allowed unrecognised
    /// activity types to be accepted as if they had converted successfully.
    /// </summary>
    public class UnsupportedActivityTypeException : Exception
    {
        /// <summary>
        /// The raw, unrecognised <c>data.type</c> value from the X6 graph node.
        /// </summary>
        public string ActivityType { get; }

        public UnsupportedActivityTypeException(string activityType)
            : base($"Activity type '{activityType}' is not supported by the X6 workflow converter.")
        {
            ActivityType = activityType;
        }

        public UnsupportedActivityTypeException(string activityType, string message)
            : base(message)
        {
            ActivityType = activityType;
        }
    }
}
