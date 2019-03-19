#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Messages;
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
using Dev2.ViewModels;
using Dev2.Common.Interfaces;
using Dev2.Instrumentation;
using Warewolf.Studio.Resources.Languages;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    public class WorkSurfaceContextViewModel : BaseViewModel,
                                 IHandle<SaveResourceMessage>,
                                 IHandle<UpdateWorksurfaceDisplayName>, IWorkSurfaceContextViewModel
    {
        #region private fields

        IDataListViewModel _dataListViewModel;
        IWorkSurfaceViewModel _workSurfaceViewModel;
        DebugOutputViewModel _debugOutputViewModel;
        IContextualResourceModel _contextualResourceModel;

        readonly IWindowManager _windowManager;

        AuthorizeCommand _viewInBrowserCommand;
        AuthorizeCommand _debugCommand;
        AuthorizeCommand _runCommand;
        AuthorizeCommand _saveCommand;        
        AuthorizeCommand _quickDebugCommand;
        AuthorizeCommand _quickViewInBrowserCommand;

        readonly IServer _server;
        readonly IPopupController _popupController;
        readonly Action<IContextualResourceModel, bool, System.Action> _saveDialogAction;
        IResourceChangeHandlerFactory _resourceChangeHandlerFactory;

        private readonly IApplicationTracker _applicationTracker;

        #endregion private fields

        #region public properties

        public IWorkSurfaceKey WorkSurfaceKey { get; }

        public IServer Environment => ContextualResourceModel?.Environment;

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
                if (WorkSurfaceViewModel is MergeViewModel mergeViewModel)
                {
                    return mergeViewModel.DataListViewModel;
                }
                return _dataListViewModel;
            }
            set
            {
                _dataListViewModel = value;
                NotifyOfPropertyChange(() => DataListViewModel);
            }
        }

        public IWorkSurfaceViewModel WorkSurfaceViewModel
        {
            get => _workSurfaceViewModel;
            set
            {
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

        public WorkSurfaceContextViewModel(IWorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
            : this(EventPublishers.Aggregator, workSurfaceKey, workSurfaceViewModel, new PopupController(), (a, b, c) => SaveDialogHelper.ShowNewWorkflowSaveDialog(a, null, b, c))
        {
        }

        public WorkSurfaceContextViewModel(IEventAggregator eventPublisher, IWorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel, IPopupController popupController, Action<IContextualResourceModel, bool, System.Action> saveDialogAction)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("popupController", popupController);
            WorkSurfaceKey = workSurfaceKey ?? throw new ArgumentNullException(nameof(workSurfaceKey));
            WorkSurfaceViewModel = workSurfaceViewModel ?? throw new ArgumentNullException(nameof(workSurfaceViewModel));

            _windowManager = CustomContainer.Get<IWindowManager>();

            _applicationTracker = CustomContainer.Get<IApplicationTracker>();

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

        void UpdateForWorkflowChange()
        {
            _workspaceSaved = false;
        }

        void OnReceivedResourceAffectedMessage(Guid resourceId, CompileMessageList compileMessageList)
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

        EventHandler<ConnectedEventArgs> EnvironmentModelOnIsConnectedChanged() => (sender, args) =>
                                                                                             {
                                                                                                 if (!args.IsConnected)
                                                                                                 {
                                                                                                     SetDebugStatus(DebugStatus.Finished);
                                                                                                 }
                                                                                             };

        #endregion ctors

        #region IHandle

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
            get => _contextualResourceModel;
            set
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
            QuickViewInBrowserCommand.UpdateContext(Environment, ContextualResourceModel);
            QuickDebugCommand.UpdateContext(Environment, ContextualResourceModel);
        }

        #region commands
        
        public AuthorizeCommand SaveCommand => _saveCommand ??
                       (_saveCommand = new AuthorizeCommand(AuthorizationContext.Contribute, param => Save(), param => CanSave()));

        public AuthorizeCommand RunCommand => _runCommand ??
                       (_runCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => Debug(ContextualResourceModel, false), param => CanExecute()));

        public AuthorizeCommand ViewInBrowserCommand => _viewInBrowserCommand ??
                       (_viewInBrowserCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => ViewInBrowser(), param => CanDebug()));

        public AuthorizeCommand DebugCommand => _debugCommand ??
                       (_debugCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => Debug(), param => CanDebug()));

        public AuthorizeCommand QuickViewInBrowserCommand => _quickViewInBrowserCommand ??
                       (_quickViewInBrowserCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => QuickViewInBrowser(), param => CanViewInBrowser()));

        public AuthorizeCommand QuickDebugCommand => _quickDebugCommand ??
                       (_quickDebugCommand = new AuthorizeCommand(AuthorizationContext.Execute, param => QuickDebug(), param => CanDebug()));

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
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackEvent(TrackEventDebugOutput.EventCategory, TrackEventDebugOutput.Debug);
            }
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

        WorkflowInputDataViewModel SetupForDebug(IContextualResourceModel resourceModel, bool isDebug)
        {
            var inputDataViewModel = GetWorkflowInputDataViewModel(resourceModel, isDebug);
            inputDataViewModel.DebugExecutionStart += () =>
            {
                SetDebugStatus(DebugStatus.Executing);
                DebugOutputViewModel.DebugStatus = DebugStatus.Executing;
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
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackEvent(TrackEventDebugOutput.EventCategory,TrackEventDebugOutput.F7Browser);
            }
            if (!ContextualResourceModel.IsWorkflowSaved)
            {
                var successfuleSave = Save(ContextualResourceModel, true);
                if (!successfuleSave)
                {
                    return;
                }
            }
            ViewInBrowserInternal(ContextualResourceModel, true);
        }

        private void ViewInBrowserInternal(IContextualResourceModel model, bool quickDebugClicked)
        {
            var workflowInputDataViewModel = GetWorkflowInputDataViewModel(model, false);
            workflowInputDataViewModel.LoadWorkflowInputs();
            //check if quick debug called then dont log view in browser event 
            if (quickDebugClicked)
            {
                workflowInputDataViewModel.WithoutActionTrackingViewInBrowser();
            }
            else
            {
                workflowInputDataViewModel.ViewInBrowser();
            } 
           
        }

        public void QuickDebug()
        {
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackEvent(TrackEventDebugOutput.EventCategory,TrackEventDebugOutput.F6Debug);
            }
            if (DebugOutputViewModel.IsProcessing)
            {
                StopExecution();
            }
            if (WorkflowDesignerViewModel.ValidatResourceModel(ContextualResourceModel.DataList))
            {
                if (!ContextualResourceModel.IsWorkflowSaved && !_workspaceSaved)
                {
                    var successfuleSave = Save(ContextualResourceModel, true);
                    if (!successfuleSave)
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

        bool _waitingforDialog;
        bool _workspaceSaved;

        public void ShowSaveDialog(IContextualResourceModel resourceModel, bool addToTabManager) => SaveDialogHelper.ShowNewWorkflowSaveDialog(resourceModel, null, addToTabManager);

        public bool Save() => Save(false, false);
        public bool Save(bool isLocalSave, bool isStudioShutdown)
        {
            var saveResult = Save(ContextualResourceModel, isLocalSave);
            WorkSurfaceViewModel?.NotifyOfPropertyChange("DisplayName");
            if (!isLocalSave)
            {
                ViewModelUtils.RaiseCanExecuteChanged(DebugOutputViewModel?.AddNewTestCommand);
            }
            return saveResult;
        }

        public bool IsEnvironmentConnected() => Environment != null && Environment.IsConnected;

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

        protected virtual bool Save(IContextualResourceModel resource, bool isLocalSave) => Save(resource, isLocalSave, true);

        protected virtual bool Save(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager)
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

            if (resource.IsNewWorkflow && !isLocalSave && !_waitingforDialog && !resource.IsNotWarewolfPath)
            {
                _waitingforDialog = true;
                _saveDialogAction(resource, addToTabManager, () => _waitingforDialog = false);

                return true;
            }
            if (resource.IsNewWorkflow && resource.IsNotWarewolfPath && !isLocalSave)
            {
                var overwrite = _popupController.ShowOverwiteResourceDialog();
                if (overwrite == MessageBoxResult.Cancel)
                {
                    return false;
                }
                resource.IsNotWarewolfPath = false;
            }

            BindToModel();
            if (!isLocalSave)
            {
                var saveResult = resource.Environment.ResourceRepository.SaveToServer(resource);
                DispatchServerDebugMessage(saveResult, resource);
                resource.IsWorkflowSaved = true;
                _workspaceSaved = true;
                UpdateResourceVersionInfo(resource);
            }
            else
            {
                _workspaceSaved = true;
                var saveResult = resource.Environment.ResourceRepository.Save(resource);
                DisplaySaveResult(saveResult, resource);
            }
            return true;
        }

        static void UpdateResourceVersionInfo(IContextualResourceModel resource)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            var explorerViewModel = mainViewModel?.ExplorerViewModel;
            var environmentViewModel = explorerViewModel?.Environments?.FirstOrDefault(model => model.ResourceId == resource.Environment.EnvironmentID);
            var explorerItemViewModel = environmentViewModel?.Children?.Flatten(model => model.Children).FirstOrDefault(model => model.ResourceId == resource.ID);
            if (explorerItemViewModel != null)
            {
                explorerItemViewModel.IsMergeVisible = true;
                if (explorerItemViewModel.GetType() == typeof(VersionViewModel) && explorerItemViewModel.Parent != null)
                {
                    explorerItemViewModel.Parent.AreVersionsVisible = true;
                }

            }
            mainViewModel?.UpdateExplorerWorkflowChanges(resource.ID);
        }

        void DisplaySaveResult(ExecuteMessage result, IContextualResourceModel resource)
        {
            DispatchServerDebugMessage(result, resource);
        }

        void DispatchServerDebugMessage(ExecuteMessage message, IContextualResourceModel resource)
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