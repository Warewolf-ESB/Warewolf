/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio;
using Dev2.Interfaces;
using FontAwesome.WPF;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
// ReSharper disable MemberCanBePrivate.Global

namespace Warewolf.Studio.ViewModels
{
    public class MenuViewModel : BindableBase, IMenuViewModel, IMenuView,IUpdatesHelp
    {

        bool _hasNewVersion;
        bool _panelLockedOpen;
        readonly IMainViewModel _viewModel;
        bool _isOverLock;
        ICommand _saveCommand;
        ICommand _executeServiceCommand;
        FontAwesomeIcon _debugIcon;
        bool _isProcessing;

        public MenuViewModel(IMainViewModel mainViewModel)
        {
            if (mainViewModel == null)
            {
                throw new ArgumentNullException(nameof(mainViewModel));
            }
            _viewModel = mainViewModel;
            _isOverLock = false;
            NewServiceCommand = _viewModel.NewServiceCommand;
            DeployCommand = _viewModel.DeployCommand;
            SaveCommand = _viewModel.SaveCommand;
            OpenSchedulerCommand = _viewModel.SchedulerCommand;
            OpenSettingsCommand = _viewModel.SettingsCommand;
            ExecuteServiceCommand = _viewModel.DebugCommand;
            StartPageCommand = _viewModel.ShowStartPageCommand;
            OnPropertyChanged(() => SaveCommand);
            OnPropertyChanged(() => ExecuteServiceCommand);
            CheckForNewVersion(_viewModel);
            CheckForNewVersionCommand = new DelegateCommand(_viewModel.DisplayDialogForNewVersion);
            SupportCommand = new DelegateCommand(() =>
            {
                Process.Start(Resources.Languages.HelpText.WarewolfHelpURL);
            });

            LockCommand = new DelegateCommand(Lock);
            SlideOpenCommand = new DelegateCommand(() =>
            {
                if (!_isOverLock)
                {
                    SlideOpen(_viewModel);
                }
            });
            SlideClosedCommand = new DelegateCommand(() =>
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (_viewModel.MenuPanelWidth >= 80 && !_isOverLock)
                {
                    SlideClosed(_viewModel);
                }
            });
            IsOverLockCommand = new DelegateCommand(() => _isOverLock = true);
            IsNotOverLockCommand = new DelegateCommand(() => _isOverLock = false);
            ButtonWidth = 125;
            IsPanelLockedOpen = true;
            IsPanelOpen = true;
            DebugIcon = FontAwesomeIcon.Play;
            
        }

        public FontAwesomeIcon DebugIcon
        {
            get
            {
                return _debugIcon;
            }
            set
            {
                _debugIcon = value;
                OnPropertyChanged(() => DebugIcon);
            }
        }

