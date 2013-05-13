using Caliburn.Micro;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Action = System.Action;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public class MainViewModel : Conductor<SettingsViewModelBase>.Collection.OneActive
    {
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
                Configuration.PropertyChanged += ConfigurationPropertyChanged;
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
                NotifyOfPropertyChange(() => ErrorsVisible);
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
                NotifyOfPropertyChange(() => SettingsObjects);
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
                NotifyOfPropertyChange(() => Errors);
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
                NotifyOfPropertyChange(() => SelectedSettingsObjects);
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
                NotifyOfPropertyChange(() => SettingsView);
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand(param => Save(), parm => CanSaveConfig()));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                       (_cancelCommand = new RelayCommand(param => Cancel(), parm => CanCancel()));
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

        public Settings.Configuration Configuration { get; private set; }

        #endregion

        #region Private Properties

        private Action<XElement> SaveCallback { get; set; }
        private Action CancelCallback { get; set; }
        private Action SettingChangedCallback { get; set; }

        #endregion

        #region Private Methods

        private static void ConfigurationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "HasChanges":
                    CommandManager.InvalidateRequerySuggested();
                    break;
                case "HasError":
                    CommandManager.InvalidateRequerySuggested();
                    break;
            }
        }

        private bool CanSaveConfig()
        {
            return Configuration.HasChanges && !Configuration.HasError;
        }

        private bool CanCancel()
        {
            return Configuration.HasChanges;
        }

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
            Configuration.HasChanges = false;
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
            var vm = CreateViewModel(settingsObject.ViewModel, settingsObject.Object);
            Items.Add(vm);
            ActivateItem(vm);
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
        #endregion
    }
}
