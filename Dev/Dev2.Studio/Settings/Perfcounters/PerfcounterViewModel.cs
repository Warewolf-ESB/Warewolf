using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Dialogs;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ViewModels;

namespace Dev2.Settings.Perfcounters
{
    public class PerfcounterViewModel : SettingsItemViewModel, IHelpSource
    {
        IResourcePickerDialog _resourcePicker;

     
        readonly IEnvironmentModel _environment;
        bool _isUpdatingHelpText;

        internal PerfcounterViewModel(IPerformanceCounterTo counters, IWin32Window parentWindow, IEnvironmentModel environment)
            : this(counters, parentWindow, environment,null)
        {
        }

        public IResourcePickerDialog CreateResourcePickerDialog()
        {
            var env = GetEnvironment();
            var res =  new ResourcePickerDialog(enDsfActivityType.All, env);
            ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, env).ContinueWith(a=> _resourcePicker=a.Result);
            return res;
        }

        static IEnvironmentViewModel GetEnvironment()
        {
            var environment = EnvironmentRepository.Instance.ActiveEnvironment;

            IServer server = new Server(environment);

            if (server.Permissions == null)
            {
                server.Permissions = new List<IWindowsGroupPermission>();
                server.Permissions.AddRange(environment.AuthorizationService.SecurityService.Permissions);
            }
            var env = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);
            return env;
        }

        public PerfcounterViewModel(IPerformanceCounterTo securitySettings,  IWin32Window parentWindow, IEnvironmentModel environment, Func<IResourcePickerDialog> createfunc = null)
        {


            VerifyArgument.IsNotNull("parentWindow", parentWindow);
            VerifyArgument.IsNotNull("environment", environment);

            _resourcePicker =(createfunc?? CreateResourcePickerDialog)();
   

            _environment = environment;


            PickResourceCommand = new DelegateCommand(PickResource);

            InitializeHelp();

            InitializeTos(securitySettings );
        }

        private void InitializeTos(IPerformanceCounterTo nativeCounters)
        {
            ServerCounters = new ObservableCollection<IPerformanceCountersByMachine>(nativeCounters.FromTo());
            ResourceCounters = new ObservableCollection<IPerformanceCountersByResource>(nativeCounters.FromTO());
        }

        public ObservableCollection<IPerformanceCountersByMachine> ServerCounters { get;  set; }

        public ObservableCollection<IPerformanceCountersByResource> ResourceCounters { get;  set; }

        public ActivityDesignerToggle ServerHelpToggle { get; private set; }

        public ActivityDesignerToggle ResourceHelpToggle { get; private set; }



        public ICommand PickResourceCommand { get; private set; }

        public bool IsServerHelpVisible
        {
            get { return (bool)GetValue(IsServerHelpVisibleProperty); }
            set { SetValue(IsServerHelpVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsServerHelpVisibleProperty = DependencyProperty.Register("IsServerHelpVisible", typeof(bool), typeof(PerfcounterViewModel), new PropertyMetadata(false, IsServerHelpVisiblePropertyChanged));

        static void IsServerHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((PerfcounterViewModel)d).UpdateHelpText(HelpType.Server);
        }

        public bool IsResourceHelpVisible
        {
            get { return (bool)GetValue(IsResourceHelpVisibleProperty); }
            set { SetValue(IsResourceHelpVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsResourceHelpVisibleProperty = DependencyProperty.Register("IsResourceHelpVisible", typeof(bool), typeof(PerfcounterViewModel), new PropertyMetadata(false, IsResourceHelpVisiblePropertyChanged));

        static void IsResourceHelpVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((PerfcounterViewModel)d).UpdateHelpText(HelpType.Resource);
        }

        public virtual void Save(SecuritySettingsTO securitySettings)
        {
            VerifyArgument.IsNotNull("securitySettings", securitySettings);


        }



        void InitializeHelp()
        {
            ServerHelpToggle = CreateHelpToggle(IsServerHelpVisibleProperty);
            ResourceHelpToggle = CreateHelpToggle(IsResourceHelpVisibleProperty);
        }

 

        void PickResource(object obj)
        {
            var permission = obj as WindowsGroupPermission;
            if(permission == null)
            {
                return;
            }

            var resourceModel = PickResource(permission);
            if(resourceModel == null)
            {
                return;
            }

            permission.ResourceID = resourceModel.ResourceId;
            permission.ResourceName = string.Format("{0}\\{1}\\{2}", resourceModel.ResourceType.GetTreeDescription(), resourceModel.ResourcePath, resourceModel.ResourceName);
        }

        IExplorerTreeItem PickResource(WindowsGroupPermission permission)
        {
            if(permission != null && permission.ResourceID != Guid.Empty)
            {
                if(_environment.ResourceRepository != null)
                {
                    var foundResourceModel = _environment.ResourceRepository.FindSingle(model => model.ID == permission.ResourceID);
                    if(foundResourceModel != null)
                    {
                        _resourcePicker.SelectResource( foundResourceModel.ID);
                    }
                }
            }
            var hasResult = _resourcePicker.ShowDialog(_environment);

            if(_environment.ResourceRepository != null)
            {
                return hasResult ? _resourcePicker.SelectedResource : null;
            }
            throw  new Exception("Server does not exist");
        }












    

        PerformanceCountersByResource CreateNewCounter()
        {
            var counter = new PerformanceCountersByResource(); 
            return counter;
        }

        ActivityDesignerToggle CreateHelpToggle(DependencyProperty targetProperty)
        {
            var toggle = ActivityDesignerToggle.Create("ServiceHelp", "Close Help", "ServiceHelp", "Open Help", "HelpToggle", this, targetProperty
                );

            return toggle;
        }

        protected override void CloseHelp()
        {
            IsServerHelpVisible = false;
            IsResourceHelpVisible = false;
        }

        void UpdateHelpText(HelpType helpType)
        {
            if(_isUpdatingHelpText)
            {
                return;
            }
            _isUpdatingHelpText = true;
            try
            {
                switch(helpType)
                {
                    case HelpType.Server:
                        IsResourceHelpVisible = false;
                        HelpText = Help.HelpTextResources.SettingsSecurityServerHelpWindowsGroup;
                        break;

                    case HelpType.Resource:
                        IsServerHelpVisible = false;
                        HelpText = Help.HelpTextResources.SettingsSecurityResourceHelpResource;
                        break;
                }
            }
            finally
            {
                _isUpdatingHelpText = false;
            }
        }

        enum HelpType
        {
            Server,
            Resource
        }



    }
}