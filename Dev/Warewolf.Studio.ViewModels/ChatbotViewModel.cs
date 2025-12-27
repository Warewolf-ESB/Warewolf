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
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Warewolf.Configuration;

namespace Warewolf.Studio.ViewModels
{
    public class ChatbotViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase, Caliburn.Micro.IHandle<Warewolf.Data.ChatbotSettingsSavedMessage>
    {
        private readonly IServer _server;
        private readonly Caliburn.Micro.IEventAggregator _eventAggregator;
        private string _message;
        private ObservableCollection<string> _messages;
        private string _displayName;
        private bool _isChatbotConfigured;
        private bool _isSending;
        private ChatbotSource _configuredSource;
        private string _systemPrompt;
        private bool _systemPromptInitialized;
        private System.Collections.Generic.List<string> _availableModels;
        private string _selectedModel;
        private bool _isInitializingPrompt;
        private string _resourcesJson;
        private string _systemLog;
        private bool _includeSystemLog = true;
        private bool _includeResourcesXaml = true;
        private bool _includeResourcesJson = true;

        public ChatbotViewModel()
        {
            DisplayName = "Chatbot";
            Messages = new ObservableCollection<string>();
            SendCommand = new DelegateCommand(Send, CanSend);
        }

        public ChatbotViewModel(IServer server, ICommand openSettingsCommand)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            OpenSettingsCommand = openSettingsCommand ?? throw new ArgumentNullException(nameof(openSettingsCommand));

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
            
            LoadChatbotConfiguration();
            // Clear any existing system prompt so it gets regenerated
            _systemPromptInitialized = false;
            _systemPrompt = null;
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

                // Load checkbox settings
                _includeSystemLog = settingsData.IncludeSystemLog;
                _includeResourcesXaml = settingsData.IncludeResourcesXaml;
                _includeResourcesJson = settingsData.IncludeResourcesJson;

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
                    && !string.IsNullOrWhiteSpace(_configuredSource.CompletionsEndpoint)
                    && !string.IsNullOrWhiteSpace(_configuredSource.ApiKey);

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

