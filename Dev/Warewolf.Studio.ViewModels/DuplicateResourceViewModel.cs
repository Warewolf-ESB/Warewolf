using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DuplicateResourceViewModel : BindableBase, IDuplicateResourceViewModel
    {
        private ICreateDuplicateResourceView _createDuplicateResourceView;

        private string _newResourceName;
        public string NewResourceName
        {
            get
            {
                return _newResourceName;
            }
            private set
            {
                _newResourceName = value;
                OnPropertyChanged(() => NewResourceName);
            }
        }

        private bool _fixReferences;
        public bool FixReferences
        {
            get
            {
                return _fixReferences;
            }
            private set
            {
                _fixReferences = value;
                OnPropertyChanged(() => FixReferences);
            }
        }
        public ICommand CancelCommand { get; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public ICommand CreateCommand { get; private set; }

        public DuplicateResourceViewModel(ICreateDuplicateResourceView createDuplicateResourceView)
        {
            _createDuplicateResourceView = createDuplicateResourceView;
            CancelCommand = new DelegateCommand(CancelAndClose);
        }

        private void CancelAndClose(object obj)
        {
            _createDuplicateResourceView.CloseView();
        }

        public void ShowDialog()
        {
            _createDuplicateResourceView.DataContext = this;
            _createDuplicateResourceView.ShowView();
        }
    }
}
