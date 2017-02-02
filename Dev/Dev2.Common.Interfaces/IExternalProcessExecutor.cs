using System;

namespace Dev2.Common.Interfaces
{
    public interface IExternalProcessExecutor
    {
        void OpenInBrowser(Uri url);
    }
}