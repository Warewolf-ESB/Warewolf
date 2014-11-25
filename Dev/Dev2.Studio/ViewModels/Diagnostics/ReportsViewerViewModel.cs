
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Diagnostics;
using Dev2.Services.Events;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Diagnostics
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The viewmodel used for displaying reports
    /// </summary>
    /// <date>2013/05/24</date>
    /// <author>
    /// Jurie.smit
    /// </author>
    public class ReportsManagerViewModel : BaseWorkSurfaceViewModel
    {
        #region private fields

        private ReportType _reportType;
        private IEnvironmentModel _selectedServer;
        private DirectoryPath _logDirectory;
        private BindableCollection<FilePath> _logFiles;
        private FilePath _selectedLogFile;
        private readonly IDebugProvider _debugProvider;
        private DebugOutputViewModel _debugOutput;
        private bool _isRefreshing;

        #endregion fields

        #region public properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy refreshing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is refreshing; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                if(_isRefreshing == value)
                {
                    return;
                }

                _isRefreshing = value;
                NotifyOfPropertyChange(() => IsRefreshing);
            }
        }

        /// <summary>
        /// Gets the debug output viewmodel used for displaying the logs.
        /// </summary>
        /// <value>
        /// The debug output.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public DebugOutputViewModel DebugOutput
        {
            get
            {
                return _debugOutput ?? (_debugOutput = new DebugOutputViewModel(EventPublishers.Studio, EnvironmentRepository.Instance, new DebugOutputFilterStrategy()) { ShowDebugStatus = false });
            }
        }

        /// <summary>
        /// Gets or sets the selected server. Also fires the logic to get the logdirectory for that server
        /// </summary>
        /// <value>
        /// The selected server.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public IEnvironmentModel SelectedServer
        {
            get
            {
                return _selectedServer;
            }
            set
            {
                // ReSharper disable PossibleUnintendedReferenceComparison
                if(_selectedServer == value)
                // ReSharper restore PossibleUnintendedReferenceComparison
                {
                    return;
                }

                if(value != null && !value.IsConnected)
                {
                    PopupController.ShowNotConnected();
                }

                _selectedServer = value;
                LogDirectory = null;
                if(value != null)
                {
                    LogDirectory = GetLogDirectory(_selectedServer);
                }
                NotifyOfPropertyChange(() => SelectedServer);
            }
        }

        /// <summary>
        /// Gets or sets the type of the report.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public ReportType ReportType
        {
            get
            {
                return _reportType;
            }
            set
            {
                if(_reportType == value)
                {
                    return;
                }

                _reportType = value;
                NotifyOfPropertyChange(() => ReportType);
            }
        }

        /// <summary>
        /// Gets or sets the web client.
        /// </summary>
        /// <value>
        /// The web client.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public WebClient WebClient { get; set; }

        /// <summary>
        /// Gets or sets the popup controller through a mef import.
        /// </summary>
        /// <value>
        /// The popup controller.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public IPopupController PopupController { get; set; }

        /// <summary>
        /// Gets or sets the log directory.
        /// </summary>
        /// <value>
        /// The log directory.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public DirectoryPath LogDirectory
        {
            get
            {
                return _logDirectory;
            }
            set
            {
                if(_logDirectory == value)
                {
                    return;
                }

                _logDirectory = value;
                if(value != null)
                {
                    GetLogFiles(SelectedServer, _logDirectory);
                }
                NotifyOfPropertyChange(() => LogDirectory);
            }
        }

        /// <summary>
        /// Gets the collections of log files.
        /// </summary>
        /// <value>
        /// The log files.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public BindableCollection<FilePath> LogFiles
        {
            get
            {
                return _logFiles ?? (_logFiles = new BindableCollection<FilePath>());
            }
        }

        /// <summary>
        /// Gets or sets the selected log file.
        /// </summary>
        /// <value>
        /// The selected log file.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public FilePath SelectedLogFile
        {
            get
            {
                return _selectedLogFile;
            }
            set
            {
                if(_selectedLogFile == value)
                {
                    return;
                }

                _selectedLogFile = value;
                if(value != null)
                {
                    GetLogDetails(_selectedLogFile);
                }
                NotifyOfPropertyChange(() => SelectedLogFile);
            }
        }

        #endregion properties

        #region ctor + init

        public ReportsManagerViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public ReportsManagerViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
            WebClient = new WebClient();
            _debugProvider = new JsonDebugProvider();
        }

        #endregion ctor

        #region public methods

        /// <summary>
        /// Deletes the specified file path by sending a message to web client.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public void Delete(FilePath filePath)
        {
            var address = String.Format(SelectedServer.Connection.WebServerUri + "{0}/{1}?Directory={2}&FilePath={3}",
                "Services", "DeleteLogService", LogDirectory.PathToSerialize, filePath.Title);
            var response = WebClient.UploadString(address, string.Empty);

            if(response.Contains("Success"))
            {
                LogFiles.Remove(filePath);
                DebugOutput.Clear();
            }
            else
            {
                ShowError(response);
            }
        }

        /// <summary>
        /// Deletes all the logs from the selected server directory.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public void DeleteAll()
        {
            var address = String.Format(SelectedServer.Connection.WebServerUri + "{0}/{1}?Directory={2}", "Services", "ClearLogService", LogDirectory.PathToSerialize);
            var response = WebClient.UploadString(address, string.Empty);

            if(response.Contains("Success"))
            {
                LogFiles.Clear();
            }
            else
            {
                ShowError(response);
            }
        }

        /// <summary>
        /// Refreshes the content of the directory.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public void RefreshDirectory()
        {
            IsRefreshing = true;
            GetLogFiles(SelectedServer, LogDirectory);
            IsRefreshing = false;
        }

        #endregion

        #region private methods
        /// <summary>
        /// Gets the log details through the webclient.
        /// </summary>
        /// <param name="selectedLogFile">The selected log file.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        private void GetLogDetails(FilePath selectedLogFile)
        {
            DebugOutput.Clear();
            try
            {
                var debugStates = _debugProvider.GetDebugStates(SelectedServer.Connection.WebServerUri.AbsoluteUri, LogDirectory, selectedLogFile);
                debugStates.ToList().ForEach(s => DebugOutput.Append(s));
            }
            catch(Exception exception)
            {
                ShowError(exception);
            }
        }

        /// <summary>
        /// Gets the log directory from the server (using webclient).
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        private DirectoryPath GetLogDirectory(IEnvironmentModel server)
        {
            var address = String.Format(server.Connection.WebServerUri + "{0}/{1}", "Services", "FindLogDirectoryService");
            var datalistJson = WebClient.UploadString(address, string.Empty);
            var directory = JsonConvert.DeserializeObject<DirectoryPath>(datalistJson);
            return directory;
        }

        /// <summary>
        /// Gets the log files from the server (using webclient).
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        public IEnumerable<FilePath> GetLogFiles(IEnvironmentModel server, DirectoryPath directory)
        {
            LogFiles.Clear();

            if(server == null || directory == null)
            {
                return new List<FilePath>();
            }

            var address = String.Format(server.Connection.WebServerUri + "{0}/{1}?DirectoryPath={2}",
                "Services", "FindDirectoryService", directory.PathToSerialize);
            var datalistJson = WebClient.UploadString(address, string.Empty);
            if(datalistJson.Contains("Error"))
            {
                var error = "Error: Log directory not found." + Environment.NewLine +
                            datalistJson.GetManagementPayload();
                ShowErrorPopup(error);
            }
            else
            {
                var filePaths = JsonConvert.DeserializeObject<List<FilePath>>(datalistJson);
                filePaths.ForEach(fp =>
                    {
                        if(fp.Title.EndsWith(".wwlfl"))
                        {
                            LogFiles.Add(fp);
                        }
                    });
                return filePaths;
            }
            return new List<FilePath>();
        }

        /// <summary>
        /// Friendly helper to shows an error.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        private void ShowError(string response)
        {
            var error = response.GetManagementPayload();
            ShowErrorPopup(error);
        }

        /// <summary>
        /// Friendly helper to shows an error.
        /// </summary>
        /// <param name="e">The exception</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        private void ShowError(Exception e)
        {
            var error = e.Message;
            ShowErrorPopup(error);
        }

        private void ShowErrorPopup(string message)
        {
            PopupController.Header = "Error";
            PopupController.Description = message;
            PopupController.ImageType = MessageBoxImage.Error;
            PopupController.Show();
        }

        #endregion private methods

        #region overrides

        /// <summary>
        /// Called by caliburn micro when [view attached].
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/24</date>
        protected override void OnViewAttached(object view, object context)
        {
            var servers = ServerProvider.Instance.Load();
            var localHost = servers.FirstOrDefault(s => s.IsLocalHost);
            if(localHost != null && localHost.IsConnected)
                SelectedServer = localHost;
            base.OnViewAttached(view, context);
        }

        #endregion
    }
}
