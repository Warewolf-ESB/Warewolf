#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
#else
using Prism.Commands;
using Dev2.Common;
using Prism.Mvvm;
#endif
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Warewolf.Configuration;

namespace Warewolf.Studio.ViewModels
{
#if NETFRAMEWORK
    public class ChatbotViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase, IDisposable
#else
	public class ChatbotViewModel : BindableBase2, IDisposable
#endif
	{
		/// <summary>Number of retry attempts when waiting for system prompt initialization (each attempt waits RetryDelayMs).</summary>
		private const int MaxRetryAttempts = 20;

		/// <summary>Delay in milliseconds between retry attempts for system prompt initialization.</summary>
		private const int RetryDelayMs = 500;

		private bool _disposed;
		private readonly IServer _server;
		private readonly Caliburn.Micro.IEventAggregator _eventAggregator;
		private readonly IChatbotContextBuilder _contextBuilder;
		private readonly IChatbotApiService _chatbotApiService;
		private string _message;
        private ObservableCollection<string> _messages;
        private string _displayName;
        private bool _isChatbotConfigured;
        private bool _isSending;
        private ChatbotSource _configuredSource;
        private string _systemPrompt;
        private bool _systemPromptInitialized;
        private string _selectedModel;
        private bool _isInitializingPrompt;
        private string _loadingStatusText;
        private string _resourcesJson;
        private string _systemLog;
        private bool _includeSystemLog = true;
        private bool _includeResourcesXaml = true;
        private bool _includeResourcesJson = true;

        public string LoadingStatusText
        {
            get => _loadingStatusText;
            set
            {
                _loadingStatusText = value;
                OnPropertyChanged(nameof(LoadingStatusText));
            }
        }

        public bool IncludeSystemLog
        {
            get => _includeSystemLog;
            set
            {
                _includeSystemLog = value;
                OnPropertyChanged(nameof(IncludeSystemLog));
            }
        }

        public bool IncludeResourcesXaml
        {
            get => _includeResourcesXaml;
            set
            {
                _includeResourcesXaml = value;
                OnPropertyChanged(nameof(IncludeResourcesXaml));
            }
        }

        public bool IncludeResourcesJson
        {
            get => _includeResourcesJson;
            set
            {
                _includeResourcesJson = value;
                OnPropertyChanged(nameof(IncludeResourcesJson));
            }
        }

        public ChatbotViewModel()
        {
            DisplayName = "Chatbot";
            Messages = new ObservableCollection<string>();
            SendCommand = new DelegateCommand(Send, CanSend);
        }

        public ChatbotViewModel(IServer server, ICommand openSettingsCommand)
            : this(server, openSettingsCommand, new ChatbotApiService())
        {
        }

        public ChatbotViewModel(IServer server, ICommand openSettingsCommand, IChatbotApiService chatbotApiService)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            OpenSettingsCommand = openSettingsCommand ?? throw new ArgumentNullException(nameof(openSettingsCommand));
            _chatbotApiService = chatbotApiService ?? throw new ArgumentNullException(nameof(chatbotApiService));

            DisplayName = "Chatbot";
            Messages = new ObservableCollection<string>();
            SendCommand = new DelegateCommand(Send, CanSend);

            LoadChatbotConfiguration();

            // Subscribe to settings saved event
            try
            {
                _eventAggregator = Dev2.Services.Events.EventPublishers.Aggregator;
                if (_eventAggregator != null)
                {
#if NETFRAMEWORK
                    _eventAggregator.Subscribe(this);
#endif
                }
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error subscribing to chatbot settings saved event", ex, "Warewolf Error");
            }
        }

        public void Handle(Warewolf.Data.ChatbotSettingsSavedMessage message)
        {
            RefreshConfiguration();
        }

