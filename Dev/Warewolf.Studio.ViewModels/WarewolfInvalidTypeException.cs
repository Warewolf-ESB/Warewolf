using System;
using Dev2.Common.Interfaces.ErrorHandling;

namespace Warewolf.Studio.ViewModels
{
    public class WarewolfInvalidTypeException : WarewolfException
    {
        // ReSharper disable TooManyDependencies
        public WarewolfInvalidTypeException(string message, Exception innerException)
            // ReSharper restore TooManyDependencies
            : base(message, innerException, ExceptionType.Execution,ExceptionSeverity.Critical )
        {

        }
    }
    public class WarewolfInvalidPermissionsException: WarewolfException
    {
        // ReSharper disable TooManyDependencies
        public WarewolfInvalidPermissionsException(string message, Exception innerException)
            // ReSharper restore TooManyDependencies
            : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.User)
        {

        }
    }
}