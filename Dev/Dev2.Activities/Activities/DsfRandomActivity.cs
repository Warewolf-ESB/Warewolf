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
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FunctionComplexityOverflow

namespace Dev2.Activities
{
    [ToolDescriptorInfo("Utility-Random", "Random", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Random")]
    public class DsfRandomActivity : DsfActivityAbstract<string>
    {

        #region Properties

        [FindMissing]
        [Inputs("Length")]
        public string Length { get; set; }

        public enRandomType RandomType { get; set; }

        [FindMissing]
        [Inputs("From")]
        public string From { get; set; }

        [FindMissing]
        [Inputs("To")]
        public string To { get; set; }

        [FindMissing]
        [Outputs("Result")]
        public new string Result { get; set; }

        #endregion

        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        #region Ctor

        public DsfRandomActivity()
            : base("Random")
        {
            Length = string.Empty;
            RandomType = enRandomType.Numbers;
            Result = string.Empty;
            From = string.Empty;
            To = string.Empty;
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
                if (!errors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(Length, From, To, dataObject.Environment, RandomType, update);
                    }

                    IWarewolfIterator lengthItr = !String.IsNullOrEmpty(Length) ? new WarewolfIterator(env.EvalStrict(Length, update)) as IWarewolfIterator : new WarewolfAtomIterator(new[] { DataStorage.WarewolfAtom.Nothing, });
                    var fromItr = !String.IsNullOrEmpty(From) ? new WarewolfIterator(env.EvalStrict(From, update)) as IWarewolfIterator : new WarewolfAtomIterator(new[] { DataStorage.WarewolfAtom.Nothing, });
                    var toItr = !String.IsNullOrEmpty(To) ? new WarewolfIterator(env.EvalStrict(To, update)) as IWarewolfIterator : new WarewolfAtomIterator(new[] { DataStorage.WarewolfAtom.Nothing, });
                    WarewolfListIterator colItr = new WarewolfListIterator();
                    colItr.AddVariableToIterateOn(lengthItr);
                    colItr.AddVariableToIterateOn(fromItr);
                    colItr.AddVariableToIterateOn(toItr);

                    Dev2Random dev2Random = new Dev2Random();
                    var counter = 1;
                    while (colItr.HasMoreData())
                    {
                        int lengthNum = -1;
                        double fromNum = -1.0;
                        double toNum = -1.0;

                        string fromValue = colItr.FetchNextValue(fromItr);
                        string toValue = colItr.FetchNextValue(toItr);
                        string lengthValue = colItr.FetchNextValue(lengthItr);

                        if (RandomType != enRandomType.Guid)
                        {
                            if (RandomType == enRandomType.Numbers)
                            {
                                #region Getting the From

                                fromNum = GetFromValue(fromValue, out errors);
                                if (errors.HasErrors())
                                {
                                    allErrors.MergeErrors(errors);
                                    continue;
                                }

                                #endregion

                                #region Getting the To

                                toNum = GetToValue(toValue, out errors);
                                if (errors.HasErrors())
                                {
                                    allErrors.MergeErrors(errors);
                                    continue;
                                }

                                #endregion
                            }
                            else
                            {
                                #region Getting the Length

                                lengthNum = GetLengthValue(lengthValue, out errors);
                                if (errors.HasErrors())
                                {
                                    allErrors.MergeErrors(errors);
                                    continue;
                                }

                                #endregion
                            }
                        }
                        string value = dev2Random.GetRandom(RandomType, lengthNum, fromNum, toNum);

                        var rule = new IsSingleValueRule(() => Result);
                        var single = rule.Check();
                        if (single != null)
                        {
                            allErrors.AddError(single.Message);
                        }
                        else
                        {
                            env.Assign(Result, value, update == 0 ? counter : update);
                        }
                        counter++;
                    }
                }
                allErrors.MergeErrors(errors);
                if (!allErrors.HasErrors())
                {
                    AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFRandomActivity", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfRandomActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    if (hasErrors)
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
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {

                    if (t.Item1 == From)
                    {
                        From = t.Item2;
                    }

                    if (t.Item1 == To)
                    {
                        To = t.Item2;
                    }

                    if (t.Item1 == Length)
                    {
                        Length = t.Item2;
                    }
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

        #region Private Methods

        private double GetFromValue(string fromValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            double fromNum;
            if (string.IsNullOrEmpty(fromValue))
            {
                errors.AddError(ErrorResource.IntegerOrDecimaExpectedForStart);
                return -1;
            }
            if (!double.TryParse(fromValue, out fromNum))
            {
                errors.AddError(string.Format(ErrorResource.IntegerOrDecimaExpectedForStart + " from {0} to {1}.", double.MinValue, double.MaxValue));
                return -1;
            }
            return fromNum;
        }

        private double GetToValue(string toValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            double toNum;
            if (string.IsNullOrEmpty(toValue))
            {
                errors.AddError(ErrorResource.IntegerOrDecimaExpectedForEnd);
                return -1;
            }
            if (!double.TryParse(toValue, out toNum))
            {
                errors.AddError(string.Format(ErrorResource.IntegerOrDecimaExpectedForEnd + " from {0} to {1}.", double.MinValue, double.MaxValue));
                return -1;
            }
            return toNum;
        }

        private int GetLengthValue(string lengthValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            int lengthNum;
            if (string.IsNullOrEmpty(lengthValue))
            {
                errors.AddError(string.Format(ErrorResource.PositiveIntegerRequired, "Length."));
                return -1;
            }

            if (!int.TryParse(lengthValue, out lengthNum))
            {
                errors.AddError(string.Format(ErrorResource.EnsureValueIsInteger, "Length"));
                return -1;
            }

            if (lengthNum < 1)
            {
                errors.AddError(string.Format(ErrorResource.PositiveIntegerRequired, "Length."));
                return -1;
            }

            return lengthNum;
        }

        private void AddDebugInputItem(string lengthExpression, string fromExpression, string toExpression, IExecutionEnvironment executionEnvironment, enRandomType randomType, int update)
        {
            AddDebugInputItem(new DebugItemStaticDataParams(randomType.GetDescription(), "Random"));

            if (randomType == enRandomType.Guid)
            {
                return;
            }

            if (randomType == enRandomType.Numbers)
            {
                AddDebugInputItem(new DebugEvalResult(fromExpression, "From", executionEnvironment, update));
                AddDebugInputItem(new DebugEvalResult(toExpression, "To", executionEnvironment, update));
            }
            else
            {
                AddDebugInputItem(new DebugEvalResult(lengthExpression, "Length", executionEnvironment, update));
            }
        }

        #endregion

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

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(To, From, Length);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
