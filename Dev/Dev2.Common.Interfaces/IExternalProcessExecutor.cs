using System;
using System.Diagnostics;

namespace Dev2.Common.Interfaces
{
    public interface IExternalProcessExecutor
    {
        void OpenInBrowser(Uri url);
        Process Start(ProcessStartInfo startInfo);
    }
}