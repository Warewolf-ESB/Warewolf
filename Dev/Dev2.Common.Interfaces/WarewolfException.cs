    using System;

namespace Dev2.Common.Interfaces
{
    public class WarewolfException:Exception
    {
        
        public WarewolfException(string message, Exception innerException,ExceptionType exceptionType, ExceptionSeverity severity) :
            base(message, innerException??new Exception())
            
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
