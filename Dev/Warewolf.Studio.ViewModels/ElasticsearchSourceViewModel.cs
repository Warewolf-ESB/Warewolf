/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Warewolf.Configuration;

namespace Warewolf.Studio.ViewModels
{
    public class ElasticsearchSourceViewModel : SourceBaseImpl<IElasticsearchSourceDefinition>,        IElasticsearchSourceViewModel
    {
        string _headerText;
        string _hostName;
        string _port;
        string _password;
        private string _username;
        bool _testPassed;
        bool _testFailed;
        string _testMessage;
        bool _testing;
        string _resourceName;
        string _searchIndex;

        AuthenticationType _authenticationType;
        IElasticsearchSourceDefinition _elasticsearchServiceSource;
        readonly IElasticsearchSourceModel _elasticsearchSourceModel;
        CancellationTokenSource _token;
        private IServer _currentEnvironment;

        public IAsyncWorker AsyncWorker { get; set; }
        public IExternalProcessExecutor Executor { get; set; }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, Task<IRequestServiceNameViewModel> requestServiceNameViewModel,IServer currentEnvironment)
            : this(elasticsearchSourceModel)
        {
            VerifyArgument.IsNotNull(nameof(requestServiceNameViewModel), requestServiceNameViewModel);
            CurrentEnvironment = currentEnvironment ?? throw new ArgumentNullException(nameof(currentEnvironment));
            _elasticsearchSourceModel = elasticsearchSourceModel;
            RequestServiceNameViewModel = requestServiceNameViewModel;

            HeaderText = Resources.Languages.Core.ElasticsearchNewHeaderLabel;
            Header = Resources.Languages.Core.ElasticsearchNewHeaderLabel;
            HostName = string.Empty;
            Port = "9200";
            Username = string.Empty;
            Password = string.Empty;
            SearchIndex = string.Empty;
        }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, IElasticsearchSourceDefinition elasticsearchServiceSource, IAsyncWorker asyncWorker,IServer currentEnvironment)
            : this(elasticsearchSourceModel)
        {
            VerifyArgument.IsNotNull(nameof(elasticsearchServiceSource), elasticsearchServiceSource);
            CurrentEnvironment = currentEnvironment ?? throw new ArgumentNullException(nameof(currentEnvironment));
            asyncWorker.Start(() => elasticsearchSourceModel.FetchSource(elasticsearchServiceSource.Id), source =>
            {
                _elasticsearchServiceSource = source;
                _elasticsearchServiceSource.Path = elasticsearchServiceSource.Path;
                SetupHeaderTextFromExisting();
                ToItem();
                FromModel(elasticsearchServiceSource);
            });
        }

        ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel)
            : base("ElasticsearchSource")
        {
            VerifyArgument.IsNotNull(nameof(elasticsearchSourceModel), elasticsearchSourceModel);

            _elasticsearchSourceModel = elasticsearchSourceModel;

            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);

            Testing = false;
            TestPassed = false;
            TestFailed = false;
            TestMessage = "";
        }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor,IServer currentEnvironment)
            : base("ElasticsearchSource")
        {
            VerifyArgument.IsNotNull(nameof(executor), executor);
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            VerifyArgument.IsNotNull(nameof(elasticsearchSourceModel), elasticsearchSourceModel);
            VerifyArgument.IsNotNull(nameof(aggregator), aggregator);
            CurrentEnvironment = currentEnvironment ?? throw new ArgumentNullException(nameof(currentEnvironment));
            AsyncWorker = asyncWorker;
            Executor = executor;
            _elasticsearchSourceModel = elasticsearchSourceModel;
            _authenticationType = AuthenticationType.Anonymous;
            _hostName = string.Empty;
            _port = "9200";
            _password = string.Empty;
            _searchIndex = string.Empty;
            _username = string.Empty;
            HeaderText = Resources.Languages.Core.ElasticsearchNewHeaderLabel;
            Header = Resources.Languages.Core.ElasticsearchNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
        }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor,IServer currentEnvironment )
            : this(elasticsearchSourceModel, aggregator, asyncWorker, executor,currentEnvironment)
        {
            VerifyArgument.IsNotNull(nameof(requestServiceNameViewModel), requestServiceNameViewModel);
            RequestServiceNameViewModel = requestServiceNameViewModel;
        }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, IEventAggregator aggregator, IElasticsearchSourceDefinition elasticsearchServiceSource, IAsyncWorker asyncWorker, IExternalProcessExecutor executor,IServer currentEnvironment )
            : this(elasticsearchSourceModel, aggregator, asyncWorker, executor,currentEnvironment)
        {
            VerifyArgument.IsNotNull(nameof(elasticsearchServiceSource), elasticsearchServiceSource);

            AsyncWorker.Start(() => elasticsearchSourceModel.FetchSource(elasticsearchServiceSource.Id), source =>
            {
                _elasticsearchServiceSource = source;
                _elasticsearchServiceSource.Path = elasticsearchServiceSource.Path;

                FromModel(_elasticsearchServiceSource);
                Item = ToSource();
                SetupHeaderTextFromExisting();
            });
        }

        [ExcludeFromCodeCoverage]
        public ElasticsearchSourceViewModel()
            : base("ElasticsearchSource")
        {

        }

        void ToItem()
        {
            Item = new ElasticsearchSourceDefinition
            {
                HostName = _elasticsearchServiceSource.HostName,
                SearchIndex =  _elasticsearchServiceSource.SearchIndex,
                Password = _elasticsearchServiceSource.Password,
                Username = _elasticsearchServiceSource.Username,
                Port = _elasticsearchServiceSource.Port,
                Name = _elasticsearchServiceSource.Name,
                Id = _elasticsearchServiceSource.Id,
                AuthenticationType = _elasticsearchServiceSource.AuthenticationType,
                Path = _elasticsearchServiceSource.Path,
            };
        }

        public override string Name
        {
            get => ResourceName;
            set => ResourceName = value;
        }

        public string HeaderText
        {
            get => _headerText;
            set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }
        public Task<IRequestServiceNameViewModel> RequestServiceNameViewModel { get; set; }
        public string HostName
        {
            get => _hostName;
            set
            {
                if (value != _hostName)
                {
                    _hostName = value;
                    OnPropertyChanged(() => HostName);
                    ResetTestValue();
                }
            }
        }
        public string Port
        {
            get => _port;
            set
            {
                if (value != _port)
                {
                    _port = value;
                    OnPropertyChanged(() => Port);
                    ResetTestValue();
                }
            }
        }
        public bool PasswordSelected => AuthenticationType == AuthenticationType.Password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(() => Password);
                ResetTestValue();
            }
        }
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(() => Username);
                ResetTestValue();
            }
        }

        public string SearchIndex
        {
            get => _searchIndex;
            set
            {
                _searchIndex = value;
                OnPropertyChanged(() => SearchIndex);
                ResetTestValue();
            }
        }

        public AuthenticationType AuthenticationType
        {
            get => _authenticationType;
            set
            {
                if (_authenticationType != value)
                {
                    _authenticationType = value;
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => PasswordSelected);
                    ResetTestValue();
                }
            }
        }
        public ICommand TestCommand { get; set; }
        public ICommand CancelTestCommand { get; set; }
        public ICommand OkCommand { get; set; }
        public bool TestPassed
        {
            get => _testPassed;
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
            }
        }
        public bool TestFailed
        {
            get => _testFailed;
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }
        public string TestMessage
        {
            get => _testMessage;
            set
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }
        public bool Testing
        {
            get { return _testing; }
            set
            {
                _testing = value;
                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
            }
        }
        public string ResourceName
        {
            get => _resourceName;
            set
            {
                _resourceName = value;
                OnPropertyChanged(_resourceName);
            }
        }

        void ResetTestValue()
        {
            TestMessage = "";
            TestFailed = false;
            Testing = false;
            OnPropertyChanged(() => Header);
            ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
        }

        void SetupHeaderTextFromExisting()
        {
            if (_elasticsearchServiceSource != null)
            {
                var headerText = _elasticsearchServiceSource.Name ?? Name;
                HeaderText = Header = !string.IsNullOrWhiteSpace(headerText) ? headerText.Trim() : "";
            }
        }

        public override bool CanSave() => !string.IsNullOrWhiteSpace(HostName) && !string.IsNullOrWhiteSpace(Port);
        public IServer CurrentEnvironment
        {
            private get => _currentEnvironment;
            set
            {
                _currentEnvironment = value;
            }
        }
        void SaveConnection()
        {
            if (_elasticsearchServiceSource == null)
            {
                RequestServiceNameViewModel.Wait();
                if (RequestServiceNameViewModel.Exception == null)
                {
                    var requestServiceNameViewModel = RequestServiceNameViewModel.Result;
                    var res = requestServiceNameViewModel.ShowSaveDialog();

                    if (res == MessageBoxResult.OK)
                    {
                        ResourceName = requestServiceNameViewModel.ResourceName.Name;
                        var src = ToSource();
                        src.Path = requestServiceNameViewModel.ResourceName.Path ?? requestServiceNameViewModel.ResourceName.Name;
                        Save(src);
                        AfterSave(requestServiceNameViewModel, src);
                        Item = src;
                        _elasticsearchServiceSource = src;
                        SetupHeaderTextFromExisting();
                    }
                }
            }
            else
            {
                var src = ToSource();
                var auditingSettingsData = CurrentEnvironment.ResourceRepository.GetAuditingSettings<AuditingSettingsData>(CurrentEnvironment);
                if (src.Id == auditingSettingsData.LoggingDataSource.Value)
                {
                    var popupController = CustomContainer.Get<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
                    var result = popupController.ShowLoggerSourceChange(src.Name);
                    if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                Save(src);
                Item = src;
                _elasticsearchServiceSource = src;
                SetupHeaderTextFromExisting();
            }
        }
        void AfterSave(IRequestServiceNameViewModel requestServiceNameViewModel, IElasticsearchSourceDefinition source)
        {
            if (requestServiceNameViewModel.SingleEnvironmentExplorerViewModel != null)
            {
                AfterSave(requestServiceNameViewModel.SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, source.Id);
            }
        }
        bool CanCancelTest() => Testing;

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

        public bool CanTest()
        {
            if (Testing)
            {
                return false;
            }

            if (string.IsNullOrEmpty(HostName))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Port))
            {
                return false;
            }
            return true;
        }

        void TestConnection()
        {
            _token = new CancellationTokenSource();
            AsyncWorker.Start(SetupProgressSpinner, () =>
                {
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
                });
        }

        void SetupProgressSpinner()
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Testing = true;
                    TestFailed = false;
                    TestPassed = false;
                }), DispatcherPriority.Background);
            }
            _elasticsearchSourceModel.TestConnection(ToNewSource());
        }

        IElasticsearchSourceDefinition ToNewSource() => new ElasticsearchSourceDefinition
        {
            HostName = HostName,
            Password = Password,
            Username = Username,
            Name = ResourceName,
            AuthenticationType = AuthenticationType,
            Port = Port,
            SearchIndex = SearchIndex,
            Id = _elasticsearchServiceSource?.Id ?? Guid.NewGuid()
        };

        public override void FromModel(IElasticsearchSourceDefinition source)
        {
            ResourceName = source.Name;
            HostName = source.HostName;
            Port = source.Port;
            AuthenticationType = source.AuthenticationType;
            Password = source.Password;
            Username = source.Username;
            SearchIndex = source.SearchIndex;
        }

        void Save(IElasticsearchSourceDefinition source) => _elasticsearchSourceModel.Save(source);

        public override void Save() => SaveConnection();

        IElasticsearchSourceDefinition ToSource()
        {
            if (_elasticsearchServiceSource == null)
            {
                return ToNewSource();
            }
            else
            {
                _elasticsearchServiceSource.HostName = HostName;
                _elasticsearchServiceSource.Port = Port;
                _elasticsearchServiceSource.Password = Password;
                _elasticsearchServiceSource.Username = Username;
                _elasticsearchServiceSource.AuthenticationType = AuthenticationType;
                _elasticsearchServiceSource.SearchIndex = SearchIndex;
                return _elasticsearchServiceSource;
            }
        }

        public override IElasticsearchSourceDefinition ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            return new ElasticsearchSourceDefinition
            {
                Name = Item.Name,
                HostName = HostName,
                Port = Port,
                AuthenticationType = AuthenticationType,
                Password = Password,
                Username = Username,
                Id = Item.Id,
                Path = Item.Path,
                SearchIndex = Item.SearchIndex
            };
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}