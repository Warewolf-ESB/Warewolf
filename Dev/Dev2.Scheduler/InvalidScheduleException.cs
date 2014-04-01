using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Scheduler
{
    public class InvalidScheduleException : Exception
    {
        public InvalidScheduleException(string message) : base(message)
        {
        }
    }
}
