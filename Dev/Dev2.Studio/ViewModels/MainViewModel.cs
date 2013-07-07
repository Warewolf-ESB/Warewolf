#region

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.ViewModels.Configuration;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Studio.Webs;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager.Events;
using UserInterfaceLayoutModel = Dev2.Studio.Core.Models.UserInterfaceLayoutModel;

#endregion

namespace Dev2.Studio.ViewModels
{
    // PBI 9397 - 2013.06.09 - TWR: made class non-sealed to facilitate testing i.e. creating mock sub-classes
    [Export(typeof(IMainViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainViewModel : BaseConductor<WorkSurfaceContextViewModel>, IMainViewModel,
                                        IHandle<DeleteResourceMessage>,
                                        IHandle<ShowDependenciesMessage>,
                                        IHandle<AddWorkSurfaceMessage>,
                                        IHandle<DebugWriterWriteMessage>,
                                        IHandle<SetActiveEnvironmentMessage>,
                                        IHandle<ShowEditResourceWizardMessage>,
                                        IHandle<DeployResourcesMessage>,
                                        IHandle<ShowHelpTabMessage>,
                                        IHandle<ShowNewResourceWizard>,
                                        IHandle<SettingsSaveCancelMessage>,
                                        IHandle<RemoveResourceAndCloseTabMessage>,
                                        IHandle<GetActiveEnvironmentCallbackMessage>,
                                        IHandle<SaveAllOpenTabsMessage>,
        IHandle<ShowReverseDependencyVisualizer>,
        IHandle<GetContextualEnvironmentCallbackMessage>,
                                        IPartImportsSatisfiedNotification
    {
        #region Fields

        private IEnvironmentModel _activeEnvironment;
        private ExplorerViewModel _explorerViewModel;
        private WorkSurfaceContextViewModel _previousActive;
        private bool _disposed;

        private RelayCommand<string> _newResourceCommand;
        private ICommand _addStudioShortcutsPageCommand;
        private ICommand _deployAllCommand;
        private ICommand _deployCommand;
        private ICommand _displayAboutDialogueCommand;
        private ICommand _exitCommand;
        private ICommand _resetLayoutCommand;
        private ICommand _settingsCommand;
        private ICommand _startFeedbackCommand;
        private ICommand _showCommunityPageCommand;
        private ICommand _startStopRecordedFeedbackCommand;
        private ICommand _reportsCommand;
        private bool _createDesigners;
        private ICommand _notImplementedCommand;
        private ICommand _showStartPageCommand;

        #endregion

        #region Properties

        #region imports
        [Import]
        public FlowController FlowController { get; set; }

        [Import(typeof(IWebController))]
        public IWebController WebController { get; set; }

        [Import]
        public IWindowManager WindowManager { get; set; }

        [Import]
        public IPopupController PopupProvider { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        [Import]
        public IFeedbackInvoker FeedbackInvoker { get; set; }

        [Import]
        public IFeedBackRecorder FeedBackRecorder { get; set; }

        [Import(typeof(IFrameworkRepository<UserInterfaceLayoutModel>))]
        public IFrameworkRepository<UserInterfaceLayoutModel> UserInterfaceLayoutRepository { get; set; }

        [Import(typeof(IResourceDependencyService))]
        public IResourceDependencyService ResourceDependencyService { get; set; }

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        #endregion imports

        public bool CloseCurrent { get; set; }

        public IWorkspaceItemRepository WorkspaceItemRepository { get; set; }

        public ExplorerViewModel ExplorerViewModel
        {
            get { return _explorerViewModel; }
            set
            {
                if(_explorerViewModel == value) return;
                _explorerViewModel = value;
                NotifyOfPropertyChange(() => ExplorerViewModel);
            }
        }

        public IEnvironmentModel ActiveEnvironment
        {
            get { return _activeEnvironment; }
            set
            {
                if(value != null)
                {
                    _activeEnvironment = value;
                }

                NotifyOfPropertyChange(() => ActiveEnvironment);
            }
        }

        public IContextualResourceModel CurrentResourceModel
        {
            get
            {
                if(ActiveItem == null || ActiveItem.WorkSurfaceViewModel == null)
                    return null;

                return ResourceHelper
                    .GetContextualResourceModel(ActiveItem.WorkSurfaceViewModel);
            }
        }

        // BUG 9798 - 2013.06.25 - TWR : added
        public IBrowserPopupController BrowserPopupController { get; private set; }

        #endregion

        #region Commands

        public ICommand EditCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new RelayCommand((p) => { }, param => false);
                }
                return ActiveItem.EditCommand;
            }
        }
        public ICommand SaveCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new RelayCommand((p) => { }, param => false);
                }
                return ActiveItem.SaveCommand;
            }
        }
        public ICommand DebugCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new RelayCommand((p) => { }, param => false);
                }
                return ActiveItem.DebugCommand;
            }
        }
        public ICommand ViewInBrowserCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new RelayCommand((p) => { }, param => false);
                }
                return ActiveItem.ViewInBrowserCommand;
            }
        }
        public ICommand NotImplementedCommand
        {
            get
            {
                return _notImplementedCommand ??
                       (_notImplementedCommand = new RelayCommand(param => MessageBox.Show("Please implement me!")));
            }
        }

        public ICommand AddStudioShortcutsPageCommand
        {
            get
            {
                return _addStudioShortcutsPageCommand ??
                       (_addStudioShortcutsPageCommand = new RelayCommand(param => AddShortcutKeysWorkSurface()));
            }
        }

        public ICommand DisplayAboutDialogueCommand
        {
            get
            {
                return _displayAboutDialogueCommand ??
                       (_displayAboutDialogueCommand = new RelayCommand(param => DisplayAboutDialogue()));
            }
        }

        public ICommand ShowStartPageCommand
        {
            get
            {
                return _showStartPageCommand ?? (_showStartPageCommand = new RelayCommand(param => ShowStartPage()));
            }
        }

        public ICommand ShowCommunityPageCommand
        {
            get { return _showCommunityPageCommand ?? (_showCommunityPageCommand = new RelayCommand(param => ShowCommunityPage())); }
        }

        public ICommand StartFeedbackCommand
        {
            get { return _startFeedbackCommand ?? (_startFeedbackCommand = new RelayCommand(param => StartFeedback())); }
        }

        public ICommand StartStopRecordedFeedbackCommand
        {
            get
            {
                return _startStopRecordedFeedbackCommand ??
                       (_startStopRecordedFeedbackCommand = new RelayCommand(param => StartStopRecordedFeedback()));
            }
        }

        public ICommand DeployAllCommand
        {
            get
            {
                return _deployAllCommand ?? (_deployAllCommand = new RelayCommand(param => DeployAll(),
                                                                     param => IsActiveEnvironmentConnected()));
            }
        }

        public ICommand ResetLayoutCommand
        {
            get
            {
                return _resetLayoutCommand ??
                       (_resetLayoutCommand = new RelayCommand(param =>
                                                               EventAggregator.Publish(
                                                                   new ResetLayoutMessage(param as FrameworkElement)),
                                                               param => true));
            }
        }

        public ICommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand =
                                            new RelayCommand(param => AddSettingsWorkSurface()));
            }
        }

        public ICommand ReportsCommand
        {
            get
            {
                return _reportsCommand ?? (_reportsCommand =
                                            new RelayCommand(param => AddReportsWorkSurface()));
            }
        }

        public RelayCommand<string> NewResourceCommand
        {
            get
            {
                return _newResourceCommand ??
                       (_newResourceCommand = new RelayCommand<string>(ShowNewResourceWizard,
                                                                       param => IsActiveEnvironmentConnected()));
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                return _exitCommand ??
                       (_exitCommand =
                        new RelayCommand(param =>
                                         Application.Current.Shutdown(), param => true));
            }
        }

        public ICommand DeployCommand
        {
            get
            {
                return _deployCommand ??
                       (_deployCommand = new RelayCommand(param => AddDeployResourcesWorkSurface(CurrentResourceModel)));
            }
        }

        #endregion

        // PBI 9512 - 2013.06.07 - TWR: added
        public ILatestGetter LatestGetter { get; private set; }

        #region ctor

        public MainViewModel()
            : this(Core.EnvironmentRepository.Instance)
        {
        }

        public MainViewModel(IEnvironmentRepository environmentRepository, bool createDesigners = true, IBrowserPopupController browserPopupController = null)
        {
            if(environmentRepository == null)
            {
                throw new ArgumentNullException("environmentRepository");
            }

            _createDesigners = createDesigners;
            BrowserPopupController = browserPopupController ?? new ExternalBrowserPopupController(); // BUG 9798 - 2013.06.25 - TWR : added
            LatestGetter = new LatestWebGetter(); // PBI 9512 - 2013.06.07 - TWR: added

            EnvironmentRepository = environmentRepository;
            WorkspaceItemRepository = ImportService.GetExportValue<IWorkspaceItemRepository>();

            AddStartTabs();
        }

        #endregion ctor

        #region IHandle

        public void Handle(ShowReverseDependencyVisualizer message)
        {
            if(message.Model != null)
            {
                AddReverseDependencyVisualizerWorkSurface(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            SaveOpenTabs();
        }

        public void Handle(GetActiveEnvironmentCallbackMessage message)
        {
            message.Callback.Invoke(ActiveEnvironment);
        }

        public void Handle(GetContextualEnvironmentCallbackMessage message)
        {
            message.Callback.Invoke(ActiveItem.Environment);
        }

        public void Handle(AddWorkSurfaceMessage message)
        {
            AddWorkSurface(message.WorkSurfaceObject);
        }

        public void Handle(DeleteResourceMessage message)
        {
            DeleteResource(message.ResourceModel as IContextualResourceModel, message.ShowDialog);
        }

        public void Handle(SetActiveEnvironmentMessage message)
        {
            ActiveEnvironment = message.EnvironmentModel;
            EventAggregator.Publish(new UpdateActiveEnvironmentMessage(ActiveEnvironment));
        }

        public void Handle(SettingsSaveCancelMessage message)
        {
            RemoveSettingsWorkSurface(message.Environment);
        }

        public void Handle(ShowDependenciesMessage message)
        {
            var model = message.ResourceModel as IContextualResourceModel;
            if (model == null)
            {
                return;
            }

            if (message.ShowDependentOnMe)
            {
                AddReverseDependencyVisualizerWorkSurface(model);    
            }
            else
            {
                AddDependencyVisualizerWorkSurface(model);        
            }
        }

        public void Handle(ShowEditResourceWizardMessage message)
        {
            ShowEditResourceWizard(message.ResourceModel);
        }

        public void Handle(ShowHelpTabMessage message)
        {
            AddHelpTabWorkSurface(message.HelpLink);
        }

        public void Handle(DebugWriterWriteMessage message)
        {
            DisplayDebugOutput(message.DebugState);
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            if(message.ResourceToRemove == null)
            {
                return;
            }

            var wfscvm = FindWorkSurfaceContextViewModel(message.ResourceToRemove);
            DeactivateItem(wfscvm, true);
            WorkspaceItemRepository.Remove(message.ResourceToRemove);
            _previousActive = null;

            var res = message.ResourceToRemove.Environment
                .ResourceRepository.FindSingle(c => c.ResourceName == message.ResourceToRemove.ResourceName);

            if(res != null)
            {
                message.ResourceToRemove.Environment.ResourceRepository.Remove(res);
            }
        }

        public void Handle(DeployResourcesMessage message)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);

            var exist = ActivateWorkSurfaceIfPresent(key);

            if(exist)
            {
                EventAggregator.Publish(new SelectItemInDeployMessage(message.ViewModel));
            }
            else
            {
                AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateDeployViewModel(message.ViewModel));
            }
        }

        public void Handle(ShowNewResourceWizard message)
        {
            ShowNewResourceWizard(message.ResourceType);
        }
        #endregion

        #region Private Methods

        private void TempSave(IEnvironmentModel activeEnvironment, string resourceType)
        {
            string newWorflowName = NewWorkflowNames.Instance.GetNext();

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(activeEnvironment, resourceType,
                                                                                              resourceType);
            tempResource.Category = "Unassigned";
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateResourceViewModel(tempResource));
        }

        private void DisplayDebugOutput(IDebugState debugState)
        {
            if(debugState == null)
            {
                return;
            }
            var key = WorkSurfaceKeyFactory.CreateKey(debugState);
            // DEBUG FAILS HERE BECAUSE ctx is always null!
            var ctx = FindWorkSurfaceContextViewModel(key);
            if(ctx != null)
            {
                ctx.DisplayDebugOutput(debugState);
            }
        }

        private void DeployAll()
        {
            object payload = null;

            if(CurrentResourceModel != null && CurrentResourceModel.Environment != null)
            {
                payload = CurrentResourceModel.Environment;
            }
            else if(ActiveEnvironment != null)
            {
                payload = ActiveEnvironment;
            }

            AddDeployResourcesWorkSurface(payload);
        }

        private void DisplayAboutDialogue()
        {
            WindowManager.ShowDialog(DialogViewModelFactory.CreateAboutDialog());
        }

        private bool ShowRemovePopup(IWorkflowDesignerViewModel workflowVM)
        {
            var result = PopupProvider.Show(StringResources.DialogBody_NotSaved, StringResources.DialogTitle_NotSaved,
                                            MessageBoxButton.YesNoCancel);

            switch(result)
            {
                case MessageBoxResult.Yes:
                    EventAggregator.Publish(new SaveResourceMessage(workflowVM.ResourceModel, false, false));
                    return true;
                case MessageBoxResult.No:
                    // We need to remove it ;)
                    var model = workflowVM.ResourceModel;
                    try
                    {
                        if(workflowVM.EnvironmentModel.ResourceRepository.DoesResourceExistInRepo(model) && workflowVM.ResourceModel.IsNewWorkflow)
                        {
                            EventAggregator.Publish(new DeleteResourceMessage(model, false));
                        }
                    }
                    catch(Exception e)
                    {
                        StudioLogger.LogMessage("Some clever chicken threw this exception : " + e.Message);
                    }

                    NewWorkflowNames.Instance.Remove(workflowVM.ResourceModel.ResourceName);
                    return true;
                case MessageBoxResult.None:
                    return true;
                case MessageBoxResult.Cancel:
                    return false;
            }
            return false;
        }

        private void StartStopRecordedFeedback()
        {
            var currentRecordFeedbackAction = FeedbackInvoker.CurrentAction as IAsyncFeedbackAction;

            //start feedback
            if(currentRecordFeedbackAction == null)
            {
                var recorderFeedbackAction = new RecorderFeedbackAction();
                FeedbackInvoker.InvokeFeedback(recorderFeedbackAction);
            }
            //stop feedback
            else
            {
                // PBI 9598 - 2013.06.10 - TWR : added environment parameter
                currentRecordFeedbackAction.FinishFeedBack(ActiveEnvironment);
            }
        }

        private void DisplayResourceWizard(IContextualResourceModel resourceModel, bool isedit, bool isSaveDialogStandAlone = false)
        {
            if(resourceModel == null)
            {
                return;
            }

            if(isedit && resourceModel.ServerResourceType == ResourceType.WorkflowService.ToString())
            {
                SaveOpenTabs();
            }

            WebController.DisplayDialogue(resourceModel, isedit);
        }

        private void ShowNewResourceWizard(string resourceType)
        {
            if(resourceType == "Workflow")
            {
                //Massimo.Guerrera:23-04-2013 - Added for PBI 8723
                TempSave(ActiveEnvironment, resourceType);
            }
            else
            {
                var resourceModel = ResourceModelFactory.CreateResourceModel(ActiveEnvironment, resourceType);
                DisplayResourceWizard(resourceModel, false, false);
            }
        }

        private void ShowEditResourceWizard(object resourceModelToEdit)
        {
            var resourceModel = resourceModelToEdit as IContextualResourceModel;
            DisplayResourceWizard(resourceModel, true);
        }


        #endregion Private Methods

        #region Public Methods

        public void ShowStartPage()
        {
            var path = FileHelper.GetFullPath(StringResources.Uri_Studio_Homepage);

            // PBI 9512 - 2013.06.07 - TWR: added
            LatestGetter.GetLatest(StringResources.Uri_Studio_Homepage_Remote, path);

            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage
                                                             , new[] { new Tuple<string, object>("Uri", path) });
        }

        // PBI 9512 - 2013.06.07 - TWR: added
        public void ShowCommunityPage()
        {
            // BUG 9798 - 2013.06.25 - TWR : changed to launch external browser
            BrowserPopupController.ShowPopup(StringResources.Uri_Community_HomePage);
        }

        public bool IsActiveEnvironmentConnected()
        {
            // Used for enabling / disabling basic server commands (Eg: Creating a new Workflow)
            if(ActiveEnvironment == null)
            {
                return false;
            }

            return ((ActiveEnvironment != null) &&
                (ActiveEnvironment.IsConnected) &&
                (ActiveEnvironment.CanStudioExecute));
        }

        public void AddDependencyVisualizerWorkSurface(IContextualResourceModel resource)
        {
            if(resource == null)
                return;

            ActivateOrCreateWorkSurface<DependencyVisualiserViewModel>
                (WorkSurfaceContext.DependencyVisualiser, resource,
                 new[] { new Tuple<string, object>("GetDependsOnMe",false),new Tuple<string, object>("ResourceModel", resource)
                                 });
        }

        public void AddReverseDependencyVisualizerWorkSurface(IContextualResourceModel resource)
        {
            if(resource == null)
                return;

            ActivateOrCreateWorkSurface<DependencyVisualiserViewModel>
                (WorkSurfaceContext.ReverseDependencyVisualiser, resource,
                 new[]
                     {
                         new Tuple<string, object>("GetDependsOnMe", true),
                         new Tuple<string, object>("ResourceModel", resource)
                     });
        }

        public void AddSettingsWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<RuntimeConfigurationViewModel>
                (WorkSurfaceContext.Settings);
        }

        public void AddReportsWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<ReportsManagerViewModel>
                (WorkSurfaceContext.ReportsManager);
        }

        public void RemoveSettingsWorkSurface(IEnvironmentModel environment)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Settings,
                                                      environment.DataListChannel.ServerID);

            var viewModel = FindWorkSurfaceContextViewModel(key);

            if(viewModel != null)
            {
                DeactivateItem(viewModel, true);
            }
        }

        public void AddHelpTabWorkSurface(string uriToDisplay)
        {
            if(!string.IsNullOrWhiteSpace(uriToDisplay))
                ActivateOrCreateUniqueWorkSurface<HelpViewModel>
                    (WorkSurfaceContext.Help,
                     new[] { new Tuple<string, object>("Uri", uriToDisplay) });
        }

        public void AddShortcutKeysWorkSurface()
        {
            var path = FileHelper.GetFullPath(StringResources.Uri_Studio_Shortcut_Keys_Document);
            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.ShortcutKeys
                                                             , new[] { new Tuple<string, object>("Uri", path) });
        }

        public void StartFeedback()
        {
            FeedbackInvoker.InvokeFeedback(new EmailFeedbackAction(), new RecorderFeedbackAction());
        }

        #endregion

        #region Overrides

        protected override void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if(disposing)
                {
                    OnDeactivate(true);
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        public override void DeactivateItem(WorkSurfaceContextViewModel item, bool close)
        {
            bool success = true;
            if(close)
            {
                success = CloseWorkSurfaceContext(item, null);
            }

            if(success)
            {
                if(_previousActive != item && Items.Contains(_previousActive))
                {
                    ActivateItem(_previousActive);
                }


                base.DeactivateItem(item, close);
                CloseCurrent = true;
            }
            else
            {
                CloseCurrent = false;
            }
        }

        protected override void OnDeactivate(bool close)
        {
            if(close)
                PersistTabs();

            base.OnDeactivate(close);
        }

        protected override void OnActivationProcessed(WorkSurfaceContextViewModel item, bool success)
        {
            if(success)
            {
                if(item != null)
                {
                    item.Parent = this;
                    IWorkflowDesignerViewModel wfItem = item.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
                    if(wfItem != null)
                    {
                        AddWorkspaceItem(wfItem.ResourceModel);
                    }
                }
                NotifyOfPropertyChange(() => EditCommand);
                NotifyOfPropertyChange(() => SaveCommand);
                NotifyOfPropertyChange(() => DebugCommand);
                NotifyOfPropertyChange(() => ViewInBrowserCommand);
            }
            base.OnActivationProcessed(item, success);
        }

        public override void ActivateItem(WorkSurfaceContextViewModel item)
        {
            _previousActive = ActiveItem;
            base.ActivateItem(item);
        }

        #endregion

        #region ImportsSatisfied

        public void OnImportsSatisfied()
        {
            DisplayName = "Warewolf";
            ExplorerViewModel = new ExplorerViewModel();
        }

        #endregion

        #region Resource Deletion

        private bool ConfirmDelete(IContextualResourceModel model)
        {
            var deletePrompt = String.Format(StringResources.DialogBody_ConfirmDelete, model.ResourceName,
                                             model.ResourceType.GetDescription());
            var deleteAnswer = PopupProvider.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete,
                                                  MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            var shouldDelete = deleteAnswer == MessageBoxResult.Yes;
            return shouldDelete && ConfirmDeleteAfterDependencies(model);
        }

        private bool ConfirmDeleteAfterDependencies(IContextualResourceModel model)
        {
            if(!ResourceDependencyService.HasDependencies(model))
                return true;

            var dialog = new DeleteResourceDialog(model);
            var result = dialog.ShowDialog();
            if(dialog.OpenDependencyGraph)
            {
                AddReverseDependencyVisualizerWorkSurface(model);
            }
            return result.HasValue && result.Value;
        }

        private void DeleteResource(IContextualResourceModel model, bool showConfirm)
        {
            if(model == null)
            {
                return;
            }

            if(showConfirm && !ConfirmDelete(model))
            {
                return;
            }

            var response = model.Environment.ResourceRepository.DeleteResource(model);
            var success = response != null && response.IsSuccessResponse();
            if(!success)
            {
                return;
            }

            //If its deleted from loalhost, and is a server, also delete from repository
            if(model.Environment.IsLocalHost())
            {
                if(model.ResourceType == ResourceType.Source)
                {
                    if(model.ServerResourceType == "Server")
                    {
                        var appserUri =
                            Core.EnvironmentRepository.GetAppServerUriFromConnectionString(model.ConnectionString);
                        var environment = EnvironmentRepository.Get(appserUri);

                        if(environment != null)
                        {
                            EventAggregator.Publish(new EnvironmentDeletedMessage(environment));
                            EnvironmentRepository.Remove(environment);
                        }
                    }
                }

            }

            DeleteContext(model);
            EventAggregator.Publish(new RemoveNavigationResourceMessage(model));
        }

        #endregion delete

        #region WorkspaceItems management

        private void SaveWorkspaceItems()
        {
            WorkspaceItemRepository.Write();
        }

        private void AddWorkspaceItem(IContextualResourceModel model)
        {
            WorkspaceItemRepository.AddWorkspaceItem(model);
        }

        private void RemoveWorkspaceItem(IDesignerViewModel viewModel)
        {
            WorkspaceItemRepository.Remove(viewModel.ResourceModel);
        }

        private void AddContextsForWorkspaceItems()
        {
            if(EnvironmentRepository == null || WorkspaceItemRepository == null) return;

            for(int i = 0; i < WorkspaceItemRepository.WorkspaceItems.Count; i++)
            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = WorkspaceItemRepository.WorkspaceItems[i];
                IEnvironmentModel environment = null;
                foreach(var env in EnvironmentRepository.All())
                {
                    if(!env.IsConnected) break;
                    if(!(env.DsfChannel is IStudioClientContext)) break;
                    var channel = (IStudioClientContext)env.DsfChannel;
                    if(channel.ServerID == item.ServerID)
                        environment = env;
                }

                if(environment == null || environment.ResourceRepository == null) continue;

                var resource = environment.ResourceRepository.All().FirstOrDefault(rm =>
                {
                    var sameEnv = true;
                    if(item.EnvironmentID != Guid.Empty)
                    {
                        sameEnv = item.EnvironmentID == environment.ID;
                    }
                    return rm.ResourceName == item.ServiceName && sameEnv;
                })
                               as IContextualResourceModel;
                if(resource == null) continue;

                if(resource.ResourceType == ResourceType.WorkflowService)
                {
                    AddWorkSurfaceContext(resource);
                }
                i++;
            }

            //foreach(var workspaceItem in WorkspaceItemRepository.WorkspaceItems)
            //{
            //    //
            //    // Get the environment for the workspace item
            //    //
            //    IWorkspaceItem item = workspaceItem;
            //    IEnvironmentModel environment = null;
            //    foreach(var env in EnvironmentRepository.All())
            //    {
            //        if(!env.IsConnected) break;
            //        if(!(env.DsfChannel is IStudioClientContext)) break;
            //        var channel = (IStudioClientContext)env.DsfChannel;
            //        if(channel.ServerID == item.ServerID)
            //            environment = env;
            //    }

            //    if(environment == null || environment.ResourceRepository == null) continue;

            //    var resource = environment.ResourceRepository.All().FirstOrDefault(rm =>
            //    {
            //        var sameEnv = true;
            //        if(item.EnvironmentID != Guid.Empty)
            //        {
            //            sameEnv = item.EnvironmentID == environment.ID;
            //        }
            //        return rm.ResourceName == item.ServiceName && sameEnv;
            //    })
            //                   as IContextualResourceModel;
            //    if(resource == null) continue;

            //    if(resource.ResourceType == ResourceType.WorkflowService)
            //    {
            //        AddWorkSurfaceContext(resource);
            //    }
            //}
        }

        #endregion

        #region Tab Management

        public void AddDeployResourcesWorkSurface(object input)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);
            bool exist = ActivateWorkSurfaceIfPresent(key);

            if(exist)
            {
                EventAggregator.Publish(new SelectItemInDeployMessage(input));
            }
            else
            {
                WorkSurfaceContextViewModel context = WorkSurfaceContextFactory.CreateDeployViewModel(input);
                Items.Add(context);
                ActivateItem(context);
            }
        }

        private void DeleteContext(IContextualResourceModel model)
        {
            var context = FindWorkSurfaceContextViewModel(model);
            if(context == null)
            {
                return;
            }

            context.DeleteRequested = true;
            DeactivateItem(context, true);
        }

        private void CreateAndActivateUniqueWorkSurface<T>
            (WorkSurfaceContext context, Tuple<string, object>[] initParms = null)
            where T : IWorkSurfaceViewModel
        {
            WorkSurfaceContextViewModel ctx = WorkSurfaceContextFactory.Create<T>(context, initParms);
            AddAndActivateWorkSurface(ctx);
        }

        private void CreateAndActivateWorkSurface<T>
            (WorkSurfaceKey key, Tuple<string, object>[] initParms = null)
            where T : IWorkSurfaceViewModel
        {
            WorkSurfaceContextViewModel ctx = WorkSurfaceContextFactory.Create<T>(key, initParms);
            AddAndActivateWorkSurface(ctx);
        }

        private void ActivateOrCreateWorkSurface<T>(WorkSurfaceContext context, IContextualResourceModel resourceModel,
                                                          Tuple<string, object>[] initParms = null)
            where T : IWorkSurfaceViewModel
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(context, resourceModel);
            bool exists = ActivateWorkSurfaceIfPresent(key, initParms);

            if(!exists)
            {
                CreateAndActivateWorkSurface<T>(key, initParms);
            }
        }

        private void ActivateOrCreateUniqueWorkSurface<T>(WorkSurfaceContext context,
                                                          Tuple<string, object>[] initParms = null)
            where T : IWorkSurfaceViewModel
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(context);
            bool exists = ActivateWorkSurfaceIfPresent(key, initParms);

            if(!exists)
                CreateAndActivateUniqueWorkSurface<T>(context, initParms);
        }

        private bool ActivateWorkSurfaceIfPresent(IContextualResourceModel resource,
                                                  Tuple<string, object>[] initParms = null)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(resource);
            return ActivateWorkSurfaceIfPresent(key, initParms);
        }

        public bool ActivateWorkSurfaceIfPresent(WorkSurfaceKey key, Tuple<string, object>[] initParms = null)
        {
            WorkSurfaceContextViewModel currentContext = FindWorkSurfaceContextViewModel(key);

            if(currentContext != null)
            {
                if(initParms != null)
                    PropertyHelper.SetValues(
                        currentContext.WorkSurfaceViewModel, initParms);

                ActivateItem(currentContext);
                return true;
            }
            return false;
        }

        public WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(WorkSurfaceKey key)
        {
            return Items.FirstOrDefault(
                c => WorkSurfaceKeyEqualityComparer.Current.Equals(key, c.WorkSurfaceKey));
        }

        public WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IContextualResourceModel resource)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(resource);
            return FindWorkSurfaceContextViewModel(key);
        }

        public void AddWorkSurfaceContext(IContextualResourceModel resourceModel)
        {
            if(resourceModel == null)
            {
                return;
            }

            //Activates if exists
            var exists = ActivateWorkSurfaceIfPresent(resourceModel);

            if(exists)
            {
                return;
            }
            //            if(!resourceModel.Environment.ResourceRepository.IsInCache(resourceModel.ID))
            //            {
            //                resourceModel.Environment.ResourceRepository.ReloadResource(resourceModel.ResourceName, resourceModel.ResourceType, ResourceModelEqualityComparer.Current);
            //            }
            resourceModel.Environment.ResourceRepository.ReloadResource(resourceModel.ResourceName, resourceModel.ResourceType, ResourceModelEqualityComparer.Current);
            AddWorkspaceItem(resourceModel);
            AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateResourceViewModel(resourceModel, _createDesigners));
        }

        private void AddAndActivateWorkSurface(WorkSurfaceContextViewModel context)
        {
            Items.Add(context);
            ActivateItem(context);
        }

        private void AddWorkSurface(IWorkSurfaceObject obj)
        {
            TypeSwitch.Do(obj, TypeSwitch.Case<IContextualResourceModel>(AddWorkSurfaceContext));
        }

        public void TryRemoveContext(IContextualResourceModel model)
        {
            WorkSurfaceContextViewModel context = FindWorkSurfaceContextViewModel(model);
            if(context != null)
            {
                context.DeleteRequested = true;
                DeactivateItem(context, true);
            }
        }

        public void AddStartTabs()
        {
            // PBI 9512 - 2013.06.07 - TWR : refactored to use common method
            ShowStartPage();
            AddContextsForWorkspaceItems();
        }

        /// <summary>
        ///     Saves all open tabs locally and writes the open tabs the to collection of workspace items
        /// </summary>
        public void PersistTabs()
        {
            SaveWorkspaceItems();
            foreach(var ctx in Items)
            {
                ctx.Save(true);
            }
        }

        /// <summary>
        ///     Saves the open tabs.
        /// </summary>
        private void SaveOpenTabs()
        {
            foreach(var ctx in Items)
            {
                ctx.Save(true);
            }
        }

        public bool CloseWorkSurfaceContext(WorkSurfaceContextViewModel context, PaneClosingEventArgs e)
        {
            bool remove = true;
            if(context != null)
            {
                if(!context.DeleteRequested)
                {
                    var vm = context.WorkSurfaceViewModel;
                    if(vm != null && vm.WorkSurfaceContext == WorkSurfaceContext.Workflow)
                    {
                        var workflowVM = vm as IWorkflowDesignerViewModel;
                        if(workflowVM != null)
                        {
                            IContextualResourceModel resource = workflowVM.ResourceModel;
                            if(resource != null)
                            {
                                remove = resource.IsWorkflowSaved;

                                if(resource.IsNewWorkflow && remove)
                                {
                                    NewWorkflowNames.Instance.Remove(resource.ResourceName);
                                }

                                if(!remove)
                                {
                                    remove = ShowRemovePopup(workflowVM);
                                }

                                if(remove)
                                {
                                    RemoveWorkspaceItem(workflowVM);
                                    Items.Remove(context);
                                    EventAggregator.Publish(new TabClosedMessage(context));
                                    if(e != null)
                                    {
                                        e.Cancel = true;
                                    }
                                }
                                else if(e != null)
                                {
                                    e.Handled = true;
                                    e.Cancel = false;
                                }
                            }
                        }
                    }
                }
            }

            return remove;
        }

        #endregion
    }
}