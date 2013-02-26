using System;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Caliburn.Micro;
using CircularDependencyTool;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Data.SystemTemplates;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Session;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Converters;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Administration;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.DataList;
using Dev2.Studio.Core.ViewModels.Navigation;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Studio.InterfaceImplementors.WizardResourceKeys;
using Dev2.Studio.ViewModels.Deploy;
using Dev2.Studio.ViewModels.Explorer;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views.Administration;
using Dev2.Studio.Views.DataList;
using Dev2.Studio.Views.Deploy;
using Dev2.Studio.Views.Explorer;
using Dev2.Studio.Views.ResourceManagement;
using Dev2.Studio.Views.UserInterfaceBuilder;
using Dev2.Studio.Views.Workflow;
using Dev2.Studio.Webs;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager;
using Unlimited.Applications.BusinessDesignStudio;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Views;
using Unlimited.Applications.BusinessDesignStudio.Views.WebsiteBuilder;
using Unlimited.Framework;
using Unlimited.Framework.Workspaces;
using Action = System.Action;
using System.Linq;

namespace Dev2.Studio
{
    [Export(typeof (IUserInterfaceLayoutProvider))]
    public class UserInterfaceLayoutProvider : PropertyChangedBase, IUserInterfaceLayoutProvider
            {
        #region Class Members

        private FrameworkElement _activeDocument;
        private WebPropertyEditorWindow _win;

        #endregion Class Members

        #region Properties

        [Import]
        public IWizardEngine WizardEngine { get; set; }

        [Import]
        public IResourceDependencyService ResourceDependencyService { get; set; }

        [Import]
        public IMainViewModel MainViewModel { get; set; }

        [Import]
        public IWebCommunication WebCommunication { get; set; }

        [Import]
        public IPopUp PopupProvider { get; set; }

        [Import]
        public IFrameworkRepository<IEnvironmentModel> EnvironmentRepository { get; set; }

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        public TabActionContext ActiveDocumentTabAction
        {
            get
            {
                if (ActiveDocument == null)
                    return TabActionContext.Unknown;

                var uielement = ActiveDocument as UIElement;
                return uielement == null
                           ? TabActionContext.Unknown
                           : UIElementTabActionContext.GetTabActionContext(uielement);
            }
        }

        public IList<IWorkspaceItem> WorkspaceItems { get; private set; }

        public TabControl Manager { get; set; }

