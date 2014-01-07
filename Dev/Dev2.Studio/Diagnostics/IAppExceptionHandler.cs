using System;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public interface IAppExceptionHandler
    {
        bool Handle(Exception e);
    }
}
