using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.ViewModels.Explorer;

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
        }

        public void Init()
        {
            if (ActivityType != null)
            {
                if (ActivityType == enDsfActivityType.Workflow)
                {
                    IconUri = new BitmapImage(new Uri("pack://application:,,,/Warewolf Studio;component/Images/Workflow-16.png"));
                    DsfActivityType = "Workflow";
                }
                else if (ActivityType == enDsfActivityType.Service)
                {
                    IconUri = new BitmapImage(new Uri("pack://application:,,,/Warewolf Studio;component/Images/workerservice.png"));
                    DsfActivityType = "Service";
                }
            }
        }

        #endregion Ctor

        #region Properties

        public enDsfActivityType ActivityType { get; set; }

        public ExplorerViewModel ExplorerViewModel { get; private set; }

        public string DsfActivityType { get; set; }

        public BitmapImage IconUri { get; set; }

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
            }
        }


        #endregion Properties

        #region Commands

        public ICommand OKCommand
        {
            get
            {
                if (_executeCommmand == null)
                {
                    _executeCommmand = new RelayCommand(param => Okay(), param => true);
                }
                return _executeCommmand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelComand == null)
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
            if (ExplorerViewModel != null)
            {
                ExplorerViewModel.Dispose();
                ExplorerViewModel = null;
            }
            base.OnDispose();
        }

        #endregion
    }
}
