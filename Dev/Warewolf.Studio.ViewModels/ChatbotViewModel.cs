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
                ((DelegateCommand)SendCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand SendCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public void RefreshConfiguration()
        {
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
                    // Use the selected model from settings if available
                    if (!string.IsNullOrEmpty(_configuredSource.SelectedModel))
                    {
                        _selectedModel = _configuredSource.SelectedModel;
                        Dev2.Common.Dev2Logger.Info($"Using model from settings: {_selectedModel}", "Warewolf Info");
                    }
                    else
                    {
                        // Fallback: Fetch available models from the Models endpoint
                        FetchAvailableModels();
                    }
                    
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
                var resourcesJson = GetWorkspaceResourcesAsJson();
                if (!string.IsNullOrEmpty(resourcesJson))
                {
                    promptBuilder.AppendLine("## Workspace Resources (JSON with Workflow XAML):");
                    promptBuilder.AppendLine("Each workflow resource includes its XAML definition showing activities, connections, and data mappings.");
                    promptBuilder.AppendLine("```json");
                    promptBuilder.AppendLine(resourcesJson);
                    promptBuilder.AppendLine("```");
                    promptBuilder.AppendLine();
                }

                // Get system log
                var systemLog = GetSystemLog();
                if (!string.IsNullOrEmpty(systemLog))
                {
                    promptBuilder.AppendLine("## System Log (Recent Entries):");
                    promptBuilder.AppendLine("```");
                    promptBuilder.AppendLine(systemLog);
                    promptBuilder.AppendLine("```");
                    promptBuilder.AppendLine();
                }

                _systemPrompt = promptBuilder.ToString();
                _systemPromptInitialized = true;

                // Add a welcome message
                Messages.Clear();
                Messages.Add("Chatbot: Hello! I'm your Warewolf debugging assistant. I have analyzed your workspace and loaded " +
                    "all resources and recent system logs. I can help you understand your workflows, debug issues, and answer " +
                    "questions about your Warewolf environment. What would you like to know?");
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error("Error initializing chatbot system prompt", ex, "Warewolf Error");
                _systemPrompt = "You are a Warewolf workflow debugging assistant. Note: Workspace context could not be loaded.";
                _systemPromptInitialized = true;
            }
        }

        private string GetWorkspaceResourcesAsJson()
        {
            try
            {
                // Get all resources from the server using the resource repository
                var allResources = _server?.ResourceRepository?.All();
                if (allResources == null || allResources.Count == 0)
                {
                    return null;
                }

                var serializer = new Dev2JsonSerializer();
                var resourceList = new System.Collections.Generic.List<object>();

                foreach (var resource in allResources)
                {
                    var resourceInfo = new
                    {
                        id = resource.ID,
                        name = resource.ResourceName,
                        type = resource.ResourceType.ToString(),
                        category = resource.Category,
                        displayName = resource.DisplayName,
                        hasErrors = resource.HasErrors,
                        xaml = GetResourceXaml(resource)
                    };
                    resourceList.Add(resourceInfo);
                }

                if (resourceList.Count == 0)
                {
                    return null;
                }

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

        private string GetResourceXaml(Dev2.Studio.Interfaces.IResourceModel resource)
        {
            try
            {
                // Get the workflow XAML definition from WorkflowXaml property for workflows only
                var xamlBuilder = resource.WorkflowXaml;
                if (xamlBuilder == null)
                {
                    return null;
                }

                var xaml = xamlBuilder.ToString();
                
                // Only include if there's actual content
                if (string.IsNullOrWhiteSpace(xaml))
                {
                    return null;
                }
                
                // Limit XAML size per resource to avoid excessive data (max 10KB per workflow)
                if (xaml.Length > 10000)
                {
                    xaml = xaml.Substring(0, 10000) + "\n... (XAML truncated)";
                }

                return xaml;
            }
            catch (Exception ex)
            {
                Dev2.Common.Dev2Logger.Error($"Error getting XAML for resource {resource.ResourceName}", ex, "Warewolf Error");
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
                Messages.Add($"AI: {response}");
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
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuredSource.ApiKey}");

                // Build messages array with system prompt
                var messages = new System.Collections.Generic.List<object>();

                // Add system prompt if we have one
                if (!string.IsNullOrEmpty(_systemPrompt))
                {
                    messages.Add(new { role = "system", content = _systemPrompt });
                }

                // Add user message
                messages.Add(new { role = "user", content = userMessage });

                // Use the selected model from the fetched list, or fall back to default
                var modelToUse = !string.IsNullOrEmpty(_selectedModel) ? _selectedModel : "gpt-4o-mini";

                // Create the request payload with the model parameter
                var payload = new
                {
                    model = modelToUse,
                    messages = messages.ToArray(),
                    temperature = 0.7, // Moderate creativity
                    max_tokens = 2000 // Reasonable response length
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_configuredSource.CompletionsEndpoint, content);
                
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
    }
}
