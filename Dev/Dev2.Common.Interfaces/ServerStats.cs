/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading;

namespace Dev2.Common.Interfaces
{
    public static class ServerStats
    {
        private static int _totalRequests;

        private static long _totalTime;

        public static int TotalRequests => _totalRequests;

        public static long TotalTime => _totalTime;

        public static void IncrementTotalRequests()
        {
            Interlocked.Increment(ref _totalRequests);
        }

        public static void IncrementTotalTime(long time)
        {
            Interlocked.Add(ref _totalTime, time);
        }
    }
}