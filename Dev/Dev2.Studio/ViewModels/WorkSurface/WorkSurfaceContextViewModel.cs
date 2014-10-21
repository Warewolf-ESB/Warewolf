
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.View;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics;
using Dev2.Factory;
using Dev2.Messages;
using Dev2.Providers.Events;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utils;
using Dev2.Webs;
using Dev2.Workspaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.WorkSurface
{
    /// <summary>
    ///     Class used as unified context across the studio - coordination across different regions
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceContextViewModel : BaseViewModel,
                                 IHandle<SaveResourceMessage>, IHandle<DebugResourceMessage>, IHandle<DebugOutputMessage>,
                                 IHandle<ExecuteResourceMessage>,
                                 IHandle<UpdateWorksurfaceDisplayName>, IWorkSurfaceContextViewModel
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

        readonly IEnvironmentModel _environmentModel;
        readonly IPopupController _popupController;
        readonly Action<IContextualResourceModel, bool> _saveDialogAction;
        IStudioCompileMessageRepoFactory _studioCompileMessageRepoFactory;
        IResourceChangeHandlerFactory _resourceChangeHandlerFactory;

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

                var environmentModel = ContextualResourceModel.Environment;
               
                return environmentModel;
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
                NotifyOfPropertyChange(() => WorkSurfaceViewModel);

                var isWorkFlowDesigner = _workSurfaceViewModel is IWorkflowDesignerViewModel;
                if(isWorkFlowDesigner)
                {
                    var workFlowDesignerViewModel = (IWorkflowDesignerViewModel)_workSurfaceViewModel;
                    ContextualResourceModel = workFlowDesignerViewModel.ResourceModel;                    
                }

                if(WorkSurfaceViewModel != null)
                {
                    WorkSurfaceViewModel.ConductWith(this);
                }
            }
        }      

        #endregion public properties

        #region ctors

        public WorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : this(EventPublishers.Aggregator, workSurfaceKey, workSurfaceViewModel, new PopupController(), (a, b) => RootWebSite.ShowNewWorkflowSaveDialog(a, null, b))
        {
        }

        public WorkSurfaceContextViewModel(IEventAggregator eventPublisher, WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel, IPopupController popupController, Action<IContextualResourceModel, bool> saveDialogAction)
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
            VerifyArgument.IsNotNull("popupController", popupController);
            WorkSurfaceKey = workSurfaceKey;
            WorkSurfaceViewModel = workSurfaceViewModel;

            _windowManager = CustomContainer.Get<IWindowManager>();
            _workspaceItemRepository = WorkspaceItemRepository.Instance;

            var model = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if(model != null)
            {
                _environmentModel = model.EnvironmentModel;
                if(_environmentModel != null)
                {
                    // MUST use connection server event publisher - debug events are published from the server!
                    DebugOutputViewModel = new DebugOutputViewModel(_environmentModel.Connection.ServerEvents, EnvironmentRepository.Instance, new DebugOutputFilterStrategy());
                    _environmentModel.IsConnectedChanged += EnvironmentModelOnIsConnectedChanged();
                    _environmentModel.Connection.ReceivedResourceAffectedMessage += OnReceivedResourceAffectedMessage;
                }
            }

            if(WorkSurfaceKey.WorkSurfaceContext == WorkSurfaceContext.Scheduler)
            {
                if(DebugOutputViewModel == null)
                {
                    DebugOutputViewModel = new DebugOutputViewModel(new EventPublisher(), EnvironmentRepository.Instance, new DebugOutputFilterStrategy());
                }
            }
            _popupController = popupController;
            _saveDialogAction = saveDialogAction;

        }

        void OnReceivedResourceAffectedMessage(Guid resourceId, CompileMessageList compileMessageList)
        {
            if (resourceId == ContextualResourceModel.ID)
            {
                var showResourceChangedUtil = ResourceChangeHandlerFactory.Create(EventPublisher);
                Execute.OnUIThread(() => { showResourceChangedUtil.ShowResourceChanged(ContextualResourceModel, compileMessageList.Dependants); });
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
            Dev2Logger.Log.Debug(message.GetType().Name);
            IContextualResourceModel contextualResourceModel = message.Resource;
            if(contextualResourceModel != null && ContextualResourceModel != null && contextualResourceModel.ID == ContextualResourceModel.ID)
            {
                Debug(contextualResourceModel, true);
            }
        }

        public void Handle(DebugOutputMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            if(WorkSurfaceKey.WorkSurfaceContext == WorkSurfaceContext.Scheduler)
            {
                DebugOutputViewModel.Clear();
                var debugState = message.DebugStates.LastOrDefault();

                if(debugState != null)
                {
                    debugState.StateType = StateType.Clear;
                    DebugOutputViewModel.AppendX(debugState);
                }
            }
        }

        public void Handle(ExecuteResourceMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            Debug(message.Resource, false);
        }

        public void Handle(SaveResourceMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            if(ContextualResourceModel != null)
            {
                if(ContextualResourceModel.ID == message.Resource.ID)
                {
                    Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
                }
            }
            else
            {
                if(!(WorkSurfaceViewModel is HelpViewModel))
                {
                    Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
                }

            }
        }

        public void Handle(UpdateWorksurfaceDisplayName message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            if(ContextualResourceModel != null && ContextualResourceModel.ID == message.WorksurfaceResourceID)
            {
                //tab title
                ContextualResourceModel.ResourceName = message.NewName;
                _workSurfaceViewModel.NotifyOfPropertyChange("DisplayName");
            }
        }

        public void Handle(UpdateWorksurfaceFlowNodeDisplayName message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
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
                               Dev2Logger.Log.Debug("Publish message of type - " + typeof(ShowEditResourceWizardMessage));
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
                       (_viewInBrowserCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => ViewInBrowser(), param => CanDebug()));
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
                       (_quickViewInBrowserCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => QuickViewInBrowser(), param => CanViewInBrowser()));
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

        public bool CanSave()
        {
            var enabled = IsEnvironmentConnected() && !DebugOutputViewModel.IsStopping && !DebugOutputViewModel.IsConfiguring;
            return enabled;
        }

        public bool CanDebug()
        {
            var enabled = ContextualResourceModel != null && ContextualResourceModel.UserPermissions.CanDebug()
                          && IsEnvironmentConnected() && !DebugOutputViewModel.IsStopping && !DebugOutputViewModel.IsConfiguring;
            return enabled;
        }

        public bool CanViewInBrowser()
        {
            var enabled = IsEnvironmentConnected() && !DebugOutputViewModel.IsStopping && !DebugOutputViewModel.IsConfiguring;
            return enabled;
        }

        public bool CanExecute()
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

            // only try saving if I can debug and contribute, else I should just debug what I have
            if(resourceModel.UserPermissions.IsContributor())
            {

                var succesfulSave = Save(resourceModel, true);
                if(!succesfulSave)
                {
                    return;
                }
            }

            SetDebugStatus(DebugStatus.Configure);
            var inputDataViewModel = SetupForDebug(resourceModel, isDebug);
            _windowManager.ShowDialog(inputDataViewModel);
        }

        WorkflowInputDataViewModel SetupForDebug(IContextualResourceModel resourceModel, bool isDebug)
        {
            var inputDataViewModel = GetWorkflowInputDataViewModel(resourceModel, isDebug);
            inputDataViewModel.DebugExecutionStart += () =>
            {
                DebugOutputViewModel.DebugStatus = DebugStatus.Executing;
                var workfloDesignerViewModel = WorkSurfaceViewModel as WorkflowDesignerViewModel;
                if(workfloDesignerViewModel != null)
                {
                    workfloDesignerViewModel.GetWorkflowLink();
                }
            };
            inputDataViewModel.DebugExecutionFinished += () =>
            {
                DebugOutputViewModel.DebugStatus = DebugStatus.Finished;

            };
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

            //Bug 10912 - Only set the Debug Status to Finished when rendering has completed
            SetDebugStatus(DebugStatus.Finished);
        }

        public void ViewInBrowser()
        {
            FindMissing();
            Dev2Logger.Log.Debug("Publish message of type - " + typeof(SaveAllOpenTabsMessage));
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
            var successfuleSave = Save(ContextualResourceModel, true);
            if(!successfuleSave)
            {
                return;
            }
            ViewInBrowserInternal(ContextualResourceModel);
        }

        void ViewInBrowserInternal(IContextualResourceModel model)
        {
            var workflowInputDataViewModel = GetWorkflowInputDataViewModel(model, false);
            workflowInputDataViewModel.LoadWorkflowInputs();
            workflowInputDataViewModel.ViewInBrowser();
        }

        public void QuickDebug()
        {
            if(DebugOutputViewModel.IsProcessing)
            {
                StopExecution();
                Thread.Sleep(500);
            }
            if(WorkflowDesignerViewModel.ValidatResourceModel(ContextualResourceModel.DataList))
            {
                var successfuleSave = Save(ContextualResourceModel, true);
                if(!successfuleSave)
                {
                    return;
                }
            }
            else
            {

                _popupController.Show("Please resolve all variable errors, before debugging." + System.Environment.NewLine, "Error Debugging", MessageBoxButton.OK, MessageBoxImage.Error, "true");
                SetDebugStatus(DebugStatus.Finished);
                return;
            }

            SetDebugStatus(DebugStatus.Configure);
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

        public Func<IStudioResourceRepository> GetStudioResourceRepository = () => Dev2.AppResources.Repositories.StudioResourceRepository.Instance;

        private IStudioResourceRepository StudioResourceRepository
        {
            get
            {
                return GetStudioResourceRepository();
            }
        }


        public void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager)
        {
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel, null, addToTabManager);
        }

        public void Save(bool isLocalSave = false, bool isStudioShutdown = false)
        {
            Save(ContextualResourceModel, isLocalSave, isStudioShutdown: isStudioShutdown);
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
            WorkflowDesignerViewModel model = WorkSurfaceViewModel as WorkflowDesignerViewModel;
            if(model != null)
            {
                var vm = model;
                vm.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
        }

        #endregion

        #region private methods

        protected virtual bool Save(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager = true, bool isStudioShutdown = false)
        {
            if(resource == null || !resource.UserPermissions.IsContributor())
            {
                return false;
            }


            FindMissing();

            if(DataListViewModel != null && DataListViewModel.HasErrors)
            {
                PopupController.Show("Please resolve the variable(s) errors below, before saving." + System.Environment.NewLine + System.Environment.NewLine + DataListViewModel.DataListErrorMessage, "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error, "true");

                return false;
            }

            if(resource.IsNewWorkflow && !isLocalSave)
            {
                _saveDialogAction(resource, addToTabManager);
                // ShowSaveDialog(resource, addToTabManager);
                return true;
            }


            BindToModel();

            var result = _workspaceItemRepository.UpdateWorkspaceItem(resource, isLocalSave);

            // shutdown - just save to workspace
            if(isStudioShutdown)
            {
                return true;
            }

            resource.Environment.ResourceRepository.Save(resource);
            DisplaySaveResult(result, resource);
            if(!isLocalSave)
            {
                ExecuteMessage saveResult = resource.Environment.ResourceRepository.SaveToServer(resource);
                DispatchServerDebugMessage(saveResult, resource);
                resource.IsWorkflowSaved = true;
                StudioResourceRepository.RefreshVersionHistory(resource.Environment.ID, resource.ID);
            }
            return true;
        }

        void DisplaySaveResult(ExecuteMessage result, IContextualResourceModel resource)
        {
            DispatchServerDebugMessage(result, resource);
        }

        void DispatchServerDebugMessage(ExecuteMessage message, IContextualResourceModel resource)
        {
            if(message != null && message.Message != null)
            {
                var debugstate = DebugStateFactory.Create(message.Message.ToString(), resource);
                if(_debugOutputViewModel != null)
                {
                    debugstate.SessionID = _debugOutputViewModel.SessionID;
                    _debugOutputViewModel.Append(debugstate);
                }
            }
        }

        public IStudioCompileMessageRepoFactory StudioCompileMessageRepoFactory
        {
            get
            {
                return _studioCompileMessageRepoFactory ?? new StudioCompileMessageRepoFactory();
            }
            set
            {
                _studioCompileMessageRepoFactory = value;
            }
        }

        public IResourceChangeHandlerFactory ResourceChangeHandlerFactory
        {
            get
            {
                return _resourceChangeHandlerFactory ?? new ResourceChangeHandlerFactory();
            }
            set
            {
                _resourceChangeHandlerFactory = value;
            }
        }
        public IPopupController PopupController
        {
            get
            {
                return _popupController;
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

        [ExcludeFromCodeCoverage]
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

                if (_environmentModel.Connection != null)
                {
                    // ReSharper disable DelegateSubtraction
                    _environmentModel.Connection.ReceivedResourceAffectedMessage-=OnReceivedResourceAffectedMessage;
                }
            }

            if(DebugOutputViewModel != null)
            {
                DebugOutputViewModel.Dispose();
            }
            var model = DataListViewModel as SimpleBaseViewModel;
            if(model != null)
            {
                DataListViewModel.Parent = null;
                model.Dispose();
                DataListViewModel.Dispose();
            }

            base.OnDispose();
        }

        #endregion

        #endregion
    }
}
