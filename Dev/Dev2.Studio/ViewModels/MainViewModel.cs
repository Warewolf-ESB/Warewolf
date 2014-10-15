
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.AppResources.Repositories;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.CustomControls.Connections;
using Dev2.Factory;
using Dev2.Helpers;
using Dev2.Instrumentation;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Controller;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Enums;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Threading;
using Dev2.Utils;
using Dev2.Webs;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager.Events;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class MainViewModel : BaseConductor<WorkSurfaceContextViewModel>, IMainViewModel,
                                        IHandle<DeleteResourcesMessage>,
                                        IHandle<DeleteFolderMessage>,
                                        IHandle<ShowDependenciesMessage>,
                                        IHandle<AddWorkSurfaceMessage>,
                                        IHandle<SetActiveEnvironmentMessage>,
                                        IHandle<ShowEditResourceWizardMessage>,
                                        IHandle<DeployResourcesMessage>,
                                        IHandle<ShowHelpTabMessage>,
                                        IHandle<ShowNewResourceWizard>,
                                        IHandle<RemoveResourceAndCloseTabMessage>,
                                        IHandle<SaveAllOpenTabsMessage>,
                                        IHandle<ShowReverseDependencyVisualizer>,
                                        IHandle<FileChooserMessage>,
                                        IHandle<DisplayMessageBoxMessage>
    {
        #region Fields

        private IEnvironmentModel _activeEnvironment;
        private ExplorerViewModel _explorerViewModel;
        private WorkSurfaceContextViewModel _previousActive;
        private bool _disposed;

        private AuthorizeCommand<string> _newResourceCommand;
        private ICommand _addLanguageHelpPageCommand;
        private ICommand _deployAllCommand;
        private ICommand _deployCommand;
        private ICommand _displayAboutDialogueCommand;
        private ICommand _exitCommand;
        private AuthorizeCommand _settingsCommand;
        private AuthorizeCommand _schedulerCommand;
        private ICommand _startFeedbackCommand;
        private ICommand _showCommunityPageCommand;
        private ICommand _startStopRecordedFeedbackCommand;
        private readonly bool _createDesigners;
        private ICommand _showStartPageCommand;
        bool _hasActiveConnection;
        bool _canDebug = true;

        #endregion

        #region Properties

        #region imports
        public FlowController FlowController { get; set; }

        public IWebController WebController { get; set; }

        public IWindowManager WindowManager { get; set; }

        public IPopupController PopupProvider { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        public IFeedbackInvoker FeedbackInvoker { get; set; }

        public IFeedBackRecorder FeedBackRecorder { get; set; }

        public IFrameworkRepository<UserInterfaceLayoutModel> UserInterfaceLayoutRepository { get; set; }

        #endregion imports

        public bool CloseCurrent { get; set; }

        public static bool IsBusy { get; set; }

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
                if(!Equals(value, _activeEnvironment))
                {
                    _activeEnvironment = value;
                    OnActiveEnvironmentChanged();
                    NotifyOfPropertyChange(() => ActiveEnvironment);
                }
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

        public IBrowserPopupController BrowserPopupController { get; private set; }

        #endregion

        void OnActiveEnvironmentChanged()
        {
            NewResourceCommand.UpdateContext(ActiveEnvironment);
            SettingsCommand.UpdateContext(ActiveEnvironment);
            SchedulerCommand.UpdateContext(ActiveEnvironment);
        }

        #region Commands

        public AuthorizeCommand EditCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.EditCommand;
            }
        }

        public AuthorizeCommand SaveCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.SaveCommand;
            }
        }

        public AuthorizeCommand DebugCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.DebugCommand;
            }
        }

        public AuthorizeCommand QuickDebugCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.QuickDebugCommand;
            }
        }

        public AuthorizeCommand QuickViewInBrowserCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.QuickViewInBrowserCommand;
            }
        }
        public AuthorizeCommand ViewInBrowserCommand
        {
            get
            {
                if(ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.ViewInBrowserCommand;
            }
        }

        public ICommand AddLanguageHelpPageCommand
        {
            get
            {
                return _addLanguageHelpPageCommand ??
                       (_addLanguageHelpPageCommand = new DelegateCommand(param => AddLanguageHelpWorkSurface()));
            }
        }

        public ICommand DisplayAboutDialogueCommand
        {
            get
            {
                return _displayAboutDialogueCommand ??
                       (_displayAboutDialogueCommand = new DelegateCommand(param => DisplayAboutDialogue()));
            }
        }

        public ICommand ShowStartPageCommand
        {
            get
            {
                return _showStartPageCommand ?? (_showStartPageCommand = new DelegateCommand(param => ShowStartPage()));
            }
        }

        public ICommand ShowCommunityPageCommand
        {
            get { return _showCommunityPageCommand ?? (_showCommunityPageCommand = new DelegateCommand(param => ShowCommunityPage())); }
        }

        public ICommand StartFeedbackCommand
        {
            get { return _startFeedbackCommand ?? (_startFeedbackCommand = new DelegateCommand(param => StartFeedback())); }
        }

        public ICommand StartStopRecordedFeedbackCommand
        {
            get
            {
                return _startStopRecordedFeedbackCommand ??
                       (_startStopRecordedFeedbackCommand = new DelegateCommand(param => StartStopRecordedFeedback()));
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

        public AuthorizeCommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => AddSettingsWorkSurface(), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand SchedulerCommand
        {
            get
            {
                return _schedulerCommand ?? (_schedulerCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => AddSchedulerWorkSurface(), param => IsActiveEnvironmentConnected()));
            }
        }


        public AuthorizeCommand<string> NewResourceCommand
        {
            get
            {
                return _newResourceCommand ?? (_newResourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, ShowNewResourceWizard, param => IsActiveEnvironmentConnected()));
            }
        }

        [ExcludeFromCodeCoverage]
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

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public ILatestGetter LatestGetter { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        public IVersionChecker Version { get; private set; }
        public IConnectControlSingleton ConnectControlSingl { get; set; }

        public bool HasActiveConnection
        {
            get
            {
                return _hasActiveConnection;
            }
            set
            {
                _hasActiveConnection = value;
                NotifyOfPropertyChange(() => HasActiveConnection);
            }
        }

        #region ctor

        public MainViewModel()
            : this(EventPublishers.Aggregator, new AsyncWorker(), Core.EnvironmentRepository.Instance, new VersionChecker())
        {
        }

        public MainViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository,
            IVersionChecker versionChecker, bool createDesigners = true, IBrowserPopupController browserPopupController = null,
            IPopupController popupController = null, IWindowManager windowManager = null, IWebController webController = null, IFeedbackInvoker feedbackInvoker = null, IStudioResourceRepository studioResourceRepository = null, IConnectControlSingleton connectControlSingleton = null, IConnectControlViewModel connectControlViewModel = null)
            : base(eventPublisher)
        {
            if(environmentRepository == null)
            {
                throw new ArgumentNullException("environmentRepository");
            }

            if(versionChecker == null)
            {
                throw new ArgumentNullException("versionChecker");
            }
            Version = versionChecker;
            ConnectControlSingl = connectControlSingleton ?? ConnectControlSingleton.Instance;

            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _createDesigners = createDesigners;
            BrowserPopupController = browserPopupController ?? new ExternalBrowserPopupController();
            StudioResourceRepository = studioResourceRepository ?? Dev2.AppResources.Repositories.StudioResourceRepository.Instance;
            PopupProvider = popupController ?? new PopupController();
            WindowManager = windowManager ?? new WindowManager();
            WebController = webController ?? new WebController();
            FeedbackInvoker = feedbackInvoker ?? new FeedbackInvoker();
            EnvironmentRepository = environmentRepository;
            FlowController = new FlowController(PopupProvider);

            if(ExplorerViewModel == null)
            {
                ExplorerViewModel = new ExplorerViewModel(eventPublisher, asyncWorker, environmentRepository, StudioResourceRepository, ConnectControlSingl, this, false, enDsfActivityType.All, AddWorkspaceItems, connectControlViewModel);
            }

            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            AddWorkspaceItems();
            ShowStartPage();
            DisplayName = "Warewolf" + string.Format(" ({0})", ClaimsPrincipal.Current.Identity.Name).ToUpperInvariant();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

        }

        public IStudioResourceRepository StudioResourceRepository { get; set; }

        #endregion ctor

        #region IHandle

        public void Handle(ShowReverseDependencyVisualizer message)
        {
            Dev2Logger.Log.Debug(message.GetType().Name);
            if(message.Model != null)
            {
                AddReverseDependencyVisualizerWorkSurface(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            Dev2Logger.Log.Debug(message.GetType().Name);
            PersistTabs();
        }


        public void Handle(AddWorkSurfaceMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            AddWorkSurface(message.WorkSurfaceObject);

            if(message.ShowDebugWindowOnLoad)
            {
                if(ActiveItem != null && _canDebug)
                {
                    ActiveItem.DebugCommand.Execute(null);
                }
            }
        }

        public void Handle(DeleteResourcesMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            DeleteResources(message.ResourceModels, message.FolderName, message.ShowDialog, message.ActionToDoOnDelete);
        }

        public void Handle(DeleteFolderMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            var result = PopupProvider;
            if(ShowDeleteDialogForFolder(message.FolderName, result))
            {
                var actionToDoOnDelete = message.ActionToDoOnDelete;
                if(actionToDoOnDelete != null)
                {
                    actionToDoOnDelete();
                }
            }
        }

        public void Handle(SetActiveEnvironmentMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            var activeEnvironment = message.EnvironmentModel;
            SetActiveEnvironment(activeEnvironment);
            ExplorerViewModel.UpdateActiveEnvironment(ActiveEnvironment, message.SetFromConnectControl);
        }

        public void SetActiveEnvironment(IEnvironmentModel activeEnvironment)
        {
            ActiveEnvironment = activeEnvironment;
            EnvironmentRepository.ActiveEnvironment = ActiveEnvironment;
            ActiveEnvironment.AuthorizationServiceSet += (sender, args) => OnActiveEnvironmentChanged();
        }

        public void Handle(ShowDependenciesMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            var model = message.ResourceModel;
            if(model == null)
            {
                return;
            }

            if(message.ShowDependentOnMe)
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
            Dev2Logger.Log.Debug(message.GetType().Name);
            ShowEditResourceWizard(message.ResourceModel);
        }

        public void Handle(ShowHelpTabMessage message)
        {
            Dev2Logger.Log.Debug(message.GetType().Name);
            AddHelpTabWorkSurface(message.HelpLink);
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            Dev2Logger.Log.Debug(message.GetType().Name);
            if(message.ResourceToRemove == null)
            {
                return;
            }

            var wfscvm = FindWorkSurfaceContextViewModel(message.ResourceToRemove);
            if(message.RemoveFromWorkspace)
            {
                DeactivateItem(wfscvm, true);
            }
            else
            {
                base.DeactivateItem(wfscvm, true);
            }

            _previousActive = null;

        }

        public void Handle(DeployResourcesMessage message)
        {
            Dev2Logger.Log.Debug(message.GetType().Name);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);

            var exist = ActivateWorkSurfaceIfPresent(key);
            if(message.ViewModel != null)
            {
                var environmentModel = EnvironmentRepository.FindSingle(model => model.ID == message.ViewModel.EnvironmentId);
                if(environmentModel != null)
                {
                    var resourceModel = environmentModel.ResourceRepository.FindSingle(model => model.ID == message.ViewModel.ResourceId);
                    if(resourceModel != null)
                    {
                        DeployResource = resourceModel as IContextualResourceModel;
                    }
                }
                if(!exist)
                {
                    AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateDeployViewModel(message.ViewModel));
                }
                else
                {
                    Dev2Logger.Log.Info("Publish message of type - " + typeof(SelectItemInDeployMessage));
                    EventPublisher.Publish(new SelectItemInDeployMessage(message.ViewModel.ResourceId, message.ViewModel.EnvironmentId));
                }
            }
        }

        public IContextualResourceModel DeployResource { get; set; }

        public void Handle(ShowNewResourceWizard message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            ShowNewResourceWizard(message.ResourceType, message.ResourcePath);
        }

        public void RefreshActiveEnvironment()
        {
            if(ActiveItem != null && ActiveItem.Environment != null)
            {
                Dev2Logger.Log.Debug("Publish message of type - " + typeof(SetActiveEnvironmentMessage));
                EventPublisher.Publish(new SetActiveEnvironmentMessage(ActiveItem.Environment));
            }
        }

        #endregion

        #region Private Methods

        private void TempSave(IEnvironmentModel activeEnvironment, string resourceType, string resourcePath)
        {
            string newWorflowName = NewWorkflowNames.Instance.GetNext();

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(activeEnvironment, resourceType,
                                                                                              resourceType);
            tempResource.Category = string.IsNullOrEmpty(resourcePath) ? "Unassigned\\" + newWorflowName : resourcePath + "\\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;
            StudioResourceRepository.AddResouceItem(tempResource);

            AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateResourceViewModel(tempResource));
            AddWorkspaceItem(tempResource);
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

        // Write CodedUI Test Because of Silly Chicken affect ;)
        private bool ShowRemovePopup(IWorkflowDesignerViewModel workflowVm)
        {
            var result = PopupProvider.Show(string.Format(StringResources.DialogBody_NotSaved, workflowVm.ResourceModel.ResourceName), StringResources.DialogTitle_NotSaved,
                                            MessageBoxButton.YesNoCancel, MessageBoxImage.Question, null);

            switch(result)
            {
                case MessageBoxResult.Yes:
                    workflowVm.ResourceModel.Commit();
                    Dev2Logger.Log.Info("Publish message of type - " + typeof(SaveResourceMessage));
                    EventPublisher.Publish(new SaveResourceMessage(workflowVm.ResourceModel, false, false));
                    return true;
                case MessageBoxResult.No:
                    // We need to remove it ;)
                    var model = workflowVm.ResourceModel;
                    try
                    {
                        if(workflowVm.EnvironmentModel.ResourceRepository.DoesResourceExistInRepo(model) && workflowVm.ResourceModel.IsNewWorkflow)
                        {
                            DeleteResources(new List<IContextualResourceModel> { model }, "", false);
                        }
                        else
                        {
                            model.Rollback();
                        }
                    }
                    catch(Exception e)
                    {
                        Dev2Logger.Log.Info("Some clever chicken threw this exception : " + e.Message);
                    }

                    NewWorkflowNames.Instance.Remove(workflowVm.ResourceModel.ResourceName);
                    return true;
                default:
                    return false;
            }
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

        private void DisplayResourceWizard(IContextualResourceModel resourceModel, bool isedit)
        {
            if(resourceModel == null)
            {
                return;
            }

            if(isedit && resourceModel.ServerResourceType == ResourceType.WorkflowService.ToString())
            {
                PersistTabs();
            }

            // we need to load it so we can extract the sourceID ;)
            if(resourceModel.WorkflowXaml == null)
            {
                resourceModel.Environment.ResourceRepository.ReloadResource(resourceModel.ID, resourceModel.ResourceType, ResourceModelEqualityComparer.Current, true);
            }

            WebController.DisplayDialogue(resourceModel, isedit);
        }

        private void ShowNewResourceWizard(string resourceType)
        {
            ShowNewResourceWizard(resourceType, "");
        }

        private void ShowNewResourceWizard(string resourceType, string resourcePath)
        {
            if(resourceType == "Workflow")
            {
                TempSave(ActiveEnvironment, resourceType, resourcePath);
                if(View != null)
                {
                    View.ClearToolboxSearch();
                }
            }
            else
            {
                var resourceModel = ResourceModelFactory.CreateResourceModel(ActiveEnvironment, resourceType);
                resourceModel.Category = string.IsNullOrEmpty(resourcePath) ? null : resourcePath;
                resourceModel.ID = Guid.Empty;
                DisplayResourceWizard(resourceModel, false);
            }
        }

        private void ShowEditResourceWizard(object resourceModelToEdit)
        {
            var resourceModel = resourceModelToEdit as IContextualResourceModel;
            DisplayResourceWizard(resourceModel, true);
        }


        #endregion Private Methods

        #region Public Methods

        public virtual void ShowStartPage()
        {
            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage);
            WorkSurfaceContextViewModel workSurfaceContextViewModel = Items.FirstOrDefault(c => c.WorkSurfaceViewModel.DisplayName == "Start Page" && c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            if(workSurfaceContextViewModel != null)
            {
                ((HelpViewModel)workSurfaceContextViewModel.WorkSurfaceViewModel).LoadBrowserUri(Version.CommunityPageUri);
            }
        }

        public void ShowCommunityPage()
        {
            BrowserPopupController.ShowPopup(StringResources.Uri_Community_HomePage);
        }

        #region Overrides of ViewAware

        protected override void OnViewAttached(object view, object context)
        {
            if(View == null)
            {
                View = view as MainView;
            }
        }

        protected MainView View { get; set; }

        public void ClearToolboxSelection()
        {
            if(View != null)
            {
                View.ClearToolboxSelection();
            }
        }

        #endregion

        public bool IsActiveEnvironmentConnected()
        {
            if(ActiveEnvironment == null)
            {
                return false;
            }

            HasActiveConnection = ActiveItem != null && ActiveItem.IsEnvironmentConnected();
            return ((ActiveEnvironment != null) && (ActiveEnvironment.IsConnected) && (ActiveEnvironment.CanStudioExecute));
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

        [ExcludeFromCodeCoverage] //Excluded due to needing a parent window
        public void AddSettingsWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<SettingsViewModel>(WorkSurfaceContext.Settings);
        }

        [ExcludeFromCodeCoverage] //Excluded due to needing a parent window
        public void AddSchedulerWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<SchedulerViewModel>(WorkSurfaceContext.Scheduler);
        }

        public void AddHelpTabWorkSurface(string uriToDisplay)
        {
            if(!string.IsNullOrWhiteSpace(uriToDisplay))
                ActivateOrCreateUniqueWorkSurface<HelpViewModel>
                    (WorkSurfaceContext.Help,
                     new[] { new Tuple<string, object>("Uri", uriToDisplay) });
            WorkSurfaceContextViewModel workSurfaceContextViewModel = Items.FirstOrDefault(c => c.WorkSurfaceViewModel.DisplayName == "Help" && c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            if(workSurfaceContextViewModel != null)
            {
                ((HelpViewModel)workSurfaceContextViewModel.WorkSurfaceViewModel).LoadBrowserUri(uriToDisplay);
            }
        }

        public void AddLanguageHelpWorkSurface()
        {
            var path = FileHelper.GetFullPath(StringResources.Uri_Studio_Language_Reference_Document);
            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.LanguageHelp
                                                             , new[] { new Tuple<string, object>("Uri", path) });
            WorkSurfaceContextViewModel workSurfaceContextViewModel = Items.FirstOrDefault(c => c.WorkSurfaceViewModel.DisplayName == "Language Help" && c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            if(workSurfaceContextViewModel != null)
            {
                ((HelpViewModel)workSurfaceContextViewModel.WorkSurfaceViewModel).LoadBrowserUri(path);
            }
        }

        public void StartFeedback()
        {
            FeedbackInvoker.InvokeFeedback(new EmailFeedbackAction(new Dictionary<string, string>(), ActiveEnvironment), new RecorderFeedbackAction());
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

        #region Overrides of ConductorBaseWithActiveItem<WorkSurfaceContextViewModel>

        #endregion

        #region Overrides of ConductorBaseWithActiveItem<WorkSurfaceContextViewModel>

        protected override void ChangeActiveItem(WorkSurfaceContextViewModel newItem, bool closePrevious)
        {
            if(_previousActive != null)
            {
                if(newItem != null)
                {
                    if(newItem.DataListViewModel != null)
                    {
                        string errors;
                        newItem.DataListViewModel.ClearCollections();
                        newItem.DataListViewModel.CreateListsOfIDataListItemModelToBindTo(out errors);
                    }
                }

            }
            base.ChangeActiveItem(newItem, closePrevious);
            RefreshActiveEnvironment();
        }

        #endregion

        public override void DeactivateItem(WorkSurfaceContextViewModel item, bool close)
        {
            if(item == null)
            {
                return;
            }

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
                item.Dispose();
                CloseCurrent = true;
            }
            else
            {
                CloseCurrent = false;
            }
        }


        // Process saving tabs and such when exiting ;)
        protected override void OnDeactivate(bool close)
        {
            if(close)
            {
                PersistTabs();
            }

            base.OnDeactivate(close);
        }

        protected override void OnActivationProcessed(WorkSurfaceContextViewModel item, bool success)
        {
            if(success)
            {
                if(item != null)
                {
                    var wfItem = item.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
                    if(wfItem != null)
                    {
                        AddWorkspaceItem(wfItem.ResourceModel);
                    }
                }
                NotifyOfPropertyChange(() => EditCommand);
                NotifyOfPropertyChange(() => SaveCommand);
                NotifyOfPropertyChange(() => DebugCommand);
                NotifyOfPropertyChange(() => QuickDebugCommand);
                NotifyOfPropertyChange(() => QuickViewInBrowserCommand);
                NotifyOfPropertyChange(() => ViewInBrowserCommand);
            }
            base.OnActivationProcessed(item, success);
        }

        public override void ActivateItem(WorkSurfaceContextViewModel item)
        {
            _previousActive = ActiveItem;
            base.ActivateItem(item);
            if(item == null || item.ContextualResourceModel == null) return;

            if(ExplorerViewModel != null)
            {
                ExplorerViewModel.BringItemIntoView(item);
            }
        }


        #endregion
        #region Resource Deletion

        private bool ConfirmDeleteAfterDependencies(ICollection<IContextualResourceModel> models)
        {
            if(!models.Any(model => model.Environment.ResourceRepository.HasDependencies(model)))
            {
                return true;
            }


            if(models.Count > 1)
            {
                new DeleteFolderDialog().ShowDialog();
                return false;
            }
            if(models.Count == 1)
            {
                var model = models.FirstOrDefault();
                var dialog = new DeleteResourceDialog(model);
                dialog.ShowDialog();
                if(dialog.OpenDependencyGraph)
                {
                    AddReverseDependencyVisualizerWorkSurface(model);
                }
                return false;
            }
            return true;
        }

        private bool ConfirmDelete(ICollection<IContextualResourceModel> models, string folderName)
        {
            bool confirmDeleteAfterDependencies = ConfirmDeleteAfterDependencies(models);
            if(confirmDeleteAfterDependencies)
            {
                IPopupController result = new PopupController();
                if(models.Count > 1)
                {
                    var contextualResourceModel = models.FirstOrDefault();
                    if(contextualResourceModel != null)
                    {
                        var folderBeingDeleted = folderName;
                        return ShowDeleteDialogForFolder(folderBeingDeleted, result);
                    }
                }
                if(models.Count == 1)
                {
                    var contextualResourceModel = models.FirstOrDefault();
                    if(contextualResourceModel != null)
                    {
                        var deletionName = folderName;
                        var description = "";
                        if(string.IsNullOrEmpty(deletionName))
                        {
                            deletionName = contextualResourceModel.ResourceName;
                            description = contextualResourceModel.ResourceType.GetDescription();
                        }
                        var deletePrompt = String.Format(StringResources.DialogBody_ConfirmDelete, deletionName,
                            description);
                        var deleteAnswer = PopupProvider.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete,
                            MessageBoxButton.YesNo, MessageBoxImage.Warning, null);

                        var shouldDelete = deleteAnswer == MessageBoxResult.Yes;
                        return shouldDelete;
                    }
                }
            }
            return false;
        }

        static bool ShowDeleteDialogForFolder(string folderBeingDeleted, IPopupController result)
        {
            var deletePrompt = String.Format(StringResources.DialogBody_ConfirmFolderDelete, folderBeingDeleted);
            var deleteAnswer = result.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning, null);
            var confirmDelete = deleteAnswer == MessageBoxResult.Yes;
            return confirmDelete;
        }

        private void DeleteResources(ICollection<IContextualResourceModel> models, string folderName, bool showConfirm = true, System.Action actionToDoOnDelete = null)
        {
            if(models == null || (showConfirm && !ConfirmDelete(models, folderName)))
            {
                return;
            }

            foreach(var contextualModel in models)
            {
                if(contextualModel == null)
                {
                    continue;
                }

                DeleteContext(contextualModel);

                if(contextualModel.Environment.ResourceRepository.DeleteResource(contextualModel).HasError)
                {
                    return;
                }
                //If its deleted from loalhost, and is a server, also delete from repository
                if(contextualModel.Environment.IsLocalHost)
                {
                    if(contextualModel.ResourceType == ResourceType.Source)
                    {
                        if(contextualModel.ServerResourceType == "Server")
                        {
                            var environment = EnvironmentRepository.Get(contextualModel.ID);

                            if(environment != null)
                            {
                                Dev2Logger.Log.Debug("Publish message of type - " + typeof(EnvironmentDeletedMessage));
                                EventPublisher.Publish(new EnvironmentDeletedMessage(environment));
                                EnvironmentRepository.Remove(environment);
                            }
                        }
                    }
                }
                if(actionToDoOnDelete != null)
                {
                    actionToDoOnDelete();
                }
            }
            ExplorerViewModel.NavigationViewModel.UpdateSearchFilter();
        }

        #endregion delete

        #region WorkspaceItems management

        private void SaveWorkspaceItems()
        {
            GetWorkspaceItemRepository().Write();
        }

        private void AddWorkspaceItem(IContextualResourceModel model)
        {
            GetWorkspaceItemRepository().AddWorkspaceItem(model);
        }

        public Func<IWorkspaceItemRepository> GetWorkspaceItemRepository = () => WorkspaceItemRepository.Instance;

        private void RemoveWorkspaceItem(IDesignerViewModel viewModel)
        {
            GetWorkspaceItemRepository().Remove(viewModel.ResourceModel);
        }

        public virtual void AddWorkspaceItems()
        {
            if(EnvironmentRepository == null) return;

            HashSet<IWorkspaceItem> workspaceItemsToRemove = new HashSet<IWorkspaceItem>();
            // ReSharper disable ForCanBeConvertedToForeach
            for(int i = 0; i < GetWorkspaceItemRepository().WorkspaceItems.Count; i++)
            // ReSharper restore ForCanBeConvertedToForeach
            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = GetWorkspaceItemRepository().WorkspaceItems[i];
                Dev2Logger.Log.Info(string.Format("Start Proccessing WorkspaceItem: {0}", item.ServiceName));
                IEnvironmentModel environment = EnvironmentRepository.All().Where(env => env.IsConnected).TakeWhile(env => env.Connection != null).FirstOrDefault(env => env.ID == item.EnvironmentID);

                if(environment == null || environment.ResourceRepository == null)
                {
                    Dev2Logger.Log.Info("Environment Not Found");
                    if(environment != null && item.EnvironmentID == environment.ID)
                    {
                        workspaceItemsToRemove.Add(item);
                    }
                }
                if(environment != null)
                {
                    Dev2Logger.Log.Info(string.Format("Proccessing WorkspaceItem: {0} for Environment: {1}", item.ServiceName, environment.DisplayName));
                    if(environment.ResourceRepository != null)
                    {
                        environment.ResourceRepository.LoadResourceFromWorkspace(item.ID, item.WorkspaceID);
                        var resource = environment.ResourceRepository.All().FirstOrDefault(rm =>
                        {
                            var sameEnv = true;
                            if(item.EnvironmentID != Guid.Empty)
                            {
                                sameEnv = item.EnvironmentID == environment.ID;
                            }
                            return rm.ID == item.ID && sameEnv;
                        }) as IContextualResourceModel;

                        if(resource == null)
                        {
                            workspaceItemsToRemove.Add(item);
                        }
                        else
                        {
                            Dev2Logger.Log.Info(string.Format("Got Resource Model: {0} ", resource.DisplayName));
                            var fetchResourceDefinition = environment.ResourceRepository.FetchResourceDefinition(environment, item.WorkspaceID, resource.ID);
                            resource.WorkflowXaml = fetchResourceDefinition.Message;
                            resource.IsWorkflowSaved = item.IsWorkflowSaved;
                            resource.OnResourceSaved += model => GetWorkspaceItemRepository().UpdateWorkspaceItemIsWorkflowSaved(model);
                            AddWorkSurfaceContextFromWorkspace(resource);
                        }
                    }
                }
                else
                {
                    workspaceItemsToRemove.Add(item);
                }
            }

            foreach(IWorkspaceItem workspaceItem in workspaceItemsToRemove)
            {
                GetWorkspaceItemRepository().WorkspaceItems.Remove(workspaceItem);
            }
        }

        #endregion

        #region Tab Management

        public void AddDeployResourcesWorkSurface(object input)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);
            bool exist = ActivateWorkSurfaceIfPresent(key);
            DeployResource = input as IContextualResourceModel;
            if(exist)
            {
                if(input is IContextualResourceModel)
                {
                    Dev2Logger.Log.Info("Publish message of type - " + typeof(SelectItemInDeployMessage));
                    EventPublisher.Publish(
                        new SelectItemInDeployMessage((input as IContextualResourceModel).ID,
                            (input as IContextualResourceModel).Environment.ID));
                }
            }
            else
            {
                WorkSurfaceContextViewModel context = WorkSurfaceContextFactory.CreateDeployViewModel(input);
                Items.Add(context);
                ActivateItem(context);
                Tracker.TrackEvent(TrackerEventGroup.Deploy, TrackerEventName.Opened);
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
            base.DeactivateItem(context, true);
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
            {
                if(typeof(T) == typeof(SettingsViewModel))
                {
                    Tracker.TrackEvent(TrackerEventGroup.Settings, TrackerEventName.Opened);
                }

                CreateAndActivateUniqueWorkSurface<T>(context, initParms);
            }
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

        public bool IsWorkFlowOpened(IContextualResourceModel resource)
        {
            return FindWorkSurfaceContextViewModel(resource) != null;
        }
        public void UpdateWorkflowLink(IContextualResourceModel resource,string newPath,string oldPath)
        {
            var x = (FindWorkSurfaceContextViewModel(resource));
            if(x != null)
            {
             
            
            var path = oldPath.Replace('\\', '/');
            var b = x.WorkSurfaceViewModel as WorkflowDesignerViewModel;
            if( b!= null )
            {
                b.UpdateWorkflowLink(b.DisplayWorkflowLink.Replace(path, newPath.Replace('\\', '/')));   
            }
            }
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

        public void AddWorkSurfaceContextFromWorkspace(IContextualResourceModel resourceModel)
        {
            AddWorkSurfaceContextImpl(resourceModel, true);
        }

        public void AddWorkSurfaceContext(IContextualResourceModel resourceModel)
        {
            AddWorkSurfaceContextImpl(resourceModel, false);
        }

        private void AddWorkSurfaceContextImpl(IContextualResourceModel resourceModel, bool isLoadingWorkspace)
        {
            if(resourceModel == null)
            {
                return;
            }

            //Activates if exists
            var exists = IsInOpeningState(resourceModel) || ActivateWorkSurfaceIfPresent(resourceModel);

            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel);

            if(exists)
            {
                return;
            }

            _canDebug = false;

            if(!isLoadingWorkspace)
            {
                OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            }

            //This is done for when the app starts up because the item isnt open but it must load it from the server or the user will lose all thier changes
            IWorkspaceItem workspaceItem = GetWorkspaceItemRepository().WorkspaceItems.FirstOrDefault(c => c.ID == resourceModel.ID);
            if(workspaceItem == null)
            {
                resourceModel.Environment.ResourceRepository.ReloadResource(resourceModel.ID, resourceModel.ResourceType, ResourceModelEqualityComparer.Current, true);
            }

            // NOTE: only if from server ;)
            if(!isLoadingWorkspace)
            {
                resourceModel.IsWorkflowSaved = true;
            }

            AddWorkspaceItem(resourceModel);
            AddAndActivateWorkSurface(GetWorkSurfaceContextViewModel(resourceModel, _createDesigners) as WorkSurfaceContextViewModel);

            OpeningWorkflowsHelper.RemoveWorkflow(workSurfaceKey);
            _canDebug = true;
        }

        public Func<IContextualResourceModel, bool, IWorkSurfaceContextViewModel> GetWorkSurfaceContextViewModel = (resourceModel, createDesigner) =>
            {
                // ReSharper disable ConvertToLambdaExpression
                return WorkSurfaceContextFactory.CreateResourceViewModel(resourceModel, createDesigner);
                // ReSharper restore ConvertToLambdaExpression
            };

        private bool IsInOpeningState(IContextualResourceModel resource)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(resource);
            return OpeningWorkflowsHelper.FetchOpeningKeys().Any(c => WorkSurfaceKeyEqualityComparer.Current.Equals(key, c));
        }

        private void AddAndActivateWorkSurface(WorkSurfaceContextViewModel context)
        {
            if(context != null)
            {
                Items.Add(context);
                ActivateItem(context);
            }
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

        /// <summary>
        ///     Saves all open tabs locally and writes the open tabs the to collection of workspace items
        /// </summary>
        public bool PersistTabs(bool isStudioShutdown = false)
        {
            SaveWorkspaceItems();
            var savingCompleted = false;
            for(var index = 0; index < Items.Count; index++)
            {
                var ctx = Items[index];
                if(ctx.IsEnvironmentConnected())
                {
                    ctx.Save(true, isStudioShutdown);
                }
                if(index == Items.Count - 1)
                {
                    savingCompleted = true;
                }
            }
            return savingCompleted;
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
                        var workflowVm = vm as IWorkflowDesignerViewModel;
                        if(workflowVm != null)
                        {
                            IContextualResourceModel resource = workflowVm.ResourceModel;
                            if(resource != null)
                            {
                                remove = !resource.IsAuthorized(AuthorizationContext.Contribute) || resource.IsWorkflowSaved;

                                if(!remove)
                                {
                                    remove = ShowRemovePopup(workflowVm);
                                }

                                if(remove)
                                {
                                    if(resource.IsNewWorkflow)
                                    {
                                        NewWorkflowNames.Instance.Remove(resource.ResourceName);
                                    }
                                    RemoveWorkspaceItem(workflowVm);
                                    Items.Remove(context);
                                    workflowVm.Dispose();
                                    if (_previousActive != null && _previousActive.WorkSurfaceViewModel == vm)
                                        _previousActive = null;
                                    Dev2Logger.Log.Info("Publish message of type - " + typeof(TabClosedMessage));
                                    EventPublisher.Publish(new TabClosedMessage(context));
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
                    else if(vm != null && vm.WorkSurfaceContext == WorkSurfaceContext.Settings)
                    {
                        var settingsViewModel = vm as SettingsViewModel;
                        if(settingsViewModel != null)
                        {
                            remove = settingsViewModel.DoDeactivate();
                            if(remove)
                            {
                                settingsViewModel.Dispose();
                            }
                        }
                    }
                    else if(vm != null && vm.WorkSurfaceContext == WorkSurfaceContext.Scheduler)
                    {
                        var schedulerViewModel = vm as SchedulerViewModel;
                        if(schedulerViewModel != null)
                        {
                            remove = schedulerViewModel.DoDeactivate();
                            if(remove)
                            {
                                schedulerViewModel.Dispose();
                            }
                        }
                    }
                }
            }

            return remove;
        }

        public bool OnStudioClosing()
        {
            List<WorkSurfaceContextViewModel> workSurfaceContextViewModels = Items.ToList();
            foreach(WorkSurfaceContextViewModel workSurfaceContextViewModel in workSurfaceContextViewModels)
            {
                var vm = workSurfaceContextViewModel.WorkSurfaceViewModel;
                if(vm != null)
                {
                    if(vm.WorkSurfaceContext == WorkSurfaceContext.Settings)
                    {
                        var settingsViewModel = vm as SettingsViewModel;
                        if(settingsViewModel != null && settingsViewModel.IsDirty)
                        {
                            ActivateItem(workSurfaceContextViewModel);
                            bool remove = settingsViewModel.DoDeactivate();
                            if(!remove)
                            {
                                return false;
                            }
                        }
                    }
                    else if(vm.WorkSurfaceContext == WorkSurfaceContext.Scheduler)
                    {
                        var schedulerViewModel = vm as SchedulerViewModel;
                        if(schedulerViewModel != null && schedulerViewModel.SelectedTask != null && schedulerViewModel.SelectedTask.IsDirty)
                        {
                            ActivateItem(workSurfaceContextViewModel);
                            bool remove = schedulerViewModel.DoDeactivate();
                            if(!remove)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            CloseRemoteConnections();
            return true;
        }

        private void CloseRemoteConnections()
        {
            var connected = EnvironmentRepository.All().Where(a => a.IsConnected);
            foreach(var environmentModel in connected)
            {
                environmentModel.Disconnect();
            }
        }

        #endregion

        public void Handle(FileChooserMessage message)
        {
            RootWebSite.ShowFileChooser(ActiveEnvironment, message);
        }

        public Func<bool> IsBusyDownloadingInstaller;

        public bool IsDownloading()
        {
            return IsBusyDownloadingInstaller != null && IsBusyDownloadingInstaller();
        }

        #region Implementation of IHandle<DisplayMessageBoxMessage>

        public void Handle(DisplayMessageBoxMessage message)
        {
            PopupProvider.Show(message.Message, message.Heading, MessageBoxButton.OK, message.MessageBoxImage, "");
        }

        #endregion
    }
}