        public ICommand SupportCommand { get; set; }
        public ICommand DeployCommand { get; set; }
        public ICommand NewServiceCommand { get; set; }
        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand;
            }
            set
            {
                _saveCommand = value;
                OnPropertyChanged(() => SaveCommand);
            }
        }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenSchedulerCommand { get; set; }
        public ICommand ExecuteServiceCommand
        {
            get
            {
                return _executeServiceCommand;
            }
            set
            {
                _executeServiceCommand = value;
                OnPropertyChanged(() => ExecuteServiceCommand);
            }
        }
        public ICommand LockCommand { get; set; }
        public ICommand SlideOpenCommand { get; set; }
        public ICommand SlideClosedCommand { get; set; }
        public ICommand CheckForNewVersionCommand { get; set; }

        public string LockImage
        {
            get
            {
                if (IsPanelLockedOpen)
                    return @"UnlockAlt";
                return @"Lock";
            }
        }

        void UpdateProperties()
        {
            OnPropertyChanged(() => NewLabel);
            OnPropertyChanged(() => SaveLabel);
            OnPropertyChanged(() => DeployLabel);
            OnPropertyChanged(() => TaskLabel);
            OnPropertyChanged(() => DebugLabel);
            OnPropertyChanged(() => SettingsLabel);
            OnPropertyChanged(() => SupportLabel);
            OnPropertyChanged(() => NewVersionLabel);
            OnPropertyChanged(() => LockLabel);
            OnPropertyChanged(() => ButtonWidth);
        }

        public ICommand IsOverLockCommand { get; private set; }
        public ICommand IsNotOverLockCommand { get; private set; }

        public ICommand StartPageCommand { get; private set; }

        public void UpdateHelpDescriptor(string helpText)
        {
            _viewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public void Lock()
        {
            if (!IsPanelLockedOpen)
            {
                IsPanelLockedOpen = true;
            }
            else
            {
                if (!IsPanelOpen && ButtonWidth == 125)
                    ButtonWidth = 35;
                if (IsPanelOpen && ButtonWidth == 35)
                    ButtonWidth = 125;

                IsPanelLockedOpen = false;
            }

            UpdateProperties();
        }

        void SlideOpen(IMainViewModel mainViewModel)
        {
            if (IsPanelLockedOpen)
            {
                IsPanelOpen = true;
                mainViewModel.MenuExpanded = IsPanelOpen;
                ButtonWidth = 125;
                UpdateProperties();
            }
        }

        void SlideClosed(IMainViewModel mainViewModel)
        {
            if (IsPanelLockedOpen && !IsPanelOpen)
            {
                mainViewModel.MenuExpanded = !IsPanelOpen;
                ButtonWidth = 35;
                IsPanelOpen = !IsPanelOpen;
            }
            else if (IsPanelLockedOpen && IsPanelOpen)
            {
                mainViewModel.MenuExpanded = !IsPanelOpen;
                ButtonWidth = 125;
                IsPanelOpen = !IsPanelOpen;
            }

            UpdateProperties();
        }

        async void CheckForNewVersion(IMainViewModel mainViewModel)
        {
            if(mainViewModel != null)
            {
                HasNewVersion = await mainViewModel.CheckForNewVersion();
            }
        }

        public int ButtonWidth { get; set; }

        public bool HasNewVersion
        {
            get
            {
                return _hasNewVersion;
            }
            set
            {
                _hasNewVersion = value;
                OnPropertyChanged(() => HasNewVersion);
            }
        }

        public bool IsPanelOpen { get; set; }

        public bool IsPanelLockedOpen
        {
            get
            {
                return _panelLockedOpen;
            }
            set
            {
                _panelLockedOpen = value;
                OnPropertyChanged(() => LockLabel);
                OnPropertyChanged(() => LockImage);
            }
        }

        
        public string NewLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return Resources.Languages.Core.MenuDialogNewLabel;
                return string.Empty;
            }
        }
        public string SaveLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return Resources.Languages.Core.MenuDialogSaveLabel;
                return string.Empty;
            }
        }
        public string DeployLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return Resources.Languages.Core.MenuDialogDeployLabel;
                return string.Empty;
            }
        }
        public string TaskLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return Resources.Languages.Core.MenuDialogTaskLabel;
                return string.Empty;
            }
        }
        public bool IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                SetProperty(ref _isProcessing, value);
                DebugIcon = _isProcessing ? FontAwesomeIcon.Stop : FontAwesomeIcon.Play;
                OnPropertyChanged(()=>DebugLabel);
            }
        }
        public string DebugLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return IsProcessing ? Resources.Languages.Core.MenuDialogStopDebugLabel : Resources.Languages.Core.MenuDialogDebugLabel;
                return string.Empty;
            }
        }
        public string SettingsLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return Resources.Languages.Core.MenuDialogSettingsLabel;
                return string.Empty;
            }
        }
        public string SupportLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return Resources.Languages.Core.MenuDialogSupportLabel;
                return string.Empty;
            }
        }
        public string NewVersionLabel
        {
            get
            {
                if (ButtonWidth == 125)
                    return Resources.Languages.Core.MenuDialogNewVersionLabel;
                return string.Empty;
            }
        }
        public string LockLabel
        {
            get
            {
                if (IsPanelLockedOpen)
                    return Resources.Languages.Core.MenuDialogLockLabel;
                return Resources.Languages.Core.MenuDialogUnLockLabel;
            }
        }

        public object DataContext { get; set; }
    }
}
