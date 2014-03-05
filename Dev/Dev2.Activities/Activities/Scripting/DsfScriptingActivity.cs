using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common.Enums;
using Dev2.Common.ExtMethods;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Development.Languages.Scripting;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Dev2.Activities
{
    /// <summary>
    /// Activity used for executing JavaScript through a tool
    /// </summary>
    public class DsfScriptingActivity : DsfActivityAbstract<string>
    {
        #region Fields

        #endregion

        #region Properties

        [Inputs("ScriptType")]
        public enScriptType ScriptType { get; set; }

        [FindMissing]
        [Inputs("Script")]
        public string Script { get; set; }

        [FindMissing]
        [Outputs("Result")]
        public new string Result { get; set; }

        #endregion

        #region Ctor

        public DsfScriptingActivity()
            : base("Script")
        {
            Script = string.Empty;
            Result = string.Empty;
        }

        #endregion

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = dlID;
            allErrors.MergeErrors(errors);
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder();
            toUpsert.IsDebug = (dataObject.IsDebugMode());
            toUpsert.ResourceID = dataObject.ResourceID;

            InitializeDebug(dataObject);
            try
            {
                if(!errors.HasErrors())
                {
                    IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                    IDev2DataListEvaluateIterator scriptItr = CreateDataListEvaluateIterator(Script, executionId, compiler, colItr, allErrors);

                    IBinaryDataListEntry scriptEntry = compiler.Evaluate(executionId, enActionType.User, Script, false, out errors);
                    allErrors.MergeErrors(errors);

                    if(dataObject.IsDebugMode())
                    {
                        var language = ScriptType.GetDescription();
                        AddDebugInputItem(new DebugItemStaticDataParams(language, "Language"));
                        AddDebugInputItem(new DebugItemVariableParams(Script, "Script", scriptEntry, executionId));
                    }
                    if(allErrors.HasErrors())
                    {
                        return;
                    }

                    while(colItr.HasMoreData())
                    {
                        string scriptValue = colItr.FetchNextRow(scriptItr).TheValue;

                        var engine = new ScriptingEngineRepo().FindMatch(ScriptType);
                        var value = engine.Execute(scriptValue);

                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                        foreach(var region in DataListCleaningUtils.SplitIntoRegions(Result))
                        {
                            toUpsert.Add(region, value);
                            toUpsert.FlushIterationFrame();
                        }
                    }
                    compiler.Upsert(executionId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);
                    if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                    {
                        foreach(var debugOutputTo in toUpsert.DebugOutputs)
                        {
                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                        }
                    }
                }
            }
            catch(Exception e)
            {
                if(e.GetType() == typeof(NullReferenceException) || e.GetType() == typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException))
                {
                    allErrors.AddError("There was an error when returning a value from your script, remember to use the 'Return' keyword when returning the result");
                }
                else
                {

                    allErrors.AddError(e.Message.Replace(" for main:Object", string.Empty));
                }
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfScriptingJavaScriptActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                if(dataObject.IsDebugMode())
                {
                    if(allErrors.HasErrors())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(Tuple<string, string> t in updates)
            {

                if(t.Item1 == Script)
                {
                    Script = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Script);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}

