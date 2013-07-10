using System;
using System.IO;
using System.Net;
using Dev2.Common;

namespace Dev2.Studio.Core.Helpers
{
    // PBI 9512 - 2013.06.07 - TWR: added
    public class LatestWebGetter : ILatestGetter
    {
        #region Implementation of IGetLatest

        public event EventHandler Invoked;

        public void GetLatest(string uri, string filePath)
        {
            using(var client = new WebClient())
            {
                try
                {
                    // DO NOT use DownloadFile as this will nuke the file even if there is an error
                    var source = client.DownloadString(uri);
                    File.WriteAllText(filePath, source);
                }
                catch(Exception ex)
                {
                    File.WriteAllText(filePath, ex.Message);
                    StudioLogger.LogMessage(string.Format("Get lastest version of '{0}' failed: {1}", uri, ex.Message));
                }
            }
            RaiseInvoked();
        }

        #endregion

        void RaiseInvoked()
        {
            if(Invoked != null)
            {
                Invoked(this, EventArgs.Empty);
            }
        }
    }
}