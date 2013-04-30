#region

using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Common.ExtMethods;
using Dev2.Composition;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.AppResources.Messages;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Diagnostics;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Studio.Webs;
using Dev2.Studio.Webs.Callbacks;
using Dev2.Util;
using Dev2.Utilities;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager.Events;
using Action = System.Action;

#endregion

namespace Dev2.Studio.ViewModels
{
    [Export(typeof(IMainViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class MainViewModel : BaseConductor<WorkSurfaceContextViewModel>, IMainViewModel,
                                        IHandle<DeleteResourceMessage>, 
                                        IHandle<ShowDependenciesMessage>, 
                                        IHandle<AddWorkSurfaceMessage>,
                                        IHandle<DebugWriterWriteMessage>,
                                        IHandle<SetActiveEnvironmentMessage>,
                                        IHandle<ShowEditResourceWizardMessage>,
                                        IHandle<DeployResourcesMessage>,
                                        IHandle<ShowHelpTabMessage>,
                                        IHandle<SettingsSaveCancelMessage>, 
                                        IHandle<RemoveResourceAndCloseTabMessage>,
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
        private ICommand _startStopRecordedFeedbackCommand;
        private ICommand _viewInBrowserCommand;
        private bool _createDesigners;
        private ICommand _notImplementedCommand;

        #endregion

        #region Properties

        #region imports

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

        public IWorkspaceItemRepository WorkspaceItemRepository { get; set; }

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        #endregion imports

        public ExplorerViewModel ExplorerViewModel
        {
            get { return _explorerViewModel; }
            set
            {
                if (_explorerViewModel == value) return;
                _explorerViewModel = value;
                NotifyOfPropertyChange(() => ExplorerViewModel);
            }
        }

        #endregion

        #region Private Method

        public IEnvironmentModel ActiveEnvironment
        {
            get { return _activeEnvironment; }
            set
            {
                if (value != null)
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
                if (ActiveItem == null || ActiveItem.WorkSurfaceViewModel == null)
                    return null;

                return ResourceHelper
                    .GetContextualResourceModel(ActiveItem.WorkSurfaceViewModel);
            }
        }


        #endregion

          #region Commands

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
            get { return _deployAllCommand ?? (_deployAllCommand = new RelayCommand(param => DeployAll())); }
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
                                            new RelayCommand(param => AddSettingsWorkSurface(ActiveEnvironment)));
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

        public bool CloseCurrent { get; set; }

        #endregion

        #region IHandle

        public void Handle(AddWorkSurfaceMessage message)
        {
            AddWorkSurface(message.WorkSurfaceObject);
        }

        public void Handle(DeleteResourceMessage message)
        {
            DeleteResource(message.ResourceModel as IContextualResourceModel);
        }

        public void Handle(SetActiveEnvironmentMessage message)
        {
            ActiveEnvironment = message.EnvironmentModel;
        }

        public void Handle(SettingsSaveCancelMessage message)
        {
            RemoveSettingsWorkSurface(message.Environment);
        }

        public void Handle(ShowDependenciesMessage message)
        {
            AddDependencyVisualizerWorkSurface(message.ResourceModel as IContextualResourceModel);
        }

        public void Handle(ShowEditResourceWizardMessage message)
        {
            if (message.ResourceModel.ResourceType == ResourceType.WorkflowService)
            {
                AddWorkSurfaceContext(message.ResourceModel as IContextualResourceModel);
                return;
            }

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
        #endregion

        #region Private Methods

        #region context management

        private void DeleteContext(IContextualResourceModel model)
        {
            var context = FindWorkSurfaceContextViewModel(model);
            if (context == null)
            {
                return;
            }

            context.DeleteRequested = true;
            DeactivateItem(context, true);
        }

        private void AddUniqueWorkSurface<T>
            (WorkSurfaceContext context, Tuple<string, object>[] initParms = null)
            where T : IWorkSurfaceViewModel
        {
            WorkSurfaceContextViewModel startCtx = WorkSurfaceContextFactory.Create<T>(context, initParms);
            Items.Add(startCtx);
            ActivateItem(startCtx);
        }

        private void ActivateOrCreateUniqueWorkSurface<T>(WorkSurfaceContext context,
                                                          Tuple<string, object>[] initParms = null)
            where T : IWorkSurfaceViewModel
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(context);
            bool exists = ActivateWorkSurfaceIfPresent(key, initParms);

            if (!exists)
                AddUniqueWorkSurface<T>(context, initParms);
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

            if (currentContext != null)
            {
                if (initParms != null)
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
            if (resourceModel == null)
            {
                return;
            }

            //Activates if exists
            var exists = ActivateWorkSurfaceIfPresent(resourceModel);

            if (exists)
            {
                return;
            }

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
  
        #endregion

        public void AddDeployResourcesWorkSurface(object input)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);
            bool exist = ActivateWorkSurfaceIfPresent(key);

            if (exist)
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

        void TempSave(IEnvironmentModel activeEnvironment, string resourceType)
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
            if (debugState == null)
            {
                return;
            }
            var key = WorkSurfaceKeyFactory.CreateKey(debugState);
            var ctx = FindWorkSurfaceContextViewModel(key);
            ctx.DisplayDebugOutput(debugState);
        }

        private void DeployAll()
        {
            object payload = null;

            if (CurrentResourceModel != null && CurrentResourceModel.Environment != null)
            {
                payload = CurrentResourceModel.Environment;
            }
            else if (ActiveEnvironment != null)
            {
                payload = ActiveEnvironment;
            }

            AddDeployResourcesWorkSurface(payload);
        }

        public void TryRemoveContext(IContextualResourceModel model)
        {
            WorkSurfaceContextViewModel context = FindWorkSurfaceContextViewModel(model);
            if (context != null)
            {
                context.DeleteRequested = true;
                DeactivateItem(context, true);
            }
        }

        private void DisplayAboutDialogue()
        {
            WindowManager.ShowDialog(DialogViewModelFactory.CreateAboutDialog());
        }

        private bool ShowRemovePopup(IWorkflowDesignerViewModel workflowVM)
        {
            var result = PopupProvider.Show(StringResources.DialogBody_NotSaved, StringResources.DialogTitle_NotSaved,
                                            MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    EventAggregator.Publish(new SaveResourceMessage(workflowVM.ResourceModel,false));
                return true;
                case MessageBoxResult.No:
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
            if (currentRecordFeedbackAction == null)
            {
                var recorderFeedbackAction = new RecorderFeedbackAction();
                FeedbackInvoker.InvokeFeedback(recorderFeedbackAction);
            }
                //stop feedback
            else
            {
                currentRecordFeedbackAction.FinishFeedBack();
            }
        }

        private void DisplayResourceWizard(IContextualResourceModel resourceModel, bool isedit)
        {
            if (resourceModel == null)
            {
                return;
            }

            if (isedit)
            {
                SaveOpenTabs();
            }

            WebController.DisplayDialogue(resourceModel, isedit);
        }

        private void ShowNewResourceWizard(string resourceType)
        {
            if (resourceType == "Workflow")
            {
                //Massimo.Guerrera:23-04-2013 - Added for PBI 8723
                TempSave(ActiveEnvironment, resourceType);
            }
            else
            {
            var resourceModel = ResourceModelFactory.CreateResourceModel(ActiveEnvironment, resourceType);
            DisplayResourceWizard(resourceModel, false);
        }
        }

        private void ShowEditResourceWizard(object resourceModelToEdit)
        {
            var resourceModel = resourceModelToEdit as IContextualResourceModel;
            DisplayResourceWizard(resourceModel, true);
        }

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
            if (!ResourceDependencyService.HasDependencies(model))
                return true;

            var dialog = new DeleteResourceDialog(model);
            var result = dialog.ShowDialog();
            if (dialog.OpenDependencyGraph)
            {
                AddDependencyVisualizerWorkSurface(model);
                return false;
            }
            return result.HasValue && result.Value;
        }

        private void DeleteResource(IContextualResourceModel model)
        {
            if (model == null)
            {
                return;
            }

            if (!ConfirmDelete(model))
            {
                return;
            }

            var response = model.Environment.ResourceRepository.DeleteResource(model);
            var success = response != null && response.IsSuccessResponse();
            if (!success)
            {
                return;
            }

            DeleteContext(model);
            EventAggregator.Publish(new RemoveNavigationResourceMessage(model));
        }

        #endregion delete

        #endregion Private Methods

        #region ctor

        public MainViewModel()
            : this(Core.EnvironmentRepository.Instance)
        {
        }


        public MainViewModel(IEnvironmentRepository environmentRepository, bool createDesigners = true)
        {
            _createDesigners = createDesigners;

            EnvironmentRepository = environmentRepository;
            WorkspaceItemRepository = ImportService.GetExportValue<IWorkspaceItemRepository>();

            AddStartTabs();
        }

        #endregion ctor

        #region Public Methods

        #region Tab Management

        public void AddStartTabs()
        {
            AddContextsForWorkspaceItems();

            string path = FileHelper.GetFullPath(StringResources.Uri_Studio_Homepage);
            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage
                                                             , new[] { new Tuple<string, object>("Uri", path) });
        }

        /// <summary>
        ///     Saves all open tabs locally and writes the open tabs the to collection of workspace items
        /// </summary>
        public void PersistTabs()
        {
            SaveWorkspaceItems();
            foreach (var ctx in Items)
            {
                ctx.Save(true);
            }
        }

        /// <summary>
        ///     Saves the open tabs.
        /// </summary>
        private void SaveOpenTabs()
        {
            foreach (var ctx in Items)
            {
                ctx.Save(true);
            }
        }

        public bool CloseWorkSurfaceContext(WorkSurfaceContextViewModel context, PaneClosingEventArgs e)
        {
            bool remove = true;
            if (!context.DeleteRequested)
            {
                var vm = context.WorkSurfaceViewModel;
                if (vm != null && vm.WorkSurfaceContext == WorkSurfaceContext.Workflow)
                {
                    var workflowVM = vm as IWorkflowDesignerViewModel;
                    if (workflowVM != null)
                    {
                        IContextualResourceModel resource = workflowVM.ResourceModel;
                        if (resource != null)
                        {
                            remove = resource.IsWorkflowSaved(workflowVM.ServiceDefinition);

                            //Massimo.Guerrera:26-04-2013 - Added for PBI 8723
                            if (resource.IsNewWorkflow && remove)
                            {
                                NewWorkflowNames.Instance.Remove(resource.ResourceName);
                            }

                    if (!remove)
                    {
                        remove = ShowRemovePopup(workflowVM);
                    }

            if (remove)
            {
                                RemoveWorkspaceItem(workflowVM);
                Items.Remove(context);
                EventAggregator.Publish(new TabClosedMessage(context));
                                if (e != null)
                                {
                                    e.Cancel = true;
                                }
                            }
                            else if (e != null)
                            {
                                e.Handled = true;
                                e.Cancel = false;
                            }
                        }
                    }
                }
            }

            return remove;
        }

        #endregion tab management

        public bool IsActiveEnvironmentConnected()
        {
            // Used for enabling / disabling basic server commands (Eg: Creating a new Workflow)
            if (ActiveEnvironment == null)
            {
                return false;
            }

            return ((ActiveEnvironment != null) && (ActiveEnvironment.IsConnected));
        }

        public void AddDependencyVisualizerWorkSurface(IContextualResourceModel resource)
        {
            if (resource == null)
                return;

            ActivateOrCreateUniqueWorkSurface<DependencyVisualiserViewModel>
                (WorkSurfaceContext.DependencyVisualiser,
                 new[] { new Tuple<string, object>("ResourceModel", resource) });
        }

        #endregion

        #region IHandle

        public void Handle(DeployResourcesMessage message)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);

