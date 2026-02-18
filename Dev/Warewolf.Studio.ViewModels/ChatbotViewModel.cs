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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Communication;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
		private DateTime _lastSendTime = DateTime.MinValue;
		private string _message;
        private ObservableCollection<ChatMessage> _messages;
        private string _displayName;
        private bool _isChatbotConfigured;
        private bool _isSending;
        private string _loadingStatusText;
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
            DisplayName = "Chatbot";
            Messages = new ObservableCollection<ChatMessage>();
            SavedConversations = new ObservableCollection<ChatConversation>();
            SendCommand = new DelegateCommand(Send, CanSend);
            NewConversationCommand = new DelegateCommand(NewConversation);
            ExportAsTextCommand = new DelegateCommand(ExportAsText, () => Messages.Count > 0);
            ExportAsJsonCommand = new DelegateCommand(ExportAsJson, () => Messages.Count > 0);
        }

        public ChatbotViewModel(IServer server, ICommand openSettingsCommand)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            OpenSettingsCommand = openSettingsCommand ?? throw new ArgumentNullException(nameof(openSettingsCommand));

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
            if (message?.ResourceToRemove != null)
            {
                RefreshConfiguration();
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

        public bool IsLoading => IsSending;

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

            LoadChatbotConfiguration();
        }

        private void LoadChatbotConfiguration()
        {
            try
            {
                if (_server?.Connection == null || !_server.Connection.IsConnected)
                {
                    IsChatbotConfigured = false;
                    return;
                }

                // Configuration validation is delegated to the server.
                // The server reads Config.Chatbot.Get() on each request and returns an error
                // if not configured. We mark as configured if the server is connected,
                // and let the first send reveal any configuration issues.
                IsChatbotConfigured = true;

                // Show greeting
                InvokeOnUiThread(() =>
                {
                    Messages.Add(ChatMessage.Create(ChatMessageType.System,
                        "Hello! I'm your Warewolf debugging assistant. " +
                        "How can I help you today?"));

                    _currentConversation = ChatConversation.CreateNew();
                });
            }
            catch
            {
                IsChatbotConfigured = false;
            }
        }

        private static void InvokeOnUiThread(Action action)
		{
#if WINDOWS || NETFRAMEWORK
			System.Windows.Application.Current?.Dispatcher?.BeginInvoke(action);
#endif
		}

		private bool CanSend()
		{
			// Check basic conditions
			if (!IsChatbotConfigured || string.IsNullOrWhiteSpace(Message) || IsSending)
			{
				return false;
			}

			// Require server connection — chatbot has no offline fallback
			if (_server?.Connection == null || !_server.Connection.IsConnected)
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

				await SendViaServerAsync(userMessage);
			}
			catch (Exception ex)
			{
				// Log full exception details for debugging
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

        private async Task SendViaServerAsync(string userMessage)
        {
            // Create a placeholder bot message
            var botMessage = ChatMessage.Create(ChatMessageType.Bot, "");
            InvokeOnUiThread(() => Messages.Add(botMessage));

            _streamingCts = new CancellationTokenSource();

            var fullResponse = await SendMessageViaServerAsync(userMessage);
            InvokeOnUiThread(() => botMessage.AppendContent(fullResponse));
        }

        /// <summary>
        /// Sends the message to the server-side SendChatbotMessage management service and returns the AI response.
        /// The server is responsible for building context (system prompt, resources, logs) and calling the AI provider.
        /// </summary>
        private async Task<string> SendMessageViaServerAsync(string userMessage)
        {
            // Build conversation history payload
            var conversationHistory = Messages
                .Where(m => m.Type == ChatMessageType.User || m.Type == ChatMessageType.Bot)
                .Select(m => new
                {
                    type = m.Type == ChatMessageType.User ? "user" : "bot",
                    content = m.Content,
                    timestamp = DateTime.UtcNow.ToString("o")
                })
                .ToList();

            var serializer = new Dev2JsonSerializer();

            // Prepare ESB execute request
            var request = new Dev2.Communication.EsbExecuteRequest
            {
                ServiceName = "SendChatbotMessage"
            };

            request.AddArgument("Message", new StringBuilder(userMessage ?? string.Empty));
            request.AddArgument("ConversationHistory", new StringBuilder(JsonConvert.SerializeObject(conversationHistory)));

            // Serialize and execute on server
            var payload = serializer.SerializeToBuilder(request);

            var rawResponse = _server.Connection.ExecuteCommand(payload, _server.Connection.WorkspaceID);

            var responseText = rawResponse?.ToString();
            if (string.IsNullOrWhiteSpace(responseText))
            {
                throw new Exception("Empty response from server SendChatbotMessage service");
            }

            try
            {
                var obj = JObject.Parse(responseText);
                var error = obj["Error"]?.ToString();
                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                var response = obj["Response"]?.ToString();
                return response ?? string.Empty;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Invalid JSON from server SendChatbotMessage: {ex.Message}");
            }
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

            Messages.Add(ChatMessage.Create(ChatMessageType.System,
                "New conversation started. How can I help you?"));
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

		/// <summary>
		/// Converts exception details into user-friendly error messages without exposing sensitive information.
		/// Full exception details are logged separately for debugging.
		/// </summary>
		private static string GetUserFriendlyErrorMessage(Exception ex)
		{
			switch (ex)
			{
				default:
					// Generic error — server errors arrive as Exception with a message from the server response
					var msg = ex.Message ?? string.Empty;
					if (msg.Contains("401") || msg.ToLower().Contains("unauthorized"))
						return "Authentication failed. Please check your API key configuration.";
					if (msg.Contains("403") || msg.ToLower().Contains("forbidden"))
						return "Access forbidden. Please verify your API permissions.";
					if (msg.Contains("429") || msg.ToLower().Contains("rate limit"))
						return "Rate limit exceeded. Please wait a moment before trying again.";
					if (msg.Contains("500") || msg.Contains("502") || msg.Contains("503"))
						return "The AI service is currently unavailable. Please try again later.";
					if (msg.ToLower().Contains("timeout"))
						return "The request timed out. Please try again.";
					if (msg.ToLower().Contains("not configured") || msg.ToLower().Contains("chatbot source"))
						return msg; // Pass through server configuration errors as-is — they are user-actionable
					if (string.IsNullOrWhiteSpace(msg))
						return "An unexpected error occurred. Please try again or contact support if the problem persists.";
					return msg;
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

        public Task HandleAsync(ChatbotSettingsSavedMessage message, CancellationToken cancellationToken)
        {
            Handle(message);
            return Task.CompletedTask;
        }

        public Task HandleAsync(Dev2.Studio.Core.Messages.RemoveResourceAndCloseTabMessage message, CancellationToken cancellationToken)
        {
            Handle(message);
            return Task.CompletedTask;
        }
    }
}
