
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
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.DataList.Contract;
using Dev2.Development.Languages.Scripting;
using Dev2.Diagnostics;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

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

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
                    var env = dataObject.Environment;
            InitializeDebug(dataObject);
            try
            {
                if(!errors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        var language = ScriptType.GetDescription();
                        AddDebugInputItem(new DebugItemStaticDataParams(language, "Language"));
                        AddDebugInputItem(new DebugEvalResult(Script, "Script", env));
                    }

                    var scriptItr = env.EvalAsListOfStrings(Script);
                    allErrors.MergeErrors(errors);

                    
                    if(allErrors.HasErrors())
                    {
                        return;
                    }

                    foreach(var scriptValue in scriptItr)
                    {


                        var engine = new ScriptingEngineRepo().CreateEngine(ScriptType);
                        var value = engine.Execute(scriptValue);

                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                        foreach(var region in DataListCleaningUtils.SplitIntoRegions(Result))
                        {
                            env.Assign(region,value);
                            if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                            {
                             
                                    AddDebugOutputItem(new DebugEvalResult(region,"",env));
                              
                            }
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
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
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
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }

        #endregion

        #region Private Methods

        #endregion

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
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

