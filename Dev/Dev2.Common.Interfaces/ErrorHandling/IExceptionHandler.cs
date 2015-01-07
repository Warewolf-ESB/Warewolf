using System;

namespace Dev2.Common.Interfaces.ErrorHandling
{
    public interface IExceptionHandler
    {
        void Handle(Exception error);
 
    }

    public interface IWarewolfApplicationEvent
    {
    }
}