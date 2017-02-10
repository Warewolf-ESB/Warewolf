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
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Dev2.Studio.Controller;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Diagnostics
{
    /// <summary>
    /// Used to display a user-friendly exceptionmessage, and allow the user to send a report via email
    /// </summary>
    /// <author>jurie.smit</author>
    /// <date>2013/01/15</date>
    public sealed class ExceptionViewModel : SimpleBaseViewModel, IExceptionViewModel
    {
        #region private fields

        private BindableCollection<ExceptionUiModel> _exception;
        private RelayCommand _cancelComand;
        private string _stackTrace;
        private ICommand _sendErrorCommand;
        private bool _testing;
        private IAsyncWorker _asyncWorker;
        private string _emailAddress;
        private string _stepsToFollow;
        private string _serverLogFile;
        private string _studioLogFile;

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

        public IAsyncWorker AsyncWorker
        {
            get
            {
                if (_asyncWorker == null)
                {
                    _asyncWorker = new AsyncWorker();
                }
                return _asyncWorker;
            }
        }

        public bool Testing
        {
            get
            {
                return _testing;
            }
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
            get
            {
                return _exception ?? (_exception = new BindableCollection<ExceptionUiModel>());
            }
            set
            {
                _exception = value;
                NotifyOfPropertyChange(() => Exception);
            }
        }

        public string StackTrace
        {
            get
            {
                return _stackTrace;
            }
            set
            {
                if (_stackTrace == value) return;

                _stackTrace = value;
                NotifyOfPropertyChange(() => StackTrace);
            }
        }

        public string ServerLogFile
        {
            get
            {
                return _serverLogFile;
            }
            set
            {
                if (_serverLogFile == value) return;

                _serverLogFile = value;
                NotifyOfPropertyChange(() => ServerLogFile);
            }
        }

        public string StudioLogFile
        {
            get
            {
                return _studioLogFile;
            }
            set
            {
                if (_studioLogFile == value) return;

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
            get { return _emailAddress; }
            set
            {
                _emailAddress = value;
                NotifyOfPropertyChange(() => EmailAddress);
            }
        }

        public string ServerVersion
        {
            get
            {
                var activeServer = CustomContainer.Get<IShellViewModel>().ActiveServer;
                return activeServer.GetServerVersion();
            }
        }

        public string StudioVersion
        {
            get
            {
                var studioVersion = Warewolf.Studio.AntiCorruptionLayer.Utils.FetchVersionInfo();
                return studioVersion;
            }
        }

        public string StepsToFollow
        {
            get { return _stepsToFollow; }
            set
            {
                _stepsToFollow = value;
                NotifyOfPropertyChange(() => StepsToFollow);
            }
        }

        #region public methods

        private void Cancel()
        {
            Testing = false;
            RequestClose();
        }

        public void SendError()
        {
            List<string> messageList = new List<string>();

            if (Exception != null)
            {
                messageList.AddRange(Exception.Select(exceptionUiModel => exceptionUiModel.Message.Replace("Error :", "")));
            }

            string url = Warewolf.Studio.Resources.Languages.Core.SendErrorReportUrl;

            AsyncWorker.Start(() => SetupProgressSpinner(messageList, url), () =>
            {
                Testing = false;
                RequestClose();
                var popupController = new PopupController();
                popupController.ShowExceptionViewAppreciation();
            });
        }

        private void SetupProgressSpinner(List<string> messageList, string url)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
            });

            string email = "No Email Provided";
            if (!string.IsNullOrWhiteSpace(EmailAddress))
            {
                email = EmailAddress;
            }
            string steps = "No Steps Provided";
            if (!string.IsNullOrWhiteSpace(StepsToFollow))
            {
                steps = StepsToFollow;
            }

            string description = "Server Version : " + ServerVersion + Environment.NewLine + " " + Environment.NewLine +
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
            var activeEnvironment = CustomContainer.Get<IMainViewModel>().ActiveEnvironment;
            WebClient client = new WebClient { Credentials = activeEnvironment.Connection.HubConnection.Credentials };
            var managementServiceUri = WebServer.GetInternalServiceUri("getlogfile?numLines=10", activeEnvironment.Connection);
            var serverLogFile = await client.DownloadStringTaskAsync(managementServiceUri);
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
                        string line = file.ReadLine();
                        buffor.Add(line);
                    }
                    string[] lastLines = buffor.Skip(Math.Max(0,  buffor.Count - numberOfLines)).Take(numberOfLines).ToArray();
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