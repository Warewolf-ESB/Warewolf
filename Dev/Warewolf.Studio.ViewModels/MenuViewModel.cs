using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core.View_Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class MenuViewModel : BindableBase, IMenuViewModel, IMenuView
    {

        bool _hasNewVersion;
        bool _panelLocked;
        bool _panelOpen;
        int _buttonWidth;


        public MenuViewModel(IShellViewModel shellViewModel)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            shellViewModel.ActiveServerChanged += ShellViewModelOnActiveServerChanged;
            NewCommand = new DelegateCommand<ResourceType?>(shellViewModel.NewResource, type => CanCreateNewService);
            DeployCommand = new DelegateCommand(() => shellViewModel.DeployService(null), () => CanDeploy);
            SaveCommand = new DelegateCommand(shellViewModel.SaveService, () => CanSave);
            OpenSchedulerCommand = new DelegateCommand(shellViewModel.OpenScheduler, () => CanSetSchedules);
            OpenSettingsCommand = new DelegateCommand(shellViewModel.OpenSettings, () => CanSetSettings);
            ExecuteServiceCommand = new DelegateCommand(shellViewModel.ExecuteService, () => CanExecuteService);
            CheckForNewVersion(shellViewModel);
            CheckForNewVersionCommand = new DelegateCommand(shellViewModel.DisplayDialogForNewVersion);

            LockCommand = new DelegateCommand(Lock);
            SlideOpenCommand = new DelegateCommand(() => SlideOpen(shellViewModel));
            SlideClosedCommand = new DelegateCommand(() =>
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (shellViewModel.MenuPanelWidth >= 80)
                {
                    SlideClosed(shellViewModel);
                }
            });

            ButtonWidth = 115;
            IsPanelLocked = true;
            IsPanelOpen = true;



        }



        public ICommand DeployCommand { get; set; }
        public ICommand NewCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand OpenSettingsCommand { get; set; }
        public ICommand OpenSchedulerCommand { get; set; }
        public ICommand ExecuteServiceCommand { get; set; }

        public ICommand LockCommand { get; set; }
        public ICommand SlideOpenCommand { get; set; }
        public ICommand SlideClosedCommand { get; set; }
        public ICommand CheckForNewVersionCommand { get; set; }






        public string LockImage
        {
            get
            {
                if (IsPanelLocked)
                    return "Lock";
                return "UnlockAlt";

            }

        }


        void UpdateProperties()
        {


            OnPropertyChanged(() => NewLabel);
            OnPropertyChanged(() => SaveLabel);
            OnPropertyChanged(() => DeployLabel);
            OnPropertyChanged(() => DatabaseLabel);
            OnPropertyChanged(() => DLLLabel);
            OnPropertyChanged(() => WebLabel);
            OnPropertyChanged(() => TaskLabel);
            OnPropertyChanged(() => DebugLabel);
            OnPropertyChanged(() => SettingsLabel);
            OnPropertyChanged(() => SupportLabel);
            OnPropertyChanged(() => ForumsLabel);
            OnPropertyChanged(() => ToursLabel);
            OnPropertyChanged(() => NewVersionLabel);
            OnPropertyChanged(() => LockLabel);

            OnPropertyChanged(() => ButtonWidth);

        }


        public void Lock()
        {

            bool xxx = IsPanelOpen;
            int yyy = ButtonWidth;

            if (IsPanelLocked)
            {

                if (IsPanelOpen && ButtonWidth == 35)
                    ButtonWidth = 115;
                //else if (IsPanelOpen && ButtonWidth == 115)
                //    ButtonWidth = 35;
                
                IsPanelLocked = false;
            }
            else // panel not locked
            {

                if (IsPanelOpen && ButtonWidth == 115)
                {
                    ButtonWidth = 35;

                }

                IsPanelLocked = true;
            }

            UpdateProperties();

        }


        public void SlideOpen(IShellViewModel shellViewModel)
        {
            if (IsPanelLocked)//&& IsPanelOpen)
            {
                IsPanelOpen = true;
                shellViewModel.MenuExpanded = IsPanelOpen;

                ButtonWidth = 115;

                UpdateProperties();
            }
        }

        public void SlideClosed(IShellViewModel shellViewModel)
        {
            if (IsPanelLocked && !IsPanelOpen)
            {
                shellViewModel.MenuExpanded = !IsPanelOpen;
                ButtonWidth = 35;

            }
            else if (IsPanelLocked && IsPanelOpen)
            {
                shellViewModel.MenuExpanded = !IsPanelOpen;
                ButtonWidth = 115;
            }

            //IsPanelOpen = !IsPanelOpen;
            UpdateProperties();
        }


        async void CheckForNewVersion(IShellViewModel shellViewModel)
        {
            HasNewVersion = await shellViewModel.CheckForNewVersion();
        }



        public int ButtonWidth
        {
            get
            {
                return _buttonWidth;
            }
            set
            {
                _buttonWidth = value;
            }
        }


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

        public bool IsPanelOpen
        {
            get
            {
                return _panelOpen;
            }
            set
            {
                _panelOpen = value;
                //OnPropertyChanged(() => LockLabel);
                //OnPropertyChanged(() => LockImage);
            }

        }


        public bool IsPanelLocked
        {
            get
            {
                return _panelLocked;
            }
            set
            {
                _panelLocked = value;
                OnPropertyChanged(() => LockLabel);
                OnPropertyChanged(() => LockImage);



            }
        }

        void ShellViewModelOnActiveServerChanged()
        {
            UpdateCommandExecutionBaseOnPermissions();
        }

        void UpdateCommandExecutionBaseOnPermissions()
        {
            CanCreateNewService = true;
            CanDeploy = true;
            CanSave = true;
            CanSetSchedules = true;
            CanSetSettings = true;
            CanExecuteService = true;
        }

        public bool CanExecuteService { get; set; }
        public bool CanSetSettings { get; set; }
        public bool CanSetSchedules { get; set; }
        public bool CanSave { get; set; }
        public bool CanDeploy { get; set; }
        public bool CanCreateNewService { get; set; }


        public string NewLabel
        {
            get
            {
                if ( ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogNewLabel;
                return String.Empty;
            }
        }
        public string SaveLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogSaveLabel;
                return String.Empty;
            }
        }
        public string DeployLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDeployLabel;
                return String.Empty;
            }
        }
        public string DatabaseLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDatabaseLabel;
                return String.Empty;
            }

        }
        public string DLLLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDLLLabel;
                return String.Empty;
            }
        }
        public string WebLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogWebLabel;
                return String.Empty;
            }
        }
        public string TaskLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogTaskLabel;
                return String.Empty;
            }
        }
        public string DebugLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogDebugLabel;
                return String.Empty;
            }
        }
        public string SettingsLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogSettingsLabel;
                return String.Empty;
            }
        }
        public string SupportLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogSupportLabel;
                return String.Empty;
            }
        }
        public string ForumsLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogForumsLabel;
                return String.Empty;
            }
        }
        public string ToursLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogToursLabel;
                return String.Empty;
            }
        }
        public string NewVersionLabel
        {
            get
            {
                if (ButtonWidth == 115)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogNewVersionLabel;
                return String.Empty;
            }
        }
        public string LockLabel
        {
            get
            {
                if (IsPanelLocked)
                    // ReSharper disable once MaximumChainedReferences
                    return Resources.Languages.Core.MenuDialogLockLabel;
                return Resources.Languages.Core.MenuDialogUnLockLabel;

            }
        }


        public object DataContext { get; set; }
    }
}