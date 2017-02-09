using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ManageEmailSourceViewModel : SourceBaseImpl<IEmailServiceSource>, IManageEmailSourceViewModel
    {
        private string _hostName;
        private string _userName;
        private string _password;
        private int _port;
        private int _timeout;
        private string _testMessage;
        private string _emailFrom;
        private string _emailTo;
        string _resourceName;
        private bool _enableSsl;
        private bool _enableSslYes;
        private bool _enableSslNo;

        private IEmailServiceSource _emailServiceSource;
        private readonly IManageEmailSourceModel _updateManager;
        CancellationTokenSource _token;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _headerText;
        private bool _enableSend;

        private bool _isDisposed;

        public ManageEmailSourceViewModel(IManageEmailSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator)
            : this(updateManager, aggregator)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _updateManager = updateManager;
            RequestServiceNameViewModel = requestServiceNameViewModel;
            HeaderText = Resources.Languages.Core.EmailSourceNewHeaderLabel;
            Header = Resources.Languages.Core.EmailSourceNewHeaderLabel;
            HostName = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
            EnableSend = false;
            EnableSslNo = true;
            Port = 25;
            Timeout = 10000;
        }

        public ManageEmailSourceViewModel(IManageEmailSourceModel updateManager, IEventAggregator aggregator, IEmailServiceSource emailServiceSource,IAsyncWorker asyncWorker)
            : this(updateManager, aggregator)
        {
            VerifyArgument.IsNotNull("emailServiceSource", emailServiceSource);
            asyncWorker.Start(() => updateManager.FetchSource(emailServiceSource.Id), source =>
            {
                _emailServiceSource = source;
                _emailServiceSource.Path = emailServiceSource.Path;
                // ReSharper disable once VirtualMemberCallInContructor
                FromModel(_emailServiceSource);
                SetupHeaderTextFromExisting();
            });
        }

        ManageEmailSourceViewModel(IManageEmailSourceModel updateManager, IEventAggregator aggregator)
            : base("EmailSource")
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;
            SendCommand = new DelegateCommand(o=>TestConnection(), o=>CanTest());
            OkCommand = new DelegateCommand(o=>SaveConnection(), o=>CanSave());
            Testing = false;
            _testPassed = false;
            _testFailed = false;
        }

        public ManageEmailSourceViewModel()
            : base("EmailSource")
        {
   
        }

        public override void FromModel(IEmailServiceSource emailServiceSource)
        {
            if (emailServiceSource != null)
            {
                HostName = emailServiceSource.HostName;
                UserName = emailServiceSource.UserName;
                Password = emailServiceSource.Password;
                EnableSsl = emailServiceSource.EnableSsl;
                if (EnableSsl)
                {
                    EnableSslYes = EnableSsl;
                }
                else
                {
                    EnableSslNo = true;
                }
                Port = emailServiceSource.Port;
                Timeout = emailServiceSource.Timeout;
                EmailFrom = emailServiceSource.EmailFrom;
                EmailTo = emailServiceSource.EmailTo;
                ResourceName = emailServiceSource.ResourceName;
            }
        }

        public override string Name
        {
            get
            {
                return ResourceName;
            }
            set
            {
                ResourceName = value;
            }
        }
        void SetupHeaderTextFromExisting()
        {
            if (_emailServiceSource != null)
            {
                HeaderText = (_emailServiceSource.ResourceName ?? ResourceName).Trim();
                Header = (_emailServiceSource.ResourceName ?? ResourceName).Trim();
            }
        }

        public override bool CanSave()
        {
            return TestPassed;
        }

        public bool CanTest()
        {
            if (Testing)
                return false;
            if (string.IsNullOrEmpty(HostName) && string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password))
            {
                return false;
            }
            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void Save()
        {
            SaveConnection();
        }

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    SetupHeaderTextFromExisting();
                }
                OnPropertyChanged(_resourceName);
            }
        }

        private void SaveConnection()
        {
            if (_emailServiceSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var requestServiceNameViewModel = RequestServiceNameViewModel.Result;
                    var res = requestServiceNameViewModel.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        var src = ToSource();
                        src.ResourceName = requestServiceNameViewModel.ResourceName.Name;
                        src.Path = requestServiceNameViewModel.ResourceName.Path ?? requestServiceNameViewModel.ResourceName.Name;
                        Save(src);
                        if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
                            AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                        Item = src;
                        _emailServiceSource = src;
                        ResourceName = _emailServiceSource.ResourceName;
                        SetupHeaderTextFromExisting();
                    }
                }
            }
            else
            {
                var src = ToSource();
                src.Path = Item.Path??"";
                src.ResourceName = Item.ResourceName;
                Save(src);
                Item = src;
                _emailServiceSource = src;
                SetupHeaderTextFromExisting();
            }
            TestPassed = false;
        }

       public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }

        void Save(IEmailServiceSource source)
        {
            _updateManager.Save(source);
        }

        public string HostName
        {
            get { return _hostName; }
            set
            {
                if (value != _hostName)
                {
                    _hostName = value;
                    TestMessage = string.Empty;

                    OnPropertyChanged(() => HostName);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    EmailFrom = _userName;
                    TestMessage = string.Empty;

                    OnPropertyChanged(() => UserName);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value != _password)
                {
                    _password = value;
                    TestMessage = string.Empty;

                    OnPropertyChanged(() => Password);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public bool EnableSsl
        {
            get { return _enableSsl; }
            set
            {
                if (value != _enableSsl)
                {
                    _enableSsl = value;
                    TestMessage = string.Empty;

                    OnPropertyChanged(() => EnableSsl);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }
        public bool EnableSslYes
        {
            get { return _enableSslYes; }
            set
            {
                _enableSslYes = value;
                if (_enableSslYes)
                {
                    EnableSsl = true;
                }

                OnPropertyChanged(() => EnableSslYes);
                OnPropertyChanged(() => EnableSsl);
                OnPropertyChanged(() => Header);
                TestPassed = false;
                ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }
        public bool EnableSslNo
        {
            get { return _enableSslNo; }
            set
            {
                _enableSslNo = value;
                if (_enableSslNo)
                {
                    EnableSsl = false;
                }

                OnPropertyChanged(() => EnableSslNo);
                OnPropertyChanged(() => EnableSsl);
                OnPropertyChanged(() => Header);
                TestPassed = false;
                ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                if (value != _port)
                {
                    _port = value;
                    TestMessage = string.Empty;

                    if (!_port.ToString().IsNumeric())
                    {
                        OkCommand.CanExecute(false);
                    }

                    OnPropertyChanged(() => Port);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public int Timeout
        {
            get { return _timeout; }
            set
            {
                if (value != _timeout)
                {
                    _timeout = value;
                    TestMessage = string.Empty;

                    if (!_timeout.ToString().IsNumeric())
                    {
                        OkCommand.CanExecute(false);
                    }

                    OnPropertyChanged(() => Timeout);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string EmailFrom
        {
            get { return _emailFrom; }
            set
            {
                if (value != _emailFrom)
                {
                    _emailFrom = value;
                    TestMessage = string.Empty;

                    EnableSend = true;
                    if (!_emailFrom.IsEmail())
                    {
                        EnableSend = false;
                    }
                    if (EmailTo == null || !EmailTo.IsEmail())
                    {
                        EnableSend = false;
                    }

                    OnPropertyChanged(() => EmailFrom);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => EnableSend);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string EmailTo
        {
            get { return _emailTo; }
            set
            {
                if (value != _emailTo)
                {
                    _emailTo = value;
                    TestMessage = string.Empty;

                    EnableSend = true;
                    if (!_emailTo.IsEmail())
                    {
                        EnableSend = false;
                    }
                    if (EmailFrom == null || !EmailFrom.IsEmail())
                    {
                        EnableSend = false;
                    }

                    OnPropertyChanged(() => EmailTo);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => EnableSend);
                    OnPropertyChanged(() => TestMessage);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }

        private void TestConnection()
        {
            _token = new CancellationTokenSource();
            var t = new Task(SetupProgressSpinner, _token.Token);

            t.ContinueWith(a => Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (!_token.IsCancellationRequested)
                    switch (t.Status)
                    {
                        case TaskStatus.Faulted:
                            {
                                TestFailed = true;
                                TestPassed = false;
                                Testing = false;
                                TestMessage = GetExceptionMessage(t.Exception);
                                break;
                            }
                        case TaskStatus.RanToCompletion:
                            {
                                TestMessage = "Passed";
                                TestFailed = false;
                                Testing = false;
                                TestPassed = true;
                                break;
                            }
                    }
            }));
            t.Start();
        }

        void SetupProgressSpinner()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });
            _updateManager.TestConnection(ToNewSource());
        }

        IEmailServiceSource ToNewSource()
        {
            return new EmailServiceSourceDefinition
            {
                HostName = HostName,
                Password = Password,
                UserName = UserName,
                Port = Port,
                Timeout = Timeout,
                EnableSsl = EnableSsl,
                EmailFrom = EmailFrom,
                EmailTo = EmailTo,
                Id = _emailServiceSource?.Id ?? Guid.NewGuid()
            };
        }

        IEmailServiceSource ToSource()
        {
            if (_emailServiceSource == null)
                return new EmailServiceSourceDefinition
                {
                    HostName = HostName,
                    Password = Password,
                    UserName = UserName,
                    Port = Port,
                    Timeout = Timeout,
                    EnableSsl = EnableSsl,
                    EmailFrom = EmailFrom,
                    EmailTo = EmailTo,
                    Id = _emailServiceSource?.Id ?? Guid.NewGuid()
                }
            ;
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                _emailServiceSource.HostName = HostName;
                _emailServiceSource.UserName = UserName;
                _emailServiceSource.Password = Password;
                _emailServiceSource.Port = Port;
                _emailServiceSource.Timeout = Timeout;
                _emailServiceSource.EnableSsl = EnableSsl;
                _emailServiceSource.EmailFrom = EmailFrom;
                _emailServiceSource.EmailTo = EmailTo;
                return _emailServiceSource;

            }
        }

        public override IEmailServiceSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }
            return new EmailServiceSourceDefinition
                {
                    HostName = HostName,
                    Password = Password,
                    UserName = UserName,
                    Port = Port,
                    Timeout = Timeout,
                    EnableSsl = EnableSsl,
                    EmailFrom = EmailFrom,
                    EmailTo = EmailTo,
                    Id = _emailServiceSource?.Id ?? Guid.NewGuid()
                };
        }

        public bool TestFailed
        {
            get
            {
                return _testFailed;
            }
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }
        public bool Testing
        {
            get
            {
                return _testing;
            }
            private set
            {
                _testing = value;
                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(SendCommand);
            }
        }

        public string TestMessage
        {
            get { return _testMessage; }
            // ReSharper disable UnusedMember.Local
            private set
            // ReSharper restore UnusedMember.Local
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        public ICommand SendCommand { get; set; }
        public ICommand OkCommand { get; set; }

        public bool EnableSend
        {
            get { return _enableSend; }
            set
            {
                _enableSend = value;
                OnPropertyChanged(() => EnableSend);
            }
        }

        public string HostNameLabel => Resources.Languages.Core.HostNameLabel;

        public string UserNameLabel => Resources.Languages.Core.UserNameLabel;

        public string PasswordLabel => Resources.Languages.Core.PasswordLabel;

        public string EnableSslLabel => Resources.Languages.Core.EmailSourceEnableSslLabel;

        public string PortLabel => Resources.Languages.Core.PortLabel;

        public string TimeoutLabel => Resources.Languages.Core.EmailSourceTimeoutLabel;

        public string TestLabel => Resources.Languages.Core.TestConnectionLabel;

        public string EmailFromLabel => Resources.Languages.Core.EmailSourceEmailFromLabel;

        public string EmailToLabel => Resources.Languages.Core.EmailSourceEmailToLabel;

        protected override void OnDispose()
        {
            if (RequestServiceNameViewModel != null)
            {
                RequestServiceNameViewModel.Result?.Dispose();
                RequestServiceNameViewModel.Dispose();
            }
            Dispose(true);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _token?.Dispose();
                }

                // Dispose unmanaged resources.
                _isDisposed = true;
            }
        }
    }
}
