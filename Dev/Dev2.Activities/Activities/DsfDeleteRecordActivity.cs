
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
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfDeleteRecordActivity : DsfActivityAbstract<string>
    {
        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("RecordsetName"), FindMissing]
        public string RecordsetName { get; set; }

        /// <summary>
        /// Gets or sets the count number.
        /// </summary>  
        [Outputs("Result"), FindMissing]
        public new string Result { get; set; }

        public DsfDeleteRecordActivity()
            : base("Delete Record")
        {
            RecordsetName = string.Empty;
            Result = string.Empty;
        }

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }
        // ReSharper restore RedundantOverridenMember

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid executionId = dataObject.DataListID;

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();

            InitializeDebug(dataObject);
            try
            {
                ValidateRecordsetName(RecordsetName, errors);

                GetDebug(dataObject);
                dataObject.Environment.EvalDelete(RecordsetName);

                dataObject.Environment.Assign(Result, "Success");
                AddDebugOutputItem(new DebugItemWarewolfAtomResult("Success", Result, "","", "="));
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
                dataObject.Environment.Assign(Result, "");
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfDeleteRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, Result, (string)null, out errors);
                }

                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void GetDebug(IDSFDataObject dataObject)
        {
            try
            {


                if (dataObject.IsDebugMode() && ExecutionEnvironment.IsRecordSetName(RecordsetName))
                {
                    var res = dataObject.Environment.Eval(RecordsetName);
                    if (res.IsWarewolfRecordSetResult)
                    {
                        // ReSharper disable PossibleNullReferenceException
                        var recset = (res as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult).Item;
                        // ReSharper restore PossibleNullReferenceException
                        if (recset != null)
                        {
                            AddDebugInputItem(new DebugItemWarewolfRecordset(recset, RecordsetName, "Recordset", "="));
                        }
                    }
                }

            }
             
            catch(Exception)
            {
                
                     AddDebugInputItem(new DebugItemWarewolfAtomResult("", RecordsetName, "Recordset","", "="));
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        #endregion Get Inputs/Outputs



        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env)
        {
            return _debugInputs;
        }

   

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env)
        {
            return _debugOutputs;
        }


        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(var t in updates)
                {

                    if(t.Item1 == RecordsetName)
                    {
                        RecordsetName = t.Item2;
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

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
