
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.ViewModels.Deploy
{
    public class DeployDialogViewModel : SimpleBaseViewModel
    {
        #region Fields
        private RelayCommand _executeCommmand;
        private RelayCommand _cancelComand;
        ObservableCollection<DeployDialogTO> _conflictingItems;

        public ObservableCollection<DeployDialogTO> ConflictingItems
        {
            get
            {
                return _conflictingItems;
            }
            set
            {
                if(Equals(value, _conflictingItems))
                {
                    return;
                }
                _conflictingItems = value;
                NotifyOfPropertyChange("ConflictingItems");
            }
        }

        public DeployDialogViewModel(ObservableCollection<DeployDialogTO> resourcesInConflict)
        {
            ConflictingItems = resourcesInConflict;
        }

        #endregion Fields

        #region Commands
        public ICommand OkCommand
        {
            get
            {
                if(_executeCommmand == null)
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
        }

        /// <summary>
        /// Used for canceling the drop of the design surface
        /// </summary>
        public void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        #endregion
    }
}
