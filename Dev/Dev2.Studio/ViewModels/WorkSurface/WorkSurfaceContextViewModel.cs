using System;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.AppResources.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Diagnostics;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Studio.Webs;
using Newtonsoft.Json;
using Unlimited.Framework;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    /// <summary>
    /// Class used as unified context across the studio - coordination across different regions
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceContextViewModel : BaseViewModel,
                                 IHandle<SaveResourceMessage>, IHandle<DebugResourceMessage>,
                                 IHandle<ExecuteResourceMessage>, IHandle<SetDebugStatusMessage>
    {
        #region private fields

        private IDataListViewModel _dataListViewModel;
        private IWorkSurfaceViewModel _workSurfaceViewModel;
        private DebugOutputViewModel _debugOutputViewModel;
        private IContextualResourceModel _contextualResourceModel;

        private readonly IWindowManager _windowManager;
        private readonly IFrameworkSecurityContext _securityContext;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWorkspaceItemRepository _workspaceItemRepository;

        private ICommand _viewInBrowserCommand;
        private ICommand _debugCommand;
        private ICommand _runCommand;
        private ICommand _saveCommand;
        private ICommand _editResourceCommand;
        #endregion private fields

        #region public properties
        public WorkSurfaceKey WorkSurfaceKey { get; private set; }

        public IEnvironmentModel Environment
        {
            get
            {
                if (_contextualResourceModel == null)
                {
                    return null;
                }
                return _contextualResourceModel.Environment;
            }
        }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get
            {
                return _debugOutputViewModel ?? (_debugOutputViewModel = new DebugOutputViewModel());
            }
            set
            {
                _debugOutputViewModel = value;
                NotifyOfPropertyChange(() => DebugOutputViewModel);
            }
        } 

        public bool DeleteRequested { get; set; }

        public IDataListViewModel DataListViewModel
        {
            get
            {
                return _dataListViewModel;
            }
            set
            {
                if (_dataListViewModel == value) return;

                _dataListViewModel = value;
                NotifyOfPropertyChange(() => DataListViewModel);
                if (DataListViewModel != null)
                {
                    DataListViewModel.ConductWith(this);
                    DataListViewModel.Parent = this;
                }
            }
        }

        public IWorkSurfaceViewModel WorkSurfaceViewModel
        {
            get
            {
                return _workSurfaceViewModel;
            }
            set
            {
                if (_workSurfaceViewModel == value) return;

                _workSurfaceViewModel = value;
                NotifyOfPropertyChange(() => WorkSurfaceViewModel);

                var isWorkFlowDesigner = _workSurfaceViewModel is WorkflowDesignerViewModel;
                if (isWorkFlowDesigner)
                {
                    var workFlowDesignerViewModel = (WorkflowDesignerViewModel) _workSurfaceViewModel;
                    _contextualResourceModel = workFlowDesignerViewModel.ResourceModel;
                }

                if (WorkSurfaceViewModel != null)
                    WorkSurfaceViewModel.ConductWith(this);
            }
        }

        public DebugWriter DebugWriter { get; set; }

        public bool CanExecute
        {
            get { return _contextualResourceModel != null && IsEnvironmentConnected() && !DebugOutputViewModel.IsProcessing; }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editResourceCommand ??
                       (_editResourceCommand =
                        new RelayCommand(param => EventAggregator.Publish(new ShowEditResourceWizardMessage(_contextualResourceModel))
                            , param => CanExecute));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand(param => Save(), param => CanExecute));
            }
        }
        #endregion public properties

        #region ctors

        public WorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
        {
            WorkSurfaceKey = workSurfaceKey;
            WorkSurfaceViewModel = workSurfaceViewModel;

             ImportService.TryGetExportValue(out _windowManager);
             ImportService.TryGetExportValue(out _securityContext);
             ImportService.TryGetExportValue(out _eventAggregator);
             ImportService.TryGetExportValue(out _workspaceItemRepository);

            if (_eventAggregator != null)
            {
                _eventAggregator.Subscribe(this);
            }

            if (WorkSurfaceViewModel is IWorkflowDesignerViewModel)
            {
                _debugOutputViewModel = new DebugOutputViewModel();
                DebugWriter = (DebugWriter)((IWorkflowDesignerViewModel) WorkSurfaceViewModel)
                    .EnvironmentModel.Connection.DebugWriter;
            }
        }

        #endregion

        #region IHandle

        public void Handle(DebugResourceMessage message)
        {
            Debug(message.Resource, true);
        }

        public void Handle(ExecuteResourceMessage message)
        {
            Debug(message.Resource, false);
        }

        public void Handle(SaveResourceMessage message)
        {
            if(_contextualResourceModel != null)
            {
                if(_contextualResourceModel.ResourceName == message.Resource.ResourceName)
                {
                    Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
                }
            }
            else
            {
                Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
            }
        }

        public void Handle(SetDebugStatusMessage message)
        {
            if (message.WorkSurfaceKey.Equals(WorkSurfaceKey))
                SetDebugStatus(message.DebugStatus);
        }

        #endregion IHandle

        #region commands

        public ICommand RunCommand
        {
            get
            {
                return _runCommand ??
                       (_runCommand = new RelayCommand(param => Debug(_contextualResourceModel, false),
                                                       param => CanExecute));
            }
        }

        public ICommand ViewInBrowserCommand
        {
            get
            {
                return _viewInBrowserCommand ??
                       (_viewInBrowserCommand = new RelayCommand(param => ViewInBrowser(),
                                                                 param => CanExecute));
            }
        }

        public ICommand DebugCommand
        {
            get
            {
                return _debugCommand ??
                       (_debugCommand =
                        new RelayCommand(param => Debug(), param => CanDebug()));
            }
        }
        #endregion commands

        #region public methods

        private bool CanDebug()
        {
            return IsEnvironmentConnected()
                && !DebugOutputViewModel.IsStopping
                && !DebugOutputViewModel.IsConfiguring;
        }

        public void SetDebugStatus(DebugStatus debugStatus)
        {
            if (debugStatus == DebugStatus.Finished)
            {
                CommandManager.InvalidateRequerySuggested();
            }

            if (debugStatus == DebugStatus.Configure)
                DebugOutputViewModel.Clear();

            DebugOutputViewModel.DebugStatus = debugStatus;
        }

        public void DisplayDebugOutput(IDebugState debugState)
        {
            DebugOutputViewModel.Append(debugState);
        }

        public void GetServiceInputDataFromUser(IServiceDebugInfoModel input)
        {
            var inputDataViewModel = new WorkflowInputDataViewModel(input) {Parent = this};
            _windowManager.ShowDialog(inputDataViewModel);
        }

        public void Debug(IContextualResourceModel resourceModel, bool isDebug)
        {

            if (resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            SetDebugStatus(DebugStatus.Configure);

            //DebugWriter = new DebugWriter(s => EventAggregator.Publish(new DebugWriterWriteMessage(s)));
            //Environment.Connection.AddDebugWriter(DebugWriter);

            Save(resourceModel, true);
            //EventAggregator.Publish(new SaveAllOpenTabsMessage());
            var mode = isDebug ? DebugMode.DebugInteractive : DebugMode.Run;
            IServiceDebugInfoModel debugInfoModel =
                ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, mode);
            GetServiceInputDataFromUser(debugInfoModel);
        }

        public void Build()
        {
            Build(_contextualResourceModel, false);
        }

        public void StopExecution()
        {
            SetDebugStatus(DebugStatus.Stopping);

            dynamic buildRequest = new UnlimitedObject();

            buildRequest.Service = "TerminateExecutionService";
            buildRequest.Roles = String.Join(",", _securityContext.Roles);

            buildRequest.ResourceXml = _contextualResourceModel.ToServiceDefinition();

            Guid workspaceID = ((IStudioClientContext)_contextualResourceModel.Environment.DsfChannel).WorkspaceID;

            string result =
                _contextualResourceModel.Environment.DsfChannel.
                      ExecuteCommand(buildRequest.XmlString, workspaceID, GlobalConstants.NullDataListID) ??
                string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, buildRequest.Service);

            DispatchServerDebugMessage(result, _contextualResourceModel);

            SetDebugStatus(DebugStatus.Finished);
        }

        public void ViewInBrowser()
        {
            FindMissing();

            //EventAggregator.Publish(new SaveAllOpenTabsMessage());

            Save(_contextualResourceModel, true);             

            if (_contextualResourceModel == null || _contextualResourceModel.Environment == null ||
                _contextualResourceModel.Environment.Connection == null) return;

            Process.Start(StudioToWizardBridge.GetWorkflowUrl(_contextualResourceModel).AbsoluteUri);
        }

        public void BindToModel()
        {
            var vm = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if (vm != null)
            {
                vm.BindToModel();
            }
        }

        public void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager)
        {
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel,null, addToTabManager);
        }

        public void Save(bool isLocalSave = false)
        {
            Save(_contextualResourceModel, isLocalSave);
        }

        public bool IsEnvironmentConnected()
        {
            return Environment != null && Environment.IsConnected;
        }

        public void FindMissing()
        {
            if (WorkSurfaceViewModel is WorkflowDesignerViewModel)
            {
                var vm = (WorkflowDesignerViewModel)WorkSurfaceViewModel;
                vm.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
        }

        #endregion

        #region private methods

        private void Save(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager = true)
        {
            if (resource == null)
            {
                return;
            }

            if (resource.IsNewWorkflow && !isLocalSave)
            {
                ShowSaveDialog(resource, addToTabManager);
                return;
            }

            FindMissing();
            BindToModel();

            if (!isLocalSave)
            {
                Build(resource);
                CheckForServerMessages(resource);
                resource.IsWorkflowSaved = true;
            }

            var resourceToUpdate = resource.Environment.ResourceRepository.FindSingle(
                c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));
            if (resourceToUpdate != null)
            {
                resourceToUpdate.Update(resource);
            }

            var result = _workspaceItemRepository.UpdateWorkspaceItem(resource, isLocalSave);

            if (!isLocalSave)
            {
                DisplaySaveResult(result, resource);
            }

            resource.Environment.ResourceRepository.Save(resource);
            EventAggregator.Publish(new UpdateDeployMessage());
            
        }

        void CheckForServerMessages(IContextualResourceModel resource)
        {
            if(resource==null) return;
            var compileMessagesFromServer = StudioCompileMessageRepo.GetCompileMessagesFromServer(resource);
            if(string.IsNullOrEmpty(compileMessagesFromServer)) return;
            if(compileMessagesFromServer.Contains("<Error>")) return;
            CompileMessageList compileMessageList = JsonConvert.DeserializeObject<CompileMessageList>(compileMessagesFromServer);
            if(compileMessageList.Count == 0) return;
            var numberOfDependants = compileMessageList.NumberOfDependants;
            ResourceChangedDialog dialog = new ResourceChangedDialog(resource, numberOfDependants);
            dialog.ShowDialog();
            if (dialog.OpenDependencyGraph)
            {
                EventAggregator.Publish(new ShowReverseDependencyVisualizer(resource));
            }
        }

        private void DisplaySaveResult(string result, IContextualResourceModel resource)
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("<Save StartDate=\"{0}\">",
                                        DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            sb.AppendLine(result);
            sb.AppendLine(String.Format("</Save>"));

            DispatchServerDebugMessage(sb.ToString(), resource);
        }

        private void DispatchServerDebugMessage(string message, IContextualResourceModel resource)
        {
            var debugstate = DebugStateFactory.Create(message, resource);
            EventAggregator.Publish(new DebugWriterWriteMessage(debugstate));
        }

        private void Build(IContextualResourceModel resource, bool deploy = true)
        {

            if (resource == null || resource.Environment == null || !resource.Environment.IsConnected)
            {
                return;
            }

            var sb = new StringBuilder();
            dynamic buildRequest = new UnlimitedObject();

            if (!deploy)
            {
                buildRequest.Service = "CompileResourceService";
            }
            else
            {
                buildRequest.Service = "AddResourceService";
                buildRequest.Roles = String.Join(",", _securityContext.Roles);
            }

            sb.AppendLine(String.Format("<Build StartDate=\"{0}\">", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

            if (resource.ResourceType == ResourceType.WorkflowService)
            {
                sb.AppendLine(String.Format("<Build WorkflowName=\"{0}\" />", resource.ResourceName));
            }
            else if (resource.ResourceType == ResourceType.Service)
            {
                sb.AppendLine(String.Format("<Build Service=\"{0}\" />", resource.ResourceName));
            }
            else if (resource.ResourceType == ResourceType.Source)
            {
                sb.AppendLine(String.Format("<Build Source=\"{0}\" />", resource.ResourceName));
            }

            buildRequest.ResourceXml = resource.ToServiceDefinition();

            Guid workspaceID = ((IStudioClientContext)resource.Environment.DsfChannel).WorkspaceID;

            string result =
                resource.Environment.DsfChannel.
                      ExecuteCommand(buildRequest.XmlString, workspaceID, GlobalConstants.NullDataListID) ??
                string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, buildRequest.Service);

            sb.AppendLine(result);
            sb.AppendLine(String.Format("</Build>"));

            DispatchServerDebugMessage(sb.ToString(), resource);
        }

        private void Debug()
        {
            if (DebugOutputViewModel.IsProcessing)
            {
                StopExecution();
            }
            else
            {
                Debug(_contextualResourceModel, true);
            }
        }

        #endregion

        #region overrides

        protected override void OnActivate()
        {
            base.OnActivate();
            DataListSingleton.SetDataList(DataListViewModel);
            DebugOutputViewModel.DebugWriter = DebugWriter;

            var workflowDesignerViewModel = WorkSurfaceViewModel as WorkflowDesignerViewModel;
            if(workflowDesignerViewModel != null)
            {
                workflowDesignerViewModel.AddMissingWithNoPopUpAndFindUnusedDataListItems();
                //2013.07.03: Ashley Lewis for bug 9637 - set focus to allow ctrl+a
                if (!workflowDesignerViewModel.Designer.Context.Items.GetValue<Selection>().SelectedObjects.Any() || workflowDesignerViewModel.Designer.Context.Items.GetValue<Selection>().SelectedObjects.Any(c => c.ItemType.Name == "StartNode" || c.ItemType.Name == "Flowchart" || c.ItemType.Name == "ActivityBuilder"))
                {
                    workflowDesignerViewModel.FocusActivityBuilder();
                }
            }
        }

        #endregion
    }
}
