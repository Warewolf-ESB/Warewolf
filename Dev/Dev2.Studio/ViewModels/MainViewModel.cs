#region

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Diagnostics;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
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
using Dev2.Studio.Core.InterfaceImplementors;
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
using Dev2.Studio.ViewModels.Configuration;
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
using UserInterfaceLayoutModel = Dev2.Studio.Core.Models.UserInterfaceLayoutModel;

#endregion

namespace Dev2.Studio.ViewModels
{
    // PBI 9397 - 2013.06.09 - TWR: made class non-sealed to facilitate testing i.e. creating mock sub-classes
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
        private ICommand _addLanguageHelpPageCommand;
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
        readonly IAsyncWorker _asyncWorker;
        bool _hasActiveConnection;

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

        public IResourceDependencyService ResourceDependencyService { get; set; }

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

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
                           Logger.TraceInfo("Publish message of type - " + typeof(ResetLayoutMessage), GetType().Name);
                           _eventPublisher.Publish(
                               new ResetLayoutMessage(param as FrameworkElement));
                       } ,
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
            IResourceDependencyService resourceDependencyService = null,IPopupController popupController = null
            ,IWindowManager windowManager = null,IWebController webController=null,IFeedbackInvoker feedbackInvoker=null)
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
            ResourceDependencyService = resourceDependencyService ?? new ResourceDependencyService();
            PopupProvider = popupController ?? new PopupController();
            WindowManager = windowManager ?? new WindowManager();
            WebController = webController ?? new WebController(PopupProvider);
            FeedbackInvoker = feedbackInvoker ?? new FeedbackInvoker();
            LatestGetter = new LatestWebGetter(); // PBI 9512 - 2013.06.07 - TWR: added

            EnvironmentRepository = environmentRepository;

            if(ExplorerViewModel == null)
            {
                ExplorerViewModel = new ExplorerViewModel(eventPublisher, asyncWorker, environmentRepository, false, enDsfActivityType.All, AddWorkspaceItems);
            }

            // PBI 9512 - 2013.06.07 - TWR : refactored to use common method
            ShowStartPage();
        }

        #endregion ctor

        #region IHandle

        public void Handle(ShowReverseDependencyVisualizer message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            if(message.Model != null)
            {
                AddReverseDependencyVisualizerWorkSurface(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            PersistTabs();
        }

        public void Handle(GetActiveEnvironmentCallbackMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            message.Callback.Invoke(ActiveEnvironment);
        }

        public void Handle(GetContextualEnvironmentCallbackMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            message.Callback.Invoke(ActiveItem.Environment);
        }

        public void Handle(AddWorkSurfaceMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            AddWorkSurface(message.WorkSurfaceObject);
        }

        public void Handle(DeleteResourcesMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            DeleteResources(message.ResourceModels, message.ShowDialog);
        }

        public void Handle(SetActiveEnvironmentMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            ActiveEnvironment = message.EnvironmentModel;
            Logger.TraceInfo("Publish message of type - " + typeof(UpdateActiveEnvironmentMessage), GetType().Name);
            _eventPublisher.Publish(new UpdateActiveEnvironmentMessage(ActiveEnvironment));
        }

        public void Handle(SettingsSaveCancelMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            RemoveSettingsWorkSurface(message.Environment);
        }

        public void Handle(ShowDependenciesMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            var model = message.ResourceModel as IContextualResourceModel;
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
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            ShowEditResourceWizard(message.ResourceModel);
        }

        public void Handle(ShowHelpTabMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            AddHelpTabWorkSurface(message.HelpLink);
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            if(message.ResourceToRemove == null)
            {
                return;
            }

            var wfscvm = FindWorkSurfaceContextViewModel(message.ResourceToRemove);
            //DeactivateItem(wfscvm, true);
            base.DeactivateItem(wfscvm, true);
            WorkspaceItemRepository.Instance.Remove(message.ResourceToRemove);
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
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);

            var exist = ActivateWorkSurfaceIfPresent(key);

            var abstractTreeViewModel = (message.ViewModel as AbstractTreeViewModel);
            if(exist)
            {
                Logger.TraceInfo("Publish message of type - " + typeof(SelectItemInDeployMessage), GetType().Name);
                _eventPublisher.Publish(new SelectItemInDeployMessage(message.ViewModel.DisplayName, abstractTreeViewModel.EnvironmentModel));
            }
            else
            {
                AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateDeployViewModel(message.ViewModel));
            }
            Logger.TraceInfo("Publish message of type - " + typeof(SelectItemInDeployMessage), GetType().Name);
            _eventPublisher.Publish(new SelectItemInDeployMessage(message.ViewModel.DisplayName, abstractTreeViewModel.EnvironmentModel));
        }

        public void Handle(ShowNewResourceWizard message)
        {
            Logger.TraceInfo(message.GetType().Name, GetType().Name);
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

        private bool ShowRemovePopup(IWorkflowDesignerViewModel workflowVM)
        {
            var result = PopupProvider.Show(string.Format(StringResources.DialogBody_NotSaved, workflowVM.ResourceModel.ResourceName), StringResources.DialogTitle_NotSaved,
                                            MessageBoxButton.YesNoCancel);

            switch(result)
            {
                case MessageBoxResult.Yes:
                    workflowVM.ResourceModel.Commit();
                    Logger.TraceInfo("Publish message of type - " + typeof(SaveResourceMessage), GetType().Name);
                    _eventPublisher.Publish(new SaveResourceMessage(workflowVM.ResourceModel, false, false));
                    return true;
                case MessageBoxResult.No:
                    // We need to remove it ;)
                    var model = workflowVM.ResourceModel;
                    try
                    {
                        if(workflowVM.EnvironmentModel.ResourceRepository.DoesResourceExistInRepo(model) &&
                            workflowVM.ResourceModel.IsNewWorkflow)
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

        public virtual void ShowStartPage()
        {
            _asyncWorker.Start(() =>
            {
            var path = FileHelper.GetAppDataPath(StringResources.Uri_Studio_Homepage);

            // PBI 9512 - 2013.06.07 - TWR: added
            // PBI 9941 - 2013.07.07 - TWR: modified
            LatestGetter.GetLatest(Version.StartPageUri, path);
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

            if(ActiveItem != null)
            {
                HasActiveConnection = ActiveItem.IsEnvironmentConnected();
            }
            else
            {
                HasActiveConnection = false;
            }
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
                if(_previousActive.DataListViewModel != null)
                {
                    _previousActive.DataListViewModel.ClearCollections();
                }
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
            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.Collect(2);
            base.ChangeActiveItem(newItem, closePrevious);
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
            GC.Collect(2);
            GC.WaitForFullGCComplete();
            GC.Collect(2);
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
                    item.Parent = this;
                    var wfItem = item.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
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
            DisplayName = "Warewolf";
        }

        #endregion

        #region Resource Deletion

        private bool ConfirmDeleteAfterDependencies(ICollection<IContextualResourceModel> models)
        {
            if(!models.Any(model => ResourceDependencyService.HasDependencies(model)))
                return true;

            if(models.Count > 1)
            {
                new DeleteFolderDialog().ShowDialog();
                return false;
            }
            if(models.Count == 1)
            {
                new DeleteResourceDialog(models.FirstOrDefault()).ShowDialog();
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
                    var deletePrompt = String.Format(StringResources.DialogBody_ConfirmFolderDelete, models.FirstOrDefault().Category);
                    var deleteAnswer = result.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return (deleteAnswer == MessageBoxResult.Yes);
                }
                if(models.Count == 1)
                {
                    var deletePrompt = String.Format(StringResources.DialogBody_ConfirmDelete, models.FirstOrDefault().ResourceName,
                        models.FirstOrDefault().ResourceType.GetDescription());
                    var deleteAnswer = PopupProvider.Show(deletePrompt, StringResources.DialogTitle_ConfirmDelete,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    var shouldDelete = deleteAnswer == MessageBoxResult.Yes;
                    return shouldDelete;
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
                if (contextualModel == null)
                {
                    continue;
                }

                DeleteContext(contextualModel);
                Logger.TraceInfo("Publish message of type - " + typeof(RemoveNavigationResourceMessage), GetType().Name);
                _eventPublisher.Publish(new RemoveNavigationResourceMessage(contextualModel));

                if(!contextualModel.Environment.ResourceRepository.DeleteResource(contextualModel).IsSuccessResponse())
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
                            var appserUri =
                                Core.EnvironmentRepository.GetAppServerUriFromConnectionString(
                                    contextualModel.ConnectionString);
                            var environment = EnvironmentRepository.Get(appserUri);

                            if(environment != null)
                            {
                                Logger.TraceInfo("Publish message of type - " + typeof(EnvironmentDeletedMessage), GetType().Name);
                                _eventPublisher.Publish(new EnvironmentDeletedMessage(environment));
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
            WorkspaceItemRepository.Instance.Write();
        }

        private void AddWorkspaceItem(IContextualResourceModel model)
        {
            WorkspaceItemRepository.Instance.AddWorkspaceItem(model);
        }

        private void RemoveWorkspaceItem(IDesignerViewModel viewModel)
        {
            WorkspaceItemRepository.Instance.Remove(viewModel.ResourceModel);
        }

        protected virtual void AddWorkspaceItems()
        {
            if(EnvironmentRepository == null) return;

            HashSet<IWorkspaceItem> workspaceItemsToRemove = new HashSet<IWorkspaceItem>();

            for(int i = 0; i < WorkspaceItemRepository.Instance.WorkspaceItems.Count; i++)
            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = WorkspaceItemRepository.Instance.WorkspaceItems[i];
                IEnvironmentModel environment = null;
                foreach(var env in EnvironmentRepository.All())
                {
                    if(!env.IsConnected) continue;
                    if(!(env.DsfChannel is IStudioClientContext)) break;
                    var channel = (IStudioClientContext)env.DsfChannel;
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
                })
                               as IContextualResourceModel;
                        if(resource == null)
                {
                    workspaceItemsToRemove.Add(item);
                    continue;
                }


                        if(resource.ResourceType == ResourceType.WorkflowService)
                {
                    resource.IsWorkflowSaved = item.IsWorkflowSaved;
                    resource.OnResourceSaved += model => WorkspaceItemRepository.Instance.UpdateWorkspaceItemIsWorkflowSaved(model);
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
                WorkspaceItemRepository.Instance.WorkspaceItems.Remove(workspaceItem);
            }
        }

        #endregion

        #region Tab Management

        public void AddDeployResourcesWorkSurface(object input)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);
            bool exist = ActivateWorkSurfaceIfPresent(key);

            if(exist)
            {
                if (input is IContextualResourceModel)
                {
                    Logger.TraceInfo("Publish message of type - " + typeof(SelectItemInDeployMessage), GetType().Name);
                    _eventPublisher.Publish(
                        new SelectItemInDeployMessage((input as IContextualResourceModel).DisplayName,
                            (input as IContextualResourceModel).Environment));
                }
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

            if (exists)
            {
                return;
            }

            //This is done for when the app starts up because the item isnt open but it must load it from the server or the user will lose all thier changes
            IWorkspaceItem workspaceItem = WorkspaceItemRepository.Instance.WorkspaceItems.FirstOrDefault(c => c.ID == resourceModel.ID);
            if(workspaceItem == null)
            {
                resourceModel.Environment.ResourceRepository.ReloadResource(resourceModel.ID, resourceModel.ResourceType, ResourceModelEqualityComparer.Current);
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
                if(index == Items.Count-1)
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
                        var workflowVM = vm as IWorkflowDesignerViewModel;
                        if(workflowVM != null)
                        {
                            IContextualResourceModel resource = workflowVM.ResourceModel;
                            if(resource != null)
                            {
                                remove = resource.IsWorkflowSaved;

                                if(!remove)
                                {
                                    remove = ShowRemovePopup(workflowVM);
                                }
//                                else
//                                {
//                                    remove = true;
//                                }

                                if(remove)
                                {
                                    if(resource.IsNewWorkflow)
                                    {
                                        NewWorkflowNames.Instance.Remove(resource.ResourceName); 
                                    }
                                    RemoveWorkspaceItem(workflowVM);
                                    Items.Remove(context);
                                    Logger.TraceInfo("Publish message of type - " + typeof(TabClosedMessage), GetType().Name);
                                    _eventPublisher.Publish(new TabClosedMessage(context));
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