using System;
using Dev2.Common.Interfaces.ErrorHandling;

namespace Warewolf.Studio.ViewModels
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class WarewolfInvalidTypeException : WarewolfException
    {
        // ReSharper disable TooManyDependencies
        public WarewolfInvalidTypeException(string message, Exception innerException)
            // ReSharper restore TooManyDependencies
            : base(message, innerException, ExceptionType.Execution,ExceptionSeverity.Critical )
        {

        }
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
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