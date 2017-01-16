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
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Operations;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <New>
    /// Activity for replacing a certain character set in a number of field with a new character set 
    /// </New>
    [ToolDescriptorInfo("Data-Replace", "Replace", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Data", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Data_Replace")]
    public class DsfReplaceActivity : DsfActivityAbstract<string>
    {

        #region Properties

        /// <summary>
        /// Property for holding a string the user enters into the "In Fields" box
        /// </summary>
        [Inputs("FieldsToSearch")]
        [FindMissing]
        public string FieldsToSearch { get; set; }

        /// <summary>
        /// Property for holding a string the user selects in the "Find" box
        /// </summary>
        [Inputs("Find")]
        [FindMissing]
        public string Find { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Replace With" box
        /// </summary>
        [Inputs("ReplaceWith")]
        [FindMissing]
        public string ReplaceWith { get; set; }

        /// <summary>
        /// Property for holding a boolean the user selects in the wizard checkbox
        /// </summary>
        [Inputs("CaseMatch")]
        public bool CaseMatch { get; set; }

        /// <summary>
        /// Property for holding a string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        #endregion Properties

        #region Ctor

        public DsfReplaceActivity()
            : base("Replace")
        {
            // Initialise all the properties here
            FieldsToSearch = string.Empty;
            Find = string.Empty;
            ReplaceWith = string.Empty;
            Result = string.Empty;
        }

        #endregion Ctor


        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        /// <summary>
        /// Executes the logic of the activity and calls the backend code to do the work
        /// Also responsible for adding the results to the data list
        /// </summary>
        /// <param name="context"></param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            IDev2ReplaceOperation replaceOperation = Dev2OperationsFactory.CreateReplaceOperation();
            ErrorResultTO errors;
            ErrorResultTO allErrors = new ErrorResultTO();

            int replacementCount = 0;
            int replacementTotal = 0;

            InitializeDebug(dataObject);
            try
            {
                IList<string> toSearch = FieldsToSearch.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach(var s in toSearch)
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(new DebugEvalResult(s, "In Field(s)", dataObject.Environment, update));
                        if(Find != null)
                        {
                            AddDebugInputItem(new DebugEvalResult(Find, "Find", dataObject.Environment, update));
                        }
                        if(ReplaceWith != null)
                        {
                            AddDebugInputItem(new DebugEvalResult(ReplaceWith, "Replace With", dataObject.Environment, update));
                        }
                    }
                }
                IWarewolfListIterator iteratorCollection = new WarewolfListIterator();

                var finRes = dataObject.Environment.Eval(Find, update);
                if (ExecutionEnvironment.IsNothing(finRes))
                {
                    if (!string.IsNullOrEmpty(Result))
                    {
                        dataObject.Environment.Assign(Result, replacementTotal.ToString(CultureInfo.InvariantCulture), update);
                    }
                }
                else
                {
                    var itrFind = new WarewolfIterator(dataObject.Environment.Eval(Find, update));
                    iteratorCollection.AddVariableToIterateOn(itrFind);

                    var itrReplace = new WarewolfIterator(dataObject.Environment.Eval(ReplaceWith, update));
                    iteratorCollection.AddVariableToIterateOn(itrReplace);
                    var rule = new IsSingleValueRule(() => Result);
                    var single = rule.Check();
                    if (single != null)
                    {
                        allErrors.AddError(single.Message);
                    }
                    else
                    {
                        var counter = 1;
                        while (iteratorCollection.HasMoreData())
                        {
                            // now process each field for entire evaluated Where expression....                    
                            var findValue = iteratorCollection.FetchNextValue(itrFind);
                            var replaceWithValue = iteratorCollection.FetchNextValue(itrReplace);
                            foreach (string s in toSearch)
                            {
                                if (!DataListUtil.IsEvaluated(s))
                                {
                                    allErrors.AddError(ErrorResource.RequiredVaraibleNameONLY);
                                    return;
                                }
                                if (!string.IsNullOrEmpty(findValue))
                                {
                                    if (!string.IsNullOrEmpty(Result) && !DataListUtil.IsValueScalar(Result))
                                    {
                                        if (!dataObject.Environment.HasRecordSet(DataListUtil.ExtractRecordsetNameFromValue(Result)))
                                        {
                                            dataObject.Environment.AssignDataShape(Result);
                                        }
                                    }
                                        dataObject.Environment.ApplyUpdate(s, a =>
                                    {
                                        replacementCount = 0;
                                        var replace = replaceOperation.Replace(a.ToString(), findValue, replaceWithValue, CaseMatch, out errors, ref replacementCount);
                                        if (!string.IsNullOrEmpty(Result) && !DataListUtil.IsValueScalar(Result))
                                        {
                                            dataObject.Environment.Assign(Result, replacementCount.ToString(CultureInfo.InvariantCulture), update == 0 ? counter : update);
                                        }   
                                        replacementTotal += replacementCount;
                                        counter++;
                                        return DataStorage.WarewolfAtom.NewDataString(replace);
                                    }, update);
                                }
                                if (DataListUtil.IsValueScalar(Result))
                                {
                                    dataObject.Environment.Assign(Result, replacementTotal.ToString(CultureInfo.InvariantCulture), update == 0 ? counter : update);
                                }
                                
                                if (dataObject.IsDebugMode() && !allErrors.HasErrors())
                                {
                                    if (!string.IsNullOrEmpty(Result))
                                    {
                                        if (replacementTotal > 0)
                                        {
                                            AddDebugOutputItem(new DebugEvalResult(s, "", dataObject.Environment, update));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    if(!string.IsNullOrEmpty(Result))
                    {
                        AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                    }
                }
                // now push the result to the server
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch(Exception ex)
            {
                Dev2Logger.Error("DSFReplace", ex);
                allErrors.AddError(ex.Message);
            }
            finally
            {
                if(allErrors.HasErrors())
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfReplaceActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    dataObject.Environment.Assign(Result, null, update);
                }

                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

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

        #region Get ForEach Inputs/Outputs Updates

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == FieldsToSearch)
                    {
                        FieldsToSearch = t.Item2;
                    }

                    if(t.Item1 == Find)
                    {
                        Find = t.Item2;
                    }

                    if(t.Item1 == ReplaceWith)
                    {
                        ReplaceWith = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if(itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(FieldsToSearch, Find, ReplaceWith);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