        private async void FetchAvailableModels()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_configuredSource.ModelsEndpoint))
                {
                    // If no models endpoint, use a default model
                    _availableModels = new System.Collections.Generic.List<string> { "gpt-4o-mini" };
                    _selectedModel = "gpt-4o-mini";
                    return;
                }

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuredSource.ApiKey}");
                    
                    var response = await client.GetAsync(_configuredSource.ModelsEndpoint);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Dev2.Common.Dev2Logger.Warn($"Failed to fetch models: {response.StatusCode}", "Warewolf Info");
                        // Fallback to default
                        _availableModels = new System.Collections.Generic.List<string> { "gpt-4o-mini" };
                        _selectedModel = "gpt-4o-mini";
                        return;
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    
                    _availableModels = new System.Collections.Generic.List<string>();
                    
                    // Parse the models from the response
                    if (result?.data != null)
                    {
                        foreach (var model in result.data)
                        {
                            var modelId = model.id?.ToString();
                            if (!string.IsNullOrWhiteSpace(modelId))
                            {
                                _availableModels.Add(modelId);
                            }
                        }
                    }

                    // Select a good default model if available
                    if (_availableModels.Count > 0)
                    {
                        // Prefer gpt-4o-mini if available
                        if (_availableModels.Contains("gpt-4o-mini"))
                        {
                            _selectedModel = "gpt-4o-mini";
                        }
                        else if (_availableModels.Contains("gpt-4o"))
                        {
                            _selectedModel = "gpt-4o";
                        }
                        else if (_availableModels.Contains("gpt-3.5-turbo"))
                        {
                            _selectedModel = "gpt-3.5-turbo";
                        }
                        else
                        {
                            // Use the first available model
                            _selectedModel = _availableModels[0];
                        }

                        Dev2.Common.Dev2Logger.Info($"Fetched {_availableModels.Count} models from API. Selected: {_selectedModel}", "Warewolf Info");
                    }
                    else
                    {
                        // No models returned, use fallback
                        _availableModels = new System.Collections.Generic.List<string> { "gpt-4o-mini" };
                        _selectedModel = "gpt-4o-mini";
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error fetching available models", ex, "Warewolf Error");
                // Fallback to default model
                _availableModels = new System.Collections.Generic.List<string> { "gpt-4o-mini" };
                _selectedModel = "gpt-4o-mini";
            }
        }

        private void InitializeSystemPrompt()
        {
            if (_systemPromptInitialized)
            {
                return;
            }

            IsInitializingPrompt = true;

            // Run initialization asynchronously to not block the UI
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var promptBuilder = new StringBuilder();
                    promptBuilder.AppendLine("You are a Warewolf workflow debugging assistant. You are non-agentic and can only answer questions about the Warewolf resources and system logs provided to you.");
                    promptBuilder.AppendLine();
                    promptBuilder.AppendLine("## Your Capabilities:");
                    
                    var capabilities = new System.Collections.Generic.List<string>();
                    
                    if (_includeResourcesJson || _includeResourcesXaml)
                    {
                        capabilities.Add("- List and identify available workflow resources");
                        
                        if (_includeResourcesXaml)
                        {
                            capabilities.Add("- Analyze workflow XAML structure, activities, and data flow");
                            capabilities.Add("- Explain workflow logic and identify potential issues");
                            capabilities.Add("- Answer questions about workflow structure and dependencies");
                        }
                    }
                    
                    if (_includeSystemLog)
                    {
                        capabilities.Add("- Help debug issues using the system log");
                        capabilities.Add("- Identify errors and warnings in recent activity");
                        capabilities.Add("- Trace execution flow from log entries");
                    }
                    
                    if (capabilities.Count > 0)
                    {
                        promptBuilder.AppendLine(string.Join("\n", capabilities));
                    }
                    else
                    {
                        promptBuilder.AppendLine("- Answer general questions about Warewolf workflows");
                    }
                    
                    promptBuilder.AppendLine();
                    promptBuilder.AppendLine("## Important Rules:");
                    promptBuilder.AppendLine("- You can ONLY discuss the resources and logs provided below");
                    promptBuilder.AppendLine("- Do NOT provide information about resources not in this context");
                    promptBuilder.AppendLine("- Do NOT make assumptions about system behavior beyond what's in the logs");
                    promptBuilder.AppendLine("- If asked about something not in your context, politely explain you only have access to the provided resources and logs");
                    promptBuilder.AppendLine();

                    int resourceCount = 0;

                    // Get resources based on settings
                    if (_includeResourcesJson || _includeResourcesXaml)
                    {
                        _resourcesJson = GetWorkspaceResourcesAsJson(includeXaml: _includeResourcesXaml);
                        
                        if (!string.IsNullOrEmpty(_resourcesJson))
                        {
                            if (_includeResourcesXaml)
                            {
                                promptBuilder.AppendLine("## Workspace Resources (JSON with Workflow XAML):");
                                promptBuilder.AppendLine("Each workflow resource includes its XAML definition showing activities, connections, and data mappings.");
                            }
                            else
                            {
                                promptBuilder.AppendLine("## Workspace Resources (JSON - Metadata Only):");
                                promptBuilder.AppendLine("Resource names, types, and IDs are provided below.");
                            }
                            promptBuilder.AppendLine("```json");
                            promptBuilder.AppendLine(_resourcesJson);
                            promptBuilder.AppendLine("```");
                            promptBuilder.AppendLine();
                            
                            // Count resources
                            try
                            {
                                var resources = JsonConvert.DeserializeObject<System.Collections.Generic.List<object>>(_resourcesJson);
                                resourceCount = resources?.Count ?? 0;
                            }
                            catch (Exception ex)
                            {
                                Dev2.Common.Dev2Logger.Error("Error counting resources from JSON", ex, "Warewolf Error");
                            }
                        }
                    }

                    // Get system log based on settings
                    if (_includeSystemLog)
                    {
                        _systemLog = GetSystemLog();
                        if (!string.IsNullOrEmpty(_systemLog))
                        {
                            promptBuilder.AppendLine("## System Log (Recent Entries):");
                            promptBuilder.AppendLine("```");
                            promptBuilder.AppendLine(_systemLog);
                            promptBuilder.AppendLine("```");
                            promptBuilder.AppendLine();
                        }
                    }

                    _systemPrompt = promptBuilder.ToString();
                    _systemPromptInitialized = true;

                    // Update UI on the dispatcher thread
                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        IsInitializingPrompt = false;
                        if (resourceCount > 0 || _includeSystemLog)
                        {
                            var contextParts = new System.Collections.Generic.List<string>();
                            if (resourceCount > 0)
                            {
                                contextParts.Add($"{resourceCount} resource{(resourceCount == 1 ? "" : "s")}");
                            }
                            if (_includeSystemLog)
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
                    }));
                }
                catch (Exception ex)
                {
                    Dev2.Common.Dev2Logger.Error("Error initializing chatbot system prompt", ex, "Warewolf Error");
                    _systemPrompt = "You are a Warewolf workflow debugging assistant. Note: Workspace context could not be loaded.";
                    _systemPromptInitialized = true;

                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        IsInitializingPrompt = false;
                        Messages.Add("Chatbot: Hello! I'm your Warewolf debugging assistant. " +
                            "Note: I had trouble loading workspace context, but I can still help answer general questions.");
                    }));
                }
            });
        }

        private string GetWorkspaceResourcesAsJson(bool includeXaml = true)
        {
            try
            {
                // Ensure server connection is established
                if (_server?.Connection == null || !_server.Connection.IsConnected)
                {
                    Dev2.Common.Dev2Logger.Warn("Server connection not established, waiting for connection...", "Warewolf Info");
                    
                    // Wait up to 10 seconds for connection
                    var retries = 0;
                    while ((_server?.Connection == null || !_server.Connection.IsConnected) && retries < 20)
                    {
                        System.Threading.Thread.Sleep(500);
                        retries++;
                    }
                    
                    if (_server?.Connection == null || !_server.Connection.IsConnected)
                    {
                        Dev2.Common.Dev2Logger.Warn("Server connection not available after waiting", "Warewolf Info");
                        return null;
                    }
                }

                // Build the request payload manually
                var serializer = new Dev2JsonSerializer();
                var servicePayload = new Dev2.Communication.EsbExecuteRequest
                {
                    ServiceName = "FetchExplorerItemsService"
                };
                
                var toSend = serializer.SerializeToBuilder(servicePayload);
                
                // Execute the command and get raw response
                var rawPayload = _server.Connection.ExecuteCommand(toSend, _server.Connection.WorkspaceID);
                
                if (rawPayload == null || rawPayload.Length == 0)
                {
                    Dev2.Common.Dev2Logger.Warn("No response from FetchExplorerItemsService", "Warewolf Info");
                    return null;
                }

                string explorerItemsJson;
                
                try
                {
                    // Try to deserialize as CompressedExecuteMessage first
                    var compressedMessage = serializer.Deserialize<Dev2.Communication.CompressedExecuteMessage>(rawPayload);
                    if (compressedMessage != null && compressedMessage.IsCompressed)
                    {
                        explorerItemsJson = compressedMessage.GetDecompressedMessage().ToString();
                    }
                    else
                    {
                        // Not compressed, try as ExecuteMessage
                        var executeMessage = serializer.Deserialize<Dev2.Communication.ExecuteMessage>(rawPayload);
                        explorerItemsJson = executeMessage?.Message?.ToString();
                    }
                }
                catch
                {
                    // Last resort - use raw payload as string
                    explorerItemsJson = rawPayload.ToString();
                }

                if (string.IsNullOrEmpty(explorerItemsJson))
                {
                    Dev2.Common.Dev2Logger.Warn("No explorer items after decompression", "Warewolf Info");
                    return null;
                }

                // Parse the explorer items to extract resources
                dynamic explorerItems = JsonConvert.DeserializeObject(explorerItemsJson);
                
                var resourceList = new System.Collections.Generic.List<object>();
                
                // Recursively extract resources from explorer tree
                ExtractResourcesFromExplorerItem(explorerItems, resourceList, includeXaml);
                
                if (resourceList.Count == 0)
                {
                    Dev2.Common.Dev2Logger.Warn("No resources extracted from explorer items", "Warewolf Info");
                    return null;
                }

                Dev2.Common.Dev2Logger.Info($"Loading {resourceList.Count} resources into chatbot context (includeXaml: {includeXaml})", "Warewolf Info");

                // Serialize to JSON with formatting
                var json = JsonConvert.SerializeObject(resourceList, Formatting.Indented);
                
                // Limit size to avoid token limits (approximately 100KB of JSON to allow for XAML)
                if (json.Length > 100000)
                {
                    json = json.Substring(0, 100000) + "\n... (truncated for size)";
                }

                return json;
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error getting workspace resources", ex, "Warewolf Error");
                return null;
            }
        }

        private void ExtractResourcesFromExplorerItem(dynamic item, System.Collections.Generic.List<object> resourceList, bool includeXaml = true)
        {
            try
            {
                // Check if this item is a resource (not a folder)
                if (item.ResourceType != null && item.ResourceType.ToString() != "Folder")
                {
                    var resourceId = item.ResourceId?.ToString();
                    var resourceName = item.DisplayName?.ToString();
                    var resourceType = item.ResourceType?.ToString();
                    
                    if (!string.IsNullOrEmpty(resourceId) && resourceId != "00000000-0000-0000-0000-000000000000")
                    {
                        // Fetch the resource XAML if it's a workflow and we want XAML
                        string xaml = null;
                        if (includeXaml && (resourceType == "WorkflowService" || resourceType == "Service"))
                        {
                            xaml = FetchResourceXaml(resourceId);
                        }

                        var resourceInfo = new
                        {
                            id = resourceId,
                            name = resourceName,
                            type = resourceType,
                            xaml = xaml
                        };
                        
                        resourceList.Add(resourceInfo);
                    }
                }

                // Recursively process children
                if (item.Children != null)
                {
                    foreach (var child in item.Children)
                    {
                        ExtractResourcesFromExplorerItem(child, resourceList, includeXaml);
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error($"Error extracting resource from explorer item", ex, "Warewolf Error");
            }
        }

        private string FetchResourceXaml(string resourceId)
        {
            try
            {
                if (!Guid.TryParse(resourceId, out Guid guid))
                {
                    return null;
                }

                var comsController = new Dev2.Controller.CommunicationController 
                { 
                    ServiceName = "FetchResourceDefinitionService" 
                };
                
                comsController.AddPayloadArgument("ResourceID", new StringBuilder(resourceId));
                
                var result = comsController.ExecuteCommand<Dev2.Communication.ExecuteMessage>(
                    _server.Connection, 
                    _server.Connection.WorkspaceID);
                
                if (result == null || result.HasError)
                {
                    return null;
                }

                var xaml = result.Message?.ToString();
                
                // Limit XAML size per resource to avoid excessive data (max 5KB per workflow)
                // This prevents token limit issues and keeps the context manageable
                if (!string.IsNullOrEmpty(xaml) && xaml.Length > 5000)
                {
                    // Return null for large XAML files to avoid bloating the context
                    // The chatbot can still see resource names and types without the full XAML
                    return null;
                }

                return xaml;
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error($"Error fetching XAML for resource {resourceId}", ex, "Warewolf Error");
                return null;
            }
        }

        private string GetSystemLog()
        {
            try
            {
                // Use the communication controller to fetch server log via SignalR
                var comsController = new Dev2.Controller.CommunicationController 
                { 
                    ServiceName = "FetchCurrentServerLogService" 
                };
                
                var result = comsController.ExecuteCommand<Dev2.Communication.ExecuteMessage>(
                    _server.Connection, 
                    _server.Connection.WorkspaceID);
                
                if (result == null || result.HasError)
                {
                    var errorMsg = result?.Message?.ToString() ?? "Failed to fetch server log";
                    Dev2.Common.Dev2Logger.Warn($"Failed to fetch server log: {errorMsg}", "Warewolf Info");
                    return "Unable to fetch server log: " + errorMsg;
                }

                var logContent = result.Message?.ToString();
                
                if (string.IsNullOrEmpty(logContent))
                {
                    return "No log data available.";
                }
                
                // Limit size to avoid token limits (approximately 20KB of log)
                if (logContent.Length > 20000)
                {
                    logContent = "... (earlier entries truncated)\n" + logContent.Substring(logContent.Length - 20000);
                }

                return logContent;
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error getting system log via API", ex, "Warewolf Error");
                return "Error reading system log: " + ex.Message;
            }
        }

        private bool CanSend()
        {
            return IsChatbotConfigured && !string.IsNullOrWhiteSpace(Message) && !IsSending;
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
                return await CallChatbotApiWithContextAsync(userMessage);
            }
            catch (HttpRequestException ex) when (IsTokenLimitError(ex))
            {
                Dev2.Common.Dev2Logger.Warn($"Token limit reached: {ex.Message}", "Warewolf Info");
                
                // Return a helpful error message with a link to settings
                return "The token limit has been exceeded. The context is too large for the selected model. " +
                       "Please open Settings (click the link at the top of the chatbot to configure) and uncheck some system prompt options " +
                       "(System Log, Resources XAML, or Resources JSON) to reduce the context size.";
            }
        }

        private bool IsTokenLimitError(HttpRequestException ex)
        {
            if (ex.Message == null)
            {
                return false;
            }

            var message = ex.Message.ToLower();
            return message.Contains("token") && (message.Contains("limit") || message.Contains("exceeded") || message.Contains("maximum"))
                || message.Contains("413") // Payload too large
                || message.Contains("context_length_exceeded");
        }

        private async Task<string> CallChatbotApiWithContextAsync(string userMessage)
        {
            // Try with default Bearer authentication first
            try
            {
                return await CallChatbotApiWithAuthAsync(userMessage, "Authorization", "Bearer ", null);
            }
            catch (HttpRequestException ex) when (IsAuthenticationError(ex))
            {
                Dev2.Common.Dev2Logger.Info("Bearer authentication failed, retrying with x-api-key authentication", "Warewolf Info");
                
                // Retry with Claude-style authentication (x-api-key header + anthropic-version)
                try
                {
                    return await CallChatbotApiWithAuthAsync(userMessage, "x-api-key", "", "anthropic-version=2023-06-01");
                }
                catch (HttpRequestException ex2)
                {
                    // If both fail, throw the original error
                    throw new HttpRequestException($"Authentication failed with both Bearer and x-api-key methods. Original error: {ex.Message}", ex);
                }
            }
        }

        private bool IsAuthenticationError(HttpRequestException ex)
        {
            if (ex.Message == null)
            {
                return false;
            }

            var message = ex.Message.ToLower();
            return message.Contains("401") || message.Contains("unauthorized") || 
                   message.Contains("403") || message.Contains("forbidden") ||
                   message.Contains("authentication") || message.Contains("invalid") && (message.Contains("key") || message.Contains("token"));
        }

        private async Task<string> CallChatbotApiWithAuthAsync(string userMessage, string authHeaderName, string authHeaderPrefix, string additionalHeaders)
        {
            using (var client = new HttpClient())
            {
                // Set authentication header
                client.DefaultRequestHeaders.Add(authHeaderName, authHeaderPrefix + _configuredSource.ApiKey);

                // Add any additional headers if specified
                if (!string.IsNullOrWhiteSpace(additionalHeaders))
                {
                    var headerPairs = additionalHeaders.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var headerPair in headerPairs)
                    {
                        var parts = headerPair.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var headerName = parts[0].Trim();
                            var headerValue = parts[1].Trim();
                            if (!string.IsNullOrWhiteSpace(headerName) && !string.IsNullOrWhiteSpace(headerValue))
                            {
                                client.DefaultRequestHeaders.Add(headerName, headerValue);
                            }
                        }
                    }
                }

                // Build messages array with system prompt and full conversation history
                var messages = new System.Collections.Generic.List<object>();

                // Use the initialized system prompt
                if (!string.IsNullOrEmpty(_systemPrompt))
                {
                    messages.Add(new { role = "system", content = _systemPrompt });
                }

                // Add entire conversation history
                foreach (var msg in Messages)
                {
                    if (msg.StartsWith("You: "))
                    {
                        messages.Add(new { role = "user", content = msg.Substring(5) });
                    }
                    else if (msg.StartsWith("Bot: "))
                    {
                        messages.Add(new { role = "assistant", content = msg.Substring(5) });
                    }
                    else if (msg.StartsWith("Chatbot: "))
                    {
                        // This is the initial greeting, include as assistant message
                        messages.Add(new { role = "assistant", content = msg.Substring(9) });
                    }
                    // Skip error messages and other system messages
                }

                // Add the current user message
                messages.Add(new { role = "user", content = userMessage });

                // Use the selected model
                var modelToUse = !string.IsNullOrEmpty(_selectedModel) ? _selectedModel : "gpt-4o-mini";

                // Try with max_completion_tokens first (newer API standard)
                var payload = CreatePayload(modelToUse, messages.ToArray(), useMaxCompletionTokens: true, includeTemperature: true);
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_configuredSource.CompletionsEndpoint, content);
                
                // If we get an error about unsupported parameters, retry with different combinations
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    // Check if max_completion_tokens is not supported
                    if (errorContent.Contains("max_completion_tokens") && errorContent.Contains("not supported"))
                    {
                        Dev2.Common.Dev2Logger.Info("Retrying with max_tokens instead of max_completion_tokens", "Warewolf Info");
                        
                        // Retry with max_tokens
                        payload = CreatePayload(modelToUse, messages.ToArray(), useMaxCompletionTokens: false, includeTemperature: true);
                        json = JsonConvert.SerializeObject(payload);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PostAsync(_configuredSource.CompletionsEndpoint, content);
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            errorContent = await response.Content.ReadAsStringAsync();
                        }
                    }
                    
                    // Check if temperature is not supported
                    if (!response.IsSuccessStatusCode && errorContent.Contains("temperature") && errorContent.Contains("not support"))
                    {
                        Dev2.Common.Dev2Logger.Info("Retrying without temperature parameter", "Warewolf Info");
                        
                        // Determine which token parameter worked (or try max_completion_tokens by default)
                        var useMaxCompletionTokensParam = !errorContent.Contains("max_tokens");
                        
                        // Retry without temperature
                        payload = CreatePayload(modelToUse, messages.ToArray(), useMaxCompletionTokens: useMaxCompletionTokensParam, includeTemperature: false);
                        json = JsonConvert.SerializeObject(payload);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await client.PostAsync(_configuredSource.CompletionsEndpoint, content);
                    }
                }
                
                // Get the response content for better error messages
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    Dev2.Common.Dev2Logger.Error($"Chatbot API Error: {response.StatusCode} - {responseContent}", "Warewolf Error");
                    throw new HttpRequestException($"API returned {response.StatusCode}: {responseContent}");
                }

                dynamic result = JsonConvert.DeserializeObject(responseContent);

                // Try OpenAI-compatible response format first
                try
                {
                    return result.choices[0].message.content.ToString();
                }
                catch
                {
                    // Try Claude response format: { "content": [{ "type": "text", "text": "..." }] }
                    if (result.content != null && result.content.Count > 0)
                    {
                        return result.content[0].text.ToString();
                    }
                    throw new HttpRequestException("Unexpected API response format");
                }
            }
        }

        private object CreatePayload(string model, object[] messages, bool useMaxCompletionTokens, bool includeTemperature = true)
        {
            if (useMaxCompletionTokens)
            {
                if (includeTemperature)
                {
                    return new
                    {
                        model = model,
                        messages = messages,
                        temperature = 0.7,
                        max_completion_tokens = 2000
                    };
                }
                else
                {
                    return new
                    {
                        model = model,
                        messages = messages,
                        max_completion_tokens = 2000
                    };
                }
            }
            else
            {
                if (includeTemperature)
                {
                    return new
                    {
                        model = model,
                        messages = messages,
                        temperature = 0.7,
                        max_tokens = 2000
                    };
                }
                else
                {
                    return new
                    {
                        model = model,
                        messages = messages,
                        max_tokens = 2000
                    };
                }
            }
        }
    }
}
