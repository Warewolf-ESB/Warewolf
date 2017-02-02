
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InconsistentNaming
// ReSharper disable ReturnTypeCanBeEnumerable.Local

namespace Warewolf.Studio.ViewModels
{

    public class ServiceTestViewModel : BindableBase, IServiceTestViewModel
    {
        private readonly IExternalProcessExecutor _processExecutor;
        private IServiceTestModel _selectedServiceTest;
        private string _runAllTestsUrl;
        private string _testPassingResult;
        private ObservableCollection<IServiceTestModel> _tests;
        private string _displayName;
        public IPopupController PopupController { get; }
        private string _errorMessage;
        private readonly IShellViewModel _shellViewModel;
        private IContextualResourceModel _resourceModel;
        private string _serverName;
        private IWorkflowDesignerViewModel _workflowDesignerViewModel;

        private static readonly IEnumerable<Type> Types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());


        public ServiceTestViewModel(IContextualResourceModel resourceModel, IAsyncWorker asyncWorker, IEventAggregator eventPublisher, IExternalProcessExecutor processExecutor, IWorkflowDesignerViewModel workflowDesignerViewModel, IMessage msg = null)
        {

            if (resourceModel == null)
                throw new ArgumentNullException(nameof(resourceModel));
            _processExecutor = processExecutor;
            AsyncWorker = asyncWorker;
            EventPublisher = eventPublisher;
            ResourceModel = resourceModel;
            ResourceModel.Environment.IsConnectedChanged += (sender, args) =>
            {
                ViewModelUtils.RaiseCanExecuteChanged(DeleteTestCommand);
                RefreshCommands();
            };

            ResourceModel.Environment.Connection.ReceivedResourceAffectedMessage += OnReceivedResourceAffectedMessage;
            SetServerName(resourceModel);
            DisplayName = resourceModel.DisplayName + " - Tests" + _serverName;

            ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();
            PopupController = CustomContainer.Get<IPopupController>();
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
            RunAllTestsInBrowserCommand = new DelegateCommand(RunAllTestsInBrowser, IsServerConnected);
            RunAllTestsCommand = new DelegateCommand(RunAllTests, IsServerConnected);
            RunSelectedTestInBrowserCommand = new DelegateCommand(RunSelectedTestInBrowser, () => CanRunSelectedTestInBrowser);
            RunSelectedTestCommand = new DelegateCommand(RunSelectedTest, () => CanRunSelectedTest);
            StopTestCommand = new DelegateCommand(StopTest, () => CanStopTest);
            CreateTestCommand = new DelegateCommand(CreateTests);
            DeleteTestCommand = new DelegateCommand<IServiceTestModel>(DeleteTest, CanDeleteTest);
            DeleteTestStepCommand = new DelegateCommand<IServiceTestStep>(DeleteTestStep);
            DuplicateTestCommand = new DelegateCommand(DuplicateTest, () => CanDuplicateTest);
            RunAllTestsUrl = WebServer.GetWorkflowUri(resourceModel, "", UrlType.Tests)?.ToString();

            UpdateHelpDescriptor(Resources.Languages.HelpText.ServiceTestGenericHelpText);

            WorkflowDesignerViewModel = workflowDesignerViewModel;
            WorkflowDesignerViewModel.IsTestView = true;
            WorkflowDesignerViewModel.ItemSelectedAction = ItemSelectedAction;
            IsLoading = true;
            AsyncWorker.Start(GetTests, models =>
            {
                var dummyTest = new DummyServiceTest(CreateTests) { TestName = "Create a new test." };
                models.Add(dummyTest);
                SelectedServiceTest = dummyTest;
                Tests = models;
                if (msg != null)
                {
                    var test = msg as NewTestFromDebugMessage;
                    if (test != null)
                    {
                        NewTestFromDebugMessage newTest = test;
                        if (newTest.RootItems == null)
                            throw new ArgumentNullException(nameof(newTest.RootItems));
                        PrepopulateTestsUsingDebug(newTest.RootItems);
                    }
                    else
                    {
                        throw new ArgumentException("expected " + typeof(NewTestFromDebugMessage).Name + " but got " + msg.GetType().Name);
                    }
                }
                IsLoading = false;
            }, OnError);
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged(() => IsLoading);
            }
        }

        public void PrepopulateTestsUsingDebug(List<IDebugTreeViewItemViewModel> models)
        {
            CreateTests();
            if (_canAddFromDebug)
                AddFromDebug(models);
        }

        private void AddFromDebug(List<IDebugTreeViewItemViewModel> models)
        {
            foreach (IDebugTreeViewItemViewModel debugState in models)
            {
                var debugItem = debugState as DebugStateTreeViewItemViewModel;
                if (debugItem != null && debugItem.Parent == null)
                {
                    var debugItemContent = debugItem.Content;
                    if (debugItemContent != null)
                    {
                        if (debugItemContent.ActivityType == ActivityType.Workflow && debugItemContent.OriginatingResourceID == ResourceModel.ID)
                        {
                            ProcessInputsAndOutputs(debugItemContent);
                        }
                        else if (debugItemContent.ActivityType == ActivityType.Workflow && debugItemContent.ActualType == typeof(DsfActivity).Name)
                        {
                            AddStepFromDebug(debugState, debugItemContent);
                        }
                        else if (debugItemContent.ActivityType != ActivityType.Workflow && debugItemContent.ActualType != typeof(DsfCommentActivity).Name)
                        {
                            ProcessRegularDebugItem(debugItemContent, debugState);
                        }
                    }
                }
            }
        }

        private void ProcessRegularDebugItem(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState)
        {
            var actualType = debugItemContent.ActualType;
            if (actualType == typeof(DsfDecision).Name || actualType == typeof(TestMockDecisionStep).Name)
            {
                DecisionFromDebug(debugState, debugItemContent);
            }
            else if (actualType == typeof(DsfSwitch).Name || actualType == typeof(TestMockSwitchStep).Name)
            {
                SwitchFromDebug(debugState, debugItemContent);
            }
            else if (actualType == typeof(DsfEnhancedDotNetDllActivity).Name)
            {
                EnhancedDotNetDllFromDebug(debugState, debugItemContent);
            }
            else
            {
                AddStepFromDebug(debugState, debugItemContent);
            }
        }

        private void EnhancedDotNetDllFromDebug(IDebugTreeViewItemViewModel debugState, IDebugState debugItemContent)
        {
            var exists = FindExistingStep(debugItemContent.ID.ToString());
            ServiceTestStep serviceTestStep = null;
            if (exists == null)
            {
                serviceTestStep = SelectedServiceTest.AddTestStep(debugItemContent.ID.ToString(), debugItemContent.DisplayName, debugItemContent.ActualType, new ObservableCollection<IServiceTestOutput>()) as ServiceTestStep;
                // ReSharper disable once PossibleNullReferenceException
                if (serviceTestStep != null)
                {
                    SetStepIcon(serviceTestStep.ActivityType, serviceTestStep);
                }
            }

            if (debugState.Children != null && debugState.Children.Count > 0)
            {
                AddChildren(debugState, serviceTestStep);
            }
        }

        private void SwitchFromDebug(IDebugTreeViewItemViewModel itemContent, IDebugState debugItemContent)
        {
            var processFlowSwitch = ProcessFlowSwitch(WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ParentID));
            if (processFlowSwitch != null)
            {
                if (debugItemContent.Outputs != null && debugItemContent.Outputs.Count > 0)
                {
                    if (debugItemContent.Outputs[0].ResultsList.Count > 0)
                    {
                        processFlowSwitch.StepOutputs[0].Value = debugItemContent.Outputs[0].ResultsList[0].Value;
                    }
                }

                var debugStateActivityTypeName = itemContent.ActivityTypeName;
                if (debugStateActivityTypeName == typeof(TestMockSwitchStep).Name)
                {
                    processFlowSwitch.MockSelected = true;
                    processFlowSwitch.AssertSelected = false;
                    processFlowSwitch.StepOutputs[0].Value = debugItemContent.AssertResultList[0].ResultsList[0].Value;
                }
            }
        }

        private void DecisionFromDebug(IDebugTreeViewItemViewModel itemContent, IDebugState debugItemContent)
        {
            var processFlowDecision = ProcessFlowDecision(WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ParentID));
            if (processFlowDecision != null)
            {
                if (debugItemContent.Outputs != null && debugItemContent.Outputs.Count > 0)
                {
                    if (debugItemContent.Outputs[0].ResultsList.Count > 0)
                    {
                        processFlowDecision.StepOutputs[0].Value = debugItemContent.Outputs[0].ResultsList[0].Value;
                    }
                }
                var debugStateActivityTypeName = itemContent.ActivityTypeName;
                if (debugStateActivityTypeName == typeof(TestMockDecisionStep).Name)
                {
                    processFlowDecision.MockSelected = true;
                    processFlowDecision.AssertSelected = false;
                    processFlowDecision.StepOutputs[0].Value = debugItemContent.AssertResultList[0].ResultsList[0].Value;
                }
            }
        }

        private void ProcessInputsAndOutputs(IDebugState debugItemContent)
        {
            if (debugItemContent.StateType == StateType.Start)
            {
                SetInputs(debugItemContent);
            }
            else if (debugItemContent.StateType == StateType.End)
            {
                SetOutputs(debugItemContent);
            }
        }

        private void AddStepFromDebug(IDebugTreeViewItemViewModel debugState, IDebugState debugItemContent)
        {
            if (debugState.Children != null && debugState.Children.Count > 0)
            {
                AddChildDebugItems(debugItemContent, debugState, null);
            }
            else
            {
                var outputs = debugItemContent.Outputs;
                var exists = FindExistingStep(debugItemContent.ID.ToString());
                if (exists == null)
                {
                    var serviceTestStep = SelectedServiceTest.AddTestStep(debugItemContent.ID.ToString(), debugItemContent.DisplayName, debugItemContent.ActualType, new ObservableCollection<IServiceTestOutput>()) as ServiceTestStep;
                    var hasOutputs = outputs?.Select(item => item.ResultsList).All(list => list.Count > 0);
                    var debugStateActivityTypeName = debugState.ActivityTypeName;
                    // ReSharper disable once PossibleNullReferenceException
                    if (outputs.Count > 0 && hasOutputs.HasValue && hasOutputs.Value)
                    {
                        AddOutputs(outputs, serviceTestStep);
                    }
                    else
                    {
                        var type = Types.FirstOrDefault(x => x.Name == debugStateActivityTypeName);
                        if (type != null)
                        {
                            var act = Activator.CreateInstance(type) as IDev2Activity;
                            if (serviceTestStep != null)
                            {
                                serviceTestStep.StepOutputs =
                                    AddOutputs(act?.GetOutputs(), serviceTestStep).ToObservableCollection();
                            }
                        }
                    }
                    if (serviceTestStep != null)
                    {
                        if (debugStateActivityTypeName == typeof(TestMockStep).Name)
                        {
                            var model = WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ID);
                            var val = model.GetCurrentValue();
                            serviceTestStep.MockSelected = true;
                            serviceTestStep.AssertSelected = false;
                            serviceTestStep.Type = StepType.Mock;
                            serviceTestStep.ActivityType = val.GetType().Name;
                        }
                        SetStepIcon(serviceTestStep.ActivityType, serviceTestStep);
                    }
                }
            }
        }

        private void AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            if (NullParent(debugItemContent, ref parent)) return;
            if (parent.ActivityType == typeof(DsfForEachActivity).Name)
            {
                ForEachParent(debugItemContent, debugState, parent);
            }
            else if (parent.ActivityType == typeof(DsfSequenceActivity).Name)
            {
                var model = WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ID);
                var sequence = model?.GetCurrentValue() as DsfSequenceActivity;
                if (sequence != null)
                {
                    parent.UniqueId = Guid.Parse(sequence.UniqueID);
                    AddChildren(debugState, parent);
                }
            }
            else
            {
                if (parent.ActivityType == typeof(DsfActivity).Name)
                {
                    var childItem = debugState as DebugStateTreeViewItemViewModel;
                    if (childItem != null)
                    {
                        var content = childItem.Content;
                        var outputs = content.Outputs;
                        var serviceTestStep = (ServiceTestStep)parent;
                        AddOutputs(outputs, serviceTestStep);
                        SetStepIcon(serviceTestStep.ActivityType, serviceTestStep);

                    }
                }
                else
                {
                    AddChildren(debugState, parent);
                }
            }
            while (parent != null)
            {
                var child = parent;
                if (child.Parent == null)
                {
                    var exists = FindExistingStep(child.UniqueId.ToString());
                    if (exists == null)
                    {
                        SelectedServiceTest.TestSteps.Add(child);
                    }
                }
                parent = child.Parent;
            }
        }

        private void ForEachParent(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            var model = WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ID);
            var forEach = model.GetCurrentValue() as DsfForEachActivity;
            if (forEach != null)
            {
                var act = forEach.DataFunc.Handler as IDev2Activity;
                var childItem = debugState.Children.LastOrDefault() as DebugStateTreeViewItemViewModel;
                if (childItem != null)
                {
                    if (act != null)
                    {
                        Guid guid = Guid.Parse(act.UniqueID);
                        childItem.Content.ID = guid;
                    }
                    var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();

                    var childItemContent = childItem.Content;
                    var outputs = childItemContent.Outputs;

                    var exists = parent.Children.FirstOrDefault(a => a.UniqueId == childItemContent.ID);
                    if (exists == null)
                    {
                        var childStep = new ServiceTestStep(childItemContent.ID, childItemContent.ActualType, serviceTestOutputs, StepType.Assert)
                        {
                            StepDescription = childItemContent.DisplayName,
                            Parent = parent,
                            Type = StepType.Assert
                        };
                        if (outputs.Count > 0)
                        {
                            AddOutputs(outputs, childStep);
                        }
                        else
                        {
                            AddOutputs(act?.GetOutputs(), childStep);
                        }
                        SetStepIcon(childStep.ActivityType, childStep);
                        parent.Children.Add(childStep);
                        if (childItem.Children != null && childItem.Children.Count > 0)
                        {
                            AddChildDebugItems(childItemContent, childItem, childStep);
                        }
                    }
                }
            }
        }

        private bool NullParent(IDebugState debugItemContent, ref IServiceTestStep parent)
        {
            if (parent == null)
            {
                var testStep = new ServiceTestStep(debugItemContent.ID, "",
                    new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                {
                    StepDescription = debugItemContent.DisplayName,
                    Parent = null
                };

                var seqTypeName = typeof(DsfSequenceActivity).Name;
                var forEachTypeName = typeof(DsfForEachActivity).Name;
                var selectApplyTypeName = typeof(DsfSelectAndApplyActivity).Name;
                var serviceName = typeof(DsfActivity).Name;
                var actualType = debugItemContent.ActualType;
                if (actualType == seqTypeName)
                {
                    SetStepIcon(typeof(DsfSequenceActivity), testStep);
                    testStep.ActivityType = seqTypeName;
                    testStep.UniqueId = debugItemContent.WorkSurfaceMappingId;
                    parent = testStep;
                }

                else if (actualType == forEachTypeName)
                {
                    SetStepIcon(typeof(DsfForEachActivity), testStep);
                    testStep.ActivityType = forEachTypeName;
                    testStep.UniqueId = debugItemContent.WorkSurfaceMappingId;
                    parent = testStep;
                }
                else if (actualType == selectApplyTypeName)
                {
                    SetStepIcon(typeof(DsfSelectAndApplyActivity), testStep);
                    testStep.ActivityType = selectApplyTypeName;
                    testStep.UniqueId = debugItemContent.WorkSurfaceMappingId;
                    parent = testStep;
                }
                else if (actualType == serviceName)
                {
                    SetStepIcon(typeof(DsfActivity), testStep);
                    testStep.ActivityType = serviceName;
                    parent = testStep;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private void AddChildren(IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            foreach (var debugTreeViewItemViewModel in debugState.Children)
            {
                var childItem = debugTreeViewItemViewModel as DebugStateTreeViewItemViewModel;
                if (childItem != null && childItem.ActivityTypeName != "DsfSelectAndApplyActivity")
                {
                    var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();

                    var childItemContent = childItem.Content;
                    var outputs = childItemContent.Outputs;

                    var exists = parent.Children.FirstOrDefault(a => a.UniqueId == childItemContent.ID);
                    if (exists == null)
                    {
                        var childStep = new ServiceTestStep(childItemContent.ID, childItemContent.ActualType, serviceTestOutputs, StepType.Assert)
                        {
                            StepDescription = childItemContent.DisplayName,
                            Parent = parent,
                            Type = StepType.Assert
                        };
                        if (outputs.Count > 0)
                        {
                            AddOutputs(outputs, childStep);
                        }
                        else
                        {
                            var type = Types.FirstOrDefault(x => x.Name == childItem.ActivityTypeName);
                            if (type != null)
                            {
                                var act = Activator.CreateInstance(type) as IDev2Activity;
                                childStep.StepOutputs = AddOutputs(act?.GetOutputs(), childStep).ToObservableCollection();
                            }
                        }
                        SetStepIcon(childStep.ActivityType, childStep);
                        if (childStep.StepOutputs != null && childStep.StepOutputs.Count > 0 && parent.ActivityType==typeof(DsfEnhancedDotNetDllActivity).Name)
                        {
                            parent.Children.Add(childStep);
                            if (childItem.Children != null && childItem.Children.Count > 0)
                            {
                                AddChildDebugItems(childItemContent, childItem, childStep);
                            }
                        }
                    }
                    else
                    {
                        var serviceTestStep = exists as ServiceTestStep;
                        if (serviceTestStep != null)
                        {
                            if (outputs.Count > 0)
                            {
                                AddOutputs(outputs, serviceTestStep);
                            }
                            else
                            {
                                var type = Types.FirstOrDefault(x => x.Name == childItem.ActivityTypeName);
                                if (type != null)
                                {
                                    var act = Activator.CreateInstance(type) as IDev2Activity;
                                    serviceTestStep.StepOutputs = AddOutputs(act?.GetOutputs(), serviceTestStep).ToObservableCollection();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddOutputs(List<IDebugItem> outputs, ServiceTestStep serviceTestStep)
        {
            if (outputs != null && outputs.Count > 0)
            {
                var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                foreach (var output in outputs)
                {
                    var actualOutputs = output.ResultsList.Where(result => result.Type == DebugItemResultType.Variable);
                    foreach (var debugItemResult in actualOutputs)
                    {
                        var variable = debugItemResult.Variable;
                        var value = debugItemResult.Value;
                        var assertOp = "=";
                        if (debugItemResult.MoreLink != null)
                        {
                            if (serviceTestStep.ActivityType == typeof(DsfEnhancedDotNetDllActivity).Name)
                            {
                                var realValue = WebClient.DownloadString(debugItemResult.MoreLink);
                                value = realValue.TrimEnd(Environment.NewLine.ToCharArray());
                            }
                            else
                            {
                                assertOp = "Contains";
                            }
                        }
                        var serviceTestOutput = new ServiceTestOutput(variable ?? "", value, "", "")
                        {
                            AssertOp = assertOp,
                            AddStepOutputRow = s => { serviceTestStep.AddNewOutput(s); }
                        };

                        serviceTestOutputs.Add(serviceTestOutput);
                    }
                }
                serviceTestStep.StepOutputs = serviceTestOutputs;
            }
        }

        private void SetInputs(IDebugState inputState)
        {
            if (inputState != null)
            {
                foreach (var debugItem in inputState.Inputs)
                {
                    var variable = debugItem.ResultsList.First().Variable.Replace("[[", "").Replace("]]", "");
                    var value = debugItem.ResultsList.First().Value;
                    var serviceTestInput = SelectedServiceTest?.Inputs?.FirstOrDefault(input => input.Variable.Equals(variable));
                    if (serviceTestInput != null)
                    {
                        serviceTestInput.Value = value;
                    }
                }
            }
        }

        public IWarewolfWebClient WebClient
        {
            private get
            {
                return _webClient ?? CustomContainer.Get<IWarewolfWebClient>();
            }
            set
            {
                _webClient = value;
            }
        }

        private void SetOutputs(IDebugState outPutState)
        {
            if (outPutState != null)
            {
                foreach (var debugItem in outPutState.Outputs)
                {
                    var debugItemResult = debugItem.ResultsList.First();
                    var variable = debugItemResult.Variable.Replace("[[", "").Replace("]]", "");
                    var value = debugItemResult.Value;
                    var serviceTestOutput = SelectedServiceTest.Outputs.FirstOrDefault(input => input.Variable.Equals(variable));
                    if (serviceTestOutput != null)
                    {
                        if (!string.IsNullOrEmpty(debugItemResult.MoreLink))
                        {
                            if (outPutState.ActualType == typeof(DsfEnhancedDotNetDllActivity).Name)
                            {
                                var realValue = WebClient.DownloadString(debugItemResult.MoreLink);
                                value = realValue.TrimEnd(Environment.NewLine.ToCharArray());
                            }
                            else
                            {
                                serviceTestOutput.AssertOp = "Contains";
                            }
                        }
                        serviceTestOutput.Value = value;
                    }
                }
                SelectedServiceTest.ErrorExpected = outPutState.HasError;
                SelectedServiceTest.NoErrorExpected = !outPutState.HasError;
                SelectedServiceTest.ErrorContainsText = outPutState.ErrorMessage;
            }
        }

        private void OnError(Exception exception)
        {
            Dev2Logger.Error(exception);
            throw exception;
        }

        private void ItemSelectedAction(ModelItem modelItem)
        {

            if (modelItem != null)
            {
                var itemType = modelItem.ItemType;
                itemType = GetInnerItemType(modelItem, itemType);
                if (itemType == typeof(Flowchart) || itemType == typeof(ActivityBuilder))
                {
                    return;
                }
                if (itemType == typeof(DsfForEachActivity))
                {
                    ProcessForEach(modelItem);
                }
                else if (itemType == typeof(DsfSelectAndApplyActivity))
                {
                    ProcessSelectAndApply(modelItem);
                }
                else if (itemType == typeof(DsfSequenceActivity))
                {
                    ProcessSequence(modelItem);
                }
                else if (itemType == typeof(DsfEnhancedDotNetDllActivity))
                {
                    ProcessEnhanchedDotNetDll(modelItem);
                }
                else if (itemType == typeof(FlowSwitch<string>))
                {
                    ProcessFlowSwitch(modelItem);
                }
                else if (itemType == typeof(DsfSwitch))
                {
                    ProcessSwitch(modelItem);
                }
                else if (itemType == typeof(FlowDecision))
                {
                    ProcessFlowDecision(modelItem);
                }
                else if (itemType == typeof(DsfDecision))
                {
                    ProcessDecision(modelItem);
                }
                else
                {
                    ProcessActivity(modelItem);
                }
            }
        }

        private static Type GetInnerItemType(ModelItem modelItem, Type itemType)
        {
            if (itemType == typeof(FlowStep))
            {
                if (modelItem.Content?.Value != null)
                {
                    itemType = modelItem.Content.Value.ItemType;
                }
            }
            return itemType;
        }

        private void ProcessSequence(ModelItem modelItem)
        {
            var sequence = GetCurrentActivity<DsfSequenceActivity>(modelItem);
            var buildParentsFromModelItem = BuildParentsFromModelItem(modelItem);
            if (buildParentsFromModelItem != null)
            {
                AddSequence(sequence, buildParentsFromModelItem as ServiceTestStep, buildParentsFromModelItem.Children);
                if (FindExistingStep(buildParentsFromModelItem.UniqueId.ToString()) == null)
                {
                    SelectedServiceTest.TestSteps.Add(buildParentsFromModelItem);
                }
            }
            else
            {
                AddSequence(sequence, null, SelectedServiceTest.TestSteps);
            }
        }

        private void ProcessEnhanchedDotNetDll(ModelItem modelItem)
        {
            var dotNetDllActivity = GetCurrentActivity<DsfEnhancedDotNetDllActivity>(modelItem);
            var buildParentsFromModelItem = BuildParentsFromModelItem(modelItem);
            if (buildParentsFromModelItem != null)
            {
                AddEnhancedDotNetDll(dotNetDllActivity, buildParentsFromModelItem as ServiceTestStep, buildParentsFromModelItem.Children);
                if (FindExistingStep(buildParentsFromModelItem.UniqueId.ToString()) == null)
                {
                    SelectedServiceTest.TestSteps.Add(buildParentsFromModelItem);
                }
            }
            else
            {
                AddEnhancedDotNetDll(dotNetDllActivity, null, SelectedServiceTest.TestSteps);
            }
        }

        private T GetCurrentActivity<T>(ModelItem modelItem) where T : class
        {
            var activity = modelItem.GetCurrentValue() as T;
            if (activity == null)
            {
                if (modelItem.Content?.Value != null)
                    activity = modelItem.Content.Value.GetCurrentValue() as T;
            }
            return activity;
        }

        private void ProcessForEach(ModelItem modelItem)
        {

            var forEachActivity = GetCurrentActivity<DsfForEachActivity>(modelItem);
            AddForEach(forEachActivity, null, SelectedServiceTest.TestSteps);
        }

        private void AddForEach(DsfForEachActivity forEachActivity, ServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (forEachActivity != null)
            {
                var uniqueId = forEachActivity.UniqueID;
                var exists = serviceTestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

                var type = typeof(DsfForEachActivity);
                var testStep = new ServiceTestStep(Guid.Parse(uniqueId), type.Name, new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
                {
                    StepDescription = forEachActivity.DisplayName,
                    Parent = parent
                };
                SetStepIcon(type, testStep);
                var activity = forEachActivity.DataFunc.Handler;
                var act = activity as DsfNativeActivity<string>;
                var workFlowService = activity as DsfActivity;
                if (act != null)
                {
                    if (act.GetType() == typeof(DsfSequenceActivity))
                    {
                        AddSequence(act as DsfSequenceActivity, testStep, testStep.Children);
                    }
                    else
                    {
                        AddChildActivity(act, testStep);
                    }

                }
                else
                {
                    if (activity.GetType() == typeof(DsfSelectAndApplyActivity))
                    {
                        AddSelectAndApply(activity as DsfSelectAndApplyActivity, testStep, testStep.Children);
                    }
                    else if (activity.GetType() == type)
                    {
                        AddForEach(activity as DsfForEachActivity, testStep, testStep.Children);
                    }

                }

                if (workFlowService != null)
                {
                    AddChildActivity(workFlowService, testStep);
                }
                if (exists == null)
                {
                    serviceTestSteps.Add(testStep);
                }
            }
        }

        private void ProcessSelectAndApply(ModelItem modelItem)
        {
            var selectAndApplyActivity = GetCurrentActivity<DsfSelectAndApplyActivity>(modelItem);
            AddSelectAndApply(selectAndApplyActivity, null, SelectedServiceTest.TestSteps);
        }

        private void AddSelectAndApply(DsfSelectAndApplyActivity selecteApplyActivity, ServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (selecteApplyActivity != null)
            {
                var uniqueId = selecteApplyActivity.UniqueID;
                var exists = serviceTestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

                var testStep = new ServiceTestStep(Guid.Parse(uniqueId), typeof(DsfSelectAndApplyActivity).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
                {
                    StepDescription = selecteApplyActivity.DisplayName,
                    Parent = parent
                };
                SetStepIcon(typeof(DsfSelectAndApplyActivity), testStep);
                var activity = selecteApplyActivity.ApplyActivityFunc.Handler;
                var act = activity as DsfNativeActivity<string>;
                var workFlowService = activity as DsfActivity;
                if (act != null)
                {
                    if (act.GetType() == typeof(DsfSequenceActivity))
                    {
                        AddSequence(act as DsfSequenceActivity, testStep, testStep.Children);
                    }
                    else
                    {
                        AddChildActivity(act, testStep);
                    }
                }
                else
                {
                    if (activity != null && activity.GetType() == typeof(DsfForEachActivity))
                    {
                        AddForEach(activity as DsfForEachActivity, testStep, testStep.Children);
                    }
                    else if (activity != null && activity.GetType() == typeof(DsfSelectAndApplyActivity))
                    {
                        AddSelectAndApply(activity as DsfSelectAndApplyActivity, testStep, testStep.Children);
                    }
                }
                if (workFlowService != null)
                {
                    AddChildActivity(workFlowService, testStep);
                }
                if (exists == null)
                {
                    serviceTestSteps.Add(testStep);
                }
                else
                {
                    AddMissingChild(serviceTestSteps, testStep);
                }
            }
        }

        private void AddSequence(DsfSequenceActivity sequence, ServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (sequence != null)
            {
                var uniqueId = sequence.UniqueID;
                var exists = serviceTestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

                var testStep = new ServiceTestStep(Guid.Parse(uniqueId), typeof(DsfSequenceActivity).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
                {
                    StepDescription = sequence.DisplayName,
                    Parent = parent
                };
                SetStepIcon(typeof(DsfSequenceActivity), testStep);
                foreach (var activity in sequence.Activities)
                {
                    var act = activity as DsfNativeActivity<string>;
                    if (act != null)
                    {
                        if (act.GetType() == typeof(DsfSequenceActivity))
                        {
                            AddSequence(act as DsfSequenceActivity, testStep, testStep.Children);
                        }
                        else
                        {
                            AddChildActivity(act, testStep);
                        }
                    }
                    else
                    {
                        if (activity.GetType() == typeof(DsfForEachActivity))
                        {
                            AddForEach(activity as DsfForEachActivity, testStep, testStep.Children);
                        }
                        else if (activity.GetType() == typeof(DsfSelectAndApplyActivity))
                        {
                            AddSelectAndApply(activity as DsfSelectAndApplyActivity, testStep, testStep.Children);
                        }
                    }
                }
                if (exists == null)
                {
                    serviceTestSteps.Add(testStep);

                }
                else
                {
                    AddMissingChild(serviceTestSteps, testStep);
                }
            }
        }

        private void AddEnhancedDotNetDll(DsfEnhancedDotNetDllActivity dotNetDllActivity, ServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (dotNetDllActivity != null)
            {
                var uniqueId = dotNetDllActivity.UniqueID;
                var exists = serviceTestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

                var testStep = new ServiceTestStep(Guid.Parse(uniqueId), typeof(DsfEnhancedDotNetDllActivity).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
                {
                    StepDescription = dotNetDllActivity.DisplayName,
                    Parent = parent
                };
                SetStepIcon(typeof(DsfEnhancedDotNetDllActivity), testStep);
                if (exists == null)
                {
                    serviceTestSteps.Add(testStep);
                    AddEnhancedDotNetDllConstructor(dotNetDllActivity, testStep);
                    foreach (var pluginAction in dotNetDllActivity.MethodsToRun)
                    {
                        if (!pluginAction.IsVoid)
                        {
                            AddEnhancedDotNetDllMethod(pluginAction, testStep);
                        }
                    }
                }
                else
                {
                    AddMissingChild(serviceTestSteps, exists);
                    var constructorStepExists = exists.Children.FirstOrDefault(step => step.UniqueId == dotNetDllActivity.Constructor.ID);
                    if (constructorStepExists == null)
                    {
                        AddEnhancedDotNetDllConstructor(dotNetDllActivity, exists);
                    }
                    foreach (var pluginAction in dotNetDllActivity.MethodsToRun)
                    {
                        if (!pluginAction.IsVoid)
                        {
                            IServiceTestStep actionExists = exists.Children.FirstOrDefault(step => step.UniqueId == pluginAction.ID);
                            if (actionExists != null)
                            {

                                AddEnhancedDotNetDllMethod(pluginAction, exists);
                            }
                        }
                    }
                }
            }
        }

        private static void AddMissingChild(ObservableCollection<IServiceTestStep> serviceTestSteps, IServiceTestStep testStep)
        {
            if (serviceTestSteps.Count > 0)
            {
                foreach (var serviceTestStep in serviceTestSteps)
                {
                    if (serviceTestStep.UniqueId == testStep.UniqueId)
                    {
                        if (serviceTestStep.Children.Count == testStep.Children.Count)
                        {
                            foreach (var child in testStep.Children)
                            {
                                var testStepChild = child as ServiceTestStep;
                                AddMissingChild(serviceTestStep.Children, testStepChild);
                            }

                        }
                        else if (serviceTestStep.Children.Count != testStep.Children.Count)
                        {
                            foreach (var child in testStep.Children)
                            {
                                var testSteps = serviceTestStep.Children.Where(a => a.UniqueId == child.UniqueId);
                                if (!testSteps.Any())
                                {
                                    var indexOf = testStep.Children.IndexOf(child);
                                    child.Parent = serviceTestStep;
                                    serviceTestStep.Children.Insert(indexOf, child);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddChildActivity<T>(DsfNativeActivity<T> act, ServiceTestStep testStep)
        {
            var outputs = act.GetOutputs();
            if (outputs != null && outputs.Count > 0)
            {
                var serviceTestStep = new ServiceTestStep(Guid.Parse(act.UniqueID), act.GetType().Name, new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
                {
                    StepDescription = act.DisplayName,
                    Parent = testStep
                };
                var serviceTestOutputs = outputs.Select(output => new ServiceTestOutput(output, "", "", "")
                {
                    HasOptionsForValue = false,
                    AddStepOutputRow = s => { serviceTestStep.AddNewOutput(s); }
                }).Cast<IServiceTestOutput>().ToObservableCollection();
                serviceTestStep.StepOutputs = serviceTestOutputs;
                SetStepIcon(act.GetType(), serviceTestStep);
                testStep.Children.Add(serviceTestStep);
            }
        }

        private void AddEnhancedDotNetDllConstructor(DsfEnhancedDotNetDllActivity dotNetConstructor, IServiceTestStep testStep)
        {
            var serviceTestStep = new ServiceTestStep(dotNetConstructor.Constructor.ID, testStep.ActivityType,
                new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
            {
                StepDescription = dotNetConstructor.Constructor.ConstructorName,
                Parent = testStep
            };
            var serviceOutputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput(dotNetConstructor.ObjectName ?? "", "", "", "")
            };
            serviceTestStep.StepOutputs = serviceOutputs;
            SetStepIcon(testStep.ActivityType, serviceTestStep);
            testStep.Children.Insert(0, serviceTestStep);
        }

        private void AddEnhancedDotNetDllMethod(IPluginAction pluginAction, IServiceTestStep testStep)
        {
            var serviceTestStep = new ServiceTestStep(pluginAction.ID, testStep.ActivityType,
                new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
            {
                StepDescription = pluginAction.Method,
                Parent = testStep
            };
            var serviceOutputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput(pluginAction.OutputVariable ?? "", "", "", "")
            };
            serviceTestStep.StepOutputs = serviceOutputs;
            SetStepIcon(testStep.ActivityType, serviceTestStep);
            testStep.Children.Add(serviceTestStep);
        }

        private void ProcessSwitch(ModelItem modelItem)
        {
            var cases = modelItem.GetProperty("Switches") as Dictionary<string, IDev2Activity>;
            var defaultCase = modelItem.GetProperty("Default") as List<IDev2Activity>;
            var uniqueId = modelItem.GetProperty("UniqueID").ToString();
            var exists = SelectedServiceTest.TestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

            if (exists == null)
            {
                if (SelectedServiceTest != null)
                {
                    var switchOptions = cases?.Select(pair => pair.Key).ToList();
                    if (defaultCase != null)
                    {
                        switchOptions?.Insert(0, "Default");
                    }
                    var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                    var serviceTestOutput = new ServiceTestOutput(GlobalConstants.ArmResultText, "", "", "")
                    {
                        HasOptionsForValue = true,
                        OptionsForValue = switchOptions
                    };
                    serviceTestOutputs.Add(serviceTestOutput);
                    var serviceTestStep = SelectedServiceTest.AddTestStep(uniqueId, modelItem.GetProperty("DisplayName").ToString(), typeof(DsfSwitch).Name, serviceTestOutputs) as ServiceTestStep;
                    if (serviceTestStep != null)
                        SetStepIcon(typeof(DsfSwitch), serviceTestStep);
                }
            }
        }

        private ServiceTestStep ProcessFlowSwitch(ModelItem modelItem)
        {
            if (modelItem != null)
            {
                var condition = modelItem.GetProperty("Expression");
                var activity = (DsfFlowNodeActivity<string>)condition;
                var flowSwitch = GetCurrentActivity<FlowSwitch<string>>(modelItem);
                if (flowSwitch == null)
                {
                    var modelItemParent = modelItem.Parent;
                    if (modelItemParent != null)
                    {
                        flowSwitch = GetCurrentActivity<FlowSwitch<string>>(modelItemParent);
                        condition = modelItemParent.GetProperty("Expression");
                    }
                    activity = (DsfFlowNodeActivity<string>)condition;
                }
                if (flowSwitch != null)
                {
                    var cases = flowSwitch.Cases;
                    var defaultCase = flowSwitch.Default;
                    var uniqueId = activity.UniqueID;
                    var exists = SelectedServiceTest.TestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

                    if (exists == null)
                    {
                        if (SelectedServiceTest != null)
                        {
                            var switchOptions = cases?.Select(pair => pair.Key).ToList();
                            if (defaultCase != null)
                            {
                                switchOptions?.Insert(0, "Default");
                            }
                            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                            var serviceTestOutput = new ServiceTestOutput(GlobalConstants.ArmResultText, "", "", "")
                            {
                                HasOptionsForValue = true,
                                OptionsForValue = switchOptions
                            };
                            serviceTestOutputs.Add(serviceTestOutput);
                            var serviceTestStep = SelectedServiceTest.AddTestStep(uniqueId, flowSwitch.DisplayName, typeof(DsfSwitch).Name, serviceTestOutputs) as ServiceTestStep;
                            if (serviceTestStep != null)
                                SetStepIcon(typeof(DsfSwitch), serviceTestStep);
                            return serviceTestStep;
                        }
                    }
                }
            }
            return null;
        }

        private void ProcessActivity(ModelItem modelItem)
        {
            var step = BuildParentsFromModelItem(modelItem);
            if (step != null)
            {
                if (step.Parent == null)
                {
                    var exists = FindExistingStep(step.UniqueId.ToString());
                    if (exists == null)
                    {
                        SelectedServiceTest.TestSteps.Add(step);
                    }
                }
                else
                {
                    var parent = step.Parent;
                    while (parent != null)
                    {
                        var child = parent;
                        if (child.Parent == null)
                        {
                            var exists = FindExistingStep(step.UniqueId.ToString());
                            if (exists == null)
                            {
                                SelectedServiceTest.TestSteps.Add(child);
                            }
                        }
                        parent = child.Parent;
                    }
                }
            }
        }

        private IServiceTestStep BuildParentsFromModelItem(ModelItem modelItem)
        {

            var computedValue = modelItem.GetCurrentValue();
            if (computedValue is FlowStep)
            {
                if (modelItem.Content?.Value != null)
                {
                    computedValue = modelItem.Content.Value.GetCurrentValue();
                }
            }
            var dsfActivityAbstract = computedValue as DsfActivityAbstract<string>;

            var activityUniqueID = dsfActivityAbstract?.UniqueID;
            var activityDisplayName = dsfActivityAbstract?.DisplayName;
            var outputs = dsfActivityAbstract?.GetOutputs();

            if (dsfActivityAbstract == null)
            {
                var boolAct = computedValue as DsfActivityAbstract<bool>;

                activityUniqueID = boolAct?.UniqueID;
                activityDisplayName = boolAct?.DisplayName;
                outputs = boolAct?.GetOutputs();
            }

            var type = computedValue.GetType();
            var item = modelItem.Parent;

            if (item != null && (item.ItemType != typeof(Flowchart) || item.ItemType == typeof(ActivityBuilder)))
            {
                var parentComputedValue = item.GetCurrentValue();
                if (parentComputedValue is FlowStep)
                {
                    if (item.Content?.Value != null)
                    {
                        parentComputedValue = item.Content.Value.GetCurrentValue();
                    }
                    var parentActivityAbstract = parentComputedValue as DsfActivityAbstract<string>;
                    var parentActivityUniqueID = parentActivityAbstract?.UniqueID;
                    if (parentActivityAbstract == null)
                    {
                        var boolParentAct = computedValue as DsfActivityAbstract<bool>;
                        parentActivityUniqueID = boolParentAct?.UniqueID;
                    }
                    if (parentActivityUniqueID == activityUniqueID)
                    {
                        return CheckForExists(activityUniqueID, outputs, activityDisplayName, type);
                    }
                }

                if (outputs != null && outputs.Count > 0)
                {
                    IServiceTestStep serviceTestStep;
                    if (ServiceTestStepWithOutputs(activityUniqueID, activityDisplayName, outputs, type, item, out serviceTestStep))
                    {
                        return serviceTestStep;
                    }
                }
                IServiceTestStep serviceTestStep1;
                if (ServiceTestStepGetParentType(item, out serviceTestStep1))
                {
                    return serviceTestStep1;
                }
                return BuildParentsFromModelItem(item);
            }
            return CheckForExists(activityUniqueID, outputs, activityDisplayName, type);
        }

        private IServiceTestStep CheckForExists(string activityUniqueID, List<string> outputs, string activityDisplayName, Type type)
        {
            var exists = FindExistingStep(activityUniqueID);
            if (exists == null)
            {
                if (outputs != null && outputs.Count > 0)
                {
                    var serviceTestStep = SelectedServiceTest.AddTestStep(activityUniqueID, activityDisplayName, type.Name, new ObservableCollection<IServiceTestOutput>()) as ServiceTestStep;

                    var serviceTestOutputs = outputs.Select(output =>
                    {
                        return new ServiceTestOutput(output ?? "", "", "", "")
                        {
                            HasOptionsForValue = false,
                            AddStepOutputRow = s => { serviceTestStep?.AddNewOutput(s); }
                        };
                    }).Cast<IServiceTestOutput>().ToList();
                    if (serviceTestStep != null)
                    {
                        serviceTestStep.StepOutputs = serviceTestOutputs.ToObservableCollection();
                        SetStepIcon(type, serviceTestStep);

                        return serviceTestStep;
                    }
                }
            }
            return null;
        }

        private bool ServiceTestStepWithOutputs(string uniqueID, string displayName, List<string> outputs, Type type, ModelItem item, out IServiceTestStep serviceTestStep)
        {
            var exists = FindExistingStep(uniqueID);
            if (exists == null)
            {
                var step = CreateServiceTestStep(Guid.Parse(uniqueID), displayName, type, new List<IServiceTestOutput>());
                var serviceTestOutputs = AddOutputsIfHasVariable(outputs, step);
                step.StepOutputs = serviceTestOutputs.ToObservableCollection();
                SetParentChild(item, step);
                {
                    serviceTestStep = step;
                    return true;
                }
            }
            serviceTestStep = null;
            return false;
        }

        private static List<IServiceTestOutput> AddOutputsIfHasVariable(List<string> outputs, ServiceTestStep step)
        {
            var serviceTestOutputs =
                outputs.Select(output => new ServiceTestOutput(output ?? "", "", "", "")
                {
                    HasOptionsForValue = false,
                    AddStepOutputRow = s => step.AddNewOutput(s)
                }).Cast<IServiceTestOutput>().ToList();
            return serviceTestOutputs;
        }

        private static List<IServiceTestOutput> AddOutputs(List<string> outputs, ServiceTestStep step)
        {
            if (outputs == null || outputs.Count == 0)
            {
                return new List<IServiceTestOutput>
                {
                    new ServiceTestOutput("", "", "", "")
                    {
                        HasOptionsForValue = false,
                        AddStepOutputRow = s => step.AddNewOutput(s)
                    }
                };
            }
            var serviceTestOutputs =
                outputs.Select(output => new ServiceTestOutput(output ?? "", "", "", "")
                {
                    HasOptionsForValue = false,
                    AddStepOutputRow = s => step.AddNewOutput(s)
                }).Cast<IServiceTestOutput>().ToList();
            return serviceTestOutputs;
        }

        private IServiceTestStep FindExistingStep(string uniqueId)
        {
            var exists = SelectedServiceTest.TestSteps.Flatten(step => step.Children).FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);
            return exists;
        }

        private bool ServiceTestStepGetParentType(ModelItem item, out IServiceTestStep serviceTestStep)
        {
            Type activityType = null;
            var uniqueId = string.Empty;
            var displayName = string.Empty;
            if (item.ItemType == typeof(DsfSequenceActivity))
            {
                var act = item.GetCurrentValue() as DsfSequenceActivity;
                if (act != null)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof(DsfSequenceActivity);
                    displayName = act.DisplayName;
                }
            }
            else if (item.ItemType == typeof(DsfForEachActivity))
            {
                var act = item.GetCurrentValue() as DsfForEachActivity;
                if (act != null)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof(DsfForEachActivity);
                    displayName = act.DisplayName;
                }
            }
            else if (item.ItemType == typeof(DsfSelectAndApplyActivity))
            {
                var act = item.GetCurrentValue() as DsfSelectAndApplyActivity;
                if (act != null)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof(DsfSelectAndApplyActivity);
                    displayName = act.DisplayName;
                }
            }
            if (!string.IsNullOrWhiteSpace(uniqueId))
            {
                var exists = FindExistingStep(uniqueId);
                if (exists == null)
                {
                    var step = CreateServiceTestStep(Guid.Parse(uniqueId), displayName, activityType, new List<IServiceTestOutput>());
                    SetParentChild(item, step);
                    {
                        serviceTestStep = step;
                        return true;
                    }
                }
                serviceTestStep = SelectedServiceTest.TestSteps.Flatten(step => step.Children).FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);
                return true;
            }
            serviceTestStep = null;
            return false;
        }

        private ServiceTestStep CreateServiceTestStep(Guid uniqueID, string displayName, Type type, List<IServiceTestOutput> serviceTestOutputs)
        {
            var step = new ServiceTestStep(uniqueID, type.Name, serviceTestOutputs.ToObservableCollection(), StepType.Assert)
            {
                StepDescription = displayName
            };
            SetStepIcon(type, step);
            return step;
        }

        private void SetParentChild(ModelItem item, ServiceTestStep step)
        {
            var parent = BuildParentsFromModelItem(item);
            if (parent != null)
            {
                step.Parent = parent;
                parent.Children.Add(step);
            }
        }

        private void SetStepIcon(Type type, ServiceTestStep serviceTestStep)
        {
            if (type == null)
                return;
            if (type.Name == "DsfDecision" || type.Name == "FlowDecision")
            {
                type = typeof(DsfFlowDecisionActivity);
            }
            if (type.Name == "DsfSwitch")
            {
                type = typeof(DsfFlowSwitchActivity);
            }
            if (type.GetCustomAttributes().Any(a => a is ToolDescriptorInfo))
            {
                var desc = GetDescriptorFromAttribute(type);
                if (serviceTestStep != null)
                    serviceTestStep.StepIcon = Application.Current?.TryFindResource(desc.Icon) as ImageSource;
            }
            if (type.Name == "DsfActivity")
            {
                if (serviceTestStep != null)
                    serviceTestStep.StepIcon = Application.Current?.TryFindResource("Explorer-WorkflowService") as ImageSource;
            }
        }

        private void SetStepIcon(string typeName, ServiceTestStep serviceTestStep)
        {
            if (string.IsNullOrEmpty(typeName))
                return;
            if (typeName == "DsfDecision" || typeName == "FlowDecision")
            {
                typeName = "DsfFlowDecisionActivity";
            }
            if (typeName == "DsfSwitch")
            {
                typeName = "DsfFlowSwitchActivity";
            }
            Type type = Types.FirstOrDefault(x => x.Name == typeName);

            if (type != null && type.GetCustomAttributes().Any(a => a is ToolDescriptorInfo))
            {
                var desc = GetDescriptorFromAttribute(type);
                if (serviceTestStep != null)
                    serviceTestStep.StepIcon = Application.Current?.TryFindResource(desc.Icon) as ImageSource;
            }
            if (type != null && type.Name == "DsfActivity")
            {
                if (serviceTestStep != null)
                    serviceTestStep.StepIcon = Application.Current?.TryFindResource("Explorer-WorkflowService") as ImageSource;
            }
        }

        IToolDescriptor GetDescriptorFromAttribute(Type type)
        {
            var info = type.GetCustomAttributes(typeof(ToolDescriptorInfo)).First() as ToolDescriptorInfo;
            // ReSharper disable once PossibleNullReferenceException
            return new ToolDescriptor(info.Id, info.Designer, new WarewolfType(type.FullName, type.Assembly.GetName().Version, type.Assembly.Location), info.Name, info.Icon, type.Assembly.GetName().Version, true, info.Category, ToolType.Native, info.IconUri, info.FilterTag, info.ResourceToolTip, info.ResourceHelpText);
        }

        private void ProcessDecision(ModelItem modelItem)
        {
            if (modelItem != null)
            {
                Dev2DecisionStack dds = modelItem.GetProperty("Conditions") as Dev2DecisionStack;
                var uniqueId = modelItem.GetProperty("UniqueID").ToString();
                var exists = SelectedServiceTest.TestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

                if (exists == null)
                {
                    if (SelectedServiceTest != null)
                    {
                        var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                        if (dds != null)
                        {
                            var serviceTestOutput = new ServiceTestOutput(GlobalConstants.ArmResultText, "", "", "")
                            {
                                HasOptionsForValue = true,
                                OptionsForValue = new List<string> { dds.TrueArmText, dds.FalseArmText }
                            };
                            serviceTestOutputs.Add(serviceTestOutput);
                        }
                        var serviceTestStep = SelectedServiceTest.AddTestStep(uniqueId, modelItem.GetProperty("DisplayName").ToString(), typeof(DsfDecision).Name, serviceTestOutputs) as ServiceTestStep;
                        if (serviceTestStep != null)
                            SetStepIcon(typeof(DsfDecision), serviceTestStep);
                    }
                }
            }
        }

        private ServiceTestStep ProcessFlowDecision(ModelItem modelItem)
        {
            if (modelItem != null)
            {
                var condition = modelItem.GetProperty("Condition");
                string expression;
                string uniqueId;
                var activity = (DsfFlowNodeActivity<bool>)condition;
                if (activity != null)
                {
                    uniqueId = activity.UniqueID;
                    expression = activity.ExpressionText;
                }
                else
                {
                    expression = modelItem.GetProperty("ExpressionText") as string;
                    uniqueId = modelItem.GetProperty("UniqueID") as string;
                }
                if (!string.IsNullOrEmpty(expression))
                {
                    var eval = Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(expression);

                    if (!string.IsNullOrEmpty(eval))
                    {
                        Dev2JsonSerializer ser = new Dev2JsonSerializer();
                        var dds = ser.Deserialize<Dev2DecisionStack>(eval);

                        var exists = SelectedServiceTest.TestSteps?.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

                        if (exists == null)
                        {
                            if (SelectedServiceTest != null)
                            {
                                var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                                var serviceTestOutput = new ServiceTestOutput(GlobalConstants.ArmResultText, "", "", "")
                                {
                                    HasOptionsForValue = true,
                                    OptionsForValue = new List<string> { dds.TrueArmText, dds.FalseArmText }
                                };
                                serviceTestOutputs.Add(serviceTestOutput);
                                var serviceTestStep = SelectedServiceTest.AddTestStep(uniqueId, dds.DisplayText, typeof(DsfDecision).Name, serviceTestOutputs) as ServiceTestStep;
                                if (serviceTestStep != null)
                                {
                                    SetStepIcon(typeof(DsfDecision), serviceTestStep);
                                }
                                return serviceTestStep;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void SetServerName(IContextualResourceModel resourceModel)
        {
            if (resourceModel.Environment == null || resourceModel.Environment.IsLocalHost)
            {
                _serverName = string.Empty;
            }
            else if (!resourceModel.Environment.IsLocalHost)
            {
                _serverName = " - " + resourceModel.Environment.Name;
            }
        }

        private void OnReceivedResourceAffectedMessage(Guid resourceId, CompileMessageList changeList)
        {
            if (resourceId == ResourceModel.ID)
            {
                IsLoading = true;
                AsyncWorker.Start(() =>
                {
                    var contextModel = ResourceModel?.Environment?.ResourceRepository?.LoadContextualResourceModel(resourceId);
                    _resourceModel = contextModel;
                    return GetTests();
                }, models =>
                {
                    var dummyTest = new DummyServiceTest(CreateTests) { TestName = "Create a new test." };
                    models.Add(dummyTest);
                    var testName = SelectedServiceTest?.TestName;
                    SelectedServiceTest = dummyTest;
                    Tests = models;
                    SelectedServiceTest = _tests.FirstOrDefault(model => model.TestName == testName);
                    var mainViewModel = CustomContainer.Get<IMainViewModel>();
                    WorkflowDesignerViewModel = mainViewModel?.CreateNewDesigner(ResourceModel);
                    if (WorkflowDesignerViewModel != null)
                    {
                        WorkflowDesignerViewModel.ItemSelectedAction = ItemSelectedAction;
                    }
                    IsLoading = false;
                });

            }
        }

        private bool IsServerConnected()
        {
            var isConnected = ResourceModel.Environment.IsConnected;
            return isConnected;
        }

        private void StopTest()
        {
            SelectedServiceTest.IsTestRunning = false;
            SelectedServiceTest.TestPending = true;
            ServiceTestCommandHandler.StopTest(ResourceModel);
        }

        #region CommandMethods

        private void RunSelectedTestInBrowser()
        {
            var runSelectedTestUrl = GetWebRunURLForTest(SelectedServiceTest);
            ServiceTestCommandHandler.RunSelectedTestInBrowser(runSelectedTestUrl, _processExecutor);
        }

        private void RunSelectedTest()
        {
            if (SelectedServiceTest != null)
            {
                if (SelectedServiceTest.IsDirty)
                {
                    if (ShowPopupWhenDuplicates())
                    {
                        return;
                    }
                    Save(new List<IServiceTestModel> { SelectedServiceTest });
                }
                ServiceTestCommandHandler.RunSelectedTest(SelectedServiceTest, ResourceModel, AsyncWorker);
                ViewModelUtils.RaiseCanExecuteChanged(StopTestCommand);
            }
        }

        private void RunAllTestsInBrowser()
        {
            ServiceTestCommandHandler.RunAllTestsInBrowser(IsDirty, RunAllTestsUrl, _processExecutor);
        }

        private void RunAllTests()
        {
            ServiceTestCommandHandler.RunAllTestsCommand(IsDirty, RealTests().Where(model => model.Enabled), ResourceModel, AsyncWorker);
            SelectedServiceTest = null;
        }

        private void DuplicateTest()
        {
            var testNumber = GetNewTestNumber(SelectedServiceTest.TestName);
            var duplicateTest = ServiceTestCommandHandler.DuplicateTest(SelectedServiceTest, testNumber);
            AddAndSelectTest(duplicateTest);
        }

        #endregion


        private bool CanDeleteTest(IServiceTestModel selectedTestModel)
        {
            return GetPermissions() && selectedTestModel != null && !selectedTestModel.Enabled && IsServerConnected();
        }

        private IAsyncWorker AsyncWorker { get; }
        private IEventAggregator EventPublisher { get; }

        private void CreateTests()
        {
            _canAddFromDebug = true;
            SelectedServiceTest = null;
            if (IsDirty)
            {
                PopupController?.Show(Resources.Languages.Core.ServiceTestSaveEditedTestsMessage, Resources.Languages.Core.ServiceTestSaveEditedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                _canAddFromDebug = false;
                return;
            }

            var testNumber = GetNewTestNumber("Test");
            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel, testNumber);
            AddAndSelectTest(testModel);

        }

        private bool _canAddFromDebug;
        private bool _isLoading;
        private bool _isValid;
        private bool _dirty;
        private IWarewolfWebClient _webClient;

        private int GetNewTestNumber(string testName)
        {
            int counter = 1;
            string fullName = testName + " " + counter;

            while (Contains(fullName))
            {
                counter++;
                fullName = testName + " " + counter;
            }

            return counter;
        }

        private bool Contains(string nameToCheck)
        {
            var serviceTestModel = RealTests().FirstOrDefault(a => a.TestName.Contains(nameToCheck));
            return serviceTestModel != null;
        }

        private void SetDuplicateTestTooltip()
        {
            if (SelectedServiceTest != null)
            {
                if (SelectedServiceTest.NewTest)
                {
                    SelectedServiceTest.DuplicateTestTooltip = Resources.Languages.Tooltips.ServiceTestNewTestDisabledDuplicateSelectedTestTooltip;
                }
                else
                {
                    SelectedServiceTest.DuplicateTestTooltip = CanDuplicateTest ? Resources.Languages.Tooltips.ServiceTestDuplicateSelectedTestTooltip : Resources.Languages.Tooltips.ServiceTestDisabledDuplicateSelectedTestTooltip;
                }
            }
        }

        private void AddAndSelectTest(IServiceTestModel testModel)
        {
            var index = _tests.Count - 1;
            if (index >= 0)
            {
                _tests.Insert(index, testModel);
            }
            else
            {
                _tests.Add(testModel);
            }
            SelectedServiceTest = testModel;
            var isDirty = IsDirty;
            SetDisplayName(isDirty);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private bool CanStopTest => SelectedServiceTest != null && SelectedServiceTest.IsTestRunning;
        private bool CanRunSelectedTestInBrowser => SelectedServiceTest != null && !SelectedServiceTest.IsDirty && IsServerConnected();
        private bool CanRunSelectedTest => GetPermissions() && IsServerConnected();
        private bool CanDuplicateTest => GetPermissions() && SelectedServiceTest != null && !SelectedServiceTest.NewTest;

        public bool CanSave { get; set; }

        private bool GetPermissions()
        {
            return true;
        }

        private bool IsValidName()
        {
            if (SelectedServiceTest != null)
            {
                var name = SelectedServiceTest.TestName;
                ErrorMessage = string.Empty;
                if (string.IsNullOrEmpty(name))
                {
                    ErrorMessage = string.Format(ErrorResource.CannotBeNull, "'name'");
                }
                else if (NameHasInvalidCharacters(name))
                {
                    ErrorMessage = string.Format(ErrorResource.ContainsInvalidCharecters, "'name'");
                }
                else if (name.Trim() != name)
                {
                    ErrorMessage = string.Format(ErrorResource.ContainsLeadingOrTrailingWhitespace, "'name'");
                }

                return string.IsNullOrEmpty(ErrorMessage);
            }
            return true;
        }
        private static bool NameHasInvalidCharacters(string name)
        {
            return Regex.IsMatch(name, @"[^a-zA-Z0-9._\s-]");
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(() => ErrorMessage);
            }
        }

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel
        {
            get { return _workflowDesignerViewModel; }
            set
            {
                _workflowDesignerViewModel = value;
                OnPropertyChanged(() => WorkflowDesignerViewModel);
            }
        }

        public bool IsDirty
        {
            get
            {
                try
                {
                    if (_tests == null || _tests.Count <= 1)
                    {
                        return false;
                    }
                    var isDirty = _tests.Any(resource => resource.IsDirty || resource.NewTest);

                    var isConnected = ResourceModel.Environment.Connection.IsConnected;
                    _dirty = isDirty && isConnected;
                    _isValid = IsValidName();
                    CanSave = _isValid && _dirty;
                    SetDisplayName(_dirty);
                    return _dirty;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public Guid ResourceID => ResourceModel?.ID ?? Guid.Empty;

        public void Save()
        {
            try
            {
                if (ShowPopupWhenDuplicates())
                {
                    return;
                }

                var serviceTestModels = RealTests().Where(a => a.IsDirty).ToList();
                Save(serviceTestModels);
                UpdateTestsFromResourceUpdate();
            }
            catch (Exception)
            {
                // MarkTestsAsDirty(true);
            }
            finally
            {
                var isDirty = IsDirty;
                SetDisplayName(isDirty);
            }
        }

        private void Save(List<IServiceTestModel> serviceTestModels)
        {
            MarkPending(serviceTestModels);
            var serviceTestModelTos = serviceTestModels.Select(CreateServiceTestModelTO).ToList();

            var result = ResourceModel.Environment.ResourceRepository.SaveTests(ResourceModel, serviceTestModelTos);
            switch (result.Result)
            {
                case SaveResult.Success:
                    MarkTestsAsNotNew();
                    SetSelectedTestUrl();
                    break;
                case SaveResult.ResourceDeleted:
                    PopupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                    _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                    break;
                case SaveResult.ResourceUpdated:
                    UpdateTestsFromResourceUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MarkPending(List<IServiceTestModel> serviceTestModels)
        {
            foreach (var serviceTestModel in serviceTestModels)
            {
                serviceTestModel.TestPending = true;
                if (serviceTestModel.TestSteps != null)
                {
                    foreach (var serviceTestStep in serviceTestModel.TestSteps)
                    {
                        MarkChildrenPending(serviceTestStep);
                        if (serviceTestStep.Children == null) continue;
                        var testSteps = serviceTestStep.Children.Flatten(step => step.Children);
                        foreach (var testStep in testSteps)
                        {
                            MarkChildrenPending(testStep);
                        }
                    }
                }

                if (serviceTestModel.Outputs == null) continue;
                foreach (var testOutput in serviceTestModel.Outputs.OfType<ServiceTestOutput>())
                {
                    testOutput.TestPending = true;
                    if (testOutput.Result != null)
                    {
                        testOutput.Result.RunTestResult = RunResult.TestPending;
                    }
                }
            }
        }

        private static void MarkChildrenPending(IServiceTestStep serviceTestStep)
        {
            var step = serviceTestStep as ServiceTestStep;
            if (step != null)
            {
                step.TestPending = true;
                if (step.Result != null)
                {
                    step.Result.RunTestResult = RunResult.TestPending;
                }

                if (step.StepOutputs != null)
                {
                    foreach (var serviceTestOutput in step.StepOutputs)
                    {
                        var stepOutput = serviceTestOutput as ServiceTestOutput;
                        if (stepOutput != null)
                        {
                            stepOutput.TestPending = true;
                            if (stepOutput.Result != null)
                            {
                                stepOutput.Result.RunTestResult = RunResult.TestPending;
                            }
                        }
                    }
                }
            }
        }

        private static IServiceTestModelTO CreateServiceTestModelTO(IServiceTestModel model)
        {
            return new ServiceTestModelTO
            {
                TestName = model.TestName,
                ResourceId = model.ParentId,
                AuthenticationType = model.AuthenticationType,
                Enabled = model.Enabled,
                ErrorExpected = model.ErrorExpected,
                NoErrorExpected = model.NoErrorExpected,
                ErrorContainsText = model.ErrorContainsText,
                TestSteps = model.TestSteps?.Select(step => CreateServiceTestStepTO(step, null)).ToList() ?? new List<IServiceTestStep>(),
                Inputs = model.Inputs?.Select(CreateServiceTestInputsTO).ToList() ?? new List<IServiceTestInput>(),
                Outputs = model.Outputs?.Select(CreateServiceTestOutputTO).ToList() ?? new List<IServiceTestOutput>(),
                LastRunDate = model.LastRunDate,
                OldTestName = model.OldTestName,
                Password = model.Password,
                IsDirty = model.IsDirty,
                TestPending = model.TestPending,
                UserName = model.UserName,
                TestFailing = model.TestFailing,
                TestInvalid = model.TestInvalid,
                TestPassed = model.TestPassed
            };
        }

        private static IServiceTestOutput CreateServiceTestOutputTO(IServiceTestOutput output)
        {
            return new ServiceTestOutputTO
            {
                Variable = output.Variable,
                Value = output.Value,
                From = output.From,
                To = output.To,
                AssertOp = output.AssertOp,
                HasOptionsForValue = output.HasOptionsForValue,
                OptionsForValue = output.OptionsForValue
            };
        }

        private static IServiceTestInput CreateServiceTestInputsTO(IServiceTestInput input)
        {
            return new ServiceTestInputTO
            {
                Variable = input.Variable,
                Value = input.Value,
                EmptyIsNull = input.EmptyIsNull
            };
        }

        private static IServiceTestStep CreateServiceTestStepTO(IServiceTestStep step, IServiceTestStep parent)
        {
            var serviceTestStepTO = new ServiceTestStepTO(step.UniqueId, step.ActivityType, step.StepOutputs.Select(CreateServiceTestStepOutputsTO).ToObservableCollection(), step.Type)
            {
                Children = new ObservableCollection<IServiceTestStep>(),
                Parent = parent,
                StepDescription = step.StepDescription
            };
            if (step.Children != null)
            {
                foreach (var serviceTestStep in step.Children)
                {
                    serviceTestStepTO.Children.Add(CreateServiceTestStepTO(serviceTestStep, serviceTestStepTO));
                }
            }
            return serviceTestStepTO;
        }

        private static IServiceTestOutput CreateServiceTestStepOutputsTO(IServiceTestOutput output)
        {
            return new ServiceTestOutputTO
            {
                Variable = output.Variable,
                Value = output.Value,
                From = output.From,
                To = output.To,
                AssertOp = output.AssertOp,
                HasOptionsForValue = output.HasOptionsForValue,
                OptionsForValue = output.OptionsForValue
            };
        }

        private void UpdateTestsFromResourceUpdate()
        {
            foreach (var serviceTestModel in Tests)
            {
                var runSelectedTestUrl = GetWebRunURLForTest(serviceTestModel);
                serviceTestModel.RunSelectedTestUrl = runSelectedTestUrl;
            }

        }

        private string GetWebRunURLForTest(IServiceTestModel serviceTestModel)
        {
            var runSelectedTestUrl = WebServer.GetWorkflowUri(ResourceModel, "", UrlType.Tests) + "/" + serviceTestModel.TestName;
            if (serviceTestModel.AuthenticationType == AuthenticationType.Public)
            {
                runSelectedTestUrl = runSelectedTestUrl.Replace("/secure/", "/public/");
            }
            return runSelectedTestUrl;
        }

        private bool ShowPopupWhenDuplicates()
        {
            if (HasDuplicates())
            {
                ShowDuplicatePopup();
                return true;
            }
            return false;
        }

        public void ShowDuplicatePopup()
        {
            PopupController?.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
        }

        public void RefreshCommands()
        {
            ViewModelUtils.RaiseCanExecuteChanged(RunAllTestsCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunAllTestsInBrowserCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            _dirty = IsDirty;
            OnPropertyChanged(() => IsDirty);
            OnPropertyChanged(() => DisplayName);
            SetDisplayName(_dirty);
        }

        public bool HasDuplicates() => RealTests().ToList().GroupBy(x => x.TestName).Where(group => @group.Count() > 1).Select(group => @group.Key).Any();

        private void SetSelectedTestUrl()
        {
            var runSelectedTestUrl = GetWebRunURLForTest(SelectedServiceTest);
            SelectedServiceTest.RunSelectedTestUrl = runSelectedTestUrl;
        }

        private void MarkTestsAsNotNew()
        {
            foreach (var model in _tests.Where(model => model.NewTest))
            {
                model.NewTest = false;
            }
            foreach (var model in RealTests())
            {
                var clone = model.Clone();
                model.SetItem(clone);
            }

        }

        public IContextualResourceModel ResourceModel
        {
            get
            {
                return _resourceModel;
            }
            private set
            {
                _resourceModel = value;
            }
        }

        public IServiceTestModel SelectedServiceTest
        {
            get { return _selectedServiceTest; }
            set
            {
                if (value == null)
                {
                    if (_selectedServiceTest != null)
                    {
                        _selectedServiceTest.PropertyChanged -= ActionsForPropChanges;
                    }

                    _selectedServiceTest = null;
                    EventPublisher.Publish(new DebugOutputMessage(new List<IDebugState>()));
                    OnPropertyChanged(() => SelectedServiceTest);
                    return;
                }
                if (Equals(_selectedServiceTest, value) || value.IsNewTest)
                {
                    return;
                }
                if (_selectedServiceTest != null)
                {
                    _selectedServiceTest.PropertyChanged -= ActionsForPropChanges;
                }

                if (_selectedServiceTest?.TestSteps != null)
                {
                    var serviceTestSteps = _selectedServiceTest?.TestSteps.Flatten(step => step.Children ?? new ObservableCollection<IServiceTestStep>());
                    foreach (var serviceTestStep in serviceTestSteps)
                    {
                        if (serviceTestStep?.StepOutputs != null)
                        {
                            foreach (var serviceTestOutput in serviceTestStep.StepOutputs)
                            {
                                ((ServiceTestOutput)serviceTestOutput).PropertyChanged -= OnStepOutputPropertyChanges;
                            }
                        }
                    }
                }
                _selectedServiceTest = value;
                _selectedServiceTest.IsTestLoading = true;
                _selectedServiceTest.PropertyChanged += ActionsForPropChanges;

                if (_selectedServiceTest?.TestSteps != null)
                {
                    var serviceTestSteps = _selectedServiceTest?.TestSteps.Flatten(step => step.Children ?? new ObservableCollection<IServiceTestStep>());
                    foreach (var serviceTestStep in serviceTestSteps)
                    {
                        if (serviceTestStep?.StepOutputs != null)
                        {
                            foreach (var serviceTestOutput in serviceTestStep.StepOutputs)
                            {
                                ((ServiceTestOutput)serviceTestOutput).PropertyChanged += OnStepOutputPropertyChanges;
                            }
                        }
                    }
                }

                SetSelectedTestUrl();
                SetDuplicateTestTooltip();
                OnPropertyChanged(() => SelectedServiceTest);
                EventPublisher.Publish(new DebugOutputMessage(_selectedServiceTest?.DebugForTest ?? new List<IDebugState>()));
                if (_selectedServiceTest != null)
                {
                    _selectedServiceTest.IsTestLoading = false;
                }
            }
        }

        private void OnStepOutputPropertyChanges(object sender, PropertyChangedEventArgs e)
        {
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            _dirty = IsDirty;
            OnPropertyChanged(() => IsDirty);
        }

        private void ActionsForPropChanges(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
            {
                ViewModelUtils.RaiseCanExecuteChanged(DeleteTestCommand);
            }
            if (e.PropertyName == "IsDirty")
            {
                ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
                _dirty = IsDirty;
                OnPropertyChanged(() => IsDirty);
            }
            if (e.PropertyName == "Inputs" || e.PropertyName == "Outputs")
            {
                ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            }
            if (e.PropertyName == "RunSelectedTestUrl")
            {
                ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            }
            if (e.PropertyName == "DebugForTest")
            {
                EventPublisher.Publish(new DebugOutputMessage(SelectedServiceTest?.DebugForTest ?? new List<IDebugState>()));
            }
            if (e.PropertyName == "TestName")
            {
                _dirty = IsDirty;
                OnPropertyChanged(() => IsDirty);
            }
            ViewModelUtils.RaiseCanExecuteChanged(DuplicateTestCommand);
        }

        private void SetDisplayName(bool isDirty)
        {
            if (isDirty)
            {
                if (!DisplayName.EndsWith(" *"))
                {
                    DisplayName += " *";
                }
            }
            else
            {
                DisplayName = _displayName.Replace("*", "").TrimEnd(' ');
            }
        }

        public IServiceTestCommandHandler ServiceTestCommandHandler { get; set; }

        public string RunAllTestsUrl
        {
            get { return _runAllTestsUrl; }
            set
            {
                _runAllTestsUrl = value;
                OnPropertyChanged(() => RunAllTestsUrl);
            }
        }

        public string TestPassingResult
        {
            get { return _testPassingResult; }
            set
            {
                _testPassingResult = value;
                OnPropertyChanged(() => TestPassingResult);
            }
        }

        private IEnumerable<IServiceTestModel> RealTests() => _tests.Where(model => model.GetType() != typeof(DummyServiceTest)).ToObservableCollection();

        public ObservableCollection<IServiceTestModel> Tests
        {
            get { return _tests; }
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
            }
        }

        private void DeleteTest(IServiceTestModel test)
        {
            if (test == null) return;
            var nameOfItemBeingDeleted = test.NameForDisplay.Replace("*", "").TrimEnd(' ');
            if (PopupController.ShowDeleteConfirmation(nameOfItemBeingDeleted) == MessageBoxResult.Yes)
            {
                try
                {
                    if (!test.IsNewTest)
                    {
                        ResourceModel.Environment.ResourceRepository.DeleteResourceTest(ResourceModel.ID, test.TestName);
                    }
                    _tests.Remove(test);
                    OnPropertyChanged(() => Tests);
                    SelectedServiceTest = null;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("IServiceTestModelTO DeleteTest(IServiceTestModel model)", ex);
                }
            }
        }

        private void DeleteTestStep(IServiceTestStep testStep)
        {
            if (testStep == null)
                return;

            DeleteStep(testStep, SelectedServiceTest.TestSteps);
        }

        private void DeleteStep(IServiceTestStep testStep, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (serviceTestSteps.Contains(testStep))
            {
                serviceTestSteps.Remove(testStep);
            }
            else
            {
                var foundParentStep = serviceTestSteps.FirstOrDefault(step => step.UniqueId == testStep.Parent?.UniqueId);
                foundParentStep?.Children?.Remove(testStep);
            }
        }

        private ObservableCollection<IServiceTestModel> GetTests()
        {
            try
            {
                var serviceTestModels = new List<ServiceTestModel>();
                var loadResourceTests = ResourceModel.Environment.ResourceRepository.LoadResourceTests(ResourceModel.ID);
                if (loadResourceTests != null)
                {
                    foreach (var test in loadResourceTests)
                    {
                        var serviceTestModel = ToServiceTestModel(test);
                        serviceTestModel.Item = (ServiceTestModel)serviceTestModel.Clone();
                        serviceTestModels.Add(serviceTestModel);
                    }
                }
                return serviceTestModels.ToObservableCollection<IServiceTestModel>();
            }
            catch (Exception)
            {
                return new ObservableCollection<IServiceTestModel>();
            }
        }

        private ServiceTestModel ToServiceTestModel(IServiceTestModelTO to)
        {
            var serviceTestModel = new ServiceTestModel(ResourceModel.ID)
            {
                OldTestName = to.TestName,
                TestName = to.TestName,
                IsTestRunning = false,
                NameForDisplay = to.TestName,
                UserName = to.UserName,
                AuthenticationType = to.AuthenticationType,
                Enabled = to.Enabled,
                ErrorExpected = to.ErrorExpected,
                NoErrorExpected = to.NoErrorExpected,
                ErrorContainsText = to.ErrorContainsText,
                LastRunDate = to.LastRunDate,
                TestPending = to.TestPending,
                TestFailing = to.TestFailing,
                TestPassed = to.TestPassed,
                Password = to.Password,
                ParentId = to.ResourceId,
                TestInvalid = to.TestInvalid,
                TestSteps = to.TestSteps?.Select(step => CreateServiceTestStep(step) as IServiceTestStep).ToObservableCollection(),
                Inputs = to.Inputs?.Select(CreateInput).ToObservableCollection(),
                Outputs = to.Outputs?.Select(CreateOutput).ToObservableCollection()
            };
            return serviceTestModel;
        }

        private IServiceTestOutput CreateOutput(IServiceTestOutput output)
        {
            var serviceTestOutput = new ServiceTestOutput(output.Variable, output.Value, output.From, output.To) as IServiceTestOutput;
            serviceTestOutput.AssertOp = output.AssertOp;
            serviceTestOutput.Result = output.Result;
            return serviceTestOutput;
        }

        private IServiceTestInput CreateInput(IServiceTestInput input)
        {
            var serviceTestInput = new ServiceTestInput(input.Variable, input.Value) as IServiceTestInput;
            serviceTestInput.EmptyIsNull = input.EmptyIsNull;
            return serviceTestInput;
        }

        private ServiceTestStep CreateServiceTestStep(IServiceTestStep step)
        {
            var testStep = new ServiceTestStep(step.UniqueId, step.ActivityType, new ObservableCollection<IServiceTestOutput>(), step.Type)
            {
                Children = new ObservableCollection<IServiceTestStep>(),
                Parent = step.Parent,
                StepDescription = step.StepDescription,
                Result = step.Result
            };
            testStep.StepOutputs = CreateServiceTestOutputFromStep(step.StepOutputs, testStep);
            if (testStep.MockSelected)
            {
                testStep.TestPending = false;
                testStep.TestPassed = false;
                testStep.TestFailing = false;
                testStep.TestInvalid = false;
            }
            SetStepIcon(testStep.ActivityType, testStep);

            if (step.Children != null)
            {
                foreach (var serviceTestStep in step.Children)
                {
                    testStep.Children.Add(CreateServiceTestStep(serviceTestStep));
                }
            }
            return testStep;
        }

        private ObservableCollection<IServiceTestOutput> CreateServiceTestOutputFromStep(ObservableCollection<IServiceTestOutput> stepStepOutputs, ServiceTestStep testStep)
        {
            var stepOutputs = new ObservableCollection<IServiceTestOutput>();
            foreach (var serviceTestOutput in stepStepOutputs)
            {
                var testOutput = new ServiceTestOutput(serviceTestOutput.Variable, serviceTestOutput.Value, serviceTestOutput.From, serviceTestOutput.To)
                {
                    AddStepOutputRow = testStep.AddNewOutput,
                    AssertOp = serviceTestOutput.AssertOp,
                    HasOptionsForValue = serviceTestOutput.HasOptionsForValue,
                    OptionsForValue = serviceTestOutput.OptionsForValue,
                    Result = serviceTestOutput.Result
                };
                if (testStep.MockSelected)
                {
                    testOutput.TestPending = false;
                    testOutput.TestPassed = false;
                    testOutput.TestFailing = false;
                    testOutput.TestInvalid = false;
                }

                stepOutputs.Add(testOutput);
            }
            return stepOutputs;
        }

        public ICommand DeleteTestCommand { get; set; }
        public ICommand DeleteTestStepCommand { get; set; }
        public ICommand DuplicateTestCommand { get; set; }
        public ICommand RunAllTestsInBrowserCommand { get; set; }
        public ICommand RunAllTestsCommand { get; set; }
        public ICommand RunSelectedTestInBrowserCommand { get; set; }
        public ICommand RunSelectedTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }
        public ICommand CreateTestCommand { get; set; }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

        public void Dispose()
        {
            // ReSharper disable DelegateSubtraction
            if (ResourceModel?.Environment?.Connection != null)
                ResourceModel.Environment.Connection.ReceivedResourceAffectedMessage -= OnReceivedResourceAffectedMessage;
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
