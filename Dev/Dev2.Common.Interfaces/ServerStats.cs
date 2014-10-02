
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Common.Interfaces
{
    public static class ServerStats
    {

        private static int _totalRequests=0;
        public static int TotalRequests { get{return _totalRequests;} }
        private static long _totalTime = 0;
        public static long TotalTime { get { return _totalTime; } }
        public static void IncrementTotalRequests()
        {
            System.Threading.Interlocked.Increment(ref _totalRequests);
        }

        public static void IncrementTotalTime( long time)
        {
            System.Threading.Interlocked.Add(ref _totalTime,time);
        }
    }
}
