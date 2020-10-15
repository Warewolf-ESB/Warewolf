/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Windows;
using System.Windows.Forms;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Dialogs;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Persistence;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;
using Warewolf.Configuration;
using Warewolf.Data;
using Warewolf.Security.Encryption;

namespace Dev2.Settings.Persistence
{
    public class PersistenceSettingsViewModel : SettingsItemViewModel, IUpdatesHelp, IPersistenceSettings
    {
        private readonly IResourceRepository _resourceRepository;
        IServer _currentEnvironment;
        private Guid _resourceSourceId;
        private PersistenceSettingsViewModel _item;
        private bool _encryptDataSource;
        private bool _enable;
        private bool _prepareSchemaIfNecessary;
        private string _dashboardHostname;
        private string _dashboardPort;
        private string _dashboardName;
        private string _serverName;
        private string _persistenceScheduler;
        private IResource _selectedPersistenceDatabaseSource;
        private string _selectedPersistenceScheduler;

        //this is here to clone the viewmodel
        [ExcludeFromCodeCoverage]
        public PersistenceSettingsViewModel()
        {
        }

        public PersistenceSettingsViewModel(IServer currentEnvironment)
        {
            CurrentEnvironment = currentEnvironment ?? throw new ArgumentNullException(nameof(currentEnvironment));
            _resourceRepository = CurrentEnvironment.ResourceRepository;

            var settingsData = CurrentEnvironment.ResourceRepository.GetPersistenceSettings<PersistenceSettingsData>(CurrentEnvironment);

            var persistenceScheduler = PersistenceSchedulers.FirstOrDefault(o => o == settingsData.PersistenceScheduler);
            SelectedPersistenceScheduler = persistenceScheduler;

            var selectedDataSource = PersistenceDataSources.FirstOrDefault(o => o.ResourceID == settingsData.PersistenceDataSource.Value);
            SelectedPersistenceDataSource = selectedDataSource;

            _encryptDataSource = settingsData.EncryptDataSource ?? true;
            _enable = settingsData.Enable ?? false;
            _prepareSchemaIfNecessary = settingsData.PrepareSchemaIfNecessary ?? true;
            _dashboardName = settingsData.DashboardName;
            _dashboardHostname = settingsData.DashboardHostname;
            _dashboardPort = settingsData.DashboardPort;
            _serverName = settingsData.ServerName;

            IsDirty = false;
        }


        public IServer CurrentEnvironment
        {
            private get => _currentEnvironment;
            set { _currentEnvironment = value; }
        }

        [JsonIgnore] public List<string> PersistenceSchedulers => new List<string> {"Hangfire"};

        [JsonIgnore] public List<IResource> PersistenceDataSources => LoadPersistenceDataSources();

