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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Chatbot;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Security.Encryption;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Core;

namespace Dev2.Settings.Chatbot
{
    public class ChatbotSettingsViewModel : SettingsItemViewModel, IUpdatesHelp, IChatbotSettings
    {
        private readonly IResourceRepository _resourceRepository;
        IServer _currentEnvironment;
        private Guid _resourceSourceId;
        private ChatbotSettingsViewModel _item;
        private IChatbotSourceResource _selectedChatbotSource;
        private ICommand _newChatbotSourceCommand;
        private ICommand _editChatbotSourceCommand;
        private System.Collections.ObjectModel.ObservableCollection<ChatbotModelInfo> _availableModels;
        private ChatbotModelInfo _selectedModel;
        private bool _isFetchingModels;
        private bool _isInitialLoad;
        private bool _includeSystemLog = true;
        private bool _includeResourcesXaml = true;
        private bool _includeResourcesJson = true;

        [ExcludeFromCodeCoverage]
        public ChatbotSettingsViewModel()
        {
        }

        public ChatbotSettingsViewModel(IServer server)
        {
            CurrentEnvironment = server ?? throw new ArgumentNullException(nameof(server));
            _resourceRepository = CurrentEnvironment.ResourceRepository;

            var settingsData = CurrentEnvironment.ResourceRepository.GetChatbotSettings<ChatbotSettingsData>(CurrentEnvironment);
            
            // Load checkbox settings using properties to trigger property change notifications
            IncludeSystemLog = settingsData.IncludeSystemLog;
            IncludeResourcesXaml = settingsData.IncludeResourcesXaml;
            IncludeResourcesJson = settingsData.IncludeResourcesJson;
            
            Dev2Logger.Info($"ChatbotSettings: Loaded settings - IncludeSystemLog={IncludeSystemLog}, IncludeResourcesXaml={IncludeResourcesXaml}, IncludeResourcesJson={IncludeResourcesJson}", "Warewolf Info");
            
            if (settingsData.ChatbotSource != null)
            {
                var selectedSource = ChatbotSources.FirstOrDefault(o => o.ResourceID == settingsData.ChatbotSource.Value);
                _selectedChatbotSource = selectedSource;
                if (_selectedChatbotSource != null)
                {
                    _resourceSourceId = _selectedChatbotSource.ResourceID;

                    // Load the saved model from the encrypted payload
                    // The ChatbotSource from ChatbotSources doesn't have SelectedModel populated,
                    // because it's stored separately in the encrypted settings payload
                    if (!string.IsNullOrEmpty(settingsData.ChatbotSource.Payload))
                    {
                        try
                        {
                            var decryptedPayload = DpapiWrapper.Decrypt(settingsData.ChatbotSource.Payload);
                            var serializer = new Dev2JsonSerializer();
                            var savedSource = serializer.Deserialize<ChatbotSource>(decryptedPayload);
							// Copy the saved model to the selected source
							if (savedSource != null && !string.IsNullOrEmpty(savedSource.SelectedModel) && _selectedChatbotSource is ChatbotSource chatbotSource)
							{
								chatbotSource.SelectedModel = savedSource.SelectedModel;
							}

						}
                        catch (Exception ex)
                        {
                            Dev2Logger.Warn($"Could not load saved model from settings: {ex.Message}", "Warewolf Info");
                        }
                    }

                    // Fetch models after the source is set, so we can select the correct saved model
                    // Mark as initial load so baseline is set after models are loaded
                    _isInitialLoad = true;
                    FetchAvailableModels();
                }
			}

			_newChatbotSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(NewChatbotSource);
            _editChatbotSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(EditChatbotSource, CanEditChatbotSource);

            // Only set baseline here if no source is selected (no async model fetch pending)
            // Otherwise, baseline will be set after FetchAvailableModels completes
            if (!_isInitialLoad)
            {
                SetItem(this);
                IsDirty = false;
            }
            }
            
        public IServer CurrentEnvironment
        {
            private get => _currentEnvironment;
            set { _currentEnvironment = value; }
        }

        [JsonIgnore]
        public List<IChatbotSourceResource> ChatbotSources => LoadChatbotSources();

