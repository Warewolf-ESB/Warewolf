
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
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;
using Newtonsoft.Json.Linq;
 

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfCreateJsonActivity : DsfActivityAbstract<string>
    {
        /// <summary>
        /// Gets or sets the Warewolf source scalars, lists or record sets, and the destination JSON names of the resulting serealisation.
        /// </summary>  
        [Inputs("JsonMappings"), FindMissing]
        public IEnumerable<JsonMappingTo> JsonMappings { get; set; }

        /// <summary>
        /// Gets or sets the JSON string.
        /// </summary>  
        [Outputs("JsonString"), FindMissing]
        public string JsonString { get; set; }

        public DsfCreateJsonActivity()
            : base("Create JSON")
        {
            JsonMappings = new List<JsonMappingTo>();
            DisplayName = "Create JSON";
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
            ExecuteTool(dataObject);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                if (JsonMappings == null)
                    dataObject.Environment.AddError("Json Mappings supplied to activity is null.");

                if (!dataObject.Environment.Errors.Count() > 0 && JsonMappings.Count() == 0)
                    dataObject.Environment.AddError("No Json Mappings supplied to activity.");

                if (!dataObject.Environment.Errors.Any())
                {
                    try
                    {
                        // JsonMappings.Count() is larger than zero
                        JObject json = new JObject();  // outermost JSON would always be a single JObject, i.e. {'name': value}
                        var jsonMappingList = JsonMappings.ToList();
                        var results =
                        jsonMappingList.Select(jsonMapping =>
                            new JsonMappingCompoundTo(
                                jsonMapping
                                )
                            ).ToList();

                        // get the longest list
                        int maxCount = results.Select(r =>r.MaxCount).Max(); 
                            
                        for (int i=0;i < maxCount;i++)
                        {
                                results.ForEach(x =>
                                    {

                                    }
                                );
                        }

                        /*
                        if (dataObject.IsDebugMode())
                        {
                            AddDebugInputItem(new DebugEvalResult(dataObject.Environment.ToStar(RecordsetName), "Recordset", dataObject.Environment));
                        }
                        var rule = new IsSingleValueRule(() => CountNumber);
                        var single = rule.Check();
                        if (single != null)
                        {
                            allErrors.AddError(single.Message);
                        }
                        else
                        {
                            var count = 0;
                            if (dataObject.Environment.HasRecordSet(RecordsetName))
                            {
                                count = dataObject.Environment.GetCount(rs);
                            }
                            var value = count.ToString();
                            dataObject.Environment.Assign(CountNumber, value);
                            AddDebugOutputItem(new DebugEvalResult(CountNumber, "", dataObject.Environment));
                        }
                         * */
                    }
                    catch (Exception e)
                    {
                        /*
                        AddDebugInputItem(new DebugItemStaticDataParams("", RecordsetName, "Recordset", "="));
                        allErrors.AddError(e.Message);
                        dataObject.Environment.Assign(CountNumber, "0");
                        AddDebugOutputItem(new DebugItemStaticDataParams("0", CountNumber, "", "="));
                         * */
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
                    DispatchDebugState(dataObject, StateType.Before);
                    DispatchDebugState(dataObject, StateType.After);
                }
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
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
            if (updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == CountNumber);
                if (itemUpdate != null)
                {
                    CountNumber = itemUpdate.Item2;
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
            return GetForEachItems(CountNumber);
        }

        #endregion

    }
}
