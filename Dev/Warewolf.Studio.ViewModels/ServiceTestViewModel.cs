﻿
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
        private bool _canSave;
        private string _errorMessage;
        private readonly IShellViewModel _shellViewModel;
        private IContextualResourceModel _resourceModel;
        private string _serverName;
        private IWorkflowDesignerViewModel _workflowDesignerViewModel;

        private static readonly IEnumerable<Type> Types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());

        private void PrepopulateTestsUsingDebug(List<IDebugTreeViewItemViewModel> models)
        {
            CreateTests();
            AddFromDebug(models);
        }


        public void AddDuplicateTestUsingDebug(NewTestFromDebugMessage message)
        {
            DuplicateTest();
            SelectedServiceTest.TestSteps = new ObservableCollection<IServiceTestStep>();
            AddFromDebug(message.RootItems);
        }


        private void AddFromDebug(List<IDebugTreeViewItemViewModel> models)
        {
            foreach(IDebugTreeViewItemViewModel debugState in models)
            {
                var debugItem = debugState as DebugStateTreeViewItemViewModel;
                if(debugItem != null && debugItem.Parent == null)
                {
                    var debugItemContent = debugItem.Content;
                    if(debugItemContent != null)
                    {
                        if(debugItemContent.ActivityType == ActivityType.Workflow && debugItemContent.OriginatingResourceID == ResourceModel.ID)
                        {
                            ProcessInputsAndOutputs(debugItemContent);
                        }
                        else if(debugItemContent.ActivityType != ActivityType.Workflow && debugItemContent.ActualType!=typeof(TestMockDecisionStep).Name && debugItemContent.ActualType != typeof(TestMockSwitchStep).Name && debugItemContent.ActualType != typeof(TestMockStep).Name)
                        {
                            var actualType = debugItemContent.ActualType;
                            if(actualType == typeof(DsfDecision).Name)
                            {
                                DecisionFromDebug(debugItemContent);
                            }
                            else if(actualType == typeof(DsfSwitch).Name)
                            {
                                SwitchFromDebug(debugItemContent);
                            }
                            else
                            {
                                AddStepFromDebug(debugState, debugItemContent);
                            }
                        }
                    }
                }
            }
        }

        private void SwitchFromDebug(IDebugState debugItemContent)
        {
            var processFlowSwitch = ProcessFlowSwitch(WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ParentID));
            if(processFlowSwitch != null)
            {
                if(debugItemContent.Outputs != null && debugItemContent.Outputs.Count > 0)
                {
                    processFlowSwitch.StepOutputs[0].Value = debugItemContent.Outputs[0].ResultsList[0].Value;
                }
            }
        }

        private void DecisionFromDebug(IDebugState debugItemContent)
        {
            var processFlowDecision = ProcessFlowDecision(WorkflowDesignerViewModel.GetModelItem(debugItemContent.WorkSurfaceMappingId, debugItemContent.ParentID));
            if(processFlowDecision != null)
            {
                if(debugItemContent.Outputs != null && debugItemContent.Outputs.Count > 0)
                {
                    if (debugItemContent.Outputs[0].ResultsList.Count > 0)
                    {
                        processFlowDecision.StepOutputs[0].Value = debugItemContent.Outputs[0].ResultsList[0].Value;
                    }
                }
            }
        }

        private void ProcessInputsAndOutputs(IDebugState debugItemContent)
        {
            if(debugItemContent.StateType == StateType.Start)
            {
                SetInputs(debugItemContent);
            }
            else if(debugItemContent.StateType == StateType.End)
            {
                SetOutputs(debugItemContent);
            }
        }

        private void AddStepFromDebug(IDebugTreeViewItemViewModel debugState, IDebugState debugItemContent)
        {
            if (debugState.Children != null && debugState.Children.Count > 0)
            {
                var testSteps = SelectedServiceTest.TestSteps;
                AddChildDebugItems(debugItemContent, debugState, testSteps, null);
            }
            else
            {
                var outputs = debugItemContent.Outputs;
                var serviceTestStep = SelectedServiceTest.AddTestStep(debugItemContent.ID.ToString(), debugItemContent.DisplayName, debugItemContent.ActualType, new ObservableCollection<IServiceTestOutput>()) as ServiceTestStep;
                AddOutputs(outputs, serviceTestStep);
                if (serviceTestStep != null)
                {
                    SetStepIcon(serviceTestStep.ActivityType, serviceTestStep);
                }
            }
        }

        private void AddChildDebugItems(IDebugState debugItemContent, IDebugTreeViewItemViewModel debugState, ObservableCollection<IServiceTestStep> testSteps, IServiceTestStep parent)
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
                if (debugItemContent.ActualType == seqTypeName)
                {
                    SetStepIcon(typeof(DsfSequenceActivity),testStep);
                    testStep.ActivityType = seqTypeName;
                    parent = testStep;
                }

                else if (debugItemContent.ActualType == forEachTypeName)
                {
                    SetStepIcon(typeof(DsfForEachActivity), testStep);
                    testStep.ActivityType = forEachTypeName;
                    parent = testStep;
                }else if(debugItemContent.ActualType == selectApplyTypeName)
                {
                    SetStepIcon(typeof(DsfSelectAndApplyActivity), testStep);
                    testStep.ActivityType = selectApplyTypeName;
                    parent = testStep;
                }
                else
                {
                    return;
                }
            }
            foreach (var debugTreeViewItemViewModel in debugState.Children)
            {
                var childItem = debugTreeViewItemViewModel as DebugStateTreeViewItemViewModel;
                var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                if(childItem != null)
                {
                    var childItemContent = childItem.Content;
                    var outputs = childItemContent.Outputs;
                    
                    var exists = parent.Children.FirstOrDefault(a => a.UniqueId == childItemContent.ID);
                    if (exists==null)
                    {
                        var childStep = new ServiceTestStep(childItemContent.ID, childItemContent.ActualType, serviceTestOutputs, StepType.Assert)
                        {
                            StepDescription = childItemContent.DisplayName,
                            Parent = parent,
                            Type = StepType.Assert
                        };
                        AddOutputs(outputs, childStep);
                        SetStepIcon(childStep.ActivityType, childStep);
                        parent.Children.Add(childStep);
                        if (childItem.Children != null && childItem.Children.Count > 0)
                        {
                            AddChildDebugItems(childItemContent, childItem, parent.Children, parent);
                        }
                    }
                }
            }
            var parentExists = testSteps.FirstOrDefault(a => a.UniqueId == parent.UniqueId);
            if (parentExists == null)
            {
                testSteps.Add(parent);
            }
        }

        private static void AddOutputs(List<IDebugItem> outputs, ServiceTestStep serviceTestStep)
        {
            if (outputs != null && outputs.Count > 0)
            {
                var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
                foreach (var output in outputs)
                {
                    var actualOutputs = output.ResultsList.Where(result => result.Type == DebugItemResultType.Variable).Where(s => !string.IsNullOrEmpty(s.Variable));
                    foreach (var debugItemResult in actualOutputs)
                    {
                        var serviceTestOutput = new ServiceTestOutput(debugItemResult.Variable, debugItemResult.Value, "", "")
                        {
                            AssertOp = "=",
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
                    var serviceTestInput = SelectedServiceTest.Inputs.FirstOrDefault(input => input.Variable.Equals(variable));
                    if (serviceTestInput != null)
                    {
                        serviceTestInput.Value = value;
                    }
                }
            }
        }

        private void SetOutputs(IDebugState outPutState)
        {
            if (outPutState != null)
            {
                foreach (var debugItem in outPutState.Outputs)
                {
                    var variable = debugItem.ResultsList.First().Variable.Replace("[[", "").Replace("]]", "");
                    var value = debugItem.ResultsList.First().Value;
                    var serviceTestInput = SelectedServiceTest.Outputs.FirstOrDefault(input => input.Variable.Equals(variable));
                    if (serviceTestInput != null)
                    {
                        serviceTestInput.Value = value;
                    }
                }
            }
        }

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
            CanSave = true;
            RunAllTestsUrl = WebServer.GetWorkflowUri(resourceModel, "", UrlType.Tests)?.ToString();
            IsLoading = true;


            UpdateHelpDescriptor(Resources.Languages.Core.ServiceTestGenericHelpText);

            WorkflowDesignerViewModel = workflowDesignerViewModel;
            WorkflowDesignerViewModel.IsTestView = true;
            WorkflowDesignerViewModel.ItemSelectedAction = ItemSelectedAction;

            AsyncWorker.Start(GetTests, models =>
            {
                var dummyTest = new DummyServiceTest(CreateTests) { TestName = "Create a new test." };
                models.Add(dummyTest);
                SelectedServiceTest = dummyTest;
                Tests = models;
                IsLoading = false;

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
            }, OnError);

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
                if (modelItem.ItemType == typeof(Flowchart) || modelItem.ItemType == typeof(ActivityBuilder))
                {
                    return;
                }
                if (modelItem.ItemType == typeof(DsfForEachActivity))
                {
                    ProcessForEach(modelItem);
                }
                else if(modelItem.ItemType == typeof(DsfSelectAndApplyActivity))
                {
                    ProcessSelectAndApply(modelItem);
                }
                else if (modelItem.ItemType == typeof(DsfSequenceActivity))
                {
                    ProcessSequence(modelItem);
                }
                else if (modelItem.ItemType == typeof(FlowSwitch<string>))
                {
                    ProcessFlowSwitch(modelItem);
                }
                else if (modelItem.ItemType == typeof(DsfSwitch))
                {
                    ProcessSwitch(modelItem);
                }
                else if (modelItem.ItemType == typeof(FlowDecision))
                {
                    ProcessFlowDecision(modelItem);
                }
                else if (modelItem.ItemType == typeof(DsfDecision))
                {
                    ProcessDecision(modelItem);
                }
                else
                {
                    ProcessActivity(modelItem);
                }
            }
        }

        private void ProcessSequence(ModelItem modelItem)
        {
            var sequence = modelItem.GetCurrentValue() as DsfSequenceActivity;
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

        private void ProcessForEach(ModelItem modelItem)
        {
            var forEachActivity = modelItem.GetCurrentValue() as DsfForEachActivity;
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
                if (exists == null)
                {
                    serviceTestSteps.Add(testStep);
                }
            }
        }

        private void ProcessSelectAndApply(ModelItem modelItem)
        {
            var selectAndApplyActivity = modelItem.GetCurrentValue() as DsfSelectAndApplyActivity;
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

        private static void AddMissingChild(ObservableCollection<IServiceTestStep> serviceTestSteps, ServiceTestStep testStep)
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

        private void AddChildActivity(DsfNativeActivity<string> act, ServiceTestStep testStep)
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
                var flowSwitch = modelItem.GetCurrentValue() as FlowSwitch<string>;
                if (flowSwitch == null)
                {
                    var modelItemParent = modelItem.Parent;
                    flowSwitch = modelItemParent.GetCurrentValue() as FlowSwitch<string>;
                    condition = modelItemParent.GetProperty("Expression");
                    activity = (DsfFlowNodeActivity<string>)condition;
                }
                if(flowSwitch != null)
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
                    SelectedServiceTest.TestSteps.Add(step);
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
            var dsfActivityAbstract = computedValue as DsfActivityAbstract<string>;
            var type = computedValue.GetType();
            var outputs = dsfActivityAbstract?.GetOutputs();
            var item = modelItem.Parent;
            
            if (item != null && (item.ItemType != typeof (Flowchart) || item.ItemType == typeof (ActivityBuilder)))
            {
                if (outputs != null && outputs.Count > 0)

                {
                    IServiceTestStep serviceTestStep;
                    if (ServiceTestStepWithOutputs(dsfActivityAbstract, outputs, type, item, out serviceTestStep))
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
            var exists = FindExistingStep(dsfActivityAbstract?.UniqueID);
            if (exists == null)
            {
                if (outputs != null && outputs.Count > 0)
                {
                    var serviceTestStep = SelectedServiceTest.AddTestStep(dsfActivityAbstract.UniqueID, dsfActivityAbstract.DisplayName, type.Name, new ObservableCollection<IServiceTestOutput>()) as ServiceTestStep;

                    var serviceTestOutputs = outputs.Where(s => !string.IsNullOrEmpty(s)).Select(output =>
                            {
                                return new ServiceTestOutput(output, "", "", "")
                                {
                                    HasOptionsForValue = false,
                                    AddStepOutputRow = s => serviceTestStep.AddNewOutput(s)
                                };
                            }).Cast<IServiceTestOutput>().ToList();
                    var step = CreateServiceTestStep(Guid.Parse(dsfActivityAbstract.UniqueID), dsfActivityAbstract.DisplayName, type, serviceTestOutputs);
                    return step;
                }
            }
            return null;
        }

        private bool ServiceTestStepWithOutputs(DsfActivityAbstract<string> dsfActivityAbstract, List<string> outputs, Type type, ModelItem item, out IServiceTestStep serviceTestStep)
        {
            var exists = FindExistingStep(dsfActivityAbstract.UniqueID);
            if (exists == null)
            {
                var step = CreateServiceTestStep(Guid.Parse(dsfActivityAbstract.UniqueID), dsfActivityAbstract.DisplayName, type, new List<IServiceTestOutput>());
                var serviceTestOutputs = outputs.Where(s => !string.IsNullOrEmpty(s)).Select(output => new ServiceTestOutput(output, "", "", "")
                {
                    HasOptionsForValue = false,
                    AddStepOutputRow = s => step.AddNewOutput(s)
                    }).Cast<IServiceTestOutput>().ToList();
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
            if (item.ItemType == typeof (DsfSequenceActivity))
            {
                var act = item.GetCurrentValue() as DsfSequenceActivity;
                if (act != null)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof (DsfSequenceActivity);
                    displayName = act.DisplayName;
                }
            }
            else if (item.ItemType == typeof (DsfForEachActivity))
            {
                var act = item.GetCurrentValue() as DsfForEachActivity;
                if (act != null)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof (DsfForEachActivity);
                    displayName = act.DisplayName;
                }
            }
            else if (item.ItemType == typeof (DsfSelectAndApplyActivity))
            {
                var act = item.GetCurrentValue() as DsfSelectAndApplyActivity;
                if (act != null)
                {
                    uniqueId = act.UniqueID;
                    activityType = typeof (DsfSelectAndApplyActivity);
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
        }

        private void SetStepIcon(string typeName, ServiceTestStep serviceTestStep)
        {
            if (typeName == "DsfDecision" || typeName == "FlowDecision")
            {
                typeName = "DsfFlowDecisionActivity";
            }
            if (typeName == "DsfSwitch")
            {
                typeName = "DsfFlowSwitchActivity";
            }
            Type type = Types.FirstOrDefault(x => x.Name == typeName);

            if (type.GetCustomAttributes().Any(a => a is ToolDescriptorInfo))
            {
                var desc = GetDescriptorFromAttribute(type);
                if (serviceTestStep != null)
                    serviceTestStep.StepIcon = Application.Current?.TryFindResource(desc.Icon) as ImageSource;
            }
        }

        IToolDescriptor GetDescriptorFromAttribute(Type type)
        {
            var info = type.GetCustomAttributes(typeof(ToolDescriptorInfo)).First() as ToolDescriptorInfo;
            // ReSharper disable once PossibleNullReferenceException
            return new ToolDescriptor(info.Id, info.Designer, new WarewolfType(type.FullName, type.Assembly.GetName().Version, type.Assembly.Location), info.Name, info.Icon, type.Assembly.GetName().Version, true, info.Category, ToolType.Native, info.IconUri, info.FilterTag);
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
                        if(dds != null)
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

                        var exists = SelectedServiceTest.TestSteps.FirstOrDefault(a => a.UniqueId.ToString() == uniqueId);

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
                                    SetStepIcon(typeof(DsfDecision), serviceTestStep);
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
            AsyncWorker.Start(() =>
            {
                var contextModel = ResourceModel.Environment.ResourceRepository.LoadContextualResourceModel(resourceId);
                _resourceModel = contextModel;
                return GetTests();
            }, models =>
            {
                var dummyTest = new DummyServiceTest(CreateTests) { TestName = "Create a new test." };
                models.Add(dummyTest);
                var testName = SelectedServiceTest?.TestName;
                SelectedServiceTest = dummyTest;
                _tests = models;
                OnPropertyChanged(() => Tests);
                SelectedServiceTest = _tests.FirstOrDefault(model => model.TestName == testName);
                IsLoading = false;
            });
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
            ServiceTestCommandHandler.RunSelectedTestInBrowser(SelectedServiceTest.RunSelectedTestUrl, _processExecutor);
        }

        private void RunSelectedTest()
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

        private void RunAllTestsInBrowser()
        {
            ServiceTestCommandHandler.RunAllTestsInBrowser(IsDirty, RunAllTestsUrl, _processExecutor);
        }

        private void RunAllTests()
        {
            ServiceTestCommandHandler.RunAllTestsCommand(IsDirty, RealTests(), ResourceModel, AsyncWorker);
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

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsLoading { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public IAsyncWorker AsyncWorker { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public IEventAggregator EventPublisher { get; set; }

        private void CreateTests()
        {
            SelectedServiceTest = null;
            if (IsDirty)
            {
                PopupController?.Show(Resources.Languages.Core.ServiceTestSaveEditedTestsMessage, Resources.Languages.Core.ServiceTestSaveEditedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                return;
            }

            var testNumber = GetNewTestNumber("Test");
            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel, testNumber);
            AddAndSelectTest(testModel);
            SetDisplayName();
        }

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
                    SelectedServiceTest.DuplicateTestTooltip = Resources.Languages.Core.ServiceTestNewTestDisabledDuplicateSelectedTestTooltip;
                }
                else
                {
                    SelectedServiceTest.DuplicateTestTooltip = CanDuplicateTest ? Resources.Languages.Core.ServiceTestDuplicateSelectedTestTooltip : Resources.Languages.Core.ServiceTestDisabledDuplicateSelectedTestTooltip;
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
            SetSelectedTestUrl();
            SetDuplicateTestTooltip();
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private bool CanStopTest => SelectedServiceTest != null && SelectedServiceTest.IsTestRunning;
        private bool CanRunSelectedTestInBrowser => SelectedServiceTest != null && !SelectedServiceTest.IsDirty && IsServerConnected();
        private bool CanRunSelectedTest => GetPermissions() && IsServerConnected();
        private bool CanDuplicateTest => GetPermissions() && SelectedServiceTest != null && !SelectedServiceTest.NewTest;

        public bool CanSave
        {
            get
            {
                var isValid = true;
                if (SelectedServiceTest != null)
                {
                    isValid = IsValidName(SelectedServiceTest.TestName);
                }
                _canSave = IsDirty && isValid;
                SetDisplayName();
                return _canSave;
            }
            set
            {
                //_canSave = value;
            }
        }

        private bool GetPermissions()
        {
            return true;
        }

        private bool IsValidName(string name)
        {
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
                    var isDirty = _tests.Any(resource => resource.IsDirty) || _tests.Any(resource => resource.NewTest);

                    var isConnected = ResourceModel.Environment.Connection.IsConnected;

                    return isDirty && isConnected;
                }
                // ReSharper disable once UnusedVariable
                catch (Exception)
                {
                    //if (!_errorShown)
                    //{
                    //    _popupController.ShowCorruptTaskResult(ex.Message);
                    //    Dev2Logger.Error(ex);
                    //    _errorShown = true;
                    //}
                }
                return false;
            }
        }

        public Guid ResourceID
        {
            get
            {
                return ResourceModel?.ID ?? Guid.Empty;
            }            
        }

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
            }
            catch (Exception)
            {
                // MarkTestsAsDirty(true);
            }
        }

        private void Save(List<IServiceTestModel> serviceTestModels)
        {
            var serviceTestModelTos = serviceTestModels.Select(CreateServiceTestModelTO).ToList();
            var result = ResourceModel.Environment.ResourceRepository.SaveTests(ResourceModel, serviceTestModelTos);
            switch (result.Result)
            {
                case SaveResult.Success:
                    MarkTestsAsNotNew();
                    SetSelectedTestUrl();
                    SetDisplayName();
                    break;
                case SaveResult.ResourceDeleted:
                    PopupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                    _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                    break;
                case SaveResult.ResourceUpdated:
                    UpdateTestsFromResourceUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                TestSteps = model.TestSteps.Select(step => CreateServiceTestStepTO(step,null)).ToList(),
                Inputs = model.Inputs.Select(CreateServiceTestInputsTO).ToList(),
                Outputs = model.Outputs.Select(CreateServiceTestOutputTO).ToList(),
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
                serviceTestModel.RunSelectedTestUrl = WebServer.GetWorkflowUri(ResourceModel, "", UrlType.Tests) + "/" + serviceTestModel.TestName;
                if (serviceTestModel.AuthenticationType == AuthenticationType.Public)
                {
                    serviceTestModel.RunSelectedTestUrl = serviceTestModel.RunSelectedTestUrl.Replace("/secure/", "/public/");
                }
            }

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
            PopupController?.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
        }

        public void RefreshCommands()
        {
            ViewModelUtils.RaiseCanExecuteChanged(RunAllTestsCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunAllTestsInBrowserCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            OnPropertyChanged(() => DisplayName);
            SetDisplayName();
        }

        public bool HasDuplicates() => RealTests().ToList().GroupBy(x => x.TestName).Where(group => @group.Count() > 1).Select(group => @group.Key).Any();

        private void SetSelectedTestUrl()
        {
            SelectedServiceTest.RunSelectedTestUrl = WebServer.GetWorkflowUri(ResourceModel, "", UrlType.Tests) + "/" + SelectedServiceTest.TestName;
            if (SelectedServiceTest.AuthenticationType == AuthenticationType.Public)
            {
                SelectedServiceTest.RunSelectedTestUrl = SelectedServiceTest.RunSelectedTestUrl.Replace("/secure/", "/public/");
            }
        }

        private void MarkTestsAsNotNew()
        {
            foreach (var model in _tests.Where(model => model.NewTest))
            {
                model.NewTest = false;
            }
            foreach (var model in RealTests())
            {
                var clone = model.Clone() as IServiceTestModel;
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
                _selectedServiceTest = value;
                _selectedServiceTest.PropertyChanged += ActionsForPropChanges;
                SetSelectedTestUrl();
                SetDuplicateTestTooltip();
                OnPropertyChanged(() => SelectedServiceTest);
                EventPublisher.Publish(new DebugOutputMessage(_selectedServiceTest?.DebugForTest ?? new List<IDebugState>()));
            }
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
                SetDisplayName();
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
            ViewModelUtils.RaiseCanExecuteChanged(DuplicateTestCommand);
        }

        private void SetDisplayName()
        {
            if (IsDirty)
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
                testStep.Parent?.Children.Remove(testStep);
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
                    foreach (var to in loadResourceTests)
                    {
                        var serviceTestModel = ToServiceTestModel(to);
                        serviceTestModel.Item = ToServiceTestModel(to);
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
                Inputs = to.Inputs?.Select(input => new ServiceTestInput(input.Variable, input.Value) as IServiceTestInput).ToObservableCollection(),
                Outputs = to.Outputs?.Select(output =>
                {
                    var serviceTestOutput = new ServiceTestOutput(output.Variable, output.Value, output.From, output.To) as IServiceTestOutput;
                    serviceTestOutput.AssertOp = output.AssertOp;
                    return serviceTestOutput;
                }).ToObservableCollection()
            };
            return serviceTestModel;
        }

        private ServiceTestStep CreateServiceTestStep(IServiceTestStep step)
        {
            var testStep = new ServiceTestStep(step.UniqueId, step.ActivityType, new ObservableCollection<IServiceTestOutput>(), step.Type)
            {
                Children = new ObservableCollection<IServiceTestStep>(),
                Parent = step.Parent,
                StepDescription = step.StepDescription
            };
            testStep.StepOutputs =  CreateServiceTestOutputFromStep(step.StepOutputs, testStep);
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
                    OptionsForValue = serviceTestOutput.OptionsForValue
                };

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
