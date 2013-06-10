using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Dev2.Composition;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Configuration;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
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
        private RelayCommand<IServer> _sourceServerChangedCommand;
        private bool _isWorking;
        private bool _saveSuccessfull;
        private Guid? _context;

        #endregion

        #region Constructor

        public RuntimeConfigurationViewModel()
        {
            RuntimeConfigurationAssemblyRepository = ImportService.GetExportValue<IRuntimeConfigurationAssemblyRepository>();
            Popup = ImportService.GetExportValue<IPopupController>();
        }

        #endregion

        #region Commands


        public RelayCommand<IServer> SourceServerChangedCommand
        {
            get
            {
                return _sourceServerChangedCommand ?? (_sourceServerChangedCommand
                                                       = new RelayCommand<IServer>(ServerChanged));
            }
        }

        #endregion Commands

        #region Properties

        [Import(typeof(IPopupController))] 
        public IPopupController PopupController { get; set; }

        public Guid? Context
        {
            get
            {
                return _context ?? (_context = Guid.NewGuid());
            }
        }

        public bool InitializationRequested { get; set; } 

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
        private IPopupController Popup { get; set; }

        #endregion Private Properties

        #region Override Methods

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                RuntimeConfigurationAssemblyRepository.Clear();
            }
            base.OnDeactivate(close);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
            {
                var servers = ServerProvider.Instance.Load();
                var localHost = servers.FirstOrDefault(s => s.IsLocalHost);
                if (localHost != null && localHost.Environment.IsConnected)
                    Load(localHost.Environment);
            };
            worker.RunWorkerAsync();
        }


        #endregion

        #region Methods

        public void Load(IEnvironmentModel environment)
        {
            if(environment == null)
            {
                return;
            }

            if (!environment.IsConnected)
            {
                environment.CanStudioExecute = false;
                PopupController.ShowNotConnected();
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

        public XElement Save(XElement configurationXML)
        {
            IsWorking = true;

            XElement newConfig;

            try
            {
               newConfig = SaveImpl(configurationXML, false);
            }
            finally
            {
                IsWorking = false;
            }

            return newConfig;
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
            Load(CurrentEnvironment);
        }

        #endregion

        #region Private Methods

        private void ServerChanged(IServer server)
        {
            Load(server.Environment);
        }

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
                ShowError("Unable to load runtime configuration assembly.", e);
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
                ShowError(string.Format("Unable to locate type '{0}' in runtime configuration assembly.", _configurationTypeLocation), e);
                return;
            }

            // Invoke entry point
            try
            {
                // Create parameters
                Func<XElement, XElement> saveCallback = Save;
                Action cancelCallback = Cancel;
                Action settingChangedCallback = () => {};
                object[] parameters = new object[]
                {
                    ConfigurationXml, saveCallback, cancelCallback, settingChangedCallback
                };

                InitializationRequested = true;

                var usercontrol = RuntimeConfigurationAssemblyRepository
                    .GetUserControlForAssembly(AssemblyHashCode);

                if (usercontrol != null)
                {
                    RuntimeConfigurationUserControl = usercontrol;
                }
                else
                {
                    // Invoke
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var userControl =
                                configurationType.InvokeMember
                                    (_configurationEntrypointMethodName,
                                     BindingFlags.Default | BindingFlags.InvokeMethod,
                                     null, null, parameters) as UserControl;
                            RuntimeConfigurationAssemblyRepository
                                .UserControlCache.Add(AssemblyHashCode, userControl);
                            RuntimeConfigurationUserControl = userControl;
                        }), null);
                    }
                }
            }
            catch (Exception e)
            {
                ShowError(string.Format("Unable to locate entry point method '{0}.{1}' in runtime configuration assembly.", _configurationTypeLocation, _configurationEntrypointMethodName), e);
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
                result = CurrentEnvironment.Connection.SendReceiveNetworkMessage(settingsMessage);
            }
            catch (Exception e)
            {
                ShowError("An error occured while sending a message to the server.", e);
                return false;
            }

            if (result == null)
            {
                ShowError("No response was received while sending a message to the server.", null);
                return false;
            }

            // Check for error result
            ErrorMessage errorMessage = result as ErrorMessage;
            if (errorMessage != null)
            {
                ShowError(errorMessage.Message, null);
                return false;
            }

            // Check if result is unknown type
            SettingsMessage resultSettingsMessage = result as SettingsMessage;
            if (resultSettingsMessage == null)
            {
                ShowError("An unknown response was received from the server.", null);
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

        private XElement SaveImpl(XElement configurationXML, bool overwriteSettings)
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
                result = CurrentEnvironment.Connection.SendReceiveNetworkMessage(settingsMessage);
            }
            catch (Exception e)
            {
                ShowError("An error occured while sending a message to the server.", e);
                return null;
            }

            // Check for error result
            ErrorMessage errorMessage = result as ErrorMessage;
            if (errorMessage != null)
            {
                ShowError(errorMessage.Message, null);
                return null;
            }

            // Check if result is unknown type
            SettingsMessage resultSettingsMessage = result as SettingsMessage;
            if (resultSettingsMessage == null)
            {
                Popup.Show("An unknown response was received from the server, please try save again.", "Unknown Response", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }

            // Deal with version conflict
            if (resultSettingsMessage.Result == NetworkMessageResult.VersionConflict)
            {
                MessageBoxResult overwriteResult = Popup.Show("The settings on the server are newer than the ones you are saving, would you like to overwrite the server settings.", "Newer Settings", MessageBoxButton.YesNo, MessageBoxImage.Question);

                switch (overwriteResult)
                {
                    case MessageBoxResult.Yes:
                    Save(configurationXML, true);
                        break;
                    default:
                        Cancel();
                        break;
                }
                return null;
            }

            if (resultSettingsMessage.Result == NetworkMessageResult.Unknown)
            {
                Popup.Show("An unknown result was received from the server, please try save again.", "Unknown Result", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return null;
            }

            //Publish settings save cancel message
            return resultSettingsMessage.ConfigurationXml;
        }

        private void ShowError(string errorMessage, Exception innerException)
        {
            Exception errorEx = new Exception(errorMessage, innerException);
            // PBI 9598 - 2013.06.10 - TWR : added environmentModel parameter
            ExceptionFactory.CreateViewModel(errorEx, _currentEnvironment).Show();
        }

        #endregion Private Methods
    }
}
