
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
using System.Activities;
using System.Activities.Core.Presentation;
using System.Activities.Debugger;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Activities.Designers2.Core;
using Dev2.AppResources.Converters;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.Collections;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.CustomControls.Utils;
using Dev2.Data.Interfaces;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Dialogs;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Factory;
using Dev2.Interfaces;
using Dev2.Messages;
using Dev2.Models;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.UndoFramework;
using Dev2.Utilities;
using Dev2.Utils;
using Dev2.ViewModels.Workflow;
using Dev2.Workspaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Workflow
// ReSharper restore CheckNamespace
{

    public class WorkflowDesignerViewModel : BaseWorkSurfaceViewModel,
                                             IHandle<AddStringListToDataListMessage>,
                                             IHandle<EditActivityMessage>,
                                             IHandle<SaveUnsavedWorkflowMessage>,
                                             IHandle<UpdateWorksurfaceFlowNodeDisplayName>, IWorkflowDesignerViewModel
    {
        static readonly Type[] DecisionSwitchTypes = { typeof(FlowSwitch<string>), typeof(FlowDecision) };



        #region Overrides of Screen

        public delegate void SourceLocationEventHandler(SourceLocation src);

        protected readonly IDesignerManagementService DesignerManagementService;
        readonly IWorkflowHelper _workflowHelper;
        DelegateCommand _collapseAllCommand;

        protected dynamic DataObject { get; set; }
        List<ModelItem> _selectedDebugItems = new List<ModelItem>();
        DelegateCommand _expandAllCommand;
        protected ModelService ModelService;
        UserControl _popupContent;
        IContextualResourceModel _resourceModel;
        Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts;
        // ReSharper disable InconsistentNaming
        protected WorkflowDesigner _wd;
        DesignerMetadata _wdMeta;
        protected DsfActivityDropViewModel _vm;
        ResourcePickerDialog _resourcePickerDialog;

        VirtualizedContainerService _virtualizedContainerService;
        MethodInfo _virtualizedContainerServicePopulateAllMethod;

        StudioSubscriptionService<DebugSelectionChangedEventArgs> _debugSelectionChangedService = new StudioSubscriptionService<DebugSelectionChangedEventArgs>();

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resource"> Workflow that appears on design surface</param>
        /// <param name="createDesigner"></param>
        public WorkflowDesignerViewModel(IContextualResourceModel resource, bool createDesigner = true)
            : this(resource, new WorkflowHelper(), createDesigner)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resource">Resource that will be opened</param>
        /// <param name="workflowHelper">seriali</param>
        /// <param name="createDesigner">create a new designer flag</param>
        public WorkflowDesignerViewModel(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            : this(EventPublishers.Aggregator, resource, workflowHelper, createDesigner)
        {
        }


        // ReSharper disable once TooManyDependencies
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventPublisher">Event publisher that is not the singleton</param>
        /// <param name="resource">Resource that will be opened</param>
        /// <param name="workflowHelper">Serialization helper</param>
        /// <param name="createDesigner">create a new designer flag</param>
        // ReSharper disable TooManyDependencies
        public WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            // ReSharper restore TooManyDependencies
            : this(eventPublisher, resource, workflowHelper,
                CustomContainer.Get<IPopupController>(), createDesigner)
        {
        }

        // BUG 9304 - 2013.05.08 - TWR - Added IWorkflowHelper parameter to facilitate testing
        // TODO Can we please not overload constructors for testing purposes. Need to systematically get rid of singletons.

        // ReSharper disable once TooManyDependencies
        /// <summary>
        /// Unit Testing Constructor
        /// </summary>
        /// <param name="eventPublisher"> Non singleton event publisher</param>
        /// <param name="resource">Resource that will be opened</param>
        /// <param name="workflowHelper">Serialisation Helper</param>
        /// <param name="popupController">Injected popup controller</param>
        /// <param name="createDesigner">Create a new designer flag</param>
        /// <param name="liteInit"> Lite initialise designer. Testing only</param>
        // ReSharper disable TooManyDependencies
        public WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, IPopupController popupController, bool createDesigner = true, bool liteInit = false, bool setupUnknownVariableTimer=true)
            // ReSharper restore TooManyDependencies
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("workflowHelper", workflowHelper);
            VerifyArgument.IsNotNull("popupController", popupController);

            _workflowHelper = workflowHelper;
            _resourceModel = resource;
            _resourceModel.OnDataListChanged += FireWdChanged;
            _resourceModel.OnResourceSaved += UpdateOriginalDataList;

            PopUp = popupController;

            if (_resourceModel.DataList != null)
            {
                SetOriginalDataList(_resourceModel);
            }
            DesignerManagementService = new DesignerManagementService(resource, _resourceModel.Environment.ResourceRepository);
            if (createDesigner)
            {
                ActivityDesignerHelper.AddDesignerAttributes(this, liteInit);
            }
            OutlineViewTitle = "Navigation Pane";
            _workflowInputDataViewModel = WorkflowInputDataViewModel.Create(_resourceModel);
            GetWorkflowLink();
            
            if(setupUnknownVariableTimer)
            SetupTimer();
            _firstWorkflowChange = true;
        }

        private void SetupTimer()
        {
           
            _changeIsPossible = true;
            _timer = new System.Timers.Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = GlobalConstants.AddPopupTimeDelay;
            _timer.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

                if (_changeIsPossible)
                {
                    AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
                    _changeIsPossible = false;
                }

        }

        void SetOriginalDataList(IContextualResourceModel contextualResourceModel)
        {
            if (!string.IsNullOrEmpty(contextualResourceModel.DataList))
            {
                // ReSharper disable MaximumChainedReferences
                _originalDataList = contextualResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "").Replace(Environment.NewLine, "").Trim();
                // ReSharper restore MaximumChainedReferences
            }
        }

        void UpdateOriginalDataList(IContextualResourceModel obj)
        {
            if (obj.IsWorkflowSaved)
            {
                SetOriginalDataList(obj);
            }
        }


        #endregion

        #region Properties

        public override bool CanSave { get { return ResourceModel.IsAuthorized(AuthorizationContext.Contribute); } }

        protected virtual bool IsDesignerViewVisible { get { return DesignerView != null && DesignerView.IsVisible; } }

        public override string DisplayName
        {
            get
            {
                var displayName = ResourceModel.UserPermissions == Permissions.View ?
                    string.Format("{0} [READONLY]", ResourceHelper.GetDisplayName(ResourceModel)) :
                    ResourceHelper.GetDisplayName(ResourceModel);
                return displayName;
            }
        }

        public string GetWorkflowLink()
        {
            if (_workflowInputDataViewModel != null)
            {
                if (!String.IsNullOrEmpty(_resourceModel.DataList))
                {

                    _workflowInputDataViewModel.DebugTo.DataList = _resourceModel.DataList;
                }
                _workflowLink = "";
                _workflowInputDataViewModel.LoadWorkflowInputs();
                _workflowInputDataViewModel.SetXmlData();
                var buildWebPayLoad = _workflowInputDataViewModel.BuildWebPayLoad();
                var workflowUri = WebServer.GetWorkflowUri(_resourceModel, buildWebPayLoad, UrlType.JSON);
                if (workflowUri != null)
                {
                    _workflowLink = workflowUri.ToString();
                }
            }
            NotifyOfPropertyChange(() => DisplayWorkflowLink);
            return _workflowLink;
        }

        public string DisplayWorkflowLink
        {
            get
            {
                var workflowLink = _workflowLink;
                if (!String.IsNullOrEmpty(workflowLink))
                {
                    var startIndex = workflowLink.IndexOf("&wid", StringComparison.InvariantCultureIgnoreCase);
                    if (startIndex != -1)
                    {
                        return workflowLink.Remove(startIndex);
                    }
                }
                return workflowLink;
            }
            private set
            {
                _workflowLink = value;
            }
        }

        public Visibility WorkflowLinkVisible
        {
            get
            {
                return _resourceModel.IsVersionResource ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public override string IconPath
        {
            get { return ResourceHelper.GetIconPath(ResourceModel); }
        }


        //2012.10.01: massimo.guerrera - Add Remove buttons made into one:)
        public IPopupController PopUp { get; set; }


        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public IList<IDataListVerifyPart> WorkflowVerifiedDataParts { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        public UserControl PopupContent
        {
            get { return _popupContent; }
            set
            {
                _popupContent = value;
                NotifyOfPropertyChange(() => PopupContent);
            }
        }

        public virtual object SelectedModelItem
        {
            get
            {
                if (_wd != null)
                {
                    if (_wd.Context != null)
                    {
                        return _wd.Context.Items.GetValue<Selection>().SelectedObjects.FirstOrDefault();
                    }
                }
                return null;
            }
        }

        public bool HasErrors { get; set; }

        public IContextualResourceModel ResourceModel { get { return _resourceModel; } set { _resourceModel = value; } }

        public string WorkflowName { get { return _resourceModel.ResourceName; } }

        public bool RequiredSignOff { get { return _resourceModel.RequiresSignOff; } }

        public WorkflowDesigner Designer { get { return _wd; } }

        public UIElement DesignerView
        {
            get
            {
                if (_wd != null)
                {
                    return _wd.View;
                }
                return null;
            }
        }

        public void UpdateWorkflowLink(string newLink)
        {
            DisplayWorkflowLink = newLink;
            NotifyOfPropertyChange("DisplayWorkflowLink");
        }

        public StringBuilder DesignerText { get { return ServiceDefinition; } }

        // BUG 9304 - 2013.05.08 - TWR - Refactored and removed setter
        public StringBuilder ServiceDefinition { get { return _workflowHelper.SerializeWorkflow(ModelService); } set { } }

        public string OutlineViewTitle { get; set; }

        #endregion

        #region Commands

        public ICommand CollapseAllCommand
        {
            get
            {
                return _collapseAllCommand ?? (_collapseAllCommand = new DelegateCommand(param =>
                {
                    bool val = Convert.ToBoolean(param);
                    if (val)
                    {
                        DesignerManagementService.RequestCollapseAll();
                    }
                    else
                    {
                        DesignerManagementService.RequestRestoreAll();
                    }
                }));
            }
        }

        public ICommand ExpandAllCommand
        {
            get
            {
                return _expandAllCommand ?? (_expandAllCommand = new DelegateCommand(param =>
                {
                    bool val = Convert.ToBoolean(param);
                    if (val)
                    {
                        DesignerManagementService.RequestExpandAll();
                    }
                    else
                    {
                        DesignerManagementService.RequestRestoreAll();
                    }
                }));
            }
        }

        public ICommand OpenWorkflowLinkCommand
        {
            get
            {
                return _openWorkflowLinkCommand ?? (_openWorkflowLinkCommand = new DelegateCommand(param =>
                {
                    if (!String.IsNullOrEmpty(_workflowLink))
                    {
                        if (_workflowInputDataViewModel.WorkflowInputCount == 0)
                        {
                            PopUp.ShowNoInputsSelectedWhenClickLink();
                        }
                        if (param.ToString() != "Do not perform action")
                        {
                            Process.Start(_workflowLink);
                        }
                    }
                }));
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Fixes up items added. Assigns unique Id. Initialises as flow step
        /// </summary>
        /// <param name="addedItems"></param>
        /// <returns></returns>

        // ReSharper disable MethodTooLong
        // ReSharper disable ExcessiveIndentation
        protected List<ModelItem> PerformAddItems(List<ModelItem> addedItems)
        // ReSharper restore ExcessiveIndentation
        // ReSharper restore MethodTooLong
        {
            for (int i = 0; i < addedItems.Count(); i++)
            {
                var mi = addedItems.ToList()[i];

                if (mi.Content != null)
                {
                    var computedValue = mi.Content.ComputedValue;
                    if (computedValue is IDev2Activity)
                    {
                        //2013.08.19: Ashley Lewis for bug 10116 - New unique id on paste
                        (computedValue as IDev2Activity).UniqueID = Guid.NewGuid().ToString();
                    }
                }

                AddSwitch(mi);

                if (mi.ItemType == typeof(FlowSwitch<string>))
                {
                    InitializeFlowSwitch(mi);
                }
                else if (mi.ItemType == typeof(FlowDecision))
                {
                    InitializeFlowDecision(mi);
                }
                else if (mi.ItemType == typeof(FlowStep))
                {
                    InitializeFlowStep(mi);
                }
            }
            return addedItems;
        }

        // ReSharper disable ExcessiveIndentation
        void AddSwitch(ModelItem mi)
        // ReSharper restore ExcessiveIndentation
        {
            if (mi.Parent != null &&
                   mi.Parent.Parent != null &&
                   mi.Parent.Parent.Parent != null &&
                   mi.Parent.Parent.Parent.ItemType == typeof(FlowSwitch<string>))
            {
                #region Extract the Switch Expression ;)

                ModelProperty activityExpression = mi.Parent.Parent.Parent.Properties["Expression"];

                if (activityExpression != null)
                {
                    var tmpModelItem = activityExpression.Value;

                    var switchExpressionValue = string.Empty;

                    if (tmpModelItem != null)
                    {
                        var tmpProperty = tmpModelItem.Properties["ExpressionText"];

                        if (tmpProperty != null)
                        {
                            if (tmpProperty.Value != null)
                            {
                                var tmp = tmpProperty.Value.ToString();

                                if (!string.IsNullOrEmpty(tmp))
                                {
                                    int start = tmp.IndexOf("(", StringComparison.Ordinal);
                                    int end = tmp.IndexOf(",", StringComparison.Ordinal);

                                    if (start < end && start >= 0)
                                    {
                                        start += 2;
                                        end -= 1;
                                        switchExpressionValue = tmp.Substring(start, (end - start));
                                    }
                                }
                            }
                        }
                    }

                #endregion

                    ModelProperty modelProperty = mi.Properties["Key"];
                    if (modelProperty != null && ((modelProperty.Value != null) && modelProperty.Value.ToString().Contains("Case")))
                    {
                        Dev2Logger.Log.Info("Publish message of type - " + typeof(ConfigureCaseExpressionMessage));
                        EventPublisher.Publish(new ConfigureCaseExpressionMessage { ModelItem = mi, ExpressionText = switchExpressionValue, EnvironmentModel = _resourceModel.Environment });
                    }
                }
            }
        }

        protected void InitializeFlowStep(ModelItem mi)
        {
            ModelProperty modelProperty = mi.Properties["Action"];
            InitialiseIsDSFWebPage(modelProperty);

            // PBI 9135 - 2013.07.15 - TWR - Changed to "as" check so that database activity also flows through this
            ModelProperty modelProperty1 = mi.Properties["Action"];
            InitialiseWithAction(modelProperty1);
        }

        void InitialiseWithAction(ModelProperty modelProperty1)
        {
            if (modelProperty1 != null)
            {
                var droppedActivity = modelProperty1.ComputedValue as DsfActivity;
                if (droppedActivity != null)
                {
                    if (!string.IsNullOrEmpty(droppedActivity.ServiceName))
                    {
                        //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                        InitialiseWithoutServiceName(modelProperty1, droppedActivity);
                    }
                    else
                    {
                        InitialiseWithServiceName(modelProperty1, droppedActivity);
                    }
                }
            }
        }

        void InitialiseWithServiceName(ModelProperty modelProperty1, DsfActivity droppedActivity)
        {
            if (DataObject != null)
            {
                InitialiseWithDataObject(droppedActivity);
            }
            else
            {
                //Massimo.Guerrera:17-04-2012 - PBI 9000                               
                if (_vm != null)
                {
                    IContextualResourceModel resource = _vm.SelectedResourceModel;
                    if (resource != null)
                    {
                        droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.Category;
                        droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());
                        modelProperty1.SetValue(droppedActivity);
                    }
                    _vm = null;
                }
            }
        }

        void InitialiseWithDataObject(DsfActivity droppedActivity)
        {
            var navigationItemViewModel = DataObject as ExplorerItemModel;

            if (navigationItemViewModel != null)
            {
                IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == navigationItemViewModel.EnvironmentId);
                if (environmentModel != null)
                {
                    var theResource = environmentModel.ResourceRepository.FindSingle(c => c.ID == navigationItemViewModel.ResourceId, true) as IContextualResourceModel;
                    //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                    DsfActivity d = DsfActivityFactory.CreateDsfActivity(theResource, droppedActivity, true, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());
                    d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = navigationItemViewModel.ResourcePath;
                    if (theResource != null)
                    {
                        d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = theResource.Category;
                    }
                    ExplorerItemModelToIconConverter converter = new ExplorerItemModelToIconConverter();
                    var bitmapImage = converter.Convert(new object[] { navigationItemViewModel.ResourceType, false }, null, null, null) as BitmapImage;
                    if (bitmapImage != null)
                    {
                        d.IconPath = bitmapImage.UriSource.ToString();
                    }
                    UpdateForRemote(d, theResource);
                    //08-07-2013 Removed for bug 9789 - droppedACtivity Is already the action
                    //Setting it twice causes double connection to startnode
                }
            }

            DataObject = null;
        }

        void InitialiseWithoutServiceName(ModelProperty modelProperty1, DsfActivity droppedActivity)
        {
            DsfActivity activity = droppedActivity;
            IContextualResourceModel resource = _resourceModel.Environment.ResourceRepository.FindSingle(
                c => c.Category == activity.ServiceName) as IContextualResourceModel;
            IEnvironmentRepository environmentRepository = EnvironmentRepository.Instance;
            droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false, environmentRepository, _resourceModel.Environment.IsLocalHostCheck());
            WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(droppedActivity, resource, environmentRepository.ActiveEnvironment);
            modelProperty1.SetValue(droppedActivity);

                               
        }

        void InitialiseIsDSFWebPage(ModelProperty modelProperty)
        {
            if (modelProperty != null && modelProperty.ComputedValue is DsfWebPageActivity)
            {
                var modelService = Designer.Context.Services.GetService<ModelService>();
                var items = modelService.Find(modelService.Root, typeof(DsfWebPageActivity));

                IEnumerable<ModelItem> modelItems = items as IList<ModelItem> ?? items.ToList();
                int totalActivities = modelItems.Count();

                ModelProperty property = modelItems.Last().Properties["DisplayName"];
                if (property != null)
                {
                    property.SetValue(string.Format("Webpage {0}", totalActivities));
                }
            }
        }

        protected void InitializeFlowSwitch(ModelItem mi)
        {
            // Travis.Frisinger : 28.01.2013 - Switch Amendments
            Dev2Logger.Log.Info("Publish message of type - " + typeof(ConfigureSwitchExpressionMessage));
            EventPublisher.Publish(new ConfigureSwitchExpressionMessage { ModelItem = mi, EnvironmentModel = _resourceModel.Environment, IsNew = true });
        }

        protected void InitializeFlowDecision(ModelItem mi)
        {
            Dev2Logger.Log.Info("Publish message of type - " + typeof(ConfigureDecisionExpressionMessage));
            EventPublisher.Publish(new ConfigureDecisionExpressionMessage { ModelItem = mi, EnvironmentModel = _resourceModel.Environment, IsNew = true });
        }

        public void EditActivity(ModelItem modelItem, Guid parentEnvironmentID, IEnvironmentRepository catalog)
        {
            if (Designer == null)
            {
                return;
            }
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root && (modelItem.ItemType == typeof(DsfActivity) || modelItem.ItemType.BaseType == typeof(DsfActivity)))
            {
                var resourceID = ModelItemUtils.TryGetResourceID(modelItem);

                var envID = ModelItemUtils.GetProperty("EnvironmentID", modelItem) as InArgument<Guid>;
                Guid environmentID;

                if (envID != null)
                {
                    Guid.TryParse(envID.Expression.ToString(), out environmentID);
                }
                else
                {
                    environmentID = parentEnvironmentID;
                }

                if (environmentID == Guid.Empty)
                {
                    // this was created on a localhost ... BUT ... we may be running it remotely!
                    // so, ensure that we are running in the context of the parent's environment
                    environmentID = parentEnvironmentID;
                }

                var environmentModel = catalog.FindSingle(c => c.ID == environmentID);

                if (environmentID == Guid.Empty && !catalog.ActiveEnvironment.IsLocalHostCheck())
                {
                    // we have an "localhost" environment id, yet it is not the active environment, must be remote execution 
                    // against a remote server, aka remote treated as local ;)
                    environmentModel = catalog.ActiveEnvironment;
                }

                if (environmentModel != null)
                {
                    // BUG 9634 - 2013.07.17 - TWR : added connect
                    if (!environmentModel.IsConnected)
                    {
                        environmentModel.Connect();
                        environmentModel.LoadResources();
                    }
                    IResourceModel resource;
                    if (resourceID != Guid.Empty)
                    {
                        resource = environmentModel.ResourceRepository.FindSingle(c => c.ID == resourceID);
                    }
                    else
                    {
                        var resourceName = ModelItemUtils.GetProperty("ServiceName", modelItem) as string;
                        resource = environmentModel.ResourceRepository.FindSingle(c => c.ResourceName == resourceName);
                    }
                    if (resource != null && environmentModel.AuthorizationService.IsAuthorized(AuthorizationContext.View, resource.ID.ToString()))
                    {
                        WorkflowDesignerUtils.EditResource(resource, EventPublisher);
                    }
                }
            }
        }

        ModelItem RecursiveForEachCheck(dynamic activity)
        {
            var innerAct = activity.DataFunc.Handler as ModelItem;
            if (innerAct != null)
            {
                if (innerAct.ItemType == typeof(DsfForEachActivity))
                {
                    innerAct = RecursiveForEachCheck(innerAct);
                }
            }
            return innerAct;
        }

        /// <summary>
        ///     Prevents the delete from being executed if it is a FlowChart.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        void PreventCommandFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {
            if (Designer != null && Designer.Context != null)
            {
                var selection = Designer.Context.Items.GetValue<Selection>();

                if (selection == null || selection.PrimarySelection == null)
                    return;

                if (selection.PrimarySelection.ItemType != typeof(Flowchart) &&
                   selection.SelectedObjects.All(modelItem => modelItem.ItemType != typeof(Flowchart)))
                    return;
            }

            e.CanExecute = false;
            e.Handled = true;
        }


        /// <summary>
        ///     Sets the last dropped point.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="DragEventArgs" /> instance containing the event data.
        /// </param>
        void SetLastDroppedPoint(DragEventArgs e)
        {
            var senderAsFrameworkElement = ModelService.Root.View as FrameworkElement;
            if (senderAsFrameworkElement != null)
            {
                UIElement freePormPanel = senderAsFrameworkElement.FindNameAcrossNamescopes("flowchartPanel");
                if (freePormPanel != null)
                {
                    e.GetPosition(freePormPanel);
                }
            }
        }

        #region DataList Workflow Specific Methods

        // We will be assuming that a workflow field is a recordset based on 2 criteria:
        // 1. If the field contains a set of parenthesis
        // 2. If the field contains a period, it is a recordset with a field.
        IList<IDataListVerifyPart> BuildWorkflowFields()
        {
            var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
            _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
            if (Designer != null)
            {
                var modelService = Designer.Context.Services.GetService<ModelService>();
                if (modelService != null)
                {
                    var flowNodes = modelService.Find(modelService.Root, typeof(FlowNode));

                    foreach (var flowNode in flowNodes)
                    {
                        var workflowFields = GetWorkflowFieldsFromModelItem(flowNode);
                        foreach (var field in workflowFields)
                        {
                            WorkflowDesignerDataPartUtils.BuildDataPart(field, _uniqueWorkflowParts);
                        }
                    }
                }
            }
            var flattenedList = _uniqueWorkflowParts.Keys.ToList();
            return flattenedList;
        }

        IEnumerable<string> GetWorkflowFieldsFromModelItem(ModelItem flowNode)
        {
            var workflowFields = new List<string>();

            var modelProperty = flowNode.Properties["Action"];
            if (modelProperty != null)
            {
                var activity = modelProperty.ComputedValue;
                workflowFields = GetActivityElements(activity);
            }
            else
            {
                string propertyName = string.Empty;
                switch (flowNode.ItemType.Name)
                {
                    case "FlowDecision":
                        propertyName = "Condition";
                        break;
                    case "FlowSwitch`1":
                        propertyName = "Expression";
                        break;
                }
                var property = flowNode.Properties[propertyName];
                if (property != null)
                {
                    var activity = property.ComputedValue;
                    if (activity != null)
                    {
                        workflowFields = GetDecisionElements((activity as dynamic).ExpressionText, DataListSingleton.ActiveDataList);
                    }
                }
                else
                {
                    return workflowFields;
                }
            }
            return workflowFields;
        }

        public List<String> GetDecisionElements(string expression, IDataListViewModel datalistModel)
        {
            var decisionFields = new List<string>();
            if (!string.IsNullOrEmpty(expression))
            {
                int startIndex = expression.IndexOf('"');
                startIndex = startIndex + 1;
                int endindex = expression.IndexOf('"', startIndex);
                string decisionValue = expression.Substring(startIndex, endindex - startIndex);

                //2013.06.21: Ashley Lewis for bug 9698 - avoid parsing entire decision stack, instantiate model and just parse column data only
                IDataListCompiler c = DataListFactory.CreateDataListCompiler();
                try
                {
                    var dds = c.ConvertFromJsonToModel<Dev2DecisionStack>(decisionValue.Replace('!', '\"').ToStringBuilder());
                    foreach (var decision in dds.TheStack)
                    {
                        var getCols = new[] { decision.Col1, decision.Col2, decision.Col3 };
                        for (var i = 0; i < 3; i++)
                        {
                            var getCol = getCols[i];
                            var parsed = GetParsedRegions(getCol, datalistModel);
                            if (!DataListUtil.IsValueRecordset((getCol)) && parsed.Any(a => DataListUtil.IsValueRecordset((a))))
                            {
                                IList<IIntellisenseResult> parts = DataListFactory.CreateLanguageParser().ParseExpressionIntoParts(decisionValue, new List<IDev2DataLanguageIntellisensePart>());


                                decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                            }
                            else
                                decisionFields = decisionFields.Union(GetParsedRegions(getCol, datalistModel)).ToList();
                        }
                    }

                }
                catch (Exception)
                {

                    if (!DataListUtil.IsValueRecordset((decisionValue)))
                    {
                        IList<IIntellisenseResult> parts = DataListFactory.CreateLanguageParser().ParseExpressionIntoParts(decisionValue, new List<IDev2DataLanguageIntellisensePart>());


                        decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                    }
                    else
                    {
                        IList<IIntellisenseResult> parts = DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(decisionValue,
                      DataListSingleton
                          .ActiveDataList
                          .WriteToResourceModel
                          (), true);
                        decisionFields.AddRange(parts.Select(part => DataListUtil.StripBracketsFromValue(part.Option.DisplayValue)));
                    }

                }
            }
            return decisionFields;
        }

        static IEnumerable<string> GetParsedRegions(string getCol, IDataListViewModel datalistModel)
        {
            var result = new List<string>();

            // Travis.Frisinger - 25.01.2013 
            // We now need to parse this data for regions ;)

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            // NEED - DataList for active workflow
            var parts = parser.ParseDataLanguageForIntellisense(getCol, datalistModel.WriteToResourceModel(), true);

            foreach (var intellisenseResult in parts)
            {
                getCol = DataListUtil.StripBracketsFromValue(intellisenseResult.Option.DisplayValue);
                if (!string.IsNullOrEmpty(getCol))
                {
                    result.Add(getCol);
                }

            }
            return result;
        }

        List<String> GetActivityElements(object activity)
        {
            var assign = activity as DsfActivityAbstract<string>;
            var other = activity as DsfActivityAbstract<bool>;
            enFindMissingType findMissingType;

            if (assign != null)
            {
                findMissingType = assign.GetFindMissingType();
            }

            else if (other != null)
            {
                findMissingType = other.GetFindMissingType();
            }
            else
            {
                return new List<String>();
            }

            var activityFields = new List<string>();

            var stratFac = new Dev2FindMissingStrategyFactory();

            IFindMissingStrategy strategy = stratFac.CreateFindMissingStrategy(findMissingType);

            foreach (var activityField in strategy.GetActivityFields(activity))
            {
                if (!string.IsNullOrEmpty(activityField))
                {
                    WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();
                    activityFields.AddRange(wdu.FormatDsfActivityField(activityField).Where(item => !item.Contains("xpath(")));
                }
            }
            return activityFields;
        }





        #endregion

        #endregion

        #region Internal Methods

        internal void OnItemSelected(Selection item)
        {
            NotifyItemSelected(item.PrimarySelection);
        }

        #endregion Internal Methods

        #region Public Methods


        /// <summary>
        ///     Handels the list of strings to be added to the data list without a pop up message
        /// </summary>
        /// <param name="message">The message.</param>
        /// <author>Massimo.Guerrera</author>
        /// <date>2013/02/06</date>
        public void Handle(AddStringListToDataListMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            IDataListViewModel dlvm = DataListSingleton.ActiveDataList;
            if (dlvm != null)
            {
                var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
                _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
                foreach (var s in message.ListToAdd)
                {
                    WorkflowDesignerDataPartUtils.BuildDataPart(s, _uniqueWorkflowParts);
                }
                IList<IDataListVerifyPart> partsToAdd = _uniqueWorkflowParts.Keys.ToList();
                List<IDataListVerifyPart> uniqueDataListPartsToAdd = dlvm.MissingDataListParts(partsToAdd);
                dlvm.AddMissingDataListItems(uniqueDataListPartsToAdd);
            }
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(UpdateResourceMessage message)
        {
            Dev2Logger.Log.Debug(message.GetType().Name);
            if (ContexttualResourceModelEqualityComparer.Current.Equals(message.ResourceModel, _resourceModel))
            {
                IObservableReadOnlyList<IErrorInfo> currentErrors = null;
                if (message.ResourceModel.Errors != null && message.ResourceModel.Errors.Count > 0)
                {
                    currentErrors = message.ResourceModel.Errors;
                }
                _resourceModel.Update(message.ResourceModel);
                if (currentErrors != null && currentErrors.Count > 0)
                {
                    foreach (var currentError in currentErrors)
                    {
                        _resourceModel.AddError(currentError);
                    }
                }
            }
        }

        /// <summary>
        /// Notifies the item selected.
        /// </summary>
        /// <param name="primarySelection">The primary selection.</param>
        /// <returns></returns>
        public bool NotifyItemSelected(object primarySelection)
        {
            var selectedItem = primarySelection as ModelItem;
            bool isItemSelected = false;

            if (selectedItem != null)
            {
                if (selectedItem.ItemType == typeof(DsfForEachActivity))
                {
                    dynamic test = selectedItem;
                    ModelItem innerActivity = RecursiveForEachCheck(test);
                    if (innerActivity != null)
                    {
                        selectedItem = innerActivity;
                    }
                }
                if (selectedItem.Properties["DisplayName"] != null)
                {
                    var modelProperty = selectedItem.Properties["DisplayName"];

                    if (modelProperty != null)
                    {
                        string displayName = modelProperty.ComputedValue.ToString();

                        var resourceModel =
                            _resourceModel.Environment.ResourceRepository.All()
                                          .FirstOrDefault(
                                              resource =>
                                              resource.ResourceName.Equals(displayName,
                                                  StringComparison.InvariantCultureIgnoreCase));

                        if (resourceModel != null || selectedItem.ItemType == typeof(DsfWebPageActivity))
                        {
                            WebActivityFactory.CreateWebActivity(selectedItem, resourceModel as IContextualResourceModel, displayName);
                            isItemSelected = true;
                        }
                    }
                }
            }
            return isItemSelected;
        }

        /// <summary>
        /// Saves the new XAML ;)
        /// </summary>
        public void BindToModel()
        {
            _resourceModel.WorkflowXaml = ServiceDefinition;
        }

        /// <summary>
        /// Initializes the designer.
        /// </summary>
        /// <param name="designerAttributes">The designer attributes.</param>
        /// <param name="liteInit">if set to <c>true</c> [lite initialize]. THIS IS FOR TESTING!!!!</param>
        public void InitializeDesigner(IDictionary<Type, Type> designerAttributes, bool liteInit = false)
        {
            _wd = new WorkflowDesigner();

            if (!liteInit)
            {
                var hashTable = new Hashtable
                {
                    {WorkflowDesignerColors.FontFamilyKey, Application.Current.Resources["DefaultFontFamily"]},
                    {WorkflowDesignerColors.FontSizeKey, Application.Current.Resources["DefaultFontSize"]},
                    {WorkflowDesignerColors.FontWeightKey, Application.Current.Resources["DefaultFontWeight"]},
                    {WorkflowDesignerColors.RubberBandRectangleColorKey, Application.Current.Resources["DesignerBackground"]},
                    {WorkflowDesignerColors.WorkflowViewElementBackgroundColorKey, Application.Current.Resources["WorkflowBackgroundBrush"]},
                    {WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColorKey, Application.Current.Resources["WorkflowBackgroundBrush"]},
                    {WorkflowDesignerColors.WorkflowViewElementSelectedBorderColorKey, Application.Current.Resources["WorkflowSelectedBorderBrush"]},
                    {WorkflowDesignerColors.DesignerViewShellBarControlBackgroundColorKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.DesignerViewShellBarColorGradientBeginKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.DesignerViewShellBarColorGradientEndKey, Application.Current.Resources["ShellBarViewBackground"]},
                    {WorkflowDesignerColors.OutlineViewItemSelectedTextColorKey, Application.Current.Resources["SolidWhite"]},
                    {WorkflowDesignerColors.OutlineViewItemHighlightBackgroundColorKey, Application.Current.Resources["DesignerBackground"]},
                    
                };

                _wd.PropertyInspectorFontAndColorData = XamlServices.Save(hashTable);
                // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade            
                var designerConfigService = _wd.Context.Services.GetService<DesignerConfigurationService>();
                if (designerConfigService != null)
                {
                    // set the runtime Framework version to 4.5 as new features are in .NET 4.5 and do not exist in .NET 4
                    designerConfigService.TargetFrameworkName = new System.Runtime.Versioning.FrameworkName(".NETFramework", new Version(4, 5));
                    designerConfigService.AutoConnectEnabled = true;
                    designerConfigService.AutoSplitEnabled = true;
                    designerConfigService.PanModeEnabled = true;
                    designerConfigService.RubberBandSelectionEnabled = true;
                    designerConfigService.BackgroundValidationEnabled = true; // prevent design-time background validation from blocking UI thread
                    // Disabled for now
                    designerConfigService.AnnotationEnabled = false;
                    designerConfigService.AutoSurroundWithSequenceEnabled = false;
                }
                _wdMeta = new DesignerMetadata();
                _wdMeta.Register();
                var builder = new AttributeTableBuilder();
                foreach (var designerAttribute in designerAttributes)
                {
                    builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
                }

                MetadataStore.AddAttributeTable(builder.CreateTable());

                _wd.Context.Services.Subscribe<ModelService>(ModelServiceSubscribe);
            }

            LoadDesignerXaml();

            if (!liteInit)
            {
                _wdMeta.Register();

                _wd.Context.Services.Subscribe<ViewStateService>(ViewStateServiceSubscribe);

                _wd.View.PreviewDrop += ViewPreviewDrop;

                _wd.View.PreviewMouseDown += ViewPreviewMouseDown;

                _wd.View.Measure(new Size(2000, 2000));

                _wd.View.LostFocus += OnViewOnLostFocus;
                _wd.Context.Services.Subscribe<DesignerView>(DesigenrViewSubscribe);

                _wd.Context.Items.Subscribe<Selection>(OnItemSelected);
                _wd.Context.Services.Publish(DesignerManagementService);

                //Jurie.Smit 2013/01/03 - Added to disable the deleting of the root flowchart
                CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
                _wd.ModelChanged += WdOnModelChanged;
                _wd.View.Focus();


                //2013.06.26: Ashley Lewis for bug 9728 - event avoids focus loss after a delete
                CommandManager.AddPreviewExecutedHandler(_wd.View, PreviewExecutedRoutedEventHandler);

                //2013.07.03: Ashley Lewis for bug 9637 - deselect flowchart after selection change (if more than one item selected)
                Selection.Subscribe(_wd.Context, SelectedItemChanged);

            }
            // BUG 9304 - 2013.05.08 - TWR
            _workflowHelper.EnsureImplementation(ModelService);

            if (!liteInit)
            {
                //For Changing the icon of the flowchart.
                WorkflowDesignerIcons.Activities.Flowchart = new DrawingBrush(new ImageDrawing(new BitmapImage(new Uri(@"pack://application:,,,/Warewolf Studio;component/Images/Workflow-32.png")), new Rect(0, 0, 16, 16)));
                WorkflowDesignerIcons.Activities.StartNode = new DrawingBrush(new ImageDrawing(new BitmapImage(new Uri(@"pack://application:,,,/Warewolf Studio;component/Images/StartNode.png")), new Rect(0, 0, 32, 32)));
                SubscribeToDebugSelectionChanged();
            }
        }

        void DesigenrViewSubscribe(DesignerView instance)
        {
            // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.None;
            instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom | ShellBarItemVisibility.PanMode | ShellBarItemVisibility.MiniMap;
        }

        void OnViewOnLostFocus(object sender, RoutedEventArgs args)
        {
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);

            // If we are opening from server skip this check, it cannot have "real" changes!
            if (!OpeningWorkflowsHelper.IsWorkflowWaitingforDesignerLoad(workSurfaceKey))
            {
                // an additional case we need to account for - Designer has resized and is only visible once focus is lost?! ;)
                if (OpeningWorkflowsHelper.IsWaitingForFistFocusLoss(workSurfaceKey) || WatermarkSential.IsWatermarkBeingApplied)
                {
                    ResourceModel.WorkflowXaml = ServiceDefinition;
                    OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
                }
            }
        }

        void ViewStateServiceSubscribe(ViewStateService instance)
        {
        }

        void ModelServiceSubscribe(ModelService instance)
        {
            ModelService = instance;
            ModelService.ModelChanged += ModelServiceModelChanged;
        }

        void SubscribeToDebugSelectionChanged()
        {
            _virtualizedContainerService = _wd.Context.Services.GetService<VirtualizedContainerService>();
            if (_virtualizedContainerService != null)
            {
                _virtualizedContainerServicePopulateAllMethod = _virtualizedContainerService.GetType().GetMethod("BeginPopulateAll", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            _debugSelectionChangedService.Unsubscribe();
            _debugSelectionChangedService.Subscribe(args =>
            {
                // we only care when the designer is visible
                if (!IsDesignerViewVisible)
                {
                    return;
                }

                if (args.SelectionType == ActivitySelectionType.None)
                {
                    ClearSelection();
                    return;
                }

                var selectedModelItem = args.DebugState != null ? GetSelectedModelItem(args.DebugState.WorkSurfaceMappingId, args.DebugState.ParentID) : null;
                if (selectedModelItem != null)
                {
                    switch (args.SelectionType)
                    {
                        case ActivitySelectionType.Single:
                            SelectSingleModelItem(selectedModelItem);
                            BringIntoView(selectedModelItem);
                            break;
                        case ActivitySelectionType.Add:
                            AddModelItemToSelection(selectedModelItem);
                            BringIntoView(selectedModelItem);
                            break;
                        case ActivitySelectionType.Remove:
                            RemoveModelItemFromSelection(selectedModelItem);
                            break;
                    }
                }
            });
        }

        protected virtual ModelItem GetSelectedModelItem(Guid itemId, Guid parentId)
        {
            var modelItems = ModelService.Find(ModelService.Root, typeof(IDev2Activity));
            // ReSharper disable MaximumChainedReferences
            var selectedModelItem = (from mi in modelItems
                                     let instanceID = ModelItemUtils.GetUniqueID(mi)
                                     where instanceID == itemId || instanceID == parentId
                                     select mi).FirstOrDefault();
            // ReSharper restore MaximumChainedReferences

            if (selectedModelItem == null)
            {
                // Find the root flow chart
                selectedModelItem = ModelService.Find(ModelService.Root, typeof(Flowchart)).FirstOrDefault();
            }
            else
            {
                if (DecisionSwitchTypes.Contains(selectedModelItem.Parent.ItemType))
                {
                    // Decision/switches activities are represented by their parents in the designer!
                    selectedModelItem = selectedModelItem.Parent;
                }
            }
            return selectedModelItem;
        }

        protected virtual void SelectSingleModelItem(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                return;
            }
            Selection.SelectOnly(_wd.Context, selectedModelItem);
            SelectedDebugItems.Add(selectedModelItem);
        }

        protected virtual void RemoveModelItemFromSelection(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                SelectedDebugItems.Remove(selectedModelItem);
            }
        }

        protected virtual void AddModelItemToSelection(ModelItem selectedModelItem)
        {
            if (SelectedDebugItems.Contains(selectedModelItem))
            {
                return;
            }
            Selection.Union(_wd.Context, selectedModelItem);
            SelectedDebugItems.Add(selectedModelItem);
        }

        protected virtual void ClearSelection()
        {
            _wd.Context.Items.SetValue(new Selection());
            _selectedDebugItems = new List<ModelItem>();
        }

        protected virtual void BringIntoView(ModelItem selectedModelItem)
        {
            var view = selectedModelItem.View as FrameworkElement;
            if (view != null && view.IsVisible)
            {
                BringIntoView(view);
                return;
            }

            var onAfterPopulateAll = new System.Action(() => BringIntoView(selectedModelItem.View as FrameworkElement));
            if (_virtualizedContainerServicePopulateAllMethod != null)
            {
                _virtualizedContainerServicePopulateAllMethod.Invoke(_virtualizedContainerService, new object[] { onAfterPopulateAll });
            }
        }

        void BringIntoView(FrameworkElement view)
        {
            if (view != null)
            {
                view.BringIntoView();
            }
        }

        protected void LoadDesignerXaml()
        {
            var xaml = _resourceModel.WorkflowXaml;

            // if null, try fetching. It appears there is more than the two routes identified to populating xaml ;(
            if (xaml == null || xaml.Length == 0)
            {
                // we always want server at this point ;)
                var workspace = GlobalConstants.ServerWorkspaceID;

                // log the trace for fetch ;)
                Dev2Logger.Log.Info(string.Format("Null Definition For {0} :: {1}. Fetching...", _resourceModel.ID, _resourceModel.ResourceName));

                // In the case of null of empty try fetching again ;)
                var msg = EnvironmentModel.ResourceRepository.FetchResourceDefinition(_resourceModel.Environment, workspace, _resourceModel.ID);
                if (msg != null)
                {
                    xaml = msg.Message;
                }
            }

            // if we still cannot find it, create a new one ;)
            if (xaml == null || xaml.Length == 0)
            {

                if (_resourceModel.ResourceType == ResourceType.WorkflowService)
                {
                    // log the trace for fetch ;)
                    Dev2Logger.Log.Info(string.Format("Could not find {0}. Creating a new workflow", _resourceModel.ResourceName));

                    // BUG 9304 - 2013.05.08 - TWR 
                    _wd.Load(_workflowHelper.CreateWorkflow(_resourceModel.ResourceName));

                    BindToModel();
                }
                else
                {
                    // we have big issues ;(
                    throw new Exception(string.Format("Could not find resource definition for {0}", _resourceModel.ResourceName));
                }
            }
            else
            {
                SetDesignerText(xaml);

                _wd.Load();
            }
        }

        void SetDesignerText(StringBuilder xaml)
        {
            // we got the correct model and clean it ;)
            var theText = _workflowHelper.SanitizeXaml(xaml);

            var length = theText.Length;
            var startIdx = 0;
            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);

            // now load the designer in chunks ;)
            for (int i = 0; i < rounds; i++)
            {
                var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                if (len > (theText.Length - startIdx))
                {
                    len = (theText.Length - startIdx);
                }

                _wd.Text += theText.Substring(startIdx, len);
                startIdx += len;
            }
        }

        void SelectedItemChanged(Selection item)
        {
            if (_wd != null)
            {
                if (_wd.Context != null)
                {
                    ContextItemManager contextItemManager = _wd.Context.Items;
                    var selection = contextItemManager.GetValue<Selection>();
                    if (selection.SelectedObjects.Count() > 1)
                    {
                        DeselectFlowchart();
                    }
                }
            }
        }

        void DeselectFlowchart()
        {
            if (_wd != null)
            {
                if (_wd.Context != null)
                {
                    EditingContext editingContext = _wd.Context;
                    var selection = editingContext.Items.GetValue<Selection>();
                    foreach (var item in selection.SelectedObjects.Where(item => item.ItemType == typeof(Flowchart)))
                    {
                        Selection.Toggle(editingContext, item);
                        break;
                    }
                }
            }
        }

        public void FocusActivityBuilder()
        {
            if (_wd != null)
            {
                if (_wd.Context != null)
                {
                    var findActivityBuilderModel = _wd.Context.Services.GetService<ModelService>();
                    if (findActivityBuilderModel != null)
                    {
                        var activityBuilderModel = findActivityBuilderModel.Find(findActivityBuilderModel.Root, typeof(ActivityBuilder)).ToList();
                        if (activityBuilderModel.Count > 0)
                        {
                            activityBuilderModel[0].Focus();
                        }
                    }
                }
            }
        }

        protected void WdOnModelChanged(object sender, EventArgs eventArgs)
        {
            if ((Designer != null && Designer.View.IsKeyboardFocusWithin) || sender != null)
            {
                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);

                // If we are opening from server skip this check, it cannot have "real" changes!
                if (!OpeningWorkflowsHelper.IsWorkflowWaitingforDesignerLoad(workSurfaceKey))
                {
                    // an additional case we need to account for - Designer has resized and is only visible once focus is lost?! ;)
                    if (OpeningWorkflowsHelper.IsWaitingForFistFocusLoss(workSurfaceKey) || WatermarkSential.IsWatermarkBeingApplied)
                    {
                        ResourceModel.WorkflowXaml = ServiceDefinition;
                        OpeningWorkflowsHelper.RemoveWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
                    }

                    var checkServiceDefinition = CheckServiceDefinition();
                    var checkDataList = CheckDataList();

                    ResourceModel.IsWorkflowSaved = checkServiceDefinition && checkDataList;
                    _workspaceSave = false;
                    NotifyOfPropertyChange(() => DisplayName);
                }
                else
                {
                    // When opening from server, save the hydrated changes for future comparison ;)
                    if (!CheckServiceDefinition())
                    {
                        // process any latent datalist changes ;)
                        ProcessDataListOnLoad();
                        ResourceModel.WorkflowXaml = ServiceDefinition;
                    }
                }

                // THIS MUST NEVER BE DELETED ;)
                WatermarkSential.IsWatermarkBeingApplied = false;
            }
            _changeIsPossible = true;
            if (_firstWorkflowChange)
            {
                AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
                _firstWorkflowChange = false;
            }
        }

        bool CheckDataList()
        {
            if (_originalDataList == null)
                return true;
            if (ResourceModel.DataList != null)
            {
                string currentDataList = ResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "");
                return currentDataList.SpaceCaseInsenstiveComparision(_originalDataList);
            }
            return true;
        }

        bool CheckServiceDefinition()
        {
            // This method was flawed with sb1 == sb2, that is object comparison. 
            // I needed to change the equality comparison to ensure my assignment works as expected ;)
            return true;// ServiceDefinition.IsEqual(ResourceModel.WorkflowXaml);
        }

        /// <summary>
        /// Processes the data list configuration load.
        /// </summary>
        public void ProcessDataListOnLoad()
        {
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(true);
        }

        public void DoWorkspaceSave()
        {
            if (ResourceModel != null && ResourceModel.IsNewWorkflow && !_workspaceSave && ResourceModel.Environment.IsConnected)
            {
                AsyncWorker asyncWorker = new AsyncWorker();
                asyncWorker.Start(() =>
                {
                    BindToModel();
                    ResourceModel.Environment.ResourceRepository.Save(ResourceModel);
                    _workspaceSave = true;

                });
            }
            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        public static bool ValidatResourceModel(string dataList)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataList))
                    // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                    XElement.Parse(dataList);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
            }
            catch (Exception)
            {

                return false;
            }
            return true;

        }

        /// <summary>
        /// Adds the missing with no pop up and find unused data list items.
        /// </summary>
        public void AddMissingWithNoPopUpAndFindUnusedDataListItems()
        {
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);

            if (!OpeningWorkflowsHelper.IsLoadedInFocusLossCatalog(workSurfaceKey))
            {
                OpeningWorkflowsHelper.AddWorkflowWaitingForFirstFocusLoss(workSurfaceKey);
            }

            AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(false);
        }

        /// <summary>
        /// Adds the missing with no pop up and find unused data list items implementation.
        /// </summary>
        /// <param name="isLoadEvent">if set to <c>true</c> [is load event].</param>
        private void AddMissingWithNoPopUpAndFindUnusedDataListItemsImpl(bool isLoadEvent)
        {
            if (DataListSingleton.ActiveDataList != null)
            {
                // given the flipping messaging, do this here, silly but works ;(
                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);
                if (OpeningWorkflowsHelper.IsWorkflowWaitingforDesignerLoad(workSurfaceKey) && !isLoadEvent)
                {
                    OpeningWorkflowsHelper.RemoveWorkflowWaitingForDesignerLoad(workSurfaceKey);
                }

                IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
                DispatcherUpdateAction(workflowFields);
            }
        }

        public Action<IList<IDataListVerifyPart>> DispatcherUpdateAction
        {
            get{return _dispatcherAction??RunUpdateOnDispatcher; }
            set{_dispatcherAction=value;}
        }
        private void RunUpdateOnDispatcher(IList<IDataListVerifyPart> workflowFields)
        {
            Application.Current.Dispatcher.Invoke(() =>
            DataListSingleton.ActiveDataList.UpdateDataListItems(ResourceModel, workflowFields));
        }

        public void Handle(UpdateWorksurfaceFlowNodeDisplayName message)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            foreach (var modelItem in ModelService.Find(ModelService.Root, typeof(DsfActivity)))
            {
                var currentName = ModelItemUtils.GetProperty("ServiceName", modelItem);
                if ((string)currentName == message.OldName)
                {
                    ModelItemUtils.SetProperty("ServiceName", message.NewName, modelItem);
                }
            }
        }

        #endregion

        #region Event Handlers

        void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = HandleMouseClick(e.LeftButton, e.ClickCount, e.OriginalSource as DependencyObject, e.Source as DesignerView);
        }

        /// <summary>
        /// Handles Mouse click events on Designer
        /// </summary>
        /// <param name="leftButtonState">State of left button</param>
        /// <param name="clickCount">Double,Single click</param>
        /// <param name="dp">Item Clicked</param>
        /// <param name="designerView">Designer view</param>
        /// <returns></returns>
        protected bool HandleMouseClick(MouseButtonState leftButtonState, int clickCount, DependencyObject dp, DesignerView designerView)
        {
            if (HandleDoubleClick(leftButtonState, clickCount, dp, designerView))
                return true;

            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                if (mvm != null && mvm.ActiveItem != null)
                {
                    mvm.RefreshActiveEnvironment();
                }
            }

            return false;
        }

        bool HandleDoubleClick(MouseButtonState leftButtonState, int clickCount, DependencyObject dp, DesignerView designerView)
        {
            if (leftButtonState == MouseButtonState.Pressed && clickCount == 2)
            {
                if (designerView != null && designerView.FocusedViewElement == null)
                {
                    return true;
                }

                var item = SelectedModelItem as ModelItem;

                // Travis.Frisinger - 28.01.2013 : Case Amendments
                if (item != null)
                {
                    string itemFn = item.ItemType.FullName;

                    //2013.03.20: Ashley Lewis - Bug 9202 Don't open any wizards if the source is a 'Microsoft.Windows.Themes.ScrollChrome' object
                    if (dp != null &&
                       string.Equals(dp.ToString(), "Microsoft.Windows.Themes.ScrollChrome",
                           StringComparison.InvariantCulture))
                    {
                        WizardEngineAttachedProperties.SetDontOpenWizard(dp, true);
                    }

                    // Handle Case Edits
                    if (itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchCaseLink", StringComparison.Ordinal) && !itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchDefaultLink", StringComparison.Ordinal))
                    {
                        if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp))
                        {
                            EventPublisher.Publish(new EditCaseExpressionMessage { ModelItem = item, EnvironmentModel = _resourceModel.Environment });
                        }
                    }

                    // Handle Switch Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) && item.ItemType == typeof(FlowSwitch<string>))
                    {
                        EventPublisher.Publish(new ConfigureSwitchExpressionMessage { ModelItem = item, EnvironmentModel = _resourceModel.Environment });
                    }

                    // Handle Decision Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) && item.ItemType == typeof(FlowDecision))
                    {
                        EventPublisher.Publish(new ConfigureDecisionExpressionMessage { ModelItem = item, EnvironmentModel = _resourceModel.Environment });
                    }
                }

                if (HandleWebActivity(designerView))
                    return true;
            }
            return false;
        }

        bool HandleWebActivity(DesignerView designerView)
        {
            if (designerView != null && designerView.FocusedViewElement != null &&
                   designerView.FocusedViewElement.ModelItem != null)
            {
                ModelItem modelItem = designerView.FocusedViewElement.ModelItem;

                if (modelItem.ItemType == typeof(DsfWebPageActivity) ||
                       modelItem.ItemType == typeof(DsfWebSiteActivity))
                {
                    ModelProperty modelProperty = modelItem.Properties["DisplayName"];
                    if (modelProperty != null)
                    {
                        IWebActivity webpageActivity = WebActivityFactory.CreateWebActivity(modelItem, _resourceModel,
                            modelProperty
                                .ComputedValue.ToString());
                        Dev2Logger.Log.Info("Publish message of type - " + typeof(AddWorkSurfaceMessage));
                        EventPublisher.Publish(new AddWorkSurfaceMessage(webpageActivity));
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Views the preview drop.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        void ViewPreviewDrop(object sender, DragEventArgs e)
        {

            bool dropOccured = true;
            SetLastDroppedPoint(e);
            DataObject = e.Data.GetData(typeof(ExplorerItemModel));
            if (DataObject != null)
            {
                IsItemDragged.Instance.IsDragged = true;
            }

            var isWorkflow = e.Data.GetData("WorkflowItemTypeNameFormat") as string;
            if (isWorkflow != null)
            {
                // PBI 10652 - 2013.11.04 - TWR - Refactored to enable re-use!

                var resourcePicked = ResourcePickerDialog.ShowDropDialog(ref _resourcePickerDialog, isWorkflow, out _vm);

                if (_vm != null && resourcePicked)
                {
                    e.Data.SetData(_vm.SelectedExplorerItemModel);
                }
                if (_vm != null && !resourcePicked)
                {
                    e.Handled = true;
                    dropOccured = false;
                }
            }
            if (dropOccured)
            {
                _workspaceSave = false;
                ResourceModel.IsWorkflowSaved = false;
                NotifyOfPropertyChange(() => DisplayName);
            }
            _resourcePickerDialog = null;

            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.ClearToolboxSelection();
            }
        }

        // BUG 9143 - 2013.07.03 - TWR - added
        //
        // Activity : Next
        // Decision : True, False
        // Switch   : Default, Key
        //
        public static readonly string[] SelfConnectProperties =
        {
            "Next", 
            "True", 
            "False", 
            "Default", 
            "Key"
        };

        string _originalDataList;
        bool _workspaceSave;
        WorkflowInputDataViewModel _workflowInputDataViewModel;
        string _workflowLink;
        ICommand _openWorkflowLinkCommand;
        private System.Timers.Timer _timer;
        private bool _changeIsPossible;

        public bool ChangeIsPossible
        {
            get { return _changeIsPossible; }
            set { _changeIsPossible = value; }
        }
        private Action<IList<IDataListVerifyPart>> _dispatcherAction;
        bool _firstWorkflowChange;

        /// <summary>
        /// Models the service model changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ModelChangedEventArgs"/> instance containing the event data.</param>
        protected void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        {
            // BUG 9143 - 2013.07.03 - TWR - added
            if (e.ModelChangeInfo != null &&
                e.ModelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged)
            {
                if (SelfConnectProperties.Contains(e.ModelChangeInfo.PropertyName))
                {
                    if (e.ModelChangeInfo.Subject == e.ModelChangeInfo.Value)
                    {
                        var modelProperty = e.ModelChangeInfo.Value.Properties[e.ModelChangeInfo.PropertyName];
                        if (modelProperty != null)
                        {
                            modelProperty.ClearValue();
                        }
                    }
                    return;
                }

                if (e.ModelChangeInfo.PropertyName == "StartNode")
                {
                    return;
                }
            }

            //ItemsAdded is obsolete - see e.ModelChangeInfo for correct usage
            //Code below is obsolete
#pragma warning disable 618
            if (e.ItemsAdded != null)
            {
                PerformAddItems(e.ItemsAdded.ToList());
            }
            else if (e.PropertiesChanged != null)
            {
                if (e.PropertiesChanged.Any(mp => mp.Name == "Handler"))
                {
                    if (DataObject != null)
                    {
                        ModelItemPropertyChanged(e);
                    }
                    else
                    {
                        ModelItemAdded(e);
                    }
                }
            }
        }

        void ModelItemAdded(ModelChangedEventArgs e)
        {
            ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");
#pragma warning restore 618

            if (modelProperty != null)
            {
                if (_vm != null)
                {
                    IContextualResourceModel resource = _vm.SelectedResourceModel;
                    if (resource != null)
                    {
                        DsfActivity droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());

                        droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.Category;
                        droppedActivity.IconPath = resource.IconPath;

                        modelProperty.SetValue(droppedActivity);
                    }
                    _vm.Dispose();
                    _vm = null;
                }
            }
        }

        void ModelItemPropertyChanged(ModelChangedEventArgs e)
        {
#pragma warning disable 618
            var navigationItemViewModel = DataObject as ExplorerItemModel;

            // ReSharper disable CSharpWarnings::CS0618
            ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");
            // ReSharper restore CSharpWarnings::CS0618

            if (navigationItemViewModel != null && modelProperty != null)
            {
                IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == navigationItemViewModel.EnvironmentId);
                if (environmentModel != null)
                {
                    var resource = environmentModel.ResourceRepository.FindSingle(c => c.ID == navigationItemViewModel.ResourceId) as IContextualResourceModel;
                    if (resource != null)
                    {
                        //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true, EnvironmentRepository.Instance, _resourceModel.Environment.IsLocalHostCheck());
                        d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.Category;
                        d.IconPath = resource.IconPath;
                        UpdateForRemote(d, resource);
                        modelProperty.SetValue(d);
                    }
                }
            }
            DataObject = null;
#pragma warning restore 618
        }

        void UpdateForRemote(DsfActivity d, IContextualResourceModel resource)
        {
            if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() && Application.Current.MainWindow != null)
            {
                dynamic mvm = Application.Current.MainWindow.DataContext;
                if (mvm != null && mvm.ActiveItem != null)
                {
                    WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(d, resource, mvm.ActiveItem.Environment);
                }
            }
            else
            {
                if (ActiveEnvironment != null)
                {
                    WorkflowDesignerUtils.CheckIfRemoteWorkflowAndSetProperties(d, resource, ActiveEnvironment);
                }
            }
        }

        protected IEnvironmentModel ActiveEnvironment { get; set; }

        /// <summary>
        ///     Handler attached to intercept checks for executing the delete command
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        protected void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete ||      //triggered from deleting an activity
                e.Command == EditingCommands.Delete ||          //triggered from editing displayname, expressions, etc
                e.Command == System.Activities.Presentation.View.DesignerView.CopyCommand ||
                e.Command == System.Activities.Presentation.View.DesignerView.CutCommand)
            {
                PreventCommandFromBeingExecuted(e);
            }
            if (e.Command == ApplicationCommands.Paste || e.Command == System.Activities.Presentation.View.DesignerView.PasteCommand)
            {
                PreventPasteFromBeingExecuted(e);
            }
        }

        void PreventPasteFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {

            var second = Clipboard.GetText();
            if (second.Contains("clr-namespace:Dev2.Activities;assembly=Dev2.Activities") && (second.Contains("Type=\"DbService\"") || second.Contains("Type=\"PluginService\"") || second.Contains("Type=\"WebService\"")))
            {
                var id = second.IndexOf("ResourceID=\"", StringComparison.Ordinal);
                if (id == -1)
                {
                    e.Handled = true;
                    e.CanExecute = false;
                    return;
                }
                var idEnd = second.Substring(id + 12, 36);
                IResourceModel resource = _resourceModel.Environment.ResourceRepository.FindSingle(a => a.ID == Guid.Parse(idEnd));
                if (resource == null)
                {
                    e.Handled = true;
                    e.CanExecute = false;
                }
            }


        }

        /// <summary>
        ///     Handler attached to intercept checks for executing the delete command
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.
        /// </param>
        void PreviewExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {

            if (e.Command == ApplicationCommands.Delete)
            {
                //2013.06.24: Ashley Lewis for bug 9728 - delete event sends focus to a strange place
                if (_wd != null)
                {
                    if (_wd.View != null)
                    {
                        _wd.View.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                }
            }
            if (e.Command == System.Activities.Presentation.View.DesignerView.PasteCommand)
            {
                new Task(() =>
                {
                    Thread.Sleep(2000);
                    BuildWorkflowFields();
                    EventPublisher.Publish(new UpdateAllIntellisenseMessage());
                }).Start();




            }
        }

        #endregion

        #region OnDispose
        protected override void OnDispose()
        {
            if(_timer != null)
                _timer.Stop();
            if (_wd != null)
            {
               
                _wd.Context.Items.Unsubscribe<Selection>(OnItemSelected);
                _wd.ModelChanged -= WdOnModelChanged;
                _wd.Context.Services.Unsubscribe<ModelService>(ModelServiceSubscribe);
                _wd.Context.Services.Unsubscribe<ViewStateService>(ViewStateServiceSubscribe);

                _wd.View.PreviewDrop -= ViewPreviewDrop;
                _wd.View.PreviewMouseDown -= ViewPreviewMouseDown;

                _wd.Context.Services.Unsubscribe<DesignerView>(DesigenrViewSubscribe);

                Selection.Unsubscribe(_wd.Context, SelectedItemChanged);
                CommandManager.RemovePreviewExecutedHandler(_wd.View, PreviewExecutedRoutedEventHandler);
                CommandManager.RemovePreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
                Selection.Unsubscribe(_wd.Context, SelectedItemChanged);
            }

            if (_debugSelectionChangedService != null)
            {
                _debugSelectionChangedService.Unsubscribe();
                _debugSelectionChangedService.Dispose();
                _debugSelectionChangedService = null;
            }

            if (DesignerManagementService != null)
            {
                DesignerManagementService.Dispose();
            }

            if (_resourceModel != null)
            {
                _resourceModel.OnDataListChanged -= FireWdChanged;
                _resourceModel.OnResourceSaved -= UpdateOriginalDataList;
            }

            if (ModelService != null)
            {
                ModelService.ModelChanged -= ModelServiceModelChanged;
            }

            if (_uniqueWorkflowParts != null)
            {
                _uniqueWorkflowParts.Clear();
                _uniqueWorkflowParts = null;
            }

            // remove this value from our helper class's cache ;)
            if (ResourceModel != null)
            {
                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(ResourceModel);
                OpeningWorkflowsHelper.PruneWorkflowFromCaches(workSurfaceKey);
            }
            if (_workflowInputDataViewModel != null)
            {
                _workflowInputDataViewModel.Dispose();
                _workflowInputDataViewModel = null;
            }
            try
            {
                CEventHelper.RemoveAllEventHandlers(_wd);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause


            _wd = null;
            base.OnDispose();
            GC.SuppressFinalize(this);
        }


        #endregion

        /// <summary>
        /// Gets the work surface context.
        /// </summary>
        /// <value>
        /// The work surface context.
        /// </value>
        public override WorkSurfaceContext WorkSurfaceContext
        {
            get
            {
                return (ResourceModel == null)
                           ? WorkSurfaceContext.Unknown
                           : ResourceModel.ResourceType.ToWorkSurfaceContext();
            }
        }

        #region Overrides of ViewAware

        #endregion

        /// <summary>
        /// Gets the environment model.
        /// </summary>
        /// <value>
        /// The environment model.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnvironmentModel EnvironmentModel { get { return ResourceModel.Environment; } }
        protected List<ModelItem> SelectedDebugItems
        {
            get
            {
                return _selectedDebugItems;
            }
        }

        #region Implementation of IHandle<EditActivityMessage>

        public void Handle(EditActivityMessage message)
        {
            Dev2Logger.Log.Info(message.GetType().Name);
            EditActivity(message.ModelItem, message.ParentEnvironmentID, message.EnvironmentRepository);
        }

        #endregion

        public void FireWdChanged()
        {
            WdOnModelChanged(new object(), new EventArgs());
            GetWorkflowLink();
        }

        #region Implementation of IHandle<SaveUnsavedWorkflow>

        public void Handle(SaveUnsavedWorkflowMessage message)
        {
            if (message == null || message.ResourceModel == null || String.IsNullOrEmpty(message.ResourceName) || message.ResourceModel.ID != ResourceModel.ID)
            {
                return;
            }
            Dev2Logger.Log.Info("Publish message of type - " + typeof(RemoveResourceAndCloseTabMessage));
            EventPublisher.Publish(new RemoveResourceAndCloseTabMessage(message.ResourceModel, false));
            var resourceModel = message.ResourceModel;
            WorkspaceItemRepository.Instance.Remove(resourceModel);
            resourceModel.Environment.ResourceRepository.DeleteResource(resourceModel);
            var unsavedName = resourceModel.ResourceName;
            BindToModel();
            UpdateResourceModel(message, resourceModel, unsavedName);
            PublishMessages(resourceModel);
            if (message.KeepTabOpen)
            {
                Dev2Logger.Log.Debug("Publish message of type - " + typeof(AddWorkSurfaceMessage));
                EventPublisher.Publish(new AddWorkSurfaceMessage(resourceModel));
            }
            NewWorkflowNames.Instance.Remove(unsavedName);

        }

        void PublishMessages(IContextualResourceModel resourceModel)
        {
            Dev2Logger.Log.Info("Publish message of type - " + typeof(UpdateResourceMessage));
            EventPublisher.Publish(new UpdateResourceMessage(resourceModel));

        }

        static void UpdateResourceModel(SaveUnsavedWorkflowMessage message, IContextualResourceModel resourceModel, string unsavedName)
        {
            resourceModel.ResourceName = message.ResourceName;
            resourceModel.DisplayName = message.ResourceName;
            resourceModel.Category = message.ResourceCategory;
            resourceModel.WorkflowXaml = resourceModel.WorkflowXaml.Replace(unsavedName, message.ResourceName);
            resourceModel.IsNewWorkflow = false;
            resourceModel.Environment.ResourceRepository.SaveToServer(resourceModel);
            resourceModel.Environment.ResourceRepository.Save(resourceModel);
            resourceModel.IsWorkflowSaved = true;
        }

        #endregion
    }

    internal class WorkflowIsSavedAction : AbstractAction
    {
        private IContextualResourceModel ResourceModel { get; set; }

        public WorkflowIsSavedAction(IContextualResourceModel resourceModel)
        {
            ResourceModel = resourceModel;
        }

        #region Overrides of AbstractAction

        protected override void ExecuteCore()
        {
            ResourceModel.IsWorkflowSaved = false;
        }

        protected override void UnExecuteCore()
        {
            ResourceModel.IsWorkflowSaved = true;
        }

        #endregion
    }
}
