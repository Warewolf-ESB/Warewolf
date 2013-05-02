using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Dev2.Studio.Core {
    public class WebResourceViewModel : SimpleBaseViewModel, IWebResourceViewModel {
        #region Locals
        private readonly ObservableCollection<IWebResourceViewModel> _children;
        RelayCommand _copyCommand;
        #endregion

        #region Properties
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public string Uri { get; set; }
        public string Base64Data { get; set; }
        public bool IsRoot { get; set; }

        public new IWebResourceViewModel Parent { get; private set; }

        private bool _isSelected;

        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                _isSelected = value;
                base.OnPropertyChanged("IsSelected");
            }
        }

        private bool _isExpanded;
        public bool IsExpanded {
            get {
                return _isExpanded;
            }
            set {
                _isExpanded = value;
                base.OnPropertyChanged("IsExpanded");
            }
        }

        public ObservableCollection<IWebResourceViewModel> Children {
            get { return _children; }
        }

        #endregion

        #region ctor
        public WebResourceViewModel(WebResourceViewModel parent) {
            Parent = parent;
            _children = new ObservableCollection<IWebResourceViewModel>();
        }

        #endregion

        #region Commands
        public ICommand CopyCommand {
            get {
                if (_copyCommand == null) {
                    _copyCommand = new RelayCommand(c => Copy());
                }
                return _copyCommand;
            }
        }
        #endregion

        #region Public Methods
        public void AddChild(IWebResourceViewModel d) {
            _children.Add(d);
        }
        public void SetParent(IWebResourceViewModel parent) {
            Parent = parent;
        }
        #endregion

        #region Private Methods
        private void Copy() {
            if (!string.IsNullOrEmpty(Name)) {
                Clipboard.SetText(Name);
            }
        }
        #endregion


    }
}
