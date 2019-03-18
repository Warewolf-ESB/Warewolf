#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data;
using Dev2.DataList.Contract;
using Dev2.Development.Languages.Scripting;
using Dev2.Diagnostics;
using Dev2.Util;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.State;

namespace Dev2.Activities
{
    //[ToolDescriptorInfo("Scripting-JavaScript", "Script", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Scripting", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Scripting_Script")]
    [ExcludeFromCodeCoverage]
    public class DsfScriptingActivity : DsfActivityAbstract<string>, IEquatable<DsfScriptingActivity>
    {
        #region Properties

        [Inputs("ScriptType")]
        public enScriptType ScriptType { get; set; }

        [Inputs("EscapeScript")]
        public bool EscapeScript { get; set; }

        [FindMissing]
        [Inputs("IncludeFile")]
        public string IncludeFile { get; set; }

        [FindMissing]
        [Inputs("Script")]
        public string Script { get; set; }

        [FindMissing]
        [Outputs("Result")]
        public new string Result { get; set; }

        #endregion Properties

        readonly IStringScriptSources _sources;
        #region Ctor

        public DsfScriptingActivity()
            : base("Script")
        {
            Script = string.Empty;
            Result = string.Empty;
            EscapeScript = true;
            IncludeFile = "";
            _sources = new StringScriptSources();
        }

        #endregion Ctor

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

        public override List<string> GetOutputs() => new List<string> { Result };
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
            var errors = new ErrorResultTO();
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
                    DisplayAndWriteError("DsfScriptingJavaScriptActivity", allErrors);
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

        #endregion Overrides of DsfNativeActivity<string>

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

        public bool Equals(DsfScriptingActivity other)
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
                && ScriptType == other.ScriptType
                && EscapeScript == other.EscapeScript
                && string.Equals(IncludeFile, other.IncludeFile)
                && string.Equals(Script, other.Script)
                && string.Equals(Result, other.Result);
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

            return Equals((DsfScriptingActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_sources != null ? _sources.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)ScriptType;
                hashCode = (hashCode * 397) ^ EscapeScript.GetHashCode();
                hashCode = (hashCode * 397) ^ (IncludeFile != null ? IncludeFile.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Script != null ? Script.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}