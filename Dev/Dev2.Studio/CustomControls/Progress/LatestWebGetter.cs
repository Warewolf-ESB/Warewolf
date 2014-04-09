using System;
using System.IO;
using System.Net;
using Dev2.Helpers;
using Dev2.Providers.Logs;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Helpers
{
    // PBI 9512 - 2013.06.07 - TWR: added
    public class LatestWebGetter : ILatestGetter, IDisposable
    {
        readonly IDev2WebClient _webClient;

        public LatestWebGetter()
            : this(new Dev2WebClient(new WebClient()))
        {

        }

        public LatestWebGetter(IDev2WebClient webClient)
        {
            if(webClient == null)
            {
                throw new ArgumentNullException("webClient");
            }
            _webClient = webClient;
        }

        #region Implementation of IGetLatest

        public event EventHandler Invoked;

        public void GetLatest(string uri, string filePath)
        {
            using(var client = _webClient)
            {
                try
                {

                    // ensure dir structure is present ;)
                    var rootPath = Path.GetDirectoryName(filePath);

                    if(rootPath != null && !Directory.Exists(rootPath))
                    {
                        Directory.CreateDirectory(rootPath);
                    }

                    // DO NOT use DownloadFile as this will nuke the file even if there is an error
                    var source = client.DownloadString(uri);
                    File.WriteAllText(filePath, source);

                }
                catch(Exception ex)
                {
                    this.TraceInfo(string.Format("Get lastest version of '{0}' failed: {1}", uri, ex.Message));
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

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _webClient.Dispose();
        }

        #endregion
    }
}