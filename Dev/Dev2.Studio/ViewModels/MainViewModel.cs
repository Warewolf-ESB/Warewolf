using Dev2.Common;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Session;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Diagnostics;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Feedback;
using Dev2.Studio.Feedback.Actions;
using Dev2.Workspaces;
using System;
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
using Unlimited.Framework;

namespace Dev2.Studio.ViewModels
{
    [Export(typeof (IMainViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainViewModel : BaseViewModel, IMainViewModel
    {
        #region Fields

        private static IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();

        private IDebugWriter _debugWriter = new DebugWriter(s =>
                                                                     Application.Current.Dispatcher.BeginInvoke(
                                                                         DispatcherPriority.Normal,
                                                                         new Action(
                                                                             () =>
                                                                             Mediator.SendMessage(
                                                                                 MediatorMessages.DebugWriterAppend, s))));

        private ILayoutObjectViewModel _activeCell;
        private IEnvironmentModel _activeEnvironment;
        private ILayoutGridViewModel _activePage;
        private ICommand _addStudioShortcutsPageCommand;

        private RelayCommand _debugCommand;
        private RelayCommand _deployAllCommand;
        private RelayCommand _deployCommand;
        private ICommand _displayAboutDialogueCommand;
        private RelayCommand _editResourceCommand;
        private RelayCommand _exitCommand;
        private RelayCommand<string> _newResourceCommand;
        private ICommand _notImplementedCommand;
        private RelayCommand<string> _openWebsiteCommand;
        private string _outputMessage;
        private RelayCommand _runCommand;
        private RelayCommand _saveCommand;
        private dynamic _selectedTab;
        private ICommand _startFeedbackCommand;
        private ICommand _startStopRecordedFeedbackCommand;
        private RelayCommand _viewInBrowserCommand;

        #endregion

        #region Events

        //public event RequestCreateResourceEventHandler OnRequestNewResource;
        public event ResourceEventHandler OnApplicationExitRequest;
        //public event ResourceEventHandler OnRequestEditResourceMetadata;

        #endregion

        #region Dependencies

        [Import]
        public Lazy<IUserInterfaceLayoutProvider> UserInterfaceLayoutProvider { get; set; }

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        [Import]
        public IWebCommunication WebCommunication { get; set; }

        #endregion

        #region Ctor

        public MainViewModel()
        {
            Mediator.RegisterToReceiveMessage(MediatorMessages.BuildResource,
                                              o => Build(o as IContextualResourceModel, true));
            Mediator.RegisterToReceiveMessage(MediatorMessages.SaveResource,
                                              o => Save(o as IContextualResourceModel, true));
            Mediator.RegisterToReceiveMessage(MediatorMessages.DebugResource, o => Debug(o as IContextualResourceModel));
            Mediator.RegisterToReceiveMessage(MediatorMessages.ExecuteResource, o => Run(o as IContextualResourceModel));
            Mediator.RegisterToReceiveMessage(MediatorMessages.SetActiveEnvironment,
                                              o => ActiveEnvironment = o as IEnvironmentModel);
            Mediator.RegisterToReceiveMessage(MediatorMessages.SetActivePage,
                                              o => SetActivePage(o as ILayoutObjectViewModel));
        }

        #endregion

        #region Private Methods

        #endregion Private Methods

        #region Private Properties

        private IEnvironmentModel ActiveEnvironment
        {
            get { return _activeEnvironment; }
            set
            {
                if (value != null)
                {
                    _activeEnvironment = value;
                }

                OnPropertyChanged("CanSave");
                OnPropertyChanged("CanDebug");
            }
        }

        private object CurrentViewModel
        {
            get
            {
                if (UserInterfaceLayoutProvider != null && UserInterfaceLayoutProvider.Value != null &&
                    UserInterfaceLayoutProvider.Value.ActiveDocumentDataContext != null)
                {
                    return UserInterfaceLayoutProvider.Value.ActiveDocumentDataContext;
                }
                return null;
            }
        }

        private IContextualResourceModel CurrentResourceModel
        {
            get
            {
                if (UserInterfaceLayoutProvider != null && UserInterfaceLayoutProvider.Value != null &&
                    UserInterfaceLayoutProvider.Value.ActiveDocumentDataContext != null)
                {
                    return UserInterfaceLayoutProvider.Value.GetContextualResourceModel(CurrentViewModel);
                }
                return null;
            }
        }

        #endregion Private Properties

        #region Properties  

        [Import]
        public IDev2WindowManager WindowNavigation { get; set; }

        [Import]
        public IFeedbackInvoker FeedbackInvoker { get; set; }

        [Import]
        public IFeedBackRecorder FeedBackRecorder { get; set; }

        [Import]
        public IFrameworkRepository<IDataListViewModel> DataListRepository { get; set; }

        [Import]
        public IFrameworkRepository<UserInterfaceLayoutModel> UserInterfaceLayoutRepository { get; set; }

        public IDataListViewModel ActiveDataList { get; set; }

        public dynamic SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                base.OnPropertyChanged("SelectedTab");
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
            get
            {
                if (CurrentViewModel != null)
                {
                    dynamic viewModel = CurrentViewModel as IWorkflowDesignerViewModel;
                    if (viewModel != null)
                    {
                        return IsActiveEnvironmentConnected();
                    }

                    viewModel = CurrentViewModel as IResourceDesignerViewModel;
                    if (viewModel != null)
                    {
                        return IsActiveEnvironmentConnected();
                    }
                }
                return false;
            }
        }

        public bool CanDebug
        {
            get
            {
                dynamic viewModel = CurrentViewModel as IWorkflowDesignerViewModel;
                if (viewModel != null)
                {
                    return IsActiveEnvironmentConnected();
                }
                return false;
            }
        }

        public string OutputMessage
        {
            get
            {
                if (_outputMessage == null)
                {
                    _outputMessage = String.Empty;
                }
                return _outputMessage;
            }
            set
            {
                _outputMessage = value;
                base.OnPropertyChanged("OutputMessage");
            }
        }

        #endregion

        #region Commands

        public ICommand ThrowExceptionCommand
        {
            get
            {
                return new RelayCommand((p) =>
                    {
                        try
                        {
                            throw new ArgumentNullException("Test Message");
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Testing inner exception", e);
                        }
                    });
            }
        }

        public ICommand NotImplementedCommand
        {
            get
            {
                if (_notImplementedCommand == null)
                    {
                    _notImplementedCommand = new RelayCommand(param => { MessageBox.Show("Please Implement Me!"); });
                        }
                return _notImplementedCommand;
                    }
            }

        public ICommand AddStudioShortcutsPageCommand
        {
            get
            {
                if (_addStudioShortcutsPageCommand == null)
                {
                    _addStudioShortcutsPageCommand = new RelayCommand(param => { AddStudioShortcutKeysPage(); });
        }
                return _addStudioShortcutsPageCommand;
            }
        }

        public ICommand DisplayAboutDialogueCommand
        {
            get
            {
                if (_displayAboutDialogueCommand == null)
                {
                    _displayAboutDialogueCommand = new RelayCommand(param => { DisplayAboutDialogue(); });
                }
                return _displayAboutDialogueCommand;
            }
        }
        
        public ICommand StartFeedbackCommand
        {
            get
            {
                if (_startFeedbackCommand == null)
                {
                    _startFeedbackCommand = new RelayCommand(param => { StartFeedback(); });
                }
                return _startFeedbackCommand;
            }
        }

        public ICommand StartStopRecordedFeedbackCommand
        {
            get
            {
                if (_startStopRecordedFeedbackCommand == null)
                {
                    _startStopRecordedFeedbackCommand = new RelayCommand(param => { StartStopRecordedFeedback(); });
                }
                return _startStopRecordedFeedbackCommand;
            }
        }

        public ICommand DeployAllCommand
        {
            get
            {
                if (_deployAllCommand == null)
                {
                    _deployAllCommand = new RelayCommand(param =>
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

                            Mediator.SendMessage(MediatorMessages.DeployResources, payload);
                        });
        }
                return _deployAllCommand;
            }
        }

        public ICommand OpenWebsiteCommand
        {
            get
            {
                return _openWebsiteCommand ??
                       (_openWebsiteCommand = new RelayCommand<string>((c) =>
                {
                               IEnumerable<IResourceModel> match =
                                   ActiveEnvironment.Resources.All()
                                                    .Where(
                                                        res =>
                                                        res.ResourceName.Equals(c,
                                                                                StringComparison
                                                                                    .InvariantCultureIgnoreCase));
                               if (match.Any())
                               {
                                   AddWorkflowDocument(match.First());
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
                    _viewInBrowserCommand = new RelayCommand(param => ViewInBrowser(),
                                                             param =>
                                                             ((CurrentViewModel is IWorkflowDesignerViewModel ||
                                                               CurrentViewModel is LayoutGridViewModel ||
                                                               CurrentViewModel is WebsiteEditorViewModel) &&
                                                              IsActiveEnvironmentConnected()));
                }
                return _viewInBrowserCommand;
            }
        }

        //public ICommand ViewStartPageCommand
        //{
        //    get
        //    {
        //        if (_viewStartPageCommand == null)
        //        {
        //            _viewStartPageCommand = new RelayCommand(param => AddStartTabs(), param => true);
        //        }
        //        return _viewStartPageCommand;
        //    }
        //}

        public RelayCommand<string> NewResourceCommand
        {
            get
            {
                if (_newResourceCommand == null)
                {
                    _newResourceCommand = new RelayCommand<string>(AddNewResource,
                                                                   param => IsChosenEnvironmentConnected());
                }
                return _newResourceCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand =
                        new RelayCommand(
                            param => { if (OnApplicationExitRequest != null) OnApplicationExitRequest(null); },
                            param => true);
                }
                return _exitCommand;
            }
        }

        public ICommand EditCommand
        {
            get
            {
                if (_editResourceCommand == null)
                {
                    _editResourceCommand = new RelayCommand(param => Edit(),
                                                            param =>
                                                            (SecurityContext.IsUserInRole(new[]
                {
                                                                    StringResources.BDSAdminRole,
                                                                    StringResources.BDSDeveloperRole,
                                                                    StringResources.BDSTestingRole
                                                                }) &&
                                                             IsActiveEnvironmentConnected()));
                }
                return _editResourceCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(param => Save(CurrentResourceModel), param => CanSave);
                }
                return _saveCommand;
            }
        }

        public ICommand DeployCommand
        {
            get
            {
                if (_deployCommand == null)
                {
                    //_deployCommand = new RelayCommand(param => Mediator.SendMessage(MediatorMessages.DeployResources, null));
                    _deployCommand = new RelayCommand(param =>
                    {
                        object payload = null;

                        if (CurrentResourceModel != null)
                        {
                            payload = CurrentResourceModel;
                        }

                        Mediator.SendMessage(MediatorMessages.DeployResources, payload);
                    });
                }
                return _deployCommand;
            }
        }

        public ICommand DebugCommand
        {
            get
            {
                if (_debugCommand == null)
                {
                    _debugCommand = new RelayCommand(param => Debug(CurrentResourceModel), param => CanDebug);
                }
                return _debugCommand;
            }
        }

        public ICommand RunCommand
        {
            get
            {
                if (_runCommand == null)
                {
                    _runCommand = new RelayCommand(param => Run((CurrentViewModel as dynamic).ResourceModel),
                                                   param => CanSave);
                }
                return _runCommand;
            }
        }

        public bool IsChosenEnvironmentConnected()
        {
            // Used for enabling / disabling basic server commands (Eg: Creating a new Workflow)
            if (ActiveEnvironment == null)
            {
                return false;
            }
            else
            {
                return ((ActiveEnvironment != null) && (ActiveEnvironment.IsConnected));
            }
        }

        public bool IsActiveEnvironmentConnected()
        {
            // Used for enabling / disabling commands based off the server status of the current tab (Eg: View in Browser)
            if (CurrentResourceModel == null)
            {
                return false;
            }
            else
            {
                return ((CurrentResourceModel.Environment != null) && (CurrentResourceModel.Environment.IsConnected));
            }
        }

        #endregion

        #region Public Methods

        public void AddNewResource(string resourceType)
        {
            Mediator.SendMessage(MediatorMessages.ShowNewResourceWizard,
                                 new Tuple<IEnvironmentModel, string>(ActiveEnvironment, resourceType));
        }

        public void Save(IContextualResourceModel resource, bool showWindow = true)
        {
            if (resource != null)
            {
                Build(resource, showWindow);

                IResourceModel resourceToUpdate = resource.Environment.Resources.FindSingle(c => c.ResourceName.Equals(resource.ResourceName, StringComparison.CurrentCultureIgnoreCase));
                resourceToUpdate.Update(resource);

                var workspaceItem = UserInterfaceLayoutProvider.Value.WorkspaceItems.FirstOrDefault(wi => wi.ServiceName == resource.ResourceName);
                if(workspaceItem == null)
                {
                    return;
                }

                workspaceItem.Action = WorkspaceItemAction.Commit;

                dynamic publishRequest = new UnlimitedObject();
                publishRequest.Service = "UpdateWorkspaceItemService";
                publishRequest.Roles = String.Join(",", SecurityContext.Roles);
                publishRequest.ItemXml = workspaceItem.ToXml();

                string result = resource.Environment.DsfChannel.ExecuteCommand(publishRequest.XmlString,
                                                                               workspaceItem.WorkspaceID,
                                                                               GlobalConstants.NullDataListID);

                if (result == null)
                {
                    result = string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, publishRequest.Service);
                }

                var sb = new StringBuilder();
                sb.AppendLine(String.Format("<Save StartDate=\"{0}\">",
                                            DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                sb.AppendLine(result);
                sb.AppendLine(String.Format("</Save>"));
                OutputMessage += sb.ToString();
                DisplayOutput(OutputMessage, showWindow);

                Mediator.SendMessage(MediatorMessages.UpdateDeploy, null);
            }
        }

        public void ViewInBrowser()
        {
            Mediator.SendMessage(MediatorMessages.FindMissingDataListItems, ActiveDataList);

            IContextualResourceModel resourceModel = CurrentResourceModel;

            // 2012.10.17 - 5782: TWR - Use build first so that changes are persisted to the client workspace on the server
            Build(resourceModel, true);

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
            ViewModelDialogResults viewModelDialogResults =
                UserInterfaceLayoutProvider.Value.GetServiceInputDataFromUser(debugInfoModel, out debugTO);

            if (viewModelDialogResults == ViewModelDialogResults.Okay)
            {
                PerformDebugTask(debugTO, resourceModel.Environment);
            }
        }

        public void AddHelpDocument(IResourceModel resource)
        {
            Mediator.SendMessage(MediatorMessages.AddHelpDocument, resource);
        }

        public void AddWorkflowDocument(IResourceModel resource)
        {
            Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, resource);
        }

        public void AddResourceDocument(IResourceModel resource)
        {
            Mediator.SendMessage(MediatorMessages.AddResourceDocument, resource);
        }

        public void AddDependencyVisualizerDocument(IResourceModel resource)
        {
            Mediator.SendMessage(MediatorMessages.ShowDependencyGraph, resource);
        }

        public void LoadExplorerPage()
        {
            Mediator.SendMessage(MediatorMessages.ShowExplorer, this);
        }

        public void Debug(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null || resourceModel.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }

            // Clear output
            Mediator.SendMessage(MediatorMessages.DebugWriterWrite, string.Empty);

            Mediator.SendMessage(MediatorMessages.BindViewToViewModel, resourceModel);

            IServiceDebugInfoModel debugInfoModel =
                ServiceDebugInfoModelFactory.CreateServiceDebugInfoModel(resourceModel, string.Empty, 0,
                                                                         DebugMode.DebugInteractive);

            DebugTO debugTO;
            ViewModelDialogResults viewModelDialogResults =
                UserInterfaceLayoutProvider.Value.GetServiceInputDataFromUser(debugInfoModel, out debugTO);

            if (viewModelDialogResults == ViewModelDialogResults.Okay)
            {
                var clientContext = resourceModel.Environment.DsfChannel as IStudioClientContext;
                if (clientContext != null)
                {
                    clientContext.AddDebugWriter(_debugWriter);

                    // 2012.10.17 - 4423: TWR - Use build first so that changes are persisted to the client workspace on the server
                    Build(resourceModel, true);

                    XElement dataList = XElement.Parse(debugTO.XmlData);
                    dataList.Add(new XElement("BDSDebugMode", debugTO.IsDebugMode));


                    // Sashen.Naidoo : 14-02-2012 : BUG 8793 : Added asynchronous callback to remove the debugwriter when the the Webserver callback has completed.
                    //                              Previously, everytime the debug method was invoked it would add a debug writer to the clientcontext and
                    //                              this would never be removed

                    Action<UploadStringCompletedEventArgs> webserverCallback = asyncCallback => clientContext.RemoveDebugWriter(_debugWriter); 
                    
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
            Mediator.SendMessage(MediatorMessages.DebugWriterWrite, string.Empty);

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

            sb.AppendLine(String.Format("<Build StartDate=\"{0}\">", DateTime.Now.ToString()));

            Mediator.SendMessage(MediatorMessages.BindViewToViewModel, model);

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

            string result = model.Environment.DsfChannel.ExecuteCommand(buildRequest.XmlString, workspaceID,
                                                                      GlobalConstants.NullDataListID);
            if (result == null)
            {

                result = string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, buildRequest.Service);
            }

            sb.AppendLine(result);      
            sb.AppendLine(String.Format("</Build>"));
            OutputMessage += sb.ToString();
            DisplayOutput(OutputMessage, showWindow);
        }

        public void DisplayOutput(string message, bool popupWindow = true)
        {
            Mediator.SendMessage(MediatorMessages.DebugWriterAppend, message);
        }

        public void SetActivePage(ILayoutObjectViewModel cell)
        {
            if (cell != null)
            {
                _activePage = cell.LayoutObjectGrid;
                _activeCell = cell;
                base.OnPropertyChanged("ActivePage");
            }
        }

        public void AddStartTabs()
        {
            Mediator.SendMessage(MediatorMessages.ShowStartTabs, this);
        }

        private void Edit()
        {
            if (CurrentResourceModel != null)
            {
                Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, CurrentResourceModel);
            }
        }

        public void PerformDebugTask(DebugTO debugTO, IEnvironmentModel environment)
        {
            // Starting new debug session so clear output
            Mediator.SendMessage(MediatorMessages.DebugWriterWrite, string.Empty);

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
                    throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, dataObject.Service));
                }

