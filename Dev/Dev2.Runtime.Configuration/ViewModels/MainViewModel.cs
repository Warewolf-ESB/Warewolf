
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Services;
using Dev2.Runtime.Configuration.ViewModels.Base;

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
        private XElement _initConfigXml;

        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _clearErrorsCommand;
        private bool _saveSuccess;

        #endregion

        #region Constructor

        public MainViewModel(XElement configurationXml, Func<XElement, XElement> saveCallback, System.Action cancelCallback, System.Action settingChangedCallback)
        {
            Errors = new ObservableCollection<string>();
            ClearErrors();

            if(!SetConfiguration(configurationXml)) return;

            SaveCallback = saveCallback;
            CancelCallback = cancelCallback;
            SettingChangedCallback = settingChangedCallback;

            CommunicationService = new WebCommunicationService();
        }

        private bool SetConfiguration(XElement configurationXml)
        {
            // Check for null
            if(configurationXml == null)
            {
                SetError("'configurationXML' of the MainViewModel was null.");
                return false;
            }

            // Try parse configuration xml
            try
            {
                Configuration = new Settings.Configuration(configurationXml);
                Configuration.PropertyChanged += ConfigurationPropertyChanged;
            }
            catch(Exception)
            {
                SetError(string.Format("Error parsing '{0}' input.", configurationXml));
                return false;
            }

            // Try create settings graph
            try
            {
                SettingsObjects = SettingsObject.BuildGraph(Configuration);
            }
            catch(Exception)
            {
                SetError(string.Format("Error building settings graph from '{0}'.", configurationXml));
                return false;
            }

            _initConfigXml = configurationXml;
            return true;
        }

        #endregion

        #region Properties

        public bool SaveSuccess
        {
            get
            {
                return _saveSuccess;
            }
            private set
            {
                _saveSuccess = value;
                NotifyOfPropertyChange(() => SaveSuccess);
            }
        }

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
            // ReSharper disable UnusedMember.Local
            private set
            // ReSharper restore UnusedMember.Local
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _settingsView = value;
                NotifyOfPropertyChange(() => SettingsView);
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand(param => Save(), parm => CanSaveConfig()));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                       (_cancelCommand = new RelayCommand(param => Cancel(), parm => CanCancel()));
            }
        }

        public RelayCommand ClearErrorsCommand
        {
            get
            {
                return _clearErrorsCommand ??
                       (_clearErrorsCommand = new RelayCommand(param => ClearErrors()));
            }
        }

        public Settings.Configuration Configuration { get; private set; }

        public ICommunicationService CommunicationService { get; set; }

        #endregion

        #region Private Properties

        private Func<XElement, XElement> SaveCallback { get; set; }
        private System.Action CancelCallback { get; set; }
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private System.Action SettingChangedCallback { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        #endregion

        #region Private Methods

        private void ConfigurationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "HasChanges":
                    CommandManager.InvalidateRequerySuggested();
                    SaveCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                    ClearErrorsCommand.RaiseCanExecuteChanged();
                    if(Configuration.HasChanges)
                    {
                        SaveSuccess = false;
                    }
                    break;
                case "HasError":
                    CommandManager.InvalidateRequerySuggested();
                    SaveCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                    ClearErrorsCommand.RaiseCanExecuteChanged();
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
            SaveSuccess = false;

            if(SaveCallback == null)
            {
                return;
            }

            try
            {
                XElement newConfig = SaveCallback(Configuration.ToXml());
                SetConfiguration(newConfig);
                SaveSuccess = true;
            }
            catch(Exception ex)
            {
                SaveSuccess = false;
                SetError(string.Format("The following error occurred while executing the save callback '{0}'.", ex.Message));
            }

        }

        private void Cancel()
        {
            SaveSuccess = false;

            if(CancelCallback == null)
            {
                return;
            }

            try
            {
                CancelCallback();
                SetConfiguration(_initConfigXml);
            }
            catch(Exception ex)
            {
                SetError(string.Format("The following error occurred while executing the cancel callback '{0}'.", ex.Message));
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
            ErrorsVisible = Errors.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateSettingsView(SettingsObject settingsObject)
        {
            if(!settingsObject.IsSelected)
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
                if(viewModel != null)
                {
                    viewModel.CommunicationService = CommunicationService;
                    viewModel.Object = Object;
                }
            }

            return viewModel;
        }
        #endregion
    }
}
