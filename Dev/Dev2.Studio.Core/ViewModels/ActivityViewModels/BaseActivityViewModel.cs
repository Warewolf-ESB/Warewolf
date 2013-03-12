using Dev2.Studio.Core.ViewModels.Base;
using System.Activities.Presentation.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace Dev2.Studio.Core.ViewModels.ActivityViewModels
{
    public abstract class BaseActivityViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ModelItem _modelItem;
        private ICommand _openWizardCommand;
        private ICommand _openSettingsCommand;
        private ICommand _openHelpCommand;
        private bool _showAdorners;
        private bool _hasWizard;
        private bool _showAdornersPreviousValue;
        private string _iconPath;
        private string _serviceName;
        private string _helpLink;
        private bool _hasHelpLink;

        #endregion Fields

        #region Ctor

        public BaseActivityViewModel(ModelItem modelItem)
        {
            _modelItem = modelItem;
            SetViewModelProperties(modelItem);
        }

        #endregion Ctor

        #region Properties

        public string IconPath
        {
            get
            {
                return _iconPath;
            }
            set
            {
                _iconPath = value;
                OnPropertyChanged("IconPath");
            }
        }

        public bool ShowAdorners
        {
            get
            {
                return _showAdorners;
            }
            set
            {
                _showAdorners = value;
                OnPropertyChanged("ShowAdorners");
            }
        }

        public bool ShowAdornersPreviousValue
        {
            get
            {
                return _showAdornersPreviousValue;
            }
            set
            {
                _showAdornersPreviousValue = value;
                OnPropertyChanged("ShowAdornersPreviousValue");
            }
        }

        public string SeriveName
        {
            get { return _serviceName; }
            set
            {
                _serviceName = value;
                OnPropertyChanged("SeriveName");
            }
        }

        public string HelpLink
        {
            get { return _helpLink; }
            set
            {
                _helpLink = value;
                OnPropertyChanged("HelpLink");
            }
        }

        public bool HasHelpLink
        {
            get
            {
                return _hasHelpLink;
            }
            set
            {
                _hasHelpLink = value;
                OnPropertyChanged("HasHelpLink");
            }
        }

        public bool HasWizard
        {
            get
            {
                return _hasWizard;
            }
            set
            {
                _hasWizard = value;
                OnPropertyChanged("HasWizard");
            }
        }

        #endregion Properties

        #region Commands

        public ICommand OpenWizardCommand
        {
            get
            {
                if (_openWizardCommand == null)
                {
                    _openWizardCommand = new RelayCommand(param => OpenWizard());
                }
                return _openWizardCommand;
            }
        }

        public ICommand OpenSettingsCommand
        {
            get
            {
                if (_openSettingsCommand == null)
                {
                    _openSettingsCommand = new RelayCommand(param => OpenSettings());
                }
                return _openSettingsCommand;
            }
        }

        public ICommand OpenHelpCommand
        {
            get
            {
                if (_openHelpCommand == null)
                {
                    _openHelpCommand = new RelayCommand(param => OpenHelp());
                }
                return _openHelpCommand;
            }
        }

        #endregion Commands

        #region Methods



        #endregion Methods

        #region Private Methods

        private void OpenWizard()
        {
            Mediator.SendMessage(MediatorMessages.ShowActivityWizard, _modelItem);
        }

        private void OpenSettings()
        {
            Mediator.SendMessage(MediatorMessages.ShowActivitySettingsWizard, _modelItem);
        }

        private void OpenHelp()
        {
            if (HasHelpLink)
            {
                Mediator.SendMessage(MediatorMessages.ShowHelpTab, HelpLink);
            }
        }

        public abstract void SetViewModelProperties(ModelItem _model);

        #endregion Private Methods

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged
    }
}
