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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Warewolf.Configuration;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace Warewolf.Studio.ViewModels
{
#if NETFRAMEWORK
    public class ChatbotViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase, IDisposable, Caliburn.Micro.IHandle<ChatbotSettingsSavedMessage>, Caliburn.Micro.IHandle<Dev2.Studio.Core.Messages.RemoveResourceAndCloseTabMessage>
#else
	public class ChatbotViewModel : BindableBase2, IDisposable, Caliburn.Micro.IHandle<ChatbotSettingsSavedMessage>, Caliburn.Micro.IHandle<Dev2.Studio.Core.Messages.RemoveResourceAndCloseTabMessage>
#endif
	{
		/// <summary>Number of retry attempts when waiting for system prompt initialization (each attempt waits RetryDelayMs).</summary>
		private const int MaxRetryAttempts = 20;

		/// <summary>Delay in milliseconds between retry attempts for system prompt initialization.</summary>
		private const int RetryDelayMs = 500;

		/// <summary>Minimum delay in milliseconds between consecutive send operations to prevent API abuse and excessive costs.</summary>
		private const int MinSendDelayMs = 1000;

		/// <summary>Maximum number of user+bot messages to keep in the sliding window before summarizing older messages.</summary>
		private const int MaxConversationMessages = 40;

		/// <summary>Number of user+bot messages that triggers conversation summarization.</summary>
		private const int SummarizationThreshold = 30;

		/// <summary>Maximum character length for conversation title derived from first user message.</summary>
		private const int MaxConversationTitleLength = 50;

		private bool _disposed;
		private CancellationTokenSource _streamingCts;
		private readonly IServer _server;
		private readonly Caliburn.Micro.IEventAggregator _eventAggregator;
		private readonly IChatbotContextBuilder _contextBuilder;
		private readonly IChatbotApiService _chatbotApiService;
		private DateTime _lastSendTime = DateTime.MinValue;
		private string _message;
        private ObservableCollection<ChatMessage> _messages;
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
        private bool _loadResourcesAsXaml = true;
        private int _numberOfLogLines = 1000;
        private List<Guid> _selectedResourceIds = new List<Guid>();
        private string _conversationSummary;
        private ChatConversation _currentConversation;
        private ChatConversation _selectedConversation;
        private ObservableCollection<ChatConversation> _savedConversations;

        private static readonly string ChatHistoryFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Warewolf", "ChatHistory");

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

        public bool LoadResourcesAsXaml
        {
            get => _loadResourcesAsXaml;
            set
            {
                _loadResourcesAsXaml = value;
                OnPropertyChanged(nameof(LoadResourcesAsXaml));
            }
        }

        public List<Guid> SelectedResourceIds
        {
            get => _selectedResourceIds;
            set
            {
                _selectedResourceIds = value ?? new List<Guid>();
                OnPropertyChanged(nameof(SelectedResourceIds));
            }
        }

        public int NumberOfLogLines
        {
            get => _numberOfLogLines;
            set
            {
                _numberOfLogLines = value;
                OnPropertyChanged(nameof(NumberOfLogLines));
            }
        }

        public ObservableCollection<ChatConversation> SavedConversations
        {
            get => _savedConversations;
            set
            {
                _savedConversations = value;
                OnPropertyChanged(nameof(SavedConversations));
            }
        }

        public ChatConversation SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                if (_selectedConversation == value)
                {
                    return;
                }

                // Auto-save current conversation before switching
                SaveCurrentConversation();

                _selectedConversation = value;
                OnPropertyChanged(nameof(SelectedConversation));

                if (value != null)
                {
                    LoadConversation(value);
                }
            }
        }

        public ChatbotViewModel()
        {
            _contextBuilder = new ChatbotContextBuilder();
            DisplayName = "Chatbot";
            Messages = new ObservableCollection<ChatMessage>();
            SavedConversations = new ObservableCollection<ChatConversation>();
            SendCommand = new DelegateCommand(Send, CanSend);
            NewConversationCommand = new DelegateCommand(NewConversation);
            ExportAsTextCommand = new DelegateCommand(ExportAsText, () => Messages.Count > 0);
            ExportAsJsonCommand = new DelegateCommand(ExportAsJson, () => Messages.Count > 0);
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
            _contextBuilder = new ChatbotContextBuilder();

            DisplayName = "Chatbot";
            Messages = new ObservableCollection<ChatMessage>();
            SavedConversations = new ObservableCollection<ChatConversation>();
            SendCommand = new DelegateCommand(Send, CanSend);
            NewConversationCommand = new DelegateCommand(NewConversation);
            ExportAsTextCommand = new DelegateCommand(ExportAsText, () => Messages.Count > 0);
            ExportAsJsonCommand = new DelegateCommand(ExportAsJson, () => Messages.Count > 0);

            LoadSavedConversationsList();
            LoadChatbotConfiguration();

            // Subscribe to settings saved event
            try
            {
                _eventAggregator = Dev2.Services.Events.EventPublishers.Aggregator;
                if (_eventAggregator != null)
                {
                    _eventAggregator.Subscribe(this);
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
                ((DelegateCommand)SendCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<ChatMessage> Messages
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
                ((DelegateCommand)SendCommand)?.RaiseCanExecuteChanged();
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
                ((DelegateCommand)SendCommand)?.RaiseCanExecuteChanged();
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
                ((DelegateCommand)SendCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading => IsInitializingPrompt || IsSending;

        public ICommand SendCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand NewConversationCommand { get; }
        public ICommand ExportAsTextCommand { get; }
        public ICommand ExportAsJsonCommand { get; }

        public void RefreshConfiguration()
        {
            // Save current conversation before clearing
            SaveCurrentConversation();

            // Clear messages when configuration is refreshed
            Messages.Clear();
            _conversationSummary = null;
            _currentConversation = null;

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
                LoadResourcesAsXaml = settingsData.LoadResourcesAsXaml;
                NumberOfLogLines = settingsData.NumberOfLogLines;
                SelectedResourceIds = settingsData.SelectedResourceIds ?? new List<Guid>();

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
                    LoadResourcesAsXaml = _loadResourcesAsXaml,
                    NumberOfLogLines = _numberOfLogLines,
                    SelectedResourceIds = _selectedResourceIds,
                    Server = _server,
                    StatusUpdateCallback = status => UpdateStatusOnUiThread(status)
                };

                var result = await _contextBuilder?.BuildContextAsync(options);

                if (result != null)
                {
                    _systemPrompt = result.SystemPrompt;
                    _resourcesJson = result.ResourcesJson;
                    _systemLog = result.SystemLog;
                    _systemPromptInitialized = true;

                    InvokeOnUiThread(() => DisplayContextLoadedGreeting(result));
                }
                else
                {
                    // Handle null result
                    _systemPrompt = "You are a Warewolf workflow debugging assistant. Note: Workspace context could not be loaded.";
                    _systemPromptInitialized = true;

                    InvokeOnUiThread(() =>
                    {
                        IsInitializingPrompt = false;
                        LoadingStatusText = string.Empty;
                        Messages.Add(ChatMessage.Create(ChatMessageType.System,
                            "Hello! I'm your Warewolf debugging assistant. " +
                            "Note: I had trouble loading workspace context, but I can still help answer general questions."));
                    });
                }
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
                    Messages.Add(ChatMessage.Create(ChatMessageType.System,
                        "Hello! I'm your Warewolf debugging assistant. " +
                        "Note: I had trouble loading workspace context, but I can still help answer general questions."));
                });
            }
        }

        private void DisplayContextLoadedGreeting(ChatbotContextResult result)
        {
            IsInitializingPrompt = false;
            LoadingStatusText = string.Empty;

            if (result.ResourceCount > 0 || result.HasSystemLog)
            {
                var contextParts = new List<string>();
                if (result.ResourceCount > 0)
                {
                    contextParts.Add($"{result.ResourceCount} resource{(result.ResourceCount == 1 ? "" : "s")}");
                }
                if (result.HasSystemLog)
                {
                    contextParts.Add("recent system logs");
                }

                Messages.Add(ChatMessage.Create(ChatMessageType.System,
                    $"Hello! I'm your Warewolf debugging assistant. I have analyzed your workspace and loaded " +
                    $"{string.Join(" and ", contextParts)}. " +
                    "I can help you understand your workflows, debug issues, and answer questions about your Warewolf environment. " +
                    "What would you like to know?"));
            }
            else
            {
                Messages.Add(ChatMessage.Create(ChatMessageType.System,
                    "Hello! I'm your Warewolf debugging assistant. " +
                    "Note: No context is currently loaded. You can enable system log and resources in Settings to provide more context."));
            }

            // Create a new conversation for this session
            _currentConversation = ChatConversation.CreateNew();
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
			// Check basic conditions
			if (!IsChatbotConfigured || string.IsNullOrWhiteSpace(Message) || IsSending || IsInitializingPrompt)
			{
				return false;
			}

			// Enforce rate limiting: ensure minimum delay between sends
			var timeSinceLastSend = DateTime.Now - _lastSendTime;
			return timeSinceLastSend.TotalMilliseconds >= MinSendDelayMs;
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

			// Record send time for rate limiting
			_lastSendTime = DateTime.Now;

			var userMessage = Message;
			Message = string.Empty;
			Messages.Add(ChatMessage.Create(ChatMessageType.User, userMessage));

			// Update conversation title from first user message
			UpdateConversationTitle(userMessage);

			IsSending = true;

			try
			{
				// Apply sliding window before sending
				ApplySlidingWindow();

				await CallChatbotApiStreamingAsync(userMessage);
			}
			catch (Exception ex)
			{
				// Log full exception details for debugging (includes stack trace, inner exceptions, etc.)
				Dev2.Common.Dev2Logger.Error("Chatbot send message failed", ex, "Warewolf Error");

				// Display user-friendly error message without sensitive details
				var userFriendlyError = GetUserFriendlyErrorMessage(ex);
				Messages.Add(ChatMessage.Create(ChatMessageType.Error, userFriendlyError));

				// Restore the user's message to the input box so they can retry
				Message = userMessage;
			}
			finally
			{
				IsSending = false;
				_streamingCts = null;

				// Schedule a refresh of CanExecute after the rate limit period
				_ = Task.Delay(MinSendDelayMs).ContinueWith(_ =>
				{
					InvokeOnUiThread(() => ((DelegateCommand)SendCommand).RaiseCanExecuteChanged());
				});
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
                Dev2.Common.Dev2Logger.Warn($"Token limit reached, attempting to trim conversation history: {ex.Message}", "Warewolf Info");

                // Attempt recovery by trimming oldest 50% of conversation messages
                var trimmed = TrimConversationHistory();
                if (trimmed)
                {
                    try
                    {
                        var chatMessages = BuildChatMessages();
                        return await _chatbotApiService.SendMessageAsync(chatMessages, _configuredSource);
                    }
                    catch (HttpRequestException retryEx) when (IsTokenLimitError(retryEx))
                    {
                        Dev2.Common.Dev2Logger.Warn($"Token limit still exceeded after trimming: {retryEx.Message}", "Warewolf Info");
                    }
                }

                return "The token limit has been exceeded. The context is too large for the selected model. " +
                       "Please open Settings (click the link at the top of the chatbot to configure) and uncheck some system prompt options " +
                       "(System Log, Resources XAML, or Resources JSON) to reduce the context size.";
            }
        }

        private async Task CallChatbotApiStreamingAsync(string userMessage)
        {
            await WaitForSystemPromptInitializationAsync();

            var chatMessages = BuildChatMessages();

            // Create a placeholder bot message for streaming tokens into
            var botMessage = ChatMessage.Create(ChatMessageType.Bot, "");
            InvokeOnUiThread(() => Messages.Add(botMessage));

            _streamingCts = new CancellationTokenSource();

            try
            {
                await _chatbotApiService.SendMessageStreamingAsync(chatMessages, _configuredSource, token =>
                {
                    InvokeOnUiThread(() => botMessage.AppendContent(token));
                }, _streamingCts.Token);
            }
            catch (HttpRequestException ex) when (IsTokenLimitError(ex))
            {
                Dev2.Common.Dev2Logger.Warn($"Token limit reached during streaming, attempting to trim: {ex.Message}", "Warewolf Info");

                // Remove the incomplete bot message
                InvokeOnUiThread(() => Messages.Remove(botMessage));

                var trimmed = TrimConversationHistory();
                if (trimmed)
                {
                    try
                    {
                        chatMessages = BuildChatMessages();
                        var retryMessage = ChatMessage.Create(ChatMessageType.Bot, "");
                        InvokeOnUiThread(() => Messages.Add(retryMessage));

                        await _chatbotApiService.SendMessageStreamingAsync(chatMessages, _configuredSource, token =>
                        {
                            InvokeOnUiThread(() => retryMessage.AppendContent(token));
                        }, _streamingCts.Token);
                        return;
                    }
                    catch (HttpRequestException retryEx) when (IsTokenLimitError(retryEx))
                    {
                        Dev2.Common.Dev2Logger.Warn($"Token limit still exceeded after trimming: {retryEx.Message}", "Warewolf Info");
                    }
                }

                InvokeOnUiThread(() => Messages.Add(ChatMessage.Create(ChatMessageType.Error,
                    "The token limit has been exceeded. The context is too large for the selected model. " +
                    "Please open Settings (click the link at the top of the chatbot to configure) and uncheck some system prompt options " +
                    "(System Log, Resources XAML, or Resources JSON) to reduce the context size.")));
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

        private List<ChatCompletionMessage> BuildChatMessages()
        {
            var chatMessages = new List<ChatCompletionMessage>();

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

            // Add conversation summary if older messages have been summarized
            if (!string.IsNullOrEmpty(_conversationSummary))
            {
                chatMessages.Add(new ChatCompletionMessage
                {
                    Role = "system",
                    Content = "Summary of earlier conversation:\n" + _conversationSummary
                });
            }

            // Add conversation history; skip System and Error messages
            // to maintain the required user/assistant alternating pattern
            foreach (var msg in Messages)
            {
                if (msg.Type == ChatMessageType.User)
                {
                    chatMessages.Add(new ChatCompletionMessage { Role = "user", Content = msg.Content });
                }
                else if (msg.Type == ChatMessageType.Bot)
                {
                    chatMessages.Add(new ChatCompletionMessage { Role = "assistant", Content = msg.Content });
                }
            }

            return chatMessages;
        }

        /// <summary>
        /// Applies a sliding window to conversation history when it exceeds the threshold.
        /// Summarizes the oldest messages into a condensed text and removes them from the collection.
        /// </summary>
        private void ApplySlidingWindow()
        {
            var conversationMessages = Messages
                .Where(m => m.Type == ChatMessageType.User || m.Type == ChatMessageType.Bot)
                .ToList();

            if (conversationMessages.Count < SummarizationThreshold)
            {
                return;
            }

            var messagesToSummarize = conversationMessages.Count - (MaxConversationMessages / 2);
            if (messagesToSummarize <= 0)
            {
                return;
            }

            var oldMessages = conversationMessages.Take(messagesToSummarize).ToList();

            // Build summary from old messages
            var summaryBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(_conversationSummary))
            {
                summaryBuilder.AppendLine(_conversationSummary);
                summaryBuilder.AppendLine();
            }

            foreach (var msg in oldMessages)
            {
                var prefix = msg.Type == ChatMessageType.User ? "User" : "Assistant";
                var truncatedContent = msg.Content.Length > 200
                    ? msg.Content.Substring(0, 200) + "..."
                    : msg.Content;
                summaryBuilder.AppendLine($"{prefix}: {truncatedContent}");
            }

            _conversationSummary = summaryBuilder.ToString().Trim();

            // Remove summarized messages from the collection
            foreach (var msg in oldMessages)
            {
                Messages.Remove(msg);
            }

            Dev2.Common.Dev2Logger.Info($"ChatbotContext: Summarized {oldMessages.Count} messages, {Messages.Count} remaining", "Warewolf Info");
        }

        /// <summary>
        /// Trims the oldest 50% of conversation messages as a recovery mechanism when token limits are hit.
        /// Returns true if messages were trimmed.
        /// </summary>
        private bool TrimConversationHistory()
        {
            var conversationMessages = Messages
                .Where(m => m.Type == ChatMessageType.User || m.Type == ChatMessageType.Bot)
                .ToList();

            if (conversationMessages.Count < 2)
            {
                return false;
            }

            var messagesToRemove = conversationMessages.Count / 2;
            var oldMessages = conversationMessages.Take(messagesToRemove).ToList();

            foreach (var msg in oldMessages)
            {
                Messages.Remove(msg);
            }

            // Clear any existing summary since we're doing emergency trimming
            _conversationSummary = null;

            Dev2.Common.Dev2Logger.Info($"ChatbotContext: Emergency trimmed {oldMessages.Count} messages due to token limit", "Warewolf Info");
            return true;
        }

        // --- Conversation History Management ---

        private void NewConversation()
        {
            SaveCurrentConversation();

            Messages.Clear();
            _conversationSummary = null;
            _currentConversation = ChatConversation.CreateNew();

            // Don't change SelectedConversation via property to avoid re-triggering save/load
            _selectedConversation = null;
            OnPropertyChanged(nameof(SelectedConversation));

            // Re-add the greeting message if system prompt is initialized
            if (_systemPromptInitialized)
            {
                Messages.Add(ChatMessage.Create(ChatMessageType.System,
                    "New conversation started. How can I help you?"));
            }
        }

        private void UpdateConversationTitle(string firstUserMessage)
        {
            if (_currentConversation == null)
            {
                _currentConversation = ChatConversation.CreateNew();
            }

            if (_currentConversation.Title == "New Chat" && !string.IsNullOrWhiteSpace(firstUserMessage))
            {
                _currentConversation.Title = firstUserMessage.Length > MaxConversationTitleLength
                    ? firstUserMessage.Substring(0, MaxConversationTitleLength) + "..."
                    : firstUserMessage;
            }
        }

        private void SaveCurrentConversation()
        {
            if (_currentConversation == null)
            {
                return;
            }

            // Only save if there are user messages
            var hasUserMessages = Messages.Any(m => m.Type == ChatMessageType.User);
            if (!hasUserMessages)
            {
                return;
            }

            try
            {
                _currentConversation.Messages = Messages.ToList();
                _currentConversation.ConversationSummary = _conversationSummary;
                _currentConversation.LastMessageAt = DateTime.Now;

                Directory.CreateDirectory(ChatHistoryFolder);

                var filePath = Path.Combine(ChatHistoryFolder, _currentConversation.Id + ".json");
                var json = JsonConvert.SerializeObject(_currentConversation, Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                // Update or add to saved conversations list
                var existing = SavedConversations.FirstOrDefault(c => c.Id == _currentConversation.Id);
                if (existing != null)
                {
                    var index = SavedConversations.IndexOf(existing);
                    SavedConversations[index] = _currentConversation;
                }
                else
                {
                    SavedConversations.Insert(0, _currentConversation);
                }

                Dev2.Common.Dev2Logger.Info($"ChatbotContext: Saved conversation '{_currentConversation.Title}' ({_currentConversation.Id})", "Warewolf Info");
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("ChatbotContext: Error saving conversation", ex, "Warewolf Error");
            }
        }

        private void LoadConversation(ChatConversation conversation)
        {
            try
            {
                var filePath = Path.Combine(ChatHistoryFolder, conversation.Id + ".json");
                if (!File.Exists(filePath))
                {
                    Dev2.Common.Dev2Logger.Warn($"ChatbotContext: Conversation file not found: {filePath}", "Warewolf Info");
                    return;
                }

                var json = File.ReadAllText(filePath, Encoding.UTF8);
                var loaded = JsonConvert.DeserializeObject<ChatConversation>(json);

                if (loaded == null)
                {
                    return;
                }

                Messages.Clear();
                foreach (var msg in loaded.Messages)
                {
                    Messages.Add(msg);
                }

                _conversationSummary = loaded.ConversationSummary;
                _currentConversation = loaded;

                Dev2.Common.Dev2Logger.Info($"ChatbotContext: Loaded conversation '{loaded.Title}' with {loaded.Messages.Count} messages", "Warewolf Info");
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error($"ChatbotContext: Error loading conversation {conversation.Id}", ex, "Warewolf Error");
            }
        }

        private void LoadSavedConversationsList()
        {
            SavedConversations = new ObservableCollection<ChatConversation>();

            try
            {
                if (!Directory.Exists(ChatHistoryFolder))
                {
                    return;
                }

                var files = Directory.GetFiles(ChatHistoryFolder, "*.json")
                    .OrderByDescending(f => File.GetLastWriteTime(f));

                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file, Encoding.UTF8);
                        var conversation = JsonConvert.DeserializeObject<ChatConversation>(json);
                        if (conversation != null)
                        {
                            // Don't load full messages into the list, just metadata
                            conversation.Messages = new List<ChatMessage>();
                            SavedConversations.Add(conversation);
                        }
                    }
                    catch (Exception ex)
                    {
                        Dev2.Common.Dev2Logger.Debug($"ChatbotContext: Skipping corrupt conversation file: {file}: {ex.Message}", "Warewolf Debug");
                    }
                }

                Dev2.Common.Dev2Logger.Info($"ChatbotContext: Loaded {SavedConversations.Count} saved conversations", "Warewolf Info");
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("ChatbotContext: Error loading saved conversations list", ex, "Warewolf Error");
            }
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

		/// <summary>
		/// Converts exception details into user-friendly error messages without exposing sensitive information.
		/// Full exception details are logged separately for debugging.
		/// </summary>
		private static string GetUserFriendlyErrorMessage(Exception ex)
		{
			// Check for specific exception types and provide appropriate user-friendly messages
			if (ex is HttpRequestException httpEx)
			{
				if (httpEx.Message.Contains("401") || httpEx.Message.ToLower().Contains("unauthorized"))
					return "Authentication failed. Please check your API key configuration.";
				if (httpEx.Message.Contains("403") || httpEx.Message.ToLower().Contains("forbidden"))
					return "Access forbidden. Please verify your API permissions.";
				if (httpEx.Message.Contains("429") || httpEx.Message.ToLower().Contains("rate limit"))
					return "Rate limit exceeded. Please wait a moment before trying again.";
				if (httpEx.Message.Contains("500") || httpEx.Message.Contains("502") || httpEx.Message.Contains("503"))
					return "The AI service is currently unavailable. Please try again later.";
				if (httpEx.Message.ToLower().Contains("timeout"))
					return "The request timed out. Please try again.";
				if (httpEx.Message.ToLower().Contains("network") || httpEx.Message.ToLower().Contains("connection"))
					return "Network connection error. Please check your internet connection.";
				return "Failed to communicate with the AI service. Please check your connection and try again.";
			}
			else if (ex is TaskCanceledException)
			{
				return "The request was cancelled or timed out. Please try again.";
			}
			else if (ex is SocketException)
			{
				return "Network connection error. Please check your internet connection.";
			}
			else if (ex is JsonException)
			{
				return "Failed to process the AI response. Please try again.";
			}
			else
			{
				// Generic error for any other exception type
				// Do not expose ex.Message as it may contain sensitive information
				return "An unexpected error occurred. Please try again or contact support if the problem persists.";
			}
		}

        private void ExportAsText()
        {
            try
            {
                var exportFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Warewolf", "ChatExports");
                Directory.CreateDirectory(exportFolder);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var title = _currentConversation?.Title ?? "Chat";
                // Sanitize title for filename
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    title = title.Replace(c, '_');
                }
                var filePath = Path.Combine(exportFolder, $"{title}_{timestamp}.txt");

                var sb = new StringBuilder();
                foreach (var msg in Messages)
                {
                    sb.AppendLine(msg.DisplayText);
                    sb.AppendLine();
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                Messages.Add(ChatMessage.Create(ChatMessageType.System,
                    $"Conversation exported to: {filePath}"));
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error exporting conversation as text", ex, "Warewolf Error");
                Messages.Add(ChatMessage.Create(ChatMessageType.Error,
                    "Failed to export conversation. Check the log for details."));
            }
        }

        private void ExportAsJson()
        {
            try
            {
                var exportFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Warewolf", "ChatExports");
                Directory.CreateDirectory(exportFolder);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var title = _currentConversation?.Title ?? "Chat";
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    title = title.Replace(c, '_');
                }
                var filePath = Path.Combine(exportFolder, $"{title}_{timestamp}.json");

                var exportData = new
                {
                    title = _currentConversation?.Title ?? "Chat",
                    exportedAt = DateTime.Now,
                    messages = Messages.Select(m => new
                    {
                        id = m.Id,
                        type = m.Type.ToString(),
                        content = m.Content,
                        timestamp = m.Timestamp
                    }).ToArray()
                };

                var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);

                Messages.Add(ChatMessage.Create(ChatMessageType.System,
                    $"Conversation exported to: {filePath}"));
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error exporting conversation as JSON", ex, "Warewolf Error");
                Messages.Add(ChatMessage.Create(ChatMessageType.Error,
                    "Failed to export conversation. Check the log for details."));
            }
        }

		public void Dispose()
        {
            if (!_disposed)
            {
                // Auto-save current conversation on dispose
                SaveCurrentConversation();

                _streamingCts?.Cancel();
                _streamingCts?.Dispose();
                _eventAggregator?.Unsubscribe(this);
                _disposed = true;
            }
        }
    }
}
