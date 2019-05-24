#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public class MainViewModel : Conductor<SettingsViewModelBase>.Collection.OneActive
    {
        #region Fields

        ObservableCollection<string> _errors;
        UserControl _settingsView;
        Visibility _errorsVisible;
        XElement _initConfigXml;

        RelayCommand _saveCommand;
        RelayCommand _cancelCommand;
        RelayCommand _clearErrorsCommand;
        bool _saveSuccess;

        #endregion

        #region Constructor

        public MainViewModel(XElement configurationXml, Func<XElement, XElement> saveCallback, System.Action cancelCallback, System.Action settingChangedCallback)
        {
            Errors = new ObservableCollection<string>();
            ClearErrors();

            if(!SetConfiguration(configurationXml))
            {
                return;
            }

            SaveCallback = saveCallback;
            CancelCallback = cancelCallback;
            SettingChangedCallback = settingChangedCallback;
        }

        bool SetConfiguration(XElement configurationXml)
        {
            // Check for null
            if (configurationXml == null)
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
            catch (Exception)
            {
                SetError(string.Format(ErrorResource.ErrorParsingInput, configurationXml));
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

        public UserControl SettingsView
        {
            get
            {
                return _settingsView;
            }            
            private set            
            {
                _settingsView = value ?? throw new ArgumentNullException("value");
                NotifyOfPropertyChange(() => SettingsView);
            }
        }

        public RelayCommand SaveCommand => _saveCommand ??
                       (_saveCommand = new RelayCommand(param => Save(), parm => CanSaveConfig()));

        public RelayCommand CancelCommand => _cancelCommand ??
                       (_cancelCommand = new RelayCommand(param => Cancel(), parm => CanCancel()));

        public RelayCommand ClearErrorsCommand => _clearErrorsCommand ??
                       (_clearErrorsCommand = new RelayCommand(param => ClearErrors()));

        public Settings.Configuration Configuration { get; private set; }

        #endregion

        #region Private Properties

        Func<XElement, XElement> SaveCallback { get; set; }
        System.Action CancelCallback { get; set; }

        System.Action SettingChangedCallback { get; set; }


        #endregion

        #region Private Methods

        void ConfigurationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "HasChanges":
                    CommandManager.InvalidateRequerySuggested();
                    SaveCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                    ClearErrorsCommand.RaiseCanExecuteChanged();
                    if (Configuration.HasChanges)
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
                default:
                    break;
            }
        }

        bool CanSaveConfig() => Configuration.HasChanges && !Configuration.HasError;

        bool CanCancel() => Configuration.HasChanges;

        void Save()
        {
            SaveSuccess = false;

            if (SaveCallback == null)
            {
                return;
            }

            try
            {
                var newConfig = SaveCallback(Configuration.ToXml());
                SetConfiguration(newConfig);
                SaveSuccess = true;
            }
            catch (Exception ex)
            {
                SaveSuccess = false;
                SetError(string.Format(ErrorResource.ErrorDuringSaveCallback, ex.Message));
            }

        }

        void Cancel()
        {
            SaveSuccess = false;

            if (CancelCallback == null)
            {
                return;
            }

            try
            {
                CancelCallback();
                SetConfiguration(_initConfigXml);
            }
            catch (Exception ex)
            {
                SetError(string.Format(ErrorResource.ErrorDuringCancelCallback, ex.Message));
            }
        }

        void ClearErrors()
        {
            Errors.Clear();
            ErrorsVisible = Visibility.Collapsed;
        }

        void SetError(string error)
        {
            Errors.Add(error);
            ErrorsVisible = Errors.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        
        #endregion
    }
}
