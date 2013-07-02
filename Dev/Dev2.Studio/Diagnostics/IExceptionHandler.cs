using System;

namespace Dev2.Studio.Diagnostics
{
    public interface IExceptionHandler
    {
        bool Handle(Exception e);
    }
}
