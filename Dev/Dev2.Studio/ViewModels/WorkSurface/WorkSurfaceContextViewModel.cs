/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Factory;
using Dev2.Messages;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Utils;
using Dev2.Webs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces.Enums;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Studio.Interfaces.Enums;
using Warewolf.Studio.ViewModels;






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
                                 IHandle<UpdateWorksurfaceDisplayName>, IWorkSurfaceContextViewModel
    {
        #region private fields

        private IDataListViewModel _dataListViewModel;
        private IWorkSurfaceViewModel _workSurfaceViewModel;
        private DebugOutputViewModel _debugOutputViewModel;
        private IContextualResourceModel _contextualResourceModel;

        private readonly IWindowManager _windowManager;

        private AuthorizeCommand _viewInBrowserCommand;
        private AuthorizeCommand _debugCommand;
        private AuthorizeCommand _runCommand;
        private AuthorizeCommand _saveCommand;
        private AuthorizeCommand _quickDebugCommand;
        private AuthorizeCommand _quickViewInBrowserCommand;

        private readonly IServer _server;
        private readonly IPopupController _popupController;
        private readonly Action<IContextualResourceModel, bool, System.Action> _saveDialogAction;
        private IStudioCompileMessageRepoFactory _studioCompileMessageRepoFactory;
        private IResourceChangeHandlerFactory _resourceChangeHandlerFactory;

        #endregion private fields

        #region public properties

        public WorkSurfaceKey WorkSurfaceKey { get; }

        public IServer Environment
        {
            get
            {
                var environmentModel = ContextualResourceModel?.Environment;
                return environmentModel;
            }
        }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get
            {
                if (WorkSurfaceViewModel is WorkflowDesignerViewModel workflowDesignerViewModel)
                {
                    return workflowDesignerViewModel.DebugOutputViewModel;
                }
                return _debugOutputViewModel;
            }
            set { _debugOutputViewModel = value; }
        }

        public bool DeleteRequested { get; set; }

        public IDataListViewModel DataListViewModel
        {
            get
            {
                if (WorkSurfaceViewModel is WorkflowDesignerViewModel workflowDesignerViewModel)
                {
                    return workflowDesignerViewModel.DataListViewModel;
                }
                return _dataListViewModel;
            }
            set
            {
                if (_dataListViewModel == value)
                {
                    return;
                }

                _dataListViewModel = value;
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
                if (_workSurfaceViewModel == value)
                {
                    return;
                }

                _workSurfaceViewModel = value;
                NotifyOfPropertyChange(() => WorkSurfaceViewModel);

                var isWorkFlowDesigner = _workSurfaceViewModel is IWorkflowDesignerViewModel;
                if (isWorkFlowDesigner)
                {
                    var workFlowDesignerViewModel = (IWorkflowDesignerViewModel)_workSurfaceViewModel;
                    ContextualResourceModel = workFlowDesignerViewModel.ResourceModel;
                }

                WorkSurfaceViewModel?.ConductWith(this);
            }
        }

        #endregion public properties

        #region ctors

        public WorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : this(EventPublishers.Aggregator, workSurfaceKey, workSurfaceViewModel, new PopupController(), (a, b, c) => SaveDialogHelper.ShowNewWorkflowSaveDialog(a, null, b, c))
        {
        }

        public WorkSurfaceContextViewModel(IEventAggregator eventPublisher, WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel, IPopupController popupController, Action<IContextualResourceModel, bool, System.Action> saveDialogAction)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("popupController", popupController);
            WorkSurfaceKey = workSurfaceKey ?? throw new ArgumentNullException(nameof(workSurfaceKey));
            WorkSurfaceViewModel = workSurfaceViewModel ?? throw new ArgumentNullException(nameof(workSurfaceViewModel));

            _windowManager = CustomContainer.Get<IWindowManager>();

            if (WorkSurfaceViewModel is IWorkflowDesignerViewModel model)
            {
                model.WorkflowChanged += UpdateForWorkflowChange;
                _server = model.Server;
                if (_server != null)
                {
                    _server.IsConnectedChanged += EnvironmentModelOnIsConnectedChanged();
                    _server.Connection.ReceivedResourceAffectedMessage += OnReceivedResourceAffectedMessage;
                }
            }

            _popupController = popupController;
            _saveDialogAction = saveDialogAction;
        }

        private void UpdateForWorkflowChange()
        {
            _workspaceSaved = false;
        }

        private void OnReceivedResourceAffectedMessage(Guid resourceId, CompileMessageList compileMessageList)
        {
            var numberOfDependants = compileMessageList.Dependants;
            if (resourceId == ContextualResourceModel.ID && numberOfDependants.Count > 0)
            {
                var showResourceChangedUtil = ResourceChangeHandlerFactory.Create(EventPublisher);
                Execute.OnUIThread(() =>
                {
                    var shellViewModel = CustomContainer.Get<IShellViewModel>();
                    if (shellViewModel != null && !shellViewModel.ResourceCalled)
                    {
                        shellViewModel.ResourceCalled = true;
                        numberOfDependants = compileMessageList.MessageList.Select(to => to.ServiceID.ToString())
                                .Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
                        showResourceChangedUtil.ShowResourceChanged(ContextualResourceModel, numberOfDependants);
                    }
                });
            }
        }

        private EventHandler<ConnectedEventArgs> EnvironmentModelOnIsConnectedChanged()
        {
            return (sender, args) =>
            {
                if (!args.IsConnected)
                {
                    SetDebugStatus(DebugStatus.Finished);
                }
            };
        }

        #endregion ctors

        #region IHandle

        public void Handle(DebugResourceMessage message)
        {
            Dev2Logger.Debug(message.GetType().Name, "Warewolf Debug");
            IContextualResourceModel contextualResourceModel = message.Resource;
            if (contextualResourceModel != null && ContextualResourceModel != null && contextualResourceModel.ID == ContextualResourceModel.ID)
            {
                Debug(contextualResourceModel, true);
            }
        }

        public void Handle(ExecuteResourceMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            Debug(message.Resource, false);
        }

        public void Handle(SaveResourceMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            if (ContextualResourceModel != null)
            {
                if (ContextualResourceModel.ID == message.Resource.ID)
                {
                    Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
                }
            }
            else
            {
                if (!(WorkSurfaceViewModel is HelpViewModel))
                {
                    Save(message.Resource, message.IsLocalSave, message.AddToTabManager);
                }
            }
        }

        public void Handle(UpdateWorksurfaceDisplayName message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            if (ContextualResourceModel != null && ContextualResourceModel.ID == message.WorksurfaceResourceID)
            {
                //tab title
                ContextualResourceModel.ResourceName = message.NewName;
                _workSurfaceViewModel.NotifyOfPropertyChange("DisplayName");
            }
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

        private void OnContextualResourceModelChanged()
        {
            ViewInBrowserCommand.UpdateContext(Environment, ContextualResourceModel);
            DebugCommand.UpdateContext(Environment, ContextualResourceModel);
            RunCommand.UpdateContext(Environment, ContextualResourceModel);
            SaveCommand.UpdateContext(Environment, ContextualResourceModel);
            QuickViewInBrowserCommand.UpdateContext(Environment, ContextualResourceModel);
            QuickDebugCommand.UpdateContext(Environment, ContextualResourceModel);
        }

        #region commands
        
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
            var enabled = IsEnvironmentConnected() && !DebugOutputViewModel.IsStopping && !DebugOutputViewModel.IsConfiguring && !ContextualResourceModel.IsWorkflowSaved;
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
            if (debugStatus == DebugStatus.Finished)
            {
                CommandManager.InvalidateRequerySuggested();
            }

            if (debugStatus == DebugStatus.Executing)
            {
                DebugOutputViewModel.Clear();
            }

            DebugOutputViewModel.DebugStatus = debugStatus;
        }

        public void Debug(IContextualResourceModel resourceModel, bool isDebug)
        {
            if (resourceModel?.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            if (!resourceModel.IsWorkflowSaved)
            {
                var succesfulSave = Save(resourceModel, true);
                if (!succesfulSave)
                {
                    return;
                }
            }

            var inputDataViewModel = SetupForDebug(resourceModel, isDebug);
            _windowManager.ShowDialog(inputDataViewModel);
        }

        private WorkflowInputDataViewModel SetupForDebug(IContextualResourceModel resourceModel, bool isDebug)
        {
            var inputDataViewModel = GetWorkflowInputDataViewModel(resourceModel, isDebug);
            inputDataViewModel.DebugExecutionStart += () =>
            {
                SetDebugStatus(DebugStatus.Executing);
                var workfloDesignerViewModel = WorkSurfaceViewModel as WorkflowDesignerViewModel;
                DebugOutputViewModel.DebugStatus = DebugStatus.Executing;
                workfloDesignerViewModel?.GetWorkflowLink();
            };
            inputDataViewModel.DebugExecutionFinished += () =>
            {
                DebugOutputViewModel.DebugStatus = DebugStatus.Finished;
            };
            return inputDataViewModel;
        }

        private WorkflowInputDataViewModel GetWorkflowInputDataViewModel(IContextualResourceModel resourceModel, bool isDebug)
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
            Dev2Logger.Debug("Publish message of type - " + typeof(SaveAllOpenTabsMessage), "Warewolf Debug");
            EventPublisher.Publish(new SaveAllOpenTabsMessage());

            if (ContextualResourceModel?.Environment?.Connection == null)
            {
                return;
            }

            Debug();
        }

        public void QuickViewInBrowser()
        {
            if (!ContextualResourceModel.IsWorkflowSaved)
            {
                var successfuleSave = Save(ContextualResourceModel, true);
                if (!successfuleSave)
                {
                    return;
                }
            }
            ViewInBrowserInternal(ContextualResourceModel);
        }

        private void ViewInBrowserInternal(IContextualResourceModel model)
        {
            var workflowInputDataViewModel = GetWorkflowInputDataViewModel(model, false);
            workflowInputDataViewModel.LoadWorkflowInputs();
            workflowInputDataViewModel.ViewInBrowser();
        }

        public void QuickDebug()
        {
            if (DebugOutputViewModel.IsProcessing)
            {
                StopExecution();
            }
            if (WorkflowDesignerViewModel.ValidatResourceModel(ContextualResourceModel.DataList))
            {
                if(!ContextualResourceModel.IsWorkflowSaved && !_workspaceSaved)
                {
                    var successfuleSave = Save(ContextualResourceModel, true);
                    if(!successfuleSave)
                    {
                        return;
                    }
                }                
            }
            else
            {
                _popupController.Show(StringResources.Debugging_Error,
                                      StringResources.Debugging_Error_Title,
                                      MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false);

                SetDebugStatus(DebugStatus.Finished);
                return;
            }
            var inputDataViewModel = SetupForDebug(ContextualResourceModel, true);
            inputDataViewModel.LoadWorkflowInputs();
            inputDataViewModel.Save();

        }

        public void BindToModel()
        {
            var vm = WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            vm?.BindToModel();
        }

        private bool _waitingforDialog;
        private bool _workspaceSaved;

        public void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager)
        {
            SaveDialogHelper.ShowNewWorkflowSaveDialog(resourceModel, null, addToTabManager);
        }

        public bool Save(bool isLocalSave = false, bool isStudioShutdown = false)
        {
            var saveResult = Save(ContextualResourceModel, isLocalSave);
            WorkSurfaceViewModel?.NotifyOfPropertyChange("DisplayName");
            if (!isLocalSave)
            {
                if (DebugOutputViewModel != null)
                {
                    ViewModelUtils.RaiseCanExecuteChanged(DebugOutputViewModel.AddNewTestCommand);
                }
            }
            return saveResult;
        }

        public bool IsEnvironmentConnected()
        {
            return Environment != null && Environment.IsConnected;
        }

        public void FindMissing()
        {
            if (WorkSurfaceViewModel is WorkflowDesignerViewModel model)
            {
                var vm = model;
                vm.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
        }

        #endregion public methods

        #region private methods

        protected virtual bool Save(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager = true)
        {
            if (resource == null || !resource.UserPermissions.IsContributor())
            {
                return false;
            }

            FindMissing();

            if (DataListViewModel != null && DataListViewModel.HasErrors && !isLocalSave)
            {
                _popupController.Show(string.Format(StringResources.Saving_Error + System.Environment.NewLine + System.Environment.NewLine + DataListViewModel.DataListErrorMessage),
                                      StringResources.Saving_Error_Title,
                                      MessageBoxButton.OK, MessageBoxImage.Error, "", false, true, false, false, false, false);

                return false;
            }

            if (resource.IsNewWorkflow && !isLocalSave && !_waitingforDialog)
            {
                _waitingforDialog = true;
                _saveDialogAction(resource, addToTabManager, () => _waitingforDialog = false);

                return true;
            }

            BindToModel();
            if (!isLocalSave)
            {
                ExecuteMessage saveResult = resource.Environment.ResourceRepository.SaveToServer(resource);
                DispatchServerDebugMessage(saveResult, resource);
                resource.IsWorkflowSaved = true;
                _workspaceSaved = true;
                UpdateResourceVersionInfo(resource);
            }
            else
            {
                _workspaceSaved = true;
                ExecuteMessage saveResult = resource.Environment.ResourceRepository.Save(resource);
                DisplaySaveResult(saveResult, resource);
            }
            return true;
        }

        private static void UpdateResourceVersionInfo(IContextualResourceModel resource)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            var explorerViewModel = mainViewModel?.ExplorerViewModel;
            var environmentViewModel =
                explorerViewModel?.Environments?.FirstOrDefault(model => model.ResourceId == resource.Environment.EnvironmentID);
            var explorerItemViewModel = environmentViewModel?.Children?.Flatten(model => model.Children).FirstOrDefault(
                model => model.ResourceId == resource.ID);
            if (explorerItemViewModel != null && explorerItemViewModel.GetType() == typeof (VersionViewModel))
            {
                if (explorerItemViewModel.Parent != null)
                {
                    explorerItemViewModel.Parent.AreVersionsVisible = true;
                }
            }
        }

        private void DisplaySaveResult(ExecuteMessage result, IContextualResourceModel resource)
        {
            DispatchServerDebugMessage(result, resource);
        }

        private void DispatchServerDebugMessage(ExecuteMessage message, IContextualResourceModel resource)
        {
            if (message?.Message != null)
            {
                var debugstate = DebugStateFactory.Create(message.Message.ToString(), resource);
                if (_debugOutputViewModel != null)
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

        public IPopupController PopupController => _popupController;

        public virtual void Debug()
        {
            if (DebugOutputViewModel.IsProcessing)
            {
                StopExecution();
            }
            else
            {
                Debug(ContextualResourceModel, true);
            }
        }

        #endregion private methods

        #region overrides

        protected override void OnActivate()
        {
            base.OnActivate();
            DataListSingleton.SetDataList(DataListViewModel);
        }

        #region Overrides of BaseViewModel

        /// <summary>
        /// Child classes can override this method to perform
        ///  clean-up logic, such as removing event handlers.
        /// </summary>
        protected override void OnDispose()
        {
            if (_server != null)
            {
                _server.IsConnectedChanged -= EnvironmentModelOnIsConnectedChanged();

                if (_server.Connection != null)
                {
                    
                    _server.Connection.ReceivedResourceAffectedMessage -= OnReceivedResourceAffectedMessage;
                }
            }

            DebugOutputViewModel?.Dispose();
            if (DataListViewModel is SimpleBaseViewModel model)
            {
                DataListViewModel.Parent = null;
                model.Dispose();
                DataListViewModel.Dispose();
            }
            var ws = (WorkSurfaceViewModel as IDisposable);
            ws?.Dispose();
            base.OnDispose();
        }

        #endregion Overrides of BaseViewModel

        #endregion overrides
    }
}