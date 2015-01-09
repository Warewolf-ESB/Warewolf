using System;
using Dev2.Common.Interfaces.ErrorHandling;

namespace Warewolf.Studio.Models.Toolbox
{
    class WarewolfUnsupportedToolException : WarewolfException
    {
        // ReSharper disable TooManyDependencies
        public WarewolfUnsupportedToolException(string message, Exception innerException)
            // ReSharper restore TooManyDependencies
            : base(message, innerException, ExceptionType.Execution, ExceptionSeverity.Error)
        {
        }
    }
}
