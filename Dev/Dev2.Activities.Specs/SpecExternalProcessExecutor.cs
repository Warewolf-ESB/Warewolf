using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Dev2.Common;

namespace Dev2.Activities.Specs
{
    class SpecExternalProcessExecutor : ISpecExternalProcessExecutor
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
                if (e is WebException webException)
                {
                    var reader = new StreamReader(webException.Response.GetResponseStream());
                    WebResult.Add(reader.ReadToEnd());
                }
                Dev2Logger.Error(e, "Warewolf Error");
            }

        }

        public void OpenInBrowserDefaultCredentials(Uri url)
        {

            try
            {
                using (var client = new WebClient())
                {
                    client.UseDefaultCredentials = true;
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