#region

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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
using Caliburn.Micro;
using Dev2.Activities;
using Dev2.Activities.Adorners;
using Dev2.Composition;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Dev2.Services.Events;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.ActivityDesigners.Singeltons;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.DependencyInjection.EqualityComparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Wizards;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;


#endregion

namespace Dev2.Studio.ViewModels.Workflow
{

    public class WorkflowDesignerViewModel : BaseWorkSurfaceViewModel,
                                             IHandle<UpdateResourceMessage>,
                                             IHandle<AddStringListToDataListMessage>,
                                             IHandle<ShowActivityWizardMessage>,
                                             IHandle<ShowActivitySettingsWizardMessage>,
                                             IHandle<EditActivityMessage>, IWorkflowDesignerViewModel
    {
        static readonly Type[] DecisionSwitchTypes = { typeof(FlowSwitch<string>), typeof(FlowDecision) };

        #region Fields

        public delegate void SourceLocationEventHandler(SourceLocation src);

        readonly IDesignerManagementService _designerManagementService;
        readonly IWorkflowHelper _workflowHelper;

        RelayCommand _collapseAllCommand;

        protected dynamic DataObject { get; set; }

        RelayCommand _expandAllCommand;
        protected ModelService _modelService;
        UserControl _popupContent;
        IContextualResourceModel _resourceModel;
        Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts;
        WorkflowDesigner _wd;
        DesignerMetadata _wdMeta;
        DsfActivityDropViewModel _vm;

        VirtualizedContainerService _virtualizedContainerService;
        MethodInfo _virtualizedContainerServicePopulateAllMethod;

        readonly StudioSubscriptionService<DebugSelectionChangedEventArgs> _debugSelectionChangedService = new StudioSubscriptionService<DebugSelectionChangedEventArgs>();

        #endregion

        #region Constructor

        public WorkflowDesignerViewModel(IContextualResourceModel resource, bool createDesigner = true)
            : this(resource, WorkflowHelper.Instance, createDesigner)
        {
        }

        public WorkflowDesignerViewModel(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            : this(EventPublishers.Aggregator, resource, workflowHelper, createDesigner)
        {
        }

        public WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
            : this(eventPublisher, resource, workflowHelper,
                ImportService.GetExportValue<IFrameworkSecurityContext>(),
                ImportService.GetExportValue<IPopupController>(), createDesigner)
        {
        }

        // BUG 9304 - 2013.05.08 - TWR - Added IWorkflowHelper parameter to facilitate testing
        // TODO Can we please not overload constructors for testing purposes. Need to systematically get rid of singletons.
        public WorkflowDesignerViewModel(IEventAggregator eventPublisher, IContextualResourceModel resource, IWorkflowHelper workflowHelper,
            IFrameworkSecurityContext securityContext, IPopupController popupController, bool createDesigner = true)
            : base(eventPublisher)
        {
            VerifyArgument.IsNotNull("workflowHelper", workflowHelper);
            VerifyArgument.IsNotNull("securityContext", securityContext);
            VerifyArgument.IsNotNull("popupController", popupController);

            _workflowHelper = workflowHelper;
            _resourceModel = resource;
            _resourceModel.OnDataListChanged += FireWdChanged;
            _resourceModel.OnResourceSaved += UpdateOriginalDataList;

            SecurityContext = securityContext;
            PopUp = popupController;

            if(_resourceModel.DataList != null)
            {
                SetOriginalDataList(_resourceModel);
            }
            _designerManagementService = new DesignerManagementService(resource, _resourceModel.Environment.ResourceRepository);
            if(createDesigner)
            {
                ActivityDesignerHelper.AddDesignerAttributes(this);
            }
            OutlineViewTitle = "Navigation Pane";
        }

        void SetOriginalDataList(IContextualResourceModel contextualResourceModel)
        {
            if(!string.IsNullOrEmpty(contextualResourceModel.DataList))
            {
                _originalDataList = contextualResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "").Replace(Environment.NewLine, "").Trim();
            }
        }

        void UpdateOriginalDataList(IContextualResourceModel obj)
        {
            if(obj.IsWorkflowSaved)
            {
                SetOriginalDataList(obj);
            }
        }


        #endregion

        #region Properties

        protected virtual bool IsDesignerViewVisible { get { return DesignerView.IsVisible; } }

        public override string DisplayName
        {
            get
            {
                return ResourceHelper.GetDisplayName(ResourceModel);
            }
        }

        public override string IconPath
        {
            get { return ResourceHelper.GetIconPath(ResourceModel); }
        }

        public IFrameworkSecurityContext SecurityContext { get; set; }

        //2012.10.01: massimo.guerrera - Add Remove buttons made into one:)
        public IPopupController PopUp { get; set; }

        public IWizardEngine WizardEngine { get; set; }

        public IList<IDataListVerifyPart> WorkflowVerifiedDataParts { get; private set; }

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
                if(_wd != null)
                {
                    return _wd.Context.Items.GetValue<Selection>().SelectedObjects.FirstOrDefault();
                }
                return null;
            }
        }

        public bool HasErrors { get; set; }

        public IContextualResourceModel ResourceModel { get { return _resourceModel; } set { _resourceModel = value; } }

        public string WorkflowName { get { return _resourceModel.ResourceName; } }

        public bool RequiredSignOff { get { return _resourceModel.RequiresSignOff; } }

        public string AuthorRoles { get { return _resourceModel.AuthorRoles; } set { _resourceModel.AuthorRoles = value; } }

        public WorkflowDesigner Designer { get { return _wd; } }

        public UIElement DesignerView
        {
            get
            {
                return _wd.View;
            }
        }

        public string DesignerText { get { return ServiceDefinition; } }

        // BUG 9304 - 2013.05.08 - TWR - Refactored and removed setter
        public string ServiceDefinition { get { return _workflowHelper.SerializeWorkflow(_modelService); } set { } }

        // PBI 9221 : TWR : 2013.04.22 - added OutlineView
        public UIElement OutlineView { get { return _wd.OutlineView; } }

        public string OutlineViewTitle { get; set; }

        #endregion

        #region Commands

        public ICommand CollapseAllCommand
        {
            get
            {
                if(_collapseAllCommand == null)
                {
                    _collapseAllCommand = new RelayCommand(param =>
                    {
                        bool val = Convert.ToBoolean(param);
                        if(val)
                        {
                            _designerManagementService.RequestCollapseAll();
                        }
                        else
                        {
                            _designerManagementService.RequestRestoreAll();
                        }
                    }, param => true);
                }
                return _collapseAllCommand;
            }
        }

        public ICommand ExpandAllCommand
        {
            get
            {
                if(_expandAllCommand == null)
                {
                    _expandAllCommand = new RelayCommand(param =>
                    {
                        bool val = Convert.ToBoolean(param);
                        if(val)
                        {
                            _designerManagementService.RequestExpandAll();
                        }
                        else
                        {
                            _designerManagementService.RequestRestoreAll();
                        }
                    }, param => true);
                }
                return _expandAllCommand;
            }
        }

        #endregion

        #region Private Methods

        protected List<ModelItem> PerformAddItems(List<ModelItem> addedItems)
        {
            for(int i = 0; i < addedItems.Count(); i++)
            {
                var mi = addedItems.ToList()[i];

                if(mi.Content != null)
                {
                    var computedValue = mi.Content.ComputedValue;
                    if(computedValue is IDev2Activity)
                    {
                        //2013.08.19: Ashley Lewis for bug 10116 - New unique id on paste
                        (computedValue as IDev2Activity).UniqueID = Guid.NewGuid().ToString();
                    }
                }

                if(mi != null &&
                   mi.Parent != null &&
                   mi.Parent.Parent != null &&
                   mi.Parent.Parent.Parent != null &&
                   mi.Parent.Parent.Parent.ItemType == typeof(FlowSwitch<string>))
                {
                    #region Extract the Switch Expression ;)

                    ModelProperty activityExpression = mi.Parent.Parent.Parent.Properties["Expression"];

                    var tmpModelItem = activityExpression.Value;

                    var switchExpressionValue = string.Empty;

                    if(tmpModelItem != null)
                    {
                        var tmpProperty = tmpModelItem.Properties["ExpressionText"];

                        if(tmpProperty != null)
                        {
                            var tmp = tmpProperty.Value.ToString();

                            if(!string.IsNullOrEmpty(tmp))
                            {
                                int start = tmp.IndexOf("(");
                                int end = tmp.IndexOf(",");

                                if(start < end && start >= 0)
                                {
                                    start += 2;
                                    end -= 1;
                                    switchExpressionValue = tmp.Substring(start, (end - start));
                                }
                            }
                        }
                    }

                    #endregion

                    if((mi.Properties["Key"].Value != null) && mi.Properties["Key"].Value.ToString().Contains("Case"))
                    {
                        EventPublisher.Publish(new ConfigureCaseExpressionMessage { ModelItem = mi, ExpressionText = switchExpressionValue, EnvironmentModel = _resourceModel.Environment });
                    }
                }

                if(mi.ItemType == typeof(FlowSwitch<string>))
                {
                    InitializeFlowSwitch(mi);
                }
                else if(mi.ItemType == typeof(FlowDecision))
                {
                    InitializeFlowDecision(mi);
                }
                else if(mi.ItemType == typeof(FlowStep))
                {
                    InitializeFlowStep(mi);
                }
            }
            return addedItems;
        }

        protected void InitializeFlowStep(ModelItem mi)
        {
            if(mi.Properties["Action"].ComputedValue is DsfWebPageActivity)
            {
                var modelService = Designer.Context.Services.GetService<ModelService>();
                var items = modelService.Find(modelService.Root, typeof(DsfWebPageActivity));

                int totalActivities = items.Count();

                items.Last().Properties["DisplayName"].SetValue(string.Format("Webpage {0}", totalActivities.ToString()));
            }

            // PBI 9135 - 2013.07.15 - TWR - Changed to "as" check so that database activity also flows through this
            var droppedActivity = mi.Properties["Action"].ComputedValue as DsfActivity;
            if(droppedActivity != null)
            {
                if(!string.IsNullOrEmpty(droppedActivity.ServiceName))
                {
                    //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                    IContextualResourceModel resource = _resourceModel.Environment.ResourceRepository.FindSingle(
                        c => c.ResourceName == droppedActivity.ServiceName) as IContextualResourceModel;
                    droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false);
                    mi.Properties["Action"].SetValue(droppedActivity);
                }
                else
                {
                    if(DataObject != null)
                    {
                        var navigationItemViewModel = DataObject as ResourceTreeViewModel;

                        if(navigationItemViewModel != null)
                        {
                            var resource = navigationItemViewModel.DataContext;

                            if(resource != null)
                            {
                                //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                                DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, true);
                                d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                                d.IconPath = resource.IconPath;
                                CheckIfRemoteWorkflowAndSetProperties(d, resource);
                                //08-07-2013 Removed for bug 9789 - droppedACtivity Is already the action
                                //Setting it twice causes double connection to startnode
                                if(droppedActivity == null)
                                {
                                    mi.Properties["Action"].SetValue(d);
                                }
                            }
                        }

                        DataObject = null;
                    }
                    else
                    {
                        //Massimo.Guerrera:17-04-2012 - PBI 9000                               
                        if(_vm != null)
                        {
                            IContextualResourceModel resource = _vm.SelectedResourceModel;
                            if(resource != null)
                            {
                                droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.ResourceName;
                                droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false);
                                mi.Properties["Action"].SetValue(droppedActivity);
                            }
                            _vm = null;
                        }
                    }
                }
            }
        }

        protected void InitializeFlowSwitch(ModelItem mi)
        {
            // Travis.Frisinger : 28.01.2013 - Switch Amendments
            EventPublisher.Publish(new ConfigureSwitchExpressionMessage { ModelItem = mi, EnvironmentModel = _resourceModel.Environment, IsNew = true });
        }

        protected void InitializeFlowDecision(ModelItem mi)
        {
            EventPublisher.Publish(new ConfigureDecisionExpressionMessage { ModelItem = mi, EnvironmentModel = _resourceModel.Environment, IsNew = true });
        }

        void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource)
        {
            if(Application.Current != null &&
                Application.Current.Dispatcher.CheckAccess()
                && Application.Current.MainWindow != null)
            {
                var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
                if(mvm != null && mvm.ActiveItem != null)
                {
                    CheckIfRemoteWorkflowAndSetProperties(dsfActivity, resource, mvm.ActiveItem.Environment);
                }
            }
        }

        protected void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel contextEnv)
        {
            if(resource.ResourceType == ResourceType.WorkflowService)
            {
                if(contextEnv.ID != resource.Environment.ID)
                {
                    dsfActivity.ServiceUri = resource.Environment.Connection.WebServerUri.AbsoluteUri;
                    dsfActivity.ServiceServer = resource.Environment.ID;
                }
            };
        }

        void EditActivity(ModelItem modelItem, Guid parentEnvironmentID, IEnvironmentRepository catalog)
        {
            if(Designer == null)
            {
                return;
            }
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if(modelService.Root == modelItem.Root && (modelItem.ItemType == typeof(DsfActivity) || modelItem.ItemType.BaseType == typeof(DsfActivity)))
            {
                var resourceID = ModelItemUtils.TryGetResourceID(modelItem);

                    var envID = ModelItemUtils.GetProperty("EnvironmentID", modelItem) as InArgument<Guid>;
                    Guid environmentID;

                    if(envID != null)
                    {
                        Guid.TryParse(envID.Expression.ToString(), out environmentID);
                    }
                    else
                    {
                        environmentID = parentEnvironmentID;
                    }

                    if(environmentID == Guid.Empty)
                    {
                        // this was created on a localhost ... BUT ... we may be running it remotely!
                        // so, ensure that we are running in the context of the parent's environment
                        environmentID = parentEnvironmentID;
                    }

                    var environmentModel = catalog.FindSingle(c => c.ID == environmentID);

                    if(environmentModel != null)
                    {
                        // BUG 9634 - 2013.07.17 - TWR : added connect
                        if(!environmentModel.IsConnected)
                        {
                            environmentModel.Connect();
                            environmentModel.LoadResources();
                        }
                        var resource =
                    environmentModel.ResourceRepository.FindSingle(c => c.ID == resourceID);

                        WorkflowDesignerUtils.EditResource(resource, EventPublisher);
                    }
                }
        }


        void ShowActivitySettingsWizard(ModelItem modelItem)
        {
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if(modelService.Root == modelItem.Root)
            {
                if(WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;
                try
                {
                    activityWizardTO = WizardEngine.GetActivitySettingsWizardInvocationTO(modelItem, _resourceModel);
                }
                catch(Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        void ShowActivityWizard(ModelItem modelItem)
        {
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if(modelService.Root == modelItem.Root)
            {
                if(WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;


                if(!WizardEngine.HasWizard(modelItem, _resourceModel.Environment))
                {
                    PopUp.Show("Wizard not found.", "Missing Wizard");
                    return;
                }
                try
                {
                    activityWizardTO = WizardEngine.GetActivityWizardInvocationTO(modelItem, _resourceModel);
                }
                catch(Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        void ShowWizard(WizardInvocationTO activityWizardTO)
        {
            if(activityWizardTO != null)
            {
                var activitySettingsView = new ActivitySettingsView();
                var activitySettingsViewModel =
                    new ActivitySettingsViewModel(activityWizardTO, _resourceModel);

                activitySettingsViewModel.Close += delegate
                {
                    activitySettingsView.Dispose();
                    activitySettingsViewModel.Dispose();
                    activitySettingsView.DataContext = null;
                    PopupContent = null;
                };

                activitySettingsView.DataContext = activitySettingsViewModel;
                bool showPopUp = activitySettingsViewModel.InvokeWizard();

                if(showPopUp)
                {
                    PopupContent = activitySettingsView;
                }
            }
            else
            {
                PopupContent = null;
            }
        }

        ModelItem RecursiveForEachCheck(dynamic activity)
        {
            var innerAct = activity.DataFunc.Handler as ModelItem;
            if(innerAct != null)
            {
                if(innerAct.ItemType == typeof(DsfForEachActivity))
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
            if(Designer != null && Designer.Context != null)
            {
                var selection = Designer.Context.Items.GetValue<Selection>();

                if(selection == null || selection.PrimarySelection == null)
                    return;

                if(selection.PrimarySelection.ItemType != typeof(Flowchart) &&
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
            var senderAsFrameworkElement = _modelService.Root.View as FrameworkElement;
            if(senderAsFrameworkElement != null)
            {
                UIElement freePormPanel = senderAsFrameworkElement.FindNameAcrossNamescopes("flowchartPanel");
                if(freePormPanel != null)
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
            if(Designer != null)
            {
                var modelService = Designer.Context.Services.GetService<ModelService>();
                if(modelService != null)
                {
                    var flowNodes = modelService.Find(modelService.Root, typeof(FlowNode));

                    foreach(var flowNode in flowNodes)
                    {
                        var workflowFields = GetWorkflowFieldsFromModelItem(flowNode);
                        foreach(var field in workflowFields)
                        {
                            BuildDataPart(field);
                        }
                    }
                }
            }
            var flattenedList = _uniqueWorkflowParts.Keys.ToList();
            return flattenedList;
        }

        List<string> GetWorkflowFieldsFromModelItem(ModelItem flowNode)
        {
            var workflowFields = new List<string>();

            var modelProperty = flowNode.Properties["Action"];
            if(modelProperty != null)
            {
                var activity = modelProperty.ComputedValue;
                workflowFields = GetActivityElements(activity);
            }
            else
            {
                string propertyName = string.Empty;
                switch(flowNode.ItemType.Name)
                {
                    case "FlowDecision":
                        propertyName = "Condition";
                        break;
                    case "FlowSwitch`1":
                        propertyName = "Expression";
                        break;
                }
                var property = flowNode.Properties[propertyName];
                if(property != null)
                {
                    var activity = property.ComputedValue;
                    if(activity != null)
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
            if(!string.IsNullOrEmpty(expression))
            {
                int startIndex = expression.IndexOf('"');
                startIndex = startIndex + 1;
                int endindex = expression.IndexOf('"', startIndex);
                string decisionValue = expression.Substring(startIndex, endindex - startIndex);

                //2013.06.21: Ashley Lewis for bug 9698 - avoid parsing entire decision stack, instantiate model and just parse column data only
                IDataListCompiler c = DataListFactory.CreateDataListCompiler();
                try
                {
                    var dds = c.ConvertFromJsonToModel<Dev2DecisionStack>(decisionValue.Replace('!', '\"'));
                    foreach(var decision in dds.TheStack)
                    {
                        var getCols = new[] { decision.Col1, decision.Col2, decision.Col3 };
                        for(var i = 0; i < 3; i++)
                        {
                            var getCol = getCols[i];
                            decisionFields = decisionFields.Union(GetParsedRegions(getCol, datalistModel)).ToList();
                        }
                    }
                }
                catch(Exception e)
                {
                    IList<IIntellisenseResult> parts = DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(decisionValue,
                        DataListSingleton
                            .ActiveDataList
                            .WriteToResourceModel
                            (), true);

                    foreach(var part in parts)
                    {
                        decisionFields.Add(DataListUtil.StripBracketsFromValue(part.Option.DisplayValue));
                    }
                }
            }
            return decisionFields;
        }

        static List<string> GetParsedRegions(string getCol, IDataListViewModel datalistModel)
        {
            var result = new List<string>();

            // Travis.Frisinger - 25.01.2013 
            // We now need to parse this data for regions ;)

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            // NEED - DataList for active workflow
            var parts = parser.ParseDataLanguageForIntellisense(getCol, datalistModel.WriteToResourceModel(), true);

            foreach(var intellisenseResult in parts)
            {
                getCol = DataListUtil.StripBracketsFromValue(intellisenseResult.Option.DisplayValue);
                if(!string.IsNullOrEmpty(getCol))
                {
                    result.Add(getCol);
                }

            }
            return result;
        }

        // WHY THE HECK ARE WE RE-INVENTING THE WHEEL AND NOT USING THE INTELLISENSE PARSER?! ;)
        void BuildDataPart(string DataPartFieldData)
        {
            DataPartFieldData = DataListUtil.StripBracketsFromValue(DataPartFieldData);
            IDataListVerifyPart verifyPart;
            string fullyFormattedStringValue;
            string[] fieldList = DataPartFieldData.Split('.');
            if(fieldList.Count() > 1 && !String.IsNullOrEmpty(fieldList[0]))
            {
                // If it's a RecordSet Containing a field
                foreach(var item in fieldList)
                {
                    if(item.EndsWith(")") && item == fieldList[0])
                    {
                        if(item.Contains("("))
                        {
                            fullyFormattedStringValue = RemoveRecordSetBrace(item);
                            verifyPart =
                                IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue,
                                    String.Empty);
                            AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                        }
                        else
                        {
                            // If it's a field containing a single brace
                            continue;
                        }
                    }
                    else if(item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
                    {
                        // If it's a field to a record set
                        verifyPart =
                            IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                RemoveRecordSetBrace(fieldList.ElementAt(0)), item);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if(fieldList.Count() == 1 && !String.IsNullOrEmpty(fieldList[0]))
            {
                // If the workflow field is simply a scalar or a record set without a child
                if(DataPartFieldData.EndsWith(")") && DataPartFieldData == fieldList[0])
                {
                    if(DataPartFieldData.Contains("("))
                    {
                        fullyFormattedStringValue = RemoveRecordSetBrace(fieldList[0]);
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(
                            fullyFormattedStringValue, String.Empty);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                }
                else
                {
                    verifyPart =
                        IntellisenseFactory.CreateDataListValidationScalarPart(RemoveRecordSetBrace(DataPartFieldData));
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                }
            }
        }

        List<String> GetActivityElements(object activity)
        {
            var assign = activity as DsfActivityAbstract<string>;
            var other = activity as DsfActivityAbstract<bool>;
            enFindMissingType findMissingType;

            if(assign != null)
            {
                findMissingType = assign.GetFindMissingType();
            }

            else if(other != null)
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

            foreach(var activityField in strategy.GetActivityFields(activity))
            {
                if(!string.IsNullOrEmpty(activityField))
                {
                    WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();
                    activityFields.AddRange(wdu.FormatDsfActivityField(activityField).Where(item => !item.Contains("xpath(")));
                }
            }
            return activityFields;
        }

        public string GetValueFromUnlimitedObject(UnlimitedObject activityField, string valueToGet)
        {
            List<UnlimitedObject> enumerableResultSet = activityField.GetAllElements(valueToGet);
            string result = string.Empty;
            foreach(var item in enumerableResultSet)
            {
                if(!string.IsNullOrEmpty(item.GetValue(valueToGet)))
                {
                    result = item.GetValue(valueToGet);
                }
            }

            return result;
        }

        string RemoveRecordSetBrace(string RecordSet)
        {
            string fullyFormattedStringValue;
            if(RecordSet.Contains("(") && RecordSet.Contains(")"))
            {
                fullyFormattedStringValue = RecordSet.Remove(RecordSet.IndexOf("("));
            }
            else
                return RecordSet;
            return fullyFormattedStringValue;
        }

        void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart)
        {
            if(!_uniqueWorkflowParts.ContainsValue(nameOfPart))
            {
                _uniqueWorkflowParts.Add(part, nameOfPart);
            }
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
            IDataListViewModel dlvm = DataListSingleton.ActiveDataList;
            if(dlvm != null)
            {
                var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
                _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
                foreach(var s in message.ListToAdd)
                {
                    BuildDataPart(s);
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
            if(
                ContexttualResourceModelEqualityComparer.Current.Equals(
                    message.ResourceModel, _resourceModel))
            {
                _resourceModel.Update(message.ResourceModel);
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

            if(selectedItem != null)
            {
                var tryFindFirstTextBoxInActivity = selectedItem.View.TryFindFirstTextBoxInActivity();
                if(tryFindFirstTextBoxInActivity != null)
                {
                    Keyboard.Focus(tryFindFirstTextBoxInActivity);
                }
                if(selectedItem.ItemType == typeof(DsfForEachActivity))
                {
                    dynamic test = selectedItem;
                    ModelItem innerActivity = RecursiveForEachCheck(test);
                    if(innerActivity != null)
                    {
                        selectedItem = innerActivity;
                    }
                }
                if(selectedItem.Properties["DisplayName"] != null)
                {
                    var modelProperty = selectedItem.Properties["DisplayName"];

                    if(modelProperty != null)
                    {
                        string displayName = modelProperty.ComputedValue.ToString();

                        var resourceModel =
                            _resourceModel.Environment.ResourceRepository.All()
                                          .FirstOrDefault(
                                              resource =>
                                              resource.ResourceName.Equals(displayName,
                                                  StringComparison.InvariantCultureIgnoreCase));

                        if(resourceModel != null || selectedItem.ItemType == typeof(DsfWebPageActivity))
                        {
                            IWebActivity webActivity = WebActivityFactory
                                .CreateWebActivity(selectedItem, resourceModel as IContextualResourceModel, displayName);
                            isItemSelected = true;
                        }
                    }
                }
            }
            return isItemSelected;
        }

        /// <summary>
        /// Binds to model.
        /// </summary>
        public void BindToModel()
        {
            _resourceModel.WorkflowXaml = ServiceDefinition;
            if(string.IsNullOrEmpty(_resourceModel.ServiceDefinition))
            {
                _resourceModel.ServiceDefinition = _resourceModel.ToServiceDefinition();
            }
        }

        /// <summary>
        /// Initializes the designer.
        /// </summary>
        /// <param name="designerAttributes">The designer attributes.</param>
        public void InitializeDesigner(IDictionary<Type, Type> designerAttributes)
        {
            _wd = new WorkflowDesigner();

            var hashTable = new Hashtable
                {
                    {WorkflowDesignerColors.FontFamilyKey, Application.Current.Resources["DefaultFontFamily"]},
                    {WorkflowDesignerColors.FontSizeKey, Application.Current.Resources["DefaultFontSize"]},
                    {WorkflowDesignerColors.FontWeightKey, Application.Current.Resources["DefaultFontWeight"]}                    
                };

            _wd.PropertyInspectorFontAndColorData = XamlServices.Save(hashTable);

            // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade            
            var designerConfigService = _wd.Context.Services.GetService<DesignerConfigurationService>();
            if(designerConfigService != null)
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
            foreach(var designerAttribute in designerAttributes)
            {
                builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
            }

            MetadataStore.AddAttributeTable(builder.CreateTable());

            _wd.Context.Services.Subscribe<ModelService>(instance =>
                        {
                            _modelService = instance;
                            _modelService.ModelChanged += ModelServiceModelChanged;
                        });

            LoadDesignerXAML();

            _wdMeta.Register();

            _wd.Context.Services.Subscribe<ViewStateService>(instance =>
            { });

            _wd.View.PreviewDrop += ViewPreviewDrop;
            _wd.View.PreviewMouseDown += ViewPreviewMouseDown;

            _wd.Context.Services.Subscribe<DesignerView>(instance =>
            {
                // PBI 9221 : TWR : 2013.04.22 - .NET 4.5 upgrade
                instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.None;
                instance.WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom | ShellBarItemVisibility.PanMode | ShellBarItemVisibility.MiniMap;
            });

            _wd.Context.Items.Subscribe<Selection>(OnItemSelected);

            _wd.Context.Services.Publish(_designerManagementService);

            //Jurie.Smit 2013/01/03 - Added to disable the deleting of the root flowchart
            CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
            _wd.ModelChanged += WdOnModelChanged;
            //2013.06.26: Ashley Lewis for bug 9728 - event avoids focus loss after a delete
            CommandManager.AddPreviewExecutedHandler(_wd.View, PreviewExecutedRoutedEventHandler);

            //2013.07.03: Ashley Lewis for bug 9637 - deselect flowchart on selection change
            Selection.Subscribe(_wd.Context, SelectedItemChanged);

            // BUG 9304 - 2013.05.08 - TWR
            _workflowHelper.EnsureImplementation(_modelService);

            //For Changing the icon of the flowchart.
            WorkflowDesignerIcons.Activities.Flowchart = new DrawingBrush(new ImageDrawing(new BitmapImage(new Uri(@"pack://application:,,,/Warewolf Studio;component/Images/Workflow-32.png")), new Rect(0, 0, 16, 16)));

            SubscribeToDebugSelectionChanged();
        }

        void SubscribeToDebugSelectionChanged()
        {
            _virtualizedContainerService = _wd.Context.Services.GetService<VirtualizedContainerService>();
            if(_virtualizedContainerService != null)
            {
                _virtualizedContainerServicePopulateAllMethod = _virtualizedContainerService.GetType().GetMethod("BeginPopulateAll", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            _debugSelectionChangedService.Unsubscribe();
            _debugSelectionChangedService.Subscribe(args =>
            {
                // we only care when the designer is visible
                if(!IsDesignerViewVisible)
                {
                    return;
                }

                if(args.SelectionType == ActivitySelectionType.None)
                {
                    ClearSelection();
                    return;
                }

                var selectedModelItem = args.DebugState != null ? GetSelectedModelItem(args.DebugState.ID, args.DebugState.ParentID) : null;
                if(selectedModelItem != null)
                {
                    switch(args.SelectionType)
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

        protected virtual ModelItem GetSelectedModelItem(Guid itemID, Guid parentID)
        {
            var modelItems = _modelService.Find(_modelService.Root, typeof(IDev2Activity));
            var selectedModelItem = (from mi in modelItems
                                     let instanceID = ModelItemUtils.GetUniqueID(mi)
                                     where instanceID == itemID || instanceID == parentID
                                     select mi).FirstOrDefault();

            if(selectedModelItem == null)
            {
                // Find the root flow chart
                selectedModelItem = _modelService.Find(_modelService.Root, typeof(Flowchart)).FirstOrDefault();
            }
            else
            {
                if(DecisionSwitchTypes.Contains(selectedModelItem.Parent.ItemType))
                {
                    // Decision/switches activities are represented by their parents in the designer!
                    selectedModelItem = selectedModelItem.Parent;
                }
            }
            return selectedModelItem;
        }

        protected virtual void SelectSingleModelItem(ModelItem selectedModelItem)
        {
            Selection.SelectOnly(_wd.Context, selectedModelItem);
        }

        protected virtual void RemoveModelItemFromSelection(ModelItem selectedModelItem)
        {
            Selection.Toggle(_wd.Context, selectedModelItem);
        }

        protected virtual void AddModelItemToSelection(ModelItem selectedModelItem)
        {
            Selection.Union(_wd.Context, selectedModelItem);
        }

        protected virtual void ClearSelection()
        {
            _wd.Context.Items.SetValue(new Selection());
        }

        protected virtual void BringIntoView(ModelItem selectedModelItem)
        {
            var view = selectedModelItem.View as FrameworkElement;
            if(view != null && view.IsVisible)
            {
                BringIntoView(view);
                return;
            }

            var onAfterPopulateAll = new System.Action(() => BringIntoView(selectedModelItem.View as FrameworkElement));
            if(_virtualizedContainerServicePopulateAllMethod != null)
            {
                _virtualizedContainerServicePopulateAllMethod.Invoke(_virtualizedContainerService, new object[] { onAfterPopulateAll });
            }
        }

        void BringIntoView(FrameworkElement view)
        {
            if(view != null)
            {
                view.BringIntoView();
            }
        }

        void LoadDesignerXAML()
        {
            if(string.IsNullOrEmpty(_resourceModel.WorkflowXaml))
            {
                // BUG 9304 - 2013.05.08 - TWR 
                _wd.Load(_workflowHelper.CreateWorkflow(_resourceModel.ResourceName));

                BindToModel();
            }
            else
            {
                _wd.Text = _workflowHelper.SanitizeXaml(_resourceModel.WorkflowXaml);
                _wd.Load();
            }
        }

        void SelectedItemChanged(Selection item)
        {
            if(_wd.Context.Items.GetValue<Selection>().SelectedObjects.Count() > 1)
            {
                DeselectFlowchart();
            }
        }

        void DeselectFlowchart()
        {
            foreach(var item in _wd.Context.Items.GetValue<Selection>().SelectedObjects.Where(item => item.ItemType == typeof(Flowchart)))
            {
                Selection.Toggle(_wd.Context, item);
                break;
            }
        }

        public void FocusActivityBuilder()
        {
            var findActivityBuilderModel = _wd.Context.Services.GetService<ModelService>();
            var activityBuilderModel = findActivityBuilderModel.Find(findActivityBuilderModel.Root, typeof(ActivityBuilder)).ToList();
            if(activityBuilderModel.Count > 0)
            {
                activityBuilderModel[0].Focus();
            }
        }

        protected void WdOnModelChanged(object sender, EventArgs eventArgs)
        {
            if(Designer.View.IsKeyboardFocusWithin || sender != null)
            {
                var checkServiceDefinition = CheckServiceDefinition();
                var checkDataList = CheckDataList();
                ResourceModel.IsWorkflowSaved = checkServiceDefinition && checkDataList;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        bool CheckDataList()
        {
            if(_originalDataList == null) return true;
            if(ResourceModel.DataList != null)
            {
                string currentDataList = ResourceModel.DataList.Replace("<DataList>", "").Replace("</DataList>", "");
                return currentDataList.SpaceCaseInsenstiveComparision(_originalDataList);
            }
            return true;
        }

        bool CheckServiceDefinition()
        {
            return ServiceDefinition == ResourceModel.WorkflowXaml;
        }

        /// <summary>
        /// Adds the missing with no pop up and find unused data list items.
        /// </summary>
        public void AddMissingWithNoPopUpAndFindUnusedDataListItems()
        {
            if(DataListSingleton.ActiveDataList != null)
            {
                IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
                DataListSingleton.ActiveDataList.UpdateDataListItems(ResourceModel, workflowFields);
            }
        }

        #endregion

        #region Event Handlers

        void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = HandleMouseClick(e.LeftButton, e.ClickCount, e.OriginalSource as DependencyObject, e.Source as DesignerView);
        }

        protected bool HandleMouseClick(MouseButtonState leftButtonState, int clickCount, DependencyObject dp, DesignerView designerView)
        {
            if(leftButtonState == MouseButtonState.Pressed && clickCount == 2)
            {
                if(designerView != null && designerView.FocusedViewElement == null)
                {
                    return true;
                }

                var item = SelectedModelItem as ModelItem;

                // Travis.Frisinger - 28.01.2013 : Case Amendments
                if(item != null)
                {
                    string itemFn = item.ItemType.FullName;

                    //2013.03.20: Ashley Lewis - Bug 9202 Don't open any wizards if the source is a 'Microsoft.Windows.Themes.ScrollChrome' object
                    if(dp != null &&
                       string.Equals(dp.ToString(), "Microsoft.Windows.Themes.ScrollChrome",
                           StringComparison.InvariantCulture))
                    {
                        WizardEngineAttachedProperties.SetDontOpenWizard(dp, true);
                    }

                    // Handle Case Edits
                    if(itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchCaseLink",
                           StringComparison.Ordinal)
                       &&
                       !itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchDefaultLink",
                           StringComparison.Ordinal))
                    {
                        if(dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp))
                        {
                            EventPublisher.Publish(new EditCaseExpressionMessage { ModelItem = item, EnvironmentModel = _resourceModel.Environment });
                        }
                    }

                    // Handle Switch Edits
                    if(dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                       item.ItemType == typeof(FlowSwitch<string>))
                    {
                        EventPublisher.Publish(new ConfigureSwitchExpressionMessage { ModelItem = item, EnvironmentModel = _resourceModel.Environment });
                    }

                    // Handle Decision Edits
                    if(dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                       item.ItemType == typeof(FlowDecision))
                    {
                        EventPublisher.Publish(new ConfigureDecisionExpressionMessage { ModelItem = item, EnvironmentModel = _resourceModel.Environment });
                    }
                }


                if(designerView != null && designerView.FocusedViewElement != null &&
                   designerView.FocusedViewElement.ModelItem != null)
                {
                    ModelItem modelItem = designerView.FocusedViewElement.ModelItem;

                    if(modelItem.ItemType == typeof(DsfWebPageActivity) ||
                       modelItem.ItemType == typeof(DsfWebSiteActivity))
                    {
                        IWebActivity webpageActivity = WebActivityFactory.CreateWebActivity(modelItem, _resourceModel,
                            modelItem.Properties["DisplayName"]
                                .ComputedValue.ToString());
                        EventPublisher.Publish(new AddWorkSurfaceMessage(webpageActivity));
                        return true;
                    }
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
            DataObject = e.Data.GetData(typeof(ResourceTreeViewModel));

            if(DataObject != null && DataObject.IsNew)
            {
                IsItemDragged.Instance.IsDragged = true;
            }

            var isWorkflow = e.Data.GetData("WorkflowItemTypeNameFormat") as string;
            if(isWorkflow != null)
            {


                if(isWorkflow.Contains("DsfMultiAssignActivity") || isWorkflow.Contains("DsfWebGetRequestActivity"))
                {
                    var overlayService = _wd.Context.Services.GetService<OverlayService>();
                    if(overlayService == null)
                    {
                        overlayService = new OverlayService { OnLoadOverlayType = OverlayType.LargeView };
                        _wd.Context.Services.Publish(overlayService);
                    }
                    else
                    {
                        overlayService.OnLoadOverlayType = OverlayType.LargeView;
                    }
                }

                _vm = DsfActivityDropUtils.DetermineDropActivityType(isWorkflow);

                if(_vm != null)
                {
                    _vm.Init();
                    if(!DsfActivityDropUtils.DoDroppedActivity(_vm))
                    {
                        e.Handled = true;
                        dropOccured = false;
                    }
                }
            }
            if(dropOccured)
            {
                ResourceModel.IsWorkflowSaved = false;
                NotifyOfPropertyChange(() => DisplayName);
            }
        }

        // BUG 9143 - 2013.07.03 - TWR - added
        //
        // Activity : Next
        // Decision : True, False
        // Switch   : Default, Key
        //
        public static readonly string[] SelfConnectProperties = new[]
            {
                "Next", 
                "True", 
                "False", 
                "Default", 
                "Key"
            };

        string _originalDataList;

        /// <summary>
        /// Models the service model changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ModelChangedEventArgs"/> instance containing the event data.</param>
        protected void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        {
            // BUG 9143 - 2013.07.03 - TWR - added
            if(e.ModelChangeInfo != null &&
                e.ModelChangeInfo.ModelChangeType == ModelChangeType.PropertyChanged)
            {
                if(SelfConnectProperties.Contains(e.ModelChangeInfo.PropertyName))
                {
                    if(e.ModelChangeInfo.Subject == e.ModelChangeInfo.Value)
                    {
                        var modelProperty = e.ModelChangeInfo.Value.Properties[e.ModelChangeInfo.PropertyName];
                        if(modelProperty != null)
                        {
                            modelProperty.ClearValue();
                        }
                    }
                    return;
                }

                if(e.ModelChangeInfo.PropertyName == "StartNode")
                {
                    return;
                }
            }

            //ItemsAdded is obsolete - see e.ModelChangeInfo for correct usage
            //Code below is obsolete
            if(e.ItemsAdded != null)
            {
                PerformAddItems(e.ItemsAdded.ToList());
            }
            else if(e.PropertiesChanged != null)
            {
                if(e.PropertiesChanged.Any(mp => mp.Name == "Handler"))
                {
                    if(DataObject != null)
                    {
                        var navigationItemViewModel = DataObject as ResourceTreeViewModel;
                        ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");

                        if(navigationItemViewModel != null && modelProperty != null)
                        {
                            var resource = navigationItemViewModel.DataContext as IContextualResourceModel;

                            if(resource != null)
                            {
                                //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                                DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true);
                                d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                                d.IconPath = resource.IconPath;

                                modelProperty.SetValue(d);
                            }
                        }

                        DataObject = null;
                    }
                    else
                    {
                        ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");

                        if(modelProperty != null)
                        {
                            if(_vm != null)
                            {
                                IContextualResourceModel resource = _vm.SelectedResourceModel;
                                if(resource != null)
                                {
                                    DsfActivity droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, null, true);

                                    droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.ResourceName;
                                    droppedActivity.IconPath = resource.IconPath;

                                    modelProperty.SetValue(droppedActivity);
                                }
                                _vm.Dispose();
                                _vm = null;
                            }
                        }
                    }
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
        protected void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if(e.Command == ApplicationCommands.Delete ||      //triggered from deleting an activity
                e.Command == EditingCommands.Delete ||          //triggered from editing displayname, expressions, etc
                e.Command == System.Activities.Presentation.View.DesignerView.CopyCommand ||
                e.Command == System.Activities.Presentation.View.DesignerView.CutCommand)
            {
                PreventCommandFromBeingExecuted(e);
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
            if(e.Command == ApplicationCommands.Delete)
            {
                //2013.06.24: Ashley Lewis for bug 9728 - delete event sends focus to a strange place
                _wd.View.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }

        #endregion

        #region OnDispose

        protected override void OnDispose()
        {
            _wd = null;
            _designerManagementService.Dispose();
            base.OnDispose();
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

        #region Implementation of IHandle<ShowActivityWizardMessage>

        public void Handle(ShowActivityWizardMessage message)
        {
            ShowActivityWizard(message.ModelItem);
        }

        #endregion

        #region Implementation of IHandle<ShowActivitySettingsWizardMessage>

        public void Handle(ShowActivitySettingsWizardMessage message)
        {
            ShowActivitySettingsWizard(message.ModelItem);
        }

        #endregion

        #region Implementation of IHandle<EditActivityMessage>

        public void Handle(EditActivityMessage message)
        {
            EditActivity(message.ModelItem, message.ParentEnvironmentID, message.EnvironmentRepository);
        }

        #endregion

        protected override void OnDeactivate(bool close)
        {
            if(close)
            {

            }
            base.OnDeactivate(close);
        }

        public void FireWdChanged()
        {
            WdOnModelChanged(new object(), new EventArgs());
        }
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
