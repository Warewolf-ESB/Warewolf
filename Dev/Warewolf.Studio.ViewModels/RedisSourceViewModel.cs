/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
    public class RedisSourceViewModel : SourceBaseImpl<IRedisServiceSource>, IRedisSourceViewModel
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
        IRedisServiceSource _redisServiceSource;
        readonly IRedisSourceModel _redisSourceModel;
        CancellationTokenSource _token;
        readonly Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;

        public IAsyncWorker AsyncWorker { get; set; }
        public IExternalProcessExecutor Executor { get; set; }

        public RedisSourceViewModel(IRedisSourceModel redisSourceModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : base("RedisSource")
        {
            VerifyArgument.IsNotNull(nameof(executor), executor);
            VerifyArgument.IsNotNull(nameof(asyncWorker), asyncWorker);
            VerifyArgument.IsNotNull(nameof(redisSourceModel), redisSourceModel);
            VerifyArgument.IsNotNull(nameof(aggregator), aggregator);
            AsyncWorker = asyncWorker;
            Executor = executor;
            _redisSourceModel = redisSourceModel;
            _warewolfserverName = redisSourceModel.ServerName;
            _authenticationType = AuthenticationType.Anonymous;
            _hostName = string.Empty;
            _port = "6379";
            _password = string.Empty;
            HeaderText = Resources.Languages.Core.RedisNewHeaderLabel;
            Header = Resources.Languages.Core.RedisNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
        }

        public RedisSourceViewModel(IRedisSourceModel redisSourceModel, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(redisSourceModel, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull(nameof(requestServiceNameViewModel), requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;
        }

        public RedisSourceViewModel(IRedisSourceModel redisSourceModel, IEventAggregator aggregator, IRedisServiceSource redisServiceSource, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(redisSourceModel, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull(nameof(redisServiceSource), redisServiceSource);
            _warewolfserverName = redisSourceModel.ServerName;
            AsyncWorker.Start(() => redisSourceModel.FetchSource(redisServiceSource.Id), source =>
            {
                _redisServiceSource = source;
                _redisServiceSource.Path = redisServiceSource.Path;

                FromModel(_redisServiceSource);
                Item = ToSource();
                SetupHeaderTextFromExisting();
            });
        }

        public RedisSourceViewModel()
            : base("RedisSource")
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
                HeaderText = (_redisServiceSource == null ? ResourceName : _redisServiceSource.Name).Trim();
                Header = (_redisServiceSource == null ? ResourceName : _redisServiceSource.Name).Trim();
            }
            else
            {
                HeaderText = (_redisServiceSource == null ? ResourceName : _redisServiceSource.Name).Trim();
                Header = (_redisServiceSource == null ? ResourceName : _redisServiceSource.Name).Trim();
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
            if (_redisServiceSource == null)
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
                    _redisServiceSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _redisServiceSource = src;
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
            _redisSourceModel.TestConnection(ToNewSource());
        }

        IRedisServiceSource ToNewSource() => new RedisSourceDefinition
        {
            AuthenticationType = AuthenticationType,
            HostName = HostName,
            Password = Password,
            Name = ResourceName,
            Port = Port,
            Id = _redisServiceSource?.Id ?? Guid.NewGuid()
        };

        public override void FromModel(IRedisServiceSource source)
        {
            ResourceName = source.Name;
            HostName = source.HostName;
            Port = source.Port;
            AuthenticationType = source.AuthenticationType;
            Password = source.Password;
        }

        void Save(IRedisServiceSource source) => _redisSourceModel.Save(source);

        public override void Save() => SaveConnection();

        IRedisServiceSource ToSource()
        {
            if (_redisServiceSource == null)
            {
                return new RedisSourceDefinition
                {
                    HostName = HostName,
                    Port = Port,
                    AuthenticationType = AuthenticationType,
                    Password = Password,
                    Name = ResourceName,
                    Id = _redisServiceSource?.Id ?? SelectedGuid
                }
            ;
            }
            else
            {
                _redisServiceSource.HostName = HostName;
                _redisServiceSource.Port = Port;
                _redisServiceSource.AuthenticationType = AuthenticationType;
                _redisServiceSource.Password = Password;
                return _redisServiceSource;
            }
        }

        public override IRedisServiceSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            return new RedisSourceDefinition
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
