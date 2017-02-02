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
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("RecordSet-CountRecords", "Count", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Recordset_Count")]
    public class DsfCountRecordsetNullHandlerActivity : DsfActivityAbstract<string>
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
        [Outputs("CountNumber"), FindMissing]
        public string CountNumber { get; set; }

        public bool TreatNullAsZero { get; set; }

        public DsfCountRecordsetNullHandlerActivity()
            : base("Count Records")
        {
            RecordsetName = string.Empty;
            CountNumber = string.Empty;
            DisplayName = "Count Records";
            TreatNullAsZero = true;
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
                if (!allErrors.HasErrors())
                {
                    try
                    {
                        string rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);
                        if (CountNumber == string.Empty)
                        {
                            allErrors.AddError(ErrorResource.BlankResultVariable);
                        }
                        if (dataObject.IsDebugMode())
                        {
                            AddDebugInputItem(new DebugEvalResult(dataObject.Environment.ToStar(RecordsetName), "Recordset", dataObject.Environment, update));
                        }
                        var rule = new IsSingleValueRule(() => CountNumber);
                        var single = rule.Check();
                        if (single != null)
                        {
                            allErrors.AddError(single.Message);
                        }
                        else
                        {
                            var hasRecordSet = dataObject.Environment.HasRecordSet(RecordsetName);
                            if (hasRecordSet)//null check happens here
                            {
                                var count = dataObject.Environment.GetCount(rs);
                                var value = count.ToString();
                                dataObject.Environment.Assign(CountNumber, value, update);
                                AddDebugOutputItem(new DebugEvalResult(CountNumber, "", dataObject.Environment, update));

                            }
                            else
                            {
                                if (TreatNullAsZero)
                                {
                                    dataObject.Environment.Assign(CountNumber, 0.ToString(), update);
                                    AddDebugOutputItem(new DebugEvalResult(CountNumber, "", dataObject.Environment, update));

                                }
                                else
                                {
                                    allErrors.AddError(string.Format(ErrorResource.NullRecordSet, RecordsetName));
                                }

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        AddDebugInputItem(new DebugItemStaticDataParams("", RecordsetName, "Recordset", "="));
                        allErrors.AddError(e.Message);
                        dataObject.Environment.Assign(CountNumber, "0", update);
                        AddDebugOutputItem(new DebugItemStaticDataParams("0", CountNumber, "", "="));
                    }
                }
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfCountRecordsActivity", allErrors);
                    var errorString = allErrors.MakeDataListReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        public override List<string> GetOutputs()
        {
            return new List<string> {CountNumber};
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }



        #endregion




        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null && updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == CountNumber);
            if (itemUpdate != null)
            {
                CountNumber = itemUpdate.Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(CountNumber);
        }

        #endregion

    }
}