        private List<IChatbotSourceResource> LoadChatbotSources()
        {
            try
            {
                // Use the communication controller to fetch chatbot sources from the server
                var comsController = new Dev2.Controller.CommunicationController 
                { 
                    ServiceName = "FetchChatbotSources" 
                };
                
                var result = comsController.ExecuteCommand<ExecuteMessage>(
                    _currentEnvironment.Connection, 
                    GlobalConstants.ServerWorkspaceID);
                
                if (result != null && !result.HasError)
                {
                    var serializer = new Dev2JsonSerializer();
                    var chatbotSourcesJson = result.Message.ToString();
                    var chatbotSources = serializer.Deserialize<List<ChatbotSourceDefinition>>(chatbotSourcesJson);
                    
                    // Convert ChatbotSourceDefinition to ChatbotSource (which implements IChatbotSourceResource)
                    return chatbotSources
                        .Where(def => def != null)
                        .Select(def => new ChatbotSource
                        {
                            ResourceID = def.Id,
                            ResourceName = def.Name,
                            ApiKey = def.ApiKey,
                            CompletionsEndpoint = def.CompletionsEndpoint,
                            ModelsEndpoint = def.ModelsEndpoint,
                            SelectedModel = def.SelectedModel
                        } as IChatbotSourceResource)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Error loading chatbot sources", ex, "Warewolf Error");
            }
            
            return new List<IChatbotSourceResource>();
        }

        [JsonIgnore]
        public IChatbotSourceResource SelectedChatbotSource
        {
            get => _selectedChatbotSource;
            set
            {
                _selectedChatbotSource = value;
                if (_selectedChatbotSource != null)
                {
                    ResourceSourceId = _selectedChatbotSource.ResourceID;
                    // Fetch models when a source is selected
                    FetchAvailableModels();
                }
                else
                {
                    AvailableModels?.Clear();
                    SelectedModel = null;
                }

                OnPropertyChanged();
                ((Microsoft.Practices.Prism.Commands.DelegateCommand)_editChatbotSourceCommand)?.RaiseCanExecuteChanged();
                
                // Check if the value has changed from the saved state
                if (Item != null)
                {
                    IsDirty = !Equals(Item);
                }
            }
        }

        [JsonIgnore]
        public System.Collections.ObjectModel.ObservableCollection<ChatbotModelInfo> AvailableModels
        {
            get => _availableModels;
            set
            {
                _availableModels = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public ChatbotModelInfo SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();
                
                // Mark as dirty when model changes
                if (Item != null && _selectedModel != null)
                {
                    IsDirty = !Equals(Item);
                }
            }
        }

        [JsonIgnore]
        public bool IsFetchingModels
        {
            get => _isFetchingModels;
            set
            {
                _isFetchingModels = value;
                OnPropertyChanged();
            }
        }

        public bool IncludeSystemLog
        {
            get => _includeSystemLog;
            set
            {
                _includeSystemLog = value;
                OnPropertyChanged();
                Dev2Logger.Debug($"ChatbotSettings: IncludeSystemLog changed to {value}", "Warewolf Debug");
                if (Item != null)
                {
                    IsDirty = !Equals(Item);
                }
            }
        }

        public bool IncludeResourcesXaml
        {
            get => _includeResourcesXaml;
            set
            {
                _includeResourcesXaml = value;
                OnPropertyChanged();
                Dev2Logger.Debug($"ChatbotSettings: IncludeResourcesXaml changed to {value}", "Warewolf Debug");
                if (Item != null)
                {
                    IsDirty = !Equals(Item);
                }
            }
        }

        public bool IncludeResourcesJson
        {
            get => _includeResourcesJson;
            set
            {
                _includeResourcesJson = value;
                OnPropertyChanged();
                Dev2Logger.Debug($"ChatbotSettings: IncludeResourcesJson changed to {value}", "Warewolf Debug");
                if (Item != null)
                {
                    IsDirty = !Equals(Item);
                }
            }
        }

        private async void FetchAvailableModels()
        {
            if (_selectedChatbotSource == null)
            {
                SetBaselineIfInitialLoad();
                return;
            }

            var source = _selectedChatbotSource as ChatbotSource;
            if (source == null || string.IsNullOrWhiteSpace(source.ModelsEndpoint))
            {
                AvailableModels = new System.Collections.ObjectModel.ObservableCollection<ChatbotModelInfo>();
                SetBaselineIfInitialLoad();
                return;
            }

            IsFetchingModels = true;

            try
            {
                // Check if this is a Google Gemini endpoint
                if (IsGoogleGeminiEndpoint(source.ModelsEndpoint))
                {
                    Dev2Logger.Info("Detected Google Gemini endpoint for models, using query parameter authentication", "Warewolf Info");
                    await FetchModelsWithAuthAsync(source, "Authorization", "Bearer ", null);
                }
                // Try with default Bearer authentication first
                else
                {
                    try
                    {
                        await FetchModelsWithAuthAsync(source, "Authorization", "Bearer ", null);
                    }
                    catch (System.Net.Http.HttpRequestException ex) when (IsAuthenticationError(ex))
                    {
                        Dev2Logger.Info("Bearer authentication failed for models endpoint, retrying with x-api-key authentication", "Warewolf Info");
                        
                        // Retry with Claude-style authentication (x-api-key header + anthropic-version)
                        await FetchModelsWithAuthAsync(source, "x-api-key", "", "anthropic-version=2023-06-01");
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Error fetching available models", ex, "Warewolf Error");
                AvailableModels = new System.Collections.ObjectModel.ObservableCollection<ChatbotModelInfo>();
            }
            finally
            {
                IsFetchingModels = false;
                SetBaselineIfInitialLoad();
            }
        }

        private static bool IsAuthenticationError(System.Net.Http.HttpRequestException ex)
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

        private static bool IsGoogleGeminiEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return false;
            }

            var lowerEndpoint = endpoint.ToLower();
            return lowerEndpoint.Contains("generativelanguage.googleapis.com") || lowerEndpoint.Contains("gemini");
        }

        private async System.Threading.Tasks.Task FetchModelsWithAuthAsync(ChatbotSource source, string authHeaderName, string authHeaderPrefix, string additionalHeaders)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var modelsEndpoint = source.ModelsEndpoint;
                
                // Check if this is a Google Gemini endpoint
                var isGemini = IsGoogleGeminiEndpoint(modelsEndpoint);
                
                if (isGemini)
                {
                    // Google Gemini uses API key as a query parameter
                    var separator = modelsEndpoint.Contains("?") ? "&" : "?";
                    modelsEndpoint = $"{modelsEndpoint}{separator}key={source.ApiKey}";
                }
                else
                {
                    // Set authentication header for non-Gemini endpoints
                    client.DefaultRequestHeaders.Add(authHeaderName, authHeaderPrefix + source.ApiKey);
                }

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
                
                var response = await client.GetAsync(modelsEndpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new System.Net.Http.HttpRequestException($"API returned {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Try to deserialize based on different API response formats
                List<ChatbotModelInfo> models = null;
                try
                {
                    // Try Google Gemini format first (has a 'models' property)
                    var geminiWrapper = Newtonsoft.Json.JsonConvert.DeserializeObject<GeminiModelsResponseWrapper>(responseContent);
                    if (geminiWrapper?.Models != null && geminiWrapper.Models.Count > 0)
                    {
                        models = geminiWrapper.Models;
                    }
                }
                catch
                {
                    // Not Gemini format, continue trying other formats
                }
                
                if (models == null)
                {
                    try
                    {
                        // Try OpenAI/xAI format (has a 'data' property)
                        var wrapper = Newtonsoft.Json.JsonConvert.DeserializeObject<ModelsResponseWrapper>(responseContent);
                        models = wrapper?.Data;
                    }
                    catch
                    {
                        // If that fails, try to deserialize directly as an array
                        try
                        {
                            models = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ChatbotModelInfo>>(responseContent);
                        }
                        catch
                        {
                            // Failed all parsing attempts
                            var truncatedResponse = responseContent != null && responseContent.Length > 200 
                                ? responseContent.Substring(0, 200) 
                                : responseContent;
                            Dev2Logger.Warn($"Failed to parse models response. Response: {truncatedResponse}", "Warewolf Warn");
                        }
                    }
                }
                
                if (models != null && models.Count > 0)
                {
                    AvailableModels = new System.Collections.ObjectModel.ObservableCollection<ChatbotModelInfo>(models);
                    
                    // Try to select the previously saved model
                    if (!string.IsNullOrEmpty(source.SelectedModel))
                    {
                        var savedModel = AvailableModels.FirstOrDefault(m => m.EffectiveId == source.SelectedModel);
                        
                        if (savedModel != null)
                        {
                            SelectedModel = savedModel;
                        }
                        else
                        {
                            SelectedModel = AvailableModels.FirstOrDefault();
                        }
                    }
                    else
                    {
                        // Select a good default
                        SelectedModel = AvailableModels.FirstOrDefault(m => m.EffectiveId.Contains("gpt-4o-mini")) 
                                     ?? AvailableModels.FirstOrDefault(m => m.EffectiveId.Contains("gpt-4o"))
                                     ?? AvailableModels.FirstOrDefault(m => m.EffectiveId.Contains("gemini-pro"))
                                     ?? AvailableModels.FirstOrDefault(m => m.EffectiveId.Contains("claude-3-5-sonnet"))
                                     ?? AvailableModels.FirstOrDefault(m => m.EffectiveId.Contains("claude-3"))
                                     ?? AvailableModels.FirstOrDefault(m => m.EffectiveId.Contains("grok"))
                                     ?? AvailableModels.FirstOrDefault();
                    }
                }
                else
                {
                    AvailableModels = new System.Collections.ObjectModel.ObservableCollection<ChatbotModelInfo>();
                }
            }
        }

        private void SetBaselineIfInitialLoad()
        {
            if (_isInitialLoad)
            {
                _isInitialLoad = false;
                SetItem(this);
                IsDirty = false;
            }
        }

        [JsonIgnore]
        public Guid ResourceSourceId
        {
            get => _resourceSourceId;
            set
            {
                _resourceSourceId = value;
                OnPropertyChanged();
            }
        }

        public virtual void Save(ChatbotSettingsTo settings)
        {
            var source = _selectedChatbotSource as ChatbotSource;
            if (source is null)
            {
                return;
            }

            // Update the selected model on the source using EffectiveId (works for both Gemini "models/..." format and regular IDs)
            if (_selectedModel != null)
            {
                source.SelectedModel = _selectedModel.EffectiveId;
            }

            var serializer = new Dev2JsonSerializer();
            var payload = serializer.Serialize(source);
            payload = DpapiWrapper.Encrypt(payload);

            var data = new ChatbotSettingsData
            {
                ChatbotSource = new NamedGuidWithEncryptedPayload
                {
                    Name = _selectedChatbotSource.ResourceName,
                    Value = _selectedChatbotSource.ResourceID,
                    Payload = payload
                },
                IncludeSystemLog = _includeSystemLog,
                IncludeResourcesXaml = _includeResourcesXaml,
                IncludeResourcesJson = _includeResourcesJson
            };
            
            Dev2Logger.Info($"ChatbotSettings: Saving settings - IncludeSystemLog={_includeSystemLog}, IncludeResourcesXaml={_includeResourcesXaml}, IncludeResourcesJson={_includeResourcesJson}", "Warewolf Info");
            
            // Populate the transfer object with checkbox settings so it gets serialized
            if (settings != null)
            {
                settings.IncludeSystemLog = _includeSystemLog;
                settings.IncludeResourcesXaml = _includeResourcesXaml;
                settings.IncludeResourcesJson = _includeResourcesJson;
                Dev2Logger.Info($"ChatbotSettings: Updated ChatbotSettingsTo - IncludeSystemLog={settings.IncludeSystemLog}, IncludeResourcesXaml={settings.IncludeResourcesXaml}, IncludeResourcesJson={settings.IncludeResourcesJson}", "Warewolf Info");
            }
            
            CurrentEnvironment.ResourceRepository.SaveChatbotSettings(CurrentEnvironment, data);
            
            Dev2Logger.Info("ChatbotSettings: Settings saved successfully", "Warewolf Info");
            
            // Update the baseline for dirty checking
            SetItem(this);
            IsDirty = false;
        }

        [JsonIgnore]
        public ChatbotSettingsViewModel Item
        {
            private get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public void SetItem(ChatbotSettingsViewModel model)
        {
            Item = Clone(model);
        }

        private static ChatbotSettingsViewModel Clone(ChatbotSettingsViewModel model)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(model, new JsonSerializerSettings { ContractResolver = resolver });
            var clone = JsonConvert.DeserializeObject<ChatbotSettingsViewModel>(ser);
            return clone;
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        public ICommand NewChatbotSourceCommand => _newChatbotSourceCommand;
        public ICommand EditChatbotSourceCommand => _editChatbotSourceCommand;

#pragma warning disable CC0091 // Use static method
		private void NewChatbotSource()
#pragma warning restore CC0091 // Use static method
		{
            // Trigger the creation of a new chatbot source
            // Pass null to create a new source
            CustomContainer.Get<IShellViewModel>()?.NewChatbotSourceCommand?.Execute(null);
        }

        private void EditChatbotSource()
        {
            if (_selectedChatbotSource != null)
            {
                // Trigger editing of the selected chatbot source by opening the resource
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                shellViewModel?.OpenResource(_selectedChatbotSource.ResourceID, CurrentEnvironment.EnvironmentID, CurrentEnvironment);
            }
        }

        private bool CanEditChatbotSource()
        {
            return _selectedChatbotSource != null;
        }

        public bool Equals(ChatbotSettingsViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return EqualsSeq(other);
        }

        bool EqualsSeq(ChatbotSettingsViewModel other)
        {
            var equalsSeq = Equals(_resourceSourceId, other._resourceSourceId);
            
            // Compare selected model IDs using EffectiveId
            var thisModelId = _selectedModel?.EffectiveId;
            var otherModelId = other._selectedModel?.EffectiveId;
            equalsSeq &= string.Equals(thisModelId, otherModelId);
            
            // Compare checkbox settings
            equalsSeq &= _includeSystemLog == other._includeSystemLog;
            equalsSeq &= _includeResourcesXaml == other._includeResourcesXaml;
            equalsSeq &= _includeResourcesJson == other._includeResourcesJson;
            
            return equalsSeq;
        }

		protected override void CloseHelp()
		{
			throw new NotImplementedException();
		}
    }

    // Wrapper class for APIs that return models in a "data" array (like OpenAI/xAI)
    public class ModelsResponseWrapper
    {
        [JsonProperty("data")]
        public List<ChatbotModelInfo> Data { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }
    }

    // Wrapper class for Google Gemini API that returns models in a "models" array
    public class GeminiModelsResponseWrapper
    {
        [JsonProperty("models")]
        public List<ChatbotModelInfo> Models { get; set; }
    }

    public class ChatbotModelInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        // Google Gemini uses 'name' as the primary identifier (e.g., "models/gemini-pro")
        // GitHub Models uses 'name' as a friendly display name
        [JsonProperty("name")]
        public string Name { get; set; }

        // Google Gemini also has a displayName field
        [JsonProperty("displayName")]
        public string GeminiDisplayName { get; set; }

        [JsonProperty("publisher")]
        public string Publisher { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        // Google Gemini has 'description' instead of 'summary'
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("supported_input_modalities")]
        public List<string> SupportedInputModalities { get; set; }

        [JsonProperty("supported_output_modalities")]
        public List<string> SupportedOutputModalities { get; set; }

        [JsonProperty("supportedGenerationMethods")]
        public List<string> SupportedGenerationMethods { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        // OpenAI/xAI format fields
        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        public long? Created { get; set; }

        [JsonProperty("owned_by")]
        public string OwnedBy { get; set; }

        // Property to get the effective ID (Gemini uses 'name' as ID, others use 'id')
        public string EffectiveId
        {
            get
            {
                // For Gemini, 'name' is the model identifier (e.g., "models/gemini-pro")
                if (!string.IsNullOrEmpty(Name) && Name.StartsWith("models/"))
                {
                    return Name;
                }
                // Otherwise use 'id'
                return Id ?? Name;
            }
        }

        public string DisplayName
        {
            get
            {
                // For Google Gemini, use displayName if available
                if (!string.IsNullOrEmpty(GeminiDisplayName))
                {
                    return GeminiDisplayName;
                }
                
                // If Name is available and looks like a friendly name (not starting with "models/"), use it with ID
                if (!string.IsNullOrEmpty(Name) && !Name.StartsWith("models/"))
                {
                    if (!string.IsNullOrEmpty(Id))
                    {
                        return $"{Name} ({Id})";
                    }
                    return Name;
                }
                
                // For Gemini format "models/gemini-pro", extract just "gemini-pro"
                if (!string.IsNullOrEmpty(Name) && Name.StartsWith("models/"))
                {
                    return Name.Substring("models/".Length);
                }
                
                // Otherwise just use ID
                return Id ?? Name ?? "Unknown";
            }
        }

        public string Tooltip
        {
            get
            {
                var tooltipParts = new List<string>();

                // Add description (Gemini) or summary (others) if available
                if (!string.IsNullOrEmpty(Description))
                {
                    tooltipParts.Add(Description);
                }
                else if (!string.IsNullOrEmpty(Summary))
                {
                    tooltipParts.Add(Summary);
                }

                // Add publisher/owner information
                if (!string.IsNullOrEmpty(Publisher))
                {
                    tooltipParts.Add($"Publisher: {Publisher}");
                }
                else if (!string.IsNullOrEmpty(OwnedBy))
                {
                    tooltipParts.Add($"Owned by: {OwnedBy}");
                }

                // Add supported generation methods for Gemini
                if (SupportedGenerationMethods != null && SupportedGenerationMethods.Count > 0)
                {
                    tooltipParts.Add($"Supported methods: {string.Join(", ", SupportedGenerationMethods)}");
                }

                // Add tags if available
                if (Tags != null && Tags.Count > 0)
                {
                    tooltipParts.Add($"Tags: {string.Join(", ", Tags)}");
                }

                return tooltipParts.Count > 0 ? string.Join("\n\n", tooltipParts) : EffectiveId;
            }
        }
    }
}
