using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Email;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Core;
using Warewolf.Studio.Models.Help;

namespace Warewolf.Studio.ViewModels
{
    public class ManageEmailSourceViewModel : SourceBaseImpl<IEmailServiceSource>, IManageEmailSourceViewModel, IDisposable
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
        readonly IEventAggregator _aggregator;
        CancellationTokenSource _token;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _headerText;
        private bool _enableSend;
        readonly string _warewolfserverName;

        private bool _isDisposed;

        public ManageEmailSourceViewModel(IManageEmailSourceModel updateManager, IRequestServiceNameViewModel requestServiceNameViewModel, IEventAggregator aggregator):base(ResourceType.EmailSource)
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _updateManager = updateManager;
            _aggregator = aggregator;
            RequestServiceNameViewModel = requestServiceNameViewModel;
            HeaderText = Resources.Languages.Core.EmailSourceNewHeaderLabel;
            Header = Resources.Languages.Core.EmailSourceNewHeaderLabel;
            SendCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            Testing = false;
            _testPassed = false;
            _testFailed = false;
            EnableSend = false;
            EnableSslNo = true;
            Port = 25;
            Timeout = 100;
            // ReSharper restore MaximumChainedReferences
            _warewolfserverName = updateManager.ServerName;
        }

        public ManageEmailSourceViewModel(IManageEmailSourceModel updateManager, IRequestServiceNameViewModel requestServiceNameViewModel, IEventAggregator aggregator, IEmailServiceSource emailServiceSource)
            : this(updateManager, requestServiceNameViewModel, aggregator)
        {
            VerifyArgument.IsNotNull("emailServiceSource", emailServiceSource);
            _emailServiceSource = emailServiceSource;
            FromSource(emailServiceSource);
            SetupHeaderTextFromExisting();
            
        }

        void FromSource(IEmailServiceSource emailServiceSource)
        {
            HostName = emailServiceSource.HostName;
            UserName = emailServiceSource.UserName;
            Password = emailServiceSource.Password;
            EnableSsl = emailServiceSource.EnableSsl;
            Port = emailServiceSource.Port;
            Timeout = emailServiceSource.Timeout;
            EmailFrom = emailServiceSource.EmailFrom;
            EmailTo = emailServiceSource.EmailTo;
        }
        void SetupHeaderTextFromExisting()
        {
            HeaderText = Resources.Languages.Core.EmailSourceEditHeaderLabel + _warewolfserverName.Trim() + "\\" + (_emailServiceSource.HostName ?? ResourceName).Trim();
            Header = ((_emailServiceSource.HostName ?? ResourceName));
        }

        bool CanSave()
        {
            return TestPassed;
        }

        public bool CanTest()
        {
            if (Testing)
                return false;
            if (String.IsNullOrEmpty(HostName) && String.IsNullOrEmpty(UserName) && String.IsNullOrEmpty(Password))
            {
                return false;
            }
            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var helpDescriptor = new HelpDescriptor("", helpText, null);
            VerifyArgument.IsNotNull("helpDescriptor", helpDescriptor);
            _aggregator.GetEvent<HelpChangedEvent>().Publish(helpDescriptor);

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
                if (!String.IsNullOrEmpty(value))
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
                var res = RequestServiceNameViewModel.ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    ResourceName = RequestServiceNameViewModel.ResourceName.Name;
                    var src = ToSource();
                    src.Path = RequestServiceNameViewModel.ResourceName.Path ?? RequestServiceNameViewModel.ResourceName.Name;
                    Save(src);
                    _emailServiceSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                Save(_emailServiceSource);
            }
        }

        IRequestServiceNameViewModel RequestServiceNameViewModel { get; set; }

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
                    TestMessage = String.Empty;

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
                    TestMessage = String.Empty;

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
                    TestMessage = String.Empty;

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
                    TestMessage = String.Empty;

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
                    TestMessage = String.Empty;

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
                    TestMessage = String.Empty;

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
                    TestMessage = String.Empty;

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
                    TestMessage = String.Empty;

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
            var t = new Task(
                SetupProgressSpinner, _token.Token);

            t.ContinueWith(a => Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                if (!_token.IsCancellationRequested)
                    switch (t.Status)
                    {
                        case TaskStatus.Faulted:
                        {
                            TestFailed = true;
                            TestPassed = false;
                            Testing = false;
                            TestMessage = t.Exception != null ? t.Exception.Message : "Failed";
                            break;
                        }
                        case TaskStatus.RanToCompletion:
                        {
                            TestMessage = "Passed";
                            TestFailed = false;
                            TestPassed = true;
                            Testing = false;
                            break;
                        }
                    }
            }));
            t.Start();
        }

        void SetupProgressSpinner()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
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
                Id = _emailServiceSource == null ? Guid.NewGuid() : _emailServiceSource.Id
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
                    Id = _emailServiceSource == null ? Guid.NewGuid() : _emailServiceSource.Id
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
                Id = Item.Id
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

        [ExcludeFromCodeCoverage]
        public string HostNameLabel
        {
            get
            {
                return Resources.Languages.Core.HostNameLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string UserNameLabel
        {
            get
            {
                return Resources.Languages.Core.UserNameLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string PasswordLabel
        {
            get
            {
                return Resources.Languages.Core.PasswordLabel;

            }
        }

        [ExcludeFromCodeCoverage]
        public string EnableSslLabel
        {
            get
            {
                return Resources.Languages.Core.EmailSourceEnableSslLabel;

            }
        }

        [ExcludeFromCodeCoverage]
        public string PortLabel
        {
            get
            {
                return Resources.Languages.Core.PortLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string TimeoutLabel
        {
            get
            {
                return Resources.Languages.Core.EmailSourceTimeoutLabel;

            }
        }

        [ExcludeFromCodeCoverage]
        public string TestLabel
        {
            get
            {
                return Resources.Languages.Core.TestConnectionLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string EmailFromLabel
        {
            get
            {
                return Resources.Languages.Core.EmailSourceEmailFromLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string EmailToLabel
        {
            get
            {
                return Resources.Languages.Core.EmailSourceEmailToLabel;
            }
        }

        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
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
                    _token.Dispose();
                }

                // Dispose unmanaged resources.
                _isDisposed = true;
            }
        }
    }
}