        public void Handle(Dev2.Studio.Core.Messages.RemoveResourceAndCloseTabMessage message)
        {
            // Check if the deleted resource is the currently configured chatbot source
            if (message?.ResourceToRemove != null && _configuredSource != null)
            {
                // Compare the resource ID of the deleted resource with the configured chatbot source ID
                var deletedResourceId = message.ResourceToRemove.ID;
                var configuredSourceId = _configuredSource.ResourceID;
                
                if (deletedResourceId == configuredSourceId)
                {
                    Dev2.Common.Dev2Logger.Info($"Chatbot source '{message.ResourceToRemove.ResourceName}' was deleted. Refreshing chatbot configuration.", "Warewolf Info");
                    
                    // The configured source was deleted, refresh to show unconfigured state
                    RefreshConfiguration();
                }
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
				((DelegateCommand)SendCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public bool IsChatbotConfigured
        {
            get => _isChatbotConfigured;
            set
            {
                _isChatbotConfigured = value;
                OnPropertyChanged(nameof(IsChatbotConfigured));
                OnPropertyChanged(nameof(ShowConfigurationMessage));
                ((DelegateCommand)SendCommand).RaiseCanExecuteChanged();
            }
        }

        public bool ShowConfigurationMessage => !IsChatbotConfigured;

        public bool IsSending
        {
            get => _isSending;
            set
            {
                _isSending = value;
                OnPropertyChanged(nameof(IsSending));
                OnPropertyChanged(nameof(IsLoading));
                ((DelegateCommand)SendCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsInitializingPrompt
        {
            get => _isInitializingPrompt;
            set
            {
                _isInitializingPrompt = value;
                OnPropertyChanged(nameof(IsInitializingPrompt));
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsLoading => IsInitializingPrompt || IsSending;

        public ICommand SendCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public void RefreshConfiguration()
        {
            // Clear messages when configuration is refreshed
            Messages.Clear();
            
            // Clear any existing system prompt so it gets regenerated
            // Do this BEFORE loading config to avoid race condition with background initialization
            _systemPromptInitialized = false;
            _systemPrompt = null;
            
            LoadChatbotConfiguration();
        }

        private void LoadChatbotConfiguration()
        {
            try
            {
                if (_server?.ResourceRepository == null)
                {
                    IsChatbotConfigured = false;
                    return;
                }

                var settingsData = _server.ResourceRepository.GetChatbotSettings<ChatbotSettingsData>(_server);

                // Load checkbox settings using properties to trigger property change notifications
                IncludeSystemLog = settingsData.IncludeSystemLog;
                IncludeResourcesXaml = settingsData.IncludeResourcesXaml;
                IncludeResourcesJson = settingsData.IncludeResourcesJson;

                if (settingsData?.ChatbotSource?.Value == null || settingsData.ChatbotSource.Value == Guid.Empty)
                {
                    IsChatbotConfigured = false;
                    return;
                }

                var payload = settingsData.ChatbotSource.Payload;
                payload = DpapiWrapper.Decrypt(payload);

                var serializer = new Dev2JsonSerializer();
                _configuredSource = serializer.Deserialize<ChatbotSource>(payload);

                IsChatbotConfigured = _configuredSource != null
                    && !string.IsNullOrWhiteSpace(_configuredSource.CompletionsEndpoint);

                if (IsChatbotConfigured)
                {
                    // Always use the selected model from settings
                    _selectedModel = !string.IsNullOrEmpty(_configuredSource.SelectedModel) 
                        ? _configuredSource.SelectedModel 
                        : "gpt-4o-mini";
                    
                    Dev2.Common.Dev2Logger.Info($"Using model from settings: {_selectedModel}", "Warewolf Info");
                    
                    // Initialize the system prompt with workspace context
                    InitializeSystemPrompt();
                }
            }
            catch
            {
                IsChatbotConfigured = false;
                _configuredSource = null;
            }
        }

        private async void InitializeSystemPrompt()
        {
            if (_systemPromptInitialized)
            {
                return;
            }

            IsInitializingPrompt = true;
            LoadingStatusText = "Initializing chatbot context...";

            try
            {
                var options = new ChatbotContextOptions
                {
                    IncludeSystemLog = _includeSystemLog,
                    IncludeResourcesXaml = _includeResourcesXaml,
                    IncludeResourcesJson = _includeResourcesJson,
                    Server = _server,
                    StatusUpdateCallback = status => UpdateStatusOnUiThread(status)
                };

                var result = await _contextBuilder.BuildContextAsync(options);

                _systemPrompt = result.SystemPrompt;
                _resourcesJson = result.ResourcesJson;
                _systemLog = result.SystemLog;
                _systemPromptInitialized = true;

                InvokeOnUiThread(() => DisplayContextLoadedGreeting(result));
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error($"ChatbotContext: Error initializing chatbot system prompt", ex, "Warewolf Error");

                _systemPrompt = "You are a Warewolf workflow debugging assistant. Note: Workspace context could not be loaded.";
                _systemPromptInitialized = true;

                InvokeOnUiThread(() =>
                {
                    IsInitializingPrompt = false;
                    LoadingStatusText = string.Empty;
                    Messages.Add("Chatbot: Hello! I'm your Warewolf debugging assistant. " +
                        "Note: I had trouble loading workspace context, but I can still help answer general questions.");
                });
            }
        }

        private void DisplayContextLoadedGreeting(ChatbotContextResult result)
        {
            IsInitializingPrompt = false;
            LoadingStatusText = string.Empty;

            if (result.ResourceCount > 0 || result.HasSystemLog)
            {
                var contextParts = new System.Collections.Generic.List<string>();
                if (result.ResourceCount > 0)
                {
                    contextParts.Add($"{result.ResourceCount} resource{(result.ResourceCount == 1 ? "" : "s")}");
                }
                if (result.HasSystemLog)
                {
                    contextParts.Add("recent system logs");
                }

                Messages.Add($"Chatbot: Hello! I'm your Warewolf debugging assistant. I have analyzed your workspace and loaded " +
                    $"{string.Join(" and ", contextParts)}. " +
                    "I can help you understand your workflows, debug issues, and answer questions about your Warewolf environment. " +
                    "What would you like to know?");
            }
            else
            {
                Messages.Add("Chatbot: Hello! I'm your Warewolf debugging assistant. " +
                    "Note: No context is currently loaded. You can enable system log and resources in Settings to provide more context.");
            }
        }

        private void UpdateStatusOnUiThread(string status)
        {
            InvokeOnUiThread(() => { LoadingStatusText = status; });
        }

        private static void InvokeOnUiThread(Action action)
        {
            System.Windows.Application.Current?.Dispatcher?.BeginInvoke(action);
        }

		private bool CanSend()
		{
			return IsChatbotConfigured && !string.IsNullOrWhiteSpace(Message) && !IsSending && !IsInitializingPrompt;
		}

		private async void Send()
        {
            await SendAsync();
        }

        private async Task SendAsync()
        {
            if (!CanSend())
            {
                return;
            }

            var userMessage = Message;
            Message = string.Empty;
            Messages.Add($"You: {userMessage}");

            IsSending = true;

            try
            {
                var response = await CallChatbotApiAsync(userMessage);
                Messages.Add($"Bot: {response}");
            }
            catch (Exception ex)
            {
                Messages.Add($"Error: {ex.Message}");
            }
            finally
            {
                IsSending = false;
            }
        }

        private async Task<string> CallChatbotApiAsync(string userMessage)
        {
            try
            {
                await WaitForSystemPromptInitializationAsync();

                var chatMessages = BuildChatMessages();

                return await _chatbotApiService.SendMessageAsync(chatMessages, _configuredSource);
            }
            catch (HttpRequestException ex) when (IsTokenLimitError(ex))
            {
                Dev2.Common.Dev2Logger.Warn($"Token limit reached: {ex.Message}", "Warewolf Info");

                return "The token limit has been exceeded. The context is too large for the selected model. " +
                       "Please open Settings (click the link at the top of the chatbot to configure) and uncheck some system prompt options " +
                       "(System Log, Resources XAML, or Resources JSON) to reduce the context size.";
            }
        }

        private async Task WaitForSystemPromptInitializationAsync()
        {
            var waitCount = 0;
            while (!_systemPromptInitialized && waitCount < MaxRetryAttempts)
            {
                await Task.Delay(RetryDelayMs);
                waitCount++;
            }
        }

        private System.Collections.Generic.List<ChatCompletionMessage> BuildChatMessages()
        {
            var chatMessages = new System.Collections.Generic.List<ChatCompletionMessage>();

            // Add system prompt (or fallback if initialization timed out)
            if (!string.IsNullOrEmpty(_systemPrompt))
            {
                chatMessages.Add(new ChatCompletionMessage { Role = "system", Content = _systemPrompt });
            }
            else
            {
                var fallbackPrompt = "You are a Warewolf workflow debugging assistant. Help the user understand and debug their workflows.";
                chatMessages.Add(new ChatCompletionMessage { Role = "system", Content = fallbackPrompt });
                Dev2.Common.Dev2Logger.Warn("Using fallback system prompt - full context initialization timed out", "Warewolf Info");
            }

            // Add conversation history; skip "Chatbot: " greetings and error/system messages
            // to maintain the required user/assistant alternating pattern
            foreach (var msg in Messages)
            {
                if (msg.StartsWith("You: "))
                {
                    chatMessages.Add(new ChatCompletionMessage { Role = "user", Content = msg.Substring(5) });
                }
                else if (msg.StartsWith("Bot: "))
                {
                    chatMessages.Add(new ChatCompletionMessage { Role = "assistant", Content = msg.Substring(5) });
                }
            }

            return chatMessages;
        }

        private static bool IsTokenLimitError(HttpRequestException ex)
        {
            if (ex.Message == null)
            {
                return false;
            }

            var message = ex.Message.ToLower();
            return message.Contains("token") && (message.Contains("limit") || message.Contains("exceeded") || message.Contains("maximum"))
                || message.Contains("413") // Payload too large
                || message.Contains("context_length_exceeded")
                || message.Contains("context") && message.Contains("overflow") // LM Studio context overflow
                || message.Contains("context length") && message.Contains("not enough"); // LM Studio context length error
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _eventAggregator?.Unsubscribe(this);
                _disposed = true;
            }
        }
    }
}
