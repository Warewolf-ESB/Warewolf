
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

namespace Dev2.Integration.Tests.MEF
{
    class StopWatch
    {
        private DateTime startTime { get; set; }
        DateTime stopTime { get; set; }
        bool isRunning { get; set; }



        public void Start()
        {
            startTime = DateTime.Now;
            isRunning = true;
        }

        public void Stop()
        {
            stopTime = DateTime.Now;
            isRunning = false;
        }

        // elapsed time in milliseconds
        public double GetElapsedTime()
        {
            TimeSpan interval;

            if(isRunning)
                interval = DateTime.Now - startTime;
            else
                interval = stopTime - startTime;

            return interval.TotalMilliseconds;
        }
    }
}
