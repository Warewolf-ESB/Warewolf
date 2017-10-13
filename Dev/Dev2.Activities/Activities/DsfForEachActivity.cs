/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
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
    [ToolDescriptorInfo("Execution-ForEach", "ForEach", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Loop Constructs", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_LoopConstruct_For Each")]
    public class DsfForEachActivity : DsfActivityAbstract<bool>
    {
        string _previousParentId;
        readonly Dev2ActivityIOIteration _inputItr = new Dev2ActivityIOIteration();
        
        #region Variables

        private string _forEachElementName;
        private string _displayName;
        
        readonly int _previousInputsIndex = -1;

        readonly int _previousOutputsIndex = -1;
        
        private string _inputsToken = "*";
        private string _outputsToken = "*";

        
        private ForEachBootstrapTO operationalData;


        #endregion Variables

        #region Properties

        
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
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                ForEachElementName = value;
            }
        }

        [Inputs("ForEachElementName")]
        [FindMissing]
        
        public string ForEachElementName
        
        {
            get
            {
                return _forEachElementName;
            }
            set
            {
                _forEachElementName = value;
            }
        }

        
        public int ExecutionCount
        
        {
            get
            {
                if (operationalData != null)
                {
                    return operationalData.IterationCount;
                }

                return 0;
            }
        }
        
        
        
        public Variable test { get; set; }
        

        public ActivityFunc<string, bool> DataFunc { get; set; }

        
        public bool FailOnFirstError { get; set; }
        
        
        
        public string ElementName { private set; get; }
        
        
        public string PreservedDataList { private set; get; }
        
        

        
#pragma warning disable 169
        
        private readonly List<bool> _results = new List<bool>();
        readonly

#pragma warning restore 169

                // REMOVE : No longer used
#pragma warning disable 169

                DelegateInArgument<string> _actionArgument = new DelegateInArgument<string>("explicitDataFromParent");
        
