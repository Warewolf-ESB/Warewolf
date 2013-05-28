using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Integration.Tests.MEF
{
    class StopWatch
    {
        private DateTime startTime { get; set; }
        DateTime stopTime { get; set; }
        bool isRunning { get; set; }



        public void Start()
        {
            this.startTime = DateTime.Now;
            this.isRunning = true;
        }

        public void Stop()
        {
            this.stopTime = DateTime.Now;
            this.isRunning = false;
        }

        // elapsed time in milliseconds
        public double GetElapsedTime()
        {
            TimeSpan interval;

            if (isRunning)
                interval = DateTime.Now - startTime;
            else
                interval = stopTime - startTime;

            return interval.TotalMilliseconds;
        }
    }
}
