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
#if NETFRAMEWORK
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
#else
using Prism.Commands;
using Dev2.Common;
using Prism.Mvvm;
#endif
using Newtonsoft.Json;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Warewolf.Configuration;

namespace Warewolf.Studio.ViewModels
{
#if NETFRAMEWORK
    public class ChatbotViewModel : Microsoft.Practices.Prism.Mvvm.BindableBase
#else
	public class ChatbotViewModel : BindableBase2
#endif
	{
		private readonly IServer _server;
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
		private readonly Caliburn.Micro.IEventAggregator _eventAggregator;

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
                    promptBuilder.AppendLine("- Analyze workflow resources and their XAML structure");
                    promptBuilder.AppendLine("- Help debug issues using the system log");
                    promptBuilder.AppendLine("- Explain workflow logic, activities, and data flow");
                    promptBuilder.AppendLine("- Identify potential issues in workflows");
                    promptBuilder.AppendLine("- Answer questions about workflow structure and dependencies");
                    promptBuilder.AppendLine();
                    promptBuilder.AppendLine("## Important Rules:");
                    promptBuilder.AppendLine("- You can ONLY discuss the resources and logs provided below");
                    promptBuilder.AppendLine("- Do NOT provide information about resources not in this context");
                    promptBuilder.AppendLine("- Do NOT make assumptions about system behavior beyond what's in the logs");
                    promptBuilder.AppendLine("- If asked about something not in your context, politely explain you only have access to the provided resources and logs");
                    promptBuilder.AppendLine("- When analyzing workflows, refer to the XAML structure provided");
                    promptBuilder.AppendLine();

                    // Get all resources as X6 JSON with XAML
                    _resourcesJson = GetWorkspaceResourcesAsJson();
                    if (!string.IsNullOrEmpty(_resourcesJson))
                    {
                        promptBuilder.AppendLine("## Workspace Resources (JSON with Workflow XAML):");
                        promptBuilder.AppendLine("Each workflow resource includes its XAML definition showing activities, connections, and data mappings.");
                        promptBuilder.AppendLine("```json");
                        promptBuilder.AppendLine(_resourcesJson);
                        promptBuilder.AppendLine("```");
                        promptBuilder.AppendLine();
                    }

                    // Get system log
                    _systemLog = GetSystemLog();
                    if (!string.IsNullOrEmpty(_systemLog))
                    {
                        promptBuilder.AppendLine("## System Log (Recent Entries):");
                        promptBuilder.AppendLine("```");
                        promptBuilder.AppendLine(_systemLog);
                        promptBuilder.AppendLine("```");
                        promptBuilder.AppendLine();
                    }

                    _systemPrompt = promptBuilder.ToString();
                    _systemPromptInitialized = true;

                    // Count actual resources loaded - use the resourceList that was already built
                    var resourceCount = 0;
                    if (!string.IsNullOrEmpty(_resourcesJson))
                    {
                        // Parse the JSON we built (which is an array of resources)
                        try
                        {
                            var resources = JsonConvert.DeserializeObject<System.Collections.Generic.List<object>>(_resourcesJson);
                            resourceCount = resources?.Count ?? 0;
                        }
                        catch (Exception ex)
                        {
                            Dev2.Common.Dev2Logger.Error("Error counting resources from JSON", ex, "Warewolf Error");
                            resourceCount = 0;
                        }
                    }

                    // Update UI on the dispatcher thread
                    System.Windows.Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        IsInitializingPrompt = false;
                        if (resourceCount > 0)
                        {
                            Messages.Add($"Chatbot: Hello! I'm your Warewolf debugging assistant. I have analyzed your workspace and loaded " +
                                $"{resourceCount} resource{(resourceCount == 1 ? "" : "s")} and recent system logs. " +
                                "I can help you understand your workflows, debug issues, and answer questions about your Warewolf environment. " +
                                "What would you like to know?");
                        }
                        else
                        {
                            Messages.Add("Chatbot: Hello! I'm your Warewolf debugging assistant. " +
                                "Note: No resources were found in the workspace. I can still help with general questions, " +
                                "but I won't have specific workflow context available.");
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
            // Try with full context first
            try
            {
                return await CallChatbotApiWithContextAsync(userMessage, includeXaml: true, includeResources: true);
            }
            catch (HttpRequestException ex) when (IsTokenLimitError(ex))
            {
                Dev2.Common.Dev2Logger.Warn("Token limit reached with full context, retrying without XAML", "Warewolf Info");
                
                // Retry without XAML
                try
                {
                    return await CallChatbotApiWithContextAsync(userMessage, includeXaml: false, includeResources: true);
                }
                catch (HttpRequestException ex2) when (IsTokenLimitError(ex2))
                {
                    Dev2.Common.Dev2Logger.Warn("Token limit reached without XAML, retrying with only logs", "Warewolf Info");
                    
                    // Retry with only logs
                    return await CallChatbotApiWithContextAsync(userMessage, includeXaml: false, includeResources: false);
                }
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

        private async Task<string> CallChatbotApiWithContextAsync(string userMessage, bool includeXaml, bool includeResources)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuredSource.ApiKey}");

                // Build messages array with system prompt and full conversation history
                var messages = new System.Collections.Generic.List<object>();

                // Build system prompt based on context level
                var systemPrompt = BuildSystemPrompt(includeXaml, includeResources);
                if (!string.IsNullOrEmpty(systemPrompt))
                {
                    messages.Add(new { role = "system", content = systemPrompt });
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

                return result.choices[0].message.content.ToString();
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

        private string BuildSystemPrompt(bool includeXaml, bool includeResources)
        {
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are a Warewolf workflow debugging assistant. You are non-agentic and can only answer questions about the Warewolf resources and system logs provided to you.");
            promptBuilder.AppendLine();

            // Build context availability notice based on what's included
            var hasResources = includeResources && !string.IsNullOrEmpty(_resourcesJson);
            var hasLog = !string.IsNullOrEmpty(_systemLog);

            if (!includeXaml && hasResources)
            {
                promptBuilder.AppendLine("## ?? IMPORTANT CONTEXT LIMITATION:");
                promptBuilder.AppendLine("Due to token/context size constraints, workflow XAML details have been REMOVED from this conversation.");
                promptBuilder.AppendLine("You can see resource names and types, but CANNOT analyze workflow internals, activities, or data flow.");
                promptBuilder.AppendLine("If asked about workflow implementation details, explain this limitation clearly.");
                promptBuilder.AppendLine();
            }
            else if (!includeResources && hasLog)
            {
                promptBuilder.AppendLine("## ?? IMPORTANT CONTEXT LIMITATION:");
                promptBuilder.AppendLine("Due to token/context size constraints, workspace resources have been REMOVED from this conversation.");
                promptBuilder.AppendLine("You can ONLY analyze the system log. You CANNOT answer questions about specific workflows or resources.");
                promptBuilder.AppendLine("If asked about workflows or resources, explain this limitation clearly and focus on log analysis.");
                promptBuilder.AppendLine();
            }

            promptBuilder.AppendLine("## Your Capabilities:");
            if (includeResources && includeXaml)
            {
                promptBuilder.AppendLine("- Analyze workflow resources and their XAML structure");
                promptBuilder.AppendLine("- Explain workflow logic, activities, and data flow");
                promptBuilder.AppendLine("- Identify potential issues in workflows");
                promptBuilder.AppendLine("- Answer questions about workflow structure and dependencies");
            }
            else if (includeResources && !includeXaml)
            {
                promptBuilder.AppendLine("- List available workflow resources by name and type");
                promptBuilder.AppendLine("- Provide general information about resource organization");
                promptBuilder.AppendLine("- CANNOT analyze workflow internals without XAML");
            }
            
            if (hasLog)
            {
                promptBuilder.AppendLine("- Help debug issues using the system log");
                promptBuilder.AppendLine("- Identify errors and warnings in recent activity");
                promptBuilder.AppendLine("- Trace execution flow from log entries");
            }
            promptBuilder.AppendLine();

            promptBuilder.AppendLine("## Important Rules:");
            promptBuilder.AppendLine("- You can ONLY discuss the resources and logs provided below");
            promptBuilder.AppendLine("- Do NOT provide information about resources not in this context");
            promptBuilder.AppendLine("- Do NOT make assumptions about system behavior beyond what's in the logs");
            if (!includeXaml && hasResources)
            {
                promptBuilder.AppendLine("- Do NOT attempt to answer questions about workflow implementation details (no XAML available)");
                promptBuilder.AppendLine("- Do NOT guess at workflow logic or activities");
            }
            if (!includeResources && hasLog)
            {
                promptBuilder.AppendLine("- Do NOT attempt to answer questions about specific workflows (no resource data available)");
                promptBuilder.AppendLine("- Focus exclusively on system log analysis");
            }
            promptBuilder.AppendLine("- If asked about something not in your context, politely explain you only have access to the provided resources and logs");
            if (includeXaml)
            {
                promptBuilder.AppendLine("- When analyzing workflows, refer to the XAML structure provided");
            }
            promptBuilder.AppendLine();

            if (includeResources)
            {
                // Get resources without XAML if needed
                var resourcesJson = includeXaml ? _resourcesJson : GetWorkspaceResourcesAsJson(includeXaml: false);
                
                if (!string.IsNullOrEmpty(resourcesJson))
                {
                    if (includeXaml)
                    {
                        promptBuilder.AppendLine("## Workspace Resources (JSON with Workflow XAML):");
                        promptBuilder.AppendLine("Each workflow resource includes its XAML definition showing activities, connections, and data mappings.");
                    }
                    else
                    {
                        promptBuilder.AppendLine("## Workspace Resources (JSON - Names and Types Only):");
                        promptBuilder.AppendLine("?? XAML workflow definitions OMITTED due to context size constraints.");
                        promptBuilder.AppendLine("You can see what resources exist but cannot analyze their internal implementation.");
                    }
                    promptBuilder.AppendLine("```json");
                    promptBuilder.AppendLine(resourcesJson);
                    promptBuilder.AppendLine("```");
                    promptBuilder.AppendLine();
                }
            }
            else if (!includeResources && hasLog)
            {
                promptBuilder.AppendLine("## ?? NO RESOURCE DATA AVAILABLE");
                promptBuilder.AppendLine("Workspace resources have been omitted. You can only work with the system log below.");
                promptBuilder.AppendLine();
            }

            if (!string.IsNullOrEmpty(_systemLog))
            {
                if (!includeResources)
                {
                    promptBuilder.AppendLine("## System Log (Recent Entries - PRIMARY CONTEXT):");
                    promptBuilder.AppendLine("This is your ONLY available context. Focus all analysis on these log entries.");
                }
                else
                {
                    promptBuilder.AppendLine("## System Log (Recent Entries):");
                }
                promptBuilder.AppendLine("```");
                promptBuilder.AppendLine(_systemLog);
                promptBuilder.AppendLine("```");
                promptBuilder.AppendLine();
            }

            return promptBuilder.ToString();
        }
    }
}
