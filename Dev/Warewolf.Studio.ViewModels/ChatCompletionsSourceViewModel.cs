#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ChatCompletionsSourceViewModel : SourceBaseImpl<IChatCompletionsSource>, IManageChatCompletionsSourceViewModel, IDisposable
    {
        public IAsyncWorker AsyncWorker { get; set; }
        IChatCompletionsSource _chatCompletionsSource;
        readonly IServer _environment;
        readonly IChatCompletionsSourceModel _updateManager;
        string _apiKey;
        string _completionsEndpoint;
        string _testMessage;
        CancellationTokenSource _token;
        bool _testPassed;
        string _resourceName;
        bool _testing;
        string _headerText;
        bool _testFailed;
        string _path;
        bool _isDisposed;
        readonly Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;

        public ChatCompletionsSourceViewModel(IChatCompletionsSourceModel updateManager, IEventAggregator aggregator, IAsyncWorker asyncWorker, IServer environment)
            : base("ChatCompletionsSource")
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            AsyncWorker = asyncWorker;
            _environment = environment;
            _updateManager = updateManager;
            _apiKey = string.Empty;
            _completionsEndpoint = string.Empty;
            HeaderText = "New Chat Completions Source";
            Header = "New Chat Completions Source";
            TestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(TestConnection, CanTest);
            SaveCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CancelTest, CanCancelTest);
        }

        public ChatCompletionsSourceViewModel(IChatCompletionsSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IServer environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;
        }

        public ChatCompletionsSourceViewModel(IChatCompletionsSourceModel updateManager, IEventAggregator aggregator, IChatCompletionsSource chatCompletionsSource, IAsyncWorker asyncWorker, IServer environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("chatCompletionsSource", chatCompletionsSource);

            asyncWorker.Start(() => updateManager.FetchSource(chatCompletionsSource.Id), source =>
            {
                _chatCompletionsSource = source;
                _chatCompletionsSource.Path = chatCompletionsSource.Path;
                SetupHeaderTextFromExisting();
                ToItem();
                FromModel(source);
            });
        }

        void ToItem()
        {
            Item = new ChatCompletionsSourceDefinition()
            {
                Path = _chatCompletionsSource.Path,
                ApiKey = _chatCompletionsSource.ApiKey,
                CompletionsEndpoint = _chatCompletionsSource.CompletionsEndpoint,
                Name = _chatCompletionsSource.Name,
                Id = _chatCompletionsSource.Id,
            };
        }

        void SetupHeaderTextFromExisting()
        {
            HeaderText = (_chatCompletionsSource == null ? ResourceName : _chatCompletionsSource.Name).Trim();
            Header = (_chatCompletionsSource == null ? ResourceName : _chatCompletionsSource.Name).Trim();
        }

        public override bool CanSave() => TestPassed;

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

            if (string.IsNullOrEmpty(CompletionsEndpoint))
            {
                return false;
            }

            // Validate that CompletionsEndpoint is a valid URI
            if (!Uri.TryCreate(CompletionsEndpoint, UriKind.Absolute, out Uri _))
            {
                return false;
            }

            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override void FromModel(IChatCompletionsSource source)
        {
            ResourceName = source.Name;
            ApiKey = source.ApiKey;
            CompletionsEndpoint = source.CompletionsEndpoint;
        }

        public override string Name
        {
            get => ResourceName;
            set => ResourceName = value;
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

        void SaveConnection()
        {
            if (_chatCompletionsSource == null)
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
                    _chatCompletionsSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _chatCompletionsSource = src;
                SetupHeaderTextFromExisting();
            }
        }

        public void Save(IChatCompletionsSource source)
        {
            _updateManager.Save(source);
        }

        public override void Save()
        {
            SaveConnection();
        }

        void TestConnection()
        {
            _token = new CancellationTokenSource();
            AsyncWorker.Start(SetupProgressSpinner, () =>
            {
                TestMessage = "Test Passed";
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
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });
            var chatCompletionsSource = ToNewSource();
            _updateManager.TestConnection(chatCompletionsSource);
        }

        IChatCompletionsSource ToNewSource() => new ChatCompletionsSourceDefinition
        {
            ApiKey = ApiKey,
            CompletionsEndpoint = CompletionsEndpoint,
            Name = ResourceName,
            Id = _chatCompletionsSource?.Id ?? Guid.NewGuid()
        };

        IChatCompletionsSource ToSource()
        {
            if (_chatCompletionsSource == null)
            {
                return new ChatCompletionsSourceDefinition
                {
                    ApiKey = ApiKey,
                    CompletionsEndpoint = CompletionsEndpoint,
                    Name = ResourceName,
                    Id = _chatCompletionsSource?.Id ?? Guid.NewGuid()
                };
            }
            else
            {
                _chatCompletionsSource.ApiKey = ApiKey;
                _chatCompletionsSource.CompletionsEndpoint = CompletionsEndpoint;
                return _chatCompletionsSource;
            }
        }

        public override IChatCompletionsSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            return new ChatCompletionsSourceDefinition
            {
                Name = ResourceName,
                ApiKey = ApiKey,
                CompletionsEndpoint = CompletionsEndpoint,
                Id = Item.Id,
                Path = Path
            };
        }

        private IRequestServiceNameViewModel GetRequestServiceNameViewModel()
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

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                OnPropertyChanged(() => ApiKey);
                OnPropertyChanged(() => Header);
                TestPassed = false;
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
        }

        public string CompletionsEndpoint
        {
            get => _completionsEndpoint;
            set
            {
                if (_completionsEndpoint != value)
                {
                    TestPassed = false;
                }
                _completionsEndpoint = value;
                OnPropertyChanged(() => CompletionsEndpoint);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
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

        public bool TestPassed
        {
            get => _testPassed;
            set
            {
                _testPassed = value;
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
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

        public ICommand TestCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CancelTestCommand { get; set; }

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

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(() => Path);
            }
        }

        #region Implementation of IDisposable

        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _token?.Dispose();
                }
                _isDisposed = true;
            }
        }

        #endregion
    }

    // Definition class to hold the data
    public class ChatCompletionsSourceDefinition : IChatCompletionsSource
    {
        public string ApiKey { get; set; }
        public string CompletionsEndpoint { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Guid Id { get; set; }

        public bool Equals(IChatCompletionsSource other)
        {
            if (other == null) return false;
            return ApiKey == other.ApiKey &&
                   CompletionsEndpoint == other.CompletionsEndpoint &&
                   Name == other.Name &&
                   Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IChatCompletionsSource);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ApiKey?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (CompletionsEndpoint?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                return hashCode;
            }
        }
    }
}
