#pragma warning disable
﻿using System;
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
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestViewModel : BindableBase, IServiceTestViewModel
    {
        readonly IExternalProcessExecutor _processExecutor;
        IServiceTestModel _selectedServiceTest;
        string _runAllTestsUrl;
        string _runAllCoverageUrl;
        string _testPassingResult;
        ObservableCollection<IServiceTestModel> _tests;
        string _displayName;
        public IPopupController PopupController { get; }
        string _errorMessage;
        readonly IShellViewModel _shellViewModel;
        IContextualResourceModel _resourceModel;
        string _serverName;
        IWorkflowDesignerViewModel _workflowDesignerViewModel;

        static readonly IEnumerable<Type> Types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());

        public ServiceTestViewModel(IContextualResourceModel resourceModel, IAsyncWorker asyncWorker, IEventAggregator eventPublisher, IExternalProcessExecutor processExecutor, IWorkflowDesignerViewModel workflowDesignerViewModel, IPopupController popupController)
            : this(resourceModel, asyncWorker, eventPublisher, processExecutor, workflowDesignerViewModel, null, popupController)
        {
        }

        public ServiceTestViewModel(IContextualResourceModel resourceModel, IAsyncWorker asyncWorker, IEventAggregator eventPublisher, IExternalProcessExecutor processExecutor, IWorkflowDesignerViewModel workflowDesignerViewModel, IMessage msg, IPopupController popupController)
        {
            _processExecutor = processExecutor;
            AsyncWorker = asyncWorker;
            EventPublisher = eventPublisher;
            ResourceModel = resourceModel ?? throw new ArgumentNullException(nameof(resourceModel));
            ResourceModel.Environment.IsConnectedChanged += (sender, args) =>
            {
                ViewModelUtils.RaiseCanExecuteChanged(DeleteTestCommand);
                RefreshCommands();
            };

            ResourceModel.Environment.Connection.ReceivedResourceAffectedMessage += OnReceivedResourceAffectedMessage;
            SetServerName(resourceModel);
            DisplayName = resourceModel.DisplayName + " - Tests" + _serverName;

            ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();
            PopupController = popupController;
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
            RunAllTestsInBrowserCommand = new DelegateCommand(RunAllTestsInBrowser, IsServerConnected);
            RunAllTestCoverageInBrowserCommand = new DelegateCommand(RunAllCoverageInBrowser, IsServerConnected);
            RunAllTestsCommand = new DelegateCommand(RunAllTests, IsServerConnected);
            RunSelectedTestInBrowserCommand = new DelegateCommand(RunSelectedTestInBrowser, () => CanRunSelectedTestInBrowser);
            RunSelectedTestCommand = new DelegateCommand(RunSelectedTest, () => CanRunSelectedTest);
            StopTestCommand = new DelegateCommand(StopTest, () => CanStopTest);
            CreateTestCommand = new DelegateCommand(() => CreateTests());
            DeleteTestCommand = new DelegateCommand<IServiceTestModel>(DeleteTest, CanDeleteTest);
            DeleteTestStepCommand = new DelegateCommand<IServiceTestStep>(DeleteTestStep);
            DuplicateTestCommand = new DelegateCommand(DuplicateTest, () => CanDuplicateTest);
            RunAllTestsUrl = resourceModel.GetWorkflowUri("", UrlType.Tests)?.ToString();
            RunAllCoverageUrl = resourceModel.GetWorkflowUri("", UrlType.Coverage)?.ToString();

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
                    if (msg is NewTestFromDebugMessage test)
                    {
                        var newTest = test;
                        if (newTest.RootItems == null)
                        {
                            throw new ArgumentNullException(nameof(newTest.RootItems));
                        }
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
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(() => IsLoading);
            }
        }

        public void PrepopulateTestsUsingDebug(List<IDebugTreeViewItemViewModel> models)
        {
            CreateTests(true);
            if (_canAddFromDebug)
            {
                WorkflowDesignerViewModel?.UpdateWorkflowInputDataViewModel(ResourceModel);
                AddFromDebug(models);
            }
        }

        void AddFromDebug(List<IDebugTreeViewItemViewModel> models)
        {
            foreach (IDebugTreeViewItemViewModel debugState in models)
            {
                if (debugState is DebugStateTreeViewItemViewModel debugItem && debugItem.Parent == null)
                {
                    if (debugItem.Content == null)
                    {
                        continue;
                    }
                    ValidateAddStepType(debugState, debugItem.Content);
                }
            }
        }

        void ValidateAddStepType(IDebugTreeViewItemViewModel debugState, IDebugState debugItemContent)
        {
            if (debugItemContent.ActivityType == ActivityType.Workflow && debugItemContent.OriginatingResourceID == ResourceModel.ID)
            {
                UpdateInputValues(debugItemContent);
                ProcessInputsAndOutputs(debugItemContent);
            }
            else if (debugItemContent.ActivityType == ActivityType.Workflow && debugItemContent.ActualType == typeof(DsfActivity).Name)
            {
                AddStepFromDebug(debugState, debugItemContent);
            }
            else
            {
                if (debugItemContent.ActivityType != ActivityType.Workflow && debugItemContent.ActualType != typeof(DsfCommentActivity).Name)
                {
                    ProcessRegularDebugItem(debugItemContent, debugState);
                }
            }
        }

        void UpdateInputValues(IDebugState debugItemContent)
        {
            foreach (var item in debugItemContent.Inputs)
            {
                foreach (var res in item?.ResultsList)
                {
                    var variable = res?.Variable?.Replace("[[", "");
                    variable = variable?.Replace("]]", "");
                    var inputsValue = WorkflowDesignerViewModel?.GetWorkflowInputs(variable);
                    if (res != null)
                    {
                        res.Value = inputsValue;
                    }
                }
            }
        }

        void ProcessRegularDebugItem(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState)
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

        void EnhancedDotNetDllFromDebug(IDebugTreeViewItemViewModel debugState, IDebugState debugItemContent)
        {
            var exists = FindExistingStep(debugItemContent.ID.ToString());
            IServiceTestStep serviceTestStep = null;
            if (exists == null)
            {
                serviceTestStep = SelectedServiceTest.AddDebugItemTestStep(debugItemContent, new ObservableCollection<IServiceTestOutput>());

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

        void SwitchFromDebug(IDebugTreeViewItemViewModel itemContent, IDebugState debugItemContent)
        {
            var processFlowSwitch = ProcessFlowSwitch(WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ParentID.GetValueOrDefault()));
            if (processFlowSwitch != null)
            {
                if (debugItemContent?.Outputs.Count > 0 && debugItemContent.Outputs[0].ResultsList?.Count > 0)
                {
                    processFlowSwitch.StepOutputs[0].Value = debugItemContent.Outputs[0].ResultsList[0].Value;
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

        void DecisionFromDebug(IDebugTreeViewItemViewModel itemContent, IDebugState debugItemContent)
        {
            var processFlowDecision = ProcessFlowDecision(WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ParentID.GetValueOrDefault()));
            if (processFlowDecision != null)
            {
                if (debugItemContent?.Outputs.Count > 0 && debugItemContent.Outputs[0].ResultsList?.Count > 0)
                {
                    processFlowDecision.StepOutputs[0].Value = debugItemContent.Outputs[0].ResultsList[0].Value;
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

        void ProcessInputsAndOutputs(IDebugState debugItemContent)
        {
            if (debugItemContent.StateType == StateType.Start)
            {
                SetInputs(debugItemContent);
            }
            else
            {
                if (debugItemContent.StateType == StateType.End)
                {
                    SetOutputs(debugItemContent);
                }
            }
        }

        void AddStepFromDebug(IDebugTreeViewItemViewModel debugState, IDebugState debugItemContent)
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
                    var serviceTestStep = SelectedServiceTest.AddDebugItemTestStep(debugItemContent, new ObservableCollection<IServiceTestOutput>());
                    var hasOutputs = outputs?.Select(item => item.ResultsList).All(list => list.Count > 0);
                    var debugStateActivityTypeName = debugState.ActivityTypeName;

                    if (outputs?.Count > 0 && hasOutputs.HasValue && hasOutputs.Value)
                    {
                        AddOutputs(outputs, serviceTestStep);
                    }
                    else
                    {
                        SetStepOutputs(serviceTestStep, debugStateActivityTypeName);
                    }
                    if (serviceTestStep != null)
                    {
                        SetMockTestStep(debugItemContent, serviceTestStep, debugStateActivityTypeName);
                        SetStepIcon(serviceTestStep.ActivityType, serviceTestStep);
                    }
                }
            }
        }

        void SetMockTestStep(IDebugState debugItemContent, IServiceTestStep serviceTestStep, string debugStateActivityTypeName)
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
        }

        static void SetStepOutputs(IServiceTestStep serviceTestStep, string debugStateActivityTypeName)
        {
            var type = Types.FirstOrDefault(x => x.Name == debugStateActivityTypeName);
            if (type != null)
            {
                var act = Activator.CreateInstance(type) as IDev2Activity;
                if (serviceTestStep != null)
                {
                    serviceTestStep.StepOutputs = AddOutputs(act?.GetOutputs(), serviceTestStep).ToObservableCollection();
                }
            }
        }

        void AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            if (NullParent(debugItemContent, ref parent))
            {
                return;
            }

            if (parent.ActivityType == typeof(DsfForEachActivity).Name)
            {
                ForEachParent(debugItemContent, debugState, parent);
            }
            if (parent.ActivityType == typeof(GateActivity).Name)
            {
                GateParent(debugItemContent, debugState, parent);
            }
            else if (parent.ActivityType == typeof(DsfSequenceActivity).Name)
            {
                var model = WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ID);
                if (model?.GetCurrentValue() is DsfSequenceActivity sequence)
                {
                    parent.ActivityID = Guid.Parse(sequence.UniqueID);
                    AddChildren(debugState, parent);
                }
            }
            else
            {
                AddNotContainerActivityType(debugState, parent);
            }
            while (parent != null)
            {
                var child = parent;
                if (child.Parent == null)
                {
                    var exists = FindExistingStep(child.ActivityID.ToString());
                    if (exists == null)
                    {
                        SelectedServiceTest.TestSteps.Add(child);
                    }
                }
                parent = child.Parent;
            }
        }

        void AddNotContainerActivityType(IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            if (parent.ActivityType == typeof(DsfActivity).Name)
            {
                if (debugState is DebugStateTreeViewItemViewModel childItem)
                {
                    var content = childItem.Content;
                    var outputs = content.Outputs;
                    AddOutputs(outputs, parent);
                    SetStepIcon(parent.ActivityType, parent);
                }
            }
            else
            {
                AddChildren(debugState, parent);
            }
        }

        void GateParent(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            var model = WorkflowDesignerViewModel?.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ID);
            if (model?.GetCurrentValue() is GateActivity gateActivity && debugState.Children.LastOrDefault() is DebugStateTreeViewItemViewModel childItem)
            {
                var act = gateActivity.DataFunc.Handler as IDev2Activity;
                if (act != null)
                {
                    var guid = Guid.Parse(act.UniqueID);
                    childItem.Content.ID = guid;
                }

                var childItemContent = childItem.Content;
                var outputs = childItemContent.Outputs;

                var exists = parent.Children.FirstOrDefault(a => a.ActivityID == childItemContent.ID);
                if (exists == null)
                {
                    AddGateChildStep(parent, childItem, act, childItemContent, outputs);
                }
            }
        }

        void AddGateChildStep(IServiceTestStep parent, DebugStateTreeViewItemViewModel childItem, IDev2Activity act, IDebugState childItemContent, List<IDebugItem> outputs)
        {
            var childStep = CreateAssertChildStep(parent, childItemContent, childItemContent.ID);
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
            if (childItem.Children?.Count > 0)
            {
                AddChildDebugItems(childItemContent, childItem, childStep);
            }
        }

        void ForEachParent(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            var model = WorkflowDesignerViewModel?.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ID);
            if (model?.GetCurrentValue() is DsfForEachActivity forEach && debugState.Children.LastOrDefault() is DebugStateTreeViewItemViewModel childItem)
            {
                var act = forEach.DataFunc.Handler as IDev2Activity;
                if (act != null)
                {
                    var guid = Guid.Parse(act.UniqueID);
                    childItem.Content.ID = guid;
                }

                var childItemContent = childItem.Content;
                var outputs = childItemContent.Outputs;

                var exists = parent.Children.FirstOrDefault(a => a.ActivityID == childItemContent.ID);
                if (exists == null)
                {
                    AddForEachChildStep(parent, childItem, act, childItemContent, outputs);
                }
            }
        }

        void AddForEachChildStep(IServiceTestStep parent, DebugStateTreeViewItemViewModel childItem, IDev2Activity act, IDebugState childItemContent, List<IDebugItem> outputs)
        {
            var childStep = CreateAssertChildStep(parent, childItemContent, childItemContent.ID);
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
            if (childItem.Children?.Count > 0)
            {
                AddChildDebugItems(childItemContent, childItem, childStep);
            }
        }

        static IServiceTestStep CreateAssertChildStep(IServiceTestStep parent, IDebugState childItemContent, Guid childItemContentId)
        {
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var childStep = new ServiceTestStep(childItemContentId, childItemContent.ActualType, serviceTestOutputs, StepType.Assert)
            {
                StepDescription = childItemContent.DisplayName,
                Parent = parent,
                Type = StepType.Assert
            };
            return childStep;
        }

        static bool NullParent(IDebugState debugItemContent, ref IServiceTestStep parent)
        {
            if (parent == null)
            {
                var testStep = new ServiceTestStep(debugItemContent.ID, "", new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
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
                    testStep.ActivityID = debugItemContent.WorkSurfaceMappingId;
                    parent = testStep;
                }

                else if (actualType == forEachTypeName)
                {
                    SetStepIcon(typeof(DsfForEachActivity), testStep);
                    testStep.ActivityType = forEachTypeName;
                    testStep.ActivityID = debugItemContent.WorkSurfaceMappingId;
                    parent = testStep;
                }
                else if (actualType == selectApplyTypeName)
                {
                    SetStepIcon(typeof(DsfSelectAndApplyActivity), testStep);
                    testStep.ActivityType = selectApplyTypeName;
                    testStep.ActivityID = debugItemContent.WorkSurfaceMappingId;
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

        void AddChildren(IDebugTreeViewItemViewModel debugState, IServiceTestStep parent)
        {
            foreach (var debugTreeViewItemViewModel in debugState.Children)
            {
                if (debugTreeViewItemViewModel is DebugStateTreeViewItemViewModel childItem && childItem.ActivityTypeName != "DsfSelectAndApplyActivity")
                {
                    var childItemContent = childItem.Content;
                    var outputs = childItemContent.Outputs;

                    var contentId = childItemContent.ID;
                    if (childItemContent.ActualType == "DsfActivity")
                    {
                        contentId = childItemContent.WorkSurfaceMappingId;
                    }

                    var exists = parent.Children.FirstOrDefault(a => a.ActivityID == contentId);
                    if (exists == null)
                    {
                        AddNewDebugStateChild(parent, childItem, childItemContent, outputs, contentId);
                    }
                    else
                    {
                        AddExistingDebugState(childItem, outputs, exists);
                    }
                }
            }
        }

        void AddExistingDebugState(DebugStateTreeViewItemViewModel childItem, List<IDebugItem> outputs, IServiceTestStep exists)
        {
            if (exists is ServiceTestStep serviceTestStep)
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

        void AddNewDebugStateChild(IServiceTestStep parent, DebugStateTreeViewItemViewModel childItem, IDebugState childItemContent, List<IDebugItem> outputs, Guid contentId)
        {
            var childStep = CreateAssertChildStep(parent, childItemContent, contentId);
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
            if (childStep.StepOutputs?.Count > 0)
            {
                parent.Children.Add(childStep);
                if (childItem.Children?.Count > 0)
                {
                    AddChildDebugItems(childItemContent, childItem, childStep);
                }
            }
        }

        void AddOutputs(List<IDebugItem> outputs, IServiceTestStep serviceTestStep)
        {
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            if (outputs == null || outputs.Count < 1)
            {
                serviceTestOutputs.Add(new ServiceTestOutput("", "", "", "")
                {
                    AssertOp = "",
                    AddStepOutputRow = s => { serviceTestStep?.AddNewOutput(s); }
                });
                serviceTestStep.StepOutputs = serviceTestOutputs;
                return;
            }
            foreach (var output in outputs)
            {
                AddOutput(output, serviceTestStep, serviceTestOutputs);
            }
            serviceTestStep.StepOutputs = serviceTestOutputs;
        }

        void AddOutput(IDebugItem output, IServiceTestStep serviceTestStep, ObservableCollection<IServiceTestOutput> serviceTestOutputs)
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
                    AddStepOutputRow = s => { serviceTestStep?.AddNewOutput(s); }
                };
                serviceTestOutputs.Add(serviceTestOutput);
            }
        }

        void SetInputs(IDebugState inputState)
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
            private get => _webClient ?? CustomContainer.Get<IWarewolfWebClient>();
            set => _webClient = value;
        }

        void SetOutputs(IDebugState outPutState)
        {
            var dataList = new DataListModel();
            dataList.Create(ResourceModel.DataList, ResourceModel.DataList);
            if (outPutState == null)
            {
                return;
            }
            var outPuts = new ObservableCollection<IServiceTestOutput>();
            foreach (var debugItem in outPutState.Outputs)
            {
                foreach (var debugItemResult in debugItem.ResultsList)
                {
                    SetResultListOutputs(outPutState, dataList, outPuts, debugItemResult);
                }
            }
            SelectedServiceTest.Outputs = outPuts;
            SelectedServiceTest.ErrorExpected = outPutState.HasError;
            SelectedServiceTest.NoErrorExpected = !outPutState.HasError;
            SelectedServiceTest.ErrorContainsText = outPutState.ErrorMessage;
        }

        void SetResultListOutputs(IDebugState outPutState, DataListModel dataList, ObservableCollection<IServiceTestOutput> outPuts, IDebugItemResult debugItemResult)
        {
            var variable = debugItemResult.Variable.Replace("[[", "").Replace("]]", "");
            var value = debugItemResult.Value;
            var serviceTestOutput = new ServiceTestOutput(variable, value, "", "");
            var output = serviceTestOutput;
            serviceTestOutput.AddNewAction = () => ((ServiceTestModel)SelectedServiceTest).AddRow(output, dataList);

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
            outPuts.Add(serviceTestOutput);
        }

        static void OnError(Exception exception)
        {
            Dev2Logger.Error(exception, GlobalConstants.WarewolfError);
            throw exception;
        }

        void ItemSelectedAction(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return;
            }

            var itemType = GetInnerItemType(modelItem);
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
            else if (itemType == typeof(GateActivity))
            {
                ProcessGate(modelItem);
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

        static Type GetInnerItemType(ModelItem modelItem)
        {
            var itemType = modelItem.ItemType;
            if (modelItem.Content?.Value != null && itemType == typeof(FlowStep))
            {
                itemType = modelItem.Content.Value.ItemType;
            }
            return itemType;
        }

        void ProcessSequence(ModelItem modelItem)
        {
            var sequence = GetCurrentActivity<DsfSequenceActivity>(modelItem);
            var testStep = BuildParentsFromModelItem(modelItem);
            if (testStep != null)
            {
                AddSequence(sequence, testStep, testStep.Children);
                if (FindExistingStep(testStep.ActivityID.ToString()) == null)
                {
                    SelectedServiceTest.TestSteps.Add(testStep);
                }
            }
            else
            {
                AddSequence(sequence, null, SelectedServiceTest.TestSteps);
            }
        }

        void ProcessGate(ModelItem modelItem)
        {
            var gateActivity = GetCurrentActivity<GateActivity>(modelItem);
            AddGate(gateActivity, null, SelectedServiceTest.TestSteps);
        }

        void ProcessEnhanchedDotNetDll(ModelItem modelItem)
        {
            var dotNetDllActivity = GetCurrentActivity<DsfEnhancedDotNetDllActivity>(modelItem);
            var buildParentsFromModelItem = BuildParentsFromModelItem(modelItem);
            if (buildParentsFromModelItem != null)
            {
                AddEnhancedDotNetDll(dotNetDllActivity, buildParentsFromModelItem, buildParentsFromModelItem.Children);
                if (FindExistingStep(buildParentsFromModelItem.ActivityID.ToString()) == null)
                {
                    SelectedServiceTest.TestSteps.Add(buildParentsFromModelItem);
                }
            }
            else
            {
                AddEnhancedDotNetDll(dotNetDllActivity, null, SelectedServiceTest.TestSteps);
            }
        }

        static T GetCurrentActivity<T>(ModelItem modelItem) where T : class
        {
            var activity = modelItem.GetCurrentValue() as T;
            if (activity == null && modelItem.Content?.Value != null)
            {
                activity = modelItem.Content.Value.GetCurrentValue() as T;
            }
            return activity;
        }

        void ProcessForEach(ModelItem modelItem)
        {
            var forEachActivity = GetCurrentActivity<DsfForEachActivity>(modelItem);
            AddForEach(forEachActivity, null, SelectedServiceTest.TestSteps);
        }

        void AddForEach(DsfForEachActivity forEachActivity, IServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (forEachActivity == null)
            {
                return;
            }
            var uniqueId = forEachActivity.UniqueID;
            var exists = serviceTestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

            var type = typeof(DsfForEachActivity);
            var testStep = CreateMockChildStep(Guid.Parse(uniqueId), parent, type.Name, forEachActivity.DisplayName);
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
                else
                {
                    if (activity.GetType() == type)
                    {
                        AddForEach(activity as DsfForEachActivity, testStep, testStep.Children);
                    }
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

        static IServiceTestStep CreateMockChildStep(Guid uniqueId, IServiceTestStep parent, string typeName, string displayName) => new ServiceTestStep(uniqueId, typeName, new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
        {
            StepDescription = displayName,
            Parent = parent
        };

        void ProcessSelectAndApply(ModelItem modelItem)
        {
            var selectAndApplyActivity = GetCurrentActivity<DsfSelectAndApplyActivity>(modelItem);
            AddSelectAndApply(selectAndApplyActivity, null, SelectedServiceTest.TestSteps);
        }

        void AddSelectAndApply(DsfSelectAndApplyActivity selectApplyActivity, IServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (selectApplyActivity == null)
            {
                return;
            }
            var uniqueId = selectApplyActivity.UniqueID;
            var exists = serviceTestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

            var type = typeof(DsfSelectAndApplyActivity);
            var testStep = CreateMockChildStep(Guid.Parse(uniqueId), parent, type.Name, selectApplyActivity.DisplayName);
            SetStepIcon(type, testStep);
            var activity = selectApplyActivity.ApplyActivityFunc.Handler;
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
                else
                {
                    if (activity != null && activity.GetType() == typeof(DsfSelectAndApplyActivity))
                    {
                        AddSelectAndApply(activity as DsfSelectAndApplyActivity, testStep, testStep.Children);
                    }
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

        void AddSequence(DsfSequenceActivity sequence, IServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (sequence is null)
            {
                return;
            }
            var uniqueId = sequence.UniqueID;
            var exists = serviceTestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

            var type = typeof(DsfSequenceActivity);
            var testStep = CreateMockChildStep(Guid.Parse(uniqueId), parent, type.Name, sequence.DisplayName);
            SetStepIcon(type, testStep);
            foreach (var activity in sequence.Activities)
            {
                AddSequenceActivity(testStep, activity);
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

        void AddSequenceActivity(IServiceTestStep testStep, Activity activity)
        {
            if (activity is DsfNativeActivity<string> act)
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
                if (activity is DsfNativeActivity<bool> act2)
                {
                    AddChildActivity(act2, testStep);
                }
                if (activity.GetType() == typeof(DsfForEachActivity))
                {
                    AddForEach(activity as DsfForEachActivity, testStep, testStep.Children);
                }
                else
                {
                    if (activity.GetType() == typeof(DsfSelectAndApplyActivity))
                    {
                        AddSelectAndApply(activity as DsfSelectAndApplyActivity, testStep, testStep.Children);
                    }
                }
            }
        }

        void AddGate(GateActivity gateActivity, IServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (gateActivity == null)
            {
                return;
            }
            var uniqueId = gateActivity.UniqueID;
            var exists = serviceTestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

            var type = typeof(GateActivity);
            var testStep = CreateMockChildStep(Guid.Parse(uniqueId), parent, type.Name, gateActivity.DisplayName);
            SetStepIcon(type, testStep);
            var activity = gateActivity.DataFunc.Handler;
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
                if (activity?.GetType() == typeof(DsfSelectAndApplyActivity))
                {
                    AddSelectAndApply(activity as DsfSelectAndApplyActivity, testStep, testStep.Children);
                }
                else
                {
                    if (activity?.GetType() == type)
                    {
                        AddGate(activity as GateActivity, testStep, testStep.Children);
                    }
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

        static void AddEnhancedDotNetDll(DsfEnhancedDotNetDllActivity dotNetDllActivity, IServiceTestStep parent, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (dotNetDllActivity == null)
            {
                return;
            }
            var uniqueId = dotNetDllActivity.UniqueID;
            var exists = serviceTestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

            var type = typeof(DsfEnhancedDotNetDllActivity);
            var testStep = CreateMockChildStep(Guid.Parse(uniqueId), parent, type.Name, dotNetDllActivity.DisplayName);
            SetStepIcon(type, testStep);
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
                var constructorStepExists = exists.Children.FirstOrDefault(step => step.ActivityID == dotNetDllActivity.Constructor.ID);
                if (constructorStepExists == null)
                {
                    AddEnhancedDotNetDllConstructor(dotNetDllActivity, exists);
                }
                foreach (var pluginAction in dotNetDllActivity.MethodsToRun)
                {
                    AddEnhancedDotNetDllMethodChild(pluginAction, exists);
                }
            }
        }

        static void AddEnhancedDotNetDllMethodChild(IPluginAction pluginAction, IServiceTestStep exists)
        {
            if (!pluginAction.IsVoid)
            {
                var actionExists = exists.Children.FirstOrDefault(step => step.ActivityID == pluginAction.ID);
                if (actionExists != null)
                {
                    AddEnhancedDotNetDllMethod(pluginAction, exists);
                }
            }
        }

        static void AddMissingChild(ObservableCollection<IServiceTestStep> serviceTestSteps, IServiceTestStep testStep)
        {
            if (serviceTestSteps.Count < 1)
            {
                return;
            }
            foreach (var serviceTestStep in serviceTestSteps)
            {
                if (serviceTestStep.ActivityID != testStep.ActivityID)
                {
                    continue;
                }
                AddMissingChild(serviceTestStep, testStep);
            }
        }

        static void AddMissingChild(IServiceTestStep serviceTestStep, IServiceTestStep testStep)
        {
            if (serviceTestStep.Children.Count == testStep.Children.Count)
            {
                foreach (var child in testStep.Children)
                {
                    AddMissingChild(serviceTestStep.Children, child);
                }
            }
            else
            {
                foreach (var child in testStep.Children)
                {
                    AddMissingChild(serviceTestStep, testStep, child);
                }
            }
        }

        static void AddMissingChild(IServiceTestStep serviceTestStep, IServiceTestStep testStep, IServiceTestStep child)
        {
            var testSteps = serviceTestStep.Children.Where(a => a.ActivityID == child.ActivityID);
            if (!testSteps.Any())
            {
                var indexOf = testStep.Children.IndexOf(child);
                child.Parent = serviceTestStep;
                serviceTestStep.Children.Insert(indexOf, child);
            }
        }

        static void AddChildActivity<T>(DsfNativeActivity<T> act, IServiceTestStep testStep)
        {
            var outputs = act.GetOutputs();
            if (outputs != null && outputs.Count > 0)
            {
                var serviceTestStep = CreateMockChildStep(Guid.Parse(act.UniqueID), testStep, act.GetType().Name, act.DisplayName);
                var serviceTestOutputs = outputs.Select(output => new ServiceTestOutput(output, "", "", "")
                {
                    HasOptionsForValue = false,
                    AddStepOutputRow = serviceTestStep.AddNewOutput
                }).Cast<IServiceTestOutput>().ToObservableCollection();
                serviceTestStep.StepOutputs = serviceTestOutputs;
                SetStepIcon(act.GetType(), serviceTestStep);
                testStep.Children.Add(serviceTestStep);
            }
        }

        static void AddEnhancedDotNetDllConstructor(DsfEnhancedDotNetDllActivity dotNetConstructor, IServiceTestStep testStep)
        {
            var serviceTestStep = CreateMockChildStep(dotNetConstructor.Constructor.ID, testStep, testStep.ActivityType, dotNetConstructor.Constructor.ConstructorName);
            var serviceOutputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput(dotNetConstructor.ObjectName ?? "", "", "", "")
            };
            serviceTestStep.StepOutputs = serviceOutputs;
            SetStepIcon(testStep.ActivityType, serviceTestStep);
            testStep.Children.Insert(0, serviceTestStep);
        }

        static void AddEnhancedDotNetDllMethod(IPluginAction pluginAction, IServiceTestStep testStep)
        {
            var serviceTestStep = CreateMockChildStep(pluginAction.ID, testStep, testStep.ActivityType, pluginAction.Method);
            var serviceOutputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput(pluginAction.OutputVariable ?? "", "", "", "")
            };
            serviceTestStep.StepOutputs = serviceOutputs;
            SetStepIcon(testStep.ActivityType, serviceTestStep);
            testStep.Children.Add(serviceTestStep);
        }

        void ProcessSwitch(ModelItem modelItem)
        {
            var cases = modelItem.GetProperty("Switches") as Dictionary<string, IDev2Activity>;
            var defaultCase = modelItem.GetProperty("Default") as List<IDev2Activity>;
            var uniqueId = modelItem.GetProperty("UniqueID").ToString();
            var exists = SelectedServiceTest.TestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

            if (exists == null && SelectedServiceTest != null)
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
                if (SelectedServiceTest.AddTestStep(uniqueId, modelItem.GetProperty("DisplayName").ToString(), typeof(DsfSwitch).Name, serviceTestOutputs) is ServiceTestStep serviceTestStep)
                {
                    SetStepIcon(typeof(DsfSwitch), serviceTestStep);
                }
            }
        }

        IServiceTestStep ProcessFlowSwitch(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return null;
            }
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
                var uniqueId = activity.UniqueID;
                var exists = SelectedServiceTest.TestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

                if (exists == null && SelectedServiceTest != null)
                {
                    return CreateFlowSwitchTestStep(flowSwitch, uniqueId);
                }
            }
            return null;
        }

        IServiceTestStep CreateFlowSwitchTestStep(FlowSwitch<string> flowSwitch, string uniqueId)
        {
            var switchOptions = flowSwitch.Cases?.Select(pair => pair.Key).ToList();
            if (flowSwitch.Default != null)
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
            var serviceTestStep = SelectedServiceTest.AddTestStep(uniqueId, flowSwitch.DisplayName, typeof(DsfSwitch).Name, serviceTestOutputs);
            if (serviceTestStep != null)
            {
                SetStepIcon(typeof(DsfSwitch), serviceTestStep);
            }

            return serviceTestStep;
        }

        void ProcessActivity(ModelItem modelItem)
        {
            var step = BuildParentsFromModelItem(modelItem);
            if (step != null)
            {
                if (step.Parent == null)
                {
                    ProcessStepActivity(step);
                }
                else
                {
                    ProcessParentsActivities(step);
                }
            }
            else
            {
                var computedValue = modelItem.GetCurrentValue();
                var boolAct = computedValue as DsfActivityAbstract<bool>;
                var activityUniqueID = boolAct?.UniqueID;
                var activityDisplayName = boolAct?.DisplayName;
                var type = computedValue.GetType();
                var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                var alreadyAdded = CheckForExists(activityUniqueID, new List<string>(), activityDisplayName, type);
                if (alreadyAdded == null && activityUniqueID != null && type == typeof(DsfActivity))
                {
                    var testStep = CreateMockChildStep(Guid.Parse(activityUniqueID), null, type.Name, activityDisplayName);
                    serviceTestOutputs.Add(new ServiceTestOutput("", "", "", "")
                    {
                        AssertOp = "",
                        AddStepOutputRow = testStep.AddNewOutput,
                        IsSearchCriteriaEnabled = true
                    });
                    testStep.StepOutputs = serviceTestOutputs;
                    SelectedServiceTest.TestSteps.Add(testStep);
                    SetStepIcon(type, testStep);
                }
            }
        }

        void ProcessStepActivity(IServiceTestStep step)
        {
            var exists = FindExistingStep(step.ActivityID.ToString());
            if (exists == null)
            {
                SelectedServiceTest.TestSteps.Add(step);
            }
        }

        void ProcessParentsActivities(IServiceTestStep step)
        {
            var parent = step.Parent;
            while (parent != null)
            {
                var child = parent;
                if (child.Parent == null)
                {
                    var exists = FindExistingStep(step.ActivityID.ToString());
                    if (exists == null)
                    {
                        SelectedServiceTest.TestSteps.Add(child);
                    }
                }
                parent = child.Parent;
            }
        }

        IServiceTestStep BuildParentsFromModelItem(ModelItem modelItem)
        {
            var computedValue = modelItem.GetCurrentValue();
            if (computedValue is FlowStep && modelItem.Content?.Value != null)
            {
                computedValue = modelItem.Content.Value.GetCurrentValue();
            }
            var dsfActivityAbstract = computedValue as DsfActivityAbstract<string>;

            var activityUniqueId = dsfActivityAbstract?.UniqueID;
            var activityDisplayName = dsfActivityAbstract?.DisplayName;
            var outputs = dsfActivityAbstract?.GetOutputs();

            if (dsfActivityAbstract == null)
            {
                var boolAct = computedValue as DsfActivityAbstract<bool>;

                activityUniqueId = boolAct?.UniqueID;
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
                    var parentActivityUniqueID = GetParentFlowStepUniqueId(computedValue, item, ref parentComputedValue);
                    if (parentActivityUniqueID == activityUniqueId)
                    {
                        parentComputedValue = item.Content.Value.GetCurrentValue();
                    }
                    var parentActivityAbstract = parentComputedValue as DsfActivityAbstract<string>;
                    var parentActivityUniqueId = parentActivityAbstract?.UniqueID;
                    if (parentActivityAbstract == null)
                    {
                        var boolParentAct = computedValue as DsfActivityAbstract<bool>;
                        parentActivityUniqueId = boolParentAct?.UniqueID;
                    }
                    if (parentActivityUniqueId == activityUniqueId)
                    {
                        return CheckForExists(activityUniqueId, outputs, activityDisplayName, type);
                    }
                }

                if (outputs != null && outputs.Count > 0 && ServiceTestStepWithOutputs(activityUniqueId, activityDisplayName, outputs, type, item, out IServiceTestStep serviceTestStep) && ServiceTestStepWithOutputs(activityUniqueId, activityDisplayName, outputs, type, item, out IServiceTestStep testStep))
                {
                    return testStep;
                }

                if (ServiceTestStepGetParentType(item, out var serviceTestStep1))
                {
                    return serviceTestStep1;
                }
                return BuildParentsFromModelItem(item);
            }
            return CheckForExists(activityUniqueId, outputs, activityDisplayName, type);
        }

        static string GetParentFlowStepUniqueId(object computedValue, ModelItem item, ref object parentComputedValue)
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

            return parentActivityUniqueID;
        }

        IServiceTestStep CheckForExists(string activityUniqueID, List<string> outputs, string activityDisplayName, Type type)
        {
            var exists = FindExistingStep(activityUniqueID);
            if (exists == null && outputs != null && outputs.Count > 0)
            {
                var serviceTestStep = SelectedServiceTest.AddTestStep(activityUniqueID, activityDisplayName, type.Name, new ObservableCollection<IServiceTestOutput>());

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
            return exists;
        }

        bool ServiceTestStepWithOutputs(string uniqueID, string displayName, List<string> outputs, Type type, ModelItem item, out IServiceTestStep serviceTestStep)
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

        static List<IServiceTestOutput> AddOutputsIfHasVariable(List<string> outputs, IServiceTestStep step)
        {
            var serviceTestOutputs =
                outputs.Select(output => new ServiceTestOutput(output ?? "", "", "", "")
                {
                    HasOptionsForValue = false,
                    AddStepOutputRow = step.AddNewOutput
                }).Cast<IServiceTestOutput>().ToList();
            return serviceTestOutputs;
        }

        static List<IServiceTestOutput> AddOutputs(List<string> outputs, IServiceTestStep step)
        {
            if (outputs == null || outputs.Count == 0)
            {
                return new List<IServiceTestOutput>
                {
                    new ServiceTestOutput("", "", "", "")
                    {
                        HasOptionsForValue = false,
                        AddStepOutputRow = step.AddNewOutput
                    }
                };
            }
            var serviceTestOutputs =
                outputs.Select(output => new ServiceTestOutput(output ?? "", "", "", "")
                {
                    HasOptionsForValue = false,
                    AddStepOutputRow = step.AddNewOutput
                }).Cast<IServiceTestOutput>().ToList();
            return serviceTestOutputs;
        }

        IServiceTestStep FindExistingStep(string uniqueId)
        {
            var exists = SelectedServiceTest.TestSteps.Flatten(step => step.Children).FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);
            return exists;
        }

        bool ServiceTestStepGetParentType(ModelItem item, out IServiceTestStep serviceTestStep)
        {
            Type activityType = null;
            var uniqueId = string.Empty;
            var displayName = string.Empty;
            if (item.ItemType == typeof(DsfSequenceActivity))
            {
                if (item.GetCurrentValue() is DsfSequenceActivity act)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof(DsfSequenceActivity);
                    displayName = act.DisplayName;
                }
            }
            else if (item.ItemType == typeof(DsfForEachActivity))
            {
                if (item.GetCurrentValue() is DsfForEachActivity act)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof(DsfForEachActivity);
                    displayName = act.DisplayName;
                }
            }
            else
            {
                if (item.ItemType == typeof(DsfSelectAndApplyActivity) && item.GetCurrentValue() is DsfSelectAndApplyActivity act)
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
                serviceTestStep = SelectedServiceTest.TestSteps.Flatten(step => step.Children).FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);
                return true;
            }
            serviceTestStep = null;
            return false;
        }

        static IServiceTestStep CreateServiceTestStep(Guid uniqueID, string displayName, Type type, List<IServiceTestOutput> serviceTestOutputs)
        {
            var step = new ServiceTestStep(uniqueID, type.Name, serviceTestOutputs.ToObservableCollection(), StepType.Assert)
            {
                StepDescription = displayName
            };
            SetStepIcon(type, step);
            return step;
        }

        void SetParentChild(ModelItem item, IServiceTestStep step)
        {
            var parent = BuildParentsFromModelItem(item);
            if (parent != null)
            {
                step.Parent = parent;
                parent.Children.Add(step);
            }
        }

        static void SetStepIcon(Type type, IServiceTestStep serviceTestStep)
        {
            SetStepIcon(type?.Name, serviceTestStep);
        }

        static void SetStepIcon(string typeName, IServiceTestStep serviceTestStep)
        {
            if (string.IsNullOrEmpty(typeName) || serviceTestStep == null)
            {
                return;
            }
            var actTypeName = typeName;
            if (actTypeName == "DsfActivity")
            {
                if (serviceTestStep is ServiceTestStep serviceStep)
                {
                    serviceStep.StepIcon = Application.Current?.TryFindResource("Explorer-WorkflowService") as ImageSource;
                }
                return;
            }
            if (typeName == "DsfDecision" || typeName == "FlowDecision")
            {
                actTypeName = "DsfFlowDecisionActivity";
            }
            if (typeName == "DsfSwitch")
            {
                actTypeName = "DsfFlowSwitchActivity";
            }
            var type = Types.FirstOrDefault(x => x.Name == actTypeName);
            if (type != null && type.GetCustomAttributes().Any(a => a is ToolDescriptorInfo))
            {
                var desc = GetDescriptorFromAttribute(type);
                if (serviceTestStep is ServiceTestStep serviceStep)
                {
                    serviceStep.StepIcon = Application.Current?.TryFindResource(desc.Icon) as ImageSource;
                }
            }
        }

        static IToolDescriptor GetDescriptorFromAttribute(Type type)
        {
            var info = type.GetCustomAttributes(typeof(ToolDescriptorInfo)).First() as ToolDescriptorInfo;
            return new ToolDescriptor(info.Id, info.Designer, new WarewolfType(type.FullName, type.Assembly.GetName().Version, type.Assembly.Location), info.Name, info.Icon, type.Assembly.GetName().Version, true, info.Category, ToolType.Native, info.IconUri, info.FilterTag, info.ResourceToolTip, info.ResourceHelpText);
        }

        void ProcessDecision(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return;
            }
            var dds = modelItem.GetProperty("Conditions") as Dev2DecisionStack;
            var uniqueId = modelItem.GetProperty("UniqueID").ToString();
            var exists = SelectedServiceTest.TestSteps.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

            if (exists == null && SelectedServiceTest != null)
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
                if (SelectedServiceTest.AddTestStep(uniqueId, modelItem.GetProperty("DisplayName").ToString(), typeof(DsfDecision).Name, serviceTestOutputs) is ServiceTestStep serviceTestStep)
                {
                    SetStepIcon(typeof(DsfDecision), serviceTestStep);
                }
            }
        }

        IServiceTestStep ProcessFlowDecision(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return null;
            }
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
                    var ser = new Dev2JsonSerializer();
                    var dds = ser.Deserialize<Dev2DecisionStack>(eval);

                    var exists = SelectedServiceTest.TestSteps?.FirstOrDefault(a => a.ActivityID.ToString() == uniqueId);

                    if (exists == null && SelectedServiceTest != null)
                    {
                        var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                        var serviceTestOutput = new ServiceTestOutput(GlobalConstants.ArmResultText, "", "", "")
                        {
                            HasOptionsForValue = true,
                            OptionsForValue = new List<string> { dds.TrueArmText, dds.FalseArmText }
                        };
                        serviceTestOutputs.Add(serviceTestOutput);
                        var serviceTestStep = SelectedServiceTest.AddTestStep(uniqueId, dds.DisplayText, typeof(DsfDecision).Name, serviceTestOutputs);
                        SetStepIcon(typeof(DsfDecision), serviceTestStep);
                        return serviceTestStep;
                    }
                }
            }
            return null;
        }

        void SetServerName(IContextualResourceModel resourceModel)
        {
            if (resourceModel.Environment == null || resourceModel.Environment.IsLocalHost)
            {
                _serverName = string.Empty;
            }
            else
            {
                if (!resourceModel.Environment.IsLocalHost)
                {
                    _serverName = " - " + resourceModel.Environment.Name;
                }
            }
        }

        void OnReceivedResourceAffectedMessage(Guid resourceId, CompileMessageList changeList)
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
                    var mainViewModel = CustomContainer.Get<IShellViewModel>();
                    WorkflowDesignerViewModel = mainViewModel?.CreateNewDesigner(ResourceModel);
                    if (WorkflowDesignerViewModel != null)
                    {
                        WorkflowDesignerViewModel.ItemSelectedAction = ItemSelectedAction;
                    }
                    IsLoading = false;
                });
            }
        }

        bool IsServerConnected() => ResourceModel.Environment.IsConnected;

        void StopTest()
        {
            SelectedServiceTest.IsTestRunning = false;
            SelectedServiceTest.TestPending = true;
            ServiceTestCommandHandler.StopTest(ResourceModel);
        }

        void RunSelectedTestInBrowser()
        {
            var runSelectedTestUrl = GetWebRunUrlForTest(SelectedServiceTest);
            ServiceTestCommandHandler.RunSelectedTestInBrowser(runSelectedTestUrl, _processExecutor);
        }

        void RunSelectedTest()
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
                    if (IsResourceDeleted)
                    {
                        return;
                    }
                }
                ServiceTestCommandHandler.RunSelectedTest(SelectedServiceTest, ResourceModel, AsyncWorker);
                ViewModelUtils.RaiseCanExecuteChanged(StopTestCommand);
            }
        }

        void RunAllTestsInBrowser()
        {
            ServiceTestCommandHandler.RunAllTestsInBrowser(IsDirty, RunAllTestsUrl, _processExecutor);
        }

        private void RunAllCoverageInBrowser()
        {
            ServiceTestCommandHandler.RunAllTestCoverageInBrowser(IsDirty, RunAllCoverageUrl, _processExecutor);
        }

        void RunAllTests()
        {
            ServiceTestCommandHandler.RunAllTestsCommand(IsDirty, RealTests().Where(model => model.Enabled), ResourceModel, AsyncWorker);
            SelectedServiceTest = null;
        }

        void DuplicateTest()
        {
            var testNumber = GetNewTestNumber(SelectedServiceTest.TestName);
            var duplicateTest = ServiceTestCommandHandler.DuplicateTest(SelectedServiceTest, testNumber);
            AddAndSelectTest(duplicateTest);
            foreach (var testStep in duplicateTest.TestSteps)
            {
                var typeName = testStep.ActivityType;
                SetStepIcon(typeName, testStep);
            }
        }

        bool CanDeleteTest(IServiceTestModel selectedTestModel) => GetPermissions() && selectedTestModel != null && !selectedTestModel.Enabled && IsServerConnected();

        IAsyncWorker AsyncWorker { get; }
        IEventAggregator EventPublisher { get; }

        void CreateTests(bool isFromDebug = false)
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
            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel, testNumber, isFromDebug);
            AddAndSelectTest(testModel);
        }

        bool _canAddFromDebug;
        bool _isLoading;
        bool _isValid;
        bool _dirty;
        IWarewolfWebClient _webClient;

        int GetNewTestNumber(string testName)
        {
            var counter = 1;
            var fullName = testName + " " + counter;

            while (Contains(fullName))
            {
                counter++;
                fullName = testName + " " + counter;
            }

            return counter;
        }

        bool Contains(string nameToCheck)
        {
            var serviceTestModel = RealTests().FirstOrDefault(a => a.TestName.Contains(nameToCheck));
            return serviceTestModel != null;
        }

        void SetDuplicateTestTooltip()
        {
            if (SelectedServiceTest != null)
            {
                SelectedServiceTest.DuplicateTestTooltip = SelectedServiceTest.NewTest ? Resources.Languages.Tooltips.ServiceTestNewTestDisabledDuplicateSelectedTestTooltip : CanDuplicateTest ? Resources.Languages.Tooltips.ServiceTestDuplicateSelectedTestTooltip : Resources.Languages.Tooltips.ServiceTestDisabledDuplicateSelectedTestTooltip;
            }
        }

        void AddAndSelectTest(IServiceTestModel testModel)
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

        bool CanStopTest => SelectedServiceTest != null && SelectedServiceTest.IsTestRunning;
        bool CanRunSelectedTestInBrowser => SelectedServiceTest != null && !SelectedServiceTest.IsDirty && IsServerConnected();
        bool CanRunSelectedTest => GetPermissions() && IsServerConnected();
        bool CanDuplicateTest => GetPermissions() && SelectedServiceTest != null && !SelectedServiceTest.NewTest;

        public bool CanSave { get; set; }

        bool IsResourceDeleted { get; set; }

        static bool GetPermissions() => true;

        bool IsValidName()
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
                else
                {
                    if (name.Trim() != name)
                    {
                        ErrorMessage = string.Format(ErrorResource.ContainsLeadingOrTrailingWhitespace, "'name'");
                    }
                }

                return string.IsNullOrEmpty(ErrorMessage);
            }
            return true;
        }

        bool AllNamesValid(IEnumerable<string> testNames)
        {
            foreach (var name in testNames)
            {
                ErrorMessage = string.Empty;
                if (string.IsNullOrEmpty(name))
                {
                    ErrorMessage = string.Format(ErrorResource.CannotBeNull, "'name'");
                    var popupController = CustomContainer.Get<IPopupController>();
                    popupController?.Show(Resources.Languages.Core.ServiceTestEmptyTestNameHeader, "Empty Test Name"
                        , MessageBoxButton.OK, MessageBoxImage.Error, null,
                        false, true, false, false, false, false);
                    return false;
                }
                else if (NameHasInvalidCharacters(name))
                {
                    ErrorMessage = string.Format(ErrorResource.ContainsInvalidCharecters, "'name'");
                    return false;
                }
                else if (name.Trim() != name)
                {
                    ErrorMessage = string.Format(ErrorResource.ContainsLeadingOrTrailingWhitespace, "'name'");
                    return false;
                }
                else
                {
                    continue;
                }
            }
            return true;
        }
        static bool NameHasInvalidCharacters(string name) => Regex.IsMatch(name, @"[^a-zA-Z0-9._\s-]");

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(() => ErrorMessage);
            }
        }

        public IWorkflowDesignerViewModel WorkflowDesignerViewModel
        {
            get => _workflowDesignerViewModel;
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
            catch (Exception ex)
            {
                Dev2Logger.Error("Service test save error.", ex, GlobalConstants.WarewolfError);
            }
            finally
            {
                var isDirty = IsDirty;
                SetDisplayName(isDirty);
            }
        }

        void Save(List<IServiceTestModel> serviceTestModels)
        {
            if (!AllNamesValid(Tests.Select(p => p.TestName).ToList()))
            {
                return;
            }
            MarkPending(serviceTestModels);
            var serviceTestModelTos = serviceTestModels.Select(CreateServiceTestModelTo).ToList();

            var result = ResourceModel.Environment.ResourceRepository.SaveTests(ResourceModel, serviceTestModelTos);
            switch (result.Result)
            {
                case SaveResult.Success:
                    MarkTestsAsNotNew();
                    SetSelectedTestUrl();
                    break;
                case SaveResult.ResourceDeleted:
                    PopupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                    _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.EnvironmentID);
                    IsResourceDeleted = true;
                    break;
                case SaveResult.ResourceUpdated:
                    UpdateTestsFromResourceUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static void MarkPending(List<IServiceTestModel> serviceTestModels)
        {
            foreach (var serviceTestModel in serviceTestModels)
            {
                serviceTestModel.TestPending = true;
                if (serviceTestModel.TestSteps != null)
                {
                    MarkTestStepsPending(serviceTestModel);
                }

                if (serviceTestModel.Outputs == null)
                {
                    continue;
                }

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

        static void MarkTestStepsPending(IServiceTestModel serviceTestModel)
        {
            foreach (var serviceTestStep in serviceTestModel.TestSteps)
            {
                MarkChildrenPending(serviceTestStep);
                if (serviceTestStep.Children == null)
                {
                    continue;
                }

                var testSteps = serviceTestStep.Children.Flatten(step => step.Children);
                foreach (var testStep in testSteps)
                {
                    MarkChildrenPending(testStep);
                }
            }
        }

        static void MarkChildrenPending(IServiceTestStep serviceTestStep)
        {
            if (serviceTestStep is ServiceTestStep step)
            {
                step.TestPending = true;
                if (step.Result != null)
                {
                    step.Result.RunTestResult = RunResult.TestPending;
                }

                if (step.StepOutputs == null)
                {
                    return;
                }
                MarkStepOutputsPending(step);
            }
        }

        static void MarkStepOutputsPending(IServiceTestStep step)
        {
            foreach (var serviceTestOutput in step.StepOutputs)
            {
                if (serviceTestOutput is ServiceTestOutput stepOutput)
                {
                    stepOutput.TestPending = true;
                    if (stepOutput.Result != null)
                    {
                        stepOutput.Result.RunTestResult = RunResult.TestPending;
                    }
                }
            }
        }

        static IServiceTestModelTO CreateServiceTestModelTo(IServiceTestModel model) => new ServiceTestModelTO
        {
            TestName = model.TestName,
            ResourceId = model.ParentId,
            AuthenticationType = model.AuthenticationType,
            Enabled = model.Enabled,
            ErrorExpected = model.ErrorExpected,
            NoErrorExpected = model.NoErrorExpected,
            ErrorContainsText = model.ErrorContainsText,
            TestSteps = model.TestSteps?.Select(step => CreateServiceTestStepTo(step, null)).ToList() ?? new List<IServiceTestStep>(),
            Inputs = model.Inputs?.Select(CreateServiceTestInputsTo).ToList() ?? new List<IServiceTestInput>(),
            Outputs = model.Outputs?.Select(CreateServiceTestOutputTo).ToList() ?? new List<IServiceTestOutput>(),
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

        static IServiceTestOutput CreateServiceTestOutputTo(IServiceTestOutput output) => new ServiceTestOutputTO
        {
            Variable = output.Variable,
            Value = output.Value,
            From = output.From,
            To = output.To,
            AssertOp = output.AssertOp,
            HasOptionsForValue = output.HasOptionsForValue,
            OptionsForValue = output.OptionsForValue
        };

        static IServiceTestInput CreateServiceTestInputsTo(IServiceTestInput input) => new ServiceTestInputTO
        {
            Variable = input.Variable,
            Value = input.Value,
            EmptyIsNull = input.EmptyIsNull
        };

        static IServiceTestStep CreateServiceTestStepTo(IServiceTestStep step, IServiceTestStep parent)
        {
            var serviceTestStepTo = new ServiceTestStepTO(step.ActivityID, step.ActivityType, step.StepOutputs.Select(CreateServiceTestStepOutputsTo).ToObservableCollection(), step.Type)
            {
                Children = new ObservableCollection<IServiceTestStep>(),
                Parent = parent,
                StepDescription = step.StepDescription
            };
            if (step.Children != null)
            {
                foreach (var serviceTestStep in step.Children)
                {
                    serviceTestStepTo.Children.Add(CreateServiceTestStepTo(serviceTestStep, serviceTestStepTo));
                }
            }
            return serviceTestStepTo;
        }

        static IServiceTestOutput CreateServiceTestStepOutputsTo(IServiceTestOutput output) => new ServiceTestOutputTO
        {
            Variable = output.Variable,
            Value = output.Value,
            From = output.From,
            To = output.To,
            AssertOp = output.AssertOp,
            HasOptionsForValue = output.HasOptionsForValue,
            OptionsForValue = output.OptionsForValue
        };

        void UpdateTestsFromResourceUpdate()
        {
            foreach (var serviceTestModel in Tests)
            {
                var runSelectedTestUrl = GetWebRunUrlForTest(serviceTestModel);
                serviceTestModel.RunSelectedTestUrl = runSelectedTestUrl;
            }
        }

        string GetWebRunUrlForTest(IServiceTestModel serviceTestModel)
        {
            var runSelectedTestUrl = ResourceModel.GetWorkflowUri("", UrlType.Tests) + "/" + serviceTestModel.TestName;
            if (serviceTestModel.AuthenticationType == AuthenticationType.Public)
            {
                runSelectedTestUrl = runSelectedTestUrl.Replace("/secure/", "/public/");
            }
            return runSelectedTestUrl;
        }

        bool ShowPopupWhenDuplicates()
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

        void SetSelectedTestUrl()
        {
            var runSelectedTestUrl = GetWebRunUrlForTest(SelectedServiceTest);
            SelectedServiceTest.RunSelectedTestUrl = runSelectedTestUrl;
        }

        void MarkTestsAsNotNew()
        {
            foreach (var model in _tests.Where(model => model.NewTest))
            {
                model.NewTest = false;
            }
            foreach (var model in RealTests())
            {
                var clone = model.Clone();
                model.SetItem(clone);
                model.ResetOldTestName();
            }
        }

        public IContextualResourceModel ResourceModel
        {
            get => _resourceModel;
            private set => _resourceModel = value;
        }

        public IServiceTestModel SelectedServiceTest
        {
            get => _selectedServiceTest;
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

                _selectedServiceTest = value;
                _selectedServiceTest.IsTestLoading = true;
                _selectedServiceTest.PropertyChanged += ActionsForPropChanges;

                var serviceTestSteps = _selectedServiceTest?.TestSteps?.Flatten(step => step.Children ?? new ObservableCollection<IServiceTestStep>());
                if (serviceTestSteps != null)
                {
                    foreach (var serviceTestOutput in serviceTestSteps.Where(serviceTestStep => serviceTestStep?.StepOutputs != null).SelectMany(serviceTestStep => serviceTestStep.StepOutputs))
                    {
                        ((ServiceTestOutput)serviceTestOutput).PropertyChanged += OnStepOutputPropertyChanges;
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

        void OnStepOutputPropertyChanges(object sender, PropertyChangedEventArgs e)
        {
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            _dirty = IsDirty;
            OnPropertyChanged(() => IsDirty);
        }

        void ActionsForPropChanges(object sender, PropertyChangedEventArgs e)
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

        void SetDisplayName(bool isDirty)
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
            get => _runAllTestsUrl;
            set
            {
                _runAllTestsUrl = value;
                OnPropertyChanged(() => RunAllTestsUrl);
            }
        }

        public string RunAllCoverageUrl
        {
            get => _runAllCoverageUrl;
            set
            {
                _runAllCoverageUrl = value;
                OnPropertyChanged(() => RunAllCoverageUrl);
            }
        }

        public string TestPassingResult
        {
            get => _testPassingResult;
            set
            {
                _testPassingResult = value;
                OnPropertyChanged(() => TestPassingResult);
            }
        }

        IEnumerable<IServiceTestModel> RealTests() => _tests.Where(model => model.GetType() != typeof(DummyServiceTest)).ToObservableCollection();

        public ObservableCollection<IServiceTestModel> Tests
        {
            get => _tests;
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
            }
        }

        void DeleteTest(IServiceTestModel test)
        {
            if (test == null)
            {
                return;
            }

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
                    if (Tests.Count == 1 && Tests.Single().GetType() == typeof(DummyServiceTest))
                    {
                        CanSave = false;
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("IServiceTestModelTO DeleteTest(IServiceTestModel model)", ex, GlobalConstants.WarewolfError);
                }
            }
            if (_tests.Count == 1 && _tests.Single().GetType() == typeof(DummyServiceTest))
            {
                CanSave = false;
            }
        }

        void DeleteTestStep(IServiceTestStep testStep)
        {
            if (testStep == null)
            {
                return;
            }

            DeleteStep(testStep, SelectedServiceTest.TestSteps);
        }

        static void DeleteStep(IServiceTestStep testStep, ObservableCollection<IServiceTestStep> serviceTestSteps)
        {
            if (serviceTestSteps.Contains(testStep))
            {
                serviceTestSteps.Remove(testStep);
            }
            else
            {
                var foundParentStep = serviceTestSteps.FirstOrDefault(step => step.ActivityID == testStep.Parent?.ActivityID);
                foundParentStep?.Children?.Remove(testStep);
            }
        }

        ObservableCollection<IServiceTestModel> GetTests()
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

        public ServiceTestModel ToServiceTestModel(IServiceTestModelTO to)
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
                TestInvalid = to.TestInvalid,
                TestPending = to.TestPending,
                TestFailing = to.TestFailing,
                TestPassed = to.TestPassed,
                Password = to.Password,
                ParentId = to.ResourceId,
                TestSteps = to.TestSteps?.Select(step => CreateServiceTestStep(step) as IServiceTestStep).ToObservableCollection(),
                Inputs = to.Inputs?.Select(CreateInput).ToObservableCollection(),
                Outputs = to.Outputs?.Select(CreateOutput).ToObservableCollection()
            };
            return serviceTestModel;
        }

        static IServiceTestOutput CreateOutput(IServiceTestOutput output)
        {
            var serviceTestOutput = new ServiceTestOutput(output.Variable, output.Value, output.From, output.To) as IServiceTestOutput;
            serviceTestOutput.AssertOp = output.AssertOp;
            serviceTestOutput.Result = output.Result;
            return serviceTestOutput;
        }

        static IServiceTestInput CreateInput(IServiceTestInput input)
        {
            var serviceTestInput = new ServiceTestInput(input.Variable, input.Value) as IServiceTestInput;
            serviceTestInput.EmptyIsNull = input.EmptyIsNull;
            return serviceTestInput;
        }

        IServiceTestStep CreateServiceTestStep(IServiceTestStep step)
        {
            var testStep = new ServiceTestStep(step.ActivityID, step.ActivityType, new ObservableCollection<IServiceTestOutput>(), step.Type)
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

        static ObservableCollection<IServiceTestOutput> CreateServiceTestOutputFromStep(ObservableCollection<IServiceTestOutput> stepStepOutputs, IServiceTestStep testStep)
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
        public ICommand RunAllTestCoverageInBrowserCommand { get; set; }
        public ICommand RunAllTestsCommand { get; set; }
        public ICommand RunSelectedTestInBrowserCommand { get; set; }
        public ICommand RunSelectedTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }
        public ICommand CreateTestCommand { get; set; }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

        public void Dispose()
        {
            if (ResourceModel?.Environment?.Connection != null)
            {
                ResourceModel.Environment.Connection.ReceivedResourceAffectedMessage -= OnReceivedResourceAffectedMessage;
            }
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
}
