
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public class DsfUniqueActivity : DsfActivityAbstract<string>
    {

        /// <summary>
        /// The property that holds all the convertions
        /// </summary>

        [FindMissing]
        public string InFields { get; set; }

        [FindMissing]
        public string ResultFields { get; set; }

        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        [FindMissing]
        public new string Result { get; set; }


        #region Ctor

        public DsfUniqueActivity()
            : base("Unique Records")
        {
            InFields = string.Empty;
            ResultFields = string.Empty;
        }

        #endregion

        #region Overrides of DsfNativeActivity<string>

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        //// ReSharper restore RedundantOverridenMember

        ///// <summary>
        ///// Breaks the InFields and validates they belong to the same recordset ;)
        ///// </summary>
        ///// <param name="dlId">The dl ID.</param>
        ///// <param name="compiler">The compiler.</param>
        ///// <param name="token">The token.</param>
        ///// <param name="dataObject">The data object.</param>
        ///// <param name="evaluateForDebug">if set to <c>true</c> [evaluate for debug].</param>
        ///// <param name="errors">The errors.</param>
        ///// <param name="rsEntry">The rs entry.</param>
        ///// <returns></returns>
        ///// <exception cref="System.Exception">Mismatched Recordsets. Encountered {  + rs +  } , but already processed {  + masterRs +  }
        ///// or
        ///// Invalid field {  + col +  } for recordset {  + masterRs +  }</exception>
        //private List<string> BreakAndValidate(Guid dlId, IDataListCompiler compiler, string token, IDSFDataObject dataObject, bool evaluateForDebug, out ErrorResultTO errors, out IBinaryDataListEntry rsEntry)
        //{
        //    var searchFields = DataListCleaningUtils.SplitIntoRegions(token);
        //    errors = new ErrorResultTO();
        //    rsEntry = null;
        //    var masterRs = string.Empty;

        //    List<string> toProcessColumns = new List<string>();
        //    // fish out each column name and validate that all belong to same recordset ;)
        //    foreach(var entry in searchFields)
        //    {
        //        // now validate as a RS in the list and extract the field ;)
        //        var rs = DataListUtil.ExtractRecordsetNameFromValue(entry);
        //        var field = DataListUtil.ExtractFieldNameFromValue(entry);

        //        if(masterRs != rs && !string.IsNullOrEmpty(masterRs))
        //        {
        //            // an issue has been detected ;(
        //            throw new Exception("Mismatched Recordsets. Encountered { " + rs + " } , but already processed { " + masterRs + " }");
        //        }

        //        // set the first pass ;)
        //        if(string.IsNullOrEmpty(masterRs) && !string.IsNullOrEmpty(rs))
        //        {
        //            // set it ;)
        //            masterRs = rs;
        //        }

        //        // add to column collection ;)
        //        toProcessColumns.Add(field);
        //    }

        //    ErrorResultTO invokeErrors;

        //    // Now validate each column ;)
        //    masterRs = DataListUtil.MakeValueIntoHighLevelRecordset(masterRs, true);
        //    var myRs = DataListUtil.AddBracketsToValueIfNotExist(masterRs);
        //    rsEntry = compiler.Evaluate(dlId, enActionType.User, myRs, false, out invokeErrors);
        //    errors.MergeErrors(invokeErrors);

        //    if(rsEntry != null)
        //    {
        //        var cols = rsEntry.Columns;
        //        foreach(var col in toProcessColumns)
        //        {
        //            if(cols.All(c => c.ColumnName != col))
        //            {
        //                throw new Exception("Invalid field { " + col + " } for recordset { " + masterRs + " }");
        //            }
        //        }
        //    }

        //    if(dataObject.IsDebugMode())
        //    {
        //        if(evaluateForDebug)
        //        {
        //            AddDebugInputItem(new DebugItemStaticDataParams("", "In Field(s)"));
        //            foreach(var field in searchFields)
        //            {
        //                // TODO : if EvaluateforDebug
        //                if(!string.IsNullOrEmpty(field))
        //                {
        //                    var debugEval = compiler.Evaluate(dlId, enActionType.User, field, false, out invokeErrors);
        //                    errors.MergeErrors(invokeErrors);
        //                    if(errors.HasErrors())
        //                    {
        //                        AddDebugInputItem(new DebugItemStaticDataParams("", field, ""));
        //                    }
        //                    else
        //                    {
        //                        AddDebugInputItem(new DebugItemVariableParams(field, "", debugEval, dlId));
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            AddDebugInputItem(DataListUtil.IsEvaluated(token) ? new DebugItemStaticDataParams("", token, "Return Fields") : new DebugItemStaticDataParams(token, "Return Fields"));
        //        }
        //    }

        //    return toProcessColumns;
        //}

        /// <summary>
        /// When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);
                 try
            {




                var toresultfields = Result.Split(new[] { ',' });
                var fromFields = InFields.Split(new[] { ',' });
                var fromResultFieldresultfields = ResultFields.Split(new[] { ',' });


                PreExecution(dataObject, fromFields);

                dataObject.Environment.AssignUnique(fromFields, fromResultFieldresultfields, toresultfields);

                PostExecute(dataObject, toresultfields);
               
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFUnique", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfUniqueActivity", allErrors);
                    foreach (var error in allErrors.FetchErrors()){


                        dataObject.Environment.AddError(error);
                    }
                }

                if(dataObject.IsDebugMode())
                {

                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        void PostExecute(IDSFDataObject dataObject, IEnumerable<string> toresultfields)
        {
            if(dataObject.IsDebugMode())
            {
                foreach(var field in toresultfields)
                {
                    // TODO : if EvaluateforDebug
                    if(!string.IsNullOrEmpty(field))
                    {
                        try
                        {
                            AddDebugOutputItem(new DebugEvalResult(field, "", dataObject.Environment));
                        }
                        catch(Exception)
                        {
                            AddDebugOutputItem(new DebugItemStaticDataParams("", field, ""));
                            throw;
                        }
                    }
                }
            }
        }

        void PreExecution(IDSFDataObject dataObject, IEnumerable<string> fromFields)
        {
            if(dataObject.IsDebugMode())
            {
                AddDebugInputItem(new DebugItemStaticDataParams("", "In Field(s)"));
                foreach(var field in fromFields)
                {
                    // TODO : if EvaluateforDebug
                    if(!string.IsNullOrEmpty(field))
                    {
                        try
                        {
                            AddDebugInputItem(new DebugEvalResult(field, "", dataObject.Environment));
                        }
                        catch(Exception)
                        {
                            AddDebugInputItem(new DebugItemStaticDataParams("", field, ""));
                        }
                    }
                }
            }
        }

        //static void UpdateStarNotationColumns(DebugItem itemToAdd)
        //{
        //    var groups = itemToAdd.ResultsList.Where(a => DataListUtil.IsValueRecordset(a.Variable) && String.IsNullOrEmpty(a.GroupName)).GroupBy(a => DataListUtil.ExtractRecordsetNameFromValue(a.Variable) + DataListUtil.ExtractFieldNameFromValue(a.Variable));

        //    var maxId = itemToAdd.ResultsList.Count > 0 ? itemToAdd.ResultsList.Max(a => a.GroupIndex) : 0;
        //    foreach(var g in groups)
        //    {

        //        foreach(var res in g)
        //        {
        //            maxId++;
        //            res.GroupIndex = maxId;
        //            res.GroupName = "[[" + DataListUtil.ExtractRecordsetNameFromValue(res.Variable) + "()." + DataListUtil.ExtractFieldNameFromValue(res.Variable) + "]]";
        //        }
        //    }
        //}



        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == InFields)
                    {
                        InFields = t.Item2;
                    }
                    if(t.Item1 == ResultFields)
                    {
                        ResultFields = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }

        #region Overrides of DsfNativeActivity<string>
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #region Private Methods

        #endregion

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(InFields, ResultFields);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion


    }
}