#pragma warning restore 169

        // used to avoid IO mapping adjustment issues ;)
        // REMOVE : 2 variables below not used any more.....

        
        private readonly Variable<string> _origInput = new Variable<string>("origInput");
        private readonly Variable<string> _origOutput = new Variable<string>("origOutput");
        
        readonly object _forEachExecutionObject = new object();
        private string _childUniqueID;
        private Guid _originalUniqueID;

        #endregion Properties

        #region Ctor

        public DsfForEachActivity()
        {
            DataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>(string.Format("explicitData_{0}", DateTime.Now.ToString("yyyyMMddhhmmss")))

            };
            DisplayName = "For Each";
        }

        #endregion Ctor

        #region CacheMetaData

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(DataFunc);
            metadata.AddImplementationVariable(_origInput);
            metadata.AddImplementationVariable(_origOutput);

            base.CacheMetadata(metadata);
        }

        #endregion CacheMetaData

        #region Execute

        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            var isNestedForEach = dataObject.ForEachNestingLevel > 0;
            if (!isNestedForEach || _originalUniqueID == Guid.Empty)
            {
                _originalUniqueID = Guid.Parse(UniqueID);
            }
            if (!isNestedForEach && _originalUniqueID != Guid.Empty)
            {
                UniqueID = _originalUniqueID.ToString();
            }
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            UniqueID = isNestedForEach ? Guid.NewGuid().ToString() : UniqueID;
        }


        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
        
        private void IterateIOMapping(int idx)
        {
            throw new NotImplementedException();
        }
      
        private ForEachBootstrapTO FetchExecutionType(IDSFDataObject dataObject, IExecutionEnvironment environment, out ErrorResultTO errors, int update)
        {
            if (dataObject.IsDebugMode())
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugItemStaticDataParams(ForEachType.GetDescription(), ""), debugItem);
                if (ForEachType == enForEachType.NumOfExecution && !string.IsNullOrEmpty(NumOfExections))
                {


                    AddDebugItem(new DebugEvalResult(NumOfExections, "Number", environment, update), debugItem);
                }
                if (ForEachType == enForEachType.InCSV && !string.IsNullOrEmpty(CsvIndexes))
                {
                    AddDebugItem(new DebugEvalResult(CsvIndexes, "Csv Indexes", environment, update), debugItem);

                }
                if (ForEachType == enForEachType.InRange && !string.IsNullOrEmpty(From))
                {
                    AddDebugItem(new DebugEvalResult(From, "From", environment, update), debugItem);

                }
                if (ForEachType == enForEachType.InRange && !string.IsNullOrEmpty(To))
                {

                    AddDebugItem(new DebugEvalResult(To, "To", environment, update), debugItem);

                }
                if (ForEachType == enForEachType.InRecordset && !string.IsNullOrEmpty(Recordset))
                {


                    AddDebugItem(new DebugEvalResult(ExecutionEnvironment.GetPositionColumnExpression(Recordset), "Recordset ", environment, update), debugItem);
                }
                _debugInputs.Add(debugItem);
            }

            var result = new ForEachBootstrapTO(ForEachType, From, To, CsvIndexes, NumOfExections, Recordset, environment, out errors, update);

            return result;

        }
        
        private void RestoreHandlerFn()
        {
            if (DataFunc.Handler is IDev2ActivityIOMapping activity)
            {
                activity.InputMapping = operationalData.InnerActivity.OrigInnerInputMapping;
                activity.OutputMapping = operationalData.InnerActivity.OrigInnerOutputMapping;
            }
            else
            {
                throw new Exception("DsfForEachActivity - RestoreHandlerFunction has encountered a null Function");
            }
            _inputsToken = "*";
            _outputsToken = "*";
        }

        private ForEachInnerActivityTO GetInnerActivity(out string error)
        {
            ForEachInnerActivityTO result = null;
            error = string.Empty;

            try
            {
                var dev2ActivityIOMapping = DataFunc.Handler as IDev2ActivityIOMapping;


                if (dev2ActivityIOMapping == null)
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

        #endregion Execute

        #region Private Methods

        #endregion Private Methodss

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            return DebugItem.EmptyList;
        }

        #endregion Get Inputs/Outputs


        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.ForEach;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _previousParentId = dataObject.ParentInstanceID;
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            ErrorResultTO allErrors = new ErrorResultTO();
            IIndexIterator itr = null;
            InitializeDebug(dataObject);
            dataObject.ForEachNestingLevel++;
            try
            {
                ForEachBootstrapTO exePayload = FetchExecutionType(dataObject, dataObject.Environment, out ErrorResultTO errors, update);

                foreach (var err in errors.FetchErrors())
                {
                    dataObject.Environment.AddError(err);
                }
                itr = exePayload.IndexIterator;

                ForEachInnerActivityTO innerA = GetInnerActivity(out string error);
                var exeAct = innerA?.InnerActivity;
                allErrors.AddError(error);
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                }
                dataObject.ParentInstanceID = UniqueID;
                dataObject.IsDebugNested = true;
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.After, update);
                }
                exePayload.InnerActivity = innerA;

                while (itr?.HasMore() ?? false)
                {
                    operationalData = exePayload;
                    int idx = exePayload.IndexIterator.FetchNextIndex();
                    int innerupdate = 0;
                    if (exePayload.ForEachType != enForEachType.NumOfExecution)
                    {
                        innerupdate = idx;
                    }
                    _childUniqueID = exeAct?.UniqueID;
                    exeAct?.Execute(dataObject, innerupdate);

                    operationalData.IncIterationCount();
                }
                if (errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFForEach", e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                if (itr != null)
                {
                    if (ForEachType != enForEachType.NumOfExecution)
                    {
                        RestoreHandlerFn();
                    }
                }
                if (dataObject.IsServiceTestExecution)
                {
                    if (_originalUniqueID == Guid.Empty)
                    {
                        _originalUniqueID = Guid.Parse(UniqueID);
                    }
                }
                var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == _originalUniqueID);
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
                dataObject.ParentInstanceID = _previousParentId;
                dataObject.ForEachNestingLevel--;
                dataObject.IsDebugNested = false;
                if (dataObject.IsDebugMode())
                {
                    if (dataObject.IsServiceTestExecution && serviceTestStep != null)
                    {
                        var debugItems = TestDebugMessageRepo.Instance.GetDebugItems(dataObject.ResourceID, dataObject.TestName);
                        debugItems = debugItems.Where(state => state.WorkSurfaceMappingId == serviceTestStep.UniqueId).ToList();
                        var debugStates = debugItems.LastOrDefault();

                        var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(serviceTestStep.Result.Message, serviceTestStep.Result.RunTestResult == RunResult.TestFailed);
                        DebugItem itemToAdd = new DebugItem();
                        itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());
                        debugStates?.AssertResultList?.Add(itemToAdd);
                    }
                    DispatchDebugState(dataObject, StateType.Duration, 0);
                }
                // Handle Errors
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

        private void UpdateDebugStateWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps)
        {
            ServiceTestHelper.UpdateDebugStateWithAssertions(dataObject, serviceTestTestSteps, _childUniqueID);
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(ForEachElementName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(ForEachElementName.Replace("*", ""));
        }

        #endregion

    }
}
