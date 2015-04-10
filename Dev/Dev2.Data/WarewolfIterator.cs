using System;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.MathOperations;
using Warewolf.Storage;

namespace Dev2.Data
{
    public class WarewolfIterator : IWarewolfIterator
    {
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult _listResult;
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult _scalarResult;
        readonly int _maxValue;
        int _currentValue;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WarewolfIterator(WarewolfDataEvaluationCommon.WarewolfEvalResult warewolfEvalResult)
        {
            if (warewolfEvalResult.IsWarewolfAtomListresult)
            {
                var warewolfAtomListresult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                if(warewolfAtomListresult != null)
                {
                    warewolfAtomListresult.Item.ResetCurrentEnumerator();
                    _listResult = warewolfAtomListresult;
                }
            }
            else if (warewolfEvalResult.IsWarewolfAtomResult)
            {
                _scalarResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;               
            }
            else if (warewolfEvalResult.IsWarewolfRecordSetResult)
            {
                var listResult = warewolfEvalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult;
                if (listResult != null)
                {
                    var stringValue = "";
                    foreach(var item in listResult.Item.Data)
                    {
                        if (item.Key != WarewolfDataEvaluationCommon.PositionColumn)
                        {
                            var data = WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomListresult(item.Value) as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                            var warewolfEvalResultToString = ExecutionEnvironment.WarewolfEvalResultToString(data);
                            if(string.IsNullOrEmpty(stringValue))
                            {
                                stringValue = warewolfEvalResultToString;
                            }else
                            {
                                stringValue += ","+warewolfEvalResultToString;
                            }
                        }
                    }
                    _scalarResult = WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomResult(DataASTMutable.WarewolfAtom.NewDataString(stringValue)) as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                }
            }
            _maxValue = _listResult != null ? _listResult.Item.Count(atom => atom!=null && !atom.IsNothing) : 1;
            _currentValue = 0;
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
                var warewolfAtomToString = ExecutionEnvironment.WarewolfAtomToString(_listResult.Item.GetNextValue());                     
                warewolfAtomToString = DoCalcution(warewolfAtomToString);
                return warewolfAtomToString;
            }
            return _scalarResult!=null ? DoCalcution(ExecutionEnvironment.WarewolfAtomToString(_scalarResult.Item)) : null;
        }

        static string DoCalcution(string warewolfAtomToString)
        {
            string cleanExpression;
            var isCalcEvaluation = DataListUtil.IsCalcEvaluation(warewolfAtomToString, out cleanExpression);
            var functionEvaluator = new FunctionEvaluator();
            if(isCalcEvaluation)
            {
                string eval;
                string error;
                var tryEvaluateFunction = functionEvaluator.TryEvaluateFunction(cleanExpression, out eval, out error);
                warewolfAtomToString = eval;
                if (!tryEvaluateFunction)
                {
                    throw new Exception(error);
                }
            }
            return warewolfAtomToString;
        }

        public bool HasMoreData()
        {
            return _currentValue<=_maxValue-1;
        }
        #endregion
    }
}