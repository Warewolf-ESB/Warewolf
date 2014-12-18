using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;


namespace Warewolf.Studio.ServerProxyLayer
{
    public class LogManagerProxy:ProxyBase, ILoggingManager
    {
        readonly ICredentials _credentials;
        readonly Uri _managementServiceUri;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event AsyncCompletedEventHandler DownloadFileCompleted;

        string _serverLogFile;

        // ReSharper disable TooManyDependencies
        public LogManagerProxy(ICredentials credentials, ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection, Uri managementServiceUri)
            // ReSharper restore TooManyDependencies
            : base(communicationControllerFactory, connection)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "credentials", credentials }, { "Uri", managementServiceUri } });
            _credentials = credentials;
            _managementServiceUri = managementServiceUri;
        }

        #region Implementation of ILoggingManager

        public void SetMaxLogSize(int sizeInMb)
        {
            var controller = CommunicationControllerFactory.CreateController("SetLogMaxSizeService");
            controller.AddPayloadArgument("sizeInMb", sizeInMb.ToString(CultureInfo.InvariantCulture));
            controller.ExecuteCommand<string>(Connection, GlobalConstants.ServerWorkspaceID);
        }

        /// <summary>
        /// get the log file
        /// </summary>
        /// <returns></returns>
        public void GetLog(int sizeInMb)
        {
            WebClient client = new WebClient { Credentials = _credentials };

            client.DownloadProgressChanged += OnDownloadProgressChanged;
            client.DownloadFileCompleted += OnDownloadFileCompleted;
            var tempPath = Path.GetTempPath();
            _serverLogFile = Path.Combine(tempPath, Connection.DisplayName + " Server Log.txt");
            client.DownloadFileAsync(_managementServiceUri, _serverLogFile);
        }

        void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadFileCompleted(sender, e);
        }

        void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressChanged(sender,e);
        }

        #endregion
    }
}
