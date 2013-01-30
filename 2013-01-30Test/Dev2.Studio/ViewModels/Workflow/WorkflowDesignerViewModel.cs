using Dev2.Data.Decision;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Interfaces;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Services;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.ViewModels.Navigation;
using Dev2.Studio.ViewModels.Wizards;
using Dev2.Studio.Views;
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
using System.ComponentModel.Composition;
using System.Linq;
using System.Parsing.Intellisense;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework;

namespace Dev2.Studio.ViewModels.Workflow
{
    public class WorkflowDesignerViewModel : BaseViewModel, IWorkflowDesignerViewModel, IDisposable
    {
        #region Fields

        private ViewStateService _viewstateService;
        private WorkflowDesigner _wd;
        private DesignerMetadata _wdMeta;
        private ModelService _modelService;
        private Dictionary<IDataListVerifyPart, string> _uniqueWorkflowParts;
        private IList<IDataListVerifyPart> _filteredDataListParts;

        private readonly IContextualResourceModel _workflowModel;
        private readonly IDesignerManagementService _designerManagementService;

        private RelayCommand _newWorkflowCommand;
        private RelayCommand _editWorkflowCommand;

        private RelayCommand _collapseAllCommand;
        private RelayCommand _expandAllCommand;

        public delegate void SourceLocationEventHandler(SourceLocation src);

        dynamic _dataObject = null;
        private Point _lastDroppedPoint;
        private ModelItem _lastDroppedModelItem = null;
        private UserControl _popupContent;

        #endregion

        #region Events

        public event ResourceEventHandler OnRequestCreateNewResource;
        public event ResourceEventHandler OnRequestEditResource;

        #endregion

        #region Constructor

        public WorkflowDesignerViewModel(IContextualResourceModel resource)
        {
            _workflowModel = resource;
            _designerManagementService = new DesignerManagementService(_workflowModel.Environment.Resources);
        }
        #endregion

        #region Properties

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        //2012.10.01: massimo.guerrera - Add Remove buttons made into one:)
        [Import]
        public IPopUp PopUp { get; set; }

        [Import]
        public IMediatorRepo MediatorRepo { get; set; }

        [Import]
        public IWizardEngine WizardEngine { get; set; }

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

        public WorkflowDesigner wfDesigner
        {
            get
            {
                return _wd;
            }
        }

        public bool HasErrors
        {
            get;
            set;
        }

        public IList<IDataListVerifyPart> WorkflowVerifiedDataParts
        {
            get
            {
                return _filteredDataListParts;
            }
        }

        public IContextualResourceModel ResourceModel
        {
            get
            {
                return _workflowModel;
            }
        }

        public string WorkflowName
        {
            get
            {
                return _workflowModel.ResourceName;
            }
        }

        public string ServiceDefinition
        {
            get
            {
                _wd.Flush();
                return _wd.Text;
            }
            set
            {
                _wd.Flush();
                _workflowModel.WorkflowXaml = _wd.Text;
            }
        }

        public bool RequiredSignOff
        {
            get
            {
                return _workflowModel.RequiresSignOff;
            }
        }

        public string AuthorRoles
        {
            get
            {
                return _workflowModel.AuthorRoles;
            }
            set
            {
                _workflowModel.AuthorRoles = value;
            }
        }

        public WorkflowDesigner Designer
        {
            get
            {
                return _wd;
            }
        }

        public UIElement DesignerView
        {
            get
            {
                return _wd.View;
            }
        }

        public UIElement PropertyView
        {
            get
            {
                return _wd.PropertyInspectorView;
            }
        }

        public string DesignerText
        {
            get
            {
                return _wd.Text;
            }
        }

        public UserControl PopupContent
        {
            get
            {
                return _popupContent;
            }
            set
            {
                _popupContent = value;
                OnPropertyChanged("PopupContent");
            }
        }

        #endregion

        #region Commands

        public ICommand NewWorkflowCommand
        {
            get
            {
                if (_newWorkflowCommand == null)
                {
                    _newWorkflowCommand = new RelayCommand(param => { if (OnRequestCreateNewResource != null) OnRequestCreateNewResource(_workflowModel); }, param => true);
                }
                return _newWorkflowCommand;
            }
        }

        public ICommand EditWorkflowCommand
        {
            get
            {
                if (_editWorkflowCommand == null)
                {
                    _editWorkflowCommand = new RelayCommand(param => { if (OnRequestEditResource != null) OnRequestEditResource(_workflowModel); }, param => true);
                }
                return _editWorkflowCommand;
            }
        }

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

        /// <summary>
        /// Updates the location of last dropped model item. 
        /// !!!!!!!!!!!!!!!!!!This method is called alot, please ensure all logic stays in the if statement which checks for nulls AND avoid doing any heavy work in it!
        /// </summary>
        private void UpdateLocationOfLastDroppedModelItem()
        {
            if (_lastDroppedModelItem != null && _lastDroppedModelItem.View != null)
            {
                AutomationProperties.SetAutomationId(_lastDroppedModelItem.View, _lastDroppedModelItem.ItemType.ToString());
                _viewstateService.StoreViewState(_lastDroppedModelItem, "ShapeLocation", _lastDroppedPoint);
                _lastDroppedModelItem = null;
            }
        }

