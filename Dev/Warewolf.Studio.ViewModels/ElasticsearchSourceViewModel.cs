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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Warewolf.Studio.ViewModels
{
    public class ElasticsearchSourceViewModel : SourceBaseImpl<IElasticsearchServiceSource>, IElasticsearchSourceViewModel
    {
        string _headerText;
        string _hostName;
        string _port;
        string _password;
        bool _testPassed;
        bool _testFailed;
        string _testMessage;
        bool _testing;
        string _resourceName;
        readonly string _warewolfserverName;

        AuthenticationType _authenticationType;
        IElasticsearchServiceSource _elasticsearchServiceSource;
        readonly IElasticsearchSourceModel _elasticsearchSourceModel;
        CancellationTokenSource _token;
        readonly Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;

        public IAsyncWorker AsyncWorker { get; set; }
        public IExternalProcessExecutor Executor { get; set; }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : base("ElasticsearchSource")
        {
            VerifyArgument.IsNotNull(nameof(executor), executor);
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            VerifyArgument.IsNotNull(nameof(elasticsearchSourceModel), elasticsearchSourceModel);
            VerifyArgument.IsNotNull(nameof(aggregator), aggregator);
            AsyncWorker = asyncWorker;
            Executor = executor;
            _elasticsearchSourceModel = elasticsearchSourceModel;
            _warewolfserverName = elasticsearchSourceModel.ServerName;
            _authenticationType = AuthenticationType.Anonymous;
            _hostName = string.Empty;
            _port = "9200";
            _password = string.Empty;
            HeaderText = Resources.Languages.Core.ElasticsearchNewHeaderLabel;
            Header = Resources.Languages.Core.ElasticsearchNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
        }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(elasticsearchSourceModel, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull(nameof(requestServiceNameViewModel), requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;
        }

        public ElasticsearchSourceViewModel(IElasticsearchSourceModel elasticsearchSourceModel, IEventAggregator aggregator, IElasticsearchServiceSource elasticsearchServiceSource, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(elasticsearchSourceModel, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull(nameof(elasticsearchServiceSource), elasticsearchServiceSource);
            _warewolfserverName = elasticsearchSourceModel.ServerName;
            AsyncWorker.Start(() => elasticsearchSourceModel.FetchSource(elasticsearchServiceSource.Id), source =>
            {
                _elasticsearchServiceSource = source;
                _elasticsearchServiceSource.Path = elasticsearchServiceSource.Path;

                FromModel(_elasticsearchServiceSource);
                Item = ToSource();
                SetupHeaderTextFromExisting();
            });
        }

        public ElasticsearchSourceViewModel()
            : base("ElasticsearchSource")
        {

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
            get => _testing;
            private set
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
            var serverName = _warewolfserverName;
            if (serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                HeaderText = (_elasticsearchServiceSource == null ? ResourceName : _elasticsearchServiceSource.Name).Trim();
                Header = (_elasticsearchServiceSource == null ? ResourceName : _elasticsearchServiceSource.Name).Trim();
            }
            else
            {
                HeaderText = (_elasticsearchServiceSource == null ? ResourceName : _elasticsearchServiceSource.Name).Trim();
                Header = (_elasticsearchServiceSource == null ? ResourceName : _elasticsearchServiceSource.Name).Trim();
            }
        }

        public IRequestServiceNameViewModel GetRequestServiceNameViewModel()
        {
            if (_requestServiceNameViewModel != null)
            {
                _requestServiceNameViewModel.Wait();
                if (_requestServiceNameViewModel.Exception == null)
                {
                    return _requestServiceNameViewModel.Result;
                }

                else
                {
                    throw _requestServiceNameViewModel.Exception;
                }
            }
            return null;
        }

        public override bool CanSave() => TestPassed || CanTest();

        void SaveConnection()
        {
            if (_elasticsearchServiceSource == null)
            {
                var res = GetRequestServiceNameViewModel().ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    ResourceName = GetRequestServiceNameViewModel().ResourceName.Name;
                    var src = ToSource();
                    src.Path = GetRequestServiceNameViewModel().ResourceName.Path ?? GetRequestServiceNameViewModel().ResourceName.Name;
                    Save(src);
                    if (GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel != null)
                    {
                        AfterSave(GetRequestServiceNameViewModel().SingleEnvironmentExplorerViewModel.Environments[0].ResourceId, src.Id);
                    }

                    Item = src;
                    _elasticsearchServiceSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _elasticsearchServiceSource = src;
                SetupHeaderTextFromExisting();
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

        IElasticsearchServiceSource ToNewSource() => new ElasticsearchSourceDefinition
        {
            HostName = HostName,
            Password = Password,
            Name = ResourceName,
            Port = Port,
            Id = _elasticsearchServiceSource?.Id ?? Guid.NewGuid()
        };

        public override void FromModel(IElasticsearchServiceSource source)
        {
            ResourceName = source.Name;
            HostName = source.HostName;
            Port = source.Port;
            AuthenticationType = source.AuthenticationType;
            Password = source.Password;
        }

        void Save(IElasticsearchServiceSource source) => _elasticsearchSourceModel.Save(source);

        public override void Save() => SaveConnection();

        IElasticsearchServiceSource ToSource()
        {
            if (_elasticsearchServiceSource == null)
            {
                return new ElasticsearchSourceDefinition
                { 
                    AuthenticationType = AuthenticationType,
                    HostName = HostName,
                    Port = Port,
                    Password = Password,
                    Name = ResourceName,
                    Id = _elasticsearchServiceSource?.Id ?? SelectedGuid
                };
            }
            else
            {
                _elasticsearchServiceSource.HostName = HostName;
                _elasticsearchServiceSource.Port = Port;
                _elasticsearchServiceSource.Password = Password;
                _elasticsearchServiceSource.AuthenticationType = AuthenticationType;
                return _elasticsearchServiceSource;
            }
        }

        public override IElasticsearchServiceSource ToModel()
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
                Id = Item.Id,
                Path = Item.Path
            };
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
