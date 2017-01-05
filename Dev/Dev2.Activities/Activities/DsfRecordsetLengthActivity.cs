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
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    public class DsfRecordsetLengthActivity : DsfActivityAbstract<string>
    {
        #region Fields

        #endregion

        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("RecordsetName"), FindMissing]
        public string RecordsetName { get; set; }

        /// <summary>
        /// Gets or sets the count number.
        /// </summary>  
        [Outputs("Length"), FindMissing]
        public string RecordsLength { get; set; }

        public DsfRecordsetLengthActivity()
            : base("Length")
        {
            RecordsetName = string.Empty;
            RecordsLength = string.Empty;
            DisplayName = "Length";
        }


        public override List<string> GetOutputs()
        {
            return new List<string> { RecordsLength };
        }

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }
        // ReSharper restore RedundantOverridenMember

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                ValidateRecordsetName(RecordsetName, errors);
                allErrors.MergeErrors(errors);
                if(!allErrors.HasErrors())
                {
                    try
                    {
                        string rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);
                        if(RecordsLength == string.Empty)
                        {
                            allErrors.AddError(ErrorResource.BlankResultVariable);
                        }
                        if(dataObject.IsDebugMode())
                        {
                            var warewolfEvalResult = dataObject.Environment.Eval(RecordsetName.Replace("()", "(*)"), update);
                            if(warewolfEvalResult.IsWarewolfRecordSetResult)
                            {
                                var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult;
                                if(recsetResult != null)
                                {
                                    AddDebugInputItem(new DebugItemWarewolfRecordset(recsetResult.Item, RecordsetName, "Recordset", "="));
                                }
                            }
                            if(warewolfEvalResult.IsWarewolfAtomListresult)
                            {
                                var recsetResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                                if(recsetResult != null)
                                {
                                    AddDebugInputItem(new DebugEvalResult(RecordsetName, "Recordset", dataObject.Environment, update));
                                }
                            }
                        }
                        var rule = new IsSingleValueRule(() => RecordsLength);
                        var single = rule.Check();
                        if(single != null)
                        {
                            allErrors.AddError(single.Message);
                        }
                        else
                        {
                            var count = 0;
                            if(dataObject.Environment.HasRecordSet(RecordsetName))
                            {
                                count = dataObject.Environment.GetLength(rs);
                            }
                            else
                            {
                                allErrors.AddError("Recordset: "+RecordsetName+" does not exist.");
                            }
                            var value = count.ToString();
                            dataObject.Environment.Assign(RecordsLength, value, update);
                            AddDebugOutputItem(new DebugItemWarewolfAtomResult(value, RecordsLength, ""));
                        }
                    }
                    catch(Exception e)
                    {
                        allErrors.AddError(e.Message);
                        dataObject.Environment.Assign(RecordsLength, "0", update);
                        AddDebugOutputItem(new DebugItemStaticDataParams("0", RecordsLength, "", "="));
                    }
                }
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfRecordsetLengthActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion


        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null && updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null && updates.Count == 1)
            {
                RecordsLength = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(RecordsLength);
        }

        #endregion

    }
}
