/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
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
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using System.Collections.ObjectModel;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Warewolf.Studio.ViewModels
{
    public class ManageDatabaseSourceViewModel : SourceBaseImpl<IDbSource>, IManageDatabaseSourceViewModel
    {
        public IAsyncWorker AsyncWorker { get; set; }
        private NameValue _serverType;
        private AuthenticationType _authenticationType;
        private ComputerName _serverName;
        private string _databaseName;
        private string _userName;
        private string _password;
        private string _testMessage;
        private IList<string> _databaseNames;
        IManageDatabaseSourceModel _updateManager;
        IDbSource _dbSource;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _resourceName;
        CancellationTokenSource _token;
        IList<ComputerName> _computerNames;
        readonly string _warewolfserverName;
        string _headerText;
        private bool _isDisposed;
        string _path;
        string _emptyServerName;
        private bool _canSelectWindows;
        private bool _canSelectServer;
        private bool _canSelectUser;
        public IComputerNameProvider Provider { get; set; }

        public bool CanSelectWindows
        {
            get
            {
                return _canSelectWindows;
            }
            set
            {
                _canSelectWindows = value;
                OnPropertyChanged(() => CanSelectWindows);
            }
        }
        public bool CanSelectUser
        {
            get
            {
                return _canSelectUser;
            }
            set
            {
                _canSelectUser = value;
                OnPropertyChanged(() => CanSelectUser);
            }
        }
        public bool CanSelectServer
        {
            get
            {
                return _canSelectServer;
            }
            set
            {
                _canSelectServer = value;
                OnPropertyChanged(() => CanSelectServer);
            }
        }

        private void PerformInitialise(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator)
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;

            HeaderText = Resources.Languages.Core.DatabaseSourceServerNewHeaderLabel;
            Header = Resources.Languages.Core.DatabaseSourceServerNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
            Testing = false;
            Types = new List<NameValue>
            {
                new NameValue { Name = "Microsoft SQL Server", Value = enSourceType.SqlDatabase.ToString() },
                new NameValue { Name = "MySql Database", Value = enSourceType.MySqlDatabase.ToString() },
                new NameValue { Name = "PostgreSql Database", Value = enSourceType.PostgreSql.ToString() },
                new NameValue { Name = "Oracle Database", Value = enSourceType.Oracle.ToString() },
                new NameValue { Name = "ODBC Database", Value = enSourceType.ODBC.ToString() }
            };
            ServerType = Types[0];
            _testPassed = false;
            _testFailed = false;
            DatabaseNames = new List<string>();
            ComputerNames = new List<ComputerName>();
            Provider = new ComputerNameProvider(new ComputerName[0]);

        }

        void GetLoadComputerNamesTask(Action additionalUiAction)
        {
            AsyncWorker.Start(() => _updateManager.GetComputerNames().Select(name => new ComputerName { Name = name }).ToList(), names =>
            {
                ComputerNames = names;
                Provider.SetNames(ComputerNames);
                if (additionalUiAction != null)
                {
                    additionalUiAction();
                }
            }, exception =>
            {
                TestFailed = true;
                TestPassed = false;
                if (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
                TestMessage = exception.Message;
            });
        }

        bool CanCancelTest()
        {
            return Testing;
        }

        void CancelTest()
        {
            if (_token != null)
            {
                if (!_token.IsCancellationRequested && _token.Token.CanBeCanceled)
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

        public ManageDatabaseSourceViewModel(IAsyncWorker asyncWorker)
            : base(ResourceType.DbSource)
        {
            AsyncWorker = asyncWorker;
        }

        public ManageDatabaseSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker)
            : this(asyncWorker)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformInitialise(updateManager, aggregator);
            RequestServiceNameViewModel = requestServiceNameViewModel;
            GetLoadComputerNamesTask(null);

        }
        public ManageDatabaseSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker)
            : this(asyncWorker)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            PerformInitialise(updateManager, aggregator);
            _warewolfserverName = updateManager.ServerName ?? "";
            _dbSource = dbSource;
            Item = new DbSourceDefinition
            {
                AuthenticationType = _dbSource.AuthenticationType,
                DbName = _dbSource.DbName,
                Id = _dbSource.Id,
                Name = _dbSource.Name,
                Password = _dbSource.Password,
                Path = _dbSource.Path,
                ServerName = _dbSource.ServerName,
                UserName = _dbSource.UserName,
                Type = _dbSource.Type
            };

            GetLoadComputerNamesTask(() =>
            {
                FromModel(_dbSource);
                SetupHeaderTextFromExisting();
            });

        }

        void SetupHeaderTextFromExisting()
        {
            if (_warewolfserverName != null)
            {
            }
            HeaderText = (_dbSource == null ? ResourceName : _dbSource.Name).Trim();

            Header = _dbSource == null ? ResourceName : _dbSource.Name;
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

        public override bool CanSave()
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

        public override void FromModel(IDbSource source)
        {
            ResourceName = source.Name;
            AuthenticationType = source.AuthenticationType;
            UserName = source.UserName;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(source.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? source.ServerName;
            }
            Password = source.Password;
            Path = source.Path;
            ServerType = Types.FirstOrDefault(value => value.Value == source.Type.ToString());
            TestConnection();
            DatabaseName = source.DbName;
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

        public bool UserAuthenticationSelected
        {
            get { return AuthenticationType == AuthenticationType.User; }
        }
        public IList<ComputerName> ComputerNames
        {
            get
            {
                return _computerNames;
            }
            set
            {
                _computerNames = value;
                OnPropertyChanged(() => ComputerNames);
            }
        }

        void SaveConnection()
        {
            if (_dbSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {

                    var res = RequestServiceNameViewModel.Result.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        _resourceName = RequestServiceNameViewModel.Result.ResourceName.Name;
                        var src = ToDbSource();

                        src.Path = RequestServiceNameViewModel.Result.ResourceName.Path ?? RequestServiceNameViewModel.Result.ResourceName.Name;
                        Save(src);
                        _dbSource = src;
                        Path = _dbSource.Path;
                        SetupHeaderTextFromExisting();
                    }
                }
                else
                {
                    throw RequestServiceNameViewModel.Exception;
                }
            }
            else
            {
                var src = ToDbSource();
                Save(src);
                _dbSource = src;
            }
        }
        void Reset()
        {
            TestPassed = false;
            TestMessage = "";
            TestFailed = false;
            Testing = false;
            ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
        }

        void Save(IDbSource toDbSource)
        {
            try
            {
                _updateManager.Save(toDbSource);
                Item = toDbSource;
                SetupHeaderTextFromExisting();
            }
            catch (Exception ex)
            {
                TestMessage = ex.Message;
                TestFailed = true;
                TestPassed = false;
            }
        }

        void TestConnection()
        {
            try
            {
                _token = new CancellationTokenSource();
                AsyncWorker.Start(SetupProgressSpinner, a =>
                {
                    DatabaseNames = a;
                    TestMessage = "Passed";
                    TestFailed = false;
                    TestPassed = true;
                    Testing = false;
                },
                _token, exception =>
                {
                    TestFailed = true;
                    TestPassed = false;
                    Testing = false;
                    TestMessage = exception != null ? exception.Message : "Failed";
                    DatabaseNames.Clear();
                });
            }
            catch (Exception exception)
            {
                TestFailed = true;
                TestPassed = false;
                Testing = false;
                TestMessage = exception.Message;
                DatabaseNames.Clear();
            }
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
                Type = (enSourceType)Enum.Parse(typeof(enSourceType), ServerType.Value),
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _dbSource == null ? Guid.NewGuid() : _dbSource.Id
            };
        }

        private string GetServerName()
        {
            string serverName = null;
            if (ServerName != null)
            {
                serverName = ServerName.Name;
            }
            return serverName;
        }

        IDbSource ToDbSource()
        {
            if (_dbSource == null)
                return new DbSourceDefinition
                {
                    AuthenticationType = AuthenticationType,
                    ServerName = GetServerName(),
                    Password = Password,
                    UserName = UserName,
                    Type = (enSourceType)Enum.Parse(typeof(enSourceType), ServerType.Value),
                    Path = Path,
                    Name = ResourceName,
                    DbName = DatabaseName,
                    Id = _dbSource == null ? SelectedGuid : _dbSource.Id
                };
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                return new DbSourceDefinition
                {
                    AuthenticationType = AuthenticationType,
                    ServerName = GetServerName(),
                    Password = Password,
                    UserName = UserName,
                    Type = (enSourceType)Enum.Parse(typeof(enSourceType), ServerType.Value),
                    Path = Path,
                    Name = ResourceName,
                    DbName = DatabaseName,
                    Id = _dbSource == null ? Guid.NewGuid() : _dbSource.Id
                };

            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }

        public override IDbSource ToModel()
        {
            if (Item == null)
            {
                Item = ToDbSource();
                return Item;
            }

            return new DbSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = (enSourceType)Enum.Parse(typeof(enSourceType), ServerType.Value),
                Name = ResourceName,
                DbName = DatabaseName,
                Id = Item.Id
            };
        }

        public override void Save()
        {
            SaveConnection();
        }

        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }

        public IList<NameValue> Types { get; set; }

        public NameValue ServerType
        {
            get { return _serverType; }
            set
            {
                _serverType = value;
                OnPropertyChanged(() => ServerType);
                OnPropertyChanged(() => Header);
                if (ServerType.Value == enSourceType.ODBC.ToString())
                {
                    CanSelectUser = false;
                    CanSelectWindows = true;
                    CanSelectServer = false;
                    ServerName.Name = "Localhost";
                    EmptyServerName = "Localhost";
                    AuthenticationType = AuthenticationType.Windows;
                }
                else if (ServerType.Value == enSourceType.Oracle.ToString())
                {
                    CanSelectWindows = false;
                    CanSelectServer = true;
                    ServerName.Name = "";
                    AuthenticationType = AuthenticationType.User;
                    CanSelectUser = true;
                }
                else
                {
                    CanSelectWindows = ServerType.Value != enSourceType.PostgreSql.ToString();
                    CanSelectUser = ServerType.Value != enSourceType.PostgreSql.ToString();
                    CanSelectServer = true;
                    ServerName.Name = "";
                }


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
                    if (_dbSource != null && _authenticationType == AuthenticationType.User && _authenticationType == _dbSource.AuthenticationType)
                    {
                        Password = _dbSource.Password;
                        UserName = _dbSource.UserName;
                    }
                    else
                    {
                        Password = String.Empty;
                        UserName = String.Empty;
                    }
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    if (DatabaseNames != null)
                    {
                        DatabaseNames.Clear();
                    }
                    Reset();
                }
            }
        }

        public ComputerName ServerName
        {
            get { return _serverName ?? new ComputerName(); }
            set
            {
                if (value != null && value != _serverName)
                {
                    _serverName = value;
                    OnPropertyChanged(() => ServerName);
                    OnPropertyChanged(() => Header);
                    Reset();
                }
            }
        }

        public string EmptyServerName
        {
            get { return _emptyServerName; }
            set
            {
                if (value != _emptyServerName)
                {
                    _emptyServerName = value;
                    if (_serverName == null || _serverName.Name != _emptyServerName)
                    {
                        _serverName = null;
                        _serverName = new ComputerName { Name = _emptyServerName };
                    }
                    if (string.IsNullOrWhiteSpace(_emptyServerName))
                    {
                        _serverName = null;
                    }
                    OnPropertyChanged(() => EmptyServerName);
                    OnPropertyChanged(() => Header);
                    Reset();
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
                Reset();
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
                Reset();
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
                OnPropertyChanged(() => TestPassed);
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
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
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

        public IList<string> DatabaseNames
        {
            get { return _databaseNames; }
            set
            {
                _databaseNames = value;
                OnPropertyChanged(() => DatabaseNames);
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        public bool IsEmpty { get { return ServerName != null && String.IsNullOrEmpty(ServerName.Name) && AuthenticationType == AuthenticationType.Windows && String.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password); } }
        public IDbSource DBSource
        {
            get
            {
                return _dbSource;
            }
            set
            {
                _dbSource = value;
            }
        }



        protected override void OnDispose()
        {
            if (RequestServiceNameViewModel != null)
            {
                if (RequestServiceNameViewModel.Result != null) RequestServiceNameViewModel.Result.Dispose();
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
                    if (_token != null) _token.Dispose();
                }

                // Dispose unmanaged resources.
                _isDisposed = true;
            }
        }
    }

    public class ComputerNameProvider : ISuggestionProvider, IComputerNameProvider
    {

        IList<ComputerName> _names;

        public ComputerNameProvider(IList<ComputerName> names)
        {
            _names = names;
        }

        public System.Collections.IEnumerable GetSuggestions(string filter)
        {
            return _names.Where(a => a.Name.ToUpper().Contains(filter.ToUpper()));
        }

        public void SetNames(IList<ComputerName> computerNames)
        {
            _names = computerNames;
        }

        #region Implementation of ISuggestionProvider

        public IEnumerable<string> GetSuggestions(string filter, int caretPosition, bool tokenise, enIntellisensePartType type)
        {
            yield break;
        }

        public ObservableCollection<string> VariableList { get; set; }

        #endregion
    }

}
