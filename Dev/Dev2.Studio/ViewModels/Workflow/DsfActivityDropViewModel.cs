using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Explorer;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Workflow
{
    public class DsfActivityDropViewModel : SimpleBaseViewModel, IHandle<SetSelectedIContextualResourceModel>
    {
        #region Fields

        private RelayCommand _executeCommmand;
        private RelayCommand _cancelComand;
        private IContextualResourceModel _selectedResource;

        #endregion Fields

        #region Ctor

        public DsfActivityDropViewModel(ExplorerViewModel explorerViewModel, enDsfActivityType dsfActivityType)
        {
            ExplorerViewModel = explorerViewModel;
            ActivityType = dsfActivityType;
            Init();
            EventPublishers.Aggregator.Subscribe(this);
        }

        void Init()
        {
            switch(ActivityType)
            {
                case enDsfActivityType.Workflow:
                    ImageSource = "Workflow-32";
                    Title = "Select A Workflow";
                    break;
                case enDsfActivityType.Service:
                    ImageSource = "ToolService-32";
                    Title = "Select A Service";
                    break;
                default:
                    ImageSource = "ExplorerWarewolfConnection-32";
                    Title = "Select A Resource";
                    break;
            }
        }

        #endregion Ctor

        #region Properties

        public string Title { get; private set; }

        public string ImageSource { get; private set; }

        public enDsfActivityType ActivityType { get; private set; }

        public ExplorerViewModel ExplorerViewModel { get; private set; }

        public string SelectedResourceName { get; set; }

        public IContextualResourceModel SelectedResourceModel
        {
            get
            {
                return _selectedResource;
            }
            set
            {
                _selectedResource = value;
                NotifyOfPropertyChange("SelectedResourceModel");
                CommandManager.InvalidateRequerySuggested();
            }
        }


        #endregion Properties

        #region Commands

        public ICommand OkCommand
        {
            get
            {
                if(_executeCommmand == null)
                {
                    _executeCommmand = new RelayCommand(param => Okay(), param => CanOkay);
                }
                return _executeCommmand;
            }
        }
        public bool CanOkay { get { return SelectedResourceModel != null; } }

        public ICommand CancelCommand
        {
            get
            {
                if(_cancelComand == null)
                {
                    _cancelComand = new RelayCommand(param => Cancel(), param => true);
                }
                return _cancelComand;
            }
        }

        #endregion Cammands

        #region Methods

        /// <summary>
        /// Used for saving the data input by the user to the file system and pushing the data back at the workflow
        /// </summary>
        public void Okay()
        {
            RequestClose(ViewModelDialogResults.Okay);
            //Dispose();
        }

        /// <summary>
        /// Used for canceling the drop of the design surface
        /// </summary>
        public void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
            //Dispose();
        }

        #endregion Methods

        #region Private Methods



        #endregion Private Methods

        #region Implementation of IHandle<SetSelectedIContextualResourceModel>

        public void Handle(SetSelectedIContextualResourceModel message)
        {
            this.TraceInfo(message.GetType().Name);
            SelectedResourceModel = message.SelectedResource;
            if(message.DidDoubleClickOccur)
            {
                Okay();
            }
        }

        #endregion

        #region Implementation of IDisposable

        protected override void OnDispose()
        {
            if(ExplorerViewModel != null)
            {
                ExplorerViewModel.Dispose();
                ExplorerViewModel = null;
            }
            EventPublishers.Aggregator.Unsubscribe(this);

            base.OnDispose();
        }

        #endregion
    }
}