            var exist = ActivateWorkSurfaceIfPresent(key);

            if (exist)
            {
                EventAggregator.Publish(new SelectItemInDeployMessage(message.ViewModel));
            }
            else
            {
                AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateDeployViewModel(message.ViewModel));
            }
        }


        public void AddSettingsWorkSurface(IEnvironmentModel environment)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Settings, environment.DataListChannel.ServerID);

            var exist = ActivateWorkSurfaceIfPresent(key);

            if (exist)
            {
                return;
            }

            AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateRuntimeConfigurationViewModel(environment));
        }

        public void RemoveSettingsWorkSurface(IEnvironmentModel environment)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Settings,
                                                      environment.DataListChannel.ServerID);

            var viewModel = FindWorkSurfaceContextViewModel(key);

            if (viewModel != null)
            {
                DeactivateItem(viewModel, true);
            }
        }

        public void AddHelpTabWorkSurface(string uriToDisplay)
        {
            if (!string.IsNullOrWhiteSpace(uriToDisplay))
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

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    OnDeactivate(true);
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Implementation of IHandle<RemoveResourceAndCloseTabMessage>

        public void OnImportsSatisfied()
        {
            DisplayName = String.Format("Business Design Studio ({0})", SecurityContext.UserIdentity.Name);
            ExplorerViewModel = new ExplorerViewModel();
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            if (message.ResourceToRemove != null)
            {
                WorkSurfaceContextViewModel wfscvm = FindWorkSurfaceContextViewModel(message.ResourceToRemove);
                DeactivateItem(wfscvm, true);            
                IResourceModel res = message.ResourceToRemove.Environment.ResourceRepository.FindSingle(c => c.DisplayName == message.ResourceToRemove.DisplayName);
                if (res != null)
                {
                    message.ResourceToRemove.Environment.ResourceRepository.Remove(res);
                }
            }
        }

        public override void DeactivateItem(WorkSurfaceContextViewModel item, bool close)
        {
            bool deactivate = true;
            if (close)
            {
                deactivate = CloseWorkSurfaceContext(item, null);
            }

            if (deactivate)
            {
            if (_previousActive != item)
                ActivateItem(_previousActive);

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
            if (close)
                PersistTabs();

            base.OnDeactivate(close);
        }

        protected override void OnActivationProcessed(WorkSurfaceContextViewModel item, bool success)
        {
            if (success)
            {
            item.Parent = this;
            }
            base.OnActivationProcessed(item, success);
        }

        public override void ActivateItem(WorkSurfaceContextViewModel item)
        {
            _previousActive = ActiveItem;
            base.ActivateItem(item);
        }

        #endregion Protected Methods

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
            if (EnvironmentRepository == null || WorkspaceItemRepository == null) return;

            foreach (var workspaceItem in WorkspaceItemRepository.WorkspaceItems)
            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = workspaceItem;
                IEnvironmentModel environment = null;
                foreach (var env in EnvironmentRepository.All())
                {
                    if (!env.IsConnected) break;
                    if (!(env.DsfChannel is IStudioClientContext)) break;
                    var channel = (IStudioClientContext)env.DsfChannel;
                    if (channel.ServerID == item.ServerID)
                        environment = env;
                }

                if (environment == null || environment.ResourceRepository == null) continue;

                var resource = environment.ResourceRepository.All().FirstOrDefault(rm => rm.ResourceName == item.ServiceName)
                               as IContextualResourceModel;
                if (resource == null) continue;

                if (resource.ResourceType == ResourceType.WorkflowService)
                {
                    AddWorkSurfaceContext(resource);
                }
            }
        }

        #endregion

    }
}