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
    [ToolDescriptorInfo("Execution-ForEach", "ForEach", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Loop Constructs", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_LoopConstruct_For Each")]
    public class DsfForEachActivity : DsfActivityAbstract<bool>,IEquatable<DsfForEachActivity>
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
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")

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
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentId = dataObject.ParentInstanceID;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            lock (_forEachExecutionObject)
            {
                IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

                _debugInputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();



                dataObject.ForEachNestingLevel++;
                ErrorResultTO allErrors = new ErrorResultTO();

                InitializeDebug(dataObject);
                try
                {
                    ForEachBootstrapTO exePayload = FetchExecutionType(dataObject, dataObject.Environment, out ErrorResultTO errors, 0);

                    if (errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                        return;
                    }

                    if (dataObject.IsDebugMode())
                    {
                        DispatchDebugState(dataObject, StateType.Before, 0);
                    }

                    dataObject.ParentInstanceID = UniqueID;

                    allErrors.MergeErrors(errors);
                    ForEachInnerActivityTO innerA = GetInnerActivity(out string error);
                    allErrors.AddError(error);

                    exePayload.InnerActivity = innerA;

                    operationalData = exePayload;
                    // flag it as scoped so we can use a single DataList
                    dataObject.IsDataListScoped = true;
                    dataObject.IsDebugNested = true;

                    if (exePayload.InnerActivity != null && exePayload.IndexIterator.HasMore())
                    {
                        int idx = exePayload.IndexIterator.FetchNextIndex();
                        if (exePayload.ForEachType != enForEachType.NumOfExecution)
                        {
                            IterateIOMapping(idx);
                        }
                        else
                        {
                            dataObject.IsDataListScoped = false;
                        }

                        // schedule the func to execute ;)
                        dataObject.ParentInstanceID = UniqueID;

                        context.ScheduleFunc(DataFunc, string.Empty, ActivityCompleted);
                    }

                }
                catch (Exception e)
                {
                    Dev2Logger.Error("DSFForEach", e, GlobalConstants.WarewolfError);
                    allErrors.AddError(e.Message);
                }
                finally
                {
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
                    if (dataObject.IsDebugMode())
                    {
                        DispatchDebugState(dataObject, StateType.After, 0);
                    }
                }
            }
        }


        /// <summary>
        /// Iterates the IO mapping.
        /// </summary>
        private void IterateIOMapping(int idx)
        {
            string newInputs = string.Empty;

            string newOutputs = string.Empty;
            bool updateInputToken = false;
            bool updateOutputToken = false;

            // Now mutate the mappings ;)
            //Bug 8725 do not mutate mappings
            if (!string.IsNullOrEmpty(operationalData.InnerActivity.OrigInnerInputMapping))
            {
                // (*) == ({idx}) ;)
                newInputs = operationalData.InnerActivity.OrigInnerInputMapping;
                newInputs = _inputItr.IterateMapping(newInputs, idx);
                newInputs = newInputs.Replace("(*)", "(" + idx + ")");
            }
            else
            {
                // coded activity

                #region Coded Activity IO ManIP

                var tmp = operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>;

                if (_previousInputsIndex != -1)
                {
                    if (_inputsToken != "*")
                    {
                        _inputsToken = _previousInputsIndex.ToString(CultureInfo.InvariantCulture);
                    }
                }

                if (_previousOutputsIndex != -1)
                {
                    if (_outputsToken != "*")
                    {
                        _outputsToken = _previousOutputsIndex.ToString(CultureInfo.InvariantCulture);
                    }
                }

                if (tmp != null)
                {
                    IList<DsfForEachItem> data = tmp.GetForEachInputs();
                    IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                    if (AmendInputs(idx, data, _inputsToken, updates))
                    {
                        updateInputToken = true;
                    }

                    // push updates for Inputs
                    tmp.UpdateForEachInputs(updates);
                    if (idx == 1)
                    {
                        operationalData.InnerActivity.OrigCodedInputs = updates;
                    }

                    operationalData.InnerActivity.CurCodedInputs = updates;

                    // Process outputs
                    data = tmp.GetForEachOutputs();
                    updates = new List<Tuple<string, string>>();

                    if (AmendOutputs(idx, data, _outputsToken, updates))
                    {
                        updateOutputToken = true;
                    }

                    // push updates 
                    tmp.UpdateForEachOutputs(updates);
                    if (idx == 1)
                    {
                        operationalData.InnerActivity.OrigCodedOutputs = updates;
                    }

                    operationalData.InnerActivity.CurCodedOutputs = updates;
                }
                else
                {

                    if (operationalData.InnerActivity.InnerActivity is DsfActivityAbstract<bool> tmp2 && !(tmp2 is DsfForEachActivity))
                    {
                        IList<DsfForEachItem> data = tmp2.GetForEachInputs();
                        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                        if (AmendInputs(idx, data, _inputsToken, updates))
                        {
                            updateInputToken = true;
                        }

                        // push updates 
                        tmp2.UpdateForEachInputs(updates);
                        if (idx == 1)
                        {
                            operationalData.InnerActivity.OrigCodedInputs = updates;
                        }
                        operationalData.InnerActivity.CurCodedInputs = updates;

                        // Process outputs
                        data = tmp2.GetForEachOutputs();
                        updates = new List<Tuple<string, string>>();

                        if (AmendOutputs(idx, data, _outputsToken, updates))
                        {
                            updateOutputToken = true;
                        }

                        // push updates 
                        tmp2.UpdateForEachOutputs(updates);
                        if (idx == 1)
                        {
                            operationalData.InnerActivity.OrigCodedOutputs = updates;
                        }

                        operationalData.InnerActivity.CurCodedOutputs = updates;
                    }
                }

                #endregion
            }

            //Bug 8725 do not mutate mappings
            if (operationalData.InnerActivity.OrigInnerOutputMapping != null)
            {
                // (*) == ({idx}) ;)
                newOutputs = operationalData.InnerActivity.OrigInnerOutputMapping;
                newOutputs = _inputItr.IterateMapping(newOutputs, idx);
            }

            if (DataFunc.Handler is IDev2ActivityIOMapping dev2ActivityIoMapping)
            {
                dev2ActivityIoMapping.InputMapping = newInputs;
            }

            if (DataFunc.Handler is IDev2ActivityIOMapping activityIoMapping)
            {
                activityIoMapping.OutputMapping = newOutputs;
            }
            if (updateInputToken)
            {
                _inputsToken = idx.ToString(CultureInfo.InvariantCulture);
            }
            if (updateOutputToken)
            {
                _outputsToken = idx.ToString(CultureInfo.InvariantCulture);
            }
        }

        static bool AmendInputs(int idx, IEnumerable<DsfForEachItem> data, string token, IList<Tuple<string, string>> updates)
        {
            bool result = false;
            // amend inputs ;)
            foreach (DsfForEachItem d in data)
            {
                string input = d.Value;
                if (input.Contains("(" + token + ")"))
                {
                    input = input.Replace("(" + token + ")", "(" + idx + ")");
                    result = true;
                }

                if (!string.IsNullOrEmpty(d.Value))
                {
                    updates.Add(new Tuple<string, string>(d.Value, input));
                }
            }
            return result;
        }

        static bool AmendOutputs(int idx, IEnumerable<DsfForEachItem> data, string token, IList<Tuple<string, string>> updates)
        {
            bool result = false;
            // amend inputs ;)
            foreach (DsfForEachItem d in data)
            {
                string input = d.Value;
                if (!string.IsNullOrEmpty(input))
                {
                    if (input.Contains("(" + token + ")"))
                    {
                        input = input.Replace("(" + token + ")", "(" + idx + ")");
                        result = true;
                    }

                    if (!string.IsNullOrEmpty(d.Value))
                    {
                        updates.Add(new Tuple<string, string>(d.Value, input));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Fetches the type of the execution.
        /// </summary>        
        /// <param name="dataObject">The data object.</param>
        /// <param name="environment"></param>
        /// <param name="errors">The errors.</param>
        /// <param name="update"></param>
        /// <returns></returns>                
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

        /// <summary>
        /// Restores the handler fn.
        /// </summary>
        private void RestoreHandlerFn()
        {


            if (DataFunc.Handler is IDev2ActivityIOMapping activity)
            {

                if (operationalData.InnerActivity.OrigCodedInputs != null)
                {

                    //MO - CHANGE:This is to be reinstated for restoring actives back to state with star
                    #region Coded Activity
                    var tmp = operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>;


                    // this is wrong, we need the last index ;)

                    int idx = operationalData.IterationCount;

                    //Handle csv and range differently ;)
                    if (ForEachType == enForEachType.InCSV || ForEachType == enForEachType.InRange)
                    {
                        Int32.TryParse(_inputsToken, out idx);
                    }

                    if (tmp != null)
                    {
                        // Restore Inputs ;)
                        IList<DsfForEachItem> data = tmp.GetForEachInputs();
                        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach (DsfForEachItem d in data)
                        {
                            string input = d.Value;
                            input = input.Replace("(" + idx + ")", "(*)");

                            updates.Add(new Tuple<string, string>(d.Value, input));
                        }

                        // push updates for Inputs
                        tmp.UpdateForEachInputs(updates);


                        // Restore Outputs ;)
                        data = tmp.GetForEachOutputs();
                        updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach (DsfForEachItem d in data)
                        {
                            string input = d.Value;
                            input = input.Replace("(" + idx + ")", "(*)");

                            updates.Add(new Tuple<string, string>(d.Value, input));
                        }

                        // push updates for Inputs
                        tmp.UpdateForEachOutputs(updates);

                    }
                    else
                    {

                        // Restore Inputs ;)
                        if (operationalData.InnerActivity.InnerActivity is DsfActivityAbstract<bool> tmp2)
                        {
                            IList<DsfForEachItem> data = tmp2.GetForEachInputs();
                            IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                            // amend inputs ;)
                            foreach (DsfForEachItem d in data)
                            {
                                string input = d.Value;
                                input = input.Replace("(" + idx + ")", "(*)");

                                updates.Add(new Tuple<string, string>(d.Value, input));
                            }

                            // push updates for Inputs
                            tmp2.UpdateForEachInputs(updates);


                            // Restore Outputs ;)
                            data = tmp2.GetForEachInputs();
                            updates = new List<Tuple<string, string>>();

                            // amend inputs ;)
                            foreach (DsfForEachItem d in data)
                            {
                                string input = d.Value;
                                input = input.Replace("(" + idx + ")", "(*)");

                                updates.Add(new Tuple<string, string>(d.Value, input));
                            }

                            // push updates for Inputs
                            tmp2.UpdateForEachOutputs(updates);
                        }
                    }
                    #endregion
                }
                else
                {
                    activity.InputMapping = operationalData.InnerActivity.OrigInnerInputMapping;
                    activity.OutputMapping = operationalData.InnerActivity.OrigInnerOutputMapping;
                }
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

        private void ActivityCompleted(NativeActivityContext context, ActivityInstance instance, bool result)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            if (dataObject != null && operationalData != null)
            {



                if (operationalData.IndexIterator.HasMore())
                {
                    var idx = operationalData.IndexIterator.FetchNextIndex();
                    // Re-jigger the mapping ;)
                    if (operationalData.ForEachType != enForEachType.NumOfExecution)
                    {
                        IterateIOMapping(idx);
                    }
                    dataObject.ParentInstanceID = UniqueID;
                    
                    context.ScheduleFunc(DataFunc, UniqueID, ActivityCompleted);
                    
                    return;
                }

                // that is all she wrote ;)
                dataObject.IsDataListScoped = false;
                // return it all to normal
                if (ForEachType != enForEachType.NumOfExecution)
                {
                    RestoreHandlerFn();
                }

                dataObject.ParentInstanceID = _previousParentId;
                dataObject.ForEachNestingLevel--;
                dataObject.IsDebugNested = false;
            }
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
                ForEachBootstrapTO exePayload = FetchExecutionType(dataObject, dataObject.Environment, out var errors, update);

                foreach (var err in errors.FetchErrors())
                {
                    dataObject.Environment.AddError(err);
                }
                itr = exePayload.IndexIterator;

                ForEachInnerActivityTO innerA = GetInnerActivity(out var error);
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

        public bool Equals(DsfForEachActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var activityFuncComparer = new ActivityFuncComparer();
            var equals = activityFuncComparer.Equals(DataFunc, other.DataFunc);
            return base.Equals(other)
                && string.Equals(ForEachElementName, other.ForEachElementName)
                && string.Equals(DisplayName, other.DisplayName)
                && ForEachType == other.ForEachType 
                && string.Equals(From, other.From)
                && string.Equals(To, other.To) 
                && string.Equals(Recordset, other.Recordset)
                && string.Equals(CsvIndexes, other.CsvIndexes)
                && string.Equals(NumOfExections, other.NumOfExections)
                && Equals(test, other.test)
                && @equals
                && FailOnFirstError == other.FailOnFirstError 
                && string.Equals(ElementName, other.ElementName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfForEachActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (ForEachElementName != null ? ForEachElementName.GetHashCode() : 0);
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
                hashCode = (hashCode * 397) ^ FailOnFirstError.GetHashCode();
                hashCode = (hashCode * 397) ^ (ElementName != null ? ElementName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PreservedDataList != null ? PreservedDataList.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
