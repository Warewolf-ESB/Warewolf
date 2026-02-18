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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ChatbotSourceViewModel : SourceBaseImpl<IChatbotSource>, IManageChatbotSourceViewModel, IDisposable
    {
        public IAsyncWorker AsyncWorker { get; set; }
        IChatbotSource _chatbotSource;
        readonly IServer _environment;
        readonly IManageChatbotSourceModel _updateManager;
        string _apiKey;
        string _modelsEndpoint;
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
        string _selectedProvider;
        readonly Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;

        // AI Provider presets
        private static readonly Dictionary<string, (string ModelsEndpoint, string CompletionsEndpoint)> ProviderPresets = new Dictionary<string, (string, string)>
        {
            { "Anthropic", ("https://api.anthropic.com/v1/models", "https://api.anthropic.com/v1/messages") },
            { "GitHub Models", ("https://models.github.com/v1/models", "https://models.github.com/v1/chat/completions") },
            { "Google Gemini", ("https://generativelanguage.googleapis.com/v1beta/models", "https://generativelanguage.googleapis.com/v1beta/{model}:generateContent") },
            { "OpenAI", ("https://api.openai.com/v1/models", "https://api.openai.com/v1/chat/completions") },
            { "OpenRouter", ("https://openrouter.ai/api/v1/models", "https://openrouter.ai/api/v1/chat/completions") },
            { "XAI", ("https://api.x.ai/v1/models", "https://api.x.ai/v1/chat/completions") }
        };

        // Documentation URLs for API key generation
        private static readonly Dictionary<string, string> ProviderDocumentationUrls = new Dictionary<string, string>
        {
            { "Anthropic", "https://console.anthropic.com/settings/keys" },
            { "GitHub Models", "https://github.com/settings/tokens" },
            { "Google Gemini", "https://aistudio.google.com/app/apikey" },
            { "OpenAI", "https://platform.openai.com/api-keys" },
            { "OpenRouter", "https://openrouter.ai/settings/keys" },
            { "XAI", "https://console.x.ai/" }
        };

        public ChatbotSourceViewModel(IManageChatbotSourceModel updateManager, IEventAggregator aggregator, IAsyncWorker asyncWorker, IServer environment)
            : base("ChatbotSource")
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            AsyncWorker = asyncWorker;
            _environment = environment;
            _updateManager = updateManager;
            _apiKey = string.Empty;
            _modelsEndpoint = string.Empty;
            _completionsEndpoint = string.Empty;
            HeaderText = "New Chatbot Source";
            Header = "New Chatbot Source";
            TestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(TestConnection, CanTest);
            SaveCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CancelTest, CanCancelTest);
            OpenProviderDocumentationCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(OpenProviderDocumentation, CanOpenProviderDocumentation);
        }

        public ChatbotSourceViewModel(IManageChatbotSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IServer environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;
        }

        public ChatbotSourceViewModel(IManageChatbotSourceModel updateManager, IEventAggregator aggregator, IChatbotSource chatbotSource, IAsyncWorker asyncWorker, IServer environment)
            : this(updateManager, aggregator, asyncWorker, environment)
        {
            VerifyArgument.IsNotNull("chatbotSource", chatbotSource);

            asyncWorker.Start(() => updateManager.FetchSource(chatbotSource.Id), source =>
            {
                _chatbotSource = source;
                _chatbotSource.Path = chatbotSource.Path;
                SetupHeaderTextFromExisting();
                ToItem();
                FromModel(source);
            });
        }

        void ToItem()
        {
            Item = new ChatbotSourceDefinition()
            {
                Path = _chatbotSource.Path,
                ApiKey = _chatbotSource.ApiKey,
                ModelsEndpoint = _chatbotSource.ModelsEndpoint,
                CompletionsEndpoint = _chatbotSource.CompletionsEndpoint,
                Name = _chatbotSource.Name,
                Id = _chatbotSource.Id,
            };
        }

        void SetupHeaderTextFromExisting()
        {
            HeaderText = (_chatbotSource == null ? ResourceName : _chatbotSource.Name).Trim();
            Header = (_chatbotSource == null ? ResourceName : _chatbotSource.Name).Trim();
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

        bool CanOpenProviderDocumentation()
        {
            return !string.IsNullOrEmpty(SelectedProvider) && ProviderDocumentationUrls.ContainsKey(SelectedProvider);
        }

        void OpenProviderDocumentation()
        {
            if (!string.IsNullOrEmpty(SelectedProvider) && ProviderDocumentationUrls.ContainsKey(SelectedProvider))
            {
                var url = ProviderDocumentationUrls[SelectedProvider];
                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error($"Failed to open documentation URL: {url}", ex, "Warewolf Error");
                }
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

		public override void FromModel(IChatbotSource source)
		{
			ResourceName = source.Name;
			ApiKey = source.ApiKey;
			ModelsEndpoint = source.ModelsEndpoint;
			CompletionsEndpoint = source.CompletionsEndpoint;
			_selectedProvider = source.Provider;
			OnPropertyChanged(() => SelectedProvider);
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
            if (_chatbotSource == null)
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
                    _chatbotSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _chatbotSource = src;
                SetupHeaderTextFromExisting();
            }
        }

        public void Save(IChatbotSource source)
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
            var chatbotSource = ToNewSource();
            _updateManager.TestConnection(chatbotSource);
        }

		IChatbotSource ToNewSource() => new ChatbotSourceDefinition
		{
			ApiKey = ApiKey,
			ModelsEndpoint = ModelsEndpoint,
			CompletionsEndpoint = CompletionsEndpoint,
			Provider = SelectedProvider,
			Name = ResourceName,
			Id = _chatbotSource?.Id ?? Guid.NewGuid()
		};

		IChatbotSource ToSource()
		{
			if (_chatbotSource == null)
			{
				return new ChatbotSourceDefinition
				{
					ApiKey = ApiKey,
					ModelsEndpoint = ModelsEndpoint,
					CompletionsEndpoint = CompletionsEndpoint,
					Provider = SelectedProvider,
					Name = ResourceName,
					Id = _chatbotSource?.Id ?? Guid.NewGuid()
				};
			}
			else
			{
				_chatbotSource.ApiKey = ApiKey;
				_chatbotSource.ModelsEndpoint = ModelsEndpoint;
				_chatbotSource.CompletionsEndpoint = CompletionsEndpoint;
				_chatbotSource.Provider = SelectedProvider;
				return _chatbotSource;
			}
		}

        public override IChatbotSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

			return new ChatbotSourceDefinition
			{
				Name = ResourceName,
				ApiKey = ApiKey,
				ModelsEndpoint = ModelsEndpoint,
				CompletionsEndpoint = CompletionsEndpoint,
				Provider = SelectedProvider,
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

        public string ModelsEndpoint
        {
            get => _modelsEndpoint;
            set
            {
                if (_modelsEndpoint != value)
                {
                    TestPassed = false;
                }
                _modelsEndpoint = value;
                OnPropertyChanged(() => ModelsEndpoint);
                
                // Auto-populate CompletionsEndpoint if it's empty or compatible
                UpdateCompletionsEndpointFromModels();
                
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(SaveCommand);
            }
        }

        private void UpdateCompletionsEndpointFromModels()
        {
            // Only update if ModelsEndpoint is not empty
            if (string.IsNullOrEmpty(_modelsEndpoint))
            {
                return;
            }

            // Check if CompletionsEndpoint is empty or if it's compatible with ModelsEndpoint
            if (string.IsNullOrEmpty(_completionsEndpoint) || IsCompatibleEndpoint(_completionsEndpoint, _modelsEndpoint))
            {
                // Try to derive completions endpoint from models endpoint
                var completionsEndpoint = DeriveCompletionsEndpoint(_modelsEndpoint);
                if (!string.IsNullOrEmpty(completionsEndpoint))
                {
                    _completionsEndpoint = completionsEndpoint;
                    OnPropertyChanged(() => CompletionsEndpoint);
                }
            }
        }

        private bool IsCompatibleEndpoint(string completionsEndpoint, string modelsEndpoint)
        {
            if (string.IsNullOrEmpty(completionsEndpoint) || string.IsNullOrEmpty(modelsEndpoint))
            {
                return false;
            }

            try
            {
                var completionsUri = new Uri(completionsEndpoint);
                var modelsUri = new Uri(modelsEndpoint);

                // Check if they have the same scheme and authority (host)
                if (completionsUri.Scheme != modelsUri.Scheme || completionsUri.Authority != modelsUri.Authority)
                {
                    return false;
                }

                // Check if the completions endpoint looks like it was derived from the models endpoint
                var modelsPath = modelsUri.AbsolutePath.TrimEnd('/');
                var completionsPath = completionsUri.AbsolutePath.TrimEnd('/');

                // Check if completions path is the expected derivation from models path
                if (modelsPath.EndsWith("/models"))
                {
                    var basePath = modelsPath.Substring(0, modelsPath.Length - "/models".Length);
                    return completionsPath == basePath + "/chat/completions";
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string DeriveCompletionsEndpoint(string modelsEndpoint)
        {
            if (string.IsNullOrEmpty(modelsEndpoint))
            {
                return null;
            }

            try
            {
                var uri = new Uri(modelsEndpoint);
                var path = uri.AbsolutePath.TrimEnd('/');

                // Replace "/models" with "/chat/completions"
                if (path.EndsWith("/models"))
                {
                    var basePath = path.Substring(0, path.Length - "/models".Length);
                    var completionsPath = basePath + "/chat/completions";
                    return $"{uri.Scheme}://{uri.Authority}{completionsPath}";
                }

                // If path doesn't end with /models, try appending /chat/completions to the base
                return $"{uri.Scheme}://{uri.Authority}{path}/chat/completions";
            }
            catch
            {
                return null;
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
        public ICommand OpenProviderDocumentationCommand { get; set; }

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

        public IEnumerable<string> Providers => ProviderPresets.Keys;

        public string SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value;
                OnPropertyChanged(() => SelectedProvider);
                
                // Auto-populate endpoints when a provider is selected
                if (!string.IsNullOrEmpty(_selectedProvider) && ProviderPresets.ContainsKey(_selectedProvider))
                {
                    var preset = ProviderPresets[_selectedProvider];
                    ModelsEndpoint = preset.ModelsEndpoint;
                    CompletionsEndpoint = preset.CompletionsEndpoint;
                }
                
                ViewModelUtils.RaiseCanExecuteChanged(OpenProviderDocumentationCommand);
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
}
