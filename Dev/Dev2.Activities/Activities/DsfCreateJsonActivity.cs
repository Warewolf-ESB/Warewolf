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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.TO;
using Dev2.Util;
using Dev2.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;

// ReSharper disable CheckNamespace

namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("Scripting-CreateJSON", "Create JSON", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Create_JSON")]
    public class DsfCreateJsonActivity : DsfActivityAbstract<string>
    {
        /// <summary>
        ///     Gets or sets the Warewolf source scalars, lists or record sets, and the destination JSON names of the resulting
        ///     serealisation.
        /// </summary>
        [Inputs("JsonMappings")]
        [FindMissing]
        public List<JsonMappingTo> JsonMappings { get; set; }

        /// <summary>
        ///     Gets or sets the JSON string.
        /// </summary>
        [Outputs("JsonString")]
        [FindMissing]
        public string JsonString { get; set; }


        public DsfCreateJsonActivity()
            : base("Create JSON")
        {
            JsonMappings = new List<JsonMappingTo>();
            DisplayName = "Create JSON";
        }

        public override List<string> GetOutputs()
        {
            return new List<string> { JsonString };
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
            ExecuteTool(dataObject,0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            var allErrors = new ErrorResultTO();
            var errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            JsonMappings = JsonMappings.Where(validMapping).ToList();
            // Process if no errors
            try
            {
                if (JsonMappings == null)
                    dataObject.Environment.AddError("Json Mappings supplied to activity is null.");

                // ReSharper disable AssignNullToNotNullAttribute
                if (!dataObject.Environment.Errors.Any() && !JsonMappings.Any())
                // ReSharper restore AssignNullToNotNullAttribute
                {
                    dataObject.Environment.AddError("No JSON Mappings supplied to activity.");
                }

                // ReSharper disable AssignNullToNotNullAttribute
                if (!dataObject.Environment.Errors.Any())
                // ReSharper restore AssignNullToNotNullAttribute
                {
                    JsonMappings.ToList().ForEach(m =>
                        {
                            var validationResult = new IsValidJsonCreateMappingInputRule(() => m).Check();
                            if (validationResult != null)
                                dataObject.Environment.AddError(validationResult.Message);
                        });
                }

                if (dataObject.IsDebugMode())
                {
                    int j = 0;
                    // ReSharper disable PossibleNullReferenceException
                    foreach (JsonMappingTo a in JsonMappings.Where(to => !String.IsNullOrEmpty(to.SourceName)))
                    // ReSharper restore PossibleNullReferenceException
                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams(string.Empty, (++j).ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(a.SourceName, string.Empty, dataObject.Environment,update), debugItem);
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
                    // ReSharper disable MaximumChainedReferences
                    List<JsonMappingCompoundTo> results = jsonMappingList.Where(to => !String.IsNullOrEmpty(to.SourceName)).Select(jsonMapping =>
                        new JsonMappingCompoundTo(dataObject.Environment, jsonMapping
                            )).ToList();
                    // ReSharper restore MaximumChainedReferences




                    // main loop for producing largest list of zipped values
                   
                        results.ForEach(x =>
                        {
                                // if it is not a compound,
                                if (!x.IsCompound)
                                {
                                    // add JProperty, with name x.DestinationName, and value eval(x.SourceName)
                                    json.Add(new JProperty(
                                        x.DestinationName,
                                        x.EvaluatedResultIndexed(0))
                                        );
                                }
                                else
                                {
                                    // if it is a compound, 
                                    if (!x.EvalResult.IsWarewolfRecordSetResult)
                                        json.Add(new JProperty(
                                                x.DestinationName,
                                                x.ComplexEvaluatedResultIndexed(0))
                                                );
                                    else if (x.EvalResult.IsWarewolfRecordSetResult)
                                    {
                                        json.Add(
                                       x.ComplexEvaluatedResultIndexed(0));
                                    }
                                }
                        }
                      );
                    

                    dataObject.Environment.Assign(JsonString, json.ToString(Formatting.None),update);

                    if (dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugEvalResult(JsonString, string.Empty, dataObject.Environment,update));
                    }

                    /*
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
            }
            catch (Exception e)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                JsonMappings.ToList().ForEach(x =>
                // ReSharper restore AssignNullToNotNullAttribute
                {
                    AddDebugInputItem(new DebugItemStaticDataParams("", x.SourceName, "SourceName", "="));
                    AddDebugInputItem(new DebugItemStaticDataParams("", x.DestinationName, "DestinationName"));
                });

                allErrors.AddError(e.Message);
                dataObject.Environment.Assign(JsonString, string.Empty,update);
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
                    DispatchDebugState(dataObject, StateType.Before,update);
                    DispatchDebugState(dataObject, StateType.After,update);
                }
            }
        }

        bool validMapping(JsonMappingTo a)
        {
            return !(String.IsNullOrEmpty(a.DestinationName) && string.IsNullOrEmpty(a.SourceName));
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
            // ReSharper disable MaximumChainedReferences
            var items = JsonMappings.Where(c => !string.IsNullOrEmpty(c.SourceName)).Select(c => c.SourceName).ToArray();
            // ReSharper restore MaximumChainedReferences
            return GetForEachItems(items);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var items = JsonString;
            return GetForEachItems(items);
        }

        #endregion

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }
    }
}
