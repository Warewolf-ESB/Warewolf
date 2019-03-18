#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Controller;
using Dev2.Dialogs;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Enums;
using Dev2.Studio.Interfaces;
using Newtonsoft.Json;
using Warewolf.Studio.ViewModels;







namespace Dev2.Settings.Perfcounters
{
    public class PerfcounterViewModel : SettingsItemViewModel, IUpdatesHelp
    {
        protected IResourcePickerDialog _resourcePicker;
        readonly IServer _environment;
        ObservableCollection<IPerformanceCountersByMachine> _serverCounters;
        ObservableCollection<IPerformanceCountersByResource> _resourceCounters;

        internal PerfcounterViewModel(IPerformanceCounterTo counters, IServer environment)
            : this(counters, environment, null)
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
            var serverRepository = CustomContainer.Get<IServerRepository>();
            var server = serverRepository.ActiveServer;
            if (server == null)
            {
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                server = shellViewModel?.ActiveServer;
            }
            if (server != null && server.Permissions == null)
            {
                server.Permissions = new List<IWindowsGroupPermission>();
                if(server.AuthorizationService?.SecurityService != null)
                {
                    server.Permissions.AddRange(server.AuthorizationService.SecurityService.Permissions);
                }
            }
            var env = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);
            return env;
        }

        public PerfcounterViewModel(IPerformanceCounterTo counters, IServer environment, Func<IResourcePickerDialog> createfunc)
        {
            VerifyArgument.IsNotNull("counters", counters);
            VerifyArgument.IsNotNull("environment", environment);
            _resourcePicker =(createfunc?? CreateResourcePickerDialog)();
            _environment = environment;
            
            PickResourceCommand = new DelegateCommand(PickResource);
            ResetCountersCommand = new DelegateCommand(ResetCounters);
            ServerCounters = new ObservableCollection<IPerformanceCountersByMachine>();
            ResourceCounters = new ObservableCollection<IPerformanceCountersByResource>();
            InitializeTos(counters);
        }

        [JsonIgnore]
        public ICommand ResetCountersCommand { get; set; }

        void ResetCounters(object obj)
        {
            var controller = CommunicationController;
            controller.ServiceName = "ResetPerformanceCounters";
            var message = controller.ExecuteCommand<IExecuteMessage>(_environment.Connection, Guid.Empty);
            if (!message.HasError)
            {
                CustomContainer.Get<IPopupController>().Show(Warewolf.Studio.Resources.Languages.Core.ResetPerfMonCountersHasNoError, Warewolf.Studio.Resources.Languages.Core.ResetPerfMonCountersHeader, MessageBoxButton.OK, MessageBoxImage.None, "", false, false, true, false, false, false);
            }
            else
            {
                CustomContainer.Get<IPopupController>().Show(Warewolf.Studio.Resources.Languages.Core.ResetPerfMonCountersHasError + Environment.NewLine + message.Message, Warewolf.Studio.Resources.Languages.Core.ResetPerfMonCountersHeader, MessageBoxButton.OK, MessageBoxImage.Information, "", false, true, false, false, false, false);
            }
        }

        void InitializeTos(IPerformanceCounterTo nativeCounters)
        {
            ServerCounters.Clear();
            ResourceCounters.Clear();
            var performanceCountersByMachines = nativeCounters.GetServerCountersTo();
            foreach (var performanceCountersByMachine in performanceCountersByMachines)
            {
                RegisterPropertyChanged(performanceCountersByMachine);
                ServerCounters.Add(performanceCountersByMachine);
            }
            var performanceCountersByResources = nativeCounters.GetResourceCountersTo();
            foreach (var performanceCountersByResource in performanceCountersByResources)
            {
                RegisterPropertyChanged(performanceCountersByResource);
                ResourceCounters.Add(performanceCountersByResource);

            }
            ResourceCounters.Add(CreateNewCounter());
            SetItem(nativeCounters);
        }

        public void Save(IPerformanceCounterTo perfCounterTo)
        {
            UpdateServerCounter(perfCounterTo);
            UpdateResourceCounter(perfCounterTo);
            InitializeTos(perfCounterTo);
            
        }

        void UpdateResourceCounter(IPerformanceCounterTo perfCounterTo)
        {
            perfCounterTo.ResourceCounters = ResourceCounters.ToList().GetResourceCounters();
        }

        void UpdateServerCounter(IPerformanceCounterTo perfCounterTo)
        {
            var serverCounter = ServerCounters[0];
            if (serverCounter != null)
            {
                var serverAvgExecTimePerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.AverageExecutionTime);
                if (serverAvgExecTimePerfCounter != null)
                {
                    serverAvgExecTimePerfCounter.IsActive = serverCounter.AverageExecutionTime;
                }

                var serverConcurrentRequestsPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.ConcurrentRequests);
                if (serverConcurrentRequestsPerfCounter != null)
                {
                    serverConcurrentRequestsPerfCounter.IsActive = serverCounter.ConcurrentRequests;
                }

                var serverExecutionErrorsPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.ExecutionErrors);
                if (serverExecutionErrorsPerfCounter != null)
                {
                    serverExecutionErrorsPerfCounter.IsActive = serverCounter.TotalErrors;
                }

                var serverNotAuthorisedErrorsPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.NotAuthorisedErrors);
                if (serverNotAuthorisedErrorsPerfCounter != null)
                {
                    serverNotAuthorisedErrorsPerfCounter.IsActive = serverCounter.NotAuthorisedErrors;
                }

                var serverRequestsPerSecondPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.RequestsPerSecond);
                if (serverRequestsPerSecondPerfCounter != null)
                {
                    serverRequestsPerSecondPerfCounter.IsActive = serverCounter.RequestPerSecond;
                }

                var serverServicesNotFoundPerfCounter = perfCounterTo.NativeCounters.FirstOrDefault(counter => counter.PerfCounterType == WarewolfPerfCounterType.ServicesNotFound);
                if (serverServicesNotFoundPerfCounter != null)
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
            IsDirty = !Equals(ItemServerCounters, ItemResourceCounters);
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

        [JsonIgnore]
        public ICommand PickResourceCommand { get; private set; }

        [JsonIgnore]
        public ICommunicationController CommunicationController
        {
            get
            {
                return _communicationController ?? new CommunicationController();
            }
            set
            {
                _communicationController = value;
            }
        }

        ICommunicationController _communicationController;


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
            counter.CounterName = resourceModel.ResourcePath;
        }

        IExplorerTreeItem PickResource(IPerformanceCountersByResource counter)
        {
            if (counter != null && counter.ResourceId != Guid.Empty)
            {
                _resourcePicker.SelectResource(counter.ResourceId);
            }
            var hasResult = _resourcePicker.ShowDialog(_environment);
            return hasResult ? _resourcePicker.SelectedResource : null;
        }

        PerformanceCountersByResource CreateNewCounter()
        {
            var counter = new PerformanceCountersByResource { IsNew = true };
            RegisterPropertyChanged(counter);
            return counter;
        }

        #region Implementation of IUpdatesHelp

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion

        #region Overrides of SettingsItemViewModel //Not used but needed due to base class
        protected override void CloseHelp()
        {
        }

        #endregion

        #region CloneItems

        public void SetItem(IPerformanceCounterTo model)
        {
            ItemServerCounters = new List<IPerformanceCountersByMachine>();
            ItemResourceCounters = new List<IPerformanceCountersByResource>();
            var performanceCountersByMachines = model.GetServerCountersTo();
            foreach (var performanceCountersByMachine in performanceCountersByMachines)
            {
                ItemServerCounters.Add(performanceCountersByMachine);
            }
            var performanceCountersByResources = model.GetResourceCountersTo();
            foreach (var performanceCountersByResource in performanceCountersByResources)
            {
                ItemResourceCounters.Add(performanceCountersByResource);
            }
            ItemResourceCounters.Add(CreateNewCounter());
        }
        
        public List<IPerformanceCountersByResource> ItemResourceCounters { get; set; }

        public List<IPerformanceCountersByMachine> ItemServerCounters { get; set; }

        bool Equals(List<IPerformanceCountersByMachine> serverCounters, List<IPerformanceCountersByResource> resourceCounters)
        {
            if (ReferenceEquals(null, serverCounters))
            {
                return false;
            }
            if (ReferenceEquals(null, resourceCounters))
            {
                return false;
            }

            return EqualsSeq(serverCounters, resourceCounters);
        }

        bool EqualsSeq(List<IPerformanceCountersByMachine> serverCounters, List<IPerformanceCountersByResource> resourceCounters)
        {
            var serverCountersCompare = ServerCountersCompare(serverCounters, true);
            var resourceCountersCompare = ResourceCountersCompare(resourceCounters, true);
            var equals = serverCountersCompare && resourceCountersCompare;

            return equals;
        }

        bool ServerCountersCompare(List<IPerformanceCountersByMachine> serverCounters, bool serverPermissionCompare)
        {
            if (_serverCounters == null)
            {
                return true;
            }
            if (_serverCounters.Count != serverCounters.Count)
            {
                return false;
            }
            for (int i = 0; i < _serverCounters.Count; i++)
            {
                if (ServerCounters[i].AverageExecutionTime != serverCounters[i].AverageExecutionTime)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerCounters[i].ConcurrentRequests != serverCounters[i].ConcurrentRequests)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerCounters[i].RequestPerSecond != serverCounters[i].RequestPerSecond)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerCounters[i].TotalErrors != serverCounters[i].TotalErrors)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerCounters[i].WorkFlowsNotFound != serverCounters[i].WorkFlowsNotFound)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare)
                {
                    continue;
                }

                if (ServerCounters[i].NotAuthorisedErrors != serverCounters[i].NotAuthorisedErrors)
                {
                    serverPermissionCompare = false;
                }
            }
            return serverPermissionCompare;
        }

        bool ResourceCountersCompare(List<IPerformanceCountersByResource> resourceCounters, bool resourcePermissionCompare)
        {
            if (_resourceCounters == null)
            {
                return true;
            }
            if (_resourceCounters.Count != resourceCounters.Count)
            {
                return false;
            }
            for (int i = 0; i < _resourceCounters.Count; i++)
            {
                if (resourceCounters[i].ResourceId == Guid.Empty)
                {
                    continue;
                }
                if (ResourceCounters[i].ResourceId != resourceCounters[i].ResourceId)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourceCounters[i].AverageExecutionTime != resourceCounters[i].AverageExecutionTime)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourceCounters[i].ConcurrentRequests != resourceCounters[i].ConcurrentRequests)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourceCounters[i].RequestPerSecond != resourceCounters[i].RequestPerSecond)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourceCounters[i].TotalErrors != resourceCounters[i].TotalErrors)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare)
                {
                    continue;
                }

                if (ResourceCounters[i].IsDeleted != resourceCounters[i].IsDeleted)
                {
                    resourcePermissionCompare = false;
                }
            }
            return resourcePermissionCompare;
        }

        #endregion
    }
}