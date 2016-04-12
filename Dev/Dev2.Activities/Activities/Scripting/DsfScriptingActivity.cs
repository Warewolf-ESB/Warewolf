
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Development.Languages.Scripting;
using Dev2.Diagnostics;
using Dev2.Util;
using Microsoft.CSharp.RuntimeBinder;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;
using WarewolfParserInterop;

// ReSharper disable CheckNamespace
namespace Dev2.Activities
{
    /// <summary>
    /// Activity used for executing JavaScript through a tool
    /// </summary>
    [ToolDescriptorInfo("Scripting-JavaScript", "Script", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Scripting", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
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

            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            var env = dataObject.Environment;
            InitializeDebug(dataObject);
            try
            {
                if(!errors.HasErrors())
                {
                    if(dataObject.IsDebugMode())
                    {
                        var language = ScriptType.GetDescription();
                        AddDebugInputItem(new DebugItemStaticDataParams(language, "Language"));
                        AddDebugInputItem(new DebugEvalResult(Script, "Script", env, update));
                    }

                    var listOfEvalResultsForInput = dataObject.Environment.EvalForDataMerge(Script, update);
                    var innerIterator = new WarewolfListIterator();
                    var innerListOfIters = new List<WarewolfIterator>();

                    foreach (var listOfIterator in listOfEvalResultsForInput)
                    {
                        var inIterator = new WarewolfIterator(listOfIterator);
                        innerIterator.AddVariableToIterateOn(inIterator);
                        innerListOfIters.Add(inIterator);
                    }
                    var atomList = new List<DataASTMutable.WarewolfAtom>();
                    while (innerIterator.HasMoreData())
                    {
                        var stringToUse = "";
                        foreach (var warewolfIterator in innerListOfIters)
                        {
                            stringToUse += warewolfIterator.GetNextValue();
                        }
                        atomList.Add(DataASTMutable.WarewolfAtom.NewDataString(stringToUse));
                    }
                    var finalString = string.Join("", atomList);
                    var inputListResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataASTMutable.WarewolfAtom>(DataASTMutable.WarewolfAtom.Nothing, atomList));
                    if (DataListUtil.IsFullyEvaluated(finalString))
                    {
                        inputListResult = dataObject.Environment.Eval(finalString, update);
                    }

                    allErrors.MergeErrors(errors);

                    if(allErrors.HasErrors())
                    {
                        return;
                    }
                    var scriptItr = new WarewolfIterator(inputListResult);
                    while(scriptItr.HasMoreData())
                    {
                        var engine = new ScriptingEngineRepo().CreateEngine(ScriptType);
                        var value = engine.Execute(scriptItr.GetNextValue());

                        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                        foreach(var region in DataListCleaningUtils.SplitIntoRegions(Result))
                        {
                            env.Assign(region, value, update);
                            if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                            {
                                AddDebugOutputItem(new DebugEvalResult(region, "", env, update));
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                if(e.GetType() == typeof(NullReferenceException) || e.GetType() == typeof(RuntimeBinderException))
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
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach(Tuple<string, string> t in updates)
            {

                if(t.Item1 == Script)
                {
                    Script = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
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

