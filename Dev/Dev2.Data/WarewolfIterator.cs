#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using static DataStorage;

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