        private List<IResource> LoadPersistenceDataSources()
        {
            var persistenceDataSources = _resourceRepository.FindResourcesByType<IPersistenceSource>(_currentEnvironment);
            return persistenceDataSources;
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

        public bool Enable
        {
            get => _enable;
            set
            {
                IsDirty = !Equals(Item);
                _enable = value;
                OnPropertyChanged();
            }
        }

        public bool PrepareSchemaIfNecessary
        {
            get => _prepareSchemaIfNecessary;
            set
            {
                IsDirty = !Equals(Item);
                _prepareSchemaIfNecessary = value;
                OnPropertyChanged();
            }
        }

        public string ServerName
        {
            get => _serverName;
            set
            {
                IsDirty = !Equals(Item);
                _serverName = value;
                OnPropertyChanged();
            }
        }

        public string DashboardHostname
        {
            get => _dashboardHostname;
            set
            {
                IsDirty = !Equals(Item);
                _dashboardHostname = value;
                OnPropertyChanged();
            }
        }

        public string DashboardPort
        {
            get => _dashboardPort;
            set
            {
                IsDirty = !Equals(Item);
                _dashboardPort = value;
                OnPropertyChanged();
            }
        }

        public string DashboardName
        {
            get => _dashboardName;
            set
            {
                IsDirty = !Equals(Item);
                _dashboardName = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public IResource SelectedPersistenceDataSource
        {
            get => _selectedPersistenceDatabaseSource;
            set
            {
                IsDirty = !Equals(Item);
                _selectedPersistenceDatabaseSource = value;
                if (_selectedPersistenceDatabaseSource != null)
                {
                    ResourceSourceId = _selectedPersistenceDatabaseSource.ResourceID;
                }

                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public Guid ResourceSourceId
        {
            get => _resourceSourceId;
            set
            {
                IsDirty = !Equals(Item);
                _resourceSourceId = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string SelectedPersistenceScheduler
        {
            get => _selectedPersistenceScheduler;
            set
            {
                IsDirty = !Equals(Item);
                _selectedPersistenceScheduler = value;
                if (_selectedPersistenceScheduler != null)
                {
                    PersistenceScheduler = _selectedPersistenceScheduler;
                }

                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string PersistenceScheduler
        {
            get => _persistenceScheduler;
            set
            {
                IsDirty = !Equals(Item);
                _persistenceScheduler = value;
                OnPropertyChanged();
            }
        }

        public virtual void Save(PersistenceSettingsTo settings)
        {
            if (IsDirty)
            {
                var popupController = CustomContainer.Get<IPopupController>();
                var result = popupController.ShowPersistenceSettingsChanged();
                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var source = _selectedPersistenceDatabaseSource as DbSource;
            if (source == null)
            {
                var popupController = CustomContainer.Get<IPopupController>();
                var result = popupController.ShowPersistenceSettingsMissingValuesChanged();
                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            var serializer = new Dev2JsonSerializer();
            var payload = serializer.Serialize(source);
            if (_encryptDataSource)
            {
                payload = DpapiWrapper.Encrypt(payload);
            }

            var data = new PersistenceSettingsData
            {
                EncryptDataSource = _encryptDataSource,
                Enable = _enable,
                ServerName = _serverName,
                DashboardHostname = _dashboardHostname,
                DashboardName = _dashboardName,
                DashboardPort = _dashboardPort,
                PersistenceScheduler = _selectedPersistenceScheduler,
                PrepareSchemaIfNecessary = _prepareSchemaIfNecessary,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = _selectedPersistenceDatabaseSource.ResourceName,
                    Value = _selectedPersistenceDatabaseSource.ResourceID,
                    Payload = payload
                }
            };
            CurrentEnvironment.ResourceRepository.SavePersistenceSettings(CurrentEnvironment, data);
            SetItem(this);
        }

        [JsonIgnore]
        public PersistenceSettingsViewModel Item
        {
            private get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public void SetItem(PersistenceSettingsViewModel model)
        {
            Item = Clone(model);
        }

        private static PersistenceSettingsViewModel Clone(PersistenceSettingsViewModel model)
        {
            var resolver = new ShouldSerializeContractResolver();
            var ser = JsonConvert.SerializeObject(model, new JsonSerializerSettings {ContractResolver = resolver});
            var clone = JsonConvert.DeserializeObject<PersistenceSettingsViewModel>(ser);
            return clone;
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            HelpText = helpText;
        }

        public bool Equals(PersistenceSettingsViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return EqualsSeq(other);
        }

        bool EqualsSeq(PersistenceSettingsViewModel other)
        {
            var equalsSeq = string.Equals(_enable.ToString(), other._enable.ToString());
            equalsSeq &= Equals(_encryptDataSource, other._encryptDataSource);
            equalsSeq &= Equals(_prepareSchemaIfNecessary, other._prepareSchemaIfNecessary);
            equalsSeq &= Equals(_serverName, other._serverName);
            equalsSeq &= Equals(_dashboardHostname, other._dashboardHostname);
            equalsSeq &= Equals(_dashboardName, other._dashboardName);
            equalsSeq &= Equals(_dashboardPort, other._dashboardPort);
            equalsSeq &= Equals(_resourceSourceId, other._resourceSourceId);
            equalsSeq &= Equals(_persistenceScheduler, other._persistenceScheduler);
            return equalsSeq;
        }

        protected override void CloseHelp()
        {
            //Implement if help is done for the log settings.
        }
    }
}