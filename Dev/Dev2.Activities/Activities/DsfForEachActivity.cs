#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Common.X6;
using Dev2.Comparer;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Resource.Messages;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("Execution-ForEach", "ForEach", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Loop Constructs", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_LoopConstruct_For Each")]
    public class DsfForEachActivity : DsfActivityAbstract<bool>, IEquatable<DsfForEachActivity>
    {
        string _previousParentId;
        string _displayName;
        readonly int _previousInputsIndex = -1;
        readonly int _previousOutputsIndex = -1;
        ForEachBootstrapTO _operationalData;

        public enForEachType ForEachType { get; set; }

        [FindMissing] public string From { get; set; }

        [FindMissing] public string To { get; set; }

        [FindMissing] public string Recordset { get; set; }

        [FindMissing] public string CsvIndexes { get; set; }

        [FindMissing] public string NumOfExections { get; set; }

        [Inputs("FromDisplayName")]
        [FindMissing]
        public string FromDisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                ForEachElementName = value;
            }
        }

        [Inputs("ForEachElementName")]
        [FindMissing]
        public string ForEachElementName { get; set; }


        public int ExecutionCount
        {
            get
            {
                if (_operationalData != null)
                {
                    return _operationalData.IterationCount;
                }

                return 0;
            }
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "ForEachElementName",
                    Type = StateVariable.StateType.Input,
                    Value = ForEachElementName
                },
                new StateVariable
                {
                    Name = "ForEachType",
                    Type = StateVariable.StateType.Input,
                    Value = ForEachType.ToString()
                },
                new StateVariable
                {
                    Name = "From",
                    Type = StateVariable.StateType.Input,
                    Value = From
                },
                new StateVariable
                {
                    Name = "To",
                    Type = StateVariable.StateType.Input,
                    Value = To
                },
                new StateVariable
                {
                    Name = "CsvIndexes",
                    Type = StateVariable.StateType.Input,
                    Value = CsvIndexes
                },
                new StateVariable
                {
                    Name = "NumOfExections",
                    Type = StateVariable.StateType.Input,
                    Value = NumOfExections
                },
                new StateVariable
                {
                    Name = "Recordset",
                    Type = StateVariable.StateType.Input,
                    Value = Recordset
                }
            };
        }

