#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
            CheckForNewVersion();
            CheckForNewVersionCommand = new DelegateCommand(ShellViewModel.DisplayDialogForNewVersion);
            SupportCommand = new DelegateCommand(OpenSupport);
            LockCommand = new DelegateCommand(Lock);
            SlideOpenCommand = new DelegateCommand(() =>
            {
                if (!_isOverLock)
                {
                    SlideOpen();
                }
            });
            SlideClosedCommand = new DelegateCommand(() =>
            {
                if (ShellViewModel.MenuPanelWidth >= 80 && !_isOverLock)
                {
                    SlideClosed();
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

        private static void OpenSupport()
        {
            var applicationTracker = CustomContainer.Get<IApplicationTracker>();
            applicationTracker?.TrackEvent(Resources.Languages.TrackEventHelp.EventCategory, Resources.Languages.TrackEventHelp.Help);
            Process.Start(Resources.Languages.HelpText.WarewolfHelpURL);
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
            ShellViewModel?.HelpViewModel?.UpdateHelpText(helpText);
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

        void SlideOpen()
        {
            if (IsPanelLockedOpen)
            {
                IsPanelOpen = true;
                ShellViewModel.MenuExpanded = IsPanelOpen;
                ButtonWidth = ButtonWidthLarge;
                UpdateProperties();
            }
        }

        void SlideClosed()
        {
            if (IsPopoutViewOpen)
            {
                return;
            }
            if (IsPanelLockedOpen && !IsPanelOpen)
            {
                ShellViewModel.MenuExpanded = !IsPanelOpen;
                IsPanelOpen = !IsPanelOpen;
            }
            else
            {
                if (IsPanelLockedOpen && IsPanelOpen)
                {
                    ShellViewModel.MenuExpanded = !IsPanelOpen;
                    ButtonWidth = ButtonWidthLarge;
                    IsPanelOpen = !IsPanelOpen;
                }
            }

            UpdateProperties();
        }

        async void CheckForNewVersion()
        {
            HasNewVersion = await ShellViewModel.CheckForNewVersionAsync().ConfigureAwait(true);
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

        public string NewLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogNewLabel : string.Empty;

        public string SaveLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogSaveLabel : string.Empty;

        public string DeployLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogDeployLabel : string.Empty;

        public string SearchLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogSearchLabel : string.Empty;

        public string TaskLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogTaskLabel : string.Empty;

        public string SchedulerLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogSchedulerLabel : string.Empty;

        public string QueueEventsLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogQueueEventsLabel : string.Empty;

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
        public string SettingsLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogSettingsLabel : string.Empty;

        public string SupportLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogSupportLabel : string.Empty;

        public string NewVersionLabel => ButtonWidth >= ButtonWidthLarge ? Resources.Languages.Core.MenuDialogNewVersionLabel : string.Empty;

        public string LockLabel => IsPanelLockedOpen ? Resources.Languages.Core.MenuDialogLockLabel : Resources.Languages.Core.MenuDialogUnLockLabel;

        public object DataContext { get; set; }
    }
}
