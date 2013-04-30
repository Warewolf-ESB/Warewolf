using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Navigation;

namespace Dev2.Studio.ViewModels.Explorer
{
    public class ExplorerViewModel : BaseViewModel, IHandle<UpdateExplorerMessage>, IHandle<RemoveEnvironmentMessage>, IHandle<AddServerToExplorerMessage>
    {
        #region Class Members

        private RelayCommand _connectCommand;
        private RelayCommand _environmentChangedCommand;

        #endregion Class Members

        #region Constructor

        public ExplorerViewModel(bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
            : this(Core.EnvironmentRepository.Instance, isFromActivityDrop, activityType)
        {
        }

        public ExplorerViewModel(IEnvironmentRepository environmentRepository, bool isFromActivityDrop = false, enDsfActivityType activityType = enDsfActivityType.All)
        {
            if(environmentRepository == null)
            {
                throw new ArgumentNullException("environmentRepository");
            }
            EnvironmentRepository = environmentRepository;
            NavigationViewModel = new NavigationViewModel(false, isFromActivityDrop, activityType);
            LoadEnvironments();
        }

        #endregion Constructor

        #region Commands

        public ICommand ConnectCommand
        {
            get { return _connectCommand ?? (_connectCommand = new RelayCommand(param => Connect(), param => true)); }
        }

        public ICommand EnvironmentChangedCommand
        {
            get
            {
                ICommand command = _environmentChangedCommand ?? (_environmentChangedCommand = new RelayCommand(param => AddEnvironment((IEnvironmentModel)param), param => true));
                return command;
            }
        }

        #endregion Commands

        #region Properties
        [Import]
        public IFilePersistenceProvider FilePersistenceProvider { get; set; }

        [Import]
        public IDev2WindowManager WindowNavigationBehavior { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        public NavigationViewModel NavigationViewModel { get; set; }

        #endregion Properties

        #region Private Methods

        private void AddEnvironment(IEnvironmentModel environmentModel)
        {
            EnvironmentRepository.Save(environmentModel);
            NavigationViewModel.AddEnvironment(environmentModel);
            EnvironmentRepository.WriteSession(NavigationViewModel.Environments.Select(e => e.ID));
        }

        private void RemoveEnvironment(IEnvironmentModel environment)
        {
            EnvironmentRepository.Remove(environment);
            NavigationViewModel.RemoveEnvironment(environment);
            EnvironmentRepository.WriteSession(NavigationViewModel.Environments.Select(e => e.ID));
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
        private void LoadEnvironments()
        {
            //
            // Load environments from repository
            //
            EnvironmentRepository.Load();

            // Load the default environment
            NavigationViewModel.AddEnvironment(Core.EnvironmentRepository.Instance.Source);
            EventAggregator.Publish(new SetActiveEnvironmentMessage(Core.EnvironmentRepository.Instance.Source));

            //
            // Add last session's environments to the navigation view model
            //
            var sessionGuids = EnvironmentRepository.ReadSession();
            foreach(var environment in EnvironmentRepository.All().Where(e => sessionGuids.Contains(e.ID)))
            {
                NavigationViewModel.AddEnvironment(environment);
            }
        }

        /// <summary>
        ///     Shows the connect view and acts on it's results.
        /// </summary>
        private void Connect()
        {
            //
            // Create and show the connect view
            //
            var connectViewModel = new ConnectViewModel();
            WindowNavigationBehavior.ShowDialog(connectViewModel);

            //
            // If connect view closed with okay then create an environment, save it and load it into the navigation view model
            //
            if(connectViewModel.DialogResult == ViewModelDialogResults.Okay)
            {
                //
                // Add the environment to the navigation view model
                //
                NavigationViewModel.AddEnvironment(connectViewModel.Server.Environment);
            }
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
            EventAggregator.Unsubscribe(this);
            base.OnDispose();
        }

        #endregion Dispose Handling

        #region Implementation of IHandle<UpdateExplorerMessage>

        public void Handle(UpdateExplorerMessage message)
        {
            RefreshEnvironments(message.Update);
        }

        #endregion

        #region Implementation of IHandle<RemoveEnvironmentMessage>

        public void Handle(RemoveEnvironmentMessage message)
        {
            RemoveEnvironment(message.EnvironmentModel);
        }

        #endregion

        #region Implementation of IHandle<AddServerToExplorerMessage>

        public void Handle(AddServerToExplorerMessage message)
        {
            AddEnvironment(message.EnvironmentModel);
        }

        #endregion
    }

}