using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Services.Events;
using Dev2.Studio.Core.Configuration;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.WorkSurface;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Dev2.Settings
{
    public class RuntimeConfigurationViewModel : BaseWorkSurfaceViewModel
    {
        #region Class Members

        private readonly string _configurationTypeLocation = "Dev2.Runtime.Configuration.Settings.Configuration";
        private readonly string _configurationEntrypointMethodName = "EntryPoint";
        private UserControl _runtimeConfigurationUserControl;
        private IEnvironmentModel _currentEnvironment;
        private RelayCommand<IEnvironmentModel> _sourceServerChangedCommand;
        private bool _isWorking;
        private bool _saveSuccessfull;
        private Guid? _context;

        #endregion

        #region Constructor

        public RuntimeConfigurationViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public RuntimeConfigurationViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
            RuntimeConfigurationAssemblyRepository = ImportService.GetExportValue<IRuntimeConfigurationAssemblyRepository>();
            Popup = ImportService.GetExportValue<IPopupController>();
        }

        #endregion

        #region Commands


        public RelayCommand<IEnvironmentModel> SourceServerChangedCommand
        {
            get
            {
                return _sourceServerChangedCommand ?? (_sourceServerChangedCommand
                                                       = new RelayCommand<IEnvironmentModel>(ServerChanged));
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
            // ReSharper disable once UnusedMember.Local
            private set
            {
                _saveSuccessfull = value;
                NotifyOfPropertyChange(() => SaveSuccessfull);
            }
        }

        #endregion

        #region Private Properties

        private IRuntimeConfigurationAssemblyRepository RuntimeConfigurationAssemblyRepository { get; set; }
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private string AssemblyHashCode { get; set; }
        private XElement ConfigurationXml { get; set; }
        private IPopupController Popup { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        #endregion Private Properties

        #region Override Methods

        protected override void OnDeactivate(bool close)
        {
            if(close)
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
                if(localHost != null && localHost.IsConnected)
                    Load(localHost);
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

            if(!environment.IsConnected)
            {
                environment.CanStudioExecute = false;
                PopupController.ShowNotConnected();
            }

            IsWorking = true;

            try
            {
                CurrentEnvironment = environment;
                LoadUserControl();
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

        private void ServerChanged(IEnvironmentModel server)
        {
            Load(server);
        }

        private void LoadUserControl()
        {
            // Load assembly
            Assembly assembly;
            try
            {
                assembly = RuntimeConfigurationAssemblyRepository.Load(AssemblyHashCode);

                if(assembly == null)
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

                if(configurationType == null)
                {
                    throw new Exception("Configuration type is null.");
                }
            }
            catch(Exception e)
            {
                ShowError(string.Format("Unable to locate type '{0}' in runtime configuration assembly.", _configurationTypeLocation), e);
                return;
            }

            // Invoke entry point
            try
            {
                // Create parameters
                System.Action cancelCallback = Cancel;
                System.Action settingChangedCallback = () => { };
                // ReSharper disable once RedundantArrayCreationExpression
                object[] parameters = new object[]
                {
                    ConfigurationXml, cancelCallback, settingChangedCallback
                };

                InitializationRequested = true;

                var usercontrol = RuntimeConfigurationAssemblyRepository
                    .GetUserControlForAssembly(AssemblyHashCode);

                if(usercontrol != null)
                {
                    RuntimeConfigurationUserControl = usercontrol;
                }
                else
                {
                    // Invoke
                    if(Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
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
            catch(Exception e)
            {
                ShowError(string.Format("Unable to locate entry point method '{0}.{1}' in runtime configuration assembly.", _configurationTypeLocation, _configurationEntrypointMethodName), e);
            }
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
