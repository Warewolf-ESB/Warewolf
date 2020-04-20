#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.MathOperations;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using System.Text;

namespace Dev2.Data
{
    public class WarewolfIterator : IWarewolfIterator
    {
        CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult _listResult;
        CommonFunctions.WarewolfEvalResult.WarewolfAtomResult _scalarResult;
        readonly int _maxValue;
        int _currentValue;
        readonly FunctionEvaluatorOption _functionEvaluatorOption;
        public string NewLineFormat { get; private set; } = "\r\n";

        public WarewolfIterator(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            SetupListResult(warewolfEvalResult);
            SetupScalarResult(warewolfEvalResult);
            SetupForWarewolfRecordSetResult(warewolfEvalResult);
            _maxValue = _listResult?.Item.Count(atom => atom != null) ?? 1;
            _currentValue = 0;
            _functionEvaluatorOption = FunctionEvaluatorOption.Dev2DateTimeFormat;
        }

        public WarewolfIterator(CommonFunctions.WarewolfEvalResult warewolfEvalResult, FunctionEvaluatorOption functionEvaluatorOption)
            : this(warewolfEvalResult)
        {
            _functionEvaluatorOption = functionEvaluatorOption;
        }

        void SetupForWarewolfRecordSetResult(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfRecordSetResult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult listResult)
            {
                var stringValue = new StringBuilder();
                foreach (var item in listResult.Item.Data)
                {
                    if (item.Key != EvaluationFunctions.PositionColumn)
                    {
                        var data = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(item.Value) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        var warewolfEvalResultToString = ExecutionEnvironment.WarewolfEvalResultToString(data);
                        AppendEvaluatedString(ref stringValue, warewolfEvalResultToString);
                    }
                }
                _scalarResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(stringValue.ToString())) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            }
        }

        private static void AppendEvaluatedString(ref StringBuilder stringValue, string warewolfEvalResultToString)
        {
            if (stringValue.Length <= 0)
            {
                stringValue.Append(warewolfEvalResultToString);
            }
            else
            {
                stringValue.Append("," + warewolfEvalResultToString);
            }
        }

        void SetupScalarResult(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                _scalarResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (_scalarResult.Item.IsDataString)
                {
                    var str = _scalarResult.Item.ToString();
                    if (str.Contains("\n") && !str.Contains("\r\n"))
                    {
                        NewLineFormat = "\n";
                    }
                }
            }
        }

        void SetupListResult(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfAtomListresult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult)
            {
                warewolfAtomListresult.Item.ResetCurrentEnumerator();
                _listResult = warewolfAtomListresult;
            }
        }

        public int GetLength() => _maxValue;

        public string GetNextValue()
        {
            _currentValue++;
            if (_listResult != null)
            {
                var warewolfAtomToString = ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(_listResult.Item.GetNextValue());
                warewolfAtomToString = DoCalcution(warewolfAtomToString);
                return warewolfAtomToString;
            }
            return _scalarResult != null ? DoCalcution(ExecutionEnvironment.WarewolfAtomToStringErrorIfNull(_scalarResult.Item)) : null;
        }

        string DoCalcution(string argWarewolfAtomToString)
        {
            if (argWarewolfAtomToString == null)
            {
                return null;
            }

            var warewolfAtomToString = argWarewolfAtomToString;

            var isCalcEvaluation = DataListUtil.IsCalcEvaluation(warewolfAtomToString, out string cleanExpression);

            if (isCalcEvaluation)
            {
                var functionEvaluator = new FunctionEvaluator(_functionEvaluatorOption);
                var tryEvaluateFunction = functionEvaluator.TryEvaluateFunction(cleanExpression, out string eval, out string error);
                warewolfAtomToString = eval;
                if (eval == cleanExpression.Replace("\"", "") && cleanExpression.Contains("\""))
                {
                    TryEvalAsFunction(ref warewolfAtomToString, cleanExpression, functionEvaluator, ref error);
                }
                if (!tryEvaluateFunction)
                {
                    if (error == ErrorResource.IncorrectOperandType)
                    {
                        error += string.Format("Unable to calculate: '{0}'. Try rewriting the expression.", cleanExpression);
                    }

                    throw new Exception(error);
                }
            }
            return warewolfAtomToString;
        }

        private static void TryEvalAsFunction(ref string warewolfAtomToString, string cleanExpression, FunctionEvaluator functionEvaluator, ref string error)
        {
            try
            {
                var b = functionEvaluator.TryEvaluateFunction(cleanExpression.Replace("\"", ""), out string eval2, out error);
                if (b)
                {
                    warewolfAtomToString = eval2;

                }
            }
            catch (Exception err)
            {
                Dev2Logger.Warn(err, "Warewolf Warn");
            }
        }

        public bool HasMoreData() => _currentValue <= _maxValue - 1;
    }
}