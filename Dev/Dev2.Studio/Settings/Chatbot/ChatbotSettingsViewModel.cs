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

namespace Dev2.Settings.Chatbot
{
    public class ChatbotSettingsViewModel : SettingsItemViewModel, IUpdatesHelp, IChatbotSettings
    {
        private readonly IResourceRepository _resourceRepository;
        IServer _currentEnvironment;
        private Guid _resourceSourceId;
        private ChatbotSettingsViewModel _item;
        private bool _encryptDataSource;
        private IChatbotSourceResource _selectedChatbotSource;
        private ICommand _newChatbotSourceCommand;
        private ICommand _editChatbotSourceCommand;

        [ExcludeFromCodeCoverage]
        public ChatbotSettingsViewModel()
        {
        }

        public ChatbotSettingsViewModel(IServer server)
        {
            CurrentEnvironment = server ?? throw new ArgumentNullException(nameof(server));
            _resourceRepository = CurrentEnvironment.ResourceRepository;

            var settingsData = CurrentEnvironment.ResourceRepository.GetChatbotSettings<ChatbotSettingsData>(CurrentEnvironment);
            if (settingsData.ChatbotSource != null)
            {
                var selectedSource = ChatbotSources.FirstOrDefault(o => o.ResourceID == settingsData.ChatbotSource.Value);
                SelectedChatbotSource = selectedSource;
			}
			_encryptDataSource = settingsData.EncryptDataSource ?? true;

			_newChatbotSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(NewChatbotSource);
            _editChatbotSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(EditChatbotSource, CanEditChatbotSource);

            IsDirty = false;
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
            var chatbotSources = _resourceRepository.GetResourceList<ChatbotSource>(_currentEnvironment);
            return chatbotSources.Cast<IResource>().ToList();
        }

        public bool EncryptDataSource
        {
            get => _encryptDataSource;
            set
            {
                IsDirty = !Equals(Item);
                _encryptDataSource = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public IChatbotSourceResource SelectedChatbotSource
        {
            get => _selectedChatbotSource;
            set
            {
                IsDirty = !Equals(Item);
                _selectedChatbotSource = value;
                if (_selectedChatbotSource != null)
                {
                    ResourceSourceId = _selectedChatbotSource.ResourceID;
                }

                OnPropertyChanged();
                ((Microsoft.Practices.Prism.Commands.DelegateCommand)_editChatbotSourceCommand)?.RaiseCanExecuteChanged();
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

            var serializer = new Dev2JsonSerializer();
            var payload = serializer.Serialize(source);
            if (_encryptDataSource)
            {
                payload = DpapiWrapper.Encrypt(payload);
            }

            var data = new ChatbotSettingsData
            {
                EncryptDataSource = _encryptDataSource,
                ChatbotSource = new NamedGuidWithEncryptedPayload
                {
                    Name = _selectedChatbotSource.ResourceName,
                    Value = _selectedChatbotSource.ResourceID,
                    Payload = payload
                }
            };
            CurrentEnvironment.ResourceRepository.SaveChatbotSettings(CurrentEnvironment, data);
            SetItem(this);
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

        private void NewChatbotSource()
        {
            // Trigger the creation of a new chatbot source
            // Pass null to create a new source
            CustomContainer.Get<IShellViewModel>()?.NewChatbotSourceCommand?.Execute(null);
        }

        private void EditChatbotSource()
        {
            if (_selectedChatbotSource != null)
            {
                // Trigger editing of the selected chatbot source
                // Pass the source to edit it (same command, different parameter)
                CustomContainer.Get<IShellViewModel>()?.NewChatbotSourceCommand?.Execute(_selectedChatbotSource);
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
            var equalsSeq = Equals(_encryptDataSource, other._encryptDataSource);
            equalsSeq &= Equals(_resourceSourceId, other._resourceSourceId);
            return equalsSeq;
        }

		protected override void CloseHelp()
		{
			throw new NotImplementedException();
		}
    }
}
