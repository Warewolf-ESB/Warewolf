
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
using System.Globalization;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    [ToolDescriptorInfo("RecordSet-Length", "Length", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Bob", "1.0.0.0", "c:\\", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]

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

            Guid dlId = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = dlId;
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {

                if(!string.IsNullOrWhiteSpace(RecordsetName))
                {


                    IBinaryDataList bdl = compiler.FetchBinaryDataList(executionId, out errors);
                    allErrors.MergeErrors(errors);

                    string err;
                    IBinaryDataListEntry recset;

                    string rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);

                    bdl.TryGetEntry(rs, out recset, out err);
                    allErrors.AddError(err);
                    try
                    {
                        // ReSharper disable UnusedVariable
                        var hasValue = recset.FetchScalar().TheValue;
                        // ReSharper restore UnusedVariable
                    }
                    catch (Exception e)
                    {
                        allErrors.AddError(e.Message);
                    }
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(new DebugItemVariableParams(RecordsetName, "Recordset", recset, executionId));
                    }
                    var rule = new IsSingleValueRule(() => RecordsLength);
                    var single = rule.Check();
                    if(single != null)
                    {
                        allErrors.AddError(single.Message);
                    }
                    else
                    {
                        if(recset != null)
                        {
                            if(recset.Columns != null && RecordsLength != string.Empty)
                            {
                                if(recset.IsEmpty())
                                {

                                    compiler.Upsert(executionId, RecordsLength, "0", out errors);
                                    if(dataObject.IsDebugMode())
                                    {
                                        AddDebugOutputItem(new DebugOutputParams(RecordsLength, "0", executionId, 0));
                                    }
                                    allErrors.MergeErrors(errors);

                                }
                                else
                                {




                                    foreach(var region in DataListCleaningUtils.SplitIntoRegions(RecordsLength))
                                    {
                                        int cnt = recset.FetchAppendRecordsetIndex() - 1;
                                        compiler.Upsert(executionId, region, cnt.ToString(CultureInfo.InvariantCulture), out errors);
                                        if(dataObject.IsDebugMode())
                                        {
                                            AddDebugOutputItem(new DebugOutputParams(region, cnt.ToString(CultureInfo.InvariantCulture), executionId, 0));
                                        }
                                        allErrors.MergeErrors(errors);
                                    }
                                }


                                allErrors.MergeErrors(errors);
                            }
                            else if(recset.Columns == null)
                            {
                                allErrors.AddError(RecordsetName + " is not a recordset");
                            }
                            else if(RecordsLength == string.Empty)
                            {
                                allErrors.AddError("Blank result variable");
                            }
                        }
                        allErrors.MergeErrors(errors);
                    }
                }
                else
                {
                    allErrors.AddError("No recordset given");
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {

                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfRecordsetLengthActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    compiler.Upsert(executionId, RecordsLength, (string)null, out errors);
                }
                if(dataObject.IsDebugMode())
                {

                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
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

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null && updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
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
