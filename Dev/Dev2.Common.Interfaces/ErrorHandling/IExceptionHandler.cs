using System;

namespace Dev2.Common.Interfaces.ErrorHandling
{
    public interface IExceptionHandler
    {
        void Handle(Exception error);
        void AddHandler(Type t, Action a);
    }

    public interface IWarewolfApplicationEvent
    {
    }
}