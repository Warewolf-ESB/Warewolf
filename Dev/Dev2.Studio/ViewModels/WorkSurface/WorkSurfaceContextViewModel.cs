using System;
using System.Activities.Presentation.View;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Composition;
using Dev2.Messages;
using Dev2.Providers.Errors;
using Dev2.Providers.Logs;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;

using Dev2.Studio.Webs;
using Dev2.Utils;
using Dev2.Workspaces;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    /// <summary>
    ///     Class used as unified context across the studio - coordination across different regions
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceContextViewModel : BaseViewModel,
                                 IHandle<SaveResourceMessage>, IHandle<DebugResourceMessage>,
                                 IHandle<ExecuteResourceMessage>,
                                 IHandle<UpdateWorksurfaceDisplayName>
    {
        #region private fields

        IDataListViewModel _dataListViewModel;
        IWorkSurfaceViewModel _workSurfaceViewModel;
        DebugOutputViewModel _debugOutputViewModel;
        IContextualResourceModel _contextualResourceModel;

        readonly IWindowManager _windowManager;
        readonly IWorkspaceItemRepository _workspaceItemRepository;

        AuthorizeCommand _viewInBrowserCommand;
        AuthorizeCommand _debugCommand;
        AuthorizeCommand _runCommand;
        AuthorizeCommand _saveCommand;
        AuthorizeCommand _editResourceCommand;
        AuthorizeCommand _quickDebugCommand;
        AuthorizeCommand _quickViewInBrowserCommand;

        bool _hasMappingChange;
        readonly IEnvironmentModel _environmentModel;

        #endregion private fields

        #region public properties

        public WorkSurfaceKey WorkSurfaceKey { get; private set; }

        public IEnvironmentModel Environment
        {
            get
            {
                if(ContextualResourceModel == null)
                {
                    return null;
                }
                return ContextualResourceModel.Environment;
            }
        }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get
            {
                return _debugOutputViewModel;
            }
            set { _debugOutputViewModel = value; }
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
                if(_dataListViewModel == value)
                {
                    return;
                }

                _dataListViewModel = value;
                if(_dataListViewModel != null)
                {
                    _dataListViewModel.ConductWith(this);
                    _dataListViewModel.Parent = this;
                }

                NotifyOfPropertyChange(() => DataListViewModel);
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
                if(_workSurfaceViewModel == value)
                {
                    return;
                }

                _workSurfaceViewModel = value;
                if(_workSurfaceViewModel == null)
                {
                    if(ContextualResourceModel != null)
                    {
                        ContextualResourceModel.OnDesignValidationReceived -= ValidationMemoReceived;
                    }
                }
                NotifyOfPropertyChange(() => WorkSurfaceViewModel);

                var isWorkFlowDesigner = _workSurfaceViewModel is IWorkflowDesignerViewModel;
                if(isWorkFlowDesigner)
                {
                    var workFlowDesignerViewModel = (IWorkflowDesignerViewModel)_workSurfaceViewModel;
                    ContextualResourceModel = workFlowDesignerViewModel.ResourceModel;
                    if(ContextualResourceModel != null)
                    {
                        ContextualResourceModel.OnDesignValidationReceived += ValidationMemoReceived;
                    }
                }

                if(WorkSurfaceViewModel != null)
                {
                    WorkSurfaceViewModel.ConductWith(this);
                }
            }
        }

        void ValidationMemoReceived(object sender, DesignValidationMemo designValidationMemo)
        {
            if(designValidationMemo.IsValid)
            {
                return;
            }
            if(designValidationMemo.Errors.Find(info => info.FixType == FixType.ReloadMapping) != null)
            {
                _hasMappingChange = true;
            }
        }

        #endregion public properties

        #region ctors

        public WorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : this(EventPublishers.Aggregator, workSurfaceKey, workSurfaceViewModel)
        {
        }

        public WorkSurfaceContextViewModel(IEventAggregator eventPublisher, WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : base(eventPublisher)
        {
            if(workSurfaceKey == null)
            {
                throw new ArgumentNullException("workSurfaceKey");
            }
            if(workSurfaceViewModel == null)
            {
                throw new ArgumentNullException("workSurfaceViewModel");
            }
            WorkSurfaceKey = workSurfaceKey;
            WorkSurfaceViewModel = workSurfaceViewModel;

            ImportService.TryGetExportValue(out _windowManager);
            _workspaceItemRepository = WorkspaceItemRepository.Instance;

            var model = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if(model != null)
            {
                _environmentModel = model.EnvironmentModel;
                if(_environmentModel != null)
                {
                    // MUST use connection server event publisher - debug events are published from the server!
                    DebugOutputViewModel = new DebugOutputViewModel(_environmentModel.Connection.ServerEvents, EnvironmentRepository.Instance);
                    _environmentModel.IsConnectedChanged += EnvironmentModelOnIsConnectedChanged();
                }
            }
        }

        EventHandler<ConnectedEventArgs> EnvironmentModelOnIsConnectedChanged()
        {
            return (sender, args) =>
            {
                if(args.IsConnected == false)
                {
                    SetDebugStatus(DebugStatus.Finished);
                }
            };
        }

        #endregion

        #region IHandle

        public void Handle(DebugResourceMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            Debug(message.Resource, true);
        }

        public void Handle(ExecuteResourceMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            Debug(message.Resource, false);
        }

        public void Handle(SaveResourceMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            if(ContextualResourceModel != null)
            {
                if(ContextualResourceModel.ResourceName == message.Resource.ResourceName)
                {
                    Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
                }
            }
            else
            {
                Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
            }
        }

        public void Handle(UpdateWorksurfaceDisplayName message)
        {
            this.TraceInfo(message.GetType().Name);
            if(ContextualResourceModel != null && ContextualResourceModel.ID == message.WorksurfaceResourceID)
            {
                //tab title
                ContextualResourceModel.ResourceName = message.NewName;
                _workSurfaceViewModel.NotifyOfPropertyChange("DisplayName");
            }
        }

        public void Handle(UpdateWorksurfaceFlowNodeDisplayName message)
        {
            this.TraceInfo(message.GetType().Name);
            //ContextualResourceModel.ServiceDefinition = ContextualResourceModel.ServiceDefinition
            //        .Replace("x:Class=\"" + ContextualResourceModel.ResourceName, "x:Class=\"" + message.NewName)
            //        .Replace("Name=\"" + ContextualResourceModel.ResourceName, "Name=\"" + message.NewName)
            //        .Replace("ToolboxFriendlyName=\"" + ContextualResourceModel.ResourceName, "ToolboxFriendlyName=\"" + message.NewName)
            //        .Replace("<DisplayName>" + ContextualResourceModel.ResourceName + "</DisplayName>", "<DisplayName>" + message.NewName + "</DisplayName>")
            //        .Replace("DisplayName=\"" + ContextualResourceModel.ResourceName, "DisplayName=\"" + message.NewName);
            NotifyOfPropertyChange("ContextualResourceModel");
        }

        #endregion IHandle

        public IContextualResourceModel ContextualResourceModel
        {
            get
            {
                return _contextualResourceModel;
            }
            private set
            {
                _contextualResourceModel = value;
                OnContextualResourceModelChanged();
            }
        }

        void OnContextualResourceModelChanged()
        {
            ViewInBrowserCommand.UpdateContext(Environment, ContextualResourceModel);
            DebugCommand.UpdateContext(Environment, ContextualResourceModel);
            RunCommand.UpdateContext(Environment, ContextualResourceModel);
            SaveCommand.UpdateContext(Environment, ContextualResourceModel);
            EditCommand.UpdateContext(Environment, ContextualResourceModel);
            QuickViewInBrowserCommand.UpdateContext(Environment, ContextualResourceModel);
            QuickDebugCommand.UpdateContext(Environment, ContextualResourceModel);
        }

        #region commands

        public AuthorizeCommand EditCommand
        {
            get
            {
                return _editResourceCommand ??
                       (_editResourceCommand =
                           new AuthorizeCommand(AuthorizationContext.Contribute, param =>
                           {
                               this.TraceInfo("Publish message of type - " + typeof(ShowEditResourceWizardMessage));
                               EventPublisher.Publish(new ShowEditResourceWizardMessage(ContextualResourceModel));
                           }
                            , param => CanExecute()));
            }
        }

        public AuthorizeCommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new AuthorizeCommand(AuthorizationContext.Contribute, param => Save(), param => CanSave()));
            }
        }

        public AuthorizeCommand RunCommand
        {
            get
            {
                return _runCommand ??
                       (_runCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => Debug(ContextualResourceModel, false), param => CanExecute()));
            }
        }

        public AuthorizeCommand ViewInBrowserCommand
        {
            get
            {
                return _viewInBrowserCommand ??
                       (_viewInBrowserCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => ViewInBrowser(),
                                                    param => CanDebug()));
            }
        }

        public AuthorizeCommand DebugCommand
        {
            get
            {
                return _debugCommand ??
                       (_debugCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => Debug(), param => CanDebug()));
            }
        }

        public AuthorizeCommand QuickViewInBrowserCommand
        {
            get
            {
                return _quickViewInBrowserCommand ??
                       (_quickViewInBrowserCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => QuickViewInBrowser(),
                                                    param => CanDebug()));
            }
        }

        public AuthorizeCommand QuickDebugCommand
        {
            get
            {
                return _quickDebugCommand ??
                       (_quickDebugCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => QuickDebug(), param => CanDebug()));
            }
        }

        bool CanSave()
        {
            var enabled = IsEnvironmentConnected() && !DebugOutputViewModel.IsStopping && !DebugOutputViewModel.IsConfiguring;
            return enabled;
        }

        bool CanDebug()
        {
            var enabled = IsEnvironmentConnected() && !DebugOutputViewModel.IsStopping && !DebugOutputViewModel.IsConfiguring;
            return enabled;
        }

        bool CanExecute()
        {
            var enabled = ContextualResourceModel != null && IsEnvironmentConnected() && !DebugOutputViewModel.IsProcessing;
            return enabled;
        }

        #endregion commands

        #region public methods

        public void SetDebugStatus(DebugStatus debugStatus)
        {
            if(debugStatus == DebugStatus.Finished)
            {
                CommandManager.InvalidateRequerySuggested();
            }

            if(debugStatus == DebugStatus.Configure)
            {
                DebugOutputViewModel.Clear();
            }

            DebugOutputViewModel.DebugStatus = debugStatus;
        }

        public void Debug(IContextualResourceModel resourceModel, bool isDebug)
        {
            if(resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            SetDebugStatus(DebugStatus.Configure);

            Save(resourceModel, true);
            var inputDataViewModel = SetupForDebug(resourceModel, isDebug);
            _windowManager.ShowDialog(inputDataViewModel);
        }

        WorkflowInputDataViewModel SetupForDebug(IContextualResourceModel resourceModel, bool isDebug)
        {
            var inputDataViewModel = GetWorkflowInputDataViewModel(resourceModel, isDebug);
            inputDataViewModel.DebugExecutionStart += () => DebugOutputViewModel.DebugStatus = DebugStatus.Executing;
            inputDataViewModel.DebugExecutionFinished += () => DebugOutputViewModel.DebugStatus = DebugStatus.Finished;
            return inputDataViewModel;
        }

        WorkflowInputDataViewModel GetWorkflowInputDataViewModel(IContextualResourceModel resourceModel, bool isDebug)
        {
            var mode = isDebug ? DebugMode.DebugInteractive : DebugMode.Run;
            var inputDataViewModel = WorkflowInputDataViewModel.Create(resourceModel, DebugOutputViewModel.SessionID, mode);
            inputDataViewModel.Parent = this;
            return inputDataViewModel;
        }

        public void StopExecution()
        {
            SetDebugStatus(DebugStatus.Stopping);

            CommandManager.InvalidateRequerySuggested();

            var result = ContextualResourceModel.Environment.ResourceRepository.StopExecution(ContextualResourceModel);

            DispatchServerDebugMessage(result, ContextualResourceModel);

            SetDebugStatus(DebugStatus.Finished);
        }

        public void ViewInBrowser()
        {
            FindMissing();
            this.TraceInfo("Publish message of type - " + typeof(SaveAllOpenTabsMessage));
            EventPublisher.Publish(new SaveAllOpenTabsMessage());

            if(ContextualResourceModel == null || ContextualResourceModel.Environment == null ||
               ContextualResourceModel.Environment.Connection == null)
            {
                return;
            }

            Debug();
        }

        public void QuickViewInBrowser()
        {
            Save(ContextualResourceModel, false);
            var workflowInputDataViewModel = GetWorkflowInputDataViewModel(ContextualResourceModel, false);
            workflowInputDataViewModel.LoadWorkflowInputs();
            workflowInputDataViewModel.ViewInBrowser();
        }

        public void QuickDebug()
        {
            SetDebugStatus(DebugStatus.Configure);
            Save(ContextualResourceModel, true);
            var inputDataViewModel = SetupForDebug(ContextualResourceModel, true);
            inputDataViewModel.LoadWorkflowInputs();
            inputDataViewModel.Save();
        }

        public void BindToModel()
        {
            var vm = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if(vm != null)
            {
                vm.BindToModel();
            }
        }

        public void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager)
        {
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel, null, addToTabManager);
        }

        public void Save(bool isLocalSave = false)
        {
            Save(ContextualResourceModel, isLocalSave);
            if(WorkSurfaceViewModel != null)
            {
                WorkSurfaceViewModel.NotifyOfPropertyChange("DisplayName");
            }
        }

        public bool IsEnvironmentConnected()
        {
            return Environment != null && Environment.IsConnected;
        }

        public void FindMissing()
        {
            if(WorkSurfaceViewModel is WorkflowDesignerViewModel)
            {
                var vm = (WorkflowDesignerViewModel)WorkSurfaceViewModel;
                vm.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
        }

        #endregion

        #region private methods

        void Save(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager = true)
        {
            if(resource == null)
            {
                return;
            }

            if(resource.IsNewWorkflow && !isLocalSave)
            {
                ShowSaveDialog(resource, addToTabManager);
                return;
            }

            FindMissing();
            BindToModel();

            var result = _workspaceItemRepository.UpdateWorkspaceItem(resource, isLocalSave);
            resource.Environment.ResourceRepository.Save(resource);
            DisplaySaveResult(result, resource);
            if(!isLocalSave)
            {
                if(_hasMappingChange)
                {
                    CheckForServerMessages(resource);
                    _hasMappingChange = false;
                }
                ExecuteMessage saveResult = resource.Environment.ResourceRepository.SaveToServer(resource);
                DispatchServerDebugMessage(saveResult, resource);
                resource.IsWorkflowSaved = true;
            }
            this.TraceInfo("Publish message of type - " + typeof(UpdateDeployMessage));
            EventPublisher.Publish(new UpdateDeployMessage());
        }

        void CheckForServerMessages(IContextualResourceModel resource)
        {
            if(resource == null)
            {
                return;
            }

            var compileMessageList = new StudioCompileMessageRepo().GetCompileMessagesFromServer(resource);

            if(compileMessageList.Count == 0)
            {
                return;
            }

            var showResourceChangedUtil = new ResourceChangeHandler(EventPublisher);
            showResourceChangedUtil.ShowResourceChanged(resource, compileMessageList.Dependants);
        }

        void DisplaySaveResult(ExecuteMessage result, IContextualResourceModel resource)
        {
            DispatchServerDebugMessage(result, resource);
        }

        void DispatchServerDebugMessage(ExecuteMessage message, IContextualResourceModel resource)
        {
            var debugstate = DebugStateFactory.Create(message.Message.ToString(), resource);
            if(_debugOutputViewModel != null)
            {
                debugstate.SessionID = _debugOutputViewModel.SessionID;
                _debugOutputViewModel.Append(debugstate);
            }
        }

        public virtual void Debug()
        {
            if(DebugOutputViewModel.IsProcessing)
            {
                StopExecution();
            }
            else
            {
                Debug(ContextualResourceModel, true);
            }
        }

        #endregion

        #region overrides

        protected override void OnActivate()
        {
            base.OnActivate();
            DataListSingleton.SetDataList(DataListViewModel);

            var workflowDesignerViewModel = WorkSurfaceViewModel as WorkflowDesignerViewModel;
            if(workflowDesignerViewModel != null)
            {
                //workflowDesignerViewModel.AddMissingWithNoPopUpAndFindUnusedDataListItems();
                //2013.07.03: Ashley Lewis for bug 9637 - set focus to allow ctrl+a
                if(!workflowDesignerViewModel.Designer.Context.Items.GetValue<Selection>().SelectedObjects.Any() || workflowDesignerViewModel.Designer.Context.Items.GetValue<Selection>().SelectedObjects.Any(c => c.ItemType.Name == "StartNode" || c.ItemType.Name == "Flowchart" || c.ItemType.Name == "ActivityBuilder"))
                {
                    workflowDesignerViewModel.FocusActivityBuilder();
                }
            }
        }

        #region Overrides of BaseViewModel

        /// <summary>
        /// Child classes can override this method to perform 
        ///  clean-up logic, such as removing event handlers.
        /// </summary>
        protected override void OnDispose()
        {
            if(_environmentModel != null)
            {
                _environmentModel.IsConnectedChanged -= EnvironmentModelOnIsConnectedChanged();
            }

            if(DebugOutputViewModel != null)
            {
                DebugOutputViewModel.Dispose();
            }

            if(ContextualResourceModel != null)
            {
                ContextualResourceModel.OnDesignValidationReceived -= ValidationMemoReceived;
            }

            if(DataListViewModel != null)
            {
                DataListViewModel.Parent = null;
                ((SimpleBaseViewModel)DataListViewModel).Dispose();
                DataListViewModel.Dispose();
            }

            base.OnDispose();
        }

        #endregion

        #endregion
    }
}
