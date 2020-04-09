#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Exceptions;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("Utility-Calculate", "Calculate", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Calculate")]
    public class DsfDotNetCalculateActivity : DsfActivityAbstract<string>
    {
        [Inputs("Expression")]
        [FindMissing]
        public string Expression { get; set; }

        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        public DsfDotNetCalculateActivity()
            : base("Calculate")
        {
            Expression = string.Empty;
            Result = string.Empty;
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
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);

                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(dataObject.Environment, update);
                }

                var input = string.IsNullOrEmpty(Expression) ? Expression : Expression.Replace("\\r", string.Empty).Replace("\\n", string.Empty).Replace(Environment.NewLine, "");
                var warewolfListIterator = new WarewolfListIterator();
                var calc = String.Format(GlobalConstants.CalculateTextConvertFormat, input);
                var warewolfEvalResult = dataObject.Environment.Eval(calc, update);
                if (warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult && scalarResult.Item.IsNothing)
                {
                    throw new NullValueInVariableException(ErrorResource.VariableInputError, input);
                }
                var inputIterator = new WarewolfIterator(warewolfEvalResult, FunctionEvaluatorOption.DotNetDateTimeFormat);
                warewolfListIterator.AddVariableToIterateOn(inputIterator);
                var counter = 1;
                while(warewolfListIterator.HasMoreData())
                {
                    var result = warewolfListIterator.FetchNextValue(inputIterator);
                    dataObject.Environment.Assign(Result, result, update == 0 ? counter : update);
                    counter++;
                }

                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    AddDebugOutputItem(Result, dataObject.Environment, update);
                }
                allErrors.MergeErrors(errors);
            }
            catch(Exception ex)
            {
                Dev2Logger.Error("Calculate Exception", ex, GlobalConstants.WarewolfError);
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                HandleErrors(dataObject, update, allErrors);
            }
        }

        void HandleErrors(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var hasErrors = allErrors.HasErrors();
            if (hasErrors)
            {
                DisplayAndWriteError("DsfCalculateActivity", allErrors);
                var errorString = allErrors.MakeDisplayReady();
                dataObject.Environment.AddError(errorString);
            }
            if (dataObject.IsDebugMode())
            {
                if (hasErrors)
                {
                    AddDebugOutputItem(Result, dataObject.Environment, update);
                }
                DispatchDebugState(dataObject, StateType.Before, update);
                DispatchDebugState(dataObject, StateType.After, update);
            }
        }

        void AddDebugInputItem(IExecutionEnvironment environment, int update)
        {
            AddDebugInputItem(new DebugEvalResult(Expression, "fx =", environment, update, false, true));
        }

        void AddDebugOutputItem(string expression, IExecutionEnvironment environment, int update)
        {
            AddDebugOutputItem(new DebugEvalResult(expression, "", environment, update));
        }

        public override List<string> GetOutputs() => new List<string> { Result };

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {

            if(updates != null && updates.Count == 1)
            {
                Expression = updates[0].Item2;
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

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(Expression);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        public bool Equals(DsfDotNetCalculateActivity other)
        {
            var eq = this.DisplayName.Equals(other.DisplayName);
            eq &= this.Expression.Equals(other.Expression);
            eq &= this.Result.Equals(other.Result);

            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfDotNetCalculateActivity instance)
            {
                return Equals(instance);
            }
            return false;
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name="Expression",
                    Type = StateVariable.StateType.Input,
                    Value = Expression
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                }
            };
        }
    }
}

