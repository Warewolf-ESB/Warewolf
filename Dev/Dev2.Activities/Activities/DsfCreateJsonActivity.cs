
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
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.TO;
using Dev2.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace

namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DsfCreateJsonActivity : DsfActivityAbstract<string>
    {
        /// <summary>
        ///     Gets or sets the Warewolf source scalars, lists or record sets, and the destination JSON names of the resulting
        ///     serealisation.
        /// </summary>
        [Inputs("JsonMappings")]
        [FindMissing]
        // ReSharper disable MemberCanBePrivate.Global
        public IEnumerable<JsonMappingTo> JsonMappings { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        /// <summary>
        ///     Gets or sets the JSON string.
        /// </summary>
        [Outputs("JsonString")]
        [FindMissing]
        // ReSharper disable MemberCanBePrivate.Global
        public string JsonString { get; set; }
        // ReSharper restore MemberCanBePrivate.Global


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
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            var allErrors = new ErrorResultTO();
            var errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                if (JsonMappings == null)
                {
                    dataObject.Environment.AddError("Json Mappings supplied to activity is null.");
                }

                // ReSharper disable AssignNullToNotNullAttribute
                if (!dataObject.Environment.Errors.Any() && !JsonMappings.Any())
                // ReSharper restore AssignNullToNotNullAttribute
                {
                    dataObject.Environment.AddError("No Json Mappings supplied to activity.");
                }
                if (dataObject.IsDebugMode())
                {
                    int j = 0;
                    // ReSharper disable PossibleNullReferenceException
                    foreach (JsonMappingTo a in JsonMappings)
                    // ReSharper restore PossibleNullReferenceException
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams(string.Empty, (++j).ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(a.SourceName, string.Empty, dataObject.Environment), debugItem);
                        _debugInputs.Add(debugItem);
                    }
                }
                // TODO: More validation through IRule, IRuleSet to throw out anything not in spec
                if (!dataObject.Environment.Errors.Any())
                {
                    // JsonMappings.Count() is larger than zero
                    var json = new JObject(); // outermost JSON would always be a single JObject, i.e. {'name': value}
                    // ReSharper disable AssignNullToNotNullAttribute
                    List<JsonMappingTo> jsonMappingList = JsonMappings.ToList();
                    // ReSharper restore AssignNullToNotNullAttribute

                    // build the list of JsonMappingCompoundTo - a compound is either a single expression or a comma seperated list of expressions
                    List<JsonMappingCompoundTo> results = jsonMappingList.Select(jsonMapping =>
                        new JsonMappingCompoundTo(dataObject.Environment, jsonMapping
                            )).ToList();

                    // do not allow IsCompound with more than one RecordSet specified 
                    if (results.Where(x => x.IsCompound)
                        .Any(y => y.HasMoreThanOneRecordSet))
                        throw new ArgumentException("Cannot specify more than one RecordSet for a comma seperated input.");

                    // get the longest list
                    int maxCount = results.Select(r => r.MaxCount).Max();

                    // main loop for producing largest list of zipped values
                    for (int i = 0; i < maxCount; i++)
                    {
                        results.ForEach(x =>
                        {
                            // if it is not a compound,
                            if (!x.IsCompound)
                            {
                                // add JProperty, with name x.DestinationName, and value eval(x.SourceName)
                                json.Add(new JProperty(
                                    x.DestinationName,
                                    x.EvaluatedResultIndexed(i))
                                    );
                            }
                            else
                            {
                                if(!x.EvalResult.IsWarewolfRecordSetResult)
                                json.Add(new JProperty(
                                        x.DestinationName,
                                        x.ComplexEvaluatedResultIndexed(i))
                                        );
                                else if (x.EvalResult.IsWarewolfRecordSetResult && i==0)
                                {
                                    json.Add(
                                   x.ComplexEvaluatedResultIndexed(i));
                                }
                            }
                            // if it is a compound, 
                        }
                            );
                    }

                    dataObject.Environment.Assign(JsonString, json.ToString(Formatting.None));

                    if (dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugEvalResult(JsonString, string.Empty, dataObject.Environment));
                    }

                    /*var rule = new IsSingleValueRule(() => CountNumber);
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
            }
            catch (Exception e)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                JsonMappings.ToList().ForEach(x =>
                // ReSharper restore AssignNullToNotNullAttribute
                {
                    AddDebugInputItem(new DebugItemStaticDataParams("", x.SourceName, "SourceName", "="));
                    AddDebugInputItem(new DebugItemStaticDataParams("", x.DestinationName, "DestinationName", "="));
                });

                allErrors.AddError(e.Message);
                dataObject.Environment.Assign(JsonString, string.Empty);
                AddDebugOutputItem(new DebugItemStaticDataParams(string.Empty, JsonString, "", "="));
            }
            finally
            {
                // Handle Errors
                bool hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfCreateJsonActivity", allErrors);
                    string errorString = allErrors.MakeDataListReady();
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
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    // locate all updates for this tuple
                    Tuple<string, string> t1 = t;
                    var items = JsonMappings.Where(c => !string.IsNullOrEmpty(c.SourceName) && c.SourceName.Equals(t1.Item1));

                    // issues updates
                    foreach (var a in items)
                    {
                        a.SourceName = t.Item2;
                    }

    }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
            {
                foreach (var t in updates)
                {
                    if (JsonString == t.Item1)
                    {
                        JsonString = t.Item2;
                    }
                }
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var items = (JsonMappings.Where(c => !string.IsNullOrEmpty(c.SourceName)).Select(c => c.SourceName)).ToArray();
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = JsonString;
            return GetForEachItems(items);
        }

        #endregion
    }
}
