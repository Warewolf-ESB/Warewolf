using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ViewModels;
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantOverload.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
namespace Dev2.Settings.Perfcounters
{
    public class PerfcounterViewModel : SettingsItemViewModel, IHelpSource
    {
        IResourcePickerDialog _resourcePicker;

     
        readonly IEnvironmentModel _environment;
        bool _isUpdatingHelpText;
        private ObservableCollection<IPerformanceCountersByMachine> _serverCounters;
        private ObservableCollection<IPerformanceCountersByResource> _resourceCounters;

        
        internal PerfcounterViewModel(IPerformanceCounterTo counters, IWin32Window parentWindow, IEnvironmentModel environment)
            : this(counters, parentWindow, environment,null)
        {
        }

        IResourcePickerDialog CreateResourcePickerDialog()
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

        public PerfcounterViewModel(IPerformanceCounterTo counters,  IWin32Window parentWindow, IEnvironmentModel environment, Func<IResourcePickerDialog> createfunc = null)
        {
            VerifyArgument.IsNotNull("parentWindow", parentWindow);
            VerifyArgument.IsNotNull("environment", environment);
            _resourcePicker =(createfunc?? CreateResourcePickerDialog)();
            _environment = environment;
            PickResourceCommand = new DelegateCommand(PickResource);

            InitializeHelp();

            InitializeTos(counters);
        }

        private void InitializeTos(IPerformanceCounterTo nativeCounters)
        {
            var performanceCountersByMachines = nativeCounters.GetServerCountersTo();
            ServerCounters = new ObservableCollection<IPerformanceCountersByMachine>();
            foreach(var performanceCountersByMachine in performanceCountersByMachines)
            {
                RegisterPropertyChanged(performanceCountersByMachine);
                ServerCounters.Add(performanceCountersByMachine);
            }
            var performanceCountersByResources = nativeCounters.GetResourceCountersTo();
            ResourceCounters = new ObservableCollection<IPerformanceCountersByResource>();
            foreach(var performanceCountersByResource in performanceCountersByResources)
            {
                RegisterPropertyChanged(performanceCountersByResource);
                ResourceCounters.Add(performanceCountersByResource);
            }
            ResourceCounters.Add(CreateNewCounter());
        }

        public virtual void Save(IPerformanceCounterTo perfCounterTo)
        {
            UpdateServerCounter(perfCounterTo);
            UpdateResourceCounter(perfCounterTo);

            InitializeTos(perfCounterTo);
        }

        private void UpdateResourceCounter(IPerformanceCounterTo perfCounterTo)
        {
            perfCounterTo.ResourceCounters = ResourceCounters.ToList().GetResourceCounters();
        }

        private void UpdateServerCounter(IPerformanceCounterTo perfCounterTo)
        {
            var serverCounter = ServerCounters[0];
            if(serverCounter != null)
            {
                var serverAvgExecTimePerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.AverageExecutionTime);
                if(serverAvgExecTimePerfCounter != null)
                {
                    serverAvgExecTimePerfCounter.IsActive = serverCounter.AverageExecutionTime;
                }

                var serverConcurrentRequestsPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.ConcurrentRequests);
                if(serverConcurrentRequestsPerfCounter != null)
                {
                    serverConcurrentRequestsPerfCounter.IsActive = serverCounter.ConcurrentRequests;
                }

                var serverExecutionErrorsPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.ExecutionErrors);
                if(serverExecutionErrorsPerfCounter != null)
                {
                    serverExecutionErrorsPerfCounter.IsActive = serverCounter.TotalErrors;
                }

                var serverNotAuthorisedErrorsPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.NotAuthorisedErrors);
                if(serverNotAuthorisedErrorsPerfCounter != null)
                {
                    serverNotAuthorisedErrorsPerfCounter.IsActive = serverCounter.NotAuthorisedErrors;
                }

                var serverRequestsPerSecondPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.RequestsPerSecond);
                if(serverRequestsPerSecondPerfCounter != null)
                {
                    serverRequestsPerSecondPerfCounter.IsActive = serverCounter.RequestPerSecond;
                }

                var serverServicesNotFoundPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.ServicesNotFound);
                if(serverServicesNotFoundPerfCounter != null)
                {
                    serverServicesNotFoundPerfCounter.IsActive = serverCounter.WorkFlowsNotFound;
                }
            }
        }

        void RegisterPropertyChanged(IPerformanceCounters counter)
        {
            counter.PropertyChanged += OnPerfCounterPropertyChanged;
        }

        void OnPerfCounterPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            IsDirty = true;
            if (args.PropertyName == "CounterName")
            {
                var countersByResource = (IPerformanceCountersByResource)sender;

                if (countersByResource.IsNew)
                {
                    {
                        countersByResource.IsNew = false;
                        var newPermission = CreateNewCounter();
                        ResourceCounters.Add(newPermission);
                    }
                }
                else
                {
                    var isEmpty = string.IsNullOrEmpty(countersByResource.CounterName);
                    if (isEmpty)
                    {
                        ResourceCounters.Remove(countersByResource);
                    }
                }
            }
        }

        public ObservableCollection<IPerformanceCountersByMachine> ServerCounters
        {
            get
            {
                return _serverCounters;
            }
            set
            {
                _serverCounters = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IPerformanceCountersByResource> ResourceCounters
        {
            get
            {
                return _resourceCounters;
            }
            set
            {
                _resourceCounters = value;
                OnPropertyChanged();
            }
        }

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

        void InitializeHelp()
        {
            ServerHelpToggle = CreateHelpToggle(IsServerHelpVisibleProperty);
            ResourceHelpToggle = CreateHelpToggle(IsResourceHelpVisibleProperty);
        }
        
        void PickResource(object obj)
        {
            var counter = obj as IPerformanceCountersByResource;
            if(counter == null)
            {
                return;
            }

            var resourceModel = PickResource(counter);
            if(resourceModel == null)
            {
                return;
            }

            counter.ResourceId = resourceModel.ResourceId;
            counter.CounterName = resourceModel.ResourcePath+"\\"+resourceModel.ResourceName;
        }

        IExplorerTreeItem PickResource(IPerformanceCountersByResource counter)
        {
            if(counter != null && counter.ResourceId != Guid.Empty)
            {
                if(_environment.ResourceRepository != null)
                {
                    var foundResourceModel = _environment.ResourceRepository.FindSingle(model => model.ID == counter.ResourceId);
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
            var counter = new PerformanceCountersByResource { IsNew = true };
            RegisterPropertyChanged(counter);
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