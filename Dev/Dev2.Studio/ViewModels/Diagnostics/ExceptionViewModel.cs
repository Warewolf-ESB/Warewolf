/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Model;
using Dev2.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dev2.Common;
using Dev2.Studio.Controller;
using Dev2.Studio.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Studio.ViewModels.Diagnostics
{
    [ExcludeFromCodeCoverage]
    public sealed class ExceptionViewModel : SimpleBaseViewModel, IExceptionViewModel
    {
        #region private fields

        BindableCollection<ExceptionUiModel> _exception;
        RelayCommand _cancelComand;
        string _stackTrace;
        ICommand _sendErrorCommand;
        bool _testing;
        IAsyncWorker _asyncWorker;
        string _emailAddress;
        string _stepsToFollow;
        string _serverLogFile;
        string _studioLogFile;

        #endregion private fields

        #region Constructor

        public ExceptionViewModel(IAsyncWorker asyncWorker)
        {
            //MEF!!!
            WindowNavigation = CustomContainer.Get<IWindowManager>();
            _asyncWorker = asyncWorker;
            Testing = false;
        }

        #endregion Constructor

        #region public properties

        public IWindowManager WindowNavigation { get; set; }

        public string OutputText { get; set; }

        public string OutputPath { get; set; }

        public IAsyncWorker AsyncWorker => _asyncWorker ?? (_asyncWorker = new AsyncWorker());

        public bool Testing
        {
            get => _testing;
            set
            {
                _testing = value;
                NotifyOfPropertyChange(() => Testing);
            }
        }

        /// <summary>
        /// Gets or sets the exception ui wrapper, using a collection to bind a treeview to it.
        /// </summary>
        /// <value>
        /// The exception UI Model, wrapping an actual exception.
        /// </value>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public BindableCollection<ExceptionUiModel> Exception
        {
            get => _exception ?? (_exception = new BindableCollection<ExceptionUiModel>());
            set
            {
                _exception = value;
                NotifyOfPropertyChange(() => Exception);
            }
        }

        public string StackTrace
        {
            get => _stackTrace;
            set
            {
                if (_stackTrace == value)
                {
                    return;
                }

                _stackTrace = value;
                NotifyOfPropertyChange(() => StackTrace);
            }
        }

        public string ServerLogFile
        {
            get => _serverLogFile;
            set
            {
                if (_serverLogFile == value)
                {
                    return;
                }

                _serverLogFile = value;
                NotifyOfPropertyChange(() => ServerLogFile);
            }
        }

        public string StudioLogFile
        {
            get => _studioLogFile;
            set
            {
                if (_studioLogFile == value)
                {
                    return;
                }

                _studioLogFile = value;
                NotifyOfPropertyChange(() => StudioLogFile);
            }
        }

        public BitmapSource ErrorIcon => Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Error.Handle, Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());

        #endregion public properties

        public ICommand CancelCommand
        {
            get
            {
                return _cancelComand ?? (_cancelComand = new RelayCommand(param => Cancel(), param => true));
            }
        }

        public ICommand SendErrorCommand
        {
            get
            {
                return _sendErrorCommand ?? (_sendErrorCommand = new RelayCommand(param =>
                {
                    Testing = true;
                    SendError();
                }, param => true));
            }
        }

        public string EmailAddress
        {
            get => _emailAddress;
            set
            {
                _emailAddress = value;
                NotifyOfPropertyChange(() => EmailAddress);
            }
        }

        private string ServerVersion
        {
            get
            {
                var activeServer = CustomContainer.Get<IShellViewModel>().ActiveServer;
                return activeServer.GetServerVersion();
            }
        }

        private string StudioVersion
        {
            get
            {
                var studioVersion = Warewolf.Studio.AntiCorruptionLayer.Utils.FetchVersionInfo();
                return studioVersion;
            }
        }

        public string StepsToFollow
        {
            get => _stepsToFollow;
            set
            {
                _stepsToFollow = value;
                NotifyOfPropertyChange(() => StepsToFollow);
            }
        }

        #region public methods

        void Cancel()
        {
            Testing = false;
            RequestClose();
        }

        private void SendError()
        {
            var messageList = new List<string>();

            if (Exception != null)
            {
                messageList.AddRange(Exception.Select(exceptionUiModel => exceptionUiModel.Message.Replace("Error :", "")));
            }

            var url = Warewolf.Studio.Resources.Languages.Core.SendErrorReportUrl;

            AsyncWorker.Start(() => SetupProgressSpinner(messageList, url), () =>
            {
                Testing = false;
                RequestClose();
                var popupController = new PopupController();
                popupController.ShowExceptionViewAppreciation();
            });
        }

        void SetupProgressSpinner(List<string> messageList, string url)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
            });

            var email = "No Email Provided";
            if (!string.IsNullOrWhiteSpace(EmailAddress))
            {
                email = EmailAddress;
            }
            var steps = "No Steps Provided";
            if (!string.IsNullOrWhiteSpace(StepsToFollow))
            {
                steps = StepsToFollow;
            }

            var description = "Server Version : " + ServerVersion + Environment.NewLine + " " + Environment.NewLine +
                                 "Studio Version : " + StudioVersion + Environment.NewLine + " " + Environment.NewLine +
                                 "Email Address : " + email + Environment.NewLine + " " + Environment.NewLine +
                                 "Steps to follow : " + steps + Environment.NewLine + " " + Environment.NewLine +
                                 StackTrace + Environment.NewLine + " " + Environment.NewLine +
                                 "Warewolf Studio log file : " + Environment.NewLine + " " + Environment.NewLine +
                                 StudioLogFile + Environment.NewLine + " " + Environment.NewLine +
                                 "Warewolf Server log file : " + Environment.NewLine + " " + Environment.NewLine +
                                 ServerLogFile;

            WebServer.SendErrorOpenInBrowser(messageList, description, url);
        }

        public static async Task<string> GetServerLogFile()
        {
            var activeEnvironment = CustomContainer.Get<IShellViewModel>().ActiveServer;
            var client = new WebClient { Credentials = activeEnvironment.Connection.HubConnection.Credentials };
            var managementServiceUri = WebServer.GetInternalServiceUri("getlogfile?numLines=10", activeEnvironment.Connection);
            var serverLogFile = await client.DownloadStringTaskAsync(managementServiceUri);
            client.Dispose();
            return serverLogFile;
        }

        public void GetStudioLogFile()
        {
            var localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logFile = Path.Combine(localAppDataFolder, "Warewolf", "Studio Logs", "Warewolf Studio.log");
            if (File.Exists(logFile))
            {
                var numberOfLines = GlobalConstants.LogFileNumberOfLines;
                var buffor = new List<string>();
                using (Stream stream = File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var realEnd = stream.Length - StackTrace.Length;
                    var file = new StreamReader(stream);
                    while (stream.Position<realEnd)
                    {
                        var line = file.ReadLine();
                        buffor.Add(line);
                    }
                    var lastLines = buffor.Skip(Math.Max(0,  buffor.Count - numberOfLines)).Take(numberOfLines).ToArray();
                    StudioLogFile = string.Join(Environment.NewLine, lastLines);
                }
            }
            else
            {
                StudioLogFile = "Could not locate Warewolf Studio log file.";
            }
        }

        /// <summary>
        /// Shows this instance.
        /// </summary>
        /// <author>jurie.smit</author>
        /// <date>2013/01/15</date>
        public void Show()
        {
            WindowNavigation.ShowDialog(this);
        }

        #endregion public methods
    }
}