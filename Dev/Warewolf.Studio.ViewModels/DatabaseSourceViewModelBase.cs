#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public abstract class DatabaseSourceViewModelBase : SourceBaseImpl<IDbSource>, IDatabaseSourceViewModel
    {
        #region Fields
        IManageDatabaseSourceModel _updateManager;
        CancellationTokenSource _token;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        string _testMessage;
        IList<string> _databaseNames;
        IList<ComputerName> _computerNames;
        ComputerName _serverName;
        AuthenticationType _authenticationType;

        string _userName;
        string _password;
        int _connectionTimeout;
        string _databaseName;
        readonly string _warewolfserverName;
        string _resourceName;
        string _headerText;
        string _emptyServerName;
        string _path;
        bool _canSelectWindows;
        bool _canSelectServer;
        bool _canSelectUser;

        #endregion

        #region Commands

        public ICommand TestCommand { get; set; }
        public ICommand OkCommand { get; set; }
        public ICommand CancelTestCommand { get; set; }

        #endregion

        #region Properties
        protected IDbSource DbSource { get; set; }
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
            set
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
                    if (DbSource != null && _authenticationType == AuthenticationType.User && _authenticationType == DbSource.AuthenticationType)
                    {
                        Password = DbSource.Password;
                        UserName = DbSource.UserName;
                    }
                    else
                    {
                        Password = string.Empty;
                        UserName = string.Empty;
                    }
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    DatabaseNames?.Clear();
                    Reset();
                }
            }
        }
        public int ConnectionTimeout
        {
            get { return _connectionTimeout; }
            set
            {
                if (_connectionTimeout != value)
                {
                    _connectionTimeout = value;
                    OnPropertyChanged(() => ConnectionTimeout);
                    OnPropertyChanged(() => Header);
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
                if (!string.IsNullOrEmpty(value))
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
        #endregion

        protected DatabaseSourceViewModelBase(IAsyncWorker asyncWorker, string dbSourceImage)
            : base(dbSourceImage)
        {
            AsyncWorker = asyncWorker;
            InitializeViewModel(dbSourceImage);
        }

        protected DatabaseSourceViewModelBase(IManageDatabaseSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, string dbSourceImage)
            : this(asyncWorker, dbSourceImage)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            PerformInitialise(updateManager, aggregator);
            RequestServiceNameViewModel = requestServiceNameViewModel;
            InitializeViewModel(dbSourceImage);
            GetLoadComputerNamesTask(null);
        }

        void InitializeViewModel(string dbSourceImage)
        {
            CanSelectServer = true;
            ConnectionTimeout = 30;
            CanSelectUser = true;
            CanSelectWindows = true;
            EmptyServerName = "";
            Image = dbSourceImage;
        }

        protected DatabaseSourceViewModelBase(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator, IDbSource dbSource, IAsyncWorker asyncWorker, string dbSourceImage)
            : this(asyncWorker, dbSourceImage)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            PerformInitialise(updateManager, aggregator);
            _warewolfserverName = updateManager.ServerName ?? "";
            AsyncWorker.Start(() => updateManager.FetchDbSource(dbSource.Id), source =>
             {
                 DbSource = source;
                 DbSource.Path = dbSource.Path;
                 Item = ToSourceDefinition();
                 GetLoadComputerNamesTask(() =>
                 {
                     FromModel(DbSource);
                     SetupHeaderTextFromExisting();
                 });
             });

            
        }

        #region Methods

        void PerformInitialise(IManageDatabaseSourceModel updateManager, IEventAggregator aggregator)
        {
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            _updateManager = updateManager;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(TrySaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
            Testing = false;
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
            {
                return false;
            }

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

        void SetupHeaderTextFromExisting()
        {
            HeaderText = (DbSource == null ? ResourceName : DbSource.Name).Trim();

            Header = DbSource == null ? ResourceName : DbSource.Name;
        }

        public override bool CanSave() => TestPassed && !string.IsNullOrEmpty(DatabaseName);

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

        protected abstract IDbSource ToNewDbSource();
        protected abstract IDbSource ToDbSource();
        protected abstract IDbSource ToSourceDefinition();
        public override IDbSource ToModel()
        {
            throw new NotImplementedException("Model not implemented");
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

        void TrySaveConnection()
        {
            Testing = true;
            TestFailed = false;

            if (DbSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    SaveConnection();
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
                DbSource = src;
            }
        }

        void SaveConnection()
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
                    DbSource = src;
                    Path = DbSource.Path;
                    SetupHeaderTextFromExisting();
                    AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                }
            }
        }

        void CancelTest()
        {
            if (_token != null && !_token.IsCancellationRequested && _token.Token.CanBeCanceled)
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
        bool CanCancelTest() => Testing;

        void Reset()
        {
            TestPassed = false;
            TestMessage = "";
            TestFailed = false;
            Testing = false;
            ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
        }

        void GetLoadComputerNamesTask(Action additionalUiAction)
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
            TrySaveConnection();
        }

        void Save(IDbSource toDbSource)
        {
            try
            {
                _updateManager.Save(toDbSource);
                Item = toDbSource;
                SetupHeaderTextFromExisting();
                Reset();
                TestPassed = true;
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
}