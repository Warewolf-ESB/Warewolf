using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Network.Execution;
using Dev2.Studio.AppResources.Behaviors;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards;
using System;
using System.Windows.Input;
using Action = System.Action;


namespace Dev2.Studio.ViewModels.Wizards
{
    public class ActivitySettingsViewModel : SimpleBaseViewModel, 
        IHandle<EnvironmentConnectedMessage>, IHandle<EnvironmentDisconnectedMessage>
    {
        #region Class Members

        private WizardInvocationTO _wizardInvocationTO;
        private IContextualResourceModel _hostResource;
        private IBinaryDataList _transferDataList;

        private RelayCommand _cancelCommand;
        private RelayCommand _tryConnectCommand;

        private bool _showConnectPrompt = false;
        #endregion Class Members

        #region Events

        #region BrowseRequested

        /// <summary>
        /// Occurs when a browse action is requested for the specific settings wizard.
        /// </summary>
        public event EventHandler<BrowseRequestedEventArgs> BrowseRequested;

        protected void OnBrowseRequested(string url, string args, IEnvironmentModel environmentModel)
        {
            if (BrowseRequested != null)
            {
                BrowseRequested(this, new BrowseRequestedEventArgs(url, args, environmentModel));
            }
        }

        #endregion BrowseRequested

        #region Close

        /// <summary>
        /// Occurs when a close request is recieved
        /// </summary>
        public event EventHandler Close;

        protected void OnClose()
        {
            if (Close != null)
            {
                Close(this, new EventArgs());
            }
        }

        #endregion Close

        #endregion Events

        #region Constructors

        public ActivitySettingsViewModel(WizardInvocationTO wizardInvocationTO, IContextualResourceModel hostResource)
        {
            if (wizardInvocationTO == null)
            {
                throw new ArgumentNullException("wizardInvocationTO");
            }

            if (hostResource == null)
            {
                throw new ArgumentNullException("hostResource");
            }

            _wizardInvocationTO = wizardInvocationTO;
            _hostResource = hostResource;

            Popup = ImportService.GetExportValue<IPopUp>();
        }

        #endregion Constructors

        #region Properties

        public IPopUp Popup { get; set; }

        public bool ShowConnectPrompt
        {
            get
            {
                return _showConnectPrompt;
            }
            private set
            {
                _showConnectPrompt = value;
                OnPropertyChanged("ShowConnectPrompt");
            }
        }

        public string WizardTitle
        {
            get
            {
                return _wizardInvocationTO.WizardTitle;
            }
        }

        #endregion Properties

        #region Commands

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(param =>
                    {
                        if (ShowConnectPrompt)
                        {
                            ForceCancel();
                        }
                        else
                        {
                            Cancel();
                        }
                    }, param => true);
                }
                return _cancelCommand;
            }
        }

        public ICommand TryConnectCommand
        {
            get
            {
                if (_tryConnectCommand == null)
                {
                    _tryConnectCommand = new RelayCommand(param =>
                    {
                        TryConnect();
                    }, param => true);
                }
                return _tryConnectCommand;
            }
        }

        #endregion Commands

        #region Public Methods

        /// <summary>
        /// Runs the specific settings wizard.
        /// </summary>
        public bool InvokeWizard()
        {
            if (_hostResource == null || _hostResource.Environment == null ||
                _hostResource.Environment.DataListChannel == null || _hostResource.Environment.ExecutionChannel == null)
            {
                Popup.Show("Invalid host resource.");
                return false;
            }

            if (!_hostResource.Environment.IsConnected)
            {
                Popup.Show("Please ensure you are connected .");
                return false;
            }

            ErrorResultTO errors = new ErrorResultTO();
            _transferDataList = _hostResource.Environment.DataListChannel.ReadDatalist(_wizardInvocationTO.TransferDatalistID, errors);

            if (_transferDataList == null)
            {
                Popup.Show("Transfer datalist not found.");
                return false;
            }

            //
            // Register callback
            //
            _hostResource.Environment.ExecutionChannel.AddExecutionStatusCallback(_wizardInvocationTO.ExecutionStatusCallbackID, OnExecutionCallback);

            //
            // Signal browser to navigate
            //
            OnBrowseRequested(_wizardInvocationTO.Endpoint.AbsoluteUri, "", _hostResource.Environment);
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Called when execution callback comes in.
        /// </summary>
        private void OnExecutionCallback(ExecutionStatusCallbackMessage message)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (message.MessageType == ExecutionStatusCallbackMessageType.CompletedCallback)
                {
                    Complete();
                }
                else if (message.MessageType == ExecutionStatusCallbackMessageType.ErrorCallback)
                {
                    throw new Exception("There was an error was on the execution callback for this wizzard workflow.");
                }
            }));
        }

        /// <summary>
        /// Called when an environment connects.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void OnEnvironmentConnected(object payload)
        {
            if (!ShowConnectPrompt)
            {
                return;
            }

            IEnvironmentModel environment = payload as IEnvironmentModel;
            if (environment == null || !EnvironmentModelEqualityComparer.Current.Equals(environment, _hostResource.Environment))
            {
                return;
            }

            //
            // Register callback again
            //
            _hostResource.Environment.ExecutionChannel.AddExecutionStatusCallback(_wizardInvocationTO.ExecutionStatusCallbackID, OnExecutionCallback);

            ShowConnectPrompt = false;
        }

        /// <summary>
        /// Called when an environment connects.
        /// </summary>
        /// <param name="payload">The payload.</param>
        private void OnEnvironmentDisconnected(object payload)
        {
            if (ShowConnectPrompt)
            {
                return;
            }

            IEnvironmentModel environment = payload as IEnvironmentModel;
            if (environment == null || !EnvironmentModelEqualityComparer.Current.Equals(environment, _hostResource.Environment))
            {
                return;
            }

            ShowConnectPrompt = true;
        }

        /// <summary>
        /// Completes this instance.
        /// </summary>
        private void Complete()
        {
            if (_wizardInvocationTO != null && _wizardInvocationTO.CallbackHandler != null)
            {
                _wizardInvocationTO.CallbackHandler.CompleteCallback();
            }

            OnClose();
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            if (_wizardInvocationTO != null && _wizardInvocationTO.CallbackHandler != null)
            {
                _wizardInvocationTO.CallbackHandler.CancelCallback();
            }

            OnClose();
        }

        /// <summary>
        /// Forces a cancel.
        /// </summary>
        private void ForceCancel()
        {
            OnClose();
        }

        /// <summary>
        /// Tries the connect.
        /// </summary>
        private void TryConnect()
        {
            if (_hostResource == null || _hostResource.Environment == null)
            {
                return;
            }

            if (!_hostResource.Environment.IsConnected)
            {
                _hostResource.Environment.Connect();

                if (!_hostResource.Environment.IsConnected)
                {
                    Popup.Show("Couldn't reconnect to environment.");
                }
            }
            else
            {
                OnEnvironmentConnected(_hostResource.Environment);
            }
        }

        #endregion Private Methods

        public void Handle(EnvironmentDisconnectedMessage message)
        {
            OnEnvironmentConnected(message.EnvironmentModel);
        }

        public void Handle(EnvironmentConnectedMessage message)
        {
            OnEnvironmentDisconnected(message.EnvironmentModel);
        }
    }
}
