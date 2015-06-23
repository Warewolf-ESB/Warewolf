using System;
using System.Diagnostics;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    public class ExternalProcessExecutor : IExternalProcessExecutor
    {
        #region Implementation of IExternalProcessExecutor

        public void OpenInBrowser(Uri url)
        {
            Process.Start(url.ToString());
        }

        #endregion
    }
}