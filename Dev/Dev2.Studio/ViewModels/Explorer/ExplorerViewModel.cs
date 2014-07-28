using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.CustomControls.Connections;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Explorer
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExplorerViewModel : BaseViewModel,
                                     IHandle<UpdateExplorerMessage>,
                                     IHandle<RemoveEnvironmentMessage>,
                                     IHandle<EnvironmentDeletedMessage>
    {
        readonly IStudioResourceRepository _studioResourceRepository;

        #region Class Members
        private Guid? _context;
        System.Action _onLoadResourcesCompletedOnceOff;
        IConnectControlViewModel _connectControlViewModel;

        #endregion Class Members

        #region Constructor

        public ExplorerViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IStudioResourceRepository studioResourceRepository, IConnectControlSingleton connectControlSingleton, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All, System.Action onLoadResourcesCompletedOnceOff = null, IConnectControlViewModel connectControlViewModel = null)
            : base(eventPublisher)
        {

            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("connectControlSingleton", connectControlSingleton);
            EnvironmentRepository = environmentRepository;
            NavigationViewModel = new NavigationViewModel(eventPublisher, asyncWorker, Context, environmentRepository, studioResourceRepository, connectControlSingleton, isFromActivityDrop, activityType) { Parent = this };
            if(onLoadResourcesCompletedOnceOff != null)
            {
                _onLoadResourcesCompletedOnceOff = onLoadResourcesCompletedOnceOff;
                NavigationViewModel.LoadResourcesCompleted += LoadResourcesCompletedOnceOff;
            }

            _studioResourceRepository = studioResourceRepository;
            ConnectControlViewModel = connectControlViewModel ?? new ConnectControlViewModel(AddEnvironment, "Connect:", true);
        }

        void LoadResourcesCompletedOnceOff(object sender, EventArgs e)
        {
            NavigationViewModel.LoadResourcesCompleted -= LoadResourcesCompletedOnceOff;
            try
            {
                _onLoadResourcesCompletedOnceOff();
            }
            finally
            {
                _onLoadResourcesCompletedOnceOff = null;
            }
        }

        #endregion Constructor

        #region Commands

        public void UpdateActiveEnvironment(IEnvironmentModel environmentModel, bool isSetFromConnectControl)
        {
            ConnectControlViewModel.UpdateActiveEnvironment(environmentModel, isSetFromConnectControl);
        }

        #endregion Commands

        #region Properties
        [Import]
        public IFilePersistenceProvider FilePersistenceProvider { get; set; }

        [Import]
        public IWindowManager WindowManager { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }


        public IConnectControlViewModel ConnectControlViewModel
        {
            get
            {
                return _connectControlViewModel;
            }
            set
            {
                if(Equals(value, _connectControlViewModel))
                {
                    return;
                }
                _connectControlViewModel = value;
                NotifyOfPropertyChange(() => ConnectControlViewModel);
            }
        }

        public NavigationViewModel NavigationViewModel { get; set; }

        public Guid? Context
        {
            get
            {
                return _context ?? (_context = Guid.NewGuid());
            }
        }

        #endregion Properties

        #region Private Methods

        private void AddEnvironment(IEnvironmentModel environmentModel)
        {
            if(environmentModel != null)
            {
                NavigationViewModel.AddEnvironment(environmentModel);
                SaveEnvironment(environmentModel);
            }
        }

        private void SaveEnvironment(IEnvironmentModel environmentModel)
        {
            EnvironmentRepository.Save(environmentModel);
            EnvironmentRepository.WriteSession(NavigationViewModel.Environments.Select(e => e.ID));
        }

        private void RemoveEnvironment(IEnvironmentModel environment)
        {
            NavigationViewModel.RemoveEnvironment(environment);
            SaveEnvironment(environment);
        }

        /// <summary>
        ///     Reloads all connected environemnts resources
        /// </summary>
        private void RefreshEnvironments(bool addMissingEnvironments)
        {
            NavigationViewModel.RefreshEnvironments();

            //
            // Ensure all environments are added to the navigation view model
            //
            if(!addMissingEnvironments) return;

            foreach(IEnvironmentModel environment in EnvironmentRepository.All())
            {
                NavigationViewModel.AddEnvironment(environment);
            }
        }

        /// <summary>
        ///     Loads the environments from the resource repository
        /// </summary>
        public void LoadEnvironments()
        {
            if(EnvironmentRepository == null) return;

            //
            // Load environments from repository
            //
            if(!EnvironmentRepository.IsLoaded)
                EnvironmentRepository.Load();

            // Load the default environment

            var environmentModel = EnvironmentRepository.Source;
            _studioResourceRepository.Load(environmentModel.ID, new AsyncWorker(), OnLoadCompletion(environmentModel));
        }

        public Action<Guid> OnLoadCompletion(IEnvironmentModel environmentModel)
        {
            return id =>
                {
                    NavigationViewModel.AddEnvironment(environmentModel);
                    //
                    // Add last session's environments to the navigation view model
                    //
                    var sessionGuids = EnvironmentRepository.ReadSession();
                    if(sessionGuids != null && sessionGuids.Count > 0)
                    {
                        ICollection<IEnvironmentModel> environmentModels = EnvironmentRepository.All();
                        if(environmentModels.Count > 0)
                        {
                            foreach(var environment in environmentModels.Where(e => sessionGuids.Contains(e.ID)))
                            {
                                NavigationViewModel.AddEnvironment(environment);
                            }
                        }
                    }
                };
        }

        #endregion Private Methods

        #region Dispose Handling

        protected override void OnDispose()
        {
            if(NavigationViewModel != null)
            {
                NavigationViewModel.Dispose();
                NavigationViewModel = null;
            }
            base.OnDispose();
        }

        #endregion Dispose Handling

        #region IHandle

        public void Handle(UpdateExplorerMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            RefreshEnvironments(message.Update);
        }

        public void Handle(RemoveEnvironmentMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            RemoveEnvironment(message.EnvironmentModel);
        }

        public void Handle(EnvironmentDeletedMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            RemoveEnvironment(message.EnvironmentModel);
        }

        #endregion  IHandle

        public void BringItemIntoView(WorkSurfaceContextViewModel item)
        {
            if(NavigationViewModel != null)
            {
                if(item != null && item.ContextualResourceModel != null)
                {
                    NavigationViewModel.BringItemIntoView(item.ContextualResourceModel);
                }
            }
        }
    }
}