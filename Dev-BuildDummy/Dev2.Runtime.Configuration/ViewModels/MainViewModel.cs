using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Impl

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Fields

        private ObservableCollection<string> _errors;
        private List<SettingsObject> _settingsObjects;
        private SettingsObject _selectedSettingsObjects;
        private UserControl _settingsView;
        private Visibility _errorsVisible;

        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _clearErrorsCommand;

        #endregion

        #region Constructor

        public MainViewModel(XElement configurationXML, Action<XElement> saveCallback, Action cancelCallback, Action settingChangedCallback)
        {
            Errors = new ObservableCollection<string>();
            ClearErrors();

            // Check for null
            if (configurationXML == null)
            {
                SetError("'configurationXML' of the MainViewModel was null.");
                return;
            }
            
            // Try parse configuration xml
            try
            {
                Configuration = new Settings.Configuration(configurationXML);
            }
            catch(Exception)
            {
                SetError(string.Format("Error parsing '{0}' input.", configurationXML));
                return;
            }

            // Try create settings graph
            try
            {
                SettingsObjects = SettingsObject.BuildGraph(Configuration);
            }
            catch(Exception)
            {
                SetError(string.Format("Error building settings graph from '{0}'.", configurationXML));
                return;
            }

            SaveCallback = saveCallback;
            CancelCallback = cancelCallback;
            SettingChangedCallback = settingChangedCallback;
        }

        #endregion

        #region Properties

        public Visibility ErrorsVisible
        {
            get
            {
                return _errorsVisible;
            }
            private set
            {
                _errorsVisible = value;
                OnPropertyChanged("ErrorsVisible");
            }
        }

        public List<SettingsObject> SettingsObjects
        {
            get
            {
                return _settingsObjects;
            }
            private set
            {
                _settingsObjects = value;
                OnPropertyChanged("SettingsObjects");
            }
        }

        public ObservableCollection<string> Errors
        {
            get
            {
                return _errors;
            }
            private set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public SettingsObject SelectedSettingsObjects
        {
            get
            {
                return _selectedSettingsObjects;
            }
            set
            {
                _selectedSettingsObjects = value;
                OnPropertyChanged("SelectedSettingsObjects");
                UpdateSettingsView(_selectedSettingsObjects);
            }
        }

        public UserControl SettingsView
        {
            get
            {
                return _settingsView;
            }
            private set
            {
                _settingsView = value;
                OnPropertyChanged("SettingsView");
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand(param => Save()));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                       (_cancelCommand = new RelayCommand(param => Cancel()));
            }
        }

        public ICommand ClearErrorsCommand
        {
            get
            {
                return _clearErrorsCommand ??
                       (_clearErrorsCommand = new RelayCommand(param => ClearErrors()));
            }
        }

        public Settings.Configuration Configuration { get; set; }

        #endregion

        #region Private Properties

        private Action<XElement> SaveCallback { get; set; }
        private Action CancelCallback { get; set; }
        private Action SettingChangedCallback { get; set; }

        #endregion

        #region Private Methods

        private void Save()
        {
            if (SaveCallback == null)
            {
                return;
            }

            try
            {
                SaveCallback(Configuration.ToXml());
            }
            catch(Exception ex)
            {
                SetError(string.Format("The following error occured while executing the save callback '{0}'.", ex.Message));
            }
        }

        private void Cancel()
        {
            if (CancelCallback == null)
            {
                return;
            }

            try
            {
                CancelCallback();
            }
            catch (Exception ex)
            {
                SetError(string.Format("The following error occured while executing the cancel callback '{0}'.", ex.Message));
            }
        }

        private void ClearErrors()
        {
            Errors.Clear();
            ErrorsVisible = Visibility.Collapsed;
        }

        private void SetError(string error)
        {
            Errors.Add(error);
            if (Errors.Count == 0)
            {
                ErrorsVisible = Visibility.Collapsed;
            }
            else
            {
                ErrorsVisible = Visibility.Visible;
            }
        }

        private void UpdateSettingsView(SettingsObject settingsObject)
        {
            if (!settingsObject.IsSelected)
            {
                settingsObject.IsSelected = true;
            }

            // Instantiate view model
            SettingsViewModelBase viewModel = CreateViewModel(settingsObject.ViewModel, settingsObject.Object);
            if (viewModel == null)
            {
                SetError("Couldn't find a view model for currently seelcted settings.");
                SettingsView = null;
                return;
            }

            // Instantiate view
            UserControl view = CreateView(settingsObject.View, viewModel);
            if (view == null)
            {
                SetError("Couldn't find a view for currently seelcted settings.");
                SettingsView = null;
                return;
            }

            SettingsView = view;
        }

        private SettingsViewModelBase CreateViewModel(Type viewModelType, object Object)
        {
            SettingsViewModelBase viewModel = null;
            try
            {
                viewModel = Activator.CreateInstance(viewModelType) as SettingsViewModelBase;
            }
            finally
            {
                if (viewModel != null)
                {
                    viewModel.Object = Object;
                }
            }

            return viewModel;
        }

        private UserControl CreateView(Type viewType, object dataContext)
        {
            UserControl view = null;
            try
            {
                view = Activator.CreateInstance(viewType) as UserControl;
            }
            finally
            {
                if (view != null)
                {
                    view.DataContext = dataContext;
                }
            }

            return view;
        }

        #endregion
    }
}
