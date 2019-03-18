#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
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
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.Scripting
{
    /// <summary>
    /// Activity used for executing JavaScript through a tool
    /// </summary>
    [ToolDescriptorInfo("Scripting-Ruby", "Ruby", ToolType.Native, "3E9FF6C9-E9C6-4C6C-B605-EF6D803373DC", "Dev2.Activities", "1.0.0.0", "Legacy", "Scripting", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Ruby")]
    public class DsfRubyActivity : DsfActivityAbstract<string>, IEquatable<DsfRubyActivity>
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

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "Script",
                    Value = Script,
                    Type = StateVariable.StateType.Input
                },
                 new StateVariable
                {
                    Name = "IncludeFile",
                    Value = IncludeFile,
                    Type = StateVariable.StateType.Input
                },
                 new StateVariable
                {
                    Name = "EscapeScript",
                    Value = EscapeScript.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        #region Overrides of DsfNativeActivity<string>

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }
        
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            AddScriptSourcePathsToList();
            var allErrors = new ErrorResultTO();
            var env = dataObject.Environment;
            InitializeDebug(dataObject);
            try
            {
                TryExecute(dataObject, update, allErrors, env);
            }

            catch (Exception e) when (e is NullReferenceException || e is RuntimeBinderException)
            {
                allErrors.AddError(e.GetType() == typeof(NullReferenceException) || e.GetType() == typeof(RuntimeBinderException) ? ErrorResource.ScriptingErrorReturningValue : e.Message.Replace(" for main:Object", string.Empty));
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

        private void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, IExecutionEnvironment env)
        {
            if (dataObject.IsDebugMode())
            {
                var language = ScriptType.GetDescription();
                AddDebugInputItem(new DebugItemStaticDataParams(language, "Language"));
                AddDebugInputItem(new DebugEvalResult(Script, "Script", env, update));
            }

            var scriptItr = new WarewolfIterator(dataObject.Environment.Eval(Script, update, false, EscapeScript));
            while (scriptItr.HasMoreData())
            {
                var engine = new ScriptingEngineRepo().CreateEngine(ScriptType, _sources);
                var value = engine.Execute(scriptItr.GetNextValue());

                foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                {

                    env.Assign(region, value, update);
                    if (dataObject.IsDebugMode() && !allErrors.HasErrors() && !string.IsNullOrEmpty(region))
                    {
                        AddDebugOutputItem(new DebugEvalResult(region, "", env, update));
                    }
                }
            }
        }

        void AddScriptSourcePathsToList()
        {
            if (!string.IsNullOrEmpty(IncludeFile))
            {
                _sources.AddPaths(IncludeFile);
            }
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

        public override List<string> GetOutputs() => new List<string> { Result };

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Debug Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(Script);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        #endregion GetForEachInputs/Outputs

        public bool Equals(DsfRubyActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && string.Equals(Script, other.Script)
                && ScriptType == other.ScriptType
                && EscapeScript == other.EscapeScript
                && string.Equals(Result, other.Result)
                && string.Equals(IncludeFile, other.IncludeFile);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfRubyActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_sources != null ? _sources.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Script != null ? Script.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)ScriptType;
                hashCode = (hashCode * 397) ^ EscapeScript.GetHashCode();
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (IncludeFile != null ? IncludeFile.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}