        public FrameworkElement ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                _activeDocument = value;
                Mediator.SendMessage(MediatorMessages.TabContextChanged, ActiveDocumentDataContext);
                NotifyOfPropertyChange(() => ActiveDocument);
                NotifyOfPropertyChange(() => ActiveDocumentDataContext);
                NotifyOfPropertyChange(() => ActiveDocumentTabAction);
            }
        }

        public object ActiveDocumentDataContext
        {
            get
            {
                FrameworkElement document = ActiveDocument;
                return document == null ? null : document.DataContext;
            }
        }

        public ObservableCollection<FrameworkElement> Tabs { get; set; }

        public ContentControl PropertyPane { get; set; }

        public ContentControl OutputPane { get; set; }

        public ContentControl NavigationPane { get; set; }

        public ContentControl DataMappingPane { get; set; }

        public ContentControl DataListPane { get; set; }

        #endregion Properties

        #region Constructor

        public UserInterfaceLayoutProvider()
        {
            LoadWorkspaceItems();

            Tabs = new ObservableCollection<FrameworkElement>();
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowStartTabs, AddStartTabs);
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddStuidoShortcutKeysPage, AddShortcutKeysPage);
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowExplorer, ShowExplorer);
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWorkflowDesigner, AddWorkflowDesigner);
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowWebpartWizard,
                                              input => ShowWebpartWizard(input as IPropertyEditorWizard));
            Mediator.RegisterToReceiveMessage(MediatorMessages.CloseWizard, CloseWizard);
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowNewResourceWizard, ShowNewResourceWizard);
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowEditResourceWizard, ShowEditResourceWizard);
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddHelpDocument, AddHelpDocument);
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWebpageDesigner,
                                              input => AddWebPageDesigner(input as IWebActivity));
            Mediator.RegisterToReceiveMessage(MediatorMessages.AddWebsiteDesigner,
                                              input => AddWebsiteDesigner(input as IWebActivity));
            Mediator.RegisterToReceiveMessage(MediatorMessages.BindViewToViewModel,
                                              input => BindViewToViewModel(input as IResourceModel));
            Mediator.RegisterToReceiveMessage(MediatorMessages.SaveResourceModel,
                                              input => SaveResourceModel(input as IContextualResourceModel));
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowDependencyGraph,
                                              input => ShowDependencyGraph(input as IContextualResourceModel));
            Mediator.RegisterToReceiveMessage(MediatorMessages.ConfigureDecisionExpression,
                                              input =>
                                              ConfigureDecisionExpression(input as Tuple<ModelItem, IEnvironmentModel>));
            Mediator.RegisterToReceiveMessage(MediatorMessages.ConfigureSwitchExpression,
                                              input =>
                                              ConfigureSwitchExpression(input as Tuple<ModelItem, IEnvironmentModel>));
            Mediator.RegisterToReceiveMessage(MediatorMessages.ConfigureCaseExpression,
                                              input =>
                                              ConfigureSwitchCaseExpression(input as Tuple<ModelItem, IEnvironmentModel>));
            // 28.01.2013 - Travis.Frisinger : Added Edit Case Functionality
            Mediator.RegisterToReceiveMessage(MediatorMessages.EditCaseExpression,
                                              input =>
                                              EditSwitchCaseExpression(input as Tuple<ModelProperty, IEnvironmentModel>));
            Mediator.RegisterToReceiveMessage(MediatorMessages.WorkflowActivitySelected,
                                              input => WorkflowActivitySelected(input as IWebActivity));
            Mediator.RegisterToReceiveMessage(MediatorMessages.RemoveDataMapping, input => RemoveDataMapping());
            Mediator.RegisterToReceiveMessage(MediatorMessages.TabContextChanged, TabContextChanged);
            Mediator.RegisterToReceiveMessage(MediatorMessages.DisplayAboutDialogue, DisplayAboutDialog);
            Mediator.RegisterToReceiveMessage(MediatorMessages.DeleteServiceExplorerResource,
                                              DeleteServiceExplorerResource);
            Mediator.RegisterToReceiveMessage(MediatorMessages.DeleteSourceExplorerResource,
                                              DeleteSourceExplorerResource);
            Mediator.RegisterToReceiveMessage(MediatorMessages.DeleteWorkflowExplorerResource,
                                              DeleteWorkflowExplorerResource);
            Mediator.RegisterToReceiveMessage(MediatorMessages.DeployResources, AddDeployResources);
            Mediator.RegisterToReceiveMessage(MediatorMessages.SaveWorkspaceItems, input => SaveWorkspaceItems());
            Mediator.RegisterToReceiveMessage(MediatorMessages.ShowHelpTab, input => ShowHelpTab(input as string));
        }

        #endregion Constructor

        #region public methods

        public void StartDebuggingSession(DebugTO input, IEnvironmentModel environment)
        {
            var contentDocument = new WorkflowDebuggerWindow(environment.DsfChannel, input.ServiceName,
                                                             input.WorkflowXaml, input.XmlData, input.DataList);
            UIElementTitleProperty.SetTitle(contentDocument, input.ServiceName + " *Debug");

            contentDocument.OnOutputMessageReceived += delegate(string message)
                {
                    Mediator.SendMessage(MediatorMessages.DebugWriterAppend, message);

                    if (!message.Equals("</DebugData>")) return;

                    Tabs.Remove(contentDocument);

                    FrameworkElement documentToSetActiveAfterDebugging = FindTabByName(input.ServiceName);
                    if (documentToSetActiveAfterDebugging != null)
                    {
                        SetActiveDocument(documentToSetActiveAfterDebugging);
                    }
                };

            foreach (FrameworkElement item in Tabs)
            {
                if (UIElementTitleProperty.GetTitle(item)
                                          .Equals(UIElementTitleProperty.GetTitle(contentDocument),
                                                  StringComparison.InvariantCultureIgnoreCase))
                {
                    Tabs.Remove(item);
                }
            }
            Tabs.Add(contentDocument);
            SetActiveDocument(contentDocument);
            //2012.10.11: massimo.guerrera - Added for debug PBI 5781
            contentDocument.RunDebugger(0);
            //contentDocument.RunDebugger(input.WaitTimeForTransition);
        }

        public ViewModelDialogResults GetServiceInputDataFromUser(IServiceDebugInfoModel input, out DebugTO debugTO)
        {
            EventAggregator.Publish(new AddMissingAndFindUnusedDataListItemsMessage());

            var inputData = new WorkflowInputDataWindow();

            debugTO = new DebugTO
            {
                DataList = !string.IsNullOrEmpty(input.ResourceModel.DataList) 
                ? input.ResourceModel.DataList : "<DataList></DataList>",//Bug 8363 & Bug 8018
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
        ///     Removes the document passed in from the tab collection
        /// </summary>
        /// <param name="document">The Document that will be removed</param>
        /// <returns>If the document has been removed successfully</returns>
        public bool RemoveDocument(object document)
        {
            bool removeTab = false;
            var documentToRemove = document as FrameworkElement;
            if (documentToRemove != null)
            {
                if (documentToRemove.DataContext is IWorkflowDesignerViewModel)
                {
                    var viewModel = documentToRemove.DataContext as IWorkflowDesignerViewModel;
                    //19.10.2012: massimo.guerrera - Added for PBI 5782
                    bool dontShowPopup = viewModel.ResourceModel.IsWorkflowSaved(viewModel.ServiceDefinition);
                    if (!dontShowPopup)
                    {
                        //TODO - Call to get all the errors for this workflow
                        string errorMessages = string.Empty;

                        IPopUp pop = new PopUp(
                            "Workflow not saved...",
                            string.Concat(
                                "The workflow that you are closing is not saved.\r\nWould you like to save the  workflow?\r\n-------------------------------------------------------------------\r\nClicking Yes will save the workflow\r\nClicking No will discard your changes\r\nClicking Cancel will return you to the workflow",
                                errorMessages),
                            MessageBoxImage.Information,
                            MessageBoxButton.YesNoCancel
                            );
                        MessageBoxResult res = pop.Show();
                        switch (res)
                        {
                            case MessageBoxResult.No:
                                removeTab = true;
                                break;
                            case MessageBoxResult.Yes:
                                Mediator.SendMessage(MediatorMessages.SaveResource, viewModel.ResourceModel);
                                removeTab = true;
                                break;
                        }
                    }
                    else
                    {
                        removeTab = true;
                    }
                    if (removeTab)
                    {
                        if (ActiveDocument != null && ActiveDocument.Equals(documentToRemove))
                        {
                            RemoveDataList();
                        }
                        Tabs.Remove(documentToRemove);
                        RemoveWorkspaceItem(viewModel);
                        viewModel.Dispose();
                    }
                }
                else
                {
                    if (ActiveDocument.Equals(documentToRemove))
                    {
                        RemoveDataList();
                    }
                    Tabs.Remove(documentToRemove);
                    removeTab = true;
                }
            }
            return removeTab;
        }

        /// <summary>
        ///     Gets all IContextualResource models from the open tabs.
        /// </summary>
        /// <returns></returns>
        public List<IContextualResourceModel> GetOpenContextualResourceModels()
        {
            return Tabs
                .Select(item => ResourceHelper.GetContextualResourceModel(item.DataContext))
                .Where(resourceModel => resourceModel != null)
                .ToList();
        }

        /// <summary>
        ///     Saves all open tabs locally and writes the open tabs the to collection of workspace items
        /// </summary>
        public void PersistTabs()
        {
            SaveWorkspaceItems();
            foreach (IContextualResourceModel resourceModel in GetOpenContextualResourceModels())
            {
                Mediator.SendMessage(MediatorMessages.BuildResource, resourceModel);
            }
        }

        public void PersistTabs(ItemCollection tabcollection)
        {
            var tmpTabs = new ObservableCollection<FrameworkElement>();
            var tmpWorkspaceItems = new List<IWorkspaceItem>();

            foreach (object item in tabcollection)
            {
                var tab = item as ContentPane;
                if (tab == null) continue;

                IWorkspaceItem tmpItem = WorkspaceItems.FirstOrDefault(c => c.ServiceName == tab.TabHeader.ToString());
                if (tmpItem != null)
                {
                    tmpWorkspaceItems.Add(tmpItem);
                }
                tmpTabs.Add(FindTabByName(tab.TabHeader.ToString()));
            }
            WorkspaceItems = tmpWorkspaceItems;
            Tabs = tmpTabs;
            SaveWorkspaceItems();

            foreach (IContextualResourceModel resourceModel in Tabs
                .Select(item => ResourceHelper.GetContextualResourceModel(item.DataContext))
                .Where(resourceModel => resourceModel != null))
            {
                Mediator.SendMessage(MediatorMessages.BuildResource, resourceModel);
            }
        }

        public FrameworkElement FindTabByResourceModel(IResourceModel resource)
        {
            return
                Tabs.FirstOrDefault(
                    tab => UIElementTitleProperty.GetTitle(tab).Equals(resource.ResourceName,
                                                                       StringComparison.InvariantCultureIgnoreCase));
        }

        public bool TabExists(IResourceModel resource)
        {
            return
                Tabs.Any(
                    tab =>
                    UIElementTitleProperty.GetTitle(tab)
                                          .Equals(resource.ResourceName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void SetActiveTab(IResourceModel resource)
        {
            FrameworkElement tabToSetActive = Tabs.FirstOrDefault(
                tab =>
                UIElementTitleProperty.GetTitle(tab)
                                      .Equals(resource.ResourceName, StringComparison.InvariantCultureIgnoreCase));

            SetActiveDocument(tabToSetActive);
        }

        public FrameworkElement FindTabByName(string tabName)
        {
            return
                Tabs.FirstOrDefault(
                    tab =>
                    UIElementTitleProperty.GetTitle(tab). Equals(tabName, StringComparison.InvariantCultureIgnoreCase));
        }

        public FrameworkElement FindTabByContext(string tabName)
        {
            return
                Tabs.FirstOrDefault(
                    tab =>
                    UIElementTitleProperty.GetTitle(tab).Equals(tabName, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion

        #region private methods

        private void DeleteServiceExplorerResource(object state)
        {
            if (!(state is KeyValuePair<ITreeNode, IContextualResourceModel>)) return;

            var kvp = (KeyValuePair<ITreeNode, IContextualResourceModel>) state;
            IContextualResourceModel model = kvp.Value;
            List<IResourceModel> dependencies = ResourceDependencyService.GetUniqueDependencies(model);
            bool openDependencyGraph;
            bool shouldRemove = QueryDeleteExplorerResource(model.ResourceName, "Service",
                                                            dependencies != null && dependencies.Count > 0,
                                                            out openDependencyGraph);

            if (shouldRemove)
            {
                if (!WizardEngine.IsWizard(model))
                {
                    IContextualResourceModel wizard = WizardEngine.GetWizard(model);
                    if (wizard != null)
                    {
                        UnlimitedObject wizardData = ExecuteDeleteResource(wizard, "Service",
                                                                           String.Join(",",
                                                                                       MainViewModel.SecurityContext
                                                                                                    .Roles));

                        if (wizardData.HasError)
                        {
                            HandleDeleteResourceError(wizardData, model.ResourceName, "Service");
                            return;
                        }
                    }
                }

                UnlimitedObject data = ExecuteDeleteResource(model, "Service",
                                                             String.Join(",", MainViewModel.SecurityContext.Roles));

                if (data.HasError)
                {
                    HandleDeleteResourceError(data, model.ResourceName, "Service");
                }
                else
                {
                    RemoveExplorerResource(model, kvp.Key);
                }
            }
            else if (openDependencyGraph)
            {
                MainViewModel.AddDependencyVisualizerDocument(model);
            }
        }

        private void DeleteSourceExplorerResource(object state)
        {
            if (state is KeyValuePair<ITreeNode, IContextualResourceModel>)
            {
                var kvp = (KeyValuePair<ITreeNode, IContextualResourceModel>) state;
                IContextualResourceModel model = kvp.Value;
                List<IResourceModel> dependencies = ResourceDependencyService.GetUniqueDependencies(model);
                bool openDependencyGraph;
                bool shouldRemove = QueryDeleteExplorerResource(model.ResourceName, "Source",
                                                                dependencies != null && dependencies.Count > 0,
                                                                out openDependencyGraph);

                if (shouldRemove)
                {
                    if (!WizardEngine.IsWizard(model))
                    {
                        IContextualResourceModel wizard = WizardEngine.GetWizard(model);
                        if (wizard != null)
                        {
                            dynamic wizardData = ExecuteDeleteResource(wizard, "Source",
                                                                       String.Join(",",
                                                                                   MainViewModel.SecurityContext.Roles));

                            if (wizardData.HasError)
                            {
                                HandleDeleteResourceError(wizardData, model.ResourceName, "Source");
                                return;
                            }
                        }
                    }

                    dynamic data = ExecuteDeleteResource(model, "Source",
                                                         String.Join(",", MainViewModel.SecurityContext.Roles));

                    if (data.HasError)
                    {
                        HandleDeleteResourceError(data, model.ResourceName, "Source");
                    }
                    else
                    {
                        RemoveExplorerResource(model, kvp.Key);
                    }
                }
                else if (openDependencyGraph)
                {
                    MainViewModel.AddDependencyVisualizerDocument(model);
                }
            }
        }

        private void DeleteWorkflowExplorerResource(object state)
        {
            if (!(state is KeyValuePair<ITreeNode, IContextualResourceModel>)) return;

            var kvp = (KeyValuePair<ITreeNode, IContextualResourceModel>) state;
            IContextualResourceModel model = kvp.Value;
            List<IResourceModel> dependencies = ResourceDependencyService.GetUniqueDependencies(model);
            bool openDependencyGraph;
            bool shouldRemove = QueryDeleteExplorerResource(model.ResourceName, "Workflow",
                                                            dependencies != null && dependencies.Count > 0,
                                                            out openDependencyGraph);

            if (shouldRemove)
            {
                if (!WizardEngine.IsWizard(model))
                {
                    IContextualResourceModel wizard = WizardEngine.GetWizard(model);
                    if (wizard != null)
                    {
                        UnlimitedObject wizardData = ExecuteDeleteResource(wizard, "WorkflowService",
                                                                           String.Join(",",
                                                                                       MainViewModel.SecurityContext
                                                                                                    .Roles));

                        if (wizardData.HasError)
                        {
                            HandleDeleteResourceError(wizardData, model.ResourceName, "WorkflowService");
                            return;
                        }
                    }
                }

                UnlimitedObject data = ExecuteDeleteResource(model, "WorkflowService",
                                                             String.Join(",", MainViewModel.SecurityContext.Roles));

                if (data.HasError)
                {
                    HandleDeleteResourceError(data, model.ResourceName, "WorkflowService");
                }
                else
                {
                    RemoveExplorerResource(model, kvp.Key);
                }
            }
            else if (openDependencyGraph)
            {
                MainViewModel.AddDependencyVisualizerDocument(model);
            }
        }

        private void HandleDeleteResourceError(dynamic data, string resourceName, string resourceType)
        {
            if (data.HasError)
            {
                MessageBox.Show(Application.Current.MainWindow, resourceType + " \"" + resourceName +
                                                                "\" could not be deleted, reason: " + data.Error,
                                resourceType + " Deletion Failed", MessageBoxButton.OK);
            }
        }

        private void RemoveExplorerResource(IContextualResourceModel model, ITreeNode navItemVM)
        {
            FrameworkElement resourceTab;

            while ((resourceTab = FindTabByResourceModel(model)) != null)
            {
                Tabs.Remove(resourceTab);
                }

            var vm = navItemVM as AbstractTreeViewModel;

            if (vm != null)
            {
                AbstractTreeViewModel itemVm = vm;

                if (itemVm.TreeParent != null)
                {
                    itemVm.TreeParent.Children.Remove(itemVm);
                    itemVm.Dispose();
                }
            }

            model.Environment.Resources.Remove(model);
        }

        private UnlimitedObject ExecuteDeleteResource(IContextualResourceModel resource, string resourceType,
                                                      string roles)
        {
            dynamic request = new UnlimitedObject();
            request.Service = "DeleteResourceService";
            request.ResourceName = resource.ResourceName;
            request.ResourceType = resourceType;
            request.Roles = roles;
            Guid workspaceID = ((IStudioClientContext) resource.Environment.DsfChannel).AccountID;

            string result = resource.Environment.DsfChannel.ExecuteCommand(request.XmlString, workspaceID,
                                                                           GlobalConstants.NullDataListID);
            if (result == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, request.Service));
            }

            return UnlimitedObject.GetStringXmlDataAsUnlimitedObject(result);
        }

        private bool QueryDeleteExplorerResource(string resourceName, string resourceType, bool hasDependencies,
                                                 out bool openDependencyGraph)
        {
            openDependencyGraph = false;

            bool shouldRemove = MessageBox.Show(Application.Current.MainWindow, "Are you sure you wish to delete the \""
                                                                                + resourceName + "\" " + resourceType +
                                                                                "?",
                                                "Confirm " + resourceType + " Deletion", MessageBoxButton.YesNo) ==
                                MessageBoxResult.Yes;

            if (shouldRemove && hasDependencies)
            {
                var dialog = new DeleteResourceDialog("Confirm " + resourceType + " Deletion", "The \""
                                                                                               + resourceName + "\" " +
                                                                                               resourceType +
                                                                                               " has resources that depend on it to function, are you sure you want to delete this "
                                                                                               + resourceType + "?",
                                                      true) {Owner = Application.Current.MainWindow};
                bool? result = dialog.ShowDialog();
                shouldRemove = result.HasValue && result.Value;

                openDependencyGraph = dialog.OpenDependencyGraph;
                shouldRemove = !dialog.OpenDependencyGraph;
            }

            return shouldRemove;
        }

        private static string GetIconPath(IContextualResourceModel resource)
        {
            string iconPath = resource.IconPath;
            if (string.IsNullOrEmpty(resource.UnitTestTargetWorkflowService))
            {
                if (string.IsNullOrEmpty(resource.IconPath))
                {
                    iconPath = ResourceType.WorkflowService.GetIconLocation();
                        }
                else if (!resource.IconPath.Contains(StringResources.Pack_Uri_Application_Image))
                    {
                    var imageUriConverter = new ContextualResourceModelToImageConverter();
                    var iconUri = imageUriConverter.Convert(resource, null, null, null) as Uri;
                    if (iconUri != null) iconPath = iconUri.ToString();
                    }
                }
                else
                {
                iconPath = string.IsNullOrEmpty(resource.IconPath)
                               ? StringResources.Navigation_UnitTest_Icon_Pack_Uri
                               : resource.IconPath;
            }
            return iconPath;
        }

        private static void AddDesignerAttributes(WorkflowDesignerViewModel workflowVm)
            {
            var designerAttributes = new Dictionary<Type, Type>
            {
                    {typeof (DsfActivity), typeof (DsfActivityDesigner)},
                    {typeof (DsfCommentActivity), typeof (DsfCommentActivityDesigner)},
                    {typeof (CommentActivity), typeof (DsfCommentActivityDesigner)},
                    {typeof (DsfAssignActivity), typeof (DsfAssignActivityDesigner)},
                    {typeof (TransformActivity), typeof (DsfTransformActivityDesigner)},
                    {typeof (DsfForEachActivity), typeof (DsfForEachActivityDesigner)},
                    {typeof (DsfWebPageActivity), typeof (DsfWebPageActivityDesigner)},
                    {typeof (DsfWebSiteActivity), typeof (DsfWebSiteActivityDesigner)},
                    {typeof (DsfCountRecordsetActivity), typeof (DsfCountRecordsetActivityDesigner)},
                    {typeof (DsfSortRecordsActivity), typeof (DsfSortRecordsActivityDesigner)},
                    {typeof (DsfMultiAssignActivity), typeof (DsfMultiAssignActivityDesigner)},
                    {typeof (DsfDataSplitActivity), typeof (DsfDataSplitActivityDesigner)},
                    {typeof (DsfPathCreate), typeof (DsfPathCreateDesigner)},
                    {typeof (DsfFileRead), typeof (DsfFileReadDesigner)},
                    {typeof (DsfFileWrite), typeof (DsfFileWriteDesigner)},
                    {typeof (DsfFolderRead), typeof (DsfFolderReadDesigner)},
                    {typeof (DsfPathCopy), typeof (DsfPathCopyDesigner)},
                    {typeof (DsfPathDelete), typeof (DsfPathDeleteDesigner)},
                    {typeof (DsfPathMove), typeof (DsfPathMoveDesigner)},
                    {typeof (DsfPathRename), typeof (DsfPathRenameDesigner)},
                    {typeof (DsfZip), typeof (DsfZipDesigner)},
                    {typeof (DsfUnZip), typeof (DsfUnzipDesigner)},
                    {typeof (DsfDateTimeActivity), typeof (DsfDateTimeActivityDesigner)},
                    {typeof (DsfCalculateActivity), typeof (DsfCalculateActivityDesigner)},
                    {typeof (DsfDateTimeDifferenceActivity), typeof (DsfDateTimeDifferenceActivityDesigner)},
                    {typeof (DsfCaseConvertActivity), typeof (DsfCaseConvertActivityDesigner)},
                    {typeof (DsfBaseConvertActivity), typeof (DsfBaseConvertActivityDesigner)},
                    {typeof (DsfReplaceActivity), typeof (DsfReplaceActivityDesigner)},
                    {typeof (DsfIndexActivity), typeof (DsfIndexActivityDesigner)},
                    {typeof (DsfDeleteRecordActivity), typeof (DsfDeleteRecordActivityDesigner)},
                    {typeof (DsfDataMergeActivity), typeof (DsfDataMergeActivityDesigner)},
                    {typeof (DsfRemoveActivity), typeof (DsfRemoveActivityDesigner)},
                    {typeof (DsfTagCountActivity), typeof (DsfTagCountActivityDesigner)},
                    {typeof (AssertActivity), typeof (DsfAssertActivityDesigner)},
                    {typeof (DsfFileForEachActivity), typeof (DsfFileForEachActivityDesigner)},
                    {typeof (DsfCheckpointActivity), typeof (DsfCheckpointActivityDesigner)},
                    {typeof (DsfFindRecordsActivity), typeof (DsfFindRecordsActivityDesigner)},
                    {typeof (DsfNumberFormatActivity), typeof (DsfNumberFormatActivityDesigner)}
                    // Travis.Frisinger : 25.09.2012 - Removed Http Activity as it is out of sync with the current release 1 plans
                };

            workflowVm.InitializeDesigner(designerAttributes);
        }

        private static bool IsWebpage(IContextualResourceModel resource)
        {
            bool isWebpage = resource.Category.Equals("Webpage", StringComparison.InvariantCultureIgnoreCase)
                             ||
                             resource.Category.Equals("Human Interface Workflow",
                                                      StringComparison.InvariantCultureIgnoreCase);

            return isWebpage;
        }

        private static Type GetUserInterfaceType(IContextualResourceModel resource)
            {
            Type userInterfaceType = null;

            if (resource.Category.Equals("Webpage", StringComparison.InvariantCultureIgnoreCase)
                || resource.Category.Equals("Human Interface Workflow", StringComparison.InvariantCultureIgnoreCase))
                {
                userInterfaceType = typeof (DsfWebPageActivity);
            }

            if (resource.Category.Equals("Website", StringComparison.InvariantCultureIgnoreCase))
            {
                userInterfaceType = typeof (DsfWebSiteActivity);
        }

            return userInterfaceType;
        }

        /// <summary>
        ///     Saves the open tabs.
        /// </summary>
        private void SaveOpenTabs()
        {
            if (Tabs == null) return;

            foreach (FrameworkElement item in Tabs)
        {
                IContextualResourceModel resource = ResourceHelper.GetContextualResourceModel(item.DataContext);

                if (resource != null)
        {
                    MainViewModel.Save(resource, false);
            }
            }
        }

        //private List<Node> GetDependencies(IContextualResourceModel resource)
        //{
        //    GraphBuilder builder = new GraphBuilder(MainViewModel.SecurityContext, resource);
        //    IEnumerable<Graph> result = builder.BuildGraphs();
        //    List<Node> dependencies = new List<Node>();

        //    foreach(Graph graph in result)
        //    {
        //        dependencies.AddRange(graph.GetAllUniqueNodesRecirsively());
        //    }

        //    return dependencies;
        //}

        #endregion

        #region Mediator Sinks

        internal void TabContextChanged(object input)
        {
            RemoveDataList();
            if (input == null)
                return;

            Type type = input.GetType();
            if (type == typeof (WorkflowDesignerViewModel))
            {
                var workflowVm = input as WorkflowDesignerViewModel;
                if (workflowVm != null)
                {
                    AddDataListView(workflowVm.ResourceModel);

                    if (PropertyPane != null)
                        PropertyPane.Content = workflowVm.PropertyView;
                    
                }
                if (workflowVm != null)
                {
                    workflowVm.AddMissingWithNoPopUpAndFindUnusedDataListItems();
                }
            }
            else
            {
                HideDataList();
            }
        }

        internal void HideDataList()
        {
            if (DataListPane == null)
                return;

            ContentControl dataListPane = DataListPane;
            var dataListView = dataListPane.Content as DataListView;
            if (dataListView == null)
                return;

            var dataListViewModel = dataListView.DataContext as DataListViewModel;
            if (dataListViewModel == null)
                return;

            dataListPane.Content = null;
            MainViewModel.ActiveDataList = null;
        }

        // Sashen : 07-08-2012 - we no longer need to dispose the datalist on a tab context change as there was a bug that was fixed
        //                       that catered for switching of tabs and mediator issues
        // Brendon : 2012-08-17 - This is actuall still needed to clear the data list pane when a tab is closed.
        internal void RemoveDataList()
        {
            if (DataListPane == null)
                return;

            ContentControl dataListPane = DataListPane;
            if (dataListPane == null)
                return;

            var dataListView = dataListPane.Content as DataListView;
            if (dataListView == null)
                return;

            var dataListViewModel = dataListView.DataContext as DataListViewModel;
            if (dataListViewModel == null)
                return;

            dataListView.DataContext = null;
            dataListPane.Content = null;
            MainViewModel.ActiveDataList = null;
        }

        internal void RemoveDataMapping()
        {
            if (DataMappingPane == null)
                return;
            DataMappingPane.Content = null;
        }

        internal void WorkflowActivitySelected(IWebActivity activity)
        {
            var mappingVm = new DataMappingViewModel(activity);
            var mappingView = new DataMapping {DataContext = mappingVm};

            if (DataMappingPane == null)
                return;

            DataMappingPane.Content = mappingView;
        }

        /// <summary>
        ///     Configures the decision expression.
        ///     Travis.Frisinger - Developed for new Decision Wizard
        /// </summary>
        /// <param name="wrapper">The wrapper.</param>
        internal void ConfigureDecisionExpression(Tuple<ModelItem, IEnvironmentModel> wrapper)
        {
            IEnvironmentModel environment = wrapper.Item2;
            ModelItem decisionActivity = wrapper.Item1;

            ModelProperty conditionProperty = decisionActivity.Properties[GlobalConstants.ConditionPropertyText];
            if (conditionProperty == null) return;

            ModelItem activity = conditionProperty.Value;
            if (activity == null) return;

                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler(environment.DataListChannel);
            ModelProperty activityExpression = activity.Properties[GlobalConstants.ExpressionPropertyText];

                    ErrorResultTO errors;
            Guid dataListID = environment.UploadToDataList(GlobalConstants.DefaultDataListInitalizationString,
                                                           out errors); // fake it to get the ID

                    if (errors.HasErrors()) //BUG 8796, Added this if to handle errors
                    {
                        // Bad things happened... Tell the user
                PopupProvider.Show(errors.MakeDisplayReady(), GlobalConstants.DecisionWizardErrorHeading,
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                        // Stop configuring!!!
                        return;
                    }

                    // Push the correct data to the server ;)
                    if (activityExpression != null && activityExpression.Value == null)
                    {
                        // Its all new, push the empty model
                        compiler.PushSystemModelToDataList(dataListID, DataListConstants.DefaultStack, out errors);
                    }
                    else if (activityExpression != null && activityExpression.Value != null)
                    {
                        //we got a model, push it in to the Model region ;)
                        // but first, strip and extract the model data ;)

                        string val = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activityExpression.Value.ToString());

                        if (!string.IsNullOrEmpty(val))
                        {
                            try
                            {
                                compiler.ConvertFromJsonToModel<Dev2DecisionStack>(val);
                                // Valid model... roll with it ;)
                                compiler.UpsertSystemTag(dataListID, enSystemTag.SystemModel, val, out errors);
                            }
                            catch
                            {
                                // An old model, time to push an empty model ;)
                                compiler.PushSystemModelToDataList(dataListID, DataListConstants.DefaultStack,
                                                                       out errors);
                            }
                        }
                        else
                        {
                            // Its all invalid   
                            compiler.PushSystemModelToDataList(dataListID, DataListConstants.DefaultStack, out errors);
                        }
                    }

                    // Now invoke the Wizard ;)
            Uri requestUri;
            if (Uri.TryCreate((environment.WebServerAddress + GlobalConstants.DecisionWizardLocation), UriKind.Absolute,
                              out requestUri))
                    {
                string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);
                        //var resourceViewModel = new ResourceWizardViewModel(decisionActivity);

                        var callBackHandler = new Dev2DecisionCallbackHandler();
                        //callBackHandler.Owner = new WebPropertyEditorWindow(callBackHandler, uriString);
                        //callBackHandler.Owner.ShowDialog();
                        WebSites.ShowWebPageDialog(uriString, callBackHandler, 824, 508);

                        // Wizard finished...
                        // Now Fetch from DL and push the model into the activityExpression.SetValue();
                        try
                        {
                    var dds = compiler.FetchSystemModelFromDataList<Dev2DecisionStack>(dataListID, out errors);

                            if (dds != null)
                            {
                                // Empty check the arms ;)
                                if (string.IsNullOrEmpty(dds.TrueArmText.Trim()))
                                {
                                    dds.TrueArmText = GlobalConstants.DefaultTrueArmText;
                                }

                                if (string.IsNullOrEmpty(dds.FalseArmText.Trim()))
                                {
                                    dds.FalseArmText = GlobalConstants.DefaultFalseArmText;
                                }

                                // Update the decision node on the workflow ;)
                                string modelData = dds.ToVBPersistableModel();

                                // build up our injected expression handler ;)
                        string expressionToInject = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"",
                                                                modelData, "\",",
                                                                GlobalConstants.InjectedDecisionDataListVariable, ")");

                                if (activityExpression != null)
                                {
                                    activityExpression.SetValue(expressionToInject);
                                }

                                // now set arms ;)
                        ModelProperty tArm = decisionActivity.Properties[GlobalConstants.TrueArmPropertyText];

                                if (tArm != null)
                                {
                                    tArm.SetValue(dds.TrueArmText);
                                }

                        ModelProperty fArm = decisionActivity.Properties[GlobalConstants.FalseArmPropertyText];

                                if (fArm != null)
                                {
                                    fArm.SetValue(dds.FalseArmText);
                                }
                            }
                        }
                        catch
                        {
                            // Bad things happened... Tell the user
                            //PopupProvider.Show("", "")
                            PopupProvider.Buttons = MessageBoxButton.OK;
                            PopupProvider.Description = GlobalConstants.DecisionWizardErrorString;
                            PopupProvider.Header = GlobalConstants.DecisionWizardErrorHeading;
                            PopupProvider.ImageType = MessageBoxImage.Error;
                            PopupProvider.Show();
                        }
                        finally
                        {
                            // clean up ;)
                            compiler.DeleteDataListByID(dataListID);
                        }
                    }
                }

        internal void ConfigureSwitchExpression(Tuple<ModelItem, IEnvironmentModel> wrapper)
        {
            IEnvironmentModel environment = wrapper.Item2;
            ModelItem switchActivity = wrapper.Item1;

            ModelProperty switchProperty = switchActivity.Properties[GlobalConstants.SwitchExpressionPropertyText];

            if (switchProperty != null)
            {
                ModelItem activity = switchProperty.Value;

                if (activity != null)
                {
                    IDataListCompiler compiler = DataListFactory.CreateDataListCompiler(environment.DataListChannel);
                    ModelProperty activityExpression =
                        activity.Properties[GlobalConstants.SwitchExpressionTextPropertyText];

                    ErrorResultTO errors;
                    Guid dataListID = environment.UploadToDataList(GlobalConstants.DefaultDataListInitalizationString,
                                                                   out errors); // fake it to get the ID

                    if (errors.HasErrors()) //BUG 8796, Added this if to handle errors
                    {
                        // Bad things happened... Tell the user
                        PopupProvider.Show(errors.MakeDisplayReady(), GlobalConstants.SwitchWizardErrorHeading,
                                           MessageBoxButton.OK, MessageBoxImage.Error);
                        // Stop configuring!!!
                        return;
                    }

                    if (activityExpression != null && activityExpression.Value == null)
                    {
                        // Its all new, push the default model
                        compiler.PushSystemModelToDataList(dataListID, DataListConstants.DefaultSwitch, out errors);
                    }
                    else
                    {
                        if (activityExpression != null)
                        {
                            if (activityExpression.Value != null)
                            {
                                string val = activityExpression.Value.ToString();

                                if (val.IndexOf(GlobalConstants.InjectedSwitchDataFetch, StringComparison.Ordinal) >= 0)
                                {
                                    // Time to extract the data
                                    int start = val.IndexOf("(", StringComparison.Ordinal);
                                    if (start > 0)
                                    {
                                        int end = val.IndexOf(@""",AmbientData", StringComparison.Ordinal);

                                        if (end > start)
                                        {
                                            start += 2;
                                            val = val.Substring(start, (end - start));

                                            // Convert back for usage ;)
                                            val = Dev2DecisionStack.FromVBPersitableModelToJSON(val);

                                            if (!string.IsNullOrEmpty(val))
                                            {
                                                try
                                                {
                                                    var ds = new Dev2Switch {SwitchVariable = val};

                                                    string webModel = compiler.ConvertModelToJson(ds);

                                                    // Valid model... roll with it ;)
                                                    compiler.UpsertSystemTag(dataListID, enSystemTag.SystemModel,
                                                                             webModel, out errors);
                                                }
                                                catch
                                                {
                                                    // An old model, time to push an empty model ;)
                                                    compiler.PushSystemModelToDataList(dataListID,
                                                                                       DataListConstants.DefaultSwitch,
                                                                                       out errors);
                                                }
                                            }
                                            else
                                            {
                                                // Its all invalid   
                                                compiler.PushSystemModelToDataList(dataListID,
                                                                                   DataListConstants.DefaultSwitch,
                                                                                   out errors);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // now invoke the wizard ;)
                    Uri requestUri;
                    if (Uri.TryCreate((environment.WebServerAddress + GlobalConstants.SwitchDropWizardLocation),
                                      UriKind.Absolute, out requestUri))
                    {
                        string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);

                        var callBackHandler = new Dev2DecisionCallbackHandler();
                        //callBackHandler.Owner = new WebPropertyEditorWindow(callBackHandler, uriString) { Width = 580, Height = 270 };
                        //callBackHandler.Owner.ShowDialog();
                        WebSites.ShowWebPageDialog(uriString, callBackHandler, 470, 285);


                        // Wizard finished...
                        // Now Fetch from DL and push the model data into the workflow
                        try
                        {
                            var ds = compiler.FetchSystemModelFromDataList<Dev2Switch>(dataListID, out errors);

                            if (ds != null)
                            {
                                // FetchSwitchData
                                string expressionToInject = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                                        "(\"", ds.SwitchVariable, "\",",
                                                                        GlobalConstants.InjectedDecisionDataListVariable,
                                                                        ")");

                                if (activityExpression != null)
                                {
                                    activityExpression.SetValue(expressionToInject);
                                }
                            }
                        }
                        catch
                        {
                            // Bad things happened... Tell the user
                            PopupProvider.Show(GlobalConstants.SwitchWizardErrorString,
                                               GlobalConstants.SwitchWizardErrorHeading, MessageBoxButton.OK,
                                               MessageBoxImage.Error);
                        }
                        finally
                        {
                            // clean up ;)
                            compiler.DeleteDataListByID(dataListID);
                        }
                    }
                }
            }
        }

        // Travis.Frisinger : 25.01.2013 - Amended to be like decision
        internal void ConfigureSwitchCaseExpression(Tuple<ModelItem, IEnvironmentModel> payload)
        {
            IEnvironmentModel environment = payload.Item2;
            ModelItem switchCase = payload.Item1;

            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler(environment.DataListChannel);
            Guid dataListID = environment.UploadToDataList(GlobalConstants.DefaultDataListInitalizationString,
                                                           out errors); // fake it to get the ID

            if (errors.HasErrors()) //BUG 8796, Added this if to handle errors
            {
                // Bad things happened... Tell the user
                PopupProvider.Show(errors.MakeDisplayReady(), GlobalConstants.SwitchWizardErrorHeading,
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                // Stop configuring!!!
                return;
            }

            // Always a new Switch, push empty model ;)
            compiler.PushSystemModelToDataList(dataListID, DataListConstants.DefaultCase, out errors);


            // now invoke the wizard ;)
            Uri requestUri;
            if (Uri.TryCreate((environment.WebServerAddress + GlobalConstants.SwitchDragWizardLocation),
                              UriKind.Absolute, out requestUri))
            {
                string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);

                var callBackHandler = new Dev2DecisionCallbackHandler();
                //callBackHandler.Owner = new WebPropertyEditorWindow(callBackHandler, uriString) { Width = 580, Height = 270 };
                //callBackHandler.Owner.ShowDialog();
                WebSites.ShowWebPageDialog(uriString, callBackHandler, 470, 285);


                // Wizard finished...
                // Now Fetch from DL and push the model data into the workflow
                try
                {
                    var ds = compiler.FetchSystemModelFromDataList<Dev2Switch>(dataListID, out errors);

                    if (ds != null)
                    {
                        ModelProperty keyProperty = switchCase.Properties["Key"];

                        if (keyProperty != null)
                        {
                            keyProperty.SetValue(ds.SwitchVariable);
                        }
                    }
                }
                catch
                {
                    // Bad things happened... Tell the user
                    PopupProvider.Buttons = MessageBoxButton.OK;
                    PopupProvider.Description = GlobalConstants.SwitchWizardErrorString;
                    PopupProvider.Header = GlobalConstants.SwitchWizardErrorHeading;
                    PopupProvider.ImageType = MessageBoxImage.Error;
                    PopupProvider.Show();
                }
                finally
                {
                    // clean up ;)
                    compiler.DeleteDataListByID(dataListID);
                }
            }
        }

        // 28.01.2013 - Travis.Frisinger : Added for Case Edits
        internal void EditSwitchCaseExpression(Tuple<ModelProperty, IEnvironmentModel> payload)
        {
            IEnvironmentModel environment = payload.Item2;
            ModelProperty switchCaseValue = payload.Item1;

            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler(environment.DataListChannel);
            Guid dataListID = environment.UploadToDataList(GlobalConstants.DefaultDataListInitalizationString,
                                                           out errors); // fake it to get the ID

            if (errors.HasErrors()) //BUG 8796, Added this if to handle errors
            {
                // Bad things happened... Tell the user
                PopupProvider.Show(errors.MakeDisplayReady(), GlobalConstants.SwitchWizardErrorHeading,
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                // Stop configuring!!!
                return;
            }

            // Extract existing value ;)

            if (switchCaseValue != null)
            {
                string val = switchCaseValue.ComputedValue.ToString();
                compiler.PushSystemModelToDataList(dataListID, new Dev2Switch {SwitchVariable = val}, out errors);
            }
            else
            {
                // Problems, push empty model ;)
                compiler.PushSystemModelToDataList(dataListID, DataListConstants.DefaultCase, out errors);
            }

            // now invoke the wizard ;)
            Uri requestUri;
            if (Uri.TryCreate((environment.WebServerAddress + GlobalConstants.SwitchDragWizardLocation),
                              UriKind.Absolute, out requestUri))
            {
                string uriString = Browser.FormatUrl(requestUri.AbsoluteUri, dataListID);

                var callBackHandler = new Dev2DecisionCallbackHandler();
                //callBackHandler.Owner = new WebPropertyEditorWindow(callBackHandler, uriString) { Width = 580, Height = 270 };
                //callBackHandler.Owner.ShowDialog();
                WebSites.ShowWebPageDialog(uriString, callBackHandler, 470, 285);


                // Wizard finished...
                // Now Fetch from DL and push the model data into the workflow
                try
                {
                    var ds = compiler.FetchSystemModelFromDataList<Dev2Switch>(dataListID, out errors);

                    if (ds != null)
                    {
                        // ReSharper disable PossibleNullReferenceException
                        switchCaseValue.SetValue(ds.SwitchVariable);
                        // ReSharper restore PossibleNullReferenceException
                    }
                }
                catch
                {
                    // Bad things happened... Tell the user
                    PopupProvider.Buttons = MessageBoxButton.OK;
                    PopupProvider.Description = GlobalConstants.SwitchWizardErrorString;
                    PopupProvider.Header = GlobalConstants.SwitchWizardErrorHeading;
                    PopupProvider.ImageType = MessageBoxImage.Error;
                    PopupProvider.Show();
                }
                finally
                {
                    // clean up ;)
                    compiler.DeleteDataListByID(dataListID);
                }
            }
        }

        internal void ShowDependencyGraph(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
                return;

            FrameworkElement selectedItem = null;
            foreach (FrameworkElement item in Tabs)
            {
                if (UIElementTitleProperty.GetTitle(item) == resourceModel.ResourceName + "*Dependencies")
                {
                    selectedItem = item;
                }
            }

            if (selectedItem != null)
            {
                SetActiveDocument(selectedItem);
            }
            else
            {
                var dependencyView = new DependencyVisualiser(MainViewModel.SecurityContext, resourceModel);
                //2012.10.02: massimo.guerrera - Added for the dependency viewer click through
                ImportService.SatisfyImports(dependencyView);
                UIElementTitleProperty.SetTitle(dependencyView, resourceModel.ResourceName + "*Dependencies");
                UIElementImageProperty.SetImage(dependencyView, "pack://application:,,,/images/dependency.png");
                Tabs.Add(dependencyView);
                SetActiveDocument(dependencyView);
            }
        }

 

        internal void SaveResourceModel(IContextualResourceModel resourceModel)
        {
            resourceModel.Environment.Resources.Save(resourceModel);
        }

        internal void BindViewToViewModel(IResourceModel resourceModel)
        {
            FrameworkElement tab = FindTabByResourceModel(resourceModel);
            if (tab == null) return;

            if (tab.DataContext is IWorkflowDesignerViewModel)
            {
                var workflowDataContext = tab.DataContext as IWorkflowDesignerViewModel;
                workflowDataContext.BindToModel();
            }
            else if (tab.DataContext is IDesignerViewModel)
            {
                var designerDataContext = tab.DataContext as IDesignerViewModel;
                designerDataContext.BindToModel();
            }
        }

        internal void AddHelpDocument(object resourceModel)
        {
            var helpResource = resourceModel as IResourceModel;
            if (helpResource != null && !string.IsNullOrWhiteSpace(helpResource.HelpLink))
            {
                FrameworkElement helpTab = FindTabByName(helpResource.ResourceName + "*Help");
                if (helpTab != null)
                {
                    SetActiveDocument(helpTab);
                }
                else
                {
                    var help = new HelpWindow(helpResource.HelpLink);
                    UIElementTitleProperty.SetTitle(help, helpResource.ResourceName + "*Help");
                    UIElementTabActionContext.SetTabActionContext(help, TabActionContext.HelpSearch);
                    Tabs.Add(help);
                    SetActiveDocument(help);
                }
            }
        }

        internal void ShowNewResourceWizard(object newResourceInfo)
        {
            var newResourceTuple = newResourceInfo as Tuple<IEnvironmentModel, string>;

            if (newResourceTuple == null) return;

            //Brendon.Page, 2012-12-03. Hack to fix Bug 6367. It was decided by the team that this should be employed because the wizards are going to be 
            //                          reworked in the near future and it was to much work to go an ammend all the javascript to be workspace aware.
            SaveOpenTabs();
            //End of hack

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

            bool doesServiceExist = environment.Resources.Find(r => r.ResourceName == "Dev2ServiceDetails").Count > 0;

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
                                   "/services/" + StudioToWizardBridge.SelectWizard(resourceModel) + "?" +
                                   ResourceKeys.Dev2ServiceType + "=" + resName, out requestUri))
                {
                    requestUri = new Uri(new Uri(StringResources.Uri_WebServer),
                                         "/services/" + StudioToWizardBridge.SelectWizard(resourceModel) + "?" +
                                         ResourceKeys.Dev2ServiceType + "=" + resName);
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
                PopupProvider.Buttons = MessageBoxButton.OK;
                PopupProvider.Description =
                    "Couldn't find the resource needed to display the wizard. Please ensure that a resource with the name 'Dev2ServiceDetails' exists.";
                PopupProvider.Header = "Missing Wizard";
                PopupProvider.ImageType = MessageBoxImage.Error;
                PopupProvider.Show();
            }
        }

        internal void ShowEditResourceWizard(object resourceModel)
        {
            //Brendon.Page, 2012-12-03. Hack to fix Bug 6367. It was decided by the team that this should be employed because the wizards are going to be 
            //                          reworked in the near future and it was to much work to go an ammend all the javascript to be workspace aware.
            SaveOpenTabs();
            //End of hack

            var resourceModelToEdit = resourceModel as IContextualResourceModel;

            if (RootWebSite.ShowDialog(resourceModelToEdit))
            {
                return;
            }

            bool doesServiceExist = resourceModelToEdit != null &&
                                    resourceModelToEdit.Environment.Resources.Find(
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
                PopupProvider.Buttons = MessageBoxButton.OK;
                PopupProvider.Description =
                    "Couldn't find the resource needed to display the wizard. Please ensure that a resource with the name 'Dev2ServiceDetails' exists.";
                PopupProvider.Header = "Missing Wizard";
                PopupProvider.ImageType = MessageBoxImage.Error;
                PopupProvider.Show();
            }
        }

        internal void CloseWizard(object input)
        {
            if (_win != null)
            {
                _win.Close();
            }
        }

        // Travis.Frisinger - 13.08.2012 : Changed to use POST request instead of fetched HTML injection ;)
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

                    if (errors.HasErrors()) //BUG 8796, Added this if to handle errors
                    {
                        // Bad things happened... Tell the user
                        PopupProvider.Show(errors.MakeDisplayReady(), "Webpart Wizard Error", MessageBoxButton.OK,
                                           MessageBoxImage.Error);
                        // Stop configuring!!!
                        return;
                    }

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

        internal void AddDataListView(IResourceModel resourceModel)
        {
            var dataListView = new DataListView();

            // Set up the DataList to be shared by all objects
            //dataListVm = MainViewModel.DataListRepository.All().FirstOrDefault(resource => resource.Resource == resourceModel) as DataListViewModel;
            //if(dataListVm == null)
            //{
            IDataListViewModel dataListVm =
                DataListViewModelFactory.CreateDataListViewModel(resourceModel);
            dataListView.DataContext = dataListVm;

            //MainViewModel.DataListRepository.Save(dataListVm);
            //}

            DataListSingleton.SetDataList(dataListVm);

            //21-11-2012:massimo.guerrera - added for bug 5024

            DataListPane.Content = dataListView;

            SetActiveDataList(dataListVm);
        }

        internal void SetActiveDataList(IDataListViewModel dataListViewModel)
        {
            MainViewModel.ActiveDataList = dataListViewModel;
            // dataList singletonset
            DataListSingleton.UpdateDataList(dataListViewModel);
        }

        internal void AddWorkflowDesigner(object resourceModel)
        {
            var resource = resourceModel as IContextualResourceModel;
            AddDataListView(resource);

            if (resource == null) return;

            foreach (
                FrameworkElement item in
                    Tabs.Where(item => UIElementTitleProperty.GetTitle(item) == resource.ResourceName))
                {
                    SetActiveDocument(item);
                    return;
                }

                AddWorkspaceItem(resource);

                var workflowDesignerWindow = new WorkflowDesignerWindow();
            string iconPath = GetIconPath(resource);
                var workflowVm = new WorkflowDesignerViewModel(resource);

            AttachedPropertyHelper.SetAttachedProperties(workflowDesignerWindow, resource, iconPath);
            AddDesignerAttributes(workflowVm);
                workflowDesignerWindow.DataContext = workflowVm;
            if (PropertyPane != null)
                PropertyPane.Content = workflowVm.PropertyView;

                Tabs.Add(workflowDesignerWindow);
                SetActiveDocument(workflowDesignerWindow);

                if (resource.Category.Equals("Webpage", StringComparison.InvariantCultureIgnoreCase)
                    || resource.Category.Equals("Human Interface Workflow", StringComparison.InvariantCultureIgnoreCase)
                    || resource.Category.Equals("Website", StringComparison.InvariantCultureIgnoreCase)
                    )
                {
                    AddUserInterfaceWorkflow(resource, workflowVm);
                }
            }

        internal void AddUserInterfaceWorkflow(IContextualResourceModel resource,
                                               IWorkflowDesignerViewModel workflowViemModel)
            {
            bool isWebpage = IsWebpage(resource);
            Type userInterfaceType = GetUserInterfaceType(resource);
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
            foreach (FrameworkElement tab in Tabs)
            {
                if (
                    !UIElementTitleProperty.GetTitle(tab)
                                           .Equals(
                                               string.Format("{0}.website", webActivity.ResourceModel.ResourceName),
                                               StringComparison.InvariantCultureIgnoreCase)) continue;
                SetActiveDocument(tab);
                return;
            }

            var viewModel = new WebsiteEditorViewModel(webActivity);
            ImportService.SatisfyImports(viewModel);
            var editor = new WebsiteEditorWindow(viewModel);
            UIElementTitleProperty.SetTitle(editor, string.Format("{0}.website", webActivity.ResourceModel.ResourceName));
            UIElementImageProperty.SetImage(editor, "pack://application:,,,/Images/webpagebuilder.png");
            UIElementTabActionContext.SetTabActionContext(editor, TabActionContext.Website);

            Tabs.Add(editor);
            SetActiveDocument(editor);
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

            UIElementTitleProperty.SetTitle(layoutGrid,
                                            string.Format("{0}/{1}.ui", webActivity.ResourceModel.ResourceName,
                                                          (webActivity.WebActivityObject as dynamic).DisplayName));
            UIElementImageProperty.SetImage(layoutGrid, "pack://application:,,,/Images/User.png");
            UIElementTabActionContext.SetTabActionContext(layoutGrid, TabActionContext.Webpage);
            //Set the active page to signal user interface transitions.
            if (l.LayoutObjects.Any())
            {
                MainViewModel.SetActivePage(l.LayoutObjects.First());
            }

            foreach (FrameworkElement tab in Tabs)
            {
                if (UIElementTitleProperty.GetTitle(tab) == UIElementTitleProperty.GetTitle(layoutGrid))
                {
                    SetActiveDocument(tab);
                    return;
                }
            }

            Tabs.Add(layoutGrid);
            SetActiveDocument(layoutGrid);
        }

        internal void SetActiveDocument(FrameworkElement document)
            //, string documentName, enResourceType resourceType = enResourceType.Unknown)
            {
            Dispatcher.CurrentDispatcher.Invoke(new Action(() => { ActiveDocument = document; }));

            //var workspaceID = Guid.NewGuid();

            //var workspaceItem = WorkspaceItems.FirstOrDefault(wi => string.Equals(wi.ServiceName, documentName, StringComparison.Ordinal));
            //if (workspaceItem == null)
            //{
            //    workspaceItem = new WorkspaceItem(workspaceID)
            //    {
            //        ServiceName = documentName,
            //        ServiceType = resourceType == enResourceType.Source ? WorkspaceItem.SourceServiceType : WorkspaceItem.ServiceServiceType
            //    };
            //    WorkspaceItems.Add(workspaceItem);
            //}
            //workspaceItem.IsSelected = true;
        }

        internal void AddStartTabs(object input)
        {
            if (EnvironmentRepository != null)
            {
                //int count = WorkspaceItems.Count - 1;
                //while (count >= 0)
                //{
                foreach (IWorkspaceItem workspaceItem in WorkspaceItems)
                {
                    //
                    // Get the environment for the workspace item
                    //
                    IWorkspaceItem item = workspaceItem;
                    IEnvironmentModel environment = EnvironmentRepository.FindSingle(e => e.IsConnected &&
                                                                                          e.DsfChannel is
                                                                                          IStudioClientContext &&
                                                                                          ((IStudioClientContext)
                                                                                           e.DsfChannel).ServerID ==
                                                                                          item.ServerID);

                    if (environment == null || environment.Resources == null) continue;

                    // TODO: 5559 B.P. to implement in new architecture
                    // This code will only start working when a constant ServerID is generated on the server
                    IResourceModel resource = environment.Resources.FindSingle(r => r.ResourceName == item.ServiceName);
                    if (resource == null) continue;

                    if (resource.ResourceType == ResourceType.WorkflowService)
                    {
                        AddWorkflowDesigner(resource);
                    }
                }
            }

            string location = Assembly.GetExecutingAssembly().Location;
            string path = Path.Combine(Path.GetDirectoryName(location), StringResources.Uri_Studio_Homepage);
            var content = new HelpWindow(path);

            UIElementTitleProperty.SetTitle(content, "Start Page");
            UIElementImageProperty.SetImage(content, StringResources.Pack_Uri_Application_Image_Home);
            Tabs.Add(content);
            SetActiveDocument(content);
            RemoveDataList();
        }

        internal void AddShortcutKeysPage(object input)
        {
            string location = Assembly.GetExecutingAssembly().Location;
            string path = Path.Combine(Path.GetDirectoryName(location),
                                       StringResources.Uri_Studio_Shortcut_Keys_Document);
            var content = new HelpWindow(path);
            UIElementTitleProperty.SetTitle(content, "Shortcut Keys");
            UIElementImageProperty.SetImage(content, StringResources.Pack_Uri_Application_Image_Information);
            Tabs.Add(content);
            SetActiveDocument(content);
        }

        internal void DisplayAboutDialog(object input)
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

        internal void ShowHelpTab(string uriToDisplay)
        {
            var content = new HelpWindow(uriToDisplay);
            UIElementTitleProperty.SetTitle(content, "Help : " + uriToDisplay);
            UIElementImageProperty.SetImage(content, StringResources.Pack_Uri_Application_Image_Help);
            Tabs.Add(content);
            SetActiveDocument(content);
            RemoveDataList();
        }

        internal void ShowExplorer(object input)
        {
            var explorerView = new ExplorerView();
            var explorerViewModel = new ExplorerViewModel();
            explorerView.DataContext = explorerViewModel;
            NavigationPane.Content = explorerView;
        }

        internal void ShowErrorConnectingToEnvironment(object environment)
        {
            MessageBox.Show("Error connecting to environment", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal void AddDeployResources(object input)
        {
            const string tabName = "Deploy Resources";

            FrameworkElement tab = FindTabByName(tabName);

            if (tab != null)
            {
                SetActiveDocument(tab);
                Mediator.SendMessage(MediatorMessages.SelectItemInDeploy, input);
            }
            else
            {
                var view = new DeployView {DataContext = DeployViewModelFactory.GetDeployViewModel(input)};

                AttachedPropertyHelper.SetAttachedProperties(view, tabName);

                Tabs.Add(view);
                SetActiveDocument(view);
            }
        }

        #endregion

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

        #region MoveWorkspaceItem

        //private void MoveWorkspaceItem(IContextualResourceModel model, int tabIndex)
        //{
        //    // TODO: Check model server uri
        //    IWorkspaceItem workspaceItem = WorkspaceItems.FirstOrDefault(wi => wi.ServiceName == model.ResourceName);
        //    if (workspaceItem == null)
        //    {
        //        return;
        //    }

        //    int currentIndex = WorkspaceItems.IndexOf(workspaceItem);
        //    if (currentIndex != tabIndex)
        //    {
        //        WorkspaceItems.RemoveAt(currentIndex);
        //        if (tabIndex >= 0 && tabIndex <= WorkspaceItems.Count)
        //        {
        //            WorkspaceItems.Insert(tabIndex, workspaceItem);
        //        }
        //        else
        //        {
        //            WorkspaceItems.Add(workspaceItem);
        //        }

        //        SaveWorkspaceItems();
        //    }
        //}

        #endregion

    }
}
