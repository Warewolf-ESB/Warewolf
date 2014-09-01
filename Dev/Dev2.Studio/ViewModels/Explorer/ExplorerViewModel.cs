using System;
using System.Linq;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.ConnectionHelpers;
using Dev2.CustomControls.Connections;
using Dev2.Interfaces;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Explorer
{
    public class ExplorerViewModel : BaseViewModel,
                                     IHandle<RemoveEnvironmentMessage>,
                                     IHandle<EnvironmentDeletedMessage>
    {
        #region Class Members
        private Guid? _context;
        IConnectControlViewModel _connectControlViewModel;

        #endregion Class Members

        #region Constructor

        public ExplorerViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository, IStudioResourceRepository studioResourceRepository, IConnectControlSingleton connectControlSingleton,IMainViewModel mainViewModel, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All, System.Action updateWorkSpaceItems = null, IConnectControlViewModel connectControlViewModel = null)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("connectControlSingleton", connectControlSingleton);
            
            EnvironmentRepository = environmentRepository;
            
            NavigationViewModel = new NavigationViewModel(eventPublisher, asyncWorker, Context, environmentRepository, studioResourceRepository, connectControlSingleton,updateWorkSpaceItems, isFromActivityDrop, activityType)
                {
                    Parent = this
                };

            ConnectControlViewModel = connectControlViewModel ?? new ConnectControlViewModel(mainViewModel, AddEnvironment, "Connect:", true);}
        
        #endregion Constructor

        #region Commands

        public void UpdateActiveEnvironment(IEnvironmentModel environmentModel, bool isSetFromConnectControl)
        {
            ConnectControlViewModel.UpdateActiveEnvironment(environmentModel, isSetFromConnectControl);
        }

        #endregion Commands

        #region Properties
        public IFilePersistenceProvider FilePersistenceProvider { get; set; }

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