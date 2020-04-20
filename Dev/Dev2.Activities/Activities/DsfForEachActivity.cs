#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Comparer;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Util;
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

        [FindMissing]
        public string From { get; set; }

        [FindMissing]
        public string To { get; set; }

        [FindMissing]
        public string Recordset { get; set; }

        [FindMissing]
        public string CsvIndexes { get; set; }

        [FindMissing]
        public string NumOfExections { get; set; }

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
        public string ElementName { private set; get; }
        public string PreservedDataList { private set; get; }
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
                Dev2Logger.Error("DSFForEach", e, GlobalConstants.WarewolfError);
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
            }
        }

        private void RestoreValues(IDSFDataObject dataObject, IIndexIterator itr)
        {
            if (itr != null && ForEachType != enForEachType.NumOfExecution)
            {
                RestoreHandlerFn();
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
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfForEachActivity", allErrors);
                    foreach (var fetchError in allErrors.FetchErrors())
                    {
                        dataObject.Environment.AddError(fetchError);
                    }

                    dataObject.ParentInstanceID = _previousParentId;
                }
            }
        }

        public override IEnumerable<IDev2Activity> GetChildrenNodes()
        {
            if (!(DataFunc.Handler is IDev2ActivityIOMapping act))
            {
                return new List<IDev2Activity>();
            }
            var nextNodes = new List<IDev2Activity> { act };
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

            return Equals((DsfForEachActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _previousInputsIndex;
                hashCode = (hashCode * 397) ^ _previousOutputsIndex;
                hashCode = (hashCode * 397) ^ (int)ForEachType;
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
