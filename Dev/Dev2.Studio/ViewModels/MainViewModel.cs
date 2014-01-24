using Caliburn.Micro;
using Dev2.Common.ExtMethods;
using Dev2.Helpers;
using Dev2.Instrumentation;
using Dev2.Providers.Logs;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings;
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
using Dev2.Studio.Enums;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Studio.Webs;
using Dev2.Threading;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Windows;
using System.Windows.Input;
using UserInterfaceLayoutModel = Dev2.Studio.Core.Models.UserInterfaceLayoutModel;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels
{
    // PBI 9397 - 2013.06.09 - TWR: made class non-sealed to facilitate testing i.e. creating mock sub-classes
    /// <summary>
    /// 
    /// </summary>
    [Export(typeof(IMainViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainViewModel : BaseConductor<WorkSurfaceContextViewModel>, IMainViewModel,
                                        IHandle<DeleteResourcesMessage>,
                                        IHandle<ShowDependenciesMessage>,
                                        IHandle<AddWorkSurfaceMessage>,
                                        IHandle<SetActiveEnvironmentMessage>,
                                        IHandle<ShowEditResourceWizardMessage>,
                                        IHandle<DeployResourcesMessage>,
                                        IHandle<ShowHelpTabMessage>,
                                        IHandle<ShowNewResourceWizard>,
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

        private AuthorizeCommand<string> _newResourceCommand;
        private ICommand _addStudioShortcutsPageCommand;
        private ICommand _addLanguageHelpPageCommand;
        private ICommand _deployAllCommand;
        private ICommand _deployCommand;
        private ICommand _displayAboutDialogueCommand;
        private ICommand _exitCommand;
        private ICommand _resetLayoutCommand;
        private AuthorizeCommand _settingsCommand;
        private ICommand _startFeedbackCommand;
        private ICommand _showCommunityPageCommand;
        private ICommand _startStopRecordedFeedbackCommand;
        private ICommand _reportsCommand;
        private readonly bool _createDesigners;
        private ICommand _notImplementedCommand;
        private ICommand _showStartPageCommand;
        readonly IAsyncWorker _asyncWorker;
        bool _hasActiveConnection;
        readonly List<WorkSurfaceKey> _resourcesCurrentlyInOpeningState = new List<WorkSurfaceKey>();
        bool _canDebug = true;

        #endregion

        #region Properties

        #region imports
        [Import]
        public FlowController FlowController { get; set; }

        [Import(typeof(IWebController))]
        public IWebController WebController { get; set; }

        public IWindowManager WindowManager { get; set; }

        public IPopupController PopupProvider { get; set; }

        public IEnvironmentRepository EnvironmentRepository { get; private set; }

        [Import]
        public IFeedbackInvoker FeedbackInvoker { get; set; }

        [Import]
        public IFeedBackRecorder FeedBackRecorder { get; set; }

        [Import(typeof(IFrameworkRepository<UserInterfaceLayoutModel>))]
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

        // BUG 9798 - 2013.06.25 - TWR : added
        public IBrowserPopupController BrowserPopupController { get; private set; }

        #endregion

        void OnActiveEnvironmentChanged()
        {
            NewResourceCommand.UpdateContext(ActiveEnvironment);
            SettingsCommand.UpdateContext(ActiveEnvironment);
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

        public ICommand AddLanguageHelpPageCommand
        {
            get
            {
                return _addLanguageHelpPageCommand ??
                       (_addLanguageHelpPageCommand = new RelayCommand(param => AddLanguageHelpWorkSurface()));
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
                       {
                           this.TraceInfo("Publish message of type - " + typeof(ResetLayoutMessage));
                           EventPublisher.Publish(
                               new ResetLayoutMessage(param as FrameworkElement));
                       },
            param => true));
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

        public ICommand ReportsCommand
        {
            get
            {
                return _reportsCommand ?? (_reportsCommand =
                                            new RelayCommand(param => AddReportsWorkSurface()));
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
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public ILatestGetter LatestGetter { get; private set; }

        // PBI 9941 - 2013.07.07 - TWR: added
        public IVersionChecker Version { get; private set; }

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
            IPopupController popupController = null, IWindowManager windowManager = null, IWebController webController = null, IFeedbackInvoker feedbackInvoker = null)
            : base(eventPublisher)
        {
            if(environmentRepository == null)
            {
                throw new ArgumentNullException("environmentRepository");
            }

            // PBI 9941 - 2013.07.07 - TWR: added
            if(versionChecker == null)
            {
                throw new ArgumentNullException("versionChecker");
            }
            Version = versionChecker;

            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;

            _createDesigners = createDesigners;
            BrowserPopupController = browserPopupController ?? new ExternalBrowserPopupController(); // BUG 9798 - 2013.06.25 - TWR : added
            //ResourceDependencyService = resourceDependencyService ?? new ResourceDependencyService();
            PopupProvider = popupController ?? new PopupController();
            WindowManager = windowManager ?? new WindowManager();
            WebController = webController ?? new WebController();
            FeedbackInvoker = feedbackInvoker ?? new FeedbackInvoker();
            EnvironmentRepository = environmentRepository;

            if(ExplorerViewModel == null)
            {
                ExplorerViewModel = new ExplorerViewModel(eventPublisher, asyncWorker, environmentRepository, false, enDsfActivityType.All, AddWorkspaceItems);
                ExplorerViewModel.LoadEnvironments();
            }

            // PBI 9512 - 2013.06.07 - TWR : refactored to use common method
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            ShowStartPage();
        }

        #endregion ctor

        #region IHandle

        public void Handle(ShowReverseDependencyVisualizer message)
        {
            this.TraceInfo(message.GetType().Name);
            if(message.Model != null)
            {
                AddReverseDependencyVisualizerWorkSurface(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            PersistTabs();
        }

        public void Handle(GetActiveEnvironmentCallbackMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            message.Callback.Invoke(ActiveEnvironment);
        }

        public void Handle(GetContextualEnvironmentCallbackMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            message.Callback.Invoke(ActiveItem.Environment);
        }

        public void Handle(AddWorkSurfaceMessage message)
        {
            this.TraceInfo(message.GetType().Name);
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
            this.TraceInfo(message.GetType().Name);
            DeleteResources(message.ResourceModels, message.ShowDialog);
        }

        public void Handle(SetActiveEnvironmentMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            ActiveEnvironment = message.EnvironmentModel;
            EnvironmentRepository.ActiveEnvironment = ActiveEnvironment;
            this.TraceInfo("Publish message of type - " + typeof(UpdateActiveEnvironmentMessage));
            EventPublisher.Publish(new UpdateActiveEnvironmentMessage(ActiveEnvironment));
        }

        public void Handle(ShowDependenciesMessage message)
        {
            this.TraceInfo(message.GetType().Name);
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
            this.TraceInfo(message.GetType().Name);
            ShowEditResourceWizard(message.ResourceModel);
        }

        public void Handle(ShowHelpTabMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            AddHelpTabWorkSurface(message.HelpLink);
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            if(message.ResourceToRemove == null)
            {
                return;
            }

            var wfscvm = FindWorkSurfaceContextViewModel(message.ResourceToRemove);
            //DeactivateItem(wfscvm, true);
            base.DeactivateItem(wfscvm, true);
            GetWorkspaceItemRepository().Remove(message.ResourceToRemove);
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
            this.TraceInfo(message.GetType().Name);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);

            var exist = ActivateWorkSurfaceIfPresent(key);
            DeployResource = message.ViewModel;
            var abstractTreeViewModel = (message.ViewModel as AbstractTreeViewModel);
            if(exist)
            {
                this.TraceInfo("Publish message of type - " + typeof(SelectItemInDeployMessage));
                if(abstractTreeViewModel != null)
                {
                    EventPublisher.Publish(new SelectItemInDeployMessage(message.ViewModel.DisplayName, abstractTreeViewModel.EnvironmentModel));
                }
            }
            else
            {
                AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateDeployViewModel(message.ViewModel));
            }
            this.TraceInfo("Publish message of type - " + typeof(SelectItemInDeployMessage));
            if(abstractTreeViewModel != null)
            {
                EventPublisher.Publish(new SelectItemInDeployMessage(message.ViewModel.DisplayName, abstractTreeViewModel.EnvironmentModel));
            }
        }

        public SimpleBaseViewModel DeployResource { get; set; }

        public void Handle(ShowNewResourceWizard message)
        {
            this.TraceInfo(message.GetType().Name);
            ShowNewResourceWizard(message.ResourceType);
        }

        public void RefreshActiveEnvironment()
        {
            if(ActiveItem != null && ActiveItem.Environment != null)
            {
                this.TraceInfo("Publish message of type - " + typeof(SetActiveEnvironmentMessage));
                EventPublisher.Publish(new SetActiveEnvironmentMessage(ActiveItem.Environment));
            }
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
                                            MessageBoxButton.YesNoCancel);

            switch(result)
            {
                case MessageBoxResult.Yes:
                    workflowVm.ResourceModel.Commit();
                    this.TraceInfo("Publish message of type - " + typeof(SaveResourceMessage));
                    EventPublisher.Publish(new SaveResourceMessage(workflowVm.ResourceModel, false, false));
                    return true;
                case MessageBoxResult.No:
                    // We need to remove it ;)
                    var model = workflowVm.ResourceModel;
                    try
                    {
                        if(workflowVm.EnvironmentModel.ResourceRepository.DoesResourceExistInRepo(model) && workflowVm.ResourceModel.IsNewWorkflow)
                        {
                            DeleteResources(new List<IContextualResourceModel> { model }, false);
                        }
                        else
                        {
                            model.Rollback();
                        }
                    }
                    catch(Exception e)
                    {
                        this.TraceInfo("Some clever chicken threw this exception : " + e.Message);
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
            _asyncWorker.Start(() =>
            {
                var path = FileHelper.GetAppDataPath(StringResources.Uri_Studio_Homepage);

                // PBI 9512 - 2013.06.07 - TWR: added
                // PBI 9941 - 2013.07.07 - TWR: modified

                using(var getter = new LatestWebGetter())
                {
                    getter.GetLatest(Version.StartPageUri, path);
                }

            }, () =>
            {
                var path = FileHelper.GetAppDataPath(StringResources.Uri_Studio_Homepage);
                var oldPath = FileHelper.GetFullPath(StringResources.Uri_Studio_Homepage);

                // ensure the user sees a home page ;)
                var invokePath = path;
                if(File.Exists(oldPath) && !File.Exists(path))
                {
                    invokePath = oldPath;
                }
                ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage, new[] { new Tuple<string, object>("Uri", invokePath) });
            });
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

        public void AddSettingsWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<SettingsViewModel>(WorkSurfaceContext.Settings);
        }

        public void AddReportsWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<ReportsManagerViewModel>
                (WorkSurfaceContext.ReportsManager);
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

        public void AddLanguageHelpWorkSurface()
        {
            var path = FileHelper.GetFullPath(StringResources.Uri_Studio_Language_Reference_Document);
            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.LanguageHelp
                                                             , new[] { new Tuple<string, object>("Uri", path) });
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
                    //Not sure what this does
                    // item.Parent = this;
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

        #region ImportsSatisfied

        public void OnImportsSatisfied()
        {
            DisplayName = "Warewolf" + string.Format(" ({0})", ClaimsPrincipal.Current.Identity.Name).ToUpperInvariant();
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

        private bool ConfirmDelete(ICollection<IContextualResourceModel> models)
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
                        var deletePrompt = String.Format(StringResources.DialogBody_ConfirmFolderDelete, contextualResourceModel.Category);
                        var deleteAnswer = result.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        return (deleteAnswer == MessageBoxResult.Yes);
                    }
                }
                if(models.Count == 1)
                {
                    var contextualResourceModel = models.FirstOrDefault();
                    if(contextualResourceModel != null)
                    {
                        var deletePrompt = String.Format(StringResources.DialogBody_ConfirmDelete, contextualResourceModel.ResourceName,
                            contextualResourceModel.ResourceType.GetDescription());
                        var deleteAnswer = PopupProvider.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete,
                            MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        var shouldDelete = deleteAnswer == MessageBoxResult.Yes;
                        return shouldDelete;
                    }
                }
            }
            return false;
        }

        private void DeleteResources(ICollection<IContextualResourceModel> models, bool showConfirm = true)
        {
            if(models == null || (showConfirm && !ConfirmDelete(models)))
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
                this.TraceInfo("Publish message of type - " + typeof(RemoveNavigationResourceMessage));
                EventPublisher.Publish(new RemoveNavigationResourceMessage(contextualModel));

                if(contextualModel.Environment.ResourceRepository.DeleteResource(contextualModel).HasError)
                {
                    return;
                }


                //If its deleted from loalhost, and is a server, also delete from repository
                if(contextualModel.Environment.IsLocalHost())
                {
                    if(contextualModel.ResourceType == ResourceType.Source)
                    {
                        if(contextualModel.ServerResourceType == "Server")
                        {
                            var environment = EnvironmentRepository.Get(contextualModel.ID);

                            if(environment != null)
                            {
                                this.TraceInfo("Publish message of type - " + typeof(EnvironmentDeletedMessage));
                                EventPublisher.Publish(new EnvironmentDeletedMessage(environment));
                                EnvironmentRepository.Remove(environment);
                            }
                        }
                    }
                }
            }
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

        protected virtual void AddWorkspaceItems()
        {
            if(EnvironmentRepository == null) return;

            HashSet<IWorkspaceItem> workspaceItemsToRemove = new HashSet<IWorkspaceItem>();

            // ReSharper disable once ForCanBeConvertedToForeach
            for(int i = 0; i < GetWorkspaceItemRepository().WorkspaceItems.Count; i++)
            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = GetWorkspaceItemRepository().WorkspaceItems[i];
                IEnvironmentModel environment = null;
                foreach(var env in EnvironmentRepository.All())
                {
                    if(!env.IsConnected) continue;
                    if(env.Connection == null) break;
                    var channel = env.Connection;
                    if(channel.ServerID == item.ServerID)
                        environment = env;
                }

                if(environment == null || environment.ResourceRepository == null)
                {
                    if(environment != null && item.EnvironmentID == environment.ID)
                    {
                        workspaceItemsToRemove.Add(item);
                    }
                }

                if(environment != null)
                {
                    if(environment.ResourceRepository != null)
                    {
                        var resource = environment.ResourceRepository.All().FirstOrDefault(rm =>
                        {
                            var sameEnv = true;
                            if(item.EnvironmentID != Guid.Empty)
                            {
                                sameEnv = item.EnvironmentID == environment.ID;
                            }
                            return rm.ResourceName == item.ServiceName && sameEnv;
                        }) as IContextualResourceModel;

                        if(resource == null)
                        {
                            workspaceItemsToRemove.Add(item);
                            continue;
                        }


                        if(resource.ResourceType == ResourceType.WorkflowService)
                        {
                            resource.IsWorkflowSaved = item.IsWorkflowSaved;
                            resource.OnResourceSaved += model => GetWorkspaceItemRepository().UpdateWorkspaceItemIsWorkflowSaved(model);

                            // We need to load the correct version of the service ;)
                            var resourceDef = environment.ResourceRepository.FetchResourceDefinition(environment, environment.Connection.WorkspaceID, resource.ID);
                            resource.WorkflowXaml = resourceDef.Message;

                            AddWorkSurfaceContext(resource);
                        }
                        else
                        {
                            workspaceItemsToRemove.Add(item);
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
            DeployResource = input as SimpleBaseViewModel;
            if(exist)
            {
                if(input is IContextualResourceModel)
                {
                    this.TraceInfo("Publish message of type - " + typeof(SelectItemInDeployMessage));
                    EventPublisher.Publish(
                        new SelectItemInDeployMessage((input as IContextualResourceModel).DisplayName,
                            (input as IContextualResourceModel).Environment));
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
            var exists = IsInOpeningState(resourceModel) || ActivateWorkSurfaceIfPresent(resourceModel);

            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel);

            if(exists)
            {
                return;
            }

            _canDebug = false;
            _resourcesCurrentlyInOpeningState.Add(workSurfaceKey);

            //This is done for when the app starts up because the item isnt open but it must load it from the server or the user will lose all thier changes
            IWorkspaceItem workspaceItem = GetWorkspaceItemRepository().WorkspaceItems.FirstOrDefault(c => c.ID == resourceModel.ID);
            if(workspaceItem == null)
            {
                resourceModel.Environment.ResourceRepository.ReloadResource(resourceModel.ID, resourceModel.ResourceType, ResourceModelEqualityComparer.Current, true);
            }

            AddWorkspaceItem(resourceModel);
            AddAndActivateWorkSurface(GetWorkSurfaceContextViewModel(resourceModel, _createDesigners) as WorkSurfaceContextViewModel);

            _resourcesCurrentlyInOpeningState.Remove(workSurfaceKey);
            _canDebug = true;
        }

        public Func<IContextualResourceModel, bool, IWorkSurfaceContextViewModel> GetWorkSurfaceContextViewModel = (resourceModel, createDesigner) =>
            {
                return WorkSurfaceContextFactory.CreateResourceViewModel(resourceModel, createDesigner);
            };

        private bool IsInOpeningState(IContextualResourceModel resource)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(resource);
            return _resourcesCurrentlyInOpeningState.Any(c => WorkSurfaceKeyEqualityComparer.Current.Equals(key, c));
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
        public bool PersistTabs()
        {
            SaveWorkspaceItems();
            var savingCompleted = false;
            for(var index = 0; index < Items.Count; index++)
            {
                var ctx = Items[index];
                if(ctx.IsEnvironmentConnected())
                {
                    ctx.Save(true);
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
                                    this.TraceInfo("Publish message of type - " + typeof(TabClosedMessage));
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
                }
            }

            return remove;
        }

        #endregion
    }
}