                DisplayOutput(msg);
            }
            else
            {
                UserInterfaceLayoutProvider.Value.StartDebuggingSession(debugTO, environment);
            }
        }

        public void AddStudioShortcutKeysPage()
        {
            Mediator.SendMessage(MediatorMessages.AddStuidoShortcutKeysPage, this);
        }

        public void DisplayAboutDialogue()
        {
            Mediator.SendMessage(MediatorMessages.DisplayAboutDialogue, this);
        }

        public void StartFeedback()
        {
            var emailFeedbackAction = new EmailFeedbackAction();
            ImportService.SatisfyImports(emailFeedbackAction);
            
            var recorderFeedbackAction = new RecorderFeedbackAction();
            ImportService.SatisfyImports(recorderFeedbackAction);

            FeedbackInvoker.InvokeFeedback(emailFeedbackAction, recorderFeedbackAction); 
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
        
        #endregion

        #region Protected Methods

        protected override void SatisfyImports()
        {
            // Overridden to prevent the base view model form trying to automatically satisfy the imports on construction.
            // This is done so that an instance of the main view model can be created from the ImportService using the 
            // GetExportedValue. That is done so that all other imports of the main view model will utilize the instance
            // being managed by the ImportService.
        }

        #endregion Protected Methods
    }
}
