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
using Dev2.Common.Interfaces.Core.Database;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Warewolf.Studio.ViewModels
{
    public interface IDatabaseSourceViewModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        AuthenticationType AuthenticationType { get; set; }
        ComputerName ServerName { get; set; }
        string EmptyServerName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        ICommand TestCommand { get; set; }
        ICommand CancelTestCommand { get; set; }
        ICommand OkCommand { get; set; }
        string TestMessage { get; }
        string HeaderText { get; set; }
        bool TestPassed { get; set; }
        bool TestFailed { get; set; }
        bool Testing { get; }
        string ResourceName { get; set; }
        IList<ComputerName> ComputerNames { get; set; }
    }

    public abstract class DatabaseSourceViewModelBase : SourceBaseImpl<IDbSource>, IDatabaseSourceViewModel
    {
        #region Fields
        private IManageDatabaseSourceModel _updateManager;
        private CancellationTokenSource _token;
        private bool _testPassed;
        private bool _testFailed;
        private bool _testing;
        private string _testMessage;
        private IList<string> _databaseNames;
        private IList<ComputerName> _computerNames;
        private ComputerName _serverName;
        private AuthenticationType _authenticationType;
        private IDbSource _dbSource;
        private string _userName;
        private string _password;
        private string _databaseName;
        private readonly string _warewolfserverName;
        private string _resourceName;
        private string _headerText;
        private string _emptyServerName;
        private string _databaseType;
        private string _path;

        #endregion

        #region Commands

        public ICommand TestCommand { get; set; }
        public ICommand OkCommand { get; set; }
        public ICommand CancelTestCommand { get; set; }

        #endregion

        #region Properties

        public IAsyncWorker AsyncWorker { get; set; }
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

        public IList<string> DatabaseNames
        {
            get { return _databaseNames; }
            set
            {
                _databaseNames = value;
                OnPropertyChanged(() => DatabaseNames);
            }
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

        public AuthenticationType AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                if (_authenticationType != value)
                {
                    _authenticationType = value;
                    /* if (_dbSource != null && _authenticationType == AuthenticationType.User && _authenticationType == _dbSource.AuthenticationType)
                     {
                         Password = _dbSource.Password;
                         UserName = _dbSource.UserName;
                     }
                     else
                     {
                         Password = string.Empty;
                         UserName = string.Empty;
                     }*/
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    DatabaseNames?.Clear();
                    Reset();
                }
            }
        }
        public bool UserAuthenticationSelected => AuthenticationType == AuthenticationType.User;
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

        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }

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

        #endregion

        protected DatabaseSourceViewModelBase(IAsyncWorker asyncWorker, string dbSourceImage)
            : base(dbSourceImage)
        {
            AsyncWorker = asyncWorker;
            _databaseType = dbSourceImage;
        }

        protected DatabaseSourceViewModelBase(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, string dbSourceImage)
            : this(asyncWorker, dbSourceImage)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformInitialise(updateManager, aggregator);
            RequestServiceNameViewModel = requestServiceNameViewModel;
            GetLoadComputerNamesTask(null);
        }

        protected DatabaseSourceViewModelBase(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker, string dbSourceImage)
            : this(asyncWorker, dbSourceImage)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            PerformInitialise(updateManager, aggregator);
            _warewolfserverName = updateManager.ServerName ?? "";
            _dbSource = dbSource;
            //Item = new DbSourceDefinition
            //{
            //    AuthenticationType = _dbSource.AuthenticationType,
            //    DbName = _dbSource.DbName,
            //    Id = _dbSource.Id,
            //    Name = _dbSource.Name,
            //    Password = _dbSource.Password,
            //    Path = _dbSource.Path,
            //    ServerName = _dbSource.ServerName,
            //    UserName = _dbSource.UserName,
            //    Type = _dbSource.Type
            //};
            Item = ToSourceDefination();
            switch (_dbSource.Type)
            {
                case enSourceType.SqlDatabase:
                    Image = "SqlDatabase";
                    break;

                case enSourceType.MySqlDatabase:
                    Image = "MySqlDatabase";
                    break;

                case enSourceType.PostgreSQL:
                    Image = "PostgreSQL";
                    break;

                case enSourceType.Oracle:
                    Image = "Oracle";
                    break;

                case enSourceType.ODBC:
                    Image = "ODBC";
                    break;

                default:
                    Image = "DbSource";
                    break;
            }

            GetLoadComputerNamesTask(() =>
            {
                FromModel(_dbSource);
                SetupHeaderTextFromExisting();
            });
        }



        #region Methods

        private void PerformInitialise(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator)
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;

            //HeaderText = Resources.Languages.Core.DatabaseSourceServerNewHeaderLabel;
            //Header = Resources.Languages.Core.DatabaseSourceServerNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
            Testing = false;
            //Types = new List<NameValue>
            //{
            //    new NameValue { Name = "Microsoft SQL Server", Value = enSourceType.SqlDatabase.ToString() },
            //    new NameValue { Name = "MySql Database", Value = enSourceType.MySqlDatabase.ToString() },
            //    new NameValue { Name = "PostgreSQL Database", Value = enSourceType.PostgreSQL.ToString() },
            //    new NameValue { Name = "Oracle Database", Value = enSourceType.Oracle.ToString() },
            //    new NameValue { Name = "ODBC Database", Value = enSourceType.ODBC.ToString() }
            //};
            //ServerType = Types[0];
            _testPassed = false;
            _testFailed = false;
            DatabaseNames = new List<string>();
            ComputerNames = new List<ComputerName>();
        }

        protected void TestConnection()
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
                    TestMessage = GetExceptionMessage(exception);
                    DatabaseNames.Clear();
                });
            }
            catch (Exception exception)
            {
                TestFailed = true;
                TestPassed = false;
                Testing = false;
                TestMessage = GetExceptionMessage(exception);
                DatabaseNames.Clear();
            }
            OnPropertyChanged(() => DatabaseNames);
        }

        public bool CanTest()
        {
            if (Testing)
                return false;
            if (ServerName != null && string.IsNullOrEmpty(ServerName.Name))
            {
                return false;
            }
            if (AuthenticationType == AuthenticationType.User)
            {
                return !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
            }
            return true;
        }

        private void SetupHeaderTextFromExisting()
        {
            if (_warewolfserverName != null)
            {
            }
            HeaderText = (_dbSource == null ? ResourceName : _dbSource.Name).Trim();

            Header = _dbSource == null ? ResourceName : _dbSource.Name;
        }

        public override bool CanSave()
        {
            return TestPassed && !string.IsNullOrEmpty(DatabaseName);
        }

        private IList<string> SetupProgressSpinner()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });
            return _updateManager.TestDbConnection(ToNewDbSource());
        }

        protected abstract IDbSource ToNewDbSource();
        protected abstract IDbSource ToDbSource();
        protected abstract IDbSource ToSourceDefination();
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
                Type = (enSourceType)Enum.Parse(typeof(enSourceType), _databaseType),
                Name = ResourceName,
                DbName = DatabaseName,
                Id = Item.Id
            };
        }

        protected string GetServerName()
        {
            string serverName = null;
            if (ServerName != null)
            {
                serverName = ServerName.Name;
            }
            return serverName;
        }
        private void SaveConnection()
        {
            Testing = true;
            TestFailed = false;
            TestPassed = false;
            if (_dbSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var requestServiceNameViewModel = RequestServiceNameViewModel.Result;
                    var res = requestServiceNameViewModel.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        _resourceName = requestServiceNameViewModel.ResourceName.Name;
                        var src = ToDbSource();

                        src.Path = requestServiceNameViewModel.ResourceName.Path ?? requestServiceNameViewModel.ResourceName.Name;
                        Save(src);
                        if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null && !TestFailed)
                        {
                            _dbSource = src;
                            Path = _dbSource.Path;
                            SetupHeaderTextFromExisting();
                            AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                        }
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

        private void CancelTest()
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
        private bool CanCancelTest()
        {
            return Testing;
        }

        private void Reset()
        {
            TestPassed = false;
            TestMessage = "";
            TestFailed = false;
            Testing = false;
            ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
        }

        private void GetLoadComputerNamesTask(Action additionalUiAction)
        {
            AsyncWorker.Start(() => _updateManager.GetComputerNames().Select(name => new ComputerName { Name = name }).ToList(), names =>
            {
                ComputerNames = names;
                additionalUiAction?.Invoke();
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

        public override void Save()
        {
            SaveConnection();
        }

        private void Save(IDbSource toDbSource)
        {
            try
            {
                _updateManager.Save(toDbSource);
                Item = toDbSource;
                SetupHeaderTextFromExisting();
                Reset();
            }
            catch (Exception ex)
            {
                Reset();
                TestMessage = ex.Message;
                TestFailed = true;
            }
        }
        #endregion


    }

    public class ManageOracleSourceViewModel : DatabaseSourceViewModelBase
    {
        private readonly IOracleSource _oracleSource;
        public ManageOracleSourceViewModel(IAsyncWorker asyncWorker, string dbSourceImage)
            : base(asyncWorker, dbSourceImage)
        {
        }

        public ManageOracleSourceViewModel(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, string dbSourceImage)
            : base(updateManager, requestServiceNameViewModel, aggregator, asyncWorker, dbSourceImage)
        {
        }

        public ManageOracleSourceViewModel(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker, string dbSourceImage)
            : base(updateManager, aggregator, dbSource, asyncWorker, dbSourceImage)
        {
            VerifyArgument.IsNotNull("dbSource", _oracleSource);
            _oracleSource = dbSource as IOracleSource;
        }

        #region Overrides of SourceBaseImpl<IDbSource>

        public override string Name { get; set; }

        public override void FromModel(IDbSource service)
        {
            var oracleSource = (IOracleSource)service;
            ResourceName = oracleSource.Name;
            ServerName = ComputerNames.FirstOrDefault(name => string.Equals(oracleSource.ServerName, name.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ServerName != null)
            {
                EmptyServerName = ServerName.Name ?? oracleSource.ServerName;
            }
            AuthenticationType = service.AuthenticationType;
            UserName = oracleSource.UserName;
            Password = oracleSource.Password;
            Path = oracleSource.Path;
            TestConnection();
            DatabaseName = oracleSource.DbName;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion

        #region Overrides of DatabaseSourceViewModelBase

        protected override IDbSource ToNewDbSource()
        {
            return new OracleSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _oracleSource?.Id ?? Guid.NewGuid()
            };
        }

        protected override IDbSource ToDbSource()
        {
            return _oracleSource == null ? new OracleSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = _oracleSource?.Id ?? SelectedGuid
            } : new OracleSourceDefination
            {
                AuthenticationType = AuthenticationType,
                ServerName = GetServerName(),
                Password = Password,
                UserName = UserName,
                Type = enSourceType.Oracle,
                Path = Path,
                Name = ResourceName,
                DbName = DatabaseName,
                Id = (Guid)_oracleSource?.Id
            };
        }

        protected override IDbSource ToSourceDefination()
        {
            return new DbSourceDefinition
            {
                AuthenticationType = _oracleSource.AuthenticationType,
                DbName = _oracleSource.DbName,
                Id = _oracleSource.Id,
                Name = _oracleSource.Name,
                Password = _oracleSource.Password,
                Path = _oracleSource.Path,
                ServerName = _oracleSource.ServerName,
                UserName = _oracleSource.UserName,
                Type = enSourceType.Oracle
            };
        }

        #endregion
    }
}