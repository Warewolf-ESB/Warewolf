using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Core;
using Warewolf.Studio.Models.Help;

namespace Warewolf.Studio.ViewModels
{
    public class ManageDatabaseSourceViewModel : BindableBase, IManageDatabaseSourceViewModel, IDockViewModel,IDisposable
    {
        private enSourceType _serverType;
        private AuthenticationType _authenticationType;
        private IComputerName _serverName;
        private string _databaseName;
        private string _userName;
        private string _password;
        private string _testMessage;
        private IList<string> _databaseNames;
        private string _header;
        IManageDatabaseSourceModel _updateManager ;
        IEventAggregator _aggregator;
         IDbSource _dbSource;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _resourceName;
        CancellationTokenSource _token;
        IList<IComputerName> _computerNames;
        readonly string _warewolfserverName;
        string _headerText;
        private bool _isDisposed;

        public ManageDatabaseSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator)
        {
            PerformInitialise(updateManager, aggregator);
            // ReSharper disable MaximumChainedReferences
            new Task(()=>
            {
                var names = _updateManager.GetComputerNames().Select(name=>new ComputerName{Name=name} as IComputerName).ToList();
                Dispatcher.CurrentDispatcher.Invoke(() => ComputerNames = names);
            }).Start();
            // ReSharper restore MaximumChainedReferences
            _warewolfserverName = updateManager.ServerName;
        }

        private void PerformInitialise(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator)
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;
            _aggregator = aggregator;

