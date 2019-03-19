#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.State;
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
using Warewolf.Storage.Interfaces;




namespace Dev2.Activities
{
    [ToolDescriptorInfo("Utility-Random", "Random", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Random")]
    public class DsfRandomActivity : DsfActivityAbstract<string>,IEquatable<DsfRandomActivity>
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

        public override List<string> GetOutputs() => new List<string> { Result };

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

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "RandomType",
                    Value = RandomType.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "From",
                    Value = From,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "To",
                    Value = To,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Length",
                    Value = Length,
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

        #endregion

        #region Overrides of DsfNativeActivity<string>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();

            var env = dataObject.Environment;
            InitializeDebug(dataObject);

            try
            {
                TryExecute(dataObject, update, allErrors, env);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFRandomActivity", e, GlobalConstants.WarewolfError);
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

        private void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, IExecutionEnvironment env)
        {
            var errors = new ErrorResultTO();
            if (!errors.HasErrors())
            {
                errors = UpdateEnvironment(dataObject, update, allErrors, env, errors);
            }
            allErrors.MergeErrors(errors);
            if (!allErrors.HasErrors())
            {
                AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        private ErrorResultTO UpdateEnvironment(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, IExecutionEnvironment env, ErrorResultTO errors)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (dataObject.IsDebugMode())
            {
                AddDebugInputItem(Length, From, To, dataObject.Environment, RandomType, update);
            }

            var lengthItr = !String.IsNullOrEmpty(Length) ? new WarewolfIterator(env.EvalStrict(Length, update)) as IWarewolfIterator : new WarewolfAtomIterator(new[] { DataStorage.WarewolfAtom.Nothing, });
            var fromItr = !String.IsNullOrEmpty(From) ? new WarewolfIterator(env.EvalStrict(From, update)) as IWarewolfIterator : new WarewolfAtomIterator(new[] { DataStorage.WarewolfAtom.Nothing, });
            var toItr = !String.IsNullOrEmpty(To) ? new WarewolfIterator(env.EvalStrict(To, update)) as IWarewolfIterator : new WarewolfAtomIterator(new[] { DataStorage.WarewolfAtom.Nothing, });
            var colItr = new WarewolfListIterator();
            colItr.AddVariableToIterateOn(lengthItr);
            colItr.AddVariableToIterateOn(fromItr);
            colItr.AddVariableToIterateOn(toItr);

            var dev2Random = new Dev2Random();
            var counter = 1;
            while (colItr.HasMoreData())
            {
                var lengthNum = -1;
                var fromNum = -1.0;
                var toNum = -1.0;

                var fromValue = colItr.FetchNextValue(fromItr);
                var toValue = colItr.FetchNextValue(toItr);
                var lengthValue = colItr.FetchNextValue(lengthItr);

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
                if (RandomType != enRandomType.Numbers && RandomType != enRandomType.Guid)
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
                var value = dev2Random.GetRandom(RandomType, lengthNum, fromNum, toNum);

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

            return errors;
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

        double GetFromValue(string fromValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (string.IsNullOrEmpty(fromValue))
            {
                errors.AddError(ErrorResource.IntegerOrDecimaExpectedForStart);
                return -1;
            }
            if (!double.TryParse(fromValue, out double fromNum))
            {
                errors.AddError(string.Format(ErrorResource.IntegerOrDecimaExpectedForStart + " from {0} to {1}.", double.MinValue, double.MaxValue));
                return -1;
            }
            return fromNum;
        }

        double GetToValue(string toValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (string.IsNullOrEmpty(toValue))
            {
                errors.AddError(ErrorResource.IntegerOrDecimaExpectedForEnd);
                return -1;
            }
            if (!double.TryParse(toValue, out double toNum))
            {
                errors.AddError(string.Format(ErrorResource.IntegerOrDecimaExpectedForEnd + " from {0} to {1}.", double.MinValue, double.MaxValue));
                return -1;
            }
            return toNum;
        }

        int GetLengthValue(string lengthValue, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if (string.IsNullOrEmpty(lengthValue))
            {
                errors.AddError(string.Format(ErrorResource.PositiveIntegerRequired, "Length."));
                return -1;
            }

            if (!int.TryParse(lengthValue, out int lengthNum))
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

        void AddDebugInputItem(string lengthExpression, string fromExpression, string toExpression, IExecutionEnvironment executionEnvironment, enRandomType randomType, int update)
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

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(To, From, Length);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        #endregion

        public bool Equals(DsfRandomActivity other)
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
                && string.Equals(Length, other.Length) 
                && RandomType == other.RandomType 
                && string.Equals(From, other.From) 
                && string.Equals(To, other.To) 
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

            return Equals((DsfRandomActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Length != null ? Length.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) RandomType;
                hashCode = (hashCode * 397) ^ (From != null ? From.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (To != null ? To.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
