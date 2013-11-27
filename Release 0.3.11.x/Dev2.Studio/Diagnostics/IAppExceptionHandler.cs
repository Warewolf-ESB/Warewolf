using System;
using Caliburn.Micro;

namespace Dev2.Studio.Diagnostics
{
    public interface IAppExceptionHandler
    {
        bool Handle(Exception e);
    }
}
