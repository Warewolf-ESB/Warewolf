using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Navigation;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Dev2.Studio.ViewModels.Explorer
{
    public class ExplorerViewModel : BaseViewModel
    {
        #region Class Members

        private readonly string _mediatorKey;
        private RelayCommand _connectCommand;

        #endregion Class Members

        #region Constructor

        public ExplorerViewModel()
        {
            _mediatorKey = Mediator.RegisterToReceiveMessage(MediatorMessages.UpdateExplorer,
                                                             o => RefreshEnvironments((o is bool) && (bool) o));

            Mediator.RegisterToReceiveMessage(MediatorMessages.RemoveServerFromExplorer,
                                              o => RemoveEnvironment((IEnvironmentModel) o));
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddServerToExplorer,
                                              o => AddEnvironment((IEnvironmentModel) o));

            NavigationViewModel = new NavigationViewModel(false);
            LoadEnvironments();
        }

        #endregion Constructor

        #region Commands

        public ICommand ConnectCommand
        {
            get { return _connectCommand ?? (_connectCommand = new RelayCommand(param => Connect(), param => true)); }
        }

        #endregion Commands

        #region Properties

        [Import]
        public IFilePersistenceProvider FilePersistenceProvider { get; set; }

        [Import]
        public IDev2WindowManager WindowNavigationBehavior { get; set; }

        [Import]
        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; set; }

        public NavigationViewModel NavigationViewModel { get; set; }

        #endregion Properties

        #region Private Methods

        private void AddEnvironment(IEnvironmentModel environmentModel)
        {
            EnvironmentRepository.Save(environmentModel);
            NavigationViewModel.AddEnvironment(environmentModel);
        }

        private void RemoveEnvironment(IEnvironmentModel environment)
        {
            EnvironmentRepository.Remove(environment);
            NavigationViewModel.RemoveEnvironment(environment);
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
            if (!addMissingEnvironments) return;

            foreach (IEnvironmentModel environment in EnvironmentRepository.All())
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

            //
            // Add all environments to the navigation view model
            //
            foreach (var environment in EnvironmentRepository.All())
            {
                NavigationViewModel.AddEnvironment(environment);
            }

            Mediator.SendMessage(MediatorMessages.SetActiveEnvironment, Dev2.Studio.Core.EnvironmentRepository.DefaultEnvironment);
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
            if (connectViewModel.DialogResult == ViewModelDialogResults.Okay)
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
            Mediator.DeRegister(MediatorMessages.UpdateExplorer, _mediatorKey);
            base.OnDispose();
        }

        #endregion Dispose Handling
    }
}