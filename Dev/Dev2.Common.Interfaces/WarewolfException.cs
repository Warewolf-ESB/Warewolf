    using System;

namespace Dev2.Common.Interfaces.ErrorHandling
{
    public class WarewolfException:Exception
    {
        // ReSharper disable TooManyDependencies
        public WarewolfException(string message, Exception innerException,ExceptionType exceptionType, ExceptionSeverity severity) :
            base(message,innerException)
            // ReSharper restore TooManyDependencies
        {
            Severity = severity;
            ExceptionType = exceptionType;
        }

        public ExceptionType ExceptionType { get; private set; }
        public ExceptionSeverity Severity{get;private set;}
    }

    public enum ExceptionSeverity
    {
        Critical,
        Error,
        User,
        Etc,
        Minor
    }

    public enum ExceptionType
    {
        Connection,
        Execution,
        Unknown

    }
}
