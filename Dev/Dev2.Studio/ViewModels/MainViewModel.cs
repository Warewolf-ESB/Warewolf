using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Session;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Diagnostics;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Administration;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.Factory;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;
using Dev2.Studio.ViewModels.Configuration;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.Web;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.Administration;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Studio.Views.UserInterfaceBuilder;
using Dev2.Studio.Webs;
using Dev2.Utilities;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager.Events;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Views;
using Unlimited.Applications.BusinessDesignStudio.Views.WebsiteBuilder;
using Unlimited.Framework;
using Action = System.Action;

namespace Dev2.Studio.ViewModels
{
    [Export(typeof (IMainViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainViewModel : BaseConductor<WorkSurfaceContextViewModel>, IMainViewModel,
                                 IHandle<DeleteResourceMessage>, IHandle<ShowDependenciesMessage>,
                                 IHandle<SaveResourceMessage>, IHandle<DebugResourceMessage>,
                                 IHandle<ExecuteResourceMessage>, IHandle<SetActiveEnvironmentMessage>,
                                 IHandle<SetActivePageMessage>, IHandle<CloseWizardMessage>,
                                 IHandle<ShowEditResourceWizardMessage>, IHandle<AddWorkflowDesignerMessage>,
                                 IHandle<AddDeployResourcesMessage>, IHandle<ShowHelpTabMessage>,
                                 IHandle<SaveResourceModelMessage>, IHandle<ConfigureDecisionExpressionMessage>,
                                 IHandle<ConfigureSwitchExpressionMessage>, IHandle<ConfigureCaseExpressionMessage>,
                                 IHandle<EditCaseExpressionMessage>, IHandle<ShowWebpartWizardMessage>,
                                 IHandle<AddWebpageDesignerMessage>, IHandle<AddWebsiteDesignerMessage>,
                                 IHandle<SettingsSaveCancelMessage>
    {
        #region Fields

        private readonly IDebugWriter _debugWriter;
        private ILayoutObjectViewModel _activeCell;
        private IEnvironmentModel _activeEnvironment;
        private ILayoutGridViewModel _activePage;
        private ICommand _addStudioShortcutsPageCommand;
        private Dev2DecisionCallbackHandler _callBackHandler;
        private RelayCommand _debugCommand;
        private DebugOutputViewModel _debugOutputViewModel;
        private RelayCommand _deployAllCommand;
        private RelayCommand _deployCommand;
        private ICommand _displayAboutDialogueCommand;
        private RelayCommand _editResourceCommand;
        private RelayCommand _exitCommand;
        private ExplorerViewModel _explorerViewModel;
        private RelayCommand<string> _newResourceCommand;
        private ICommand _notImplementedCommand;
        private RelayCommand<string> _openWebsiteCommand;
        private string _outputMessage;
        private WorkSurfaceContextViewModel _previousActive;
        private RelayCommand _resetLayoutCommand;
        private RelayCommand _runCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _settingsCommand;
        private ICommand _startFeedbackCommand;
        private ICommand _startStopRecordedFeedbackCommand;
        private RelayCommand _viewInBrowserCommand;
        private WebPropertyEditorWindow _win;

        #endregion

        #region Events

        public event ResourceEventHandler OnApplicationExitRequest;

        #endregion

        #region Dependencies

        [Import]
        public IPopUp PopupProvider { get; set; }

        [Import]
        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; set; }

        public DebugOutputViewModel DebugOutputViewModel
        {
            get { return _debugOutputViewModel; }
            set
            {
                if (_debugOutputViewModel == value) return;

                _debugOutputViewModel = value;
                NotifyOfPropertyChange(() => DebugOutputViewModel);
            }
        }

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

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        [Import]
        public IWebCommunication WebCommunication { get; set; }

        public IList<IWorkspaceItem> WorkspaceItems { get; private set; }

        #endregion

        #region Ctor

        public MainViewModel()
        {
            LoadWorkspaceItems();

            _debugWriter = new DebugWriter
                (s => Application.Current.Dispatcher.BeginInvoke
                          (DispatcherPriority.Normal, new Action(
                                                          () =>
                                                          EventAggregator.Publish(new DebugWriterAppendMessage(s)))));

            _callBackHandler = new Dev2DecisionCallbackHandler();
        }

        #endregion

        #region Private Methods

        private void Exit()
        {
            if (OnApplicationExitRequest != null)
                OnApplicationExitRequest(null);
        }

        private void Deploy()
        {
            AddDeployResources(CurrentResourceModel);
        }

        private void Settings()
        {
            AddSettings(ActiveEnvironment);
        }

        private void Edit()
        {
            if (CurrentResourceModel != null)
            {
                EventAggregator.Publish(new ShowEditResourceWizardMessage(CurrentResourceModel));
//                Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, CurrentResourceModel);
            }
        }

        private void StartStopRecordedFeedback()
        {
            if (FeedbackInvoker.CurrentAction == null)
            {
                var recorderFeedbackAction = new RecorderFeedbackAction();
                ImportService.SatisfyImports(recorderFeedbackAction);

                FeedbackInvoker.InvokeFeedback(recorderFeedbackAction);
            }
            else
            {
                var recorderFeedbackAction = FeedbackInvoker.CurrentAction as RecorderFeedbackAction;
                if (recorderFeedbackAction == null)
                {
                    return;
                }

                recorderFeedbackAction.FinishFeedBack();
            }
        }

        private void AddWorkflowDesigner(object obj)
        {
            TypeSwitch.Do(obj,
                          TypeSwitch.Case<IContextualResourceModel>(AddWorkSurfaceContext));
        }

        #endregion Private Methods

        #region Web parts [NOT IN USE ATM]

        internal void ShowWebpartWizard(IPropertyEditorWizard layoutObjectToOpenWizardFor)
        {
            if (layoutObjectToOpenWizardFor != null && layoutObjectToOpenWizardFor.SelectedLayoutObject != null &&
                layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid != null &&
                layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid.ResourceModel != null &&
                layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid.ResourceModel.Environment != null)
            {
                IEnvironmentModel environment =
                    layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid.ResourceModel.Environment;
                string relativeUri = string.Format("services/{0}.wiz",
                                                   layoutObjectToOpenWizardFor.SelectedLayoutObject.WebpartServiceName);
                Uri requestUri;
                if (!Uri.TryCreate(environment.WebServerAddress, relativeUri, out requestUri))
                {
                    requestUri = new Uri(environment.WebServerAddress, relativeUri);
                }

                try
                {
                    string xmlConfig = layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid == null
                                           ? layoutObjectToOpenWizardFor.SelectedLayoutObject.XmlConfiguration
                                           : layoutObjectToOpenWizardFor.SelectedLayoutObject.LayoutObjectGrid
                                                                        .XmlConfiguration;

                    string elementNames = ResourceHelper.GetWebPageElementNames(xmlConfig);
                    // Travis.Frisinger : 06-07-2012 - Remove junk in the config
                    string xmlOutput =
                        ResourceHelper.MergeXmlConfig(
                            layoutObjectToOpenWizardFor.SelectedLayoutObject.XmlConfiguration, elementNames);
                    ErrorResultTO errors;
                    Guid dataListID = environment.UploadToDataList(xmlOutput, out errors);

                    if (CheckAndDisplayErrors(errors)) return;

                    string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);
                    _win = new WebPropertyEditorWindow(layoutObjectToOpenWizardFor, uriString)
                        {
                            Width = 850,
                            Height = 600
                        };
                    _win.ShowDialog();
                }
                catch
                {
                }
            }
        }

        private bool CheckAndDisplayErrors(ErrorResultTO errors)
        {
            if (errors.HasErrors())
            {
                // Bad things happened... Tell the user
                PopupProvider.Show(errors.MakeDisplayReady(), GlobalConstants.SwitchWizardErrorHeading,
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                // Stop configuring!!!
                return true;
            }
            return false;
        }

        internal void AddUserInterfaceWorkflow(IContextualResourceModel resource,
                                               IWorkflowDesignerViewModel workflowViemModel)
        {
            bool isWebpage = ResourceHelper.IsWebpage(resource);
            Type userInterfaceType = ResourceHelper.GetUserInterfaceType(resource);
            if (userInterfaceType == null) return;

            var modelService = workflowViemModel.wfDesigner.Context.Services.GetService<ModelService>();

            IEnumerable<ModelItem> items = modelService.Find(modelService.Root, userInterfaceType);

            IWebActivity webActivity = null;

            IList<ModelItem> modelItems = items as IList<ModelItem> ?? items.ToList();
            if (modelItems.Any())
            {
                ModelItem item = modelItems.First();
                ModelProperty displayNameProperty = item.Properties["DisplayName"];
                if (displayNameProperty != null)
                    webActivity = WebActivityFactory.CreateWebActivity(item, resource,
                                                                       displayNameProperty.ComputedValue.ToString());
                if (isWebpage)
                {
                    AddWebPageDesigner(webActivity);
                }
                else
                {
                    AddWebsiteDesigner(webActivity);
                }
            }
            else
            {
                ModelProperty implementationProperty = modelService.Root.Properties["Implementation"];
                if (modelService.Root.Content != null)
                {
                    var fc = (Flowchart) modelService.Root.Content.ComputedValue;
                    var fs = new FlowStep();
                    dynamic wsa = Activator.CreateInstance(userInterfaceType);
                    fs.Action = wsa;
                    fc.StartNode = fs;
                    if (implementationProperty != null)
                        if (implementationProperty.Value != null)
                        {
                            ModelProperty nodesProperty = implementationProperty.Value.Properties["Nodes"];
                            if (nodesProperty != null)
                                if (nodesProperty.Collection != null) nodesProperty.Collection.Add(fs);
                        }
                }
                if (implementationProperty != null)
                {
                    if (implementationProperty.Value != null)
                    {
                        ModelProperty nodesProperty = implementationProperty.Value.Properties["Nodes"];
                        if (nodesProperty != null)
                        {
                            if (nodesProperty.Collection != null)
                            {
                                ModelItem wsmodelitem = nodesProperty.Collection.Last();
                                ModelProperty actionProperty = wsmodelitem.Properties["Action"];
                                if (actionProperty != null)
                                {
                                    ModelItem amodelitem = actionProperty.Value;

                                    if (amodelitem != null)
                                    {
                                        ModelProperty displayNameProperty = amodelitem.Properties["DisplayName"];
                                        if (displayNameProperty != null)
                                            if (isWebpage)
                                            {
                                                webActivity = WebActivityFactory
                                                    .CreateWebActivity(amodelitem, resource,
                                                                       displayNameProperty.ComputedValue.ToString());
                                                AddWebPageDesigner(webActivity);
                                            }
                                            else
                                            {
                                                webActivity = WebActivityFactory
                                                    .CreateWebActivity(amodelitem, resource,
                                                                       displayNameProperty.ComputedValue.ToString());
                                                AddWebsiteDesigner(webActivity);
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void AddWebsiteDesigner(IWebActivity webActivity)
        {
            var viewModel = new WebsiteEditorViewModel(webActivity);
            ImportService.SatisfyImports(viewModel);
            var editor = new WebsiteEditorWindow(viewModel);
            //SetActive
        }

        internal void AddWebPageDesigner(IWebActivity webActivity)
        {
            if (webActivity == null) return;

            string xmlConfig = "<WebParts/>";

            if (!String.IsNullOrEmpty(webActivity.XMLConfiguration))
            {
                xmlConfig = webActivity.XMLConfiguration;
            }

            if (xmlConfig.StartsWith("[[")) return;

            if (!XmlHelper.IsValidXElement(xmlConfig)) return;

            var l = new LayoutGridViewModel(webActivity);
            ImportService.SatisfyImports(l);
            var layoutGrid = new AutoLayoutGridWindow(l);

            //Set the active page to signal user interface transitions.
            if (l.LayoutObjects.Any())
            {
                SetActivePage(l.LayoutObjects.First());
            }

            //SetActive
        }

        #endregion

        #region Properties

        public IEnvironmentModel ActiveEnvironment
        {
            get { return _activeEnvironment; }
            set
            {
                if (value != null)
                {
                    _activeEnvironment = value;
                }

                NotifyOfPropertyChange(() => CanSave);
                NotifyOfPropertyChange(() => CanDebug);
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

        [Import]
        public IFeedbackInvoker FeedbackInvoker { get; set; }

        [Import]
        public IFeedBackRecorder FeedBackRecorder { get; set; }

        [Import(typeof (IWizardEngine))]
        public IWizardEngine WizardEngine { get; set; }

        [Import]
        public IResourceDependencyService ResourceDependencyService { get; set; }

        [Import]
        public IFrameworkRepository<UserInterfaceLayoutModel> UserInterfaceLayoutRepository { get; set; }

        public bool CanRun
        {
            get { return IsActiveEnvironmentConnected(); }
        }

        public bool CanEdit
        {
            get
            {
                return (SecurityContext.IsUserInRole(new[]
                    {
                        StringResources.BDSAdminRole,
                        StringResources.BDSDeveloperRole,
                        StringResources.BDSTestingRole
                    }) && IsActiveEnvironmentConnected());
            }
        }

        public bool CanViewInBrowser
        {
            get
            {
                if (ActiveItem == null || ActiveItem.WorkSurfaceViewModel == null)
                    return false;
                IWorkSurfaceViewModel activeWorkSurfaceVM =
                    ActiveItem.WorkSurfaceViewModel;
                return (activeWorkSurfaceVM is IWorkflowDesignerViewModel ||
                        activeWorkSurfaceVM is LayoutGridViewModel ||
                        activeWorkSurfaceVM is WebsiteEditorViewModel) &&
                       IsActiveEnvironmentConnected();
            }
        }

        public ILayoutGridViewModel ActivePage
        {
            get { return _activePage; }
        }

        public ILayoutObjectViewModel ActiveCell
        {
            get { return _activeCell; }
        }

        public string Title
        {
            get { return String.Format("Business Design Studio ({0})", SecurityContext.UserIdentity.Name); }
        }

        public bool CanSave
        {
            get { return IsActiveEnvironmentConnected(); }
        }

        public bool CanDebug
        {
            get { return IsActiveEnvironmentConnected(); }
        }

        public string OutputMessage
        {
            get { return _outputMessage ?? (_outputMessage = String.Empty); }
            set
            {
                _outputMessage = value;
                NotifyOfPropertyChange(() => OutputMessage);
            }
        }

        #endregion

        #region Commands

        public ICommand NotImplementedCommand
        {
            get
            {
                return _notImplementedCommand ??
                       (_notImplementedCommand = new RelayCommand(param => MessageBox.Show("Please Implement Me!")));
            }
        }

        public ICommand AddStudioShortcutsPageCommand
        {
            get
            {
                return _addStudioShortcutsPageCommand ??
                       (_addStudioShortcutsPageCommand = new RelayCommand(param => AddStudioShortcutKeysPage()));
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
            get { return _settingsCommand ?? (_settingsCommand = new RelayCommand(param => Settings())); }
        }

        public ICommand OpenWebsiteCommand
        {
            get
            {
                return _openWebsiteCommand ??
                       (_openWebsiteCommand = new RelayCommand<string>(c =>
                           {
                               List<IResourceModel> matches =
                                   ActiveEnvironment.ResourceRepository.All()
                                                    .Where(res =>
                                                           res.ResourceName.Equals(c,
                                                                                   StringComparison
                                                                                       .InvariantCultureIgnoreCase))
                                                    .ToList();
                               if (matches.Any())
                               {
                                   AddWorkflowDocument(matches.First());
                               }
                           }
                                                                       ,
                                                                       c =>
                                                                       IsActiveEnvironmentConnected
                                                                           ()));
            }
        }

        public ICommand ViewInBrowserCommand
        {
            get
            {
                if (_viewInBrowserCommand == null)
                {
                    var eventAggregator = ImportService.GetExportValue<IEventAggregator>();

                    if (eventAggregator != null)
                    {
                        eventAggregator.Publish(new AddMissingAndFindUnusedDataListItemsMessage(CurrentResourceModel));
                    }

                    _viewInBrowserCommand = new RelayCommand(param => ViewInBrowser(),
                                                             param => CanViewInBrowser);
                }
                return _viewInBrowserCommand;
            }
        }

        public RelayCommand<string> NewResourceCommand
        {
            get
            {
                return _newResourceCommand ??
                       (_newResourceCommand = new RelayCommand<string>(AddNewResource,
                                                                       param => IsChosenEnvironmentConnected()));
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                return _exitCommand ??
                       (_exitCommand =
                        new RelayCommand(param => Exit(), param => true));
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editResourceCommand ??
                       (_editResourceCommand = new RelayCommand(param => Edit(), param => CanEdit));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ??
                       (_saveCommand = new RelayCommand(param => Save(CurrentResourceModel), param => CanSave));
            }
        }

        public ICommand DeployCommand
        {
            get { return _deployCommand ?? (_deployCommand = new RelayCommand(param => Deploy())); }
        }

        public ICommand DebugCommand
        {
            get
            {
                return _debugCommand ??
                       (_debugCommand = new RelayCommand(param => Debug(CurrentResourceModel), param => CanDebug));
            }
        }

        public ICommand RunCommand
        {
            get
            {
                return _runCommand ??
                       (_runCommand = new RelayCommand(param => Run(CurrentResourceModel),
                                                       param => CanRun));
            }
        }

        public bool IsChosenEnvironmentConnected()
        {
            // Used for enabling / disabling basic server commands (Eg: Creating a new Workflow)
            if (ActiveEnvironment == null)
            {
                return false;
            }

            return ((ActiveEnvironment != null) && (ActiveEnvironment.IsConnected));
        }

        public bool IsActiveEnvironmentConnected()
        {
            // Used for enabling / disabling commands based off the server status of the current tab (Eg: View in Browser)
            if (CurrentResourceModel == null)
            {
                return false;
            }
            return ((CurrentResourceModel.Environment != null) && (CurrentResourceModel.Environment.IsConnected));
        }

        #endregion

        #region Public Methods

        public void AddNewResource(string resourceType)
        {
            ShowNewResourceWizard(new Tuple<IEnvironmentModel, string>(ActiveEnvironment, resourceType));
        }

        public void Save(IContextualResourceModel resource, bool showWindow = true)
        {
            var vm = ActiveItem.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            if (vm != null)
            {
                vm.BindToModel();
            }
            if (resource == null) return;


            Build(resource, showWindow);

            IResourceModel resourceToUpdate =
                resource.Environment.ResourceRepository.FindSingle(
                    c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));
            if (resourceToUpdate != null)
            {
                resourceToUpdate.Update(resource);
            }

            IWorkspaceItem workspaceItem = WorkspaceItems.FirstOrDefault(
                wi => wi.ServiceName == resource.ResourceName);
            if (workspaceItem == null)
            {
                return;
            }

            workspaceItem.Action = WorkspaceItemAction.Commit;

            dynamic publishRequest = new UnlimitedObject();
            publishRequest.Service = "UpdateWorkspaceItemService";
            publishRequest.Roles = String.Join(",", SecurityContext.Roles);
            publishRequest.ItemXml = workspaceItem.ToXml();

            string result = resource.Environment.DsfChannel
                                    .ExecuteCommand(publishRequest.XmlString, workspaceItem.WorkspaceID,
                                                    GlobalConstants.NullDataListID) ??
                            string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat,
                                          publishRequest.Service);

            var sb = new StringBuilder();
            sb.AppendLine(String.Format("<Save StartDate=\"{0}\">",
                                        DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            sb.AppendLine(result);
            sb.AppendLine(String.Format("</Save>"));
            OutputMessage += sb.ToString();
            DisplayOutput(OutputMessage, showWindow);

            resource.Environment.ResourceRepository.Save(resource);
            EventAggregator.Publish(new UpdateDeployMessage());
//                Mediator.SendMessage(MediatorMessages.UpdateDeploy, null);
        }

        public void ViewInBrowser()
        {
            if (ActiveItem.DataListViewModel != null)
                EventAggregator.Publish(new FindMissingDataListItemsMessage(ActiveItem.DataListViewModel));
//
//                Mediator.SendMessage(MediatorMessages.FindMissingDataListItems,
//                                     ActiveItem.DataListViewModel);

            IContextualResourceModel resourceModel = CurrentResourceModel;
            Save(resourceModel, false);
            // 2012.10.17 - 5782: TWR - Use build first so that changes are persisted to the client workspace on the server
            Build(resourceModel);

            if (resourceModel != null && resourceModel.Environment != null)
            {
                string relativeUrl = string.Format("/services/{0}?wid={1}", resourceModel.ResourceName,
                                                   ((IStudioClientContext) resourceModel.Environment.DsfChannel)
                                                       .AccountID);
                Uri url;
                if (!Uri.TryCreate(resourceModel.Environment.WebServerAddress, relativeUrl, out url))
                {
                    Uri.TryCreate(new Uri(StringResources.Uri_WebServer), relativeUrl, out url);
                }

                Process.Start(url.AbsoluteUri);
            }
        }

        public void Run(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            IServiceDebugInfoModel debugInfoModel =
                ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, 0, DebugMode.Run);

            DebugTO debugTO;
            ViewModelDialogResults viewModelDialogResults = GetServiceInputDataFromUser(debugInfoModel, out debugTO);

            if (viewModelDialogResults == ViewModelDialogResults.Okay)
            {
                PerformDebugTask(debugTO, resourceModel.Environment);
            }
        }

        public void AddHelpDocument(IResourceModel resource)
        {
            ShowHelpTab(resource.HelpLink);
        }

        public void AddWorkflowDocument(IResourceModel resource)
        {
            EventAggregator.Publish(new AddWorkflowDesignerMessage(resource));
        }

        public void AddDependencyVisualizerDocument(IContextualResourceModel resource)
        {
            if (resource == null)
                return;

            ActivateOrCreateUniqueWorkSurface<DependencyVisualiserViewModel>
                (WorkSurfaceContext.DependencyVisualiser,
                 new[] {new Tuple<string, object>("ResourceModel", resource)});
        }

        public void Debug(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }
            Save(resourceModel, false);
            IServiceDebugInfoModel debugInfoModel =
                ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, 0,
                                                                         DebugMode.DebugInteractive);

            DebugTO debugTO;
            ViewModelDialogResults viewModelDialogResults = GetServiceInputDataFromUser(debugInfoModel, out debugTO);

            if (viewModelDialogResults == ViewModelDialogResults.Okay)
            {
                EventAggregator.Publish(new DebugWriterWriteMessage(string.Empty));
                //Mediator.SendMessage(MediatorMessages.DebugWriterWrite, string.Empty);

                // Try show the output window

                var clientContext = resourceModel.Environment.DsfChannel as IStudioClientContext;
                if (clientContext != null)
                {
                    clientContext.AddDebugWriter(_debugWriter);

                    // 2012.10.17 - 4423: TWR - Use build first so that changes are persisted to the client workspace on the server
                    Build(resourceModel);

                    XElement dataList = XElement.Parse(debugTO.XmlData);
                    dataList.Add(new XElement("BDSDebugMode", debugTO.IsDebugMode));


                    // Sashen.Naidoo : 14-02-2012 : BUG 8793 : Added asynchronous callback to remove the debugwriter when the the Webserver callback has completed.
                    //                              Previously, everytime the debug method was invoked it would add a debug writer to the clientcontext and
                    //                              this would never be removed

                    if (EventAggregator != null)
                    {
                        EventAggregator.Publish(new DebugStatusMessage(true));
                    }


                    Action<UploadStringCompletedEventArgs> webserverCallback =
                        asyncCallback => clientContext.RemoveDebugWriter(_debugWriter);

                    WebServer.SendAsync(WebServerMethod.POST, resourceModel, dataList.ToString(), webserverCallback);
                }
            }
        }

        public void Build(IContextualResourceModel model, bool showWindow = true, bool deploy = true)
        {
            if (model == null || model.Environment == null || !model.Environment.IsConnected)
            {
                return;
            }

            // Clear output
            EventAggregator.Publish(new DebugWriterWriteMessage(string.Empty));
            //Mediator.SendMessage(MediatorMessages.DebugWriterWrite, string.Empty);

            OutputMessage = String.Empty;

            var sb = new StringBuilder();
            dynamic buildRequest = new UnlimitedObject();

            if (!deploy)
            {
                buildRequest.Service = "CompileResourceService";
            }
            else
            {
                buildRequest.Service = "AddResourceService";
                buildRequest.Roles = String.Join(",", SecurityContext.Roles);
            }

            sb.AppendLine(String.Format("<Build StartDate=\"{0}\">", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

            if (model.ResourceType == ResourceType.WorkflowService)
            {
                sb.AppendLine(String.Format("<Build WorkflowName=\"{0}\" />", model.ResourceName));
            }
            else if (model.ResourceType == ResourceType.Service)
            {
                sb.AppendLine(String.Format("<Build Service=\"{0}\" />", model.ResourceName));
            }
            else if (model.ResourceType == ResourceType.Source)
            {
                sb.AppendLine(String.Format("<Build Source=\"{0}\" />", model.ResourceName));
            }

            buildRequest.ResourceXml = model.ToServiceDefinition();

            Guid workspaceID = ((IStudioClientContext) model.Environment.DsfChannel).AccountID;

            string result =
                model.Environment.DsfChannel.
                      ExecuteCommand(buildRequest.XmlString, workspaceID, GlobalConstants.NullDataListID) ??
                string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, buildRequest.Service);

            sb.AppendLine(result);
            sb.AppendLine(String.Format("</Build>"));
            OutputMessage += sb.ToString();
            DisplayOutput(OutputMessage, showWindow);
        }

        public void DisplayOutput(string message, bool popupWindow = true)
        {
            EventAggregator.Publish(new DebugWriterAppendMessage(message));
            //    Mediator.SendMessage(MediatorMessages.DebugWriterAppend, message);
        }

        public void SetActivePage(ILayoutObjectViewModel cell)
        {
            if (cell == null) return;

            _activePage = cell.LayoutObjectGrid;
            _activeCell = cell;
            NotifyOfPropertyChange(() => ActivePage);
        }

        public void AddStartTabs()
        {
            AddWorkspaceItems();

            string path = FileHelper.GetFullPath(StringResources.Uri_Studio_Homepage);
            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage
                                                             , new[] {new Tuple<string, object>("Uri", path)});
        }

        /// <summary>
        ///     Saves all open tabs locally and writes the open tabs the to collection of workspace items
        /// </summary>
        public void PersistTabs()
        {
            SaveWorkspaceItems();
            foreach (IContextualResourceModel resourceModel in GetOpenContextualResourceModels())
            {
                Build(resourceModel);
            }
        }

        /// <summary>
        ///     Gets all IContextualResource models from the open tabs.
        /// </summary>
        /// <returns></returns>
        public List<IContextualResourceModel> GetOpenContextualResourceModels()
        {
            return Items.Select(c => c.WorkSurfaceViewModel)
                        .Select(ResourceHelper.GetContextualResourceModel)
                        .Where(resourceModel => resourceModel != null)
                        .ToList();
        }

        public void ShowNewResourceWizard(object newResourceInfo)
        {
            var newResourceTuple = newResourceInfo as Tuple<IEnvironmentModel, string>;

            if (newResourceTuple == null) return;

            SaveOpenTabs();

            IEnvironmentModel environment = newResourceTuple.Item1;
            string resourceType = newResourceTuple.Item2;

            IContextualResourceModel resourceModel = ResourceModelFactory.CreateResourceModel(environment, resourceType,
                                                                                              resourceType);
            var resourceViewModel = new ResourceWizardViewModel(resourceModel);

            //
            // TWR: 2013.02.14
            // PBI: 801
            // BUG: 8477
            //
            if (RootWebSite.ShowDialog(resourceModel))
            {
                return;
            }

            bool doesServiceExist =
                environment.ResourceRepository.Find(r => r.ResourceName == "Dev2ServiceDetails").Count > 0;

            if (doesServiceExist)
            {
                // Travis.Frisinger: 07.90.2012 - Amended to convert studio resources into server resources
                string resName =
                    StudioToWizardBridge.ConvertStudioToWizardType(resourceType.ToString(CultureInfo.InvariantCulture),
                                                                   resourceModel.ServiceDefinition,
                                                                   resourceModel.Category);
                //string requestUri = string.Format("{0}/services/{1}?{2}={3}&Dev2NewService=1", MainViewModel.CurrentWebServer, StudioToWizardBridge.SelectWizard(resourceModel), ResourceKeys.Dev2ServiceType, resName);

                Uri requestUri;
                if (
                    !Uri.TryCreate(environment.WebServerAddress,
                                   BuildUri(resourceModel, resName), out requestUri))
                {
                    requestUri = new Uri(new Uri(StringResources.Uri_WebServer), BuildUri(resourceModel, resName));
                }

                try
                {
                    _win = new WebPropertyEditorWindow(resourceViewModel, requestUri.AbsoluteUri)
                        {
                            Width = 850,
                            Height = 600
                        };
                    _win.ShowDialog();
                }
                catch
                {
                }
            }
            else
            {
                PopupProvider.Show(
                    "Couldn't find the resource needed to display the wizard. Please ensure that a resource with the name 'Dev2ServiceDetails' exists.",
                    "Missing Wizard", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string BuildUri(IContextualResourceModel resourceModel, string resName)
        {
            string uriString = "/services/" + StudioToWizardBridge.SelectWizard(resourceModel);
            if (resourceModel.ResourceType == ResourceType.WorkflowService ||
                resourceModel.ResourceType == ResourceType.Service)
            {
                uriString += "?" + ResourceKeys.Dev2ServiceType + "=" + resName;
            }
            return uriString;
        }

        public void ShowEditResourceWizard(object resourceModel)
        {
            //Brendon.Page, 2012-12-03. Hack to fix Bug 6367. It was decided by the team that this should be employed because the wizards are going to be 
            //                          reworked in the near future and it was to much work to go an ammend all the javascript to be workspace aware.
            SaveOpenTabs();

            var resourceModelToEdit = resourceModel as IContextualResourceModel;

            if (RootWebSite.ShowDialog(resourceModelToEdit))
            {
                return;
            }

            bool doesServiceExist = resourceModelToEdit != null &&
                                    resourceModelToEdit.Environment.ResourceRepository.Find(
                                        r => r.ResourceName == "Dev2ServiceDetails").Count > 0;

            if (doesServiceExist)
            {
                var resourceViewModel = new ResourceWizardViewModel(resourceModelToEdit);

                Uri requestUri;
                if (
                    !Uri.TryCreate(resourceModelToEdit.Environment.WebServerAddress,
                                   "/services/" + StudioToWizardBridge.SelectWizard(resourceModelToEdit), out requestUri))
                {
                    requestUri = new Uri(new Uri(StringResources.Uri_WebServer),
                                         "/services/" + StudioToWizardBridge.SelectWizard(resourceModelToEdit));
                }

                try
                {
                    ErrorResultTO errors;
                    string args =
                        StudioToWizardBridge.BuildStudioEditPayload(resourceModelToEdit.ResourceType.ToString(),
                                                                    resourceModelToEdit);
                    Guid dataListID = resourceModelToEdit.Environment.UploadToDataList(args, out errors);

                    if (errors.HasErrors()) //BUG 8796, Added this if to handle errors
                    {
                        // Bad things happened... Tell the user
                        PopupProvider.Show(errors.MakeDisplayReady(), "Webpart Wizard Error", MessageBoxButton.OK,
                                           MessageBoxImage.Error);
                        // Stop configuring!!!
                        return;
                    }

                    string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);

                    _win = new WebPropertyEditorWindow(resourceViewModel, uriString) {Width = 850, Height = 600};
                    _win.ShowDialog();
                }
                catch
                {
                }
            }
            else
            {
                PopupProvider.Show(
                    "Couldn't find the resource needed to display the wizard. Please ensure that a resource with the name 'Dev2ServiceDetails' exists.",
                    "Missing Wizard", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        ///     Saves the open tabs.
        /// </summary>
        private void SaveOpenTabs()
        {
            foreach (IContextualResourceModel resourceModel in GetOpenContextualResourceModels())
            {
                Save(resourceModel, false);
            }
        }

        public void AddDeployResources(object input)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DeployResources);
            bool exist = ActivateWorkSurfaceIfPresent(key);

            if (exist)
            {
                EventAggregator.Publish(new SelectItemInDeployMessage(input));
                //Mediator.SendMessage(MediatorMessages.SelectItemInDeploy, input);
            }
            else
            {
                WorkSurfaceContextViewModel context = WorkSurfaceContextFactory.CreateDeployViewModel(input);
                Items.Add(context);
                ActivateItem(context);
            }
        }

        public void AddSettings(IEnvironmentModel environment)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Settings, environment.DataListChannel.ServerID);

            bool exist = ActivateWorkSurfaceIfPresent(key);

            if (!exist)
            {
                WorkSurfaceContextViewModel context =
                    WorkSurfaceContextFactory.CreateRuntimeConfigurationViewModel(environment);
                Items.Add(context);
                ActivateItem(context);
            }
        }

        public void ShowHelpTab(string uriToDisplay)
        {
            if (!string.IsNullOrWhiteSpace(uriToDisplay))
                ActivateOrCreateUniqueWorkSurface<HelpViewModel>
                    (WorkSurfaceContext.Help,
                     new[] {new Tuple<string, object>("Uri", uriToDisplay)});
        }

        public void CloseWizard()
        {
            if (_win != null)
            {
                _win.Close();
            }
        }

        public bool RemoveContext(WorkSurfaceKey key)
        {
            WorkSurfaceContextViewModel context = FindWorkSurfaceContextViewModel(key);
            return Items.Remove(context);
        }

        public bool RemoveContext(IContextualResourceModel model)
        {
            WorkSurfaceContextViewModel context = FindWorkSurfaceContextViewModel(model);
            return Items.Remove(context);
        }

        public static bool QueryDeleteExplorerResource(IContextualResourceModel model, bool hasDependencies,
                                                       out bool openDependencyGraph)
        {
            openDependencyGraph = false;

            bool shouldRemove = MessageBox.Show
                                    (Application.Current.MainWindow, "Are you sure you wish to delete the \""
                                                                     + model.ResourceName + "\" " +
                                                                     model.ResourceType.GetDescription() +
                                                                     "?",
                                     "Confirm " + model.ResourceType.GetDescription() +
                                     " Deletion", MessageBoxButton.YesNo) ==
                                MessageBoxResult.Yes;

            if (shouldRemove && hasDependencies)
            {
                var dialog = new DeleteResourceDialog
                    ("Confirm " + model.ResourceType.GetDescription() + " Deletion", "The \""
                                                                                     + model.ResourceName + "\" " +
                                                                                     model.ResourceType.GetDescription() +
                                                                                     " has resources that depend on it to function, are you sure you want to delete this "
                                                                                     +
                                                                                     model.ResourceType.GetDescription() +
                                                                                     "?",
                     true) {Owner = Application.Current.MainWindow};
                bool? result = dialog.ShowDialog();
                shouldRemove = result.HasValue && result.Value;

                openDependencyGraph = dialog.OpenDependencyGraph;
            }

            return shouldRemove;
        }

        public ViewModelDialogResults GetServiceInputDataFromUser(IServiceDebugInfoModel input, out DebugTO debugTO)
        {
            EventAggregator.Publish(new AddMissingAndFindUnusedDataListItemsMessage(CurrentResourceModel));

            var inputData = new WorkflowInputDataWindow();

            debugTO = new DebugTO
                {
                    DataList = !string.IsNullOrEmpty(input.ResourceModel.DataList)
                                   ? input.ResourceModel.DataList
                                   : "<DataList></DataList>", //Bug 8363 & Bug 8018
                    ServiceName = input.ResourceModel.ResourceName,
                    WorkflowID = input.ResourceModel.ResourceName,
                    WorkflowXaml = input.ResourceModel.WorkflowXaml,
                    RememberInputs = true
                };
            //debugTO.WorkflowID = input.ResourceModel.ResourceName;
            if (input.DebugModeSetting == DebugMode.DebugInteractive)
            {
                debugTO.IsDebugMode = true;
            }
            //Call the InitDebugSession(debugTO);
            var inputDataViewModel = new WorkflowInputDataViewModel(debugTO);
            //2012.10.11: massimo.guerrera - Added for PBI 5781
            inputDataViewModel.LoadWorkflowInputs();
            inputData.DataContext = inputDataViewModel;
            inputData.ShowDialog();

            return inputDataViewModel.DialogResult;
        }

        /// <summary>
        ///     Configures the decision expression.
        ///     Travis.Frisinger - Developed for new Decision Wizard
        /// </summary>
        /// <param name="wrapper">The wrapper.</param>
        public void ConfigureDecisionExpression(Tuple<ModelItem, IEnvironmentModel> wrapper)
        {
            IEnvironmentModel environment = wrapper.Item2;

            ModelItem activity = ActivityHelper.GetActivityFromWrapper(wrapper, GlobalConstants.ConditionPropertyText);
            if (activity == null) return;

            ModelProperty activityExpression = activity.Properties[GlobalConstants.ExpressionPropertyText];

            string val = JsonConvert.SerializeObject(DataListConstants.DefaultStack);

            if (activityExpression != null && activityExpression.Value != null)
            {
                val = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activityExpression.Value.ToString());
            }

            // Now invoke the Wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.WebServerAddress + GlobalConstants.DecisionWizardLocation)
                               , UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, val, 824, 508);

            // Wizard finished...
            // Now Fetch from DL and push the model into the activityExpression.SetValue();
            try
            {
                string tmp = WebHelper.CleanModelData(_callBackHandler);
                var dds = JsonConvert.DeserializeObject<Dev2DecisionStack>(tmp);

                if (dds != null)
                {
                    ActivityHelper.SetArmTextDefaults(dds);
                    ActivityHelper.InjectExpression(dds, activityExpression);
                    ActivityHelper.SetArmText(activity, dds);
                }
            }
            catch
            {
                PopupProvider.Show(GlobalConstants.DecisionWizardErrorString,
                                   GlobalConstants.DecisionWizardErrorHeading, MessageBoxButton.OK,
                                   MessageBoxImage.Error);
            }
        }

        public void ConfigureSwitchExpression(Tuple<ModelItem, IEnvironmentModel> wrapper)
        {
            IEnvironmentModel environment = wrapper.Item2;
            ModelItem activity = ActivityHelper.GetActivityFromWrapper(wrapper,
                                                                       GlobalConstants.SwitchExpressionPropertyText);

            if (activity == null) return;

            ModelProperty activityExpression =
                activity.Properties[GlobalConstants.SwitchExpressionTextPropertyText];

            string webModel = JsonConvert.SerializeObject(DataListConstants.DefaultSwitch);

            if (activityExpression != null && activityExpression.Value != null)
            {
                string val = ActivityHelper.ExtractData(activityExpression.Value.ToString());
                if (!string.IsNullOrEmpty(val))
                {
                    var ds = new Dev2Switch {SwitchVariable = val};
                    webModel = JsonConvert.SerializeObject(ds);
                }
            }

            // now invoke the wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.WebServerAddress + GlobalConstants.SwitchDropWizardLocation),
                               UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, webModel, 470, 285);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);
                ActivityHelper.InjectExpression(ds, activityExpression);
            }
            catch
            {
                PopupProvider.Show(GlobalConstants.SwitchWizardErrorString,
                                   GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                   MessageBoxImage.Error);
            }
        }

        public void ConfigureSwitchCaseExpression(Tuple<ModelItem, IEnvironmentModel> payload)
        {
            IEnvironmentModel environment = payload.Item2;
            ModelItem switchCase = payload.Item1;
            string modelData = JsonConvert.SerializeObject(DataListConstants.DefaultCase);

            // now invoke the wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.WebServerAddress + GlobalConstants.SwitchDragWizardLocation),
                               UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, modelData, 470, 285);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);
                ActivityHelper.SetSwitchKeyProperty(ds, switchCase);
            }
            catch
            {
                PopupProvider.Show(GlobalConstants.SwitchWizardErrorString,
                                   GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                   MessageBoxImage.Error);
            }
        }

        // 28.01.2013 - Travis.Frisinger : Added for Case Edits
        public void EditSwitchCaseExpression(Tuple<ModelProperty, IEnvironmentModel> payload)
        {
            IEnvironmentModel environment = payload.Item2;
            ModelProperty switchCaseValue = payload.Item1;

            string modelData = JsonConvert.SerializeObject(DataListConstants.DefaultCase);

            // Extract existing value ;)
            if (switchCaseValue != null)
            {
                string val = switchCaseValue.ComputedValue.ToString();
                modelData = JsonConvert.SerializeObject(new Dev2Switch {SwitchVariable = val});
            }


            // now invoke the wizard ;)
            Uri requestUri;
            if (!Uri.TryCreate((environment.WebServerAddress + GlobalConstants.SwitchDragWizardLocation),
                               UriKind.Absolute, out requestUri)) return;

            _callBackHandler = WebHelper.ShowWebpage(requestUri, modelData, 470, 285);

            // Wizard finished...
            // Now Fetch from DL and push the model data into the workflow
            try
            {
                var ds = JsonConvert.DeserializeObject<Dev2Switch>(_callBackHandler.ModelData);

                if (ds != null)
                {
                    if (switchCaseValue != null) switchCaseValue.SetValue(ds.SwitchVariable);
                }
            }
            catch
            {
                PopupProvider.Show(GlobalConstants.SwitchWizardErrorString,
                                   GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                   MessageBoxImage.Error);
            }
        }

        public void DeployAll()
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

            AddDeployResources(payload);
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
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Help);
            bool exists = ActivateWorkSurfaceIfPresent(key, initParms);

            if (!exists)
                AddUniqueWorkSurface<T>(context, initParms);
        }

        private void AddWorkspaceItems()
        {
            if (EnvironmentRepository == null) return;

            foreach (IWorkspaceItem workspaceItem in WorkspaceItems)
            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = workspaceItem;
                IEnvironmentModel environment =
                    EnvironmentRepository
                        .FindSingle(e => e.IsConnected && e.DsfChannel is IStudioClientContext
                                         && ((IStudioClientContext) e.DsfChannel).ServerID == item.ServerID);

                if (environment == null || environment.ResourceRepository == null) continue;

                // TODO: 5559 B.P. to implement in new architecture
                // This code will only start working when a constant ServerID is generated on the server
                var resource = environment.ResourceRepository.FindSingle(r => r.ResourceName == item.ServiceName)
                               as IContextualResourceModel;
                if (resource == null) continue;

                if (resource.ResourceType == ResourceType.WorkflowService)
                {
                    AddWorkSurfaceContext(resource);
                }
            }
        }

        public void AddWorkSurfaceContext(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null) return;

            //Activates if exists
            bool exists = ActivateWorkSurfaceIfPresent(resourceModel);

            if (exists) return;

            //else create new
            AddWorkspaceItem(resourceModel);
            WorkSurfaceContextViewModel ctx = WorkSurfaceContextFactory.CreateResourceViewModel(resourceModel);
            Items.Add(ctx);
            ActivateItem(ctx);

            if (resourceModel.Category.Equals("Webpage", StringComparison.InvariantCultureIgnoreCase)
                ||
                resourceModel.Category.Equals("Human Interface Workflow", StringComparison.InvariantCultureIgnoreCase)
                || resourceModel.Category.Equals("Website", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                AddUserInterfaceWorkflow(resourceModel, ctx.WorkSurfaceViewModel as IWorkflowDesignerViewModel);
            }
        }

        private bool ActivateWorkSurfaceIfPresent(IContextualResourceModel resource,
                                                  Tuple<string, object>[] initParms = null)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(resource);
            return ActivateWorkSurfaceIfPresent(key, initParms);
        }

        private bool ActivateWorkSurfaceIfPresent(WorkSurfaceKey key, Tuple<string, object>[] initParms = null)
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

        private WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(WorkSurfaceKey key)
        {
            return Items.FirstOrDefault(
                c => WorkSurfaceKeyEqualityComparer.Current.Equals(key, c.WorkSurfaceKey));
        }

        private WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IContextualResourceModel resource)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(resource);
            return FindWorkSurfaceContextViewModel(key);
        }

        public void PerformDebugTask(DebugTO debugTO, IEnvironmentModel environment)
        {
            // Starting new debug session so clear output
            EventAggregator.Publish(new DebugWriterWriteMessage(string.Empty));
            //Mediator.SendMessage(MediatorMessages.DebugWriterWrite, string.Empty);

            if (!debugTO.IsDebugMode)
            {
                if (environment == null || !environment.IsConnected)
                {
                    return;
                }

                dynamic dataObject = new UnlimitedObject();

                if (!String.IsNullOrEmpty(debugTO.XmlData))
                {
                    dataObject = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(debugTO.XmlData);
                }

                dataObject.Service = debugTO.ServiceName;

                var ctx = (IStudioClientContext) environment.DsfChannel;
                string msg = environment.DsfChannel.ExecuteCommand(dataObject.XmlString, ctx.AccountID,
                                                                   GlobalConstants.NullDataListID);
                if (msg == null)
                {
                    throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat,
                                                      dataObject.Service));
                }

                DisplayOutput(msg);
            }
        }

        public void AddStudioShortcutKeysPage()
        {
            string path = FileHelper.GetFullPath(StringResources.Uri_Studio_Shortcut_Keys_Document);
            ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.ShortcutKeys
                                                             , new[] {new Tuple<string, object>("Uri", path)});
        }

        public void DisplayAboutDialogue()
        {
            IDev2DialogueViewModel dialogueViewModel = new Dev2DialogueViewModel();
            ImportService.SatisfyImports(dialogueViewModel);
            string packUri = StringResources.Dev2_Logo;
            dialogueViewModel.SetupDialogue(StringResources.About_Header_Text,
                                            String.Format(StringResources.About_Content, StringResources.CurrentVersion,
                                                          StringResources.CurrentVersion), packUri,
                                            StringResources.About_Description_Header);
            var dev2Dialog = new Dev2Dialogue
                {
                    Owner = Application.Current.MainWindow,
                    DataContext = dialogueViewModel
                };
            dialogueViewModel.OnOkClick += (e, f) =>
                {
                    dev2Dialog.Close();
                    dialogueViewModel.Dispose();
                };
            dev2Dialog.ShowDialog();
        }

        public void CloseWorkSurfaceContext(WorkSurfaceContextViewModel context, PaneClosingEventArgs e)
        {
            bool remove = true;
            IWorkSurfaceViewModel vm = context.WorkSurfaceViewModel;
            if (vm != null && vm.WorkSurfaceContext == WorkSurfaceContext.Workflow)
            {
                var workflowVM = vm as IWorkflowDesignerViewModel;
                if (workflowVM == null) return;

                remove = workflowVM.ResourceModel.IsWorkflowSaved(workflowVM.ServiceDefinition);
                if (!remove)
                {
                    remove = ShowRemovePopup(workflowVM);
                }
                if (remove) RemoveWorkspaceItem(workflowVM);
            }

            if (remove)
            {
                Items.Remove(context);
                EventAggregator.Publish(new TabClosedMessage(context));
            }
            if (e != null)
                e.Cancel = !remove;
        }

        private bool ShowRemovePopup(IWorkflowDesignerViewModel workflowVM)
        {
            IPopUp pop = new PopUp(
                "Workflow not saved...",
                "The workflow that you are closing is not saved.\r\nWould you like to save the  workflow?\r\n-------------------------------------------------------------------\r\nClicking Yes will save the workflow\r\nClicking No will discard your changes\r\nClicking Cancel will return you to the workflow",
                MessageBoxImage.Information,
                MessageBoxButton.YesNoCancel
                );
            MessageBoxResult res = pop.Show();

            switch (res)
            {
                case MessageBoxResult.Yes:
                    //workflowVM.BindToModel();
                    EventAggregator.Publish(new SaveResourceMessage(workflowVM.ResourceModel));
                    //Mediator.SendMessage(MediatorMessages.SaveResource, workflowVM.ResourceModel);
                    return true;
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Cancel:
                    return false;
            }
            return false;
        }

        public void StartFeedback()
        {
            var emailFeedbackAction = new EmailFeedbackAction();
            ImportService.SatisfyImports(emailFeedbackAction);

            var recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            FeedbackInvoker.InvokeFeedback(emailFeedbackAction, recorderFeedbackAction);
        }

        #endregion

        #region IHandle

        public void Handle(AddDeployResourcesMessage message)
        {
            AddDeployResources(message.Model);
        }

        public void Handle(AddWebpageDesignerMessage message)
        {
            AddWebPageDesigner(message.WebActivity);
        }

        public void Handle(AddWebsiteDesignerMessage message)
        {
            AddWebPageDesigner(message.WebActivity);
        }

        public void Handle(AddWorkflowDesignerMessage message)
        {
            AddWorkflowDesigner(message.Resource);
        }

        public void Handle(CloseWizardMessage message)
        {
            CloseWizard();
        }

        public void Handle(ConfigureCaseExpressionMessage message)
        {
            ConfigureSwitchCaseExpression(message.Model);
        }

        public void Handle(ConfigureDecisionExpressionMessage message)
        {
            ConfigureDecisionExpression(message.Model);
        }

        public void Handle(ConfigureSwitchExpressionMessage message)
        {
            ConfigureSwitchExpression(message.Model);
        }

        public void Handle(DebugResourceMessage message)
        {
            Debug(message.Resource);
        }

        public void Handle(DeleteResourceMessage message)
        {
            var model = message.ResourceModel as IContextualResourceModel;
            if (model == null) return;

            List<IResourceModel> dependencies = ResourceDependencyService.GetUniqueDependencies(model);
            bool openDependencyGraph;
            bool shouldRemove = QueryDeleteExplorerResource(model,
                                                            dependencies != null && dependencies.Count > 0,
                                                            out openDependencyGraph);

            if (shouldRemove)
            {
                UnlimitedObject success = model.Environment.ResourceRepository.DeleteResource(model);
                if (success != null)
                {
                    RemoveContext(model);
                    ActivateItem(_previousActive);
                    EventAggregator.Publish(new RemoveNavigationResourceMessage(model));
                }
            }
            else if (openDependencyGraph)
            {
                AddDependencyVisualizerDocument(model);
            }
        }

        public void Handle(EditCaseExpressionMessage message)
        {
            EditSwitchCaseExpression(message.Model);
        }

        public void Handle(ExecuteResourceMessage message)
        {
            Run(message.Resource);
        }

        public void Handle(SaveResourceMessage message)
        {
            Save(message.Resource);
        }

        public void Handle(SaveResourceModelMessage message)
        {
            Save(message.ResourceModel, false);
        }

        public void Handle(SetActiveEnvironmentMessage message)
        {
            ActiveEnvironment = message.EnvironmentModel;
        }

        public void Handle(SetActivePageMessage message)
        {
            SetActivePage(message.LayoutObjectViewModel);
        }

        public void Handle(SettingsSaveCancelMessage message)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Settings, message.Environment.DataListChannel.ServerID);

            WorkSurfaceContextViewModel viewModel = FindWorkSurfaceContextViewModel(key);

            if (viewModel != null)
            {
                DeactivateItem(viewModel, true);
            }
        }

        public void Handle(ShowDependenciesMessage message)
        {
            AddDependencyVisualizerDocument(message.ResourceModel as IContextualResourceModel);
        }

        public void Handle(ShowEditResourceWizardMessage message)
        {
            ShowEditResourceWizard(message.ResourceModel);
        }

        public void Handle(ShowHelpTabMessage message)
        {
            ShowHelpTab(message.HelpLink);
        }

        public void Handle(ShowWebpartWizardMessage message)
        {
            ShowWebpartWizard(message.LayoutObjectViewModel);
        }

        #endregion

        #region Protected Methods

        public override void DeactivateItem(WorkSurfaceContextViewModel item, bool close)
        {
            if (close)
            {
                CloseWorkSurfaceContext(item, null);

            }

            ActivateItem(_previousActive);

            base.DeactivateItem(item, close);
        }

        protected override void SatisfyImports()
        {
            // Overridden to prevent the base view model form trying to automatically satisfy the imports on construction.
            // This is done so that an instance of the main view model can be created from the ImportService using the 
            // GetExportedValue. That is done so that all other imports of the main view model will utilize the instance
            // being managed by the ImportService.
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            ExplorerViewModel = new ExplorerViewModel();
            DebugOutputViewModel = new DebugOutputViewModel();
            AddStartTabs();
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
                PersistTabs();

            base.OnDeactivate(close);
        }

        public override void ActivateItem(WorkSurfaceContextViewModel item)
        {
            _previousActive = ActiveItem;
            base.ActivateItem(item);
        }

        protected override void OnActivationProcessed(WorkSurfaceContextViewModel item, bool success)
        {
            base.OnActivationProcessed(item, success);

            if (success)
            {
                NotifyOfPropertyChange(() => ActiveItem);
                NotifyOfPropertyChange(() => ActiveItem.WorkSurfaceViewModel);
                NotifyOfPropertyChange(() => ActiveItem.WorkSurfaceViewModel.WorkSurfaceContext);
            }
        }

        #endregion Protected Methods

        #region WorkspaceItems management

        #region Load/Save WorkspaceItems

        private void LoadWorkspaceItems()
        {
            WorkspaceItems = WorkspaceItemRepository.Read();
        }

        private void SaveWorkspaceItems()
        {
            WorkspaceItemRepository.Write(WorkspaceItems);
        }

        #endregion

        #region AddWorkspaceItem

        private void AddWorkspaceItem(IContextualResourceModel model)
        {
            // TODO: Check model server uri
            IWorkspaceItem workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ServiceName == model.ResourceName);
            if (workspaceItem != null) return;

            var context = (IStudioClientContext) model.Environment.DsfChannel;
            workspaceItem = new WorkspaceItem(context.AccountID, context.ServerID)
                {
                    ServiceName = model.ResourceName,
                    ServiceType =
                        model.ResourceType == ResourceType.Source
                            ? WorkspaceItem.SourceServiceType
                            : WorkspaceItem.ServiceServiceType,
                };
            WorkspaceItems.Add(workspaceItem);
            SaveWorkspaceItems();
        }

        #endregion

        private void RemoveWorkspaceItem(IWorkflowDesignerViewModel viewModel)
        {
            // TODO: Check model server uri
            IWorkspaceItem itemToRemove =
                WorkspaceItems.FirstOrDefault(c => c.ServiceName == viewModel.ResourceModel.ResourceName);
            if (itemToRemove == null) return;

            WorkspaceItems.Remove(itemToRemove);
            SaveWorkspaceItems();
        }

        #endregion
    }
}