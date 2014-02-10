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
