using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;
using Unlimited.Framework;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    public class DsfForEachActivity : DsfActivityAbstract<bool>
    {
        string _previousParentID;
        Dev2ActivityIOIteration inputItr = new Dev2ActivityIOIteration();
        #region Variables

        private IList<IDebugItem> _debugInputs = new List<IDebugItem>();
        private string _forEachElementName;
        private string _displayName;

        // ReSharper disable InconsistentNaming
        private ForEachBootstrapTO operationalData;
        // ReSharper restore InconsistentNaming

        #endregion Variables

        #region Properties

        [Inputs("FromDisplayName")]
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

        public int ExecutionCount { 
            get
            {
                if(operationalData != null)
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

        // REMOVE : Travis.Frisinger - 28.11.2012 : The two variables below are no longer required
        private Variable<IEnumerator<UnlimitedObject>> dataTags = new Variable<IEnumerator<UnlimitedObject>>("dataTags");
        private Variable<UnlimitedObject> inputData = new Variable<UnlimitedObject>("inputData");
        private List<bool> results = new List<bool>();

        // REMOVE : No longer used
        DelegateInArgument<string> actionArgument = new DelegateInArgument<string>("explicitDataFromParent");

        // used to avoid IO mapping adjustment issues ;)
        // REMOVE : 2 variables below not used any more.....
        private Variable<string> _origInput = new Variable<string>("origInput");
        private Variable<string> _origOutput = new Variable<string>("origOutput");


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
            metadata.AddImplementationVariable(dataTags);
            metadata.AddImplementationVariable(inputData);
            metadata.AddImplementationVariable(_origInput);
            metadata.AddImplementationVariable(_origOutput);

            base.CacheMetadata(metadata);
        }

        #endregion CacheMetaData

        #region Execute

        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentID = dataObject.ParentInstanceID;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<IDebugItem>();            
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            dataObject.ParentInstanceID = InstanceID;

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionID = DataListExecutionID.Get(context);

            try
            {
                string elmName = ForEachElementName;
                if(dataObject.IsDebug)
                {
                    IBinaryDataListEntry tmpEntry = compiler.Evaluate(executionID, enActionType.User, ForEachElementName, false, out errors);
                    AddDebugInputItem(ForEachElementName, string.Empty, tmpEntry, executionID);
                    DispatchDebugState(context,StateType.Before);
                }

                ForEachBootstrapTO exePayload = FetchExecutionType(elmName, executionID, compiler, out errors);
                allErrors.MergeErrors(errors);
                string error;
                ForEachInnerActivityTO innerA = GetInnerActivity(out error);
                allErrors.AddError(error);
                exePayload.InnerActivity = innerA;

                operationalData = exePayload;
                // flag it as scoped so we can use a single DataList
                dataObject.IsDataListScoped = true;

                if (exePayload.MaxExecutions > 0)
                {

                    // set the iteration data ;)
                    IterateIOMapping(1);

                    // schedule the func to execute ;)
                    // ReSharper disable RedundantTypeArgumentsOfMethod
                    dataObject.ParentInstanceID = InstanceID;
                    dataObject.IsDataListScoped = true; // set for ForEach execution ;)

                    context.ScheduleFunc<string, bool>(DataFunc, string.Empty, ActivityCompleted);
                    // ReSharper restore RedundantTypeArgumentsOfMethod
                }

                /*
                 * 1. Extract IO Mapping if Activity
                 * 2. Extract Wizard Mapping if Coded Activity
                 * 3. Build a new DataList per execution
                 * 4. Delete DataList after execution
                 * 5. 
                 */

            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfForEachActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebug)
                {
                    DispatchDebugState(context, StateType.After);
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

            // Now mutate the mappings ;)
            //Bug 8725 do not mutate mappings
            if (operationalData.InnerActivity.OrigInnerInputMapping != null)
            {
                // (*) == ({idx}) ;)
                newInputs = operationalData.InnerActivity.OrigInnerInputMapping;
                newInputs = inputItr.IterateMapping(newInputs, idx);
                //newInputs = newInputs.Replace("(*)", "(" + idx + ")");
            }
            else
            {
                // coded activity

                #region Coded Activity IO ManIP

                //var tmp = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>);

                //string token = "*";

                //if (idx > 1)
                //{
                //    token = (idx - 1).ToString();
                //}

                //if (tmp != null)
                //{
                //    IList<DsfForEachItem> data = tmp.GetForEachInputs(context);
                //    IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();


                //    AmendInputs(idx, data, token, updates);

                //    // push updates for Inputs
                //    tmp.UpdateForEachInputs(updates, context);
                //    if (idx == 1)
                //    {
                //        operationalData.InnerActivity.OrigCodedInputs = updates;
                //    }

                //    operationalData.InnerActivity.CurCodedInputs = updates;


                //    // Process outputs
                //    data = tmp.GetForEachOutputs(context);
                //    updates = new List<Tuple<string, string>>();

                //    AmendInputs(idx, data, token, updates);

                //    // push updates 
                //    tmp.UpdateForEachOutputs(updates, context);
                //    if (idx == 1)
                //    {
                //        operationalData.InnerActivity.OrigCodedOutputs = updates;
                //    }

                //    operationalData.InnerActivity.CurCodedOutputs = updates;
                //}
                //else if (tmp == null)
                //{
                //    var tmp2 = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<bool>);

                //    if (tmp2 != null && !(tmp2 is DsfForEachActivity))
                //    {
                //        IList<DsfForEachItem> data = tmp2.GetForEachInputs(context);
                //        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();


                //        AmendInputs(idx, data, token, updates);

                //        // push updates 
                //        tmp2.UpdateForEachInputs(updates, context);
                //        if (idx == 1)
                //        {
                //            operationalData.InnerActivity.OrigCodedInputs = updates;
                //        }
                //        operationalData.InnerActivity.CurCodedInputs = updates;

                //        // Process outputs
                //        data = tmp2.GetForEachOutputs(context);
                //        updates = new List<Tuple<string, string>>();


                //        AmendInputs(idx, data, token, updates);

                //        // push updates 
                //        tmp2.UpdateForEachOutputs(updates, context);
                //        if (idx == 1)
                //        {
                //            operationalData.InnerActivity.OrigCodedOutputs = updates;
        //        }

                //        operationalData.InnerActivity.CurCodedOutputs = updates;
        //    }
                //}

                #endregion

            }

            //Bug 8725 do not mutate mappings
            if (operationalData.InnerActivity.OrigInnerOutputMapping != null)
            {
                // (*) == ({idx}) ;)
                newOutputs = operationalData.InnerActivity.OrigInnerOutputMapping;
                //newOutputs = newOutputs.Replace("(*)", "(" + idx + ")");
                newOutputs = inputItr.IterateMapping(newOutputs, idx);
            }


            var dev2ActivityIoMapping = DataFunc.Handler as IDev2ActivityIOMapping;
            if (dev2ActivityIoMapping != null)
        {
                dev2ActivityIoMapping.InputMapping = newInputs;
            }

            var activityIoMapping = DataFunc.Handler as IDev2ActivityIOMapping;
            if (activityIoMapping != null)
            {
                activityIoMapping.OutputMapping = newOutputs;
            }

            }

        //static void AmendInputs(int idx, IList<DsfForEachItem> data, string token, IList<Tuple<string, string>> updates)
        //{
        //    // amend inputs ;)
        //    foreach(DsfForEachItem d in data)
        //    {
        //        string input = d.Value;
        //        input = input.Replace("(" + token + ")", "(" + idx + ")");

        //        updates.Add(new Tuple<string, string>(d.Value, input));
        //    }
        //}

        /// <summary>
        /// Fetches the type of the execution.
        /// </summary>
        /// <param name="iterateToken">The iterate token.</param>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="compiler">The compiler.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private ForEachBootstrapTO FetchExecutionType(string iterateToken, Guid dlID, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            IBinaryDataListEntry token = compiler.Evaluate(dlID, enActionType.User, iterateToken, false, out errors);
            ForEachBootstrapTO result = new ForEachBootstrapTO(enForEachExecutionType.Scalar, 0, null);

            if (token != null)
            {
                int totalPasses;

                if (token.IsRecordset)
                {
                    // Extract the index for iteration count...
                    string idx = DataListUtil.ExtractIndexRegionFromRecordset(iterateToken);

                    if (!Int32.TryParse(idx, out totalPasses))
                    {
                        totalPasses = token.FetchLastRecordsetIndex();
                    }

                    result = new ForEachBootstrapTO(enForEachExecutionType.Recordset, totalPasses, token);
                }
                else
                {
                    // ghost 
                    if (!DataListUtil.isRootVariable(iterateToken))
                    {
                        result = new ForEachBootstrapTO(enForEachExecutionType.GhostService, Int32.MaxValue, token);
                    }
                    else
                    {
                        // numeric
                        if (Int32.TryParse(token.FetchScalar().TheValue, out totalPasses))
                        {
                            result = new ForEachBootstrapTO(enForEachExecutionType.Numeric, totalPasses, null);
                        }
                    }
                }
            }
            else
            {
                errors.AddError("Cannot evaluate [ " + iterateToken + " ] for ForEach execution");
            }

            return result;
        }

        /// <summary>
        /// Restores the handler fn.
        /// </summary>
        private void RestoreHandlerFn(NativeActivityContext context)
        {

            var activity = (DataFunc.Handler as IDev2ActivityIOMapping);

            if (activity != null)
            {

                if (operationalData.InnerActivity.OrigCodedInputs != null)
                {
                    #region Coded Activity
                    //var tmp = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>);

                    //int idx = operationalData.IterationCount;

                    //if (tmp != null)
                    //{
                    //    // Restore Inputs ;)
                    //    IList<DsfForEachItem> data = tmp.GetForEachInputs(context);
                    //    IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                    //    // amend inputs ;)
                    //    foreach (DsfForEachItem d in data)
                    //    {
                    //        string input = d.Value;
                    //        input = input.Replace("(" + idx + ")", "(*)");

                    //        updates.Add(new Tuple<string, string>(d.Value, input));
                    //    }

                    //    // push updates for Inputs
                    //    tmp.UpdateForEachInputs(updates, context);


                    //    // Restore Outputs ;)
                    //    data = tmp.GetForEachInputs(context);
                    //    updates = new List<Tuple<string, string>>();

                    //    // amend inputs ;)
                    //    foreach (DsfForEachItem d in data)
                    //    {
                    //        string input = d.Value;
                    //        input = input.Replace("(" + idx + ")", "(*)");

                    //        updates.Add(new Tuple<string, string>(d.Value, input));
                    //    }

                    //    // push updates for Inputs
                    //    tmp.UpdateForEachOutputs(updates, context);

                    //}
                    //else
                    //{
                    //    var tmp2 = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<bool>);

                    //    // Restore Inputs ;)
                    //    if (tmp2 != null)
                    //    {
                    //        IList<DsfForEachItem> data = tmp2.GetForEachInputs(context);
                    //        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                    //        // amend inputs ;)
                    //        foreach (DsfForEachItem d in data)
                    //        {
                    //            string input = d.Value;
                    //            input = input.Replace("(" + idx + ")", "(*)");

                    //            updates.Add(new Tuple<string, string>(d.Value, input));
                    //        }

                    //        // push updates for Inputs
                    //        tmp2.UpdateForEachInputs(updates, context);


                    //        // Restore Outputs ;)
                    //        data = tmp2.GetForEachInputs(context);
                    //        updates = new List<Tuple<string, string>>();

                    //        // amend inputs ;)
                    //        foreach (DsfForEachItem d in data)
        //        {
                    //            string input = d.Value;
                    //            input = input.Replace("(" + idx + ")", "(*)");

                    //            updates.Add(new Tuple<string, string>(d.Value, input));
        //        }

                    //        // push updates for Inputs
                    //        tmp2.UpdateForEachOutputs(updates, context);
        //    }
        //}
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
        }

        private ForEachInnerActivityTO GetInnerActivity(out string error)
        {
            ForEachInnerActivityTO result = null;
            error = string.Empty;

            try
            {
                IDev2ActivityIOMapping tmp = DataFunc.Handler as IDev2ActivityIOMapping;
                result = new ForEachInnerActivityTO(tmp);
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            return result;
        }

        private void ActivityCompleted(NativeActivityContext context, ActivityInstance instance, bool result)
        {

            if (operationalData != null)
            {
                operationalData.IncIterationCount();

                if (operationalData.HasMoreData())
                {
                    // Re-jigger the mapping ;)
                    IterateIOMapping((operationalData.IterationCount + 1));
                    IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
                    dataObject.ParentInstanceID = InstanceID;
                    // ReSharper disable RedundantTypeArgumentsOfMethod
                    context.ScheduleFunc<string, bool>(DataFunc, InstanceID, ActivityCompleted);
                    // ReSharper restore RedundantTypeArgumentsOfMethod
                }
                else
                {
                    // that is all she wrote ;)
                    IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
                    dataObject.IsDataListScoped = false;
                    // return it all to normal
                    RestoreHandlerFn(context);
                    dataObject.ParentInstanceID = _previousParentID;

                }
            }
        }

        #endregion Execute

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);           
        }  


        #endregion Private Methodss

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {            
            return DebugItem.EmptyList;
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, ForEachElementName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, ForEachElementName.Replace("*", ""));
        }

        #endregion

    }
}
