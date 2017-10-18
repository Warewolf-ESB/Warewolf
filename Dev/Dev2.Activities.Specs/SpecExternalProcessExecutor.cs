using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Dev2.Common;

namespace Dev2.Activities.Specs
{
    internal class SpecExternalProcessExecutor : ISpecExternalProcessExecutor
    {
        #region Implementation of IExternalProcessExecutor

        public SpecExternalProcessExecutor()
        {
            WebResult = new List<string>();
        }

        public void OpenInBrowser(Uri url)
        {

            try
            {
                using (var client = new WebClient())
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    WebResult.Add(client.DownloadString(url));
                }

            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, "Warewolf Error");
            }

        }

        public Process Start(ProcessStartInfo startInfo)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of ISpecExternalProcessExecutor

        public List<string> WebResult { get; set; }

        #endregion
    }
}