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
using Warewolf.Storage.Interfaces;



namespace Unlimited.Applications.BusinessDesignStudio.Activities

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

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
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
                {
                    dataObject.Environment.AddError("Json Mappings supplied to activity is null.");
                }

                if (!dataObject.Environment.Errors.Any() && !JsonMappings.Any())

                {
                    dataObject.Environment.AddError("No JSON Mappings supplied to activity.");
                }


                if (!dataObject.Environment.Errors.Any())

                {
                    JsonMappings.ToList().ForEach(m =>
                    {
                        var validationResult = new IsValidJsonCreateMappingInputRule(() => m).Check();
                        if (validationResult != null)
                        {
                            dataObject.Environment.AddError(validationResult.Message);
                        }
                    });
                }

                if (dataObject.IsDebugMode())
                {
                    int j = 0;

                    foreach (JsonMappingTo a in JsonMappings.Where(to => !String.IsNullOrEmpty(to.SourceName)))

                    {
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemStaticDataParams(string.Empty, (++j).ToString(CultureInfo.InvariantCulture)), debugItem);
                        AddDebugItem(new DebugEvalResult(a.SourceName, string.Empty, dataObject.Environment, update), debugItem);
                        _debugInputs.Add(debugItem);
                    }
                }
                if (!dataObject.Environment.Errors.Any())
                {
                    var json = new JObject(); 

                    List<JsonMappingTo> jsonMappingList = JsonMappings.ToList();
                    List<JsonMappingCompoundTo> results = jsonMappingList.Where(to => !String.IsNullOrEmpty(to.SourceName)).Select(jsonMapping =>
                        new JsonMappingCompoundTo(dataObject.Environment, jsonMapping
                            )).ToList();
                    results.ForEach(x =>
                    {
                        if (!x.IsCompound)
                        {
                            json.Add(new JProperty(
                                x.DestinationName,
                                x.EvaluatedResultIndexed(0))
                                );
                        }
                        else
                        {
                            if (!x.EvalResult.IsWarewolfRecordSetResult)
                            {
                                json.Add(new JProperty(
                                            x.DestinationName,
                                            x.ComplexEvaluatedResultIndexed(0))
                                            );
                            }
                            else
                            {
                                if (x.EvalResult.IsWarewolfRecordSetResult)
                                {
                                    json.Add(
                                   x.ComplexEvaluatedResultIndexed(0));
                                }
                            }
                        }
                    }
                  );


                    dataObject.Environment.Assign(JsonString, json.ToString(Formatting.None), update);

                    if (dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugEvalResult(JsonString, string.Empty, dataObject.Environment, update));
                    }
                }
            }
            catch (Exception e)
            {
                JsonMappings.ToList().ForEach(x =>

                {
                    AddDebugInputItem(new DebugItemStaticDataParams("", x.SourceName, "SourceName", "="));
                    AddDebugInputItem(new DebugItemStaticDataParams("", x.DestinationName, "DestinationName"));
                });

                allErrors.AddError(e.Message);
                dataObject.Environment.Assign(JsonString, string.Empty, update);
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
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
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


        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates) => throw new NotImplementedException();
        
        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates) => throw new NotImplementedException();

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => throw new NotImplementedException();

        public override IList<DsfForEachItem> GetForEachOutputs() => throw new NotImplementedException();

        #endregion

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.MixedActivity;
        }
    }
}
