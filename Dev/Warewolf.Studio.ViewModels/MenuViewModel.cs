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
using System.Diagnostics;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio;
using Dev2.Studio.Interfaces;
using FontAwesome.WPF;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

using Dev2;
using Dev2.Instrumentation;

namespace Warewolf.Studio.ViewModels
{
    public class MenuViewModel : BindableBase, IMenuViewModel, IMenuView,IUpdatesHelp
    {
        bool _hasNewVersion;
        bool _panelLockedOpen;
        private IShellViewModel _viewModel;
        bool _isOverLock;
        ICommand _saveCommand;
        ICommand _executeServiceCommand;
        FontAwesomeIcon _debugIcon;
        bool _isProcessing;
        const int ButtonWidthSmall = 35;
        const int ButtonWidthLarge = 125;

        public MenuViewModel(IShellViewModel mainViewModel)
        {
            ShellViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _isOverLock = false;
            SaveCommand = _viewModel.SaveCommand;
            OpenSettingsCommand = _viewModel.SettingsCommand;
            ExecuteServiceCommand = _viewModel.DebugCommand;
            OnPropertyChanged(() => SaveCommand);
            OnPropertyChanged(() => ExecuteServiceCommand);
            CheckForNewVersion(_viewModel);
            CheckForNewVersionCommand = new DelegateCommand(_viewModel.DisplayDialogForNewVersion);
            SupportCommand = new DelegateCommand(() =>
            {
                var applicationTracker = CustomContainer.Get<IApplicationTracker>();
                if (applicationTracker != null)
                {
                    applicationTracker.TrackEvent(Resources.Languages.TrackEventHelp.EventCategory,
                                                        Resources.Languages.TrackEventHelp.Help);
                }
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
                
                if (_viewModel.MenuPanelWidth >= 80 && !_isOverLock)
                {
                    SlideClosed(_viewModel);
                }
            });
            IsOverLockCommand = new DelegateCommand(() => _isOverLock = true);
            IsNotOverLockCommand = new DelegateCommand(() => _isOverLock = false);
            ButtonWidth = ButtonWidthLarge;
            IsPanelLockedOpen = true;
            IsPanelOpen = true;
            IsPopoutViewOpen = false;
            DebugIcon = FontAwesomeIcon.Bug;
            
        }

        public IShellViewModel ShellViewModel
        {
            get => _viewModel;
            set => SetProperty(ref _viewModel, value);
        }
        
        public FontAwesomeIcon DebugIcon
        {
            get => _debugIcon;
            set
            {
                _debugIcon = value;
                OnPropertyChanged(() => DebugIcon);
            }
        }

