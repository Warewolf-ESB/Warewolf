namespace Log4Net.Async
{
    using log4net.Core;

    internal sealed class LoggingEventContext
    {
        public LoggingEventContext(LoggingEvent loggingEvent)
        {
            LoggingEvent = loggingEvent;
            //HttpContext = httpContext;
        }

        public LoggingEvent LoggingEvent { get; set; }

        //public object HttpContext { get; set; }
    }
}