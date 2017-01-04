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
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Enums;
using Newtonsoft.Json;
using Warewolf.Studio.AntiCorruptionLayer;
using Warewolf.Studio.ViewModels;
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable RedundantOverload.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable InconsistentNaming
namespace Dev2.Settings.Perfcounters
{
    public class PerfcounterViewModel : SettingsItemViewModel, IUpdatesHelp
    {
        protected IResourcePickerDialog _resourcePicker;
        readonly IEnvironmentModel _environment;
        private ObservableCollection<IPerformanceCountersByMachine> _serverCounters;
        private ObservableCollection<IPerformanceCountersByResource> _resourceCounters;

        internal PerfcounterViewModel(IPerformanceCounterTo counters, IEnvironmentModel environment)
            : this(counters, environment,null)
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
                if(environment.AuthorizationService?.SecurityService != null)
                {
                    server.Permissions.AddRange(environment.AuthorizationService.SecurityService.Permissions);
                }
            }
            var env = new EnvironmentViewModel(server, CustomContainer.Get<IShellViewModel>(), true);
            return env;
        }

        public PerfcounterViewModel(IPerformanceCounterTo counters, IEnvironmentModel environment, Func<IResourcePickerDialog> createfunc = null)
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

        private void ResetCounters(object obj)
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

        private void InitializeTos(IPerformanceCounterTo nativeCounters)
        {
            ServerCounters.Clear();
            ResourceCounters.Clear();
            var performanceCountersByMachines = nativeCounters.GetServerCountersTo();
            foreach(var performanceCountersByMachine in performanceCountersByMachines)
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

        private ICommunicationController _communicationController;


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
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
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

        private bool Equals(List<IPerformanceCountersByMachine> serverCounters, List<IPerformanceCountersByResource> resourceCounters)
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

        private bool EqualsSeq(List<IPerformanceCountersByMachine> serverCounters, List<IPerformanceCountersByResource> resourceCounters)
        {
            var serverCountersCompare = ServerCountersCompare(serverCounters, true);
            var resourceCountersCompare = ResourceCountersCompare(resourceCounters, true);
            var equals = serverCountersCompare && resourceCountersCompare;

            return equals;
        }

        private bool ServerCountersCompare(List<IPerformanceCountersByMachine> serverCounters, bool serverPermissionCompare)
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
                if (!serverPermissionCompare) continue;
                if (ServerCounters[i].ConcurrentRequests != serverCounters[i].ConcurrentRequests)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerCounters[i].RequestPerSecond != serverCounters[i].RequestPerSecond)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerCounters[i].TotalErrors != serverCounters[i].TotalErrors)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerCounters[i].WorkFlowsNotFound != serverCounters[i].WorkFlowsNotFound)
                {
                    serverPermissionCompare = false;
                }
                if (!serverPermissionCompare) continue;
                if (ServerCounters[i].NotAuthorisedErrors != serverCounters[i].NotAuthorisedErrors)
                {
                    serverPermissionCompare = false;
                }                
            }
            return serverPermissionCompare;
        }

        private bool ResourceCountersCompare(List<IPerformanceCountersByResource> resourceCounters, bool resourcePermissionCompare)
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
                if (!resourcePermissionCompare) continue;
                if (ResourceCounters[i].AverageExecutionTime != resourceCounters[i].AverageExecutionTime)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourceCounters[i].ConcurrentRequests != resourceCounters[i].ConcurrentRequests)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourceCounters[i].RequestPerSecond != resourceCounters[i].RequestPerSecond)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
                if (ResourceCounters[i].TotalErrors != resourceCounters[i].TotalErrors)
                {
                    resourcePermissionCompare = false;
                }
                if (!resourcePermissionCompare) continue;
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