        public ICommand SupportCommand { get; set; }
        public ICommand SaveCommand
        {
            get => _saveCommand;
            set
            {
                _saveCommand = value;
                OnPropertyChanged(() => SaveCommand);
            }
        }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand ExecuteServiceCommand
        {
            get => _executeServiceCommand;
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

        public string LockImage => IsPanelLockedOpen ? @"UnlockAlt" : @"Lock";

        void UpdateProperties()
        {
            OnPropertyChanged(() => NewLabel);
            OnPropertyChanged(() => SaveLabel);
            OnPropertyChanged(() => DeployLabel);
            OnPropertyChanged(() => SearchLabel);
            OnPropertyChanged(() => TaskLabel);
            OnPropertyChanged(() => SchedulerLabel);
            OnPropertyChanged(() => QueueEventsLabel);
            OnPropertyChanged(() => DebugLabel);
            OnPropertyChanged(() => SettingsLabel);
            OnPropertyChanged(() => SupportLabel);
            OnPropertyChanged(() => NewVersionLabel);
            OnPropertyChanged(() => LockLabel);
            OnPropertyChanged(() => ButtonWidth);
        }

        public ICommand IsOverLockCommand { get; private set; }
        public ICommand IsNotOverLockCommand { get; private set; }

        public void UpdateHelpDescriptor(string helpText)
        {
            _viewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }

        public void Lock()
        {
            if (!IsPanelLockedOpen)
            {
                IsPanelLockedOpen = true;
            }
            else
            {
                if (!IsPanelOpen && ButtonWidth > ButtonWidthSmall)
                {
                    ButtonWidth = ButtonWidthSmall;
                }

                if (IsPanelOpen && ButtonWidth < ButtonWidthLarge)
                {
                    ButtonWidth = ButtonWidthLarge;
                }

                IsPanelLockedOpen = false;
            }

            UpdateProperties();
        }

        void SlideOpen(IShellViewModel mainViewModel)
        {
            if (IsPanelLockedOpen)
            {
                IsPanelOpen = true;
                mainViewModel.MenuExpanded = IsPanelOpen;
                ButtonWidth = ButtonWidthLarge;
                UpdateProperties();
            }
        }

        void SlideClosed(IShellViewModel mainViewModel)
        {
            if (IsPopoutViewOpen)
            {
                return;
            }
            if (IsPanelLockedOpen && !IsPanelOpen)
            {
                mainViewModel.MenuExpanded = !IsPanelOpen;
                IsPanelOpen = !IsPanelOpen;
            }
            else
            {
                if (IsPanelLockedOpen && IsPanelOpen)
                {
                    mainViewModel.MenuExpanded = !IsPanelOpen;
                    ButtonWidth = ButtonWidthLarge;
                    IsPanelOpen = !IsPanelOpen;
                }
            }

            UpdateProperties();
        }

        async void CheckForNewVersion(IShellViewModel mainViewModel)
        {
            if(mainViewModel != null)
            {
                HasNewVersion = await mainViewModel.CheckForNewVersionAsync().ConfigureAwait(true);
            }
        }

        public int ButtonWidth { get; set; }

        public bool HasNewVersion
        {
            get => _hasNewVersion;
            set
            {
                _hasNewVersion = value;
                OnPropertyChanged(() => HasNewVersion);
            }
        }

        public bool IsPanelOpen { get; set; }

        public bool IsPanelLockedOpen
        {
            get => _panelLockedOpen;
            set
            {
                _panelLockedOpen = value;
                OnPropertyChanged(() => LockLabel);
                OnPropertyChanged(() => LockImage);
            }
        }

        public bool IsPopoutViewOpen { get; set; }

        public string NewLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogNewLabel;
                }

                return string.Empty;
            }
        }
        public string SaveLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogSaveLabel;
                }

                return string.Empty;
            }
        }
        public string DeployLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogDeployLabel;
                }

                return string.Empty;
            }
        }
        public string SearchLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogSearchLabel;
                }
                return string.Empty;
            }
        }
        public string TaskLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogTaskLabel;
                }

                return string.Empty;
            }
        }

        public string SchedulerLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogSchedulerLabel;
                }

                return string.Empty;
            }
        }

        public string QueueEventsLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogQueueEventsLabel;
                }

                return string.Empty;
            }
        }
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                SetProperty(ref _isProcessing, value);
                DebugIcon = _isProcessing ? FontAwesomeIcon.Stop : FontAwesomeIcon.Bug;
                OnPropertyChanged(()=>DebugLabel);
            }
        }
        public string DebugLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return IsProcessing ? Resources.Languages.Core.MenuDialogStopDebugLabel : Resources.Languages.Core.MenuDialogDebugLabel;
                }

                return string.Empty;
            }
        }
        public string SettingsLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogSettingsLabel;
                }

                return string.Empty;
            }
        }
        public string SupportLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogSupportLabel;
                }

                return string.Empty;
            }
        }
        public string NewVersionLabel
        {
            get
            {
                if (ButtonWidth >= ButtonWidthLarge)
                {
                    return Resources.Languages.Core.MenuDialogNewVersionLabel;
                }

                return string.Empty;
            }
        }
        public string LockLabel
        {
            get
            {
                if (IsPanelLockedOpen)
                {
                    return Resources.Languages.Core.MenuDialogLockLabel;
                }

                return Resources.Languages.Core.MenuDialogUnLockLabel;
            }
        }

        public object DataContext { get; set; }
    }
}