        private void EditActivity(ModelItem modelItem)
        {
            var modelService = wfDesigner.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root && modelItem.ItemType == typeof(DsfActivity))
            {

                var modelProperty = modelItem.Properties["ServiceName"];
                if (modelProperty != null)
                {
                    var res = modelProperty.ComputedValue;

                    var resource =
                        _workflowModel.Environment.Resources.FindSingle(c => c.ResourceName == res.ToString());

                    if (resource != null)
                    {
                        switch (resource.ResourceType)
                        {
                            case ResourceType.WorkflowService:
                                Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, resource);
                                break;

                            case ResourceType.Service:
                                Mediator.SendMessage(MediatorMessages.ShowEditResourceWizard, resource);
                                break;
                        }
                    }
                }
            }
        }

        private void DoesActivityHaveWizard(ModelItem modelItem)
        {
            var modelService = wfDesigner.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root)
            {
                var modelProperty = modelItem.Properties["ServiceName"];
                if (modelProperty != null)
                {
                    var res = modelProperty.ComputedValue;

                    var resource =
                        _workflowModel.Environment.Resources.FindSingle(c => c.ResourceName == res.ToString());

                    if (resource != null)
                    {
                        Mediator.SendMessage(MediatorMessages.HasWizard, WizardEngine.HasWizard(modelItem, resource as IContextualResourceModel));
                    }
                }
            }
        }

        private void ShowActivitySettingsWizard(ModelItem modelItem)
        {
            var modelService = wfDesigner.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root)
            {
                if (WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;
                try
                {
                    activityWizardTO = WizardEngine.GetActivitySettingsWizardInvocationTO(modelItem, _workflowModel);
                }
                catch (Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        private void ShowActivityWizard(ModelItem modelItem)
        {
            var modelService = wfDesigner.Context.Services.GetService<ModelService>();
            if (modelService.Root == modelItem.Root)
            {
                if (WizardEngine == null)
                {
                    return;
                }
                WizardInvocationTO activityWizardTO = null;


                if (!WizardEngine.HasWizard(modelItem, _workflowModel))
                {
                    PopUp.Show("Wizard not found.", "Missing Wizard");
                    return;
                }
                try
                {
                    activityWizardTO = WizardEngine.GetActivityWizardInvocationTO(modelItem, _workflowModel);
                }
                catch (Exception e)
                {
                    PopUp.Show(e.Message, "Error");
                }
                ShowWizard(activityWizardTO);
            }
        }

        private void ShowWizard(WizardInvocationTO activityWizardTO)
        {
            if (activityWizardTO != null)
            {
                ActivitySettingsView activitySettingsView = new ActivitySettingsView();
                ActivitySettingsViewModel activitySettingsViewModel = new ActivitySettingsViewModel(activityWizardTO, _workflowModel);

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

        private ModelItem RecursiveForEachCheck(dynamic activity)
        {
            ModelItem innerAct = activity.DataFunc.Handler as ModelItem;
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
        /// Prevents the delete from being executed if it is a FlowChart.
        /// </summary>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.</param>
        private void PreventDeleteFromBeingExecuted(CanExecuteRoutedEventArgs e)
        {
            if (Designer != null && Designer.Context != null)
            {
                var selection = Designer.Context.Items.GetValue<Selection>();

                if (selection == null || selection.PrimarySelection == null) return;

                if (selection.PrimarySelection.ItemType != typeof(Flowchart) &&
                    selection.SelectedObjects.All(modelItem => modelItem.ItemType != typeof(Flowchart))) return;
            }

            e.CanExecute = false;
            e.Handled = true;
        }

        /// <summary>
        /// Sets the last dropped point.
        /// </summary>
        /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        private void SetLastDroppedPoint(DragEventArgs e)
        {
            FrameworkElement senderAsFrameworkElement = _modelService.Root.View as FrameworkElement;
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
        /// Sets the last dropped model item.
        /// </summary>
        /// <param name="modelItem">The model item.</param>
        private void SetLastDroppedModelItem(ModelItem modelItem)
        {
            _lastDroppedModelItem = modelItem;
        }

        #region DataList Workflow Specific Methods

        // We will be assuming that a workflow field is a recordset based on 2 criteria:
        // 1. If the field contains a set of parenthesis
        // 2. If the field contains a period, it is a recordset with a field.

        private IList<IDataListVerifyPart> BuildWorkflowFields()
        {
            DataListVerifyPartDuplicationParser DataPartVerifyDuplicates = new DataListVerifyPartDuplicationParser();
            _uniqueWorkflowParts = new Dictionary<IDataListVerifyPart, string>(DataPartVerifyDuplicates);
            var modelService = wfDesigner.Context.Services.GetService<ModelService>();
            var flowNodes = modelService.Find(modelService.Root, typeof(FlowNode));

            foreach (var flowNode in flowNodes)
            {
                List<string> workflowFields = new List<string>();
                try
                {
                    var activity = flowNode.Properties["Action"].ComputedValue;
                    var activityType = GetActivityType(activity);
                    if (activityType != null)
                    {
                        workflowFields = GetActivityElements(activity, activityType);
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        string propertyName = string.Empty;
                        if (flowNode.ItemType.Name == "FlowDecision")
                        {
                            propertyName = "Condition";
                        }
                        else if (flowNode.ItemType.Name == "FlowSwitch`1")
                        {
                            propertyName = "Expression";
                        }
                        var activity = flowNode.Properties[propertyName].ComputedValue;
                        if (activity != null)
                        {
                            workflowFields = GetDecisionElements(activity);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                foreach (var field in workflowFields)
                {
                    BuildDataPart(field);
                }
            }
            var flattenedList = _uniqueWorkflowParts.Keys.ToList();
            return flattenedList;
        }

        private List<String> GetDecisionElements(dynamic decision)
        {
            List<string> DecisionFields = new List<string>();
            string expression = decision.ExpressionText;
            int startIndex = expression.IndexOf('"');
            startIndex = startIndex + 1;
            int endindex = expression.IndexOf('"', startIndex);
            string decisionValue = expression.Substring(startIndex, endindex - startIndex);

            // Travis.Frisinger - 25.01.2013 
            // We now need to parse this data for regions ;)

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            // NEED - DataList for active workflow
            IList<IIntellisenseResult> parts = parser.ParseDataLanguageForIntellisense(decisionValue, DataListSingleton.ActiveDataList.WriteToResourceModel(), true);

            // push them into the list
            foreach(IIntellisenseResult p in parts)
            {
                DecisionFields.Add(DataListUtil.StripBracketsFromValue(p.Option.DisplayValue));
            }


            return DecisionFields;

        }



        private void BuildDataPart(string DataPartFieldData)
        {

            IDataListVerifyPart verifyPart;
            string fullyFormattedStringValue;
            string[] fieldList = DataPartFieldData.Split('.');
            if (fieldList.Count() > 1 && !String.IsNullOrEmpty(fieldList[0]))
            {  // If it's a RecordSet Containing a field
                foreach (string item in fieldList)
                {
                    if (item.EndsWith(")") && item == fieldList[0])
                    {
                        if (item.Contains("("))
                        {
                            fullyFormattedStringValue = RemoveRecordSetBrace(item);
                            verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue, String.Empty);
                            AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                        }
                        else
                        { // If it's a field containing a single brace
                            continue;
                        }
                    }
                    else if (item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
                    { // If it's a field to a record set
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(RemoveRecordSetBrace(fieldList.ElementAt(0)), item);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (fieldList.Count() == 1 && !String.IsNullOrEmpty(fieldList[0]))
            { // If the workflow field is simply a scalar or a record set without a child
                if (DataPartFieldData.EndsWith(")") && DataPartFieldData == fieldList[0])
                {
                    if (DataPartFieldData.Contains("("))
                    {
                        fullyFormattedStringValue = RemoveRecordSetBrace(fieldList[0]);
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue, String.Empty);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                    }
                }
                else
                {
                    verifyPart = IntellisenseFactory.CreateDataListValidationScalarPart(RemoveRecordSetBrace(DataPartFieldData));
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue);
                }
            }
        }

        private List<String> GetActivityElements(object activity, Type activityType)
        {
            string stringContainerDSFActivity;
            UnlimitedObject activityDefinition;
            object InnerActivity;
            Type InnerActivityType;
            IList<string> variable = new List<String>();
            DsfActivityAbstract<string> assign = activity as DsfActivityAbstract<string>;
            DsfActivityAbstract<bool> other = activity as DsfActivityAbstract<bool>;


            if (assign != null)
            {
                activityDefinition = new UnlimitedObject(assign);
            }

            else if (other != null)
            {
                activityDefinition = new UnlimitedObject(other);
            }
            else
            {
                return new List<String>();
            }

            List<string> ActivityFields = new List<string>();
            activityType = GetActivityType(activity);

            switch (activityType.Name)
            {
                #region DsfAssignActivity
                case ("DsfAssignActivity"):
                    stringContainerDSFActivity = activityDefinition.GetValue("FieldName");
                    string fieldvalue = activityDefinition.GetValue("FieldValue");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(fieldvalue)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfAssignActivity

                #region DsfSortRecordsActivity
                case ("DsfSortRecordsActivity"):
                    stringContainerDSFActivity = activityDefinition.GetValue("RecordsetName");
                    string recsetSortField = activityDefinition.GetValue("SortField");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(recsetSortField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfSortRecordsActivity

                #region DsfDataSplitActivity
                case ("DsfDataSplitActivity"):
                    XmlDocument dataSplitXdoc = new XmlDocument();
                    dataSplitXdoc.LoadXml(activityDefinition.XmlString);
                    XmlNodeList outputList = dataSplitXdoc.SelectNodes("//OutputVariable");
                    XmlNodeList atList = dataSplitXdoc.SelectNodes("//At");

                    for (int i = 0; i < outputList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(outputList[i].InnerText))
                        {
                            stringContainerDSFActivity = outputList[i].InnerText;
                            string multifieldvalue = atList[i].InnerText;
                            if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                            {
                                stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                            }
                            foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                            foreach (string item in (FormatDsfActivityField(multifieldvalue)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                        }
                    }

                    string sourceString = GetValueFromUnlimitedObject(activityDefinition, "SourceString");
                    foreach (string item in (FormatDsfActivityField(sourceString)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }

                    break;
                #endregion DsfDataSplitActivity

                #region DsfDataMergeActivity
                case ("DsfDataMergeActivity"):
                    dataSplitXdoc = new XmlDocument();
                    dataSplitXdoc.LoadXml(activityDefinition.XmlString);
                    outputList = dataSplitXdoc.SelectNodes("//InputVariable");
                    atList = dataSplitXdoc.SelectNodes("//At");
                    string resultText = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    for (int i = 0; i < outputList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(outputList[i].InnerText))
                        {
                            stringContainerDSFActivity = outputList[i].InnerText;
                            string multifieldvalue = atList[i].InnerText;

                            foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                            foreach (string item in (FormatDsfActivityField(multifieldvalue)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                        }
                        foreach (string item in (FormatDsfActivityField(resultText)))
                        {
                            if (!item.Contains("xpath("))
                            {
                                ActivityFields.Add(item);
                            }
                        }
                    }
                    break;
                #endregion DsfDataMergeActivity

                #region DsfCountRecordsetActivity
                case ("DsfCountRecordsetActivity"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "RecordsetName");
                    string countOutput = GetValueFromUnlimitedObject(activityDefinition, "CountNumber");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(countOutput)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfCountRecordsetActivity

                #region DsfDeleteRecordActivity
                case ("DsfDeleteRecordActivity"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "RecordsetName");
                    countOutput = GetValueFromUnlimitedObject(activityDefinition, "Result");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(countOutput)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfDeleteRecordActivity

                #region DsfFindRecordsActivity
                case ("DsfFindRecordsActivity"):

                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "FieldsToSearch");

                    string match = GetValueFromUnlimitedObject(activityDefinition, "SearchCriteria");
                    string result = GetValueFromUnlimitedObject(activityDefinition, "Result");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(match)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(result)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfFindRecordsActivity

                #region DsfReplaceActivity
                case ("DsfReplaceActivity"):

                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "FieldsToSearch");
                    string find = GetValueFromUnlimitedObject(activityDefinition, "Find");
                    string replaceVal = GetValueFromUnlimitedObject(activityDefinition, "ReplaceWith");
                    result = GetValueFromUnlimitedObject(activityDefinition, "Result");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(find)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(replaceVal)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(result)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfReplaceActivity

                #region DsfIndexActivity
                case ("DsfIndexActivity"):
                    string inField = GetValueFromUnlimitedObject(activityDefinition, "InField");
                    string index = GetValueFromUnlimitedObject(activityDefinition, "Index");
                    string characters = GetValueFromUnlimitedObject(activityDefinition, "Characters");
                    string direction = GetValueFromUnlimitedObject(activityDefinition, "Direction");
                    string activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(inField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(index)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(characters)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(direction)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfIndexActivity

                #region DsfCalculateActivity
                case ("DsfCalculateActivity"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "Expression");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfCalculateActivity

                #region DsfDateTimeActivity
                case ("DsfDateTimeActivity"):
                    string dateTime = GetValueFromUnlimitedObject(activityDefinition, "DateTime");
                    string inputFormat = GetValueFromUnlimitedObject(activityDefinition, "InputFormat");
                    string outputFormat = GetValueFromUnlimitedObject(activityDefinition, "OutputFormat");
                    string timeModifier = GetValueFromUnlimitedObject(activityDefinition, "TimeModifierAmountDisplay");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(dateTime)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(inputFormat)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputFormat)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(timeModifier)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfDateTimeActivity

                #region DsfNumberFormatActivity
                case ("DsfNumberFormatActivity"):
                    string expression = GetValueFromUnlimitedObject(activityDefinition, "Expression");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(expression)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfNumberFormatActivity

                #region DsfDateTimeDifferenceActivity
                case ("DsfDateTimeDifferenceActivity"):

                    string input1 = GetValueFromUnlimitedObject(activityDefinition, "Input1");
                    string input2 = GetValueFromUnlimitedObject(activityDefinition, "Input2");
                    inputFormat = GetValueFromUnlimitedObject(activityDefinition, "InputFormat");
                    string outputType = GetValueFromUnlimitedObject(activityDefinition, "OutputType");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");


                    foreach (string item in (FormatDsfActivityField(input1)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(input2)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(inputFormat)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputType)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfDateTimeDifferenceActivity

                #region DsfFileRead

                case ("DsfFileRead"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    string userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    string password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfFileRead

                #region DsfFileWrite
                case ("DsfFileWrite"):

                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "OutputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    string fileContents = GetValueFromUnlimitedObject(activityDefinition, "FileContents");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(fileContents)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfFileWrite

                #region DsfFolderRead
                case ("DsfFolderRead"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfFolderRead

                #region DsfPathCopy
                case ("DsfPathCopy"):

                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    string outputPath = GetValueFromUnlimitedObject(activityDefinition, "OutputPath");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfPathCopy

                #region DsfPathCreate
                case ("DsfPathCreate"):

                    outputPath = GetValueFromUnlimitedObject(activityDefinition, "OutputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");


                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfPathCreate

                #region DsfPathDelete
                case ("DsfPathDelete"):

                    string inputPath = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(inputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfPathDelete

                #region DsfPathMove
                case ("DsfPathMove"):

                    outputPath = GetValueFromUnlimitedObject(activityDefinition, "OutputPath");
                    inputPath = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(inputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfPathMove

                #region DsfPathRename
                case ("DsfPathRename"):

                    outputPath = GetValueFromUnlimitedObject(activityDefinition, "OutputPath");
                    inputPath = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");

                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(inputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfPathRename

                #region DsfUnZip
                case ("DsfUnZip"):

                    outputPath = GetValueFromUnlimitedObject(activityDefinition, "OutputPath");
                    inputPath = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    string archivePassword = GetValueFromUnlimitedObject(activityDefinition, "ArchivePassword");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");


                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(inputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(archivePassword)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfUnZip

                #region DsfZip
                case ("DsfZip"):

                    outputPath = GetValueFromUnlimitedObject(activityDefinition, "OutputPath");
                    inputPath = GetValueFromUnlimitedObject(activityDefinition, "InputPath");
                    userName = GetValueFromUnlimitedObject(activityDefinition, "UserName");
                    password = GetValueFromUnlimitedObject(activityDefinition, "Password");
                    archivePassword = GetValueFromUnlimitedObject(activityDefinition, "ArchivePassword");
                    string archiveName = GetValueFromUnlimitedObject(activityDefinition, "ArchiveName");
                    string compressionRatio = GetValueFromUnlimitedObject(activityDefinition, "CompressionRatio");
                    activityResultField = GetValueFromUnlimitedObject(activityDefinition, "Result");


                    foreach (string item in (FormatDsfActivityField(activityResultField)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(userName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(password)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(inputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(outputPath)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(archivePassword)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(archiveName)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    foreach (string item in (FormatDsfActivityField(compressionRatio)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfZip

                #region DsfCaseConvertActivity
                case ("DsfCaseConvertActivity"):
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(activityDefinition.XmlString);
                    XmlNodeList nameList = xdoc.SelectNodes("//StringToConvert");

                    for (int i = 0; i < nameList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(nameList[i].InnerText))
                        {
                            stringContainerDSFActivity = nameList[i].InnerText;
                            if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                            {
                                stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                            }
                            foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                        }
                    }
                    break;
                #endregion DsfCaseConvertActivity

                #region DsfBaseConvertActivity
                case ("DsfBaseConvertActivity"):
                    xdoc = new XmlDocument();
                    xdoc.LoadXml(activityDefinition.XmlString);
                    nameList = xdoc.SelectNodes("//FromExpression");

                    for (int i = 0; i < nameList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(nameList[i].InnerText))
                        {
                            stringContainerDSFActivity = nameList[i].InnerText;
                            if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                            {
                                stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                            }
                            foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                        }
                    }
                    break;
                #endregion DsfCaseConvertActivity

                #region DsfMultiAssignActivity
                case ("DsfMultiAssignActivity"):
                    xdoc = new XmlDocument();
                    xdoc.LoadXml(activityDefinition.XmlString);
                    nameList = xdoc.SelectNodes("//FieldName");
                    XmlNodeList valueList = xdoc.SelectNodes("//FieldValue");

                    for (int i = 0; i < nameList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(nameList[i].InnerText))
                        {
                            stringContainerDSFActivity = nameList[i].InnerText;
                            string multifieldvalue = valueList[i].InnerText;
                            if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                            {
                                stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                            }
                            foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                            foreach (string item in (FormatDsfActivityField(multifieldvalue)))
                            {
                                if (!item.Contains("xpath("))
                                {
                                    ActivityFields.Add(item);
                                }
                            }
                        }
                    }
                    break;
                #endregion DsfMultiAssignActivity

                #region DsfForEachActivity
                case ("DsfForEachActivity"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "ForEachElementName");
                    if (stringContainerDSFActivity.Contains("[[") && stringContainerDSFActivity.Contains("]]"))
                    {
                        ActivityFields.AddRange(FormatDsfActivityField(stringContainerDSFActivity));
                    }

                    DsfForEachActivity forEachActivity = activity as DsfForEachActivity;
                    InnerActivity = forEachActivity.DataFunc.Handler;
                    InnerActivityType = GetActivityType(InnerActivity);
                    foreach (string item in (GetActivityElements(InnerActivity, InnerActivityType)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfForEachActivity

                #region DsfFileForEachActivity
                case ("DsfFileForEachActivity"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "ForEachElementName");
                    ActivityFields.AddRange(FormatDsfActivityField(stringContainerDSFActivity));
                    DsfFileForEachActivity fileForEachActivity = activity as DsfFileForEachActivity;
                    InnerActivity = fileForEachActivity.DataFunc.Handler;
                    InnerActivityType = GetActivityType(InnerActivity);
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (GetActivityElements(InnerActivity, InnerActivityType)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfFileForEachActivity

                #region DsfTransformActivity
                case ("TransformActivity"):
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "RootTag");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "Transformation");
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "TransformationElementName");
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    stringContainerDSFActivity = GetValueFromUnlimitedObject(activityDefinition, "TransformElementName");
                    if (!stringContainerDSFActivity.StartsWith("[[") && !stringContainerDSFActivity.EndsWith("]]"))
                    {
                        stringContainerDSFActivity = "[[" + stringContainerDSFActivity + "]]";
                    }
                    foreach (string item in (FormatDsfActivityField(stringContainerDSFActivity)))
                    {
                        if (!item.Contains("xpath("))
                        {
                            ActivityFields.Add(item);
                        }
                    }
                    break;
                #endregion DsfTransformActivity

                #region DsfActivity
                case ("DsfActivity"):

                    if (!activityDefinition.RootName.Equals("dsfwebpageactivity", StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            XElement root = activityDefinition.xmlData;
                            XElement serviceNameNode = root.Descendants().FirstOrDefault(c => c.Name.ToString().Equals("ServiceName", StringComparison.InvariantCultureIgnoreCase));
                            string serviceNameElement = serviceNameNode.Value;
                            if (serviceNameElement.StartsWith("[[") && serviceNameElement.EndsWith("]]"))
                            {
                                serviceNameElement = serviceNameElement.Replace("[[", "");
                                serviceNameElement = serviceNameElement.Replace("]]", "");
                                ActivityFields.Add(serviceNameElement);
                            }
                        }
                        catch (Exception)
                        {
                            //TODO:Justification needed for empty catch
                        }
                        try
                        {
                            XElement root = activityDefinition.xmlData;
                            XElement isWorkflow = root.Descendants().FirstOrDefault(c => c.Name.ToString().Equals("IsWorkflow", StringComparison.InvariantCultureIgnoreCase));
                            XElement inputMappingNode = root.Descendants().FirstOrDefault(c => c.Name.ToString().Equals("inputmapping", StringComparison.InvariantCultureIgnoreCase));
                            XElement inputMapping = XElement.Parse(inputMappingNode.Value);
                            string inputElement = "Input";
                            IEnumerable<XElement> inputs = inputMapping.DescendantsAndSelf().Where(c => c.Name.ToString().Equals(inputElement, StringComparison.InvariantCultureIgnoreCase));
                            List<string> namesOfInputs = new List<string>();
                            foreach (XElement element in inputs)
                            {
                                //19.09.2012: massimo.guerrera - Change made due to a change in expected behaviour, the names in the input mapping shouldnt be added when add missing is clicked 
                                //if (isWorkflow.Value == "true")
                                //{
                                //    variable = FormatDsfActivityField("[[" + element.Attribute("Name").Value + "]]");
                                //    if (variable.Count > 0)
                                //    {
                                //        namesOfInputs.AddRange(variable);
                                //    }
                                //}

                                variable = FormatDsfActivityField(element.Attribute("Source").Value);
                                if (variable.Count > 0)
                                {
                                    namesOfInputs.AddRange(variable);
                                }
                            }
                            ActivityFields.AddRange(namesOfInputs);
                        }
                        catch (Exception)
                        {
                            //TODO:Justification needed for empty catch
                        }

                        try
                        {
                            XElement root = activityDefinition.xmlData;
                            XElement outputMappingNode = root.Descendants().FirstOrDefault(c => c.Name.ToString().Equals("outputmapping", StringComparison.InvariantCultureIgnoreCase));
                            XElement outputMapping = XElement.Parse(outputMappingNode.Value);
                            string outputElement = "Output";
                            IEnumerable<XElement> outputs = outputMapping.DescendantsAndSelf().Where(c => c.Name.ToString().Equals(outputElement, StringComparison.InvariantCultureIgnoreCase));
                            List<string> namesOfOutputs = new List<string>();
                            foreach (XElement element in outputs)
                            {
                                variable = FormatDsfActivityField(element.Attribute("Value").Value);
                                if (variable.Count > 0)
                                {
                                    namesOfOutputs.AddRange(variable);
                                }
                            }
                            ActivityFields.AddRange(namesOfOutputs);
                        }
                        catch (Exception)
                        {
                            //TODO:Justification needed for empty catch
                        }
                    }
                    else if (activityDefinition.RootName.Equals("dsfwebpageactivity", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Sashen: Must be a better way - using the exact element name does make it awful.
                        string sanitizedWebpageObject = activityDefinition.GetElement("XMLConfiguration").XmlString.Replace("&gt;", ">").Replace("&lt;", "<");
                        XElement element = XElement.Parse(sanitizedWebpageObject);
                        string webpageData = DataListFactory.GenerateMappingFromWebpage(sanitizedWebpageObject,"", enDev2ArgumentType.Input);
                        XElement webpageElements = XElement.Parse(webpageData);
                        webpageElements.Elements().ToList().ForEach(c => ActivityFields.Add(c.Attribute("Name").Value));
                    }

                    break;
                default:
                    break;
                #endregion DsfActivity
            }
            return ActivityFields;
        }

        private Type GetActivityType(object ActivityItem)
        {
            return ActivityItem.GetType();
        }

        private List<IDataListVerifyPart> MissingDataListParts(IList<IDataListVerifyPart> partsToVerify)
        {
            List<IDataListVerifyPart> MissingDataParts = new List<IDataListVerifyPart>();
            foreach (var part in partsToVerify)
            {
                if (!(part.IsScalar))
                {
                    if (DataListSingleton.ActiveDataList.DataList.Count(c => c.Name == part.Recordset && c.IsRecordset) == 0)
                    {
                        MissingDataParts.Add(part);
                    }
                    else
                    {
                        if (DataListSingleton.ActiveDataList.DataList.First(c => c.Name == part.Recordset && c.IsRecordset).Children.Count(c => c.Name == part.Field) == 0)
                        {
                            MissingDataParts.Add(part);
                        }
                    }
                }
                else if (DataListSingleton.ActiveDataList.DataList.Count(c => c.Name == part.Field && !c.IsRecordset) == 0)
                {
                    MissingDataParts.Add(part);
                }
            }
            return MissingDataParts;
        }


        private bool Equals(IDataListVerifyPart part, IDataListVerifyPart part2)
        {
            if (part.DisplayValue == part2.DisplayValue)
            {
                return true;
            }
            return false;

        }

        private List<IDataListVerifyPart> MissingWorkflowItems(IList<IDataListVerifyPart> PartsToVerify)
        {
            List<IDataListVerifyPart> MissingWorkflowParts = new List<IDataListVerifyPart>();
            foreach (IDataListItemModel dataListItem in DataListSingleton.ActiveDataList.DataList)
            {
                if (String.IsNullOrEmpty(dataListItem.Name))
                {
                    continue;
                }
                if ((dataListItem.Children.Count > 0))
                {

                    if (PartsToVerify.Count(part => part.Recordset == dataListItem.Name) == 0)
                    {
                        //19.09.2012: massimo.guerrera - Added in the description to creating the part
                        if (dataListItem.IsEditable)
                        {
                            MissingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name, String.Empty, dataListItem.Description));
                            foreach (var child in dataListItem.Children)
                                if (!(String.IsNullOrEmpty(child.Name)))
                                    //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                    if (dataListItem.IsEditable)
                                    {
                                        MissingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name, child.Name, child.Description));
                                    }
                        }
                    }
                    else foreach (IDataListItemModel child in dataListItem.Children)
                            if (PartsToVerify.Count(part => part.Field == child.Name && part.Recordset == child.Parent.Name) == 0)
                            {
                                //19.09.2012: massimo.guerrera - Added in the description to creating the part
                                if (child.IsEditable)
                                {
                                    MissingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart(dataListItem.Name, child.Name, child.Description));
                                }
                            }
                }
                else if (PartsToVerify.Count(part => part.Field == dataListItem.Name) == 0)
                {
                    {
                        if (dataListItem.IsEditable)
                        {
                            //19.09.2012: massimo.guerrera - Added in the description to creating the part
                            MissingWorkflowParts.Add(IntellisenseFactory.CreateDataListValidationScalarPart(dataListItem.Name, dataListItem.Description));
                        }
                    }
                }
            }
            return MissingWorkflowParts;
        }


        private IList<String> FormatDsfActivityField(string activityField)
        {
            // Sashen: 09-10-2012 : Using the new parser
            SyntaxTreeBuilder intellisenseParser = new SyntaxTreeBuilder();

            IList<string> result = new List<string>();
            Node[] nodes = intellisenseParser.Build(activityField);
            if (intellisenseParser.EventLog.HasEventLogs)
            {
                string test = intellisenseParser.EventLog.GetEventLogs().First().ErrorStart.Contents;
                //2013.01.23: Ashley Lewis - Removed this condition for Bug 6413
                //if (intellisenseParser.EventLog.GetEventLogs().First().ErrorStart.Contents == "{{")
                //{
                // Sashen: 09-10-2012: If the new parser is unable to parse a region, use the old one.
                //                     this is only to cater for the parser being unable to parse "{{" and literals - TEMPORARY
                // TO DO: Remove this when new parser caters for all cases.

                IDev2StudioDataLanguageParser languageParser = DataListFactory.CreateStudioLanguageParser();

                try
                {
                    result = languageParser.ParseForActivityDataItems(activityField);
                }
                catch (Dev2DataLanguageParseError)
                {
                    return new List<String>();
                }
                catch (NullReferenceException)
                {
                    return new List<String>();
                }
                //}
            }
            List<Node> allNodes = new List<Node>();


            if (nodes.Count() > 0 && !(intellisenseParser.EventLog.HasEventLogs))
            {

                nodes[0].CollectNodes(allNodes);

                for (int i = 0; i < allNodes.Count; i++)
                {
                    if (allNodes[i] is DatalistRecordSetNode)
                    {
                        DatalistRecordSetNode refNode = allNodes[i] as DatalistRecordSetNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                    else if (allNodes[i] is DatalistReferenceNode)
                    {
                        DatalistReferenceNode refNode = allNodes[i] as DatalistReferenceNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                    else if (allNodes[i] is DatalistRecordSetFieldNode)
                    {
                        DatalistRecordSetFieldNode refNode = allNodes[i] as DatalistRecordSetFieldNode;
                        string nodeName = refNode.GetRepresentationForEvaluation();
                        nodeName = nodeName.Substring(2, nodeName.Length - 4);
                        result.Add(nodeName);
                    }
                }

            }
            return result;
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

        private string RemoveRecordSetBrace(string RecordSet)
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

        private string RemoveSquareBraces(string field)
        {
            return (field.Contains("[[") && field.Contains("]]")) ? field.Replace("[[", "").Replace("]]", "") : field;
        }

        private void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart)
        {
            try
            {
                _uniqueWorkflowParts.Add(part, nameOfPart);
            }
            catch (ArgumentException)
            {
                //TODO: Justify why there is an empty catch
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

        public void InitializeDesigner(IDictionary<Type, Type> designerAttributes)
        {
            //2012.10.01: massimo.guerrera - Add Remove buttons made into one:)
            MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.AddRemoveDataListItems, Mediator.RegisterToReceiveMessage(MediatorMessages.AddRemoveDataListItems, input => AddRemoveDataListItems(input as IDataListViewModel)));

            MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.FindMissingDataListItems, Mediator.RegisterToReceiveMessage(MediatorMessages.FindMissingDataListItems, input =>
            {
                AddMissingOnlyWithNoPopUp(input as IDataListViewModel);
            }));
            //07-12-2012 - Massimo.Guerrera - Added for PBI 6665
            MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.ShowActivityWizard, Mediator.RegisterToReceiveMessage(MediatorMessages.ShowActivityWizard, input => ShowActivityWizard(input as ModelItem)));
            // MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.GetMappingViewModel, Mediator.RegisterToReceiveMessage(MediatorMessages.GetMappingViewModel, input => GetMappingViewModel(input as ModelItem)));

            MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.ShowActivitySettingsWizard, Mediator.RegisterToReceiveMessage(MediatorMessages.ShowActivitySettingsWizard, input
                                                                                                                                                                         =>
            {
                ShowActivitySettingsWizard(input as ModelItem);
            }));
            MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.EditActivity, Mediator.RegisterToReceiveMessage(MediatorMessages.EditActivity, input => EditActivity(input as ModelItem)));
            MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.DoesActivityHaveWizard, Mediator.RegisterToReceiveMessage(MediatorMessages.DoesActivityHaveWizard, input => DoesActivityHaveWizard(input as ModelItem)));
            //MediatorRepo.addKey(this.GetHashCode(), MediatorMessages.FindUnusedDataListitems, Mediator.RegisterToReceiveMessage(MediatorMessages.FindUnusedDataListitems, input => FindUnusedDataListItems(input as IDataListViewModel)));
            _wd = new WorkflowDesigner();

            _wdMeta = new DesignerMetadata();
            _wdMeta.Register();
            var builder = new AttributeTableBuilder();
            foreach (var designerAttribute in designerAttributes)
            {
                builder.AddCustomAttributes(designerAttribute.Key, new DesignerAttribute(designerAttribute.Value));
            }

            MetadataStore.AddAttributeTable(builder.CreateTable());
            if (string.IsNullOrEmpty(_workflowModel.WorkflowXaml))
            {
                _wd.Load(GetBaseUnlimitedFlowchartActivity());
                BindToModel();
            }
            else
            {
                _wd.Text = _workflowModel.WorkflowXaml;
                _wd.Load();
            }

            _wdMeta.Register();

            _modelService = _wd.Context.Services.GetService<ModelService>();
            _modelService.ModelChanged += new EventHandler<ModelChangedEventArgs>(ModelServiceModelChanged);

            _viewstateService = _wd.Context.Services.GetService<ViewStateService>();
            
            _wd.View.PreviewDrop += new DragEventHandler(ViewPreviewDrop);
            _wd.View.PreviewMouseDown += ViewPreviewMouseDown;
            _wd.View.LayoutUpdated += ViewOnLayoutUpdated;

            _wd.Context.Services.GetService<DesignerView>().WorkflowShellBarItemVisibility = ShellBarItemVisibility.Zoom;
            _wd.Context.Items.Subscribe<Selection>(OnItemSelected);

            _wd.Context.Services.Publish(_designerManagementService);

            //Jurie.Smit 2013/01/03 - Added to disable the deleting of the root flowchart
            CommandManager.AddPreviewCanExecuteHandler(_wd.View, CanExecuteRoutedEventHandler);
        }

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
                        //modelProperty = selectedItem.Properties["ServiceName"];

                        //if (modelProperty != null && selectedItem != null && selectedItem.ItemType == typeof(DsfWebPageActivity))
                        //{
                        //    string serviceName = modelProperty.ComputedValue.ToString();
                        //    var resourceModel = _workflowModel.Environment.Resources.All().FirstOrDefault(resource => resource.ResourceName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase));
                        //    bool sendMediatorMessage = resourceModel != null;

                        //    if (selectedItem.ItemType == typeof(Flowchart))
                        //    {
                        //        sendMediatorMessage = false;
                        //    }

                        //    if (sendMediatorMessage)
                        //    {
                        //        IWebActivity webActivity = WebActivityFactory.CreateWebActivity(selectedItem, resourceModel as IContextualResourceModel, displayName);
                        //        Mediator.SendMessage(MediatorMessages.WorkflowActivitySelected, webActivity);
                        //        isItemSelected = true;
                        //    }
                        //    else Mediator.SendMessage(MediatorMessages.RemoveDataMapping, this);
                        //}
                        //if (selectedItem.ItemType == typeof(DsfWebPageActivity))
                        //{
                        var resourceModel = _workflowModel.Environment.Resources.All().FirstOrDefault(resource => resource.ResourceName.Equals(displayName, StringComparison.InvariantCultureIgnoreCase));
                        bool sendMediatorMessage = resourceModel != null;

                        if (selectedItem.ItemType == typeof(DsfWebPageActivity))
                        {
                            sendMediatorMessage = true;
                        }
                        // (selectedItem.ItemType == typeof(Flowchart))
                        else
                        {
                            sendMediatorMessage = false;
                        }

                        if (sendMediatorMessage)
                        {
                            IWebActivity webActivity = WebActivityFactory.CreateWebActivity(selectedItem, resourceModel as IContextualResourceModel, displayName);
                            Mediator.SendMessage(MediatorMessages.WorkflowActivitySelected, webActivity);
                            isItemSelected = true;
                        }
                        else
                        {
                            Mediator.SendMessage(MediatorMessages.RemoveDataMapping, this);
                        }
                        //}
                    }
                }

            }
            return isItemSelected;
        }

        public void BindToModel()
        {
            _wd.Flush();
            _workflowModel.WorkflowXaml = _wd.Text;
            if (string.IsNullOrEmpty(_workflowModel.ServiceDefinition))
            {
                _workflowModel.ServiceDefinition = _workflowModel.ToServiceDefinition();
            }
        }

        public void InvalidateUI()
        {
            base.OnPropertyChanged("DesignerView");
        }

        // Travis.Frisinger : 23.01.2013 - Added A Dev2DataListDecisionHandler Variable
        public ActivityBuilder GetBaseUnlimitedFlowchartActivity()
        {
            ActivityBuilder emptyWorkflow = new ActivityBuilder()
            {
                Name = _workflowModel.ResourceName,
                Properties = {
                    new DynamicActivityProperty{Name = "AmbientDataList",Type = typeof(InOutArgument<List<string>>)}
                    ,new DynamicActivityProperty{ Name = "ParentWorkflowInstanceId", Type = typeof(InOutArgument<Guid>)}
                    ,new DynamicActivityProperty{ Name = "ParentServiceName", Type = typeof(InOutArgument<string>)}
                },
                Implementation = new Flowchart
                {
                    DisplayName = _workflowModel.ResourceName,
                    Variables = {
                         new Variable<List<string>>{Name = "InstructionList"},
                         new Variable<string>{Name = "LastResult"},
                         new Variable<bool>{Name = "HasError"},
                         new Variable<string>{Name = "ExplicitDataList"},
                         new Variable<bool>{Name = "IsValid"},
                         new Variable<UnlimitedObject>{Name = "d"},
                         new Variable<Unlimited.Applications.BusinessDesignStudio.Activities.Util>{ Name = "t"},
                         new Variable<Dev2DataListDecisionHandler>{Name = "Dev2DecisionHandler"}
                    }
                }
            };

            return emptyWorkflow;
        }

        //2012.10.01: massimo.guerrera - Add Remove buttons made into one:)
        public void AddRemoveDataListItems(IDataListViewModel dataListViewModel)
        {
            if (ResourceModel == dataListViewModel.Resource)
            {
                IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();
                IList<IDataListVerifyPart> _removeParts = MissingWorkflowItems(workflowFields);

                _filteredDataListParts = MissingDataListParts(workflowFields);

                if (_removeParts.Count == 0 && _filteredDataListParts.Count == 0)
                {
                    PopUp.Header = "Message";
                    PopUp.Description = "No missing or unused DataList items found!";
                    PopUp.ImageType = MessageBoxImage.Information;
                    PopUp.Buttons = MessageBoxButton.OK;
                    var respones = PopUp.Show();
                    return;
                }
                else
                {
                    PopUp.Header = "Message";
                    PopUp.Description = string.Format("{0} item(s) are about to be removed and {1} item(s) added to the DataList. Would you like to proceed?", _removeParts.Count, _filteredDataListParts.Count);
                    PopUp.ImageType = MessageBoxImage.Information;
                    PopUp.Buttons = MessageBoxButton.YesNo;
                    MessageBoxResult response = PopUp.Show();
                    if (response == MessageBoxResult.Yes)
                    {
                        dataListViewModel.AddMissingDataListItems(_filteredDataListParts);
                        dataListViewModel.RemoveUnusedDataListItems(_removeParts);
                    }
                }
            }

        }

        public void AddMissingOnlyWithNoPopUp(IDataListViewModel dataListViewModel)
        {
            if (dataListViewModel != null && ResourceModel == dataListViewModel.Resource)
            {
                IList<IDataListVerifyPart> workflowFields = BuildWorkflowFields();

                _filteredDataListParts = MissingDataListParts(workflowFields);
                dataListViewModel.AddMissingDataListItems(_filteredDataListParts);
            }
        }

        #endregion

        #region Event Handlers

        private void ViewOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            UpdateLocationOfLastDroppedModelItem();
        }

        private void ViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dcontxt = (e.Source as dynamic).DataContext;
            var vm = dcontxt as WorkflowDesignerViewModel;

            if (!SecurityContext.IsUserInRole(new string[] { StringResources.BDSAdminRole, StringResources.BDSDeveloperRole, StringResources.BDSTestingRole }))
            {
                e.Handled = true;
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                DesignerView designerView = e.Source as DesignerView;
                if (designerView != null && designerView.FocusedViewElement == null)
                {
                    e.Handled = true;
                    return;
                }

                ModelItem item = SelectedModelItem as ModelItem;

                // Travis.Frisinger - 28.01.2013 : Case Amendments
                if (item != null)
                {
                    DependencyObject dp = e.OriginalSource as DependencyObject;
                    string itemFn = item.ItemType.FullName;

                    // Handle Case Edits
                    if (item != null && itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchCaseLink", StringComparison.Ordinal)
                        && !itemFn.StartsWith("System.Activities.Core.Presentation.FlowSwitchDefaultLink", StringComparison.Ordinal))
                    {

                        ModelProperty tmp = item.Properties["Case"];

                        if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp))
                        {
                            Mediator.SendMessage(MediatorMessages.EditCaseExpression, new Tuple<ModelProperty, IEnvironmentModel>(tmp, _workflowModel.Environment));
                        }
                    }

                    // Handle Switch Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) && item.ItemType == typeof(FlowSwitch<string>))
                    {
                        Mediator.SendMessage(MediatorMessages.ConfigureSwitchExpression, new Tuple<ModelItem, IEnvironmentModel>(item, _workflowModel.Environment));
                    }

                    // Handle Decision Edits
                    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp) && item.ItemType == typeof(FlowDecision))
                    {
                        Mediator.SendMessage(MediatorMessages.ConfigureDecisionExpression, new Tuple<ModelItem, IEnvironmentModel>(item, _workflowModel.Environment));
                    }
                }


                if (designerView != null && designerView.FocusedViewElement != null && designerView.FocusedViewElement.ModelItem != null)
                {
                    ModelItem modelItem = designerView.FocusedViewElement.ModelItem;

                    if (modelItem.ItemType == typeof(DsfWebPageActivity))
                    {
                        IWebActivity webpageActivity = WebActivityFactory.CreateWebActivity(modelItem, _workflowModel, modelItem.Properties["DisplayName"].ComputedValue.ToString());
                        Mediator.SendMessage(MediatorMessages.AddWebpageDesigner, webpageActivity);
                        e.Handled = true;
                    }
                    else if (modelItem.ItemType == typeof(DsfWebSiteActivity))
                    {
                        IWebActivity webpageActivity = WebActivityFactory.CreateWebActivity(modelItem, _workflowModel, modelItem.Properties["DisplayName"].ComputedValue.ToString());
                        Mediator.SendMessage(MediatorMessages.AddWebsiteDesigner, webpageActivity);
                        e.Handled = true;
                    }
                    //else if (modelItem.ItemType == typeof(DsfActivity))
                    //{
                    //    var test = modelItem.Properties["ServiceName"].ComputedValue;

                    //    var resource = _workflowModel.Environment.Resources.FindSingle(c => c.ResourceName == test.ToString());

                    //    if (resource != null)
                    //    {
                    //        switch (resource.ResourceType)
                    //        {
                    //            case enResourceType.WorkflowService:
                    //                Mediator.SendMessage(MediatorMessages.AddWorkflowDesigner, resource);
                    //                break;

                    //            case enResourceType.Service:
                    //                Mediator.SendMessage(MediatorMessages.AddResourceDocument, resource);
                    //                break;
                    //        }
                    //    }
                    //}
                    //else if (e.Source is DesignerView && modelItem.ItemType.InheritsOrImplements(typeof(DsfActivityAbstract<>)))
                    //{
                    //    DependencyObject dp = e.OriginalSource as DependencyObject;

                    //    if (dp != null && !WizardEngineAttachedProperties.GetDontOpenWizard(dp))
                    //    {
                    //        ShowActivityWizard(modelItem);
                    //    }

                    //    e.Handled = true;
                    //}
                }
            }
        }

        private void ViewPreviewDrop(object sender, DragEventArgs e)
        {
            SetLastDroppedPoint(e);
            _dataObject = e.Data.GetData(typeof(ResourceTreeViewModel));
        }

        private void ModelServiceModelChanged(object sender, ModelChangedEventArgs e)
        {
            if (e.ItemsAdded != null)
            {
                foreach (ModelItem mi in e.ItemsAdded)
                {

                    if ((mi.Parent.Parent.Parent != null) && mi.Parent.Parent.Parent.ItemType == typeof(FlowSwitch<string>))
                    {
                        if ((mi.Properties["Key"].Value != null) && mi.Properties["Key"].Value.ToString().Contains("Case"))
                        {
                            Tuple<ModelItem, IEnvironmentModel> wrapper = new Tuple<ModelItem, IEnvironmentModel>(mi, _workflowModel.Environment);
                            Mediator.SendMessage(MediatorMessages.ConfigureCaseExpression, wrapper);
                            //Mediator.SendMessage(MediatorMessages.ConfigureCaseExpression, mi);
                        }
                    }

                    if (mi.ItemType == typeof(FlowSwitch<string>))
                    {
                        //This line is necessary to fix the issue were decisions and switches didn't have the correct positioning when dragged on
                        SetLastDroppedModelItem(mi);

                        // Travis.Frisinger : 28.01.2013 - Switch Amendments
                        Tuple<ModelItem, IEnvironmentModel> wrapper = new Tuple<ModelItem, IEnvironmentModel>(mi, _workflowModel.Environment);
                        Mediator.SendMessage(MediatorMessages.ConfigureSwitchExpression, wrapper);
                    }

                    if (mi.ItemType == typeof(FlowDecision))
                    {
                        //This line is necessary to fix the issue were decisions and switches didn't have the correct positioning when dragged on
                        SetLastDroppedModelItem(mi);
                        
                        Tuple<ModelItem, IEnvironmentModel> wrapper = new Tuple<ModelItem, IEnvironmentModel>(mi, _workflowModel.Environment);
                        Mediator.SendMessage(MediatorMessages.ConfigureDecisionExpression, wrapper);

                        //Mediator.SendMessage(MediatorMessages.ConfigureDecisionExpression, mi);
                    }

                    if (mi.ItemType == typeof(FlowStep))
                    {

                        if (mi.Properties["Action"].ComputedValue is DsfWebPageActivity)
                        {
                            var modelService = wfDesigner.Context.Services.GetService<ModelService>();
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
                                IContextualResourceModel resource = _workflowModel.Environment.Resources.FindSingle(
                                     c => c.ResourceName == droppedActivity.ServiceName) as IContextualResourceModel;
                                droppedActivity = DsfActivityFactory.CreateDsfActivity(resource, droppedActivity, false);
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

                                            d.ServiceName = d.DisplayName = d.ToolboxFriendlyName = resource.ResourceName;
                                            d.IconPath = resource.IconPath;
                                            mi.Properties["Action"].SetValue(d);
                                        }
                                    }

                                    _dataObject = null;
                                }
                            }
                        }
                    }
                }
            }
            else if (e.PropertiesChanged != null)
            {
                if (e.PropertiesChanged.Any(mp => mp.Name == "Handler") && _dataObject != null)
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
            }
        }

        /// <summary>
        /// Handler attached to intercept checks for executing the delete command
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CanExecuteRoutedEventArgs" /> instance containing the event data.</param>
        private void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete) //triggered from deleting an activity
            {
                PreventDeleteFromBeingExecuted(e);
            }
            else if (e.Command == System.Windows.Documents.EditingCommands.Delete) //triggered from editing displayname, expressions, etc
            {
                PreventDeleteFromBeingExecuted(e);
            }
        }

        #endregion

        #region Dispose

        public new void Dispose()
        {
            MediatorRepo.deregisterAllItemMessages(this.GetHashCode());
            _wd = null;
            base.Dispose();
        }

        #endregion Dispose
    }
}
