using System;

namespace Dev2.Scheduler
{
    public class InvalidScheduleException : Exception
    {
        public InvalidScheduleException(string message)
            : base(message)
        {
        }
    }
}
