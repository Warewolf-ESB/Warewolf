using Dev2.Composition;
using Dev2.Network.Messages;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Configuration;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.WorkSurface;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Dev2.Studio.ViewModels.Configuration
{
    public class RuntimeConfigurationViewModel : BaseWorkSurfaceViewModel
    {
        #region Class Members

        private readonly string _configurationTypeLocation = "Dev2.Runtime.Configuration.Settings.Configuration";
        private readonly string _configurationEntrypointMethodName = "EntryPoint";
        private UserControl _runtimeConfigurationUserControl;
        private IEnvironmentModel _currentEnvironment;
        private bool _isWorking;
        private bool _saveSuccessfull;

        #endregion

        #region Constructor

        public RuntimeConfigurationViewModel(IEnvironmentModel environment)
        {
            CurrentEnvironment = environment;

            RuntimeConfigurationAssemblyRepository = ImportService.GetExportValue<IRuntimeConfigurationAssemblyRepository>();
            Popup = ImportService.GetExportValue<IPopUp>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The user control used to configure the runtime configuration
        /// </summary>
        public UserControl RuntimeConfigurationUserControl
        {
            get
            {
                return _runtimeConfigurationUserControl;
            }
            private set
            {
                _runtimeConfigurationUserControl = value;
                NotifyOfPropertyChange(() => RuntimeConfigurationUserControl);
            }
        }

        /// <summary>
        /// The environment to edit the runtim configuration of
        /// </summary>
        public IEnvironmentModel CurrentEnvironment
        {
            get
            {
                return _currentEnvironment;
            }
            private set
            {
                _currentEnvironment = value;
                NotifyOfPropertyChange(() => CurrentEnvironment);
            }
        }

        /// <summary>
        /// Used to indicate if the view model is busy working
        /// </summary>
        public bool IsWorking
        {
            get
            {
                return _isWorking;
            }
            private set
            {
                _isWorking = value;
                NotifyOfPropertyChange(() => IsWorking);
            }
        }

        /// <summary>
        /// Used to indicate that a save happened successfully
        /// </summary>
        public bool SaveSuccessfull
        {
            get
            {
                return _saveSuccessfull;
            }
            private set
            {
                _saveSuccessfull = value;
                NotifyOfPropertyChange(() => SaveSuccessfull);
            }
        }

        #endregion

        #region Private Properties

        private IRuntimeConfigurationAssemblyRepository RuntimeConfigurationAssemblyRepository { get; set; }
        private string AssemblyHashCode { get; set; }
        private XElement ConfigurationXml { get; set; }
        private IPopUp Popup { get; set; }

        #endregion Private Properties

        #region Override Methods

        protected override void OnActivate()
        {
            base.OnActivate();
            Load(CurrentEnvironment);
        }

        #endregion

        #region Methods

        public void Load(IEnvironmentModel environment)
        {
            if(environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            IsWorking = true;

            try
            {
                CurrentEnvironment = environment;

                if(FetchRuntimeConfiguration(false))
                {
                    LoadUserControl();
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        public void Save(XElement configurationXML)
        {
            IsWorking = true;

            try
            {
                SaveImpl(configurationXML, false);
            }
            finally
            {
                IsWorking = false;
            }
        }

        public void Save(XElement configurationXML, bool overwriteSettings)
        {
            IsWorking = true;

            try
            {
                SaveImpl(configurationXML, overwriteSettings);
            }
            finally
            {
                IsWorking = false;
            }
        }

        public void Cancel()
        {
            //Publish settings save cancel message
            EventAggregator.Publish(new SettingsSaveCancelMessage());
        }

        #endregion

        #region Private Methods

        private void LoadUserControl()
        {
            // Load assembly
            Assembly assembly;
            try
            {
                assembly = RuntimeConfigurationAssemblyRepository.Load(AssemblyHashCode);

                if (assembly == null)
                {
                    throw new Exception("Runtime configuration assembly is null.");
                }
            }
            catch(Exception e)
            {
                ShowErrorAndCancel("Unable to load runtime configuration assembly.", e);
                return;
            }

            // Locate configuration type
            Type configurationType;
            try
            {
                configurationType = assembly.GetType(_configurationTypeLocation);

                if (configurationType == null)
                {
                    throw new Exception("Configuration type is null.");
                }
            }
            catch (Exception e)
            {
                ShowErrorAndCancel(string.Format("Unable to locate type '{0}' in runtime configuration assembly.", _configurationTypeLocation), e);
                return;
            }

            // Invoke entry point
            try
            {
                // Create parameters
                Action<XElement> saveCallback = Save;
                Action cancelCallback = Cancel;
                Action settingChangedCallback = () => {};
                object[] parameters = new object[]
                {
                    ConfigurationXml, saveCallback, cancelCallback, settingChangedCallback
                };

                // Invoke
                RuntimeConfigurationUserControl = configurationType.InvokeMember(_configurationEntrypointMethodName, BindingFlags.Default | BindingFlags.InvokeMethod, null, null, parameters) as UserControl;
            }
            catch (Exception e)
            {
                ShowErrorAndCancel(string.Format("Unable to locate entry point method '{0}.{1}' in runtime configuration assembly.", _configurationTypeLocation, _configurationEntrypointMethodName), e);
            }
        }

        private bool FetchRuntimeConfiguration(bool overwriteAssemblyIfExists)
        {
            // Construct & send read message
            SettingsMessage settingsMessage = new SettingsMessage
            {
                Action = NetworkMessageAction.Read,
                AssemblyHashCode = "",
            };

            // Send message
            INetworkMessage result;
            try
            {
                result = CurrentEnvironment.DsfChannel.SendSynchronousMessage(settingsMessage);
            }
            catch (Exception e)
            {
                ShowErrorAndCancel("An error occured while sending a message to the server.", e);
                return false;
            }

            // Check for error result
            ErrorMessage errorMessage = result as ErrorMessage;
            if (errorMessage != null)
            {
                ShowErrorAndCancel(errorMessage.Message, null);
                return false;
            }

            // Check if result is unknown type
            SettingsMessage resultSettingsMessage = result as SettingsMessage;
            if (resultSettingsMessage == null)
            {
                ShowErrorAndCancel("An unknown response was received from the server.", null);
                return false;
            }

            // Set data
            AssemblyHashCode = resultSettingsMessage.AssemblyHashCode;
            ConfigurationXml = resultSettingsMessage.ConfigurationXml;

            // If assembly not in repository add it
            if (overwriteAssemblyIfExists || !RuntimeConfigurationAssemblyRepository.AllHashes().Contains(resultSettingsMessage.AssemblyHashCode))
            {
                RuntimeConfigurationAssemblyRepository.Add(resultSettingsMessage.AssemblyHashCode, resultSettingsMessage.Assembly);
            }

            return true;
        }

        private void SaveImpl(XElement configurationXML, bool overwriteSettings)
        {
            // Construct write/overwrite message
            SettingsMessage settingsMessage = new SettingsMessage
            {
                Action = (overwriteSettings) ? NetworkMessageAction.Overwrite : NetworkMessageAction.Write,
                AssemblyHashCode = AssemblyHashCode,
                ConfigurationXml = configurationXML,
            };

            // Send message
            INetworkMessage result;
            try
            {
                result = CurrentEnvironment.DsfChannel.SendSynchronousMessage(settingsMessage);
            }
            catch (Exception e)
            {
                ShowErrorAndCancel("An error occured while sending a message to the server.", e);
                return;
            }

            // Check for error result
            ErrorMessage errorMessage = result as ErrorMessage;
            if (errorMessage != null)
            {
                ShowErrorAndCancel(errorMessage.Message, null);
                return;
            }

            // Check if result is unknown type
            SettingsMessage resultSettingsMessage = result as SettingsMessage;
            if (resultSettingsMessage == null)
            {
                Popup.Show("An unknown response was received from the server, please try save again.", "Unknown Response", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Deal with version conflict
            if (resultSettingsMessage.Result == NetworkMessageResult.VersionConflict)
            {
                MessageBoxResult overwriteResult = Popup.Show("The settings on the server are newer than the ones you are saving, would you like to overwrite the server settings.", "Newer Settings", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (overwriteResult == MessageBoxResult.Yes)
                {
                    Save(configurationXML, true);
                }
                return;
            }

            if (resultSettingsMessage.Result == NetworkMessageResult.Unknown)
            {
                Popup.Show("An unknown result was received from the server, please try save again.", "Unknown Result", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            //Publish settings save cancel message
            EventAggregator.Publish(new SettingsSaveCancelMessage());
        }

        private void ShowErrorAndCancel(string errorMessage, Exception innerException)
        {
            Exception errorEx = new Exception(errorMessage, innerException);
            ExceptionFactory.CreateViewModel(errorEx).Show();
            Cancel();
        }

        #endregion Private Methods
    }
}
