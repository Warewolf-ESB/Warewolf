
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
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfSortRecordsActivity : DsfActivityAbstract<string>
    {

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        [Inputs("SortField")]
        [FindMissing]
        public string SortField { get; set; }

        /// <summary>
        /// Gets or sets the selected sort.
        /// </summary>
        [Inputs("SelectedSort")]
        public string SelectedSort { get; set; }

        public DsfSortRecordsActivity()
            : base("Sort Records")
        {
            SortField = string.Empty;
            SelectedSort = "Forward";
            DisplayName = "Sort Records";
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
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ErrorResultTO errors;
            ErrorResultTO allErrors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            InitializeDebug(dataObject);

            try
            {

               

                bool descOrder = String.IsNullOrEmpty(SelectedSort) || SelectedSort.Equals("Backwards");
                if (dataObject.IsDebugMode())
                {
                    AddDebugInputItem(SortField, "Sort Field", dataObject.Environment, executionId);
                }
                // Travis.Frisinger : New Stuff....
                if (!string.IsNullOrEmpty(SortField))
                {
                    dataObject.Environment.SortRecordSet(SortField, descOrder);

                    DebugOutputs(dataObject);
                }
                else
                {
                    allErrors.AddError("No recordset given");
                }
            }
            finally
            {

                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfSortRecordsActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

            // End Travis.Frisinger New Stuff
        }

        void DebugOutputs(IDSFDataObject dataObject)
        {
            if(dataObject.IsDebugMode())
            {
                var data = dataObject.Environment.Eval(dataObject.Environment.ToStar(SortField));
                if(data.IsWarewolfAtomListresult)
                {
                    var lst = data as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                    AddDebugOutputItem(new DebugItemWarewolfAtomListResult(lst, "", "", SortField, "", "", "="));
                }
            }
        }

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IExecutionEnvironment env, Guid executionId)
        {
            var data =  env.Eval(env.ToStar( expression));
            if (data.IsWarewolfAtomListresult)
            {
                var lst = data as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                AddDebugInputItem(new DebugItemWarewolfAtomListResult(lst,"","",expression, labelText,"","="));
                AddDebugInputItem(new DebugItemStaticDataParams(SelectedSort, "Sort Order"));
            }
        }

        private string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {

            string rawRef = DataListUtil.StripBracketsFromValue(value);
            string objRef = string.Empty;

            if(partType == enIntellisensePartType.RecordsetsOnly)
            {
                objRef = DataListUtil.ExtractRecordsetNameFromValue(rawRef);
            }
            else if(partType == enIntellisensePartType.RecordsetFields)
            {
                objRef = DataListUtil.ExtractFieldNameFromValue(rawRef);
            }

            return objRef;
        }

        #endregion Private Methods

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == SortField)
                    {
                        SortField = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == SortField)
                    {
                        SortField = t.Item2;
                    }
                }
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(SortField);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(SortField);
        }

        #endregion


        #region GetDebugInputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env)
        {
            return _debugInputs;
        }


        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env)
        {
            return _debugOutputs;
        }

        #endregion

        #endregion

    }
}
