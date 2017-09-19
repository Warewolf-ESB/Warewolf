using System;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.MathOperations;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Data
{
    public class WarewolfIterator : IWarewolfIterator
    {
        CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult _listResult;
        CommonFunctions.WarewolfEvalResult.WarewolfAtomResult _scalarResult;
        readonly int _maxValue;
        int _currentValue;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WarewolfIterator(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            SetupListResult(warewolfEvalResult);
            SetupScalarResult(warewolfEvalResult);
            SetupForWarewolfRecordSetResult(warewolfEvalResult);
            _maxValue = _listResult?.Item.Count(atom => atom != null) ?? 1;
            _currentValue = 0;
        }

        void SetupForWarewolfRecordSetResult(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfRecordSetResult)
            {
                var listResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult;
                if (listResult != null)
                {
                    var stringValue = "";
                    foreach (var item in listResult.Item.Data)
                    {
                        if (item.Key != EvaluationFunctions.PositionColumn)
                        {
                            var data = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(item.Value) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                            var warewolfEvalResultToString = ExecutionEnvironment.WarewolfEvalResultToString(data);
                            if (string.IsNullOrEmpty(stringValue))
                            {
                                stringValue = warewolfEvalResultToString;
                            }
                            else
                            {
                                stringValue += "," + warewolfEvalResultToString;
                            }
                        }
                    }
                    _scalarResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(stringValue)) as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                }
            }
        }

        void SetupScalarResult(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                _scalarResult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
            }
        }

        void SetupListResult(CommonFunctions.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var warewolfAtomListresult = warewolfEvalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                if (warewolfAtomListresult != null)
                {
                    warewolfAtomListresult.Item.ResetCurrentEnumerator();
                    _listResult = warewolfAtomListresult;
                }
            }
        }

        #region Implementation of IWarewolfIterator

        public int GetLength()
        {
            return _maxValue;
        }

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

        static string DoCalcution(string warewolfAtomToString)
        {
            if(warewolfAtomToString == null)
            {
                return null;
            }
            string cleanExpression;
            var isCalcEvaluation = DataListUtil.IsCalcEvaluation(warewolfAtomToString, out cleanExpression);

            if (isCalcEvaluation)
            {
                var functionEvaluator = new FunctionEvaluator();
                string eval;
                string error;
                var tryEvaluateFunction = functionEvaluator.TryEvaluateFunction(cleanExpression, out eval, out error);
                warewolfAtomToString = eval;
                if (eval == cleanExpression.Replace("\"", "") && cleanExpression.Contains("\""))
                {
                    try
                    {
                        string eval2;
                        var b = functionEvaluator.TryEvaluateFunction(cleanExpression.Replace("\"", ""), out eval2, out error);
                        if (b)
                        {
                            warewolfAtomToString = eval2;

                        }
                    }
                    catch (Exception err)
                    {

                        Dev2Logger.Warn(err, "Warewolf Warning");
                    }
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

        public bool HasMoreData()
        {
            return _currentValue <= _maxValue - 1;
        }
        #endregion
    }
}