            HeaderText = Resources.Languages.Core.DatabaseSourceServerNewHeaderLabel;
                // "New Database Connector Source Server DatabaseSourceServerHeaderLabel";
            Header = Resources.Languages.Core.DatabaseSourceServerNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
            Testing = false;
            Types = new List<enSourceType> {enSourceType.SqlDatabase};
            ServerType = enSourceType.SqlDatabase;
            _testPassed = false;
            _testFailed = false;
            DatabaseNames = new List<string>();
            ComputerNames = new List<IComputerName>();
        }

        bool CanCancelTest()
        {
            return Testing;
        }

        void CancelTest()
        {
            if(_token != null)
            {
                if(!_token.IsCancellationRequested && _token.Token.CanBeCanceled)
                {
                    _token.Cancel();
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        Testing = false;
                        TestFailed = true;
                        TestPassed = false;
                        TestMessage = "Test Cancelled";
                    });
                }
            }
        }

        public ManageDatabaseSourceViewModel(IManageDatabaseSourceModel updateManager, IRequestServiceNameViewModel requestServiceNameViewModel, IEventAggregator aggregator)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformInitialise(updateManager, aggregator);
            RequestServiceNameViewModel = requestServiceNameViewModel;

        }
        public ManageDatabaseSourceViewModel(IManageDatabaseSourceModel updateManager,  IEventAggregator aggregator, IDbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            PerformInitialise(updateManager, aggregator);
            _warewolfserverName = "localhost";
            _dbSource = dbSource;
            var task = new Task(() =>
            {
                var names = _updateManager.GetComputerNames().Select(name => new ComputerName { Name = name } as IComputerName).ToList();
                Dispatcher.CurrentDispatcher.Invoke(() => ComputerNames = names);               
            });
            task.ContinueWith(task1 =>
            {
                FromDbSource(dbSource);
                SetupHeaderTextFromExisting();
            });
            task.Start();
            
            
        }

        void SetupHeaderTextFromExisting()
        {
            HeaderText = Resources.Languages.Core.DatabaseSourceServerEditHeaderLabel + _warewolfserverName.Trim() + "\\" + (_dbSource == null ? ResourceName : _dbSource.Name).Trim();
            Header = ((_dbSource == null ? ResourceName : _dbSource.Name));
        }

        bool CanSave()
        {
            return TestPassed && !String.IsNullOrEmpty(DatabaseName);
        }

        public bool CanTest()
        {
            if (Testing)
                return false;
            if (ServerName != null && String.IsNullOrEmpty(ServerName.Name))
            {
                return false;
            }
            if (AuthenticationType == AuthenticationType.User)
            {
                return !String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(Password);
            }
            return true;
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var helpDescriptor = new HelpDescriptor("",helpText,null);
            VerifyArgument.IsNotNull("helpDescriptor", helpDescriptor);
            _aggregator.GetEvent<HelpChangedEvent>().Publish(helpDescriptor);

        }

        void FromDbSource(IDbSource dbSource)
        {
            ResourceName = dbSource.Name;
            AuthenticationType = dbSource.AuthenticationType;
            UserName = dbSource.UserName;
            ServerName = ComputerNames.FirstOrDefault(name => dbSource.ServerName==name.Name);
            Password = dbSource.Password;
            TestConnection();
            DatabaseName = dbSource.DbName;
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
                if(!String.IsNullOrEmpty(value))
                {
                    SetupHeaderTextFromExisting();
                }
                OnPropertyChanged(_resourceName);
            }
        }

        public bool UserAuthenticationSelected
        {
            get { return AuthenticationType==AuthenticationType.User; }            
        }
        public IList<IComputerName> ComputerNames
        {
            get
            {
                return _computerNames;
            }
            set
            {
                _computerNames = value;
                OnPropertyChanged(()=>ComputerNames);
            }
        }

        void SaveConnection()
        {
            if(_dbSource == null)
            {
                var res = RequestServiceNameViewModel.ShowSaveDialog();
               
                if(res==MessageBoxResult.OK)
                {
                    ResourceName = RequestServiceNameViewModel.ResourceName.Name;
                    var src = ToDbSource();
                    src.Path = RequestServiceNameViewModel.ResourceName.Path ?? RequestServiceNameViewModel.ResourceName.Name;
                    Save(src);
                    _dbSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                Save(ToDbSource());
            }
        }

        void Save(IDbSource toDbSource)
        {
            _updateManager.Save(toDbSource);
           
        }

 

        void TestConnection()
        {

          _token = new CancellationTokenSource();
                var t = new Task<IList<string>> (
                    SetupProgressSpinner,_token.Token);
            
                t.ContinueWith(a=> Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    if(!_token.IsCancellationRequested)
                    switch (t.Status)
                    {
                        case TaskStatus.Faulted:
                        {
                            TestFailed = true;
                            TestPassed = false;
                            Testing = false;
                            TestMessage = t.Exception != null ? t.Exception.Message : "Failed";
                            DatabaseNames.Clear();
                            break;
                        }
                        case TaskStatus.RanToCompletion:
                        {
                            DatabaseNames = t.Result;
                            TestMessage = "Passed";
                            TestFailed = false;
                            TestPassed = true;
                            Testing = false;
                            break;
                        }
                    }
                }));
               t.Start();
                

            OnPropertyChanged(() => DatabaseNames);
        }

        IList<string> SetupProgressSpinner()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });
            return _updateManager.TestDbConnection(ToNewDbSource());
        }

        IDbSource ToNewDbSource()
        {
            return new DbSourceDefinition
                {
                    AuthenticationType = AuthenticationType,
                    ServerName = GetServerName(),
                    Password = Password,
                    UserName = UserName,
                    Type = ServerType,
                    Name = ResourceName,
                    DbName = DatabaseName,
                    Id = _dbSource == null ? Guid.NewGuid() : _dbSource.Id
                };
        }

        private string GetServerName()
        {
            var serverName = "";
            if (ServerName != null)
            {
                serverName = ServerName.Name;
            }
            return serverName;
        }

        IDbSource ToDbSource()
        {
            if(_dbSource == null)
            return new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName =  UserName ,
                Type = ServerType,
                Name = ResourceName,
                DbName = DatabaseName,
                Id =  _dbSource==null?Guid.NewGuid():_dbSource.Id
            };
                // ReSharper disable once RedundantIfElseBlock
            else
            {
                _dbSource.AuthenticationType = AuthenticationType;
                _dbSource.DbName = DatabaseName;
                _dbSource.Password = Password;
                _dbSource.ServerName = GetServerName();
                _dbSource.UserName = UserName;
                return _dbSource;

            }
        }
        IRequestServiceNameViewModel RequestServiceNameViewModel { get; set; }
        bool Haschanged
        {
            get { return !ToNewDbSource().Equals(_dbSource) ; }
        }

        public IList<enSourceType> Types { get; set; }

        public enSourceType ServerType
        {
            get { return _serverType; }
            set
            {
                _serverType = value;
                OnPropertyChanged(() => ServerType);
                OnPropertyChanged(()=>Header);
            }
        }

        public AuthenticationType AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                if (_authenticationType != value)
                {
                    _authenticationType = value;
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public IComputerName ServerName
        {
            get { return _serverName?? new ComputerName(); }
            set
            {
                if (value != _serverName)
                {
                    _serverName = value;
                    OnPropertyChanged(() => ServerName);
                    OnPropertyChanged(() => Header);
                    TestPassed = false;
                    ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                }
            }
        }

        public string DatabaseName
        {
            get { return _databaseName; }
            set
            {
             
                _databaseName = value;
                OnPropertyChanged(() => DatabaseName);
                OnPropertyChanged(() => Header);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(() => UserName);
                OnPropertyChanged(() => Header);
                TestPassed = false;
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(() => Password);
                OnPropertyChanged(() => Header);
                TestPassed = false;
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }

        public ICommand TestCommand { get; set; }

        public ICommand CancelTestCommand { get; set; }

        public string TestMessage
        {
            get { return _testMessage; }
            // ReSharper disable UnusedMember.Local
            private set
                // ReSharper restore UnusedMember.Local
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(()=>TestPassed);
            }
        }


        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged(()=>TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
           
            }
        
 
        }

        [ExcludeFromCodeCoverage]
        public string ServerTypeLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceTypeLabel;
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
        public string AuthenticationLabel
        {
            get
            {
                return Resources.Languages.Core.AuthenticationTypeLabel;
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
        public string TestLabel
        {
            get
            {
                return Resources.Languages.Core.TestConnectionLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string CancelTestLabel
        {
            get
            {
                return Resources.Languages.Core.CancelTest;
            }
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
                OnPropertyChanged(()=>TestFailed);
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
               
                OnPropertyChanged(()=>Testing);
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            }
        }

        [ExcludeFromCodeCoverage]
        public string ServerLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceServerLabel;
            }
        }

        [ExcludeFromCodeCoverage]
        public string DatabaseLabel
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceDatabaseLabel;
            }
        }

        public ICommand OkCommand { get; set; }

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

        [ExcludeFromCodeCoverage]
        public string WindowsAuthenticationToolTip
        {
            get
            {
                return Resources.Languages.Core.WindowsAuthenticationToolTip;
            }
        }

        [ExcludeFromCodeCoverage]
        public string UserAuthenticationToolTip
        {
            get
            {
                return Resources.Languages.Core.UserAuthenticationToolTip;
            }
        }

        [ExcludeFromCodeCoverage]
        public string ServerTypeTool
        {
            get
            {
                return Resources.Languages.Core.DatabaseSourceTypeToolTip;
            }
        }

        public IList<string> DatabaseNames
        {
            get { return _databaseNames; }
            set
            {
                _databaseNames = value;
                OnPropertyChanged(() => DatabaseNames);
            }
        }

        public bool IsActive { get; set; }

        public event EventHandler IsActiveChanged;

        public string Header
        {
            get
            {
                return _header + ((_dbSource!= null )&&Haschanged || (_dbSource == null && !IsEmpty) ? " *" : "");
            }
            set
            {
                _header = value;
                OnPropertyChanged(() => Header);
            }
        }
        public bool IsEmpty { get { return ServerName != null && (String.IsNullOrEmpty(ServerName.Name) && AuthenticationType == AuthenticationType.Windows && String.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password)); } }

        public ResourceType? Image
        {
            get { return ResourceType.DbSource; }
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

    public class ComputerName : IComputerName
    {
        public string Name { get; set; }
    }
}