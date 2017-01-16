using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Development.Languages.Scripting;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Microsoft.CSharp.RuntimeBinder;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities.Scripting
{
    /// <summary>
    /// Activity used for executing JavaScript through a tool
    /// </summary>
    [ToolDescriptorInfo("Scripting-Ruby", "Ruby", ToolType.Native, "3E9FF6C9-E9C6-4C6C-B605-EF6D803373DC", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Scripting", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Ruby")]
    public class DsfRubyActivity : DsfActivityAbstract<string>
    {
        public DsfRubyActivity()
            : base("Ruby")
        {
            ScriptType = enScriptType.Ruby;
            _sources = new StringScriptSources();
            Script = string.Empty;
            Result = string.Empty;
            EscapeScript = true;
            IncludeFile = "";
        }

        [FindMissing]
        [Inputs("Script")]
        public string Script { get; set; }

        public enScriptType ScriptType { get; set; }

        [Inputs("EscapeScript")]
        public bool EscapeScript { get; set; }

        [FindMissing]
        [Outputs("Result")]
        public new string Result { get; set; }

        [FindMissing]
        [Inputs("IncludeFile")]
        public string IncludeFile { get; set; }

        readonly IStringScriptSources _sources;


        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }


        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            AddScriptSourcePathsToList();
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            var env = dataObject.Environment;
            InitializeDebug(dataObject);
            try
            {
                if (!errors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        var language = ScriptType.GetDescription();
                        AddDebugInputItem(new DebugItemStaticDataParams(language, "Language"));
                        AddDebugInputItem(new DebugEvalResult(Script, "Script", env, update));
                    }

                    allErrors.MergeErrors(errors);

                    if (allErrors.HasErrors())
                    {
                        return;
                    }

                    var scriptItr = new WarewolfIterator(dataObject.Environment.Eval(Script, update, false, EscapeScript));
                    while (scriptItr.HasMoreData())
                    {
                        var engine = new ScriptingEngineRepo().CreateEngine(ScriptType, _sources);
                        var value = engine.Execute(scriptItr.GetNextValue());

                        foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                        {

                            env.Assign(region, value, update);
                            if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                            {
                                if (!string.IsNullOrEmpty(region))
                                {
                                    AddDebugOutputItem(new DebugEvalResult(region, "", env, update));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) when (e is NullReferenceException || e is RuntimeBinderException)
            {
                if (e.GetType() == typeof(NullReferenceException) || e.GetType() == typeof(RuntimeBinderException))
                {
                    allErrors.AddError(ErrorResource.ScriptingErrorReturningValue);
                }
                else
                {
                    allErrors.AddError(e.Message.Replace(" for main:Object", string.Empty));
                }
            }
            finally
            {
                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfScriptingRubyActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }

                if (dataObject.IsDebugMode())
                {
                    if (allErrors.HasErrors())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }
        private void AddScriptSourcePathsToList()
        {
            if (!string.IsNullOrEmpty(IncludeFile))
                _sources.AddPaths(IncludeFile);
        }
        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {
                if (t.Item1 == Script)
                {
                    Script = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #endregion

        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
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

        #endregion Get Debug Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Script);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion GetForEachInputs/Outputs
    }
}