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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Dev2.Studio.ActivityDesigners;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
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
using Unlimited.Framework;

#endregion

namespace Dev2.Studio.ViewModels.Workflow
{
    public class WorkflowDesignerViewModel : BaseWorkSurfaceViewModel,
                                             IWorkflowDesignerViewModel, IDisposable,
                                             IHandle<UpdateResourceMessage>,
                                             IHandle<AddStringListToDataListMessage>,
                                             IHandle<AddRemoveDataListItemsMessage>,
                                             IHandle<ShowActivityWizardMessage>,
                                             IHandle<ShowActivitySettingsWizardMessage>,
                                             IHandle<EditActivityMessage>
    {
        #region Fields

        public delegate void SourceLocationEventHandler(SourceLocation src);

        readonly IDesignerManagementService _designerManagementService;
        readonly IWorkflowHelper _workflowHelper;

        RelayCommand _collapseAllCommand;

        dynamic _dataObject;
        RelayCommand _expandAllCommand;
        IList<IDataListVerifyPart> _filteredDataListParts;
        ModelItem _lastDroppedModelItem;
        Point _lastDroppedPoint;
        ModelService _modelService;
        UserControl _popupContent;
        IContextualResourceModel _resourceModel;
        Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts;
        ViewStateService _viewstateService;
        WorkflowDesigner _wd;
        DesignerMetadata _wdMeta;
        DsfActivityDropViewModel _vm;
        string _originalWorkflowXaml;

        #endregion

        #region Constructor

        public WorkflowDesignerViewModel(IContextualResourceModel resource, bool createDesigner = true)
            : this(resource, WorkflowHelper.Instance, createDesigner)
        {
        }

        // BUG 9304 - 2013.05.08 - TWR - Added IWorkflowHelper parameter to facilitate testing
        public WorkflowDesignerViewModel(IContextualResourceModel resource, IWorkflowHelper workflowHelper, bool createDesigner = true)
        {
            if (workflowHelper == null)
            {
                throw new ArgumentNullException("workflowHelper");
            }
            _workflowHelper = workflowHelper;

            SecurityContext = ImportService.GetExportValue<IFrameworkSecurityContext>();
            PopUp = ImportService.GetExportValue<IPopupController>();
            WizardEngine = ImportService.GetExportValue<IWizardEngine>();
            _resourceModel = resource;
            _designerManagementService = new DesignerManagementService(_resourceModel.Environment.ResourceRepository);
            if (createDesigner)
            {
                ActivityDesignerHelper.AddDesignerAttributes(this);
            }
            OutlineViewTitle = "Navigation Pane";
        }


        //
        //        void ViewOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        //        {
        //            var key = keyEventArgs.Key;
        //
        //            if (key == Key.LeftCtrl)
        //            {
        //                switch (key)
        //                {
        //                    case Key.C:
        //                    case Key.P:
        //                    case Key.X:
        //                        keyEventArgs.Handled = true;
        //                        break;
        //                }
        //            }
        //
        //            if (key == Key.OemCopy)
        //            {
        //                   keyEventArgs.Handled = true;
        //            }
        //        }

        #endregion

        #region Properties

        public override string DisplayName
        {
            get { return ResourceHelper.GetDisplayName(ResourceModel); }
        }

        public override string IconPath
        {
            get { return ResourceHelper.GetIconPath(ResourceModel); }
        }

        public IFrameworkSecurityContext SecurityContext { get; set; }

        //2012.10.01: massimo.guerrera - Add Remove buttons made into one:)
        public IPopupController PopUp { get; set; }

        public IWizardEngine WizardEngine { get; set; }

        public IList<IDataListVerifyPart> WorkflowVerifiedDataParts { get { return _filteredDataListParts; } }

        public UserControl PopupContent
        {
            get { return _popupContent; }
            set
            {
                _popupContent = value;
                NotifyOfPropertyChange(() => PopupContent);
            }
        }

        public object SelectedModelItem
        {
            get
            {
                if (_wd != null)
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

        public UIElement DesignerView { get { return _wd.View; } }

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
                if (_collapseAllCommand == null)
                {
                    _collapseAllCommand = new RelayCommand(param =>
                    {
                        bool val = Convert.ToBoolean(param);
                        if (val)
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
                if (_expandAllCommand == null)
                {
                    _expandAllCommand = new RelayCommand(param =>
                    {
                        bool val = Convert.ToBoolean(param);
                        if (val)
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

        void PerformAddItems(List<ModelItem> addedItems)
        {
            for (int i = 0; i < addedItems.Count(); i++)
            {
                ModelItem mi = addedItems.ToList()[i];

                if (mi != null && (mi.Parent.Parent.Parent != null) && mi.Parent.Parent.Parent.ItemType == typeof(FlowSwitch<string>))
                {
                    #region Extract the Switch Expression ;)

                    ModelProperty activityExpression = mi.Parent.Parent.Parent.Properties["Expression"];

                    var tmpModelItem = activityExpression.Value;

                    var switchExpressionValue = string.Empty;

                    if (tmpModelItem != null)
                    {
                        var tmpProperty = tmpModelItem.Properties["ExpressionText"];

                        if (tmpProperty != null)
                        {
                            var tmp = tmpProperty.Value.ToString();
                            // Dev2DecisionHandler.Instance.FetchSwitchData("[[res]]",AmbientDataList)

                            if (!string.IsNullOrEmpty(tmp))
                            {
                                int start = tmp.IndexOf("(");
                                int end = tmp.IndexOf(",");

                                if (start < end && start >= 0)
                                {
                                    start += 2;
                                    end -= 1;
                                    switchExpressionValue = tmp.Substring(start, (end - start));
                                }
                            }
                        }
                    }

                    #endregion

                    if ((mi.Properties["Key"].Value != null) && mi.Properties["Key"].Value.ToString().Contains("Case"))
                    {
                        Tuple<ConfigureCaseExpressionTO, IEnvironmentModel> wrapper = new Tuple<ConfigureCaseExpressionTO, IEnvironmentModel>(new ConfigureCaseExpressionTO() { TheItem = mi, ExpressionText = switchExpressionValue }, _resourceModel.Environment);
                        EventAggregator.Publish(new ConfigureCaseExpressionMessage(wrapper));
                    }
                }

                if (mi.ItemType == typeof(FlowSwitch<string>))
                {
                    //This line is necessary to fix the issue were decisions and switches didn't have the correct positioning when dragged on
                    SetLastDroppedModelItem(mi);

                    // Travis.Frisinger : 28.01.2013 - Switch Amendments
                    Tuple<ModelItem, IEnvironmentModel> wrapper = new Tuple<ModelItem, IEnvironmentModel>(mi, _resourceModel.Environment);
                    EventAggregator.Publish(new ConfigureSwitchExpressionMessage(wrapper));
                }

                if (mi.ItemType == typeof(FlowDecision))
                {
                    //This line is necessary to fix the issue were decisions and switches didn't have the correct positioning when dragged on
                    SetLastDroppedModelItem(mi);

                    //2013.06.22: Ashley Lewis for bug 9717 - dont show wizard on copy paste
                    var tmpProperty = (mi.Properties["Condition"].ComputedValue as DsfFlowNodeActivity<bool>).ExpressionText;
                    if (tmpProperty == null)
                    {
                        var wrapper = new Tuple<ModelItem, IEnvironmentModel>(mi, _resourceModel.Environment);
                        EventAggregator.Publish(new ConfigureDecisionExpressionMessage(wrapper));
                    }
                }

                if (mi.ItemType == typeof(FlowStep))
                {
                    if (mi.Properties["Action"].ComputedValue is DsfWebPageActivity)
                    {
                        var modelService = Designer.Context.Services.GetService<ModelService>();
                        var items = modelService.Find(modelService.Root, typeof(DsfWebPageActivity));

                        int totalActivities = items.Count();

                        items.Last().Properties["DisplayName"].SetValue(string.Format("Webpage {0}", totalActivities.ToString()));
                    }

                    if (mi.Properties["Action"].ComputedValue is DsfActivity)
                    {
                        DsfActivity droppedActivity = mi.Properties["Action"].ComputedValue as DsfActivity;

                        if (!string.IsNullOrEmpty(droppedActivity.ServiceName))
                        {
                            //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                            IContextualResourceModel resource = _resourceModel.Environment.ResourceRepository.FindSingle(
                                c => c.ResourceName == droppedActivity.ServiceName) as IContextualResourceModel;
                            droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false);
                            droppedActivity.EnvironmentID = resource.Environment.ID;
                            mi.Properties["Action"].SetValue(droppedActivity);
                        }
                        else
                        {
                            if (_dataObject != null)
                            {
                                var navigationItemViewModel = _dataObject as ResourceTreeViewModel;

                                if (navigationItemViewModel != null)
                                {
                                    var resource = navigationItemViewModel.DataContext;

                                    if (resource != null)
                                    {
                                        //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                                        DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, true);
                                        d.EnvironmentID = resource.Environment.ID;
                                        d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                                        d.IconPath = resource.IconPath;
                                        CheckIfRemoteWorkflowAndSetProperties(d, resource);
                                        mi.Properties["Action"].SetValue(d);
                                    }
                                }

                                _dataObject = null;
                            }
                            else
                            {
                                //Massimo.Guerrera:17-04-2012 - PBI 9000                               
                                if (_vm != null)
                                {
                                    IContextualResourceModel resource = _vm.SelectedResourceModel;
                                    if (resource != null)
                                    {
                                        droppedActivity.ServiceName = droppedActivity.DisplayName = droppedActivity.ToolboxFriendlyName = resource.ResourceName;
                                        droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false);
                                        droppedActivity.EnvironmentID = resource.Environment.ID;
                                        mi.Properties["Action"].SetValue(droppedActivity);
                                    }
                                    _vm = null;
                                }
                            }
                        }
                    }
                }
                i++;
            }
        }

        void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource)
        {
            var mvm = Application.Current.MainWindow.DataContext as MainViewModel;
            if (mvm != null && mvm.ActiveItem != null)
            {
                CheckIfRemoteWorkflowAndSetProperties(dsfActivity, resource, mvm.ActiveItem.Environment);
            }
        }

        protected void CheckIfRemoteWorkflowAndSetProperties(DsfActivity dsfActivity, IContextualResourceModel resource, IEnvironmentModel contextEnv)
        {
            if (resource.ResourceType == ResourceType.WorkflowService)
            {
                if (contextEnv.ID != resource.Environment.ID)
                {
                    dsfActivity.ServiceUri = resource.Environment.Connection.WebServerUri.AbsoluteUri;
                    dsfActivity.ServiceServer = resource.Environment.ID;
                }
            };
        }

        void EditActivity(ModelItem modelItem)
        {
            if(Designer == null)
                return;
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if(modelService.Root == modelItem.Root && (modelItem.ItemType == typeof(DsfActivity) || modelItem.ItemType.BaseType == typeof(DsfActivity)))
            {
                var modelProperty = modelItem.Properties["ServiceName"];
                if(modelProperty != null)
                {
                    var modelPropertyServer = modelItem.Properties["EnvironmentID"];

                    if(modelPropertyServer != null)
                    {
                        InArgument<Guid> serverId = modelPropertyServer.ComputedValue as InArgument<Guid>;
                        string serverIdString="";
                        if(serverId == null)
                        {
                            serverIdString = Guid.Empty.ToString();
                        }
                        else
                        {
                            serverIdString = serverId.Expression.ToString();
                        }
                        Guid serverGuid;
                        if(Guid.TryParse(serverIdString, out serverGuid))
                        {
                            IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == serverGuid);

                            var res = modelProperty.ComputedValue;

                            if(environmentModel != null)
                            {
                                var resource =
                                    environmentModel.ResourceRepository.FindSingle(c => c.ResourceName == res.ToString());

                                if(resource != null)
                                {
                                    switch(resource.ResourceType)
                                    {
                                        case ResourceType.WorkflowService:
                                            EventAggregator.Publish(new AddWorkSurfaceMessage(resource));
                                            break;

                                        case ResourceType.Service:
                                            EventAggregator.Publish(new ShowEditResourceWizardMessage(resource));
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        void ShowActivitySettingsWizard(ModelItem modelItem)
        {
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root)
            {
                if (WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;
                try
                {
                    activityWizardTO = WizardEngine.GetActivitySettingsWizardInvocationTO(modelItem, _resourceModel);
                }
                catch (Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        void ShowActivityWizard(ModelItem modelItem)
        {
            var modelService = Designer.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root)
            {
                if (WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;


                if (!WizardEngine.HasWizard(modelItem, _resourceModel.Environment))
                {
                    PopUp.Show("Wizard not found.", "Missing Wizard");
                    return;
                }
                try
                {
                    activityWizardTO = WizardEngine.GetActivityWizardInvocationTO(modelItem, _resourceModel);
                }
                catch (Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        void ShowWizard(WizardInvocationTO activityWizardTO)
        {
            if (activityWizardTO != null)
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

                if (showPopUp)
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
            var senderAsFrameworkElement = _modelService.Root.View as FrameworkElement;
            if (senderAsFrameworkElement != null)
            {
                UIElement freePormPanel = senderAsFrameworkElement.FindNameAcrossNamescopes("flowchartPanel");
                if (freePormPanel != null)
                {
                    _lastDroppedPoint = e.GetPosition(freePormPanel);
                }
            }
        }

        /// <summary>
        ///     Sets the last dropped model item.
        /// </summary>
        /// <param name="modelItem">The model item.</param>
        void SetLastDroppedModelItem(ModelItem modelItem)
        {
            _lastDroppedModelItem = modelItem;
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
                        //workflowFields = GetDecisionElements((activity as dynamic).ExpressionText);
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
            var DecisionFields = new List<string>();
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
                    var dds = c.ConvertFromJsonToModel<Dev2DecisionStack>(decisionValue.Replace('!', '\"'));
                    foreach (var decision in dds.TheStack)
                    {
                        var getCols = new[] { decision.Col1, decision.Col2, decision.Col3 };
                        for (var i = 0; i < 3; i++)
                        {
                            var getCol = getCols[i];
                            DecisionFields = DecisionFields.Union(GetParsedRegions(getCol, datalistModel)).ToList();
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
                        DecisionFields.Add(DataListUtil.StripBracketsFromValue(part.Option.DisplayValue));
                    }
                }
            }
            return DecisionFields;
        }

        static List<string> GetParsedRegions(string getCol, IDataListViewModel datalistModel)
        {
            var result = new List<string>();

            // Travis.Frisinger - 25.01.2013 
            // We now need to parse this data for regions ;)

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            // NEED - DataList for active workflow
            var parts = parser.ParseDataLanguageForIntellisense(getCol, datalistModel.WriteToResourceModel(), true);

            foreach (var intellisenseResult in parts)
            {
                if(intellisenseResult.Type != enIntellisenseResultType.Selectable)//selectables are in the datalist already
                {
                    getCol = DataListUtil.StripBracketsFromValue(intellisenseResult.Option.DisplayValue);
                    if(!string.IsNullOrEmpty(getCol))
                    {
                        result.Add(getCol);
                    }
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
            if (fieldList.Count() > 1 && !String.IsNullOrEmpty(fieldList[0]))
            {
                // If it's a RecordSet Containing a field
                foreach (var item in fieldList)
                {
                    if (item.EndsWith(")") && item == fieldList[0])
                    {
                        if (item.Contains("("))
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
                    else if (item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
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
            else if (fieldList.Count() == 1 && !String.IsNullOrEmpty(fieldList[0]))
            {
                // If the workflow field is simply a scalar or a record set without a child
                if (DataPartFieldData.EndsWith(")") && DataPartFieldData == fieldList[0])
                {
                    if (DataPartFieldData.Contains("("))
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

        List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify)
        {
            var MissingDataParts = new List<IDataListVerifyPart>();
            foreach (var part in partsToVerify)
            {
                if (DataListSingleton.ActiveDataList != null)
                {
                    if (!(part.IsScalar))
                    {
                        var recset =
                            DataListSingleton.ActiveDataList.DataList.Where(
                                c => c.Name == part.Recordset && c.IsRecordset).ToList();
                        if (!recset.Any())
                        {
                            MissingDataParts.Add(part);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(part.Field) &&
                               recset[0].Children.Count(c => c.Name == part.Field) == 0)
                            {
                                MissingDataParts.Add(part);
                            }
                        }
                    }
                    else if (DataListSingleton.ActiveDataList.DataList
                                             .Count(c => c.Name == part.Field && !c.IsRecordset) == 0)
                    {
                        MissingDataParts.Add(part);
                    }
                }
            }
            return MissingDataParts;
        }

        public string GetValueFromUnlimitedObject(UnlimitedObject activityField, string valueToGet)
        {
            List<UnlimitedObject> enumerableResultSet = activityField.GetAllElements(valueToGet);
            string result = string.Empty;
            foreach (var item in enumerableResultSet)
            {
                if (!string.IsNullOrEmpty(item.GetValue(valueToGet)))
                {
                    result = item.GetValue(valueToGet);
                }
            }

            return result;
        }

        string RemoveRecordSetBrace(string RecordSet)
        {
            string fullyFormattedStringValue;
            if (RecordSet.Contains("(") && RecordSet.Contains(")"))
            {
                fullyFormattedStringValue = RecordSet.Remove(RecordSet.IndexOf("("));
            }
            else
                return RecordSet;
            return fullyFormattedStringValue;
        }

        void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart)
        {
            if (!_uniqueWorkflowParts.ContainsValue(nameOfPart))
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
            if (dlvm != null)
            {
                var dataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
                _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(dataPartVerifyDuplicates);
                foreach (var s in message.ListToAdd)
                {
                    BuildDataPart(s);
                }
                IList<IDataListVerifyPart> partsToAdd = _uniqueWorkflowParts.Keys.ToList();
                List<IDataListVerifyPart> uniqueDataListPartsToAdd = MissingDataListParts(partsToAdd);
                dlvm.AddMissingDataListItems(uniqueDataListPartsToAdd);
            }
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(UpdateResourceMessage message)
        {
            if (
                ContexttualResourceModelEqualityComparer.Current.Equals(
                    message.ResourceModel as IContextualResourceModel, _resourceModel))
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
            _resourceModel.IsWorkflowSaved = true;
            if (string.IsNullOrEmpty(_resourceModel.ServiceDefinition))
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

            _wd.Context.Services.Subscribe<ModelService>(instance =>
                        {
                            _modelService = instance;
                            _modelService.ModelChanged += ModelServiceModelChanged;
                        });

            if (string.IsNullOrEmpty(_resourceModel.WorkflowXaml))
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

            _wdMeta.Register();

            _wd.Context.Services.Subscribe<ViewStateService>(instance =>
            {
                _viewstateService = instance;
            });

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

            //2013.06.26: Ashley Lewis for bug 9728 - set focus after delete activity
            CommandManager.AddPreviewExecutedHandler(_wd.View, ExecuteRoutedEventHandler);

            // BUG 9304 - 2013.05.08 - TWR
            _workflowHelper.EnsureImplementation(_modelService);
        }

        void WdOnModelChanged(object sender, EventArgs eventArgs)
        {
            ResourceModel.IsWorkflowSaved = false;
        }

        /// <summary>
        /// Adds the missing with no pop up and find unused data list items.
        /// </summary>
        public void AddMissingWithNoPopUpAndFindUnusedDataListItems()
        {
            if (DataListSingleton.ActiveDataList != null)
            {
                WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();
                IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
                IList<IDataListVerifyPart> removeParts = wdu.MissingWorkflowItems(workflowFields);
                _filteredDataListParts = MissingDataListParts(workflowFields);
                var eventAggregator = ImportService.GetExportValue<IEventAggregator>();

                if (eventAggregator != null)
                {
                    // Allow it to always fire becuse we need to make un-used parts now used as active again ;)
                    eventAggregator.Publish(new ShowUnusedDataListVariablesMessage(removeParts, ResourceModel));

                    // Be more intelligent about when we fire ;)
                    if (_filteredDataListParts.Count > 0)
                    {
                        eventAggregator.Publish(new AddMissingDataListItems(_filteredDataListParts, ResourceModel));
                    }
                }
            }
        }

        /// <summary>
        /// Finds the unused data list items.
        /// </summary>
        public void FindUnusedDataListItems()
        {
            WorkflowDesignerUtils wdu = new WorkflowDesignerUtils();
            IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
            IList<IDataListVerifyPart> removeParts = wdu.MissingWorkflowItems(workflowFields);

            var eventAggregator = ImportService.GetExportValue<IEventAggregator>();

            if (eventAggregator != null)
            {
                eventAggregator.Publish(new ShowUnusedDataListVariablesMessage(removeParts, ResourceModel));
            }
        }

        /// <summary>
        /// Removes all unused data list items.
        /// </summary>
        /// <param name="dataListViewModel">The data list view model.</param>
        public void RemoveAllUnusedDataListItems(IDataListViewModel dataListViewModel)
        {
            if (dataListViewModel != null && ResourceModel == dataListViewModel.Resource)
            {
                dataListViewModel.RemoveUnusedDataListItems();
            }
        }

        /// <summary>
        /// Adds the missing only with no pop up.
        /// </summary>
        /// <param name="dataListViewModel">The data list view model.</param>
        public void AddMissingOnlyWithNoPopUp(IDataListViewModel dataListViewModel)
        {
            IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
            _filteredDataListParts = MissingDataListParts(workflowFields);
            if (dataListViewModel != null && ResourceModel == dataListViewModel.Resource)
            {
                dataListViewModel.AddMissingDataListItems(_filteredDataListParts);
            }
            else
            {
                var eventAggregator = ImportService.GetExportValue<IEventAggregator>();

                if (eventAggregator != null)
                {
                    eventAggregator.Publish(new AddMissingDataListItems(_filteredDataListParts, ResourceModel));
                }
            }
        }

        #endregion

        #region Event Handlers       

        void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dcontxt = (e.Source as dynamic).DataContext;
            var vm = dcontxt as WorkflowDesignerViewModel;

            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                var designerView = e.Source as DesignerView;
                if (designerView != null && designerView.FocusedViewElement == null)
                {
                    e.Handled = true;
                    return;
                }

                var item = SelectedModelItem as ModelItem;

                // Travis.Frisinger - 28.01.2013 : Case Amendments
                if (item != null)
                {
                    var dp = e.OriginalSource as DependencyObject;
                    string itemFn = item.ItemType.FullName;

                    //2013.03.20: Ashley Lewis - Bug 9202 Don't open any wizards if the source is a 'Microsoft.Windows.Themes.ScrollChrome' object
                    if (dp != null &&
                       string.Equals(dp.ToString(), "Microsoft.Windows.Themes.ScrollChrome",
                           StringComparison.InvariantCulture))
                    {
                        WizardEngineAttachedProperties.SetDontOpenWizard(dp, true);
                    }

                    // Handle Case Edits
                    if (item != null &&
                       itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchCaseLink",
                           StringComparison.Ordinal)
                       &&
                       !itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchDefaultLink",
                           StringComparison.Ordinal))
                    {
                        ModelProperty tmp = item.Properties["Case"];

                        if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp))
                        {
                            EventAggregator.Publish(
                                new EditCaseExpressionMessage(new Tuple<ModelProperty, IEnvironmentModel>(tmp,
                                    _resourceModel
                                        .Environment)));
                        }
                    }

                    // Handle Switch Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                       item.ItemType == typeof(FlowSwitch<string>))
                    {
                        EventAggregator.Publish(
                            new ConfigureSwitchExpressionMessage(new Tuple<ModelItem, IEnvironmentModel>(item,
                                _resourceModel
                                    .Environment)));
                    }

                    // Handle Decision Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) &&
                       item.ItemType == typeof(FlowDecision))
                    {
                        EventAggregator.Publish(
                            new ConfigureDecisionExpressionMessage(new Tuple<ModelItem, IEnvironmentModel>(item,
                                _resourceModel
                                    .Environment)));
                    }
                }


                if (designerView != null && designerView.FocusedViewElement != null &&
                   designerView.FocusedViewElement.ModelItem != null)
                {
                    ModelItem modelItem = designerView.FocusedViewElement.ModelItem;

                    if (modelItem.ItemType == typeof(DsfWebPageActivity) ||
                       modelItem.ItemType == typeof(DsfWebSiteActivity))
                    {
                        IWebActivity webpageActivity = WebActivityFactory.CreateWebActivity(modelItem, _resourceModel,
                            modelItem.Properties["DisplayName"]
                                .ComputedValue.ToString());
                        EventAggregator.Publish(new AddWorkSurfaceMessage(webpageActivity));
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Views the preview drop.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        void ViewPreviewDrop(object sender, DragEventArgs e)
        {
            SetLastDroppedPoint(e);
            _dataObject = e.Data.GetData(typeof(ResourceTreeViewModel));
            var isWorkflow = e.Data.GetData("WorkflowItemTypeNameFormat") as string;
            if (isWorkflow != null)
            {
                if (isWorkflow.Contains("DsfFlowDecisionActivity") || isWorkflow.Contains("DsfFlowSwitchActivity"))
                {
                    FlowNodeActivityDropUtils.RegisterFlowNodeDrop(_viewstateService, _lastDroppedPoint);
                }

                _vm = DsfActivityDropUtils.DetermineDropActivityType(isWorkflow);

                if (_vm != null)
                {
                    _vm.Init();
                    if (!DsfActivityDropUtils.DoDroppedActivity(_vm))
                    {
                        e.Handled = true;
                    }
                }
            }

        }

        /// <summary>
        /// Models the service model changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ModelChangedEventArgs"/> instance containing the event data.</param>
        void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        {
            if (e.ItemsAdded != null)
            {
                PerformAddItems(e.ItemsAdded.ToList());
            }
            else if (e.PropertiesChanged != null)
            {
                if (e.PropertiesChanged.Any(mp => mp.Name == "Handler"))
                {
                    if (_dataObject != null)
                    {
                        var navigationItemViewModel = _dataObject as ResourceTreeViewModel;
                        ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");

                        if (navigationItemViewModel != null && modelProperty != null)
                        {
                            var resource = navigationItemViewModel.DataContext as IContextualResourceModel;

                            if (resource != null)
                            {
                                //06-12-2012 - Massimo.Guerrera - Added for PBI 6665
                                DsfActivity d = DsfActivityFactory.CreateDsfActivity(resource, null, true);
                                d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                                d.IconPath = resource.IconPath;

                                modelProperty.SetValue(d);
                            }
                        }

                        _dataObject = null;
                    }
                    else
                    {
                        ModelProperty modelProperty = e.PropertiesChanged.FirstOrDefault(mp => mp.Name == "Handler");

                        if (modelProperty != null)
                        {
                            if (_vm != null)
                            {
                                IContextualResourceModel resource = _vm.SelectedResourceModel;
                                if (resource != null)
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
        void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete ||      //triggered from deleting an activity
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
        void ExecuteRoutedEventHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                //2013.06.24: Ashley Lewis for bug 9728 - can only undo this command if focus has been changed, yes, this is a hack
                _wd.View.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }
                

        #endregion

        #region Dispose

        public new void Dispose()
        {
            //MediatorRepo.deregisterAllItemMessages(this.GetHashCode());
            _wd = null;
            _designerManagementService.Dispose();
            EventAggregator.Unsubscribe(this);
            base.Dispose();
        }

        #endregion Dispose

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


        #region Implementation of IHandle<AddRemoveDataListItemsMessage>

        public void Handle(AddRemoveDataListItemsMessage message)
        {
            RemoveAllUnusedDataListItems(message.DataListViewModel);
        }

        #endregion

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
            EditActivity(message.ModelItem);
        }

        #endregion
    }
}