#pragma warning disable S100 // Methods and properties should be named in camel case
#pragma warning disable IDE1006 // Naming Styles
        public Variable test { get; set; } //Suppressed Warning as this Property is serialized to the XAML and changing it's name could cause issues.
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore S100 // Methods and properties should be named in camel case
        public ActivityFunc<string, bool> DataFunc { get; set; }
        public bool FailOnFirstError { get; set; }
        public string ElementName { get; private set; }
        public string PreservedDataList { get; private set; }
        readonly Variable<string> _origInput = new Variable<string>("origInput");
        readonly Variable<string> _origOutput = new Variable<string>("origOutput");

        string _childUniqueID;
        Guid _originalUniqueID;

        public DsfForEachActivity()
        {
            DataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
            };
            DisplayName = "For Each";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(DataFunc);
            metadata.AddImplementationVariable(_origInput);
            metadata.AddImplementationVariable(_origOutput);

            base.CacheMetadata(metadata);
        }

        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            if (_originalUniqueID == Guid.Empty)
            {
                _originalUniqueID = Guid.Parse(UniqueID);
            }

            WorkSurfaceMappingId = _originalUniqueID;
            UniqueID = Guid.NewGuid().ToString();
        }

        protected override void OnBeforeExecute(NativeActivityContext context) => throw new NotImplementedException();

        protected override void OnExecute(NativeActivityContext context) => throw new NotImplementedException();

        ForEachBootstrapTO FetchExecutionType(IDSFDataObject dataObject, IExecutionEnvironment environment, out ErrorResultTO errors, int update)
        {
            AddDebug(dataObject, environment, update);

            var result = new ForEachBootstrapTO(ForEachType, From, To, CsvIndexes, NumOfExections, Recordset, environment, out errors, update);

            return result;
        }

        private void AddDebug(IDSFDataObject dataObject, IExecutionEnvironment environment, int update)
        {
            if (dataObject.IsDebugMode())
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams(ForEachType.GetDescription(), ""), debugItem);
                NumberOfExecutionsDebug(environment, update, debugItem);
                CsvDebug(environment, update, debugItem);
                InRangeDebug(environment, update, debugItem);
                InRecordsetDebug(environment, update, debugItem);
                _debugInputs.Add(debugItem);
            }
        }

        private void InRecordsetDebug(IExecutionEnvironment environment, int update, DebugItem debugItem)
        {
            if (ForEachType == enForEachType.InRecordset && !string.IsNullOrEmpty(Recordset))
            {
                AddDebugItem(new DebugEvalResult(ExecutionEnvironment.GetPositionColumnExpression(Recordset), "Recordset ", environment, update), debugItem);
            }
        }

        private void InRangeDebug(IExecutionEnvironment environment, int update, DebugItem debugItem)
        {
            if (ForEachType == enForEachType.InRange && !string.IsNullOrEmpty(From))
            {
                AddDebugItem(new DebugEvalResult(From, "From", environment, update), debugItem);
            }

            if (ForEachType == enForEachType.InRange && !string.IsNullOrEmpty(To))
            {
                AddDebugItem(new DebugEvalResult(To, "To", environment, update), debugItem);
            }
        }

        private void CsvDebug(IExecutionEnvironment environment, int update, DebugItem debugItem)
        {
            if (ForEachType == enForEachType.InCSV && !string.IsNullOrEmpty(CsvIndexes))
            {
                AddDebugItem(new DebugEvalResult(CsvIndexes, "Csv Indexes", environment, update), debugItem);
            }
        }

        private void NumberOfExecutionsDebug(IExecutionEnvironment environment, int update, DebugItem debugItem)
        {
            if (ForEachType == enForEachType.NumOfExecution && !string.IsNullOrEmpty(NumOfExections))
            {
                AddDebugItem(new DebugEvalResult(NumOfExections, "Number", environment, update), debugItem);
            }
        }

        void RestoreHandlerFn()
        {
            if (DataFunc.Handler is IDev2ActivityIOMapping activity)
            {
                activity.InputMapping = _operationalData.InnerActivity.OrigInnerInputMapping;
                activity.OutputMapping = _operationalData.InnerActivity.OrigInnerOutputMapping;
            }
            else
            {
                throw new Exception("DsfForEachActivity - RestoreHandlerFunction has encountered a null Function");
            }
        }

        ForEachInnerActivityTO GetInnerActivity(out string error)
        {
            ForEachInnerActivityTO result = null;
            error = string.Empty;

            try
            {
                if (!(DataFunc.Handler is IDev2ActivityIOMapping dev2ActivityIOMapping))
                {
                    error = ErrorResource.ForEachWithNoContentError;
                }
                else
                {
                    var tmp = dev2ActivityIOMapping;
                    result = new ForEachInnerActivityTO(tmp);
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            return result;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }

            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update) => DebugItem.EmptyList;


        public override List<string> GetOutputs() => new List<string>();

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.ForEach;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _previousParentId = dataObject.ParentInstanceID;
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            var allErrors = new ErrorResultTO();
            IIndexIterator itr = null;
            InitializeDebug(dataObject);
            dataObject.ForEachNestingLevel++;
            try
            {
                var exePayload = FetchExecutionType(dataObject, dataObject.Environment, out ErrorResultTO errors, update);

                foreach (var err in errors.FetchErrors())
                {
                    dataObject.Environment.AddError(err);
                }

                itr = exePayload.IndexIterator;

                var innerA = GetInnerActivity(out string error);
                var exeAct = innerA?.InnerActivity;
                allErrors.AddError(error);
                DispatchDebug(dataObject, StateType.Before, update);
                dataObject.ParentInstanceID = UniqueID;
                dataObject.IsDebugNested = true;
                DispatchDebug(dataObject, StateType.After, update);
                exePayload.InnerActivity = innerA;

                while (itr?.HasMore() ?? false)
                {
                    _operationalData = exePayload;
                    var idx = exePayload.IndexIterator.FetchNextIndex();
                    var innerupdate = 0;
                    innerupdate = UpdateInnerUpdate(exePayload, idx, innerupdate);
                    _childUniqueID = exeAct?.UniqueID;
                    exeAct?.Execute(dataObject, innerupdate);

                    _operationalData.IncIterationCount();
                }

                allErrors.MergeErrors(errors);
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                RestoreValues(dataObject, itr);
                var serviceTestStep = HandleServiceTestExecution(dataObject);
                dataObject.ParentInstanceID = _previousParentId;
                UniqueID = _originalUniqueID.ToString();
                dataObject.ForEachNestingLevel--;
                dataObject.IsDebugNested = false;
                HandleDebug(dataObject, serviceTestStep);
                HandleErrors(dataObject, allErrors);
                RunOnErrorSteps(dataObject, allErrors, update);
            }
        }

        private void RestoreValues(IDSFDataObject dataObject, IIndexIterator itr)
        {
            if (itr != null && ForEachType != enForEachType.NumOfExecution)
            {
                if (DataFunc.Handler != null)
                {
                    RestoreHandlerFn();
                }
            }

            if (dataObject.IsServiceTestExecution && _originalUniqueID == Guid.Empty)
            {
                _originalUniqueID = Guid.Parse(UniqueID);
            }
        }

        private IServiceTestStep HandleServiceTestExecution(IDSFDataObject dataObject)
        {
            var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.ActivityID == _originalUniqueID);
            if (dataObject.IsServiceTestExecution)
            {
                var serviceTestSteps = serviceTestStep?.Children;
                UpdateDebugStateWithAssertions(dataObject, serviceTestSteps?.ToList());
                if (serviceTestStep != null)
                {
                    var testRunResult = new TestRunResult();
                    GetFinalTestRunResult(serviceTestStep, testRunResult);
                    serviceTestStep.Result = testRunResult;
                }
            }

            return serviceTestStep;
        }

        private void HandleDebug(IDSFDataObject dataObject, IServiceTestStep serviceTestStep)
        {
            if (dataObject.IsDebugMode())
            {
                if (dataObject.IsServiceTestExecution && serviceTestStep != null)
                {
                    var debugItems = TestDebugMessageRepo.Instance.GetDebugItems(dataObject.ResourceID, dataObject.TestName);
                    debugItems = debugItems.Where(state => state.WorkSurfaceMappingId == serviceTestStep.ActivityID).ToList();
                    var debugStates = debugItems.LastOrDefault();

                    var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(serviceTestStep.Result.Message, serviceTestStep.Result.RunTestResult == RunResult.TestFailed);
                    var itemToAdd = new DebugItem();
                    itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());
                    debugStates?.AssertResultList?.Add(itemToAdd);
                }

                DispatchDebugState(dataObject, StateType.Duration, 0);
            }
        }

        private static int UpdateInnerUpdate(ForEachBootstrapTO exePayload, int idx, int innerupdate)
        {
            if (exePayload.ForEachType != enForEachType.NumOfExecution)
            {
                return idx;
            }

            return innerupdate;
        }

        private void DispatchDebug(IDSFDataObject dataObject, StateType stateType, int update)
        {
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, stateType, update);
            }
        }

        private void HandleErrors(IDSFDataObject dataObject, ErrorResultTO allErrors)
        {
            if (allErrors.HasErrors())
            {
                dataObject.ParentInstanceID = _previousParentId;
                dataObject.ForEachNestingLevel--;
                dataObject.IsDebugNested = false;
                if (!this.IsErrorHandled)
                {
                    foreach (var fetchError in allErrors.FetchErrors())
                    {
                        dataObject.Environment.AddError(fetchError);
                    }
                }
                dataObject.ParentInstanceID = _previousParentId;
                DisplayAndWriteError(dataObject, DisplayName, allErrors);
            }
        }

        public override IEnumerable<IDev2Activity> GetChildrenNodes()
        {
            if (!(DataFunc.Handler is IDev2ActivityIOMapping act))
            {
                return new List<IDev2Activity>();
            }

            var nextNodes = new List<IDev2Activity> {act};
            return nextNodes;
        }

        private static void GetFinalTestRunResult(IServiceTestStep serviceTestStep, TestRunResult testRunResult)
        {
            var nonPassingSteps = serviceTestStep.Children?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult != RunResult.TestPassed).ToList();
            if (nonPassingSteps != null && nonPassingSteps.Count == 0)
            {
                testRunResult.Message = Messages.Test_PassedResult;
                testRunResult.RunTestResult = RunResult.TestPassed;
            }
            else
            {
                if (nonPassingSteps != null)
                {
                    var failMessage = string.Join(Environment.NewLine, nonPassingSteps.Select(step => step.Result.Message));
                    testRunResult.Message = failMessage;
                }

                testRunResult.RunTestResult = RunResult.TestFailed;
            }
        }

        void UpdateDebugStateWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps)
        {
            ServiceTestHelper.UpdateDebugStateWithAssertions(dataObject, serviceTestTestSteps, _childUniqueID);
        }

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(ForEachElementName);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(ForEachElementName.Replace("*", ""));

        /// <summary>
        /// Serializes the ForEach activity to X6 JSON format using the comprehensive structure
        /// </summary>
        /// <param name="cell">The X6 cell to populate with ForEach data</param>
        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new Dictionary<string, object>();

            // Call base implementation for common properties (OnError handling, etc.)
            base.ToX6Json(cell);

            cell.shape = Constants.DSFFOREACHACTIVITY;
            // Set the activity type
            cell.data["type"] = "dsfforeachactivity";
            cell.data["displayName"] = DisplayName ?? "For Each";

            // Create comprehensive ForEach data structure matching the rich JSON format
            cell.data["forEachType"] = ForEachType.ToString();
            cell.data["forEachElementName"] = ForEachElementName ?? string.Empty;
            cell.data["from"] = From ?? string.Empty;
            cell.data["to"] = To ?? string.Empty;
            cell.data["recordset"] = Recordset ?? string.Empty;
            cell.data["csvIndexes"] = CsvIndexes ?? string.Empty;
            cell.data["numOfExecutions"] = NumOfExections ?? string.Empty;
            cell.data["failOnFirstError"] = FailOnFirstError;

            // Note: droppedNodes are no longer included here as nested activities are now handled 
            // separately as standalone nodes with nesting properties by the workflow converter
            cell.data["droppedNodes"] = new List<object>();

            // Add ngArguments for the frontend framework integration
            if (cell.id != null)
            {
                cell.data["ngArguments"] = new
                {
                    graphId = Guid.NewGuid().ToString(), // Generate a graph ID for UI purposes
                    nodeId = cell.id
                };
            }

            // Serialize the DataFunc (child activities) information for legacy support
            var dataFuncInfo = SerializeDataFunc();
            if (dataFuncInfo != null)
            {
                cell.data["dataFunc"] = dataFuncInfo;
            }
        }

        /// <summary>
        /// Deserializes the ForEach activity from X6 JSON format using the comprehensive structure
        /// </summary>
        /// <param name="cell">The X6 cell containing ForEach data</param>
        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            // Call base implementation for common properties (OnError handling, etc.)
            base.FromX6Json(cell);

            // Deserialize comprehensive ForEach data
            try
            {
                // Parse ForEachType
                if (cell.data.TryGetValue("forEachType", out var forEachTypeObj) && 
                    forEachTypeObj is string forEachTypeStr && 
                    Enum.TryParse<enForEachType>(forEachTypeStr, out var forEachType))
                {
                    ForEachType = forEachType;
                }

                // Parse other properties directly from the data object
                ForEachElementName = ExtractStringValue(cell.data, "forEachElementName");
                FromDisplayName = ForEachElementName; // Set both properties as they're linked
                From = ExtractStringValue(cell.data, "from");
                To = ExtractStringValue(cell.data, "to");
                Recordset = ExtractStringValue(cell.data, "recordset");
                CsvIndexes = ExtractStringValue(cell.data, "csvIndexes");
                NumOfExections = ExtractStringValue(cell.data, "numOfExecutions");

                // Parse boolean property
                if (cell.data.TryGetValue("failOnFirstError", out var failOnFirstErrorObj) && 
                    bool.TryParse(failOnFirstErrorObj?.ToString(), out var failOnFirstError))
                {
                    FailOnFirstError = failOnFirstError;
                }

                // Handle droppedNodes if present (for child activities)
                if (cell.data.TryGetValue("droppedNodes", out var droppedNodesObj))
                {
                    DeserializeDroppedNodes(droppedNodesObj);
                }

                // Handle ngArguments if present (UI framework data)
                if (cell.data.TryGetValue("ngArguments", out var ngArgumentsObj))
                {
                    // Store for potential UI integration needs
                    // This typically doesn't affect the core activity logic
                }

                // Deserialize DataFunc if present (legacy support)
                if (cell.data.TryGetValue("dataFunc", out var dataFuncObj))
                {
                    DeserializeDataFunc(dataFuncObj);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - graceful degradation
                Dev2Logger.Error($"Error deserializing ForEach data from comprehensive X6 JSON: {ex.Message}", ex, GlobalConstants.WarewolfError);
            }
        }

        /// <summary>
        /// Deserializes dropped nodes and sets up the DataFunc.Handler property
        /// </summary>
        /// <param name="droppedNodesObj">The droppedNodes data from the X6 cell</param>
        private void DeserializeDroppedNodes(object droppedNodesObj)
        {
            try
            {
            // Handle different possible formats of droppedNodes
            List<object> droppedNodesList = null;
                
            if (droppedNodesObj is JArray jArray)
            {
                // Handle JArray from JSON deserialization
                droppedNodesList = jArray.ToObject<List<object>>();
            }
            else if (droppedNodesObj != null)
            {
                // Try to serialize and deserialize as a fallback
                var json = JsonConvert.SerializeObject(droppedNodesObj);
                droppedNodesList = JsonConvert.DeserializeObject<List<object>>(json);
            }

            // Process the first dropped node (ForEach should only contain one or no child activities)
            if (droppedNodesList != null && droppedNodesList.Count > 0)
            {
                var firstDroppedNode = droppedNodesList[0];
                var childActivity = CreateActivityFromDroppedNode(firstDroppedNode);
                    
                if (childActivity != null)
                {
                    // Set up the ActivityFunc with the deserialized child activity
                    if (DataFunc == null)
                    {
                        DataFunc = new ActivityFunc<string, bool>
                        {
                            DisplayName = "Data Action",
                            Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
                        };
                    }
                        
                    DataFunc.Handler = childActivity;
                }
            }
        }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error deserializing droppedNodes: {ex.Message}", ex, GlobalConstants.WarewolfError);
            }
        }

        /// <summary>
        /// Creates an activity from a dropped node object
        /// </summary>
        /// <param name="droppedNodeObj">The dropped node data</param>
        /// <returns>The created activity or null</returns>
        private Activity CreateActivityFromDroppedNode(object droppedNodeObj)
        {
            try
            {
                // Convert the dropped node object to a Cell for processing
                Cell droppedNodeCell = null;
                
                if (droppedNodeObj is string droppedNodeJson)
                {
                    // If it's a JSON string, deserialize it to a Cell
                    droppedNodeCell = JsonConvert.DeserializeObject<Cell>(droppedNodeJson);
                }
                else
                {
                    // If it's already an object, try to convert it to a Cell
                    var json = JsonConvert.SerializeObject(droppedNodeObj);
                    droppedNodeCell = JsonConvert.DeserializeObject<Cell>(json);
                }

                if (droppedNodeCell?.data == null)
                {
                    return null;
                }

                // Use similar logic to X6ToWorkflowConverter.CreateActivityFromNode
                if (!droppedNodeCell.data.TryGetValue("type", out var typeObj) || 
                    typeObj is not string type || 
                    string.IsNullOrWhiteSpace(type))
                {
                    return null;
                }

                var nodeType = type.ToLowerInvariant();
                
                // Create activities based on type
                if (nodeType.Contains("dsfdotnetmultiassignactivity") || nodeType.Contains("assigntool"))
                {
                    return CreateAssignActivityFromDroppedNode(droppedNodeCell);
                }
                else if (nodeType.Contains("flowdecision"))
                {
                    return CreateFlowDecisionActivityFromDroppedNode(droppedNodeCell);
                }
                else if (nodeType.Contains("dsfdecision"))
                {
                    return CreateDecisionActivityFromDroppedNode(droppedNodeCell);
                }
                else if (nodeType.Contains("dsfflowswitchactivity") || nodeType.Contains("flowswitch"))
                {
                    return CreateSwitchActivityFromDroppedNode(droppedNodeCell);
                }
                else if (nodeType.Contains("dsfforeachactivity") || nodeType.Contains("foreach"))
                {
                    return CreateForEachActivityFromDroppedNode(droppedNodeCell);
                }
                else
                {
                    // For unknown types, create a comment activity as a fallback
                    return new DsfCommentActivity { Text = $"Unknown activity type: {type}" };
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error creating activity from dropped node: {ex.Message}", ex, GlobalConstants.WarewolfError);
                return null;
            }
        }

        /// <summary>
        /// Creates a DsfDotNetMultiAssignActivity from a dropped node
        /// </summary>
        /// <param name="droppedNodeCell">The dropped node cell</param>
        /// <returns>DsfDotNetMultiAssignActivity instance</returns>
        private static DsfDotNetMultiAssignActivity CreateAssignActivityFromDroppedNode(Cell droppedNodeCell)
        {
            var activity = new DsfDotNetMultiAssignActivity();
            activity.FromX6Json(droppedNodeCell);
            return activity;
        }

        /// <summary>
        /// Creates a DsfFlowDecisionActivity from a dropped node
        /// </summary>
        /// <param name="droppedNodeCell">The dropped node cell</param>
        /// <returns>DsfFlowDecisionActivity instance</returns>
        private static DsfFlowDecisionActivity CreateFlowDecisionActivityFromDroppedNode(Cell droppedNodeCell)
        {
            var activity = new DsfFlowDecisionActivity();
            activity.FromX6Json(droppedNodeCell);
            return activity;
        }

        /// <summary>
        /// Creates a DsfDecision from a dropped node
        /// </summary>
        /// <param name="droppedNodeCell">The dropped node cell</param>
        /// <returns>DsfDecision instance</returns>
        private static DsfDecision CreateDecisionActivityFromDroppedNode(Cell droppedNodeCell)
        {
            var activity = new DsfDecision();
            activity.FromX6Json(droppedNodeCell);
            return activity;
        }

        /// <summary>
        /// Creates a DsfFlowSwitchActivity from a dropped node
        /// </summary>
        /// <param name="droppedNodeCell">The dropped node cell</param>
        /// <returns>DsfFlowSwitchActivity instance</returns>
        private static DsfFlowSwitchActivity CreateSwitchActivityFromDroppedNode(Cell droppedNodeCell)
        {
            var activity = new DsfFlowSwitchActivity();
            activity.FromX6Json(droppedNodeCell);
            return activity;
        }

        /// <summary>
        /// Creates a DsfForEachActivity from a dropped node
        /// </summary>
        /// <param name="droppedNodeCell">The dropped node cell</param>
        /// <returns>DsfForEachActivity instance</returns>
        private static DsfForEachActivity CreateForEachActivityFromDroppedNode(Cell droppedNodeCell)
        {
            var activity = new DsfForEachActivity();
            activity.FromX6Json(droppedNodeCell);
            return activity;
        }

        /// <summary>
        /// Helper method to safely extract string values from the data dictionary
        /// </summary>
        /// <param name="data">The data dictionary</param>
        /// <param name="key">The key to extract</param>
        /// <returns>String value or empty string if not found</returns>
        private static string ExtractStringValue(Dictionary<string, object> data, string key)
        {
            return data.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;
        }

        /// <summary>
        /// Serializes the DataFunc (child activity) to a JSON-friendly format
        /// </summary>
        /// <returns>Serialized DataFunc data</returns>
        private object SerializeDataFunc()
        {
            if (DataFunc?.Handler == null)
            {
                return null;
            }

            try
            {
                return new
                {
                    displayName = DataFunc.DisplayName ?? "Data Action",
                    argumentName = DataFunc.Argument?.Name ?? string.Empty,
                    handlerType = DataFunc.Handler.GetType().Name,
                    handlerUniqueId = (DataFunc.Handler as IDev2Activity)?.UniqueID ?? string.Empty,
                    handlerDisplayName = (DataFunc.Handler as Activity)?.DisplayName ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error serializing DataFunc: {ex.Message}", ex, GlobalConstants.WarewolfError);
                return null;
            }
        }

        /// <summary>
        /// Deserializes the DataFunc (child activity) from JSON format
        /// </summary>
        /// <param name="dataFuncData">The serialized DataFunc data</param>
        private void DeserializeDataFunc(dynamic dataFuncData)
        {
            if (dataFuncData == null) return;

            try
            {
                // Note: For full deserialization of child activities, we would need access to the 
                // activity factory and the complete activity definition. For now, we preserve
                // the basic structure and properties that can be restored.
                
                if (DataFunc == null)
                {
                    DataFunc = new ActivityFunc<string, bool>();
                }

                // Restore basic properties
                if (dataFuncData.displayName != null)
                {
                    DataFunc.DisplayName = dataFuncData.displayName.ToString();
                }

                if (dataFuncData.argumentName != null && DataFunc.Argument != null)
                {
                    // Note: Argument name is typically auto-generated and may not need restoration
                    // but we preserve it for consistency
                }

                // The actual Handler restoration would require more complex logic involving
                // activity factories and full activity serialization/deserialization
                // This is typically handled at a higher level during workflow reconstruction
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Error deserializing DataFunc: {ex.Message}", ex, GlobalConstants.WarewolfError);
            }
        }

        public bool Equals(DsfForEachActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var activityFuncComparer = new ActivityFuncComparer();
            var equals = base.Equals(other);
            equals &= activityFuncComparer.Equals(DataFunc, other.DataFunc);
            equals &= string.Equals(ForEachElementName, other.ForEachElementName);
            equals &= string.Equals(DisplayName, other.DisplayName);
            equals &= ForEachType == other.ForEachType;
            equals &= string.Equals(From, other.From);
            equals &= string.Equals(To, other.To);
            equals &= string.Equals(Recordset, other.Recordset);
            equals &= string.Equals(CsvIndexes, other.CsvIndexes);
            equals &= string.Equals(NumOfExections, other.NumOfExections);
            equals &= Equals(test, other.test);
            equals &= FailOnFirstError == other.FailOnFirstError;
            equals &= string.Equals(ElementName, other.ElementName);
            return equals;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((DsfForEachActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _previousInputsIndex;
                hashCode = (hashCode * 397) ^ _previousOutputsIndex;
                hashCode = (hashCode * 397) ^ (int) ForEachType;
                hashCode = (hashCode * 397) ^ (From != null ? From.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (To != null ? To.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Recordset != null ? Recordset.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CsvIndexes != null ? CsvIndexes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NumOfExections != null ? NumOfExections.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DataFunc != null ? DataFunc.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}