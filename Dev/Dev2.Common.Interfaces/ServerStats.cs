/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading;

namespace Dev2.Common.Interfaces
{
    public static class ServerStats
    {
        static int _totalRequests;
        static long _totalTime;
        static int _totalExecutions;
        static int _usageServerRetry;

        public static Guid SessionId { get; set; }
        public static int TotalRequests => _totalRequests;
        public static int TotalExecutions => _totalExecutions;
        public static int UsageServerRetry => _usageServerRetry;
        public static long TotalTime => _totalTime;

        public static void IncrementUsageServerRetry()
        {
            Interlocked.Increment(ref _usageServerRetry);
        }
        public static void ResetTotalExecutions()
        {
            Interlocked.Exchange(ref _totalExecutions, 0);
        }
        public static void ResetUsageServerRetry()
        {
            Interlocked.Exchange(ref _usageServerRetry, 0);
        }

        public static void IncrementTotalRequests()
        {
            Interlocked.Increment(ref _totalRequests);
        }

        public static void IncrementTotalExecutions()
        {
            Interlocked.Increment(ref _totalExecutions);
        }

        public static void IncrementTotalTime(long time)
        {
            Interlocked.Add(ref _totalTime, time);
        }